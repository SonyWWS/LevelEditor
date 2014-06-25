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
        