//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

//#define FLIP_TEXTURE_Y


cbuffer ConstantBufferPerFrame : register( b0 )
{
    float4x4   view;
    float4x4   proj;
};

cbuffer ConstantBufferPerDraw : register( b1 )
{
    float4x4   world;
    float4x4   textureTrans;
    float      intensity;  // 0 to 1
};


//--------------------------------------------------------------------------------------
// Textures and Samplers
//--------------------------------------------------------------------------------------
Texture2D    diffuseTex                     : register( t0 );
SamplerState diffuseSampler                 : register( s0 );


//--------------------------------------------------------------------------------------
// Input / Output structures
//--------------------------------------------------------------------------------------
struct VS_INPUT
{
    float4 posL                             : POSITION;
    float2 tex0                             : TEXCOORD0;
};

struct PS_INPUT
{
    float4 posH                             : SV_POSITION;
    float2 tex0                             : TEXCOORD0;
};

//--------------------------------------------------------------------------------------
// VSMain
//--------------------------------------------------------------------------------------
PS_INPUT VSMain( VS_INPUT input )
{
    PS_INPUT output = (PS_INPUT)0;;

	float4x4 wvp = mul(world,mul(view,proj));

	output.posH  = mul( input.posL, wvp );

	#ifdef FLIP_TEXTURE_Y                                               
	output.tex0 = float2(input.tex0.x,(1.0-input.tex0.y));                 
	#else                                                               
	output.tex0 = input.tex0;                                             
	#endif 

   // transform texture coordinates
   float4 tex = float4(output.tex0.x, output.tex0.y, 0, 1);
   output.tex0 = mul(tex, textureTrans).xy;

   return output;
}


//--------------------------------------------------------------------------------------
//    PSMain
//--------------------------------------------------------------------------------------
float4 PSMain( PS_INPUT input ) : SV_TARGET
{
   float4 fc = diffuseTex.Sample( diffuseSampler, input.tex0 );
   clip(fc.a - 0.5);
   fc.xyz = fc.xyz * intensity;
   return fc;
}
