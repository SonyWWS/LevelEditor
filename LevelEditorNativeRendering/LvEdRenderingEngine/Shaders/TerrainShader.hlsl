//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

//---------------------------------------------------------------------------
// TerrainShader.hlsl
//
//    Terrain Texture splatting Shader
//---------------------------------------------------------------------------

#include "Fog.shh"
#include "Lighting.shh"

//---------------------------------------------------------------------------
//  Constant Buffers
//---------------------------------------------------------------------------

#define MaxNumLayers 8

cbuffer PerFrameCb : register( b0 )
{
    float4x4  cb_view;
    float4x4  cb_proj;
	float4    Viewport;  // used for rendering wireframe.
    float3    cb_camPosW;
};

cbuffer RenderStateCb : register( b1 )
{
    int                 cb_textured;
    int                 cb_lit;
    int                 cb_shadowed;
    float               cb_b1_pad1;
};

cbuffer PerTerrainCb : register( b2 )
{    
     float4   layerTexScale[MaxNumLayers];
	 float4   wirecolor;
	 float3  terrainTrans;
	 float   cellSize;
	 float2  hmSize;                // size of hiehgt map in texels 
	 float2  hmTexelsize;           // texel-size of the height texture.         
	 ExpFog  fog;
	 float  numLayers;	
	    
};

cbuffer PerPatchCb : register( b3 )
{    
    float3              patchTrans; // patch translation.		
	float               PerPatchCb_Pad;
    LightEnvironment    cb_lightEnv;    
};

cbuffer ShadowMappingCb : register( b4 )
{
    matrix              cb_smShadowTransform;
    float               cb_smTexelSize; 
};


cbuffer perDecomapCb : register( b5 )
{
    float3 decoScale;    
	//float decoPad; auto padded
};

//--------------------------------------------------------------------------------------
// Textures
//--------------------------------------------------------------------------------------

Texture2D         shadowTex   : register(t0);
Texture2D<float>  hnTex       : register(t1);
Texture2D         layers[MaxNumLayers] : register(t2);
Texture2D<float>  layermasks[MaxNumLayers] : register(t10);
Texture2D         layernormalmaps[MaxNumLayers];


//---------------------------------------------------------------
// Samplers
//------------------------------------------------------------
SamplerState            hnSampler        : register(s0); // height 
SamplerComparisonState  shadowSamplerCmp : register(s1);
SamplerState            linwrapSampler   : register(s2); // linear and wrapuv sampler.
SamplerState            linclampSampler  : register(s3); // linear and clampuv sampler.
SamplerState            anisoWrap        : register(s4); // anisotropic and clampuv sampler.


struct PS_INPUT
{
    float4 posH         : SV_POSITION;
    float3 posW         : POSITION;    	
    float2 stretchedTex : TEXCOORD0;    
    float4 texShadow    : TEXCOORD1;
	
};


//--------------------------------------------------------------------------------------
// Vertex shader
//--------------------------------------------------------------------------------------
PS_INPUT VSMain( float3 posL : POSITION )
{     
    //input.posL is in terrain-patch space.

	// posHm  postion in heightmap space.
	float3 posHm = (posL + patchTrans);
	// tex coord generated such that each vertex tex coord corresponds to the 
	// center of texel.
	float2 tex = posHm.xz / hmSize + hmTexelsize/2.0f;

	// PosT   position in terrain space.
	float3 posT = posHm * cellSize;		
	posT.y = hnTex.SampleLevel(hnSampler, tex,0);
				
	// position in world space.
	float3 posW = posT + terrainTrans;
	
    PS_INPUT output = (PS_INPUT)0;

	float4x4 vp   = mul(cb_view, cb_proj);
	output.posH   = mul(float4(posW,1), vp);	
	output.posW   = posW;	
	output.stretchedTex = tex;
    
	if ( cb_shadowed )
    {      
        // Transform the shadow texture coordinates.
        output.texShadow = mul( float4(posW,1),  cb_smShadowTransform);
    }

    return output;
}

