//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

//-----------------------------------------------             
// Constant Buffer Variables                                  
//-----------------------------------------------             
                                                              
cbuffer ConstantBufferPerFrame  : register( b0 )              
{                                                             
   float4x4 view;                                             
   float4x4 proj;   
};                                                            
                                                              
cbuffer ConstantBufferPerDraw  : register( b1 )               
{       
   float4x4 world;                                                         
   float4   color;                                              
};                                                            
                                                       
float4 VS( float4 pos : POSITION ) : SV_POSITION
{                                                             	
	float4x4 wvp = mul(world,mul(view,proj));
	float4 posH = mul( pos, wvp );                        
	return posH;
}                                                             

float4 PS( float4 pos : SV_POSITION ) : SV_Target                        
{
   return color;
}    




#include "Lighting.shh"


/*
cbuffer ConstantBufferPerFrame  : register( b0 )              
{                                                             
   float4x4 view;                                             
   float4x4 proj; 
   float3   camPosW;
   DirLight dirlight;
};

cbuffer ConstantBufferPerDraw  : register( b1 )               
{       
   float4x4 world;   
   float4x4 worldInvTrans;                                                      
   float4   color;
   float4   specular; // xyz = specular color and  w = specular power
   bool lit;
};     

*/     

struct PS_PN_INPUT
{
    float4 posH  : SV_POSITION;
    float3 posW  : POSITION;
    float3 normW : NORMAL;    
};

PS_PN_INPUT VS_PN( float4 posL : POSITION, float3 normL : NORMAL) : SV_POSITION
{            
    PS_PN_INPUT vout = (PS_PN_INPUT)0;                                                 	
	float4x4 wvp = mul(world,mul(view,proj));
	vout.posH = mul( posL, wvp ); 
    vout.posW = mul( posL, world).xyz;
    //vout.normW = mul( normL, (float3x3)worldInvTrans);
	return vout;
}                                                             

float4 PS_PN( PS_PN_INPUT psIn ) : SV_Target                        
{
    //float3 norm = normalize(psIn.normW);
    //float specPower = max(1.0,matspecular.a);
    //float specPower = 25; // for testing
    //float3 A,D,S;    
    //float3 toEye = normalize(camPosW - psIn.posW);
    //void ComputeDirLight(psIn.posW, norm, toEye, float specPower, DirLight dl,
    //out float3 ambient, out float3 diffuse, out float3 specular)
    //float4  fc = color.xyz * (A + D) + specular.xyz * S;				       
   return color;
}    

        