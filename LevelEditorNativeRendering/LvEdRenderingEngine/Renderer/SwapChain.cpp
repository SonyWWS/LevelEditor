//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include <stdio.h>
#include "SwapChain.h"
#include "../Core/Utils.h"
#include "../Core/Logger.h"
#include "Texture.h"
#include "../DirectX/DXUtil.h"

namespace LvEdEngine
{

SwapChain::SwapChain(HWND hwnd,ID3D11Device* pd3dDevice, IDXGIFactory1* pDXGIFactory1)
	: m_pd3dDevice(pd3dDevice),
      m_pDXGIFactory1(pDXGIFactory1),
	  m_pSwapChain(NULL)
	 
{	    
    m_multiSampleCount = 4;
    m_multiSampleQuality = 0;    
	UINT backbufferQuality;
	m_pd3dDevice->CheckMultisampleQualityLevels(DXGI_FORMAT_R8G8B8A8_UNORM,m_multiSampleCount,&backbufferQuality);

	UINT depthQuality;
	m_pd3dDevice->CheckMultisampleQualityLevels(DXGI_FORMAT_D24_UNORM_S8_UINT,m_multiSampleCount,&depthQuality);

    UINT maxQuality = 4;
    m_multiSampleQuality = min(maxQuality,(min(backbufferQuality,depthQuality)-1)); 
    if(m_multiSampleQuality < 1)
    {
        // reset sample count.
        m_multiSampleCount = 1;
    }
	// create swap chain, render target view, depth buffer.
	RECT rc;
    GetClientRect( hwnd, &rc );
    UINT width = rc.right - rc.left;
    UINT height = rc.bottom - rc.top;
	if(width == 0) width = 16;
	if(height == 0) height = 16;


	DXGI_SWAP_CHAIN_DESC sd;
    SecureZeroMemory( &sd, sizeof( sd ) );
    sd.BufferCount = 1;
    sd.BufferDesc.Width = width;
    sd.BufferDesc.Height = height;
    sd.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;
    sd.BufferDesc.RefreshRate.Numerator = 60;
    sd.BufferDesc.RefreshRate.Denominator = 1;
    sd.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
    sd.OutputWindow = hwnd;
    sd.SampleDesc.Count = GetMultiSampleCount();
    sd.SampleDesc.Quality = GetMultiSampleQuality();
    sd.Windowed = TRUE;

	// create swap chain.
	HRESULT hr = S_OK;		
	hr = m_pDXGIFactory1->CreateSwapChain(m_pd3dDevice,&sd,&m_pSwapChain);
	if (Logger::IsFailureLog(hr, L"CreateSwapChain"))
	{
		return;
	}

	// Create a render target view
    ID3D11Texture2D* pBackBuffer = NULL;
    hr = m_pSwapChain->GetBuffer( 0, __uuidof( ID3D11Texture2D ), ( LPVOID* )&pBackBuffer );
    if (Logger::IsFailureLog(hr, L"GetBuffer"))
	{
		return;
	}
    DXUtil::SetDebugName(pBackBuffer, "renderTarget" );  

    m_pColorBuffer = new Texture(pBackBuffer,false);

    /*D3D11_RENDER_TARGET_VIEW_DESC rtv;
    SecureZeroMemory( &rtv, sizeof( rtv ) );
    rtv.Format = DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;
    rtv.ViewDimension = sd.SampleDesc.Quality == 0 ?        
        D3D11_RTV_DIMENSION_TEXTURE2D : D3D11_RTV_DIMENSION_TEXTURE2DMS;*/
                    
    hr = m_pd3dDevice->CreateRenderTargetView( pBackBuffer, NULL, &m_pRenderTargetView );
        
    if (Logger::IsFailureLog(hr, L"CreateRenderTargetView"))
	{
		return;
	}        

    DXUtil::SetDebugName(m_pRenderTargetView,"renderTargetView");
    
	// Create the depth stencil view
    D3D11_TEXTURE2D_DESC descDepth;
	SecureZeroMemory( &descDepth, sizeof(descDepth) );
    descDepth.Width = width;
    descDepth.Height = height;
    descDepth.MipLevels = 1;
    descDepth.ArraySize = 1;
    descDepth.Format = DXGI_FORMAT_D24_UNORM_S8_UINT;
    descDepth.SampleDesc.Count = GetMultiSampleCount();
    descDepth.SampleDesc.Quality = GetMultiSampleQuality();
    descDepth.Usage = D3D11_USAGE_DEFAULT;
    descDepth.BindFlags = D3D11_BIND_DEPTH_STENCIL;
    descDepth.CPUAccessFlags = 0;
    descDepth.MiscFlags = 0;
    ID3D11Texture2D* pDepthStencil; 
    hr = m_pd3dDevice->CreateTexture2D( &descDepth, NULL, &pDepthStencil);
    if (Logger::IsFailureLog(hr, L"CreateTexture2D"))
	{
		return;
	}        

    DXUtil::SetDebugName(pDepthStencil,"depth buffer"  );
  

    m_pDepthStencilBuffer = new Texture(pDepthStencil,false);
   
    hr = m_pd3dDevice->CreateDepthStencilView( pDepthStencil, NULL, &m_pDepthStencilView );
	if (Logger::IsFailureLog(hr, L"CreateDepthStencilView"))
	{
		return;
	}

    // create depth buffer for Forground scene
    ID3D11Texture2D* pDepthStencilFg; 
    hr = m_pd3dDevice->CreateTexture2D( &descDepth, NULL, &pDepthStencilFg);
    if (Logger::IsFailureLog(hr, L"CreateTexture2D"))
        return;
    hr = m_pd3dDevice->CreateDepthStencilView( pDepthStencilFg, NULL, &m_pDepthStencilViewFg );
	if (Logger::IsFailureLog(hr, L"CreateDepthStencilView"))
        return;
    pDepthStencilFg->Release();
	
    m_width = width;
	m_height = height;	

    m_viewport.TopLeftX = 0.0f;
    m_viewport.TopLeftY = 0.0f;
    m_viewport.Width    = (float)(m_width);
    m_viewport.Height   = (float)(m_height);
    m_viewport.MinDepth = 0.0f;
    m_viewport.MaxDepth = 1.0f;


    DXUtil::SetDebugName(m_pDepthStencilView,"depth buffer view");
    
    Logger::Log(OutputMessageType::Debug, "swapchain created %d %d\n",m_width,m_height);

    
}

SwapChain::~SwapChain(void)
{			
    SAFE_RELEASE(m_pSwapChain)
    SAFE_RELEASE(m_pRenderTargetView)
    SAFE_RELEASE(m_pDepthStencilView)
    SAFE_RELEASE(m_pDepthStencilViewFg)
    SAFE_DELETE(m_pDepthStencilBuffer)
    SAFE_DELETE(m_pColorBuffer);

    Logger::Log(OutputMessageType::Debug, "swapchain deleted %d %d\n",m_width,m_height);
		
}

void SwapChain::Resize(int w, int h)
{	
	
	if(w <= 0) w = 1;
	if(h <= 0) h = 1;

    m_width = w;
	m_height = h; 

	HRESULT hr;	
	m_pRenderTargetView->Release();
	m_pRenderTargetView = NULL;
	m_pDepthStencilView->Release();
	m_pDepthStencilView = NULL;
    SAFE_DELETE(m_pColorBuffer);
    
	hr = m_pSwapChain->ResizeBuffers(0, w, h, DXGI_FORMAT_UNKNOWN, 0);
	if (Logger::IsFailureLog(hr, L"ResizeBuffers"))
	{
		return;
	}
    

	// Get buffer and create a render-target-view.
	ID3D11Texture2D* pBuffer;
	hr = m_pSwapChain->GetBuffer(0, __uuidof( ID3D11Texture2D), (void**) &pBuffer );
	if (Logger::IsFailureLog(hr, L"GetBuffer"))
	{
		return;
	}
    
	m_pColorBuffer = new Texture(pBuffer,false);

	hr = m_pd3dDevice->CreateRenderTargetView(pBuffer, NULL,&m_pRenderTargetView);	
    
	if (Logger::IsFailureLog(hr, L"CreateRenderTargetView"))
	{
		return;
	}
	
    DXUtil::SetDebugName(m_pRenderTargetView,"renderTargetView");
    
	// re-create depth buffer.
	D3D11_TEXTURE2D_DESC depthDescr;	
	m_pDepthStencilBuffer->GetTex()->GetDesc(&depthDescr);
	SAFE_DELETE(m_pDepthStencilBuffer);

	depthDescr.Width = w;
	depthDescr.Height = h;
	
    ID3D11Texture2D* pBackBuffer;
	hr = m_pd3dDevice->CreateTexture2D( &depthDescr, NULL, &pBackBuffer );
    if (Logger::IsFailureLog(hr, L"CreateTexture2D"))
	{
		return;
	}        
    
    DXUtil::SetDebugName(pBackBuffer,"depth buffer");
    
    m_pDepthStencilBuffer = new Texture(pBackBuffer,false);

	hr = m_pd3dDevice->CreateDepthStencilView( pBackBuffer, NULL, &m_pDepthStencilView );
	if (Logger::IsFailureLog(hr, L"CreateDepthStencilView"))
	{
		return;
	}

    DXUtil::SetDebugName(m_pDepthStencilView,"depth buffer view");


    ID3D11Texture2D* pDepthStencilFg; 
    hr = m_pd3dDevice->CreateTexture2D( &depthDescr, NULL, &pDepthStencilFg);
    if (Logger::IsFailureLog(hr, L"CreateTexture2D"))
        return;

    m_pDepthStencilViewFg->Release();
    m_pDepthStencilViewFg = NULL;
    hr = m_pd3dDevice->CreateDepthStencilView( pDepthStencilFg, NULL, &m_pDepthStencilViewFg );
	if (Logger::IsFailureLog(hr, L"CreateDepthStencilView"))
        return;
    pDepthStencilFg->Release();

          
    m_viewport.Width    = (float)(m_width);
    m_viewport.Height   = (float)(m_height);    
}


RenderSurface::SurfaceType SwapChain::GetType()
{
	return kSwapChain;
}

IDXGISwapChain* SwapChain::GetDXGISwapChain()
{
	return m_pSwapChain;
}

}// namespace LvEdEngine
