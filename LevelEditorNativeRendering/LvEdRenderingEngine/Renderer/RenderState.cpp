//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "RenderState.h"
#include "ShadowMaps.h"
#include "../Core/Utils.h"
#include "../Core/Logger.h"


using namespace LvEdEngine;

#define MakeRSKey(_FillModeEnum_ , _CullModeEnum_) (  (_FillModeEnum_) << 16 | (_CullModeEnum_) );
    const uint32_t BL_MASK = RenderFlags::AlphaBlend;
    const uint32_t DP_MASK = RenderFlags::DisableDepthTest | RenderFlags::DisableDepthWrite;

    ID3D11RasterizerState* RSCache::GetRasterState(FillModeEnum fillMode, CullModeEnum cullMode)
    {
        uint32_t key = MakeRSKey(fillMode,cullMode);
        auto it = RasterStates.find(key);
        if(it != RasterStates.end())
            return it->second;
        return NULL;            
    }

    ID3D11BlendState* RSCache::GetBlendState(RenderFlagsEnum renderflag)
    {        
        auto it = BlendStates.find(renderflag & BL_MASK);
        if(it != BlendStates.end())
            return it->second;
        return NULL;            
        
    }

    ID3D11DepthStencilState* RSCache::GetDepthStencilState(RenderFlagsEnum renderflag)
    {
        auto it = DepthStencilStates.find(renderflag & DP_MASK);
        if(it != DepthStencilStates.end())
            return it->second;
        return NULL;
        
    }


    
// RSCache      
void RSCache::InitInstance(ID3D11Device* device)
{
    if(s_inst == NULL)
        s_inst = new RSCache(device);
}

void  RSCache::DestroyInstance()
{       
    SAFE_DELETE(s_inst);
}

