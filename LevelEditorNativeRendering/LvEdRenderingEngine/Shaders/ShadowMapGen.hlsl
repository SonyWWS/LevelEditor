
//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

//---------------------------------------------------------------------------
//  ShadowMapGen.hlsl
//
//  Writes depth information to the shadow map.
//--------------------------------------------------------------------------------------

cbuffer cbPerFrame : register( b0 )
{
    matrix          view;
	matrix			proj;
};

cbuffer cbPerDraw : register( b1 )
{
    matrix          world;    
};
    
float4 VSMain( float4 posL : POSITION) : SV_POSITION
{    
	float4x4 wvp = mul(world,mul(view,proj));    
	return  mul( posL, wvp );    
}
