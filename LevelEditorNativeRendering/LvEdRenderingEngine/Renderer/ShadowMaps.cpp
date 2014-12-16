//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    ShadowMaps.cpp

****************************************************************************/
#include "../VectorMath/V3dMath.h"
#include "Lights.h"
#include "RenderBuffer.h"
#include "RenderUtil.h"
#include "ShadowMaps.h"
#include "RenderState.h"
#include "../Core/Utils.h"
#include "../Core/Logger.h"

using namespace LvEdEngine;



ShadowMaps*   ShadowMaps::s_inst = NULL;
void ShadowMaps::InitInstance(ID3D11Device* device, uint32_t dim)
{
    if(s_inst == NULL)
        s_inst = new ShadowMaps(device,dim);
}

void ShadowMaps::DestroyInstance()
{
    SAFE_DELETE(s_inst);
}

//---------------------------------------------------------------------------
ShadowMaps::ShadowMaps(ID3D11Device* device, uint32_t dim) 
{
    HRESULT hr;
    m_depthStencilView = NULL;
    m_samplerState     = NULL;
    m_resourceView     = NULL;

    // create depth buffer.
    D3D11_TEXTURE2D_DESC tex2dDesc;
    SecureZeroMemory( &tex2dDesc, sizeof(tex2dDesc) );
    tex2dDesc.Width                 = dim;
    tex2dDesc.Height                = dim;
    tex2dDesc.MipLevels             = 1;
    tex2dDesc.ArraySize             = 1;
    tex2dDesc.Format                = DXGI_FORMAT_R32_TYPELESS;
    tex2dDesc.SampleDesc.Count      = 1;
    tex2dDesc.SampleDesc.Quality    = 0;
    tex2dDesc.Usage                 = D3D11_USAGE_DEFAULT;
    tex2dDesc.BindFlags             = D3D11_BIND_DEPTH_STENCIL|D3D11_BIND_SHADER_RESOURCE;
    tex2dDesc.CPUAccessFlags        = 0;
    tex2dDesc.MiscFlags             = 0;
    ID3D11Texture2D* depthbuffer = NULL;
    hr = device->CreateTexture2D( &tex2dDesc, NULL, &depthbuffer );
    Logger::IsFailureLog(hr, L"Create depth buffer");
    assert(depthbuffer);
  
    D3D11_DEPTH_STENCIL_VIEW_DESC  depthStencilViewDsc;
    SecureZeroMemory( &depthStencilViewDsc, sizeof(depthStencilViewDsc) );
    depthStencilViewDsc.Format = DXGI_FORMAT_D32_FLOAT;    
    depthStencilViewDsc.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2D;    
    hr = device->CreateDepthStencilView( depthbuffer, &depthStencilViewDsc, &m_depthStencilView );
    Logger::IsFailureLog(hr, L"CreateDepthStencilView");
    assert(m_depthStencilView);
    
    D3D11_SHADER_RESOURCE_VIEW_DESC shaderResourceViewDsc;
    SecureZeroMemory( &shaderResourceViewDsc, sizeof(shaderResourceViewDsc) );
    shaderResourceViewDsc.Format = DXGI_FORMAT_R32_FLOAT;
    shaderResourceViewDsc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D;
    shaderResourceViewDsc.Texture2D.MipLevels = 1;
    
    hr = device->CreateShaderResourceView( depthbuffer, &shaderResourceViewDsc, &m_resourceView  );
    Logger::IsFailureLog(hr, L"CreateShaderResourceView");
    assert(m_resourceView);    
    depthbuffer->Release();


    // create sampler state         
    D3D11_SAMPLER_DESC SamDesc = RSCache::Inst()->GetDefaultSampler();
    SamDesc.Filter = D3D11_FILTER_COMPARISON_MIN_MAG_LINEAR_MIP_POINT;
    SamDesc.AddressU = D3D11_TEXTURE_ADDRESS_BORDER;
    SamDesc.AddressV = D3D11_TEXTURE_ADDRESS_BORDER;
    SamDesc.AddressW = D3D11_TEXTURE_ADDRESS_BORDER;
    SamDesc.MipLODBias = 0.0f;    
    SamDesc.MinLOD = 0;
    SamDesc.MaxLOD = 0;
    SamDesc.ComparisonFunc = D3D11_COMPARISON_LESS;
    SamDesc.BorderColor[0] = SamDesc.BorderColor[1] = SamDesc.BorderColor[2] = SamDesc.BorderColor[3] = 0;    
    hr = device->CreateSamplerState( &SamDesc, &m_samplerState );
    Logger::IsFailureLog(hr, L"CreateSamplerState");
    assert(m_samplerState);    
    
    // set viewport
    m_viewport.TopLeftX = 0.0f;
    m_viewport.TopLeftY = 0.0f;
    m_viewport.Width    = static_cast<float>(dim);
    m_viewport.Height   = static_cast<float>(dim);
    m_viewport.MinDepth = 0.0f;
    m_viewport.MaxDepth = 1.0f;


    // create shadow constant buffer
    m_cbShadow.Construct(device);    
}


