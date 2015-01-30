//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "TextureRenderSurface.h"
#include "Texture.h"
#include "../DirectX/DXUtil.h"
#include "../Core/Logger.h"



namespace LvEdEngine
{

//
//void TextureRenderSurface::Begin(ID3D11DeviceContext* dc)
//{
//    dc->RSSetViewports(1, &m_viewport);
//
//    ID3D11RenderTargetView* renderTargets[1] = {m_pRenderTargetView};
//    dc->OMSetRenderTargets(1, renderTargets, m_pDepthStencilView);
//
//    
//    if(m_pRenderTargetView)
//    {
//        float4 bkgcolor = this->GetBkgColor();
//        dc->ClearRenderTargetView(m_pRenderTargetView,&bkgcolor.x);
//    }
//    
//    if(m_pDepthStencilView)
//        dc->ClearDepthStencilView(m_pDepthStencilView, D3D11_CLEAR_DEPTH, 1.0f, 0);
//
//
//}
//void TextureRenderSurface::End()
//{
//}

TextureRenderSurface::TextureRenderSurface(ID3D11Device* pd3dDevice,
  int width,
  int height,
  bool hasDepthBuffer,
  bool hasColorBuffer,
  DXGI_FORMAT colorBufferFormat,
  uint32_t sampleCount,
  uint32_t sampleQuality)
{

    
    DXGI_FORMAT depthBufferFormat = DXGI_FORMAT_D24_UNORM_S8_UINT;

    m_width  = max(1,width);
    m_height = max(1,height);

    
    m_viewport.TopLeftX = 0.0f;
    m_viewport.TopLeftY = 0.0f;
    m_viewport.Width    = (float)(m_width);
    m_viewport.Height   = (float)(m_height);
    m_viewport.MinDepth = 0.0f;
    m_viewport.MaxDepth = 1.0f;


    UINT depthQuality;
    pd3dDevice->CheckMultisampleQualityLevels(depthBufferFormat,sampleCount,&depthQuality);

    UINT colorBufferQuality;
    pd3dDevice->CheckMultisampleQualityLevels(colorBufferFormat,sampleCount,&colorBufferQuality);

    sampleQuality = min(sampleQuality,(min(colorBufferQuality,depthQuality)-1)); 
    
    D3D11_TEXTURE2D_DESC texDescr;
    SecureZeroMemory( &texDescr, sizeof(texDescr) );
    texDescr.Width = m_width;
    texDescr.Height = m_height;
    texDescr.MipLevels = 1;
    texDescr.ArraySize = 1;
    texDescr.SampleDesc.Count = sampleCount;
    texDescr.SampleDesc.Quality = sampleQuality;
    texDescr.Usage = D3D11_USAGE_DEFAULT;

    HRESULT hr;

    if(hasDepthBuffer)
    {
        texDescr.Format =  depthBufferFormat;
        texDescr.BindFlags = D3D11_BIND_DEPTH_STENCIL;
        texDescr.CPUAccessFlags = 0;
        texDescr.MiscFlags = 0;
        ID3D11Texture2D* pDepthStencil; 
        hr = pd3dDevice->CreateTexture2D( &texDescr, NULL, &pDepthStencil);
        if (Logger::IsFailureLog(hr, L"CreateTexture2D"))
        {
            return;
        }        

        DXUtil::SetDebugName(pDepthStencil,  "depth buffer");
        
        
        
        hr = pd3dDevice->CreateDepthStencilView( pDepthStencil, NULL, &m_pDepthStencilView );
        if (Logger::IsFailureLog(hr, L"CreateDepthStencilView"))
        {
            return;
        }
               
        m_pDepthStencilBuffer = new Texture(pDepthStencil,false);
        
    }

    if(hasColorBuffer)
    {
        texDescr.Format =  colorBufferFormat;
        texDescr.BindFlags = D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE;
        texDescr.CPUAccessFlags = 0;
        texDescr.MiscFlags = 0;

        ID3D11Texture2D* colorBuffer; 
        hr = pd3dDevice->CreateTexture2D( &texDescr, NULL, &colorBuffer);
        if (Logger::IsFailureLog(hr, L"CreateTexture2D"))
        {
            return;
        }        
        DXUtil::SetDebugName(colorBuffer,  "color buffer");

        hr = pd3dDevice->CreateRenderTargetView( colorBuffer, NULL, &m_pRenderTargetView );
        if (Logger::IsFailureLog(hr, L"CreateRenderTargetView"))
        {
            return;
        } 

        DXUtil::SetDebugName(m_pRenderTargetView,  "render target view");

        ID3D11ShaderResourceView* colorBufferView;
        hr = pd3dDevice->CreateShaderResourceView(colorBuffer, NULL, &colorBufferView);
        if (Logger::IsFailureLog(hr, L"CreateShaderResourceView"))
        {
            return;
        }

        DXUtil::SetDebugName(colorBufferView,  "color buffer shader-view");

        m_pColorBuffer = new Texture(colorBuffer,colorBufferView);
    }
 }

TextureRenderSurface::~TextureRenderSurface(void)
{
    SAFE_RELEASE(m_pRenderTargetView)
    SAFE_RELEASE(m_pDepthStencilView)
    SAFE_DELETE(m_pDepthStencilBuffer)
    SAFE_DELETE(m_pColorBuffer);

    Logger::Log(OutputMessageType::Debug, "TextureRenderSurface deleted %d %d\n",m_width,m_height);
}


void TextureRenderSurface::Resize(int /*w*/, int /*h*/)
{
    assert(false);
}

}
