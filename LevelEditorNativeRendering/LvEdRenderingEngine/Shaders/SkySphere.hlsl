//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


cbuffer cbPerFrame  : register( b0 )
{              
   float4x4 view; 
   float4x4 proj;   
};
 
TextureCube gCubeMap : register( t0 );
SamplerState cubeMapSampler : register( s0 );                          

struct VS_OUT
{
	float4 posH : SV_POSITION;	
    float3 posW : TEXCOORD;
};

VS_OUT VS_Render( float4 posL : POSITION)
{
  VS_OUT vout;
  float4x4 vp = mul(view,proj);
  vout.posH = mul(posL, vp).xyww;
  vout.posW = posL.xyz;     
  return vout;
}
 
float4 PS_Render(VS_OUT pIn) : SV_Target
{
   // temp solution, simple gradient sky, 
   float3 skyc1 = float3(229.0f/255.0f, 236.0f/255.0f, 244.0f/255.0f);
   float3 skyc2 = float3(128.0f/255.0f, 155.0f/255.0f, 198.0f/255.0f);
   float3 skyc3 = float3(14.0f/255.0f, 65.0f/255.0f, 148.0f/255.0f);


   float3 fc;	
   float skcLerp = saturate(pIn.posW.y * 2);
   float r = 0.1f;
   if( skcLerp <= r)
   {
     fc = lerp(skyc1,skyc2, skcLerp / r);
   }
   else
   {
     fc = lerp(skyc2,skyc3, (skcLerp-r)/(1-r));
   }
   //float3 fc = gCubeMap.Sample(cubeMapSampler, pIn.posW).xyz;
   //float3 fogC = float3(150.0f/255.0f, 178.0f/255.0f, 220.0f/255.0f);
   //fc = lerp(fc, fogC,0.15f);	
   return float4(fc,1);

}
