//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

Texture2D fontAtlas : register( t0 );

SamplerState samLinear
{
	Filter = MIN_MAG_MIP_LINEAR;

	AddressU = WRAP;
	AddressV = WRAP;
};

struct VertexIn
{
	float3 PosNdc : POSITION;
	float2 Tex    : TEXCOORD;
	float4 Color  : COLOR;
};

struct VertexOut
{
	float4 PosNdc : SV_POSITION;
    float2 Tex    : TEXCOORD;
	float4 Color  : COLOR;
};

VertexOut VS(VertexIn vin)
{
	VertexOut vout;

	// It's already in Ndc space
	vout.PosNdc = float4(vin.PosNdc, 1.0f);
	vout.Tex    = vin.Tex;
	vout.Color  = vin.Color;

	return vout;
}

float4 PS(VertexOut pin) : SV_Target
{
	return ( pin.Color * fontAtlas.Sample( samLinear, pin.Tex) );
}