RSCache* RSCache::s_inst = NULL;
RSCache::RSCache(ID3D11Device* device)
{

    m_resources.clear();

    HRESULT hr = S_OK;
    ID3D11RasterizerState* rasterState;
    uint32_t rsKey = 0;

    // ======================== raster states ==================
    D3D11_RASTERIZER_DESC rsDcr;
    SecureZeroMemory( &rsDcr, sizeof(rsDcr));	
    
    rsDcr.FillMode =  D3D11_FILL_SOLID;        
    rsDcr.CullMode =  D3D11_CULL_BACK;        
    rsDcr.FrontCounterClockwise = TRUE; // front face is CCW like OpenGL
    rsDcr.DepthBias = 0;
    rsDcr.SlopeScaledDepthBias = 0.0f;
    rsDcr.DepthBiasClamp = 0.0f; 
    rsDcr.DepthClipEnable = TRUE;
    rsDcr.ScissorEnable = FALSE; 
    rsDcr.MultisampleEnable = TRUE;
    rsDcr.AntialiasedLineEnable = FALSE;
    m_defRsDcr = rsDcr; // save default 
            
    // solid and cull back faces
    rsDcr.CullMode =  D3D11_CULL_BACK;        
    hr = device->CreateRasterizerState(&rsDcr,  &rasterState);
    if (Logger::IsFailureLog(hr))
        return;
    m_resources.push_back(rasterState);
    m_solidCullBack = rasterState;
    rsKey = MakeRSKey(FillMode::Solid, CullMode::BACK);
    RasterStates[rsKey] = rasterState;
           
    // solid  and cull front faces
    rsDcr.CullMode =  D3D11_CULL_FRONT;    
    hr = device->CreateRasterizerState(&rsDcr,  &rasterState);
    if (Logger::IsFailureLog(hr))
        return;	
    m_resources.push_back(rasterState);

    m_solidCullFront = rasterState;         
    rsKey = MakeRSKey(FillMode::Solid, CullMode::FRONT);
    RasterStates[rsKey] = rasterState;

    // solid  and cull none 
    rsDcr.CullMode =  D3D11_CULL_NONE;    
    hr = device->CreateRasterizerState(&rsDcr,  &rasterState);
    if (Logger::IsFailureLog(hr))
        return;       
    m_resources.push_back(rasterState);

    m_solidCullNone = rasterState;        
    rsKey = MakeRSKey(FillMode::Solid, CullMode::NONE);
    RasterStates[rsKey] = rasterState;
	
    
    // wire frame and cullback 
    rsDcr.AntialiasedLineEnable = TRUE;
    rsDcr.FillMode =  D3D11_FILL_WIREFRAME;

    // Wireframe and cull back faces
    rsDcr.CullMode =  D3D11_CULL_BACK;        
    hr = device->CreateRasterizerState(&rsDcr,  &rasterState);
    if (Logger::IsFailureLog(hr))
        return;
    m_resources.push_back(rasterState);

    m_wireCullBack = rasterState;                 
    rsKey = MakeRSKey(FillMode::Wireframe, CullMode::BACK);
    RasterStates[rsKey] = rasterState;
           
    // Wireframe  and cull front faces
    rsDcr.CullMode =  D3D11_CULL_FRONT;    
    hr = device->CreateRasterizerState(&rsDcr,  &rasterState);
    if (Logger::IsFailureLog(hr))
        return;

    m_resources.push_back(rasterState);
    m_wireCullFront = rasterState;                
    rsKey = MakeRSKey(FillMode::Wireframe, CullMode::FRONT);
    RasterStates[rsKey] = rasterState;

    // Wireframe  and cull none 
    rsDcr.CullMode =  D3D11_CULL_NONE;    
    hr = device->CreateRasterizerState(&rsDcr,  &rasterState);
    if (Logger::IsFailureLog(hr))
        return;

    m_resources.push_back(rasterState);
    m_wireCullNone = rasterState;        
    rsKey = MakeRSKey(FillMode::Wireframe, CullMode::NONE);
    RasterStates[rsKey] = rasterState;

    // ============= create depth stencil state blocks =============
    D3D11_DEPTH_STENCIL_DESC dsDcr;
    SecureZeroMemory( &dsDcr, sizeof(dsDcr));		
    dsDcr.DepthEnable = TRUE;
    dsDcr.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
    dsDcr.DepthFunc = D3D11_COMPARISON_LESS;
    dsDcr.StencilEnable = FALSE;
    dsDcr.StencilReadMask = D3D11_DEFAULT_STENCIL_READ_MASK;
    dsDcr.StencilWriteMask = D3D11_DEFAULT_STENCIL_WRITE_MASK;
    dsDcr.FrontFace.StencilFunc = D3D11_COMPARISON_ALWAYS;
    dsDcr.BackFace.StencilFunc = D3D11_COMPARISON_ALWAYS;
    dsDcr.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP; 
    dsDcr.BackFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;
    dsDcr.FrontFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;
    dsDcr.BackFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;
    dsDcr.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP; 
    dsDcr.BackFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;
    m_defDpDcr = dsDcr; // save default;
    
    ID3D11DepthStencilState* depthState;
    hr = device->CreateDepthStencilState(&dsDcr,&depthState);
    if (Logger::IsFailureLog(hr))
        return;

    m_resources.push_back(depthState);
    m_depthTestAndWrite = depthState;
    

    RenderFlagsEnum rf = (RenderFlagsEnum)0;
    DepthStencilStates[rf] = depthState;

    // test no-write
    dsDcr.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ZERO;
    hr = device->CreateDepthStencilState(&dsDcr,&depthState);
    if (Logger::IsFailureLog(hr))
        return;
    m_resources.push_back(depthState);
    m_depthTestOnly = depthState;
    
    rf = RenderFlags::DisableDepthWrite;
    DepthStencilStates[rf] = depthState;

    // no-test  write
    dsDcr.DepthEnable = FALSE;
    dsDcr.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
    hr = device->CreateDepthStencilState(&dsDcr,&depthState);
    if (Logger::IsFailureLog(hr))
        return;
    m_resources.push_back(depthState);
    m_depthWriteOnly = depthState;
    
    rf = RenderFlags::DisableDepthTest;
    DepthStencilStates[rf] = depthState;

    // no-test no-write
    dsDcr.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ZERO;        
    hr = device->CreateDepthStencilState(&dsDcr,&depthState);
    if (Logger::IsFailureLog(hr))
        return;
    m_resources.push_back(depthState);
    m_depthNoTestNoWrite = depthState;
    rf = (RenderFlagsEnum)(RenderFlags::DisableDepthTest | RenderFlags::DisableDepthWrite);        
    DepthStencilStates[rf] = depthState;  


    // =================== create blend states. ==================

    D3D11_BLEND_DESC blendDcr;
    SecureZeroMemory( &blendDcr, sizeof(blendDcr));
    blendDcr.AlphaToCoverageEnable = FALSE;
    blendDcr.IndependentBlendEnable = FALSE;
    
    D3D11_RENDER_TARGET_BLEND_DESC rtblendDcr;
    SecureZeroMemory( &rtblendDcr, sizeof(rtblendDcr));
    rtblendDcr.BlendEnable = FALSE;
    rtblendDcr.SrcBlend = D3D11_BLEND_ONE;
    rtblendDcr.DestBlend = D3D11_BLEND_ZERO;
    rtblendDcr.BlendOp = D3D11_BLEND_OP_ADD;
    rtblendDcr.SrcBlendAlpha = D3D11_BLEND_ONE;
    rtblendDcr.DestBlendAlpha = D3D11_BLEND_ZERO;
    rtblendDcr.BlendOpAlpha = D3D11_BLEND_OP_ADD;
    rtblendDcr.RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL; 
    blendDcr.RenderTarget[0] = rtblendDcr;

    m_defBsDcr = blendDcr;  // save default

    // create alphaToCoverage
    blendDcr.AlphaToCoverageEnable = TRUE;
    hr = device->CreateBlendState(&blendDcr,&m_alphaToCoverage);
    if (Logger::IsFailureLog(hr))
        return;
    m_resources.push_back(m_alphaToCoverage);
    

    // create default blendstate 
    blendDcr.AlphaToCoverageEnable = FALSE;
    ID3D11BlendState* blendState;
    hr = device->CreateBlendState(&blendDcr,&blendState);
    if (Logger::IsFailureLog(hr))
        return;
    m_resources.push_back(blendState);
    
    rf = (RenderFlagsEnum)0;        
    BlendStates[rf] = blendState;

    // enable alpha blend
    rtblendDcr.BlendEnable = TRUE;
    rtblendDcr.SrcBlend = D3D11_BLEND_SRC_ALPHA;
    rtblendDcr.DestBlend = D3D11_BLEND_INV_SRC_ALPHA;
    blendDcr.RenderTarget[0] = rtblendDcr;
    hr = device->CreateBlendState(&blendDcr,&blendState);
    if (Logger::IsFailureLog(hr))
        return;
    m_resources.push_back(blendState);
    
    
    rf = RenderFlags::AlphaBlend;
    BlendStates[rf] = blendState;    



    // ================== sampler states ========================
    D3D11_SAMPLER_DESC samDecr;
    SecureZeroMemory( &samDecr, sizeof(samDecr));
    samDecr.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
    samDecr.AddressU = D3D11_TEXTURE_ADDRESS_WRAP;
    samDecr.AddressV = D3D11_TEXTURE_ADDRESS_WRAP;
    samDecr.AddressW = D3D11_TEXTURE_ADDRESS_WRAP;
    samDecr.MipLODBias = 0.0f;
    samDecr.MaxAnisotropy = 1;
    samDecr.ComparisonFunc = D3D11_COMPARISON_NEVER;        
    samDecr.MinLOD = -FLT_MAX;
    samDecr.MaxLOD = FLT_MAX;
    samDecr.BorderColor[0] = 1.0f;
    samDecr.BorderColor[1] = 1.0f;
    samDecr.BorderColor[2] = 1.0f;
    samDecr.BorderColor[3] = 1.0f;
    m_defSampler = samDecr; // save default;

    m_linearWrap = NULL;
    hr = device->CreateSamplerState( &samDecr, &m_linearWrap );
    assert(m_linearWrap);
    m_resources.push_back(m_linearWrap);

    m_linearClamp = NULL;
    samDecr.AddressU = D3D11_TEXTURE_ADDRESS_CLAMP;
    samDecr.AddressV = D3D11_TEXTURE_ADDRESS_CLAMP;
    samDecr.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
    hr = device->CreateSamplerState( &samDecr, &m_linearClamp );
    assert(m_linearClamp);
    m_resources.push_back(m_linearClamp);

    m_pointClamp = NULL;
    samDecr.Filter = D3D11_FILTER_MIN_MAG_MIP_POINT;
    hr = device->CreateSamplerState( &samDecr, &m_pointClamp );
    assert(m_pointClamp);
    m_resources.push_back(m_pointClamp);

    m_pointWrap = NULL;
    samDecr.AddressU = D3D11_TEXTURE_ADDRESS_WRAP;
    samDecr.AddressV = D3D11_TEXTURE_ADDRESS_WRAP;
    samDecr.AddressW = D3D11_TEXTURE_ADDRESS_WRAP;
    hr = device->CreateSamplerState( &samDecr, &m_pointWrap );
    assert(m_pointWrap);
    m_resources.push_back(m_pointWrap);

    m_anisotropicWrap = NULL;        
    samDecr.Filter = D3D11_FILTER_ANISOTROPIC;
    hr = device->CreateSamplerState( &samDecr, &m_anisotropicWrap );
    assert(m_anisotropicWrap);
    m_resources.push_back(m_anisotropicWrap);

    m_anisotropicClamp = NULL;
    samDecr.AddressU = D3D11_TEXTURE_ADDRESS_CLAMP;
    samDecr.AddressV = D3D11_TEXTURE_ADDRESS_CLAMP;
    samDecr.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
    hr = device->CreateSamplerState( &samDecr, &m_anisotropicClamp );
    assert(m_anisotropicClamp);
    m_resources.push_back(m_anisotropicClamp);
}

RSCache::~RSCache()
{            
    for(auto it = m_resources.begin(); it != m_resources.end(); it++)
            (*it)->Release(); 
    m_resources.clear();
}


