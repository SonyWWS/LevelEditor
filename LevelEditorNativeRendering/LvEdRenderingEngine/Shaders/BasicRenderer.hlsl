//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

// This shader is used by BasicRenderer.h/cpp
// BasicRenderer is mostly used from C# side for rendering 
// Non gameplay visuals

#include "Lighting.shh"

cbuffer ConstantBufferPerFrame  : register( b0 )              
{                                                             
   float4x4 view;                                             
   float4x4 proj; 
   float3   camPosW;
   float    pad;
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

struct VsOut
{
    float4 posH  : SV_POSITION;
    float3 posW  : POSITION;
    float3 normW : NORMAL;    
};

VsOut VS( float4 posL : POSITION, float3 normL : NORMAL)
{            
    VsOut vout = (VsOut)0;                                                 	
	float4x4 wvp = mul(world,mul(view,proj));
	vout.posH = mul( posL, wvp ); 
    if(lit)
    {
       vout.posW = mul( posL, world).xyz;
       vout.normW = mul( normL, (float3x3)worldInvTrans);
    }
	return vout;
}                                                             

float4 PS( VsOut psIn ) : SV_Target                        
{
    float4 fc = color;
    if(lit)
    {
      float3 norm = normalize(psIn.normW);
      float specPower = max(1.0,specular.w);
      float3 A,D,S;    
      float3 toEye = normalize(camPosW - psIn.posW);
      ComputeDirLight(psIn.posW, norm, toEye, specPower, dirlight, A, D, S);
      fc.xyz = color.xyz * (A + D) + specular.xyz * S;				             
    }
    return fc;
}    

        