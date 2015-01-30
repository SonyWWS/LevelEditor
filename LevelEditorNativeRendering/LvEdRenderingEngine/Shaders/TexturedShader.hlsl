//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

//---------------------------------------------------------------------------
// TexturedShader.hlsl
//
//    Shader for textured surfaces, optionally with shadow maps.
//---------------------------------------------------------------------------
#define FLIP_TEXTURE_Y

#include "Fog.shh"
#include "Lighting.shh"

//---------------------------------------------------------------------------
//  Constant Buffers
//---------------------------------------------------------------------------

cbuffer ConstantBufferPerFrame : register( b0 )
{
    float4x4            cb_view;
    float4x4            cb_proj;
	ExpFog              cb_fog;
    float3              cb_camPosW;    
};

cbuffer ConstantBufferRenderState : register( b1 )
{
    int                 cb_textured;
    int                 cb_lit;
    int                 cb_shadowed;
    float               cb_b1_pad1;
};

cbuffer ConstantBufferPerDraw : register( b2 )
{
    float4x4            cb_world;
    float4x4            cb_worldInvTrans;
    float4x4            cb_textureTrans;     // transformation for texture coordinates.                                                
    float4              cb_matEmissive;
    float4              cb_matDiffuse;
    float4              cb_matSpecular;
    LightEnvironment    cb_lightEnv;
    bool                cb_hasDiffuseMap;
    bool                cb_hasNormalMap;
    bool                cb_hasSpecularMap;
    bool                cb_pad1;
};

cbuffer ConstantBufferShadowMapping : register( b3 )
{
    matrix              cb_smShadowTransform;    
    float               cb_smTexelSize;     
};

//--------------------------------------------------------------------------------------
// Textures and Samplers
//--------------------------------------------------------------------------------------
Texture2D    diffuseTex                     : register( t0 );
Texture2D    normalTex                      : register( t1 );
Texture2D    specularTex                    : register( t2 );
Texture2D    shadowTex                      : register( t3 );

SamplerState diffuseSampler                 : register( s0 );
SamplerComparisonState shadowSamplerCmp     : register( s1 );

//--------------------------------------------------------------------------------------
// Input / Output structures
//--------------------------------------------------------------------------------------
struct VS_INPUT
{
    float4 posL                             : POSITION;
    float3 normL                            : NORMAL;
    float2 tex0                             : TEXCOORD;
    float3 tanL                             : TANGENT;
};

struct PS_INPUT
{
    float4 posH                             : SV_POSITION;
    float3 posW                             : POSITION;
    float3 normW                            : NORMAL;
    float3 tanW                             : TANGENT;
    float2 tex0                             : TEXCOORD0;
    float4 texShadow                        : TEXCOORD1;
};

//--------------------------------------------------------------------------------------
// VSMain
//--------------------------------------------------------------------------------------
PS_INPUT VSMain( VS_INPUT input )
{
    PS_INPUT output = (PS_INPUT)0;

	float4x4 wvp = mul(cb_world,mul(cb_view,cb_proj));

	output.posH  = mul( input.posL, wvp );
	output.posW  = mul( input.posL, cb_world).xyz;               
	output.normW = mul( input.normL, (float3x3)cb_worldInvTrans);               
	output.tanW  = mul( input.tanL, (float3x3)cb_worldInvTrans);

	#ifdef FLIP_TEXTURE_Y                                               
	output.tex0 = float2(input.tex0.x,(1.0-input.tex0.y));                 
	#else                                                               
	output.tex0 = input.tex0;                                             
	#endif 

    float4 tex = float4(output.tex0.x, output.tex0.y, 0, 1);
    // transform texture coordinates
    output.tex0 = mul(tex, cb_textureTrans).xy;

    if ( cb_shadowed )
    {      
        // Transform the shadow texture coordinates.
        output.texShadow = mul( input.posL, mul(cb_world, cb_smShadowTransform) );
    }

    return output;
}

//--------------------------------------------------------------------------------------
//    PSMain
//--------------------------------------------------------------------------------------
float4 PSMain( PS_INPUT input ) : SV_TARGET
{
   float4  matdiffuse  = cb_matDiffuse;
   float4  matspecular = cb_matSpecular;
   float4  fc = matdiffuse;
   if(cb_textured)                                                        
   {   
      if(cb_hasDiffuseMap)
	   {
	      float4 texcolor = diffuseTex.Sample( diffuseSampler, input.tex0 );
		  clip(texcolor.a - 0.5);
		  matdiffuse *= texcolor;
          fc = matdiffuse;
	   }
	  // if(cb_hasSpecularMap)
	 //  {
	 //     matspecular = specularTex.Sample( diffuseSampler, input.tex0 );
	//	  matspecular.a *= 255;
	//   }
   }
   if(cb_lit)
   {

        float3 norm = normalize(input.normW);
        if( cb_hasNormalMap )
        {         
            float3 T = normalize(input.tanW - dot(input.tanW, norm) * norm);
            float3 B = cross(norm,T);
            float3 normalMap = normalTex.Sample(diffuseSampler, input.tex0).xyz;
            // expand the range of the normal map from (0,1) to (-1,1)
            normalMap = (( 2 * normalMap ) - 1);

            float3x3 TBN = float3x3(T, B, norm);
            // Transform from tangent space to world space.
            norm = normalize(mul(normalMap, TBN));
        }
        
		float ShadowFactor = 1.0;
		if ( cb_shadowed )
        {
		   ShadowFactor = ComputeShadowFactor(shadowTex, shadowSamplerCmp, input.texShadow.xyz,cb_smTexelSize);		   
		}
		float3 A,D,S;
		float specPower = max(1.0,matspecular.a);
		
        ComputeLighting(input.posW,
		                       norm,
							   cb_camPosW,
							   specPower,
							   ShadowFactor,
							    cb_lightEnv, 
								A,D,S);

		fc.xyz = cb_matEmissive.xyz + matdiffuse.xyz * (A + D) + matspecular.xyz * S;
    }

	if(cb_fog.enabled)
	{
	   float dist = distance( cb_camPosW, input.posW  );	   
	   float foglerp = ComputeFogFactor(cb_fog,dist);
	   fc.xyz = lerp(cb_fog.color.xyz,fc.xyz , foglerp);	   	   
	}	

    return fc;
}