ShadowMaps::~ShadowMaps()
{
    SAFE_RELEASE(m_samplerState);
    SAFE_RELEASE(m_resourceView);
    SAFE_RELEASE(m_depthStencilView);    
}

//---------------------------------------------------------------------------
void ShadowMaps::UpdateLightCamera( ID3D11DeviceContext* dc, const DirLight* light, const AABB& renderedArea )
{
    float3 worldUp(0,1,0);
    float3 lightDir = normalize(light->dir);

    // compute ligtcam params.
    float dt = dot(-lightDir, worldUp);
    float3 center = renderedArea.GetCenter();
    float dim = length(renderedArea.Max() - renderedArea.Min());
    float radi = dim * 0.5f;

    float3 camPos = center - (radi * lightDir);
    float3 up;
    float3 right;

    if ((dt + Epsilon) >= 1)
    {
        up = float3(0, 0, -1);
        right = float3(1, 0, 0);

    }
    else
    {
        right = normalize(cross(lightDir, worldUp));
        up = normalize(cross(right, lightDir));
        right = cross(lightDir, worldUp);
        up = cross(right, lightDir);
    }


    // create view matrix from right, up and look, and position.
     float rp = -dot(right, camPos);
     float upp = -dot(up, camPos);
     float zp = dot(lightDir, camPos);
     Matrix view(
                right.x, up.x, -lightDir.x, 0.0f,
                right.y, up.y, -lightDir.y, 0.0f,
                right.z, up.z, -lightDir.z, 0.0f,
                rp, upp, zp, 1.0f);

    // compute the width, height, near, far by transforming the AABB into view space.
    AABB lbounds = renderedArea;
    lbounds.Transform(view);
    float3 vmin = lbounds.Min();
    float3 vmax = lbounds.Max();
    float width = vmax.x - vmin.x;
    float height = vmax.y - vmin.y;
    float nearz = 0.0f;
    float farz = dim;

    Matrix proj = Matrix::CreateOrthographic(width,height, nearz, farz);
    //Matrix proj = Matrix::CreateOrthographicOffCenter(vmin.x, vmax.x, vmin.y, vmax.y, nearz, farz);
    m_lightCamera.SetViewProj(view, proj);

    // update cb        
    m_cbShadow.Data.texelSize  = ( 1.0f / MapSize() );        

    // udpate constant buffer using lightcamera.
    // transform coords from NDC space to texture space.
    float4x4 ndcToTexSpace(  0.5f,  0.0f, 0.0f, 0.0f,
                             0.0f, -0.5f, 0.0f, 0.0f,
                             0.0f,  0.0f, 1.0f, 0.0f,
                             0.5f,  0.5f, 0.0f, 1.0f);
                                 
    float4x4 shadowViewProjection = (m_lightCamera.View() * m_lightCamera.Proj()) * ndcToTexSpace;
    Matrix::Transpose(shadowViewProjection, m_cbShadow.Data.xform);    
    m_cbShadow.Update(dc);
}

void ShadowMaps::SetAndClear(ID3D11DeviceContext* dc)
{
    dc->RSSetViewports(1,&m_viewport);    
    dc->OMSetRenderTargets(0, NULL, m_depthStencilView);
    dc->ClearDepthStencilView(m_depthStencilView, D3D11_CLEAR_DEPTH, 1.0f, 0);
}
