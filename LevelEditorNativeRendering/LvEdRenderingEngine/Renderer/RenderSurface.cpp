//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "RenderSurface.h"
#include "../Core/Utils.h"
#include "Texture.h"

namespace LvEdEngine
{

float3 RenderSurface::Unproject(const float3 &v, Matrix& invVP)
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