//--------------------------------------------------------------------------------------
//    Pixel shader
//--------------------------------------------------------------------------------------
float4 PSMain( PS_INPUT input ) : SV_TARGET
{
    float3 fc = float3(0.5,0.5,0.5);	
	float3 normL = float3(0,0,0);
	bool textured = cb_textured && numLayers > 0;
    if (textured)
    {	    	    
  	    [unroll]
	    for(int i = 0; i < numLayers; i++)
		{		   		   
		   float2 tiledTex  = input.stretchedTex * layerTexScale[i].x;			    		   
		   float mask = layermasks[i].Sample(linclampSampler,input.stretchedTex);

		   float3 di = layers[i].Sample(anisoWrap,tiledTex).xyz;		   		  
		   fc = lerp(fc,di,mask);	

		   float3 n  = layernormalmaps[i].Sample(anisoWrap,tiledTex).xyz;		   	   
		   normL = lerp(normL,n,mask);		   
		}	

		// decode normal
		normL = ( 2 * normL - 1);				
    }
	    	
    if(cb_lit)
    {
	    // compute normal from height.
		float2 left =   input.stretchedTex + float2(-hmTexelsize.x,0);
		float2 right =  input.stretchedTex + float2(hmTexelsize.x,0);
		float2 top =    input.stretchedTex + float2(0,-hmTexelsize.y);
		float2 bottom = input.stretchedTex + float2(0,hmTexelsize.y);

		float leftH = hnTex.SampleLevel(linclampSampler, left,0);
		float rightH = hnTex.SampleLevel(linclampSampler, right,0);
		float topH = hnTex.SampleLevel(linclampSampler, top,0);
		float bottomH = hnTex.SampleLevel(linclampSampler, bottom,0);

		float3 T = normalize(float3(2 * cellSize,rightH - leftH, 0));
		float3 B = normalize(float3(0, bottomH - topH, 2 * cellSize));
		float3 N = cross(B,T);

		float3x3 TBN = float3x3(T, B, N);
		float3 norm = textured ? normalize(mul(normL, TBN)) : N;
					
		
		float ShadowFactor = 1.0;
		if ( cb_shadowed )
        {
		  ShadowFactor = ComputeShadowFactor(shadowTex, shadowSamplerCmp, input.texShadow.xyz,cb_smTexelSize);		   
		}
		float3 A,D,S;
		float specPower = 1;
		
        ComputeLighting(input.posW,
		                       norm,
							   cb_camPosW,
							   specPower,
							   ShadowFactor,
							    cb_lightEnv, 
								A,D,S);

		//fc.xyz = fc.xyz * (A + D) + matspecular.xyz * S;		
		fc = fc * (A + D);  // don't compute specular for terrain.
    }
		
    if(fog.enabled)
	{
	   float dist = distance( cb_camPosW, input.posW  );	   
	   float foglerp = ComputeFogFactor(fog,dist);
	   fc = lerp(fog.color.xyz,fc , foglerp);	   	   
	}	
    return float4(fc,1);
}



//------------------------------------------------------------------------------
//                                      Render decoration layers
//-------------------------------------------------------------------------------

// 1-  render decoration using instances of star-shaped intersecting quad.
struct decoVSIn
{
	float3 posL   : POSITION;
	float3 normL  : NORMAL;
	float2 tex    : TEXCOORD;	
	float2 insTex : INSPOS; // 
};

struct decoPSIn
{
   float4 posH : SV_POSITION;
   float3 posW : POSITION;
   float2 tex  : TEXCOORD;
};

decoPSIn VSDeco(decoVSIn vin)
{
    decoPSIn output;   
	float3 posT;	
	posT.xz = (cellSize * hmSize) * vin.insTex;

    float2 tex =  vin.insTex + hmTexelsize/2.0f;
	posT.y = hnTex.SampleLevel(linclampSampler, tex,0);

   // compute per instance world space.   
   float3 instPosW = posT + terrainTrans;
   output.posW = vin.posL * decoScale + instPosW;
      
   float4x4 vp   = mul(cb_view, cb_proj);
   output.posH   = mul(float4(output.posW,1), vp);	
   output.tex = vin.tex;   
   return output;
}


//2- render decoration using camera facing billboards.


float3 VSDecoBB(float2 posL : POSITION) : POSITION
{    
	float3 posT;	
	posT.xz = (cellSize * hmSize) *  posL;
    float2 tex =  posL + hmTexelsize/2.0f;
	posT.y = hnTex.SampleLevel(linclampSampler, tex,0);

   // compute per instance world space.   
   float3 posW = posT + terrainTrans;
   return posW;            
}

[maxvertexcount(4)]
void GSDecoBB(point float3 gin[1] : POSITION, inout TriangleStream<decoPSIn> stream)
{
    float4x4 vp = mul(cb_view, cb_proj);

	float3 up = float3(0.0f, 1.0f , 0.0f);
	float3 look = cb_camPosW - gin[0];
	look.y = 0.0f; 
	look = normalize(look);
	float3 right = cross(up, look);

	
	float halfWidth  = 0.5f*decoScale.x;
	up.y = decoScale.y;
	
	
	float4 v[4];
	v[0] = float4(gin[0] + halfWidth*right, 1.0f);
	v[1] = float4(gin[0] + halfWidth*right + up, 1.0f);
	v[2] = float4(gin[0] - halfWidth*right, 1.0f);
	v[3] = float4(gin[0] - halfWidth*right + up, 1.0f);

	float2 texc[4] = 
	{
		float2(1.0f, 1.0f),
		float2(1.0f, 0.0f),
		float2(0.0f, 1.0f),
		float2(0.0f, 0.0f)
	};
	
	
	//
	// Transform quad vertices to world space and output 
	// them as a triangle strip.
	//
	decoPSIn gout;
	[unroll]
	for(int i = 0; i < 4; ++i)
	{
		gout.posH     = mul(v[i], vp);
		gout.posW     = v[i].xyz;
		//gout.NormalW  = look;
		gout.tex      = texc[i];				
		stream.Append(gout);
	}

}



