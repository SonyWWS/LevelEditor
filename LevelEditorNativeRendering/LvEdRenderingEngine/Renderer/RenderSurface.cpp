//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "RenderSurface.h"
#include "../Core/Utils.h"
#include "Texture.h"

namespace LvEdEngine
{



float3 RenderSurface::Project(const float3& v, const Matrix& m)
{

//    float w = v.x * m.M14 + v.y * m.M24 + v.z * m.M34 + m.M44;
    float3 vscr = v.Transform(v,m);    
    vscr.x =((vscr.x + 1) * 0.5f * m_width);
    vscr.y =((1.0f - vscr.y) * 0.5f * m_height);
    return vscr;

}
float3 RenderSurface::Unproject(const float3 &v, const Matrix& invVP)
{	
	float3 vw; // point in world space.	
	vw.x =   v.x/m_width *  2.0f - 1.0f;
	vw.y = -(v.y/m_height * 2.0f - 1.0f);
	vw.z =  v.z;    
	vw.Transform(invVP);	
	return vw;
}


RenderSurface::RenderSurface() :
      m_pRenderTargetView(NULL),	  
	  m_pDepthStencilView(NULL),
      m_pDepthStencilViewFg(NULL),
      m_pDepthStencilBuffer(NULL),
      m_pColorBuffer(NULL),
	  m_width(0),
	  m_height(0)
{
    m_bkgColor = float4( 0.62745f, 0.62745f, 0.62745f, 1.0f ); 
}

RenderSurface::~RenderSurface(void)
{		    			
}

}

