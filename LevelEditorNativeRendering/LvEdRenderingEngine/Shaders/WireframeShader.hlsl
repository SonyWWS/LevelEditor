//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

//-----------------------------------------------
// simplified version of
// Render solid wireframe using Geometry shader.
// From NVidia SDK 10.6    
//-----------------------------------------------

cbuffer cbPerFrame  : register( b0 )
{              
   float4x4 view;
   float4x4 proj;
   float4   Viewport;
};                                 

cbuffer cbPerObject  : register( b1 )
{
   float4x4 world;   
   float4   wirecolor;
};                                      

struct GS_INPUT
{
    float4 posH  : POSITION;    
};

GS_INPUT VSSolidWire( float4 pos : POSITION )
{ 
    GS_INPUT output;
    float4x4 wvp = mul(world,mul(view,proj));
    output.posH = mul( pos, wvp );    
    return output;
}               

#include "SolidWireframeHelper.shh"