float4 PSDeco(decoPSIn psIn) : SV_TARGET
{
    float4 fc = {0.7,0.7,0.7,1};
    if (cb_textured)
    {	
	   fc = layers[0].Sample(linwrapSampler,psIn.tex);	   
	   clip(fc.a - 0.03f);	   
	}

	if(cb_lit)
    {
	   fc.xyz *= cb_lightEnv.dirLight[0].diffuse;
	}

	if(fog.enabled)
	{
	   float dist = distance( cb_camPosW, psIn.posW  );	   
	   float foglerp = ComputeFogFactor(fog,dist);
	   fc.xyz = lerp(fog.color.xyz,fc.xyz , foglerp);	   	   
	}
	return fc;
}

//************* end of render decoration layer *******************

//--------------------------------------------------------------
// rendering solid wireframe.
//--------------------------------------------------------------

struct GS_INPUT
{
    float4 posH  : POSITION;    
};

GS_INPUT VSSolidWire( float3 posL : POSITION )
{ 
    GS_INPUT output;

	// posHm  postion in heightmap space.
	float3 posHm = (posL + patchTrans);
	// tex coord generated such that each vertex tex coord corresponds to the 
	// center of texel.
	float2 tex = posHm.xz / hmSize + hmTexelsize/2.0f;

	// PosT   position in terrain space.
	float3 posT = posHm * cellSize;		
	posT.y = hnTex.SampleLevel(hnSampler, tex,0);	
	float3 posW = posT + terrainTrans;	   

	float4x4 vp   = mul(cb_view, cb_proj);
	output.posH   = mul(float4(posW,1), vp);
	return output;
} 
          
#include "SolidWireframeHelper.shh"


//-------------------------------------------------------------------------//
//                         Render normals                                  //
//-------------------------------------------------------------------------//

//--------------------------------------------------------------------------------------
struct VSNormals_OUTPUT
{
    float3 posW : POSITION;
    float3 normW : NORMAL;
};

		                                               
VSNormals_OUTPUT VSNormals( float3 posL : POSITION ) 
{      
    VSNormals_OUTPUT vout;                                                       	

	// position in terrain space.
	// posHm  postion in heightmap space.
	float3 posHm = (posL + patchTrans);
	// tex coord generated such that each vertex tex coord corresponds to the 
	// center of texel.
	float2 tex = posHm.xz / hmSize + hmTexelsize/2.0f;

	// PosT   position in terrain space.
	float3 posT = posHm * cellSize;		
	posT.y = hnTex.SampleLevel(hnSampler, tex,0);	
	vout.posW = posT + terrainTrans;

	// compute normal from height.
	float2 left   =  tex + float2(-hmTexelsize.x,0);
	float2 right  =  tex + float2(hmTexelsize.x,0);
	float2 top    =  tex + float2(0,-hmTexelsize.y);
	float2 bottom =  tex + float2(0,hmTexelsize.y);

	float leftH = hnTex.SampleLevel(hnSampler, left,0);
	float rightH = hnTex.SampleLevel(hnSampler, right,0);
	float topH = hnTex.SampleLevel(hnSampler, top,0);
	float bottomH = hnTex.SampleLevel(hnSampler, bottom,0);

	float3 tangent = float3(2 * cellSize,rightH - leftH, 0);
	float3 bitan   = float3(0, bottomH - topH, 2 * cellSize);
	vout.normW     = normalize( cross(bitan,tangent));
	
	  	
	return vout;	
}                                                             


struct GSNormals_OUT
{
  float4 posH : SV_POSITION;
};



// GS outputs line list.
[maxvertexcount(2)]
void GSNormals( point VSNormals_OUTPUT gIn[1],
         inout LineStream<GSNormals_OUT> outStream )
{
   float4x4 vp   = mul(cb_view, cb_proj);

   // create line from position and normal.
   float3 normW = normalize(gIn[0].normW);
   float3 posW2 = gIn[0].posW + 0.5f * normW;
   GSNormals_OUT p1,p2;
   p1.posH = mul(float4(gIn[0].posW,1),vp);   
   p2.posH = mul(float4(posW2,1),vp);

   outStream.Append(p1);
   outStream.Append(p2);   
}

float4 PSNormals( GSNormals_OUT PIn ) : SV_Target                        
{
   return float4(1,0,0,1);
}    
        