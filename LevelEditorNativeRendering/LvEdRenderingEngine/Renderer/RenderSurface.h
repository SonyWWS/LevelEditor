//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include <d3d11.h>
#include "../VectorMath/V3dMath.h"
#include "../Core/Object.h"

// interface on rendering surface.
// render surface can be a swap chain or texture.
// Depending on the implementation ID3D11RenderTargetView and/or ID3D11DepthStencilView 
// can be shared by multiple RenderSurface instances.

namespace LvEdEngine
{
    
class Texture;
class RenderSurface : public Object
{
public: 
	enum SurfaceType
	{
		kSwapChain,
		kTexture2d
	};

    RenderSurface();
    virtual ~RenderSurface(void);


    Texture* GetDepthStencilBuffer(){return m_pDepthStencilBuffer;}
    Texture* GetColorBuffer() {return m_pColorBuffer;}

    ID3D11RenderTargetView* GetRenderTargetView() {return m_pRenderTargetView;}
    ID3D11DepthStencilView* GetDepthStencilView() {return m_pDepthStencilView;}

    void  SetBkgColor(const float4& color){ m_bkgColor = color;}
    const float4& GetBkgColor(){ return m_bkgColor;}
	
    int GetWidth() {return m_width;}
    int GetHeight() {return m_height;}
    
    // project v from 3d space to viewport space.
    // if m is Proj then v is in view space.
    // if m is View * Proj then v is in word space.
    // if m is world * View * Proj then v is in local space.
    float3 Project(const float3& v, const Matrix& m);

	float3 Unproject(const float3 &v, const Matrix& invVP);

    virtual void Resize(int w, int h) = 0;	
    virtual SurfaceType GetType() = 0;

    const D3D11_VIEWPORT& GetViewPort() const
    {
        return m_viewport;
    }
	
    
protected:
    ID3D11RenderTargetView* m_pRenderTargetView;    
    ID3D11DepthStencilView* m_pDepthStencilView;

    Texture* m_pDepthStencilBuffer;
    Texture* m_pColorBuffer;

    int m_width;
	int m_height;

    D3D11_VIEWPORT m_viewport;

private:    
    float4 m_bkgColor;

};

}//namespace LvEdEngine


