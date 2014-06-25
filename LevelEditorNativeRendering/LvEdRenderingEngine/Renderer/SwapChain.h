//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include "../Core/WinHeaders.h"
#include "RenderSurface.h"
#include "../Core/Object.h"

namespace LvEdEngine
{

class SwapChain : public RenderSurface
{
public:
	SwapChain(HWND hwnd, ID3D11Device* pd3dDevice, IDXGIFactory1* pDXGIFactory1);

	virtual ~SwapChain(void);
    virtual const char* ClassName() const {return StaticClassName();}
    static const char* StaticClassName(){return "SwapChain";}
    
	
	IDXGISwapChain* GetDXGISwapChain();
	void Resize(int w, int h);
	
    int GetMultiSampleCount()     { return m_multiSampleCount; }
    int GetMultiSampleQuality()   { return m_multiSampleQuality; }
	SurfaceType GetType();
	    
private:	
	IDXGIFactory1*          m_pDXGIFactory1;
	ID3D11Device*           m_pd3dDevice;
		
	// render target
	IDXGISwapChain*         m_pSwapChain;
	

    int m_multiSampleCount;    
    int m_multiSampleQuality; 

	
};

}

