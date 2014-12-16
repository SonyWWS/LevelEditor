//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

// Shader for drawing normals


//-----------------------------------------------             
// Constant Buffer Variables                                  
//-----------------------------------------------             
                                                              
cbuffer ConstantBufferPerFrame  : register( b0 )              
{                                                             
   float4x4 vp;
   float4 ViewPort;
   float4 color;
};                                                            
                                                              
cbuffer ConstantBufferPerDraw  : register( b1 )               
{       
   float4x4 world;                                              
   float4x4 worldInvTrans;   
};                                                            
          
//--------------------------------------------------------------------------------------
struct VS_OUTPUT
{
    float3 posW : POSITION;
    float3 normW : NORMAL;
};

		                                               
VS_OUTPUT VS( float4 pos : POSITION, float3 norm : NORMAL ) 
{      
    VS_OUTPUT vout;                                                       	

	vout.posW  = mul( pos, world ).xyz;
	vout.normW = mul(norm,  (float3x3)worldInvTrans);
	return vout;	
}                                                             


struct GS_OUT
{
  float4 posH : SV_POSITION;
};



// GS outputs line list.
[maxvertexcount(2)]
void GS( point VS_OUTPUT gIn[1],
         inout LineStream<GS_OUT> outStream )
{
   
   // create line from position and normal.
   float3 normW = normalize(gIn[0].normW);
   float3 posW2 = gIn[0].posW + 0.5f * normW;
   GS_OUT p1,p2;
   p1.posH = mul(float4(gIn[0].posW,1),vp);   
   p2.posH = mul(float4(posW2,1),vp);

   outStream.Append(p1);
   outStream.Append(p2);   
}

float4 PS( GS_OUT PIn ) : SV_Target                        
{
   return color;
}    
        