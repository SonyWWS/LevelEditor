//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "RenderState.h"
#include "ShadowMaps.h"
#include <DxErr.h>
#include "../Core/Utils.h"



namespace LvEdEngine
{

#define MakeRSKey(_FillModeEnum_ , _CullModeEnum_) (  (_FillModeEnum_) << 16 | (_CullModeEnum_) );
    const uint32_t BL_MASK = RenderFlags::AlphaBlend;
    const uint32_t DP_MASK = RenderFlags::DisableDepthTest | RenderFlags::DisableDepthWrite;

    ID3D11RasterizerState* RenderStateCache::GetRasterState(FillModeEnum fillMode, CullModeEnum cullMode)
    {
        uint32_t key = MakeRSKey(fillMode,cullMode);
        auto it = RasterStates.find(key);
        if(it != RasterStates.end())
            return it->second;
        return NULL;            
    }

    ID3D11BlendState* RenderStateCache::GetBlendState(RenderFlagsEnum renderflag)
    {        
        auto it = BlendStates.find(renderflag & BL_MASK);
        if(it != BlendStates.end())
            return it->second;
        return NULL;            
        
    }

    ID3D11DepthStencilState* RenderStateCache::GetDepthStencilState(RenderFlagsEnum renderflag)
    {
        auto it = DepthStencilStates.find(renderflag & DP_MASK);
        if(it != DepthStencilStates.end())
            return it->second;
        return NULL;
        
    }

    // RenderStateCache      
    RenderStateCache::RenderStateCache(ID3D11Device* device)
    {
        HRESULT hr = S_OK;
        ID3D11RasterizerState* rasterState;
        uint32_t rsKey = 0;

        D3D11_RASTERIZER_DESC rsDcr;
        SecureZeroMemory( &rsDcr, sizeof(rsDcr));	

        // solid and cullback
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
        if(FAILED(hr))
        {
            const WCHAR *wstr =  DXGetErrorString( hr);
            wprintf(L"%s\n",wstr);
            return;			 
        }                
        rsKey = MakeRSKey(FillMode::Solid, CullMode::BACK);
        RasterStates[rsKey] = rasterState;
               
        // solid  and cull front faces
        rsDcr.CullMode =  D3D11_CULL_FRONT;    
        hr = device->CreateRasterizerState(&rsDcr,  &rasterState);
        if(FAILED(hr))
        {
            const WCHAR *wstr =  DXGetErrorString( hr);
            wprintf(L"%s\n",wstr);
            return;			 
        }
        rsKey = MakeRSKey(FillMode::Solid, CullMode::FRONT);
        RasterStates[rsKey] = rasterState;

        // solid  and cull none 
        rsDcr.CullMode =  D3D11_CULL_NONE;    
        hr = device->CreateRasterizerState(&rsDcr,  &rasterState);
        if(FAILED(hr))
        {
            const WCHAR *wstr =  DXGetErrorString( hr);
            wprintf(L"%s\n",wstr);
            return;			 
        }
        rsKey = MakeRSKey(FillMode::Solid, CullMode::NONE);
        RasterStates[rsKey] = rasterState;
	
    
        // wire frame and cullback 
        rsDcr.AntialiasedLineEnable = TRUE;
        rsDcr.FillMode =  D3D11_FILL_WIREFRAME;

        // Wireframe and cull back faces
        rsDcr.CullMode =  D3D11_CULL_BACK;        
        hr = device->CreateRasterizerState(&rsDcr,  &rasterState);
        if(FAILED(hr))
        {
            const WCHAR *wstr =  DXGetErrorString( hr);
            wprintf(L"%s\n",wstr);
            return;			 
        }                
        rsKey = MakeRSKey(FillMode::Wireframe, CullMode::BACK);
        RasterStates[rsKey] = rasterState;
               
        // Wireframe  and cull front faces
        rsDcr.CullMode =  D3D11_CULL_FRONT;    
        hr = device->CreateRasterizerState(&rsDcr,  &rasterState);
        if(FAILED(hr))
        {
            const WCHAR *wstr =  DXGetErrorString( hr);
            wprintf(L"%s\n",wstr);
            return;			 
        }
        rsKey = MakeRSKey(FillMode::Wireframe, CullMode::FRONT);
        RasterStates[rsKey] = rasterState;

        // Wireframe  and cull none 
        rsDcr.CullMode =  D3D11_CULL_NONE;    
        hr = device->CreateRasterizerState(&rsDcr,  &rasterState);
        if(FAILED(hr))
        {
            const WCHAR *wstr =  DXGetErrorString( hr);
            wprintf(L"%s\n",wstr);
            return;			 
        }
        rsKey = MakeRSKey(FillMode::Wireframe, CullMode::NONE);
        RasterStates[rsKey] = rasterState;


        // create depth stencil state blocks

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
        m_defDpDcr = dsDcr;
        
        ID3D11DepthStencilState* depthState;
        hr = device->CreateDepthStencilState(&dsDcr,&depthState);
        if(FAILED(hr))
        {
            const WCHAR *wstr =  DXGetErrorString( hr);
            wprintf(L"%s\n",wstr);
            return;			 
        }
        RenderFlagsEnum rf = (RenderFlagsEnum)0;
        DepthStencilStates[rf] = depthState;

        // test no-write
        dsDcr.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ZERO;
        hr = device->CreateDepthStencilState(&dsDcr,&depthState);
        if(FAILED(hr))
        {
            const WCHAR *wstr =  DXGetErrorString( hr);
            wprintf(L"%s\n",wstr);
            return;			 
        }
        rf = RenderFlags::DisableDepthWrite;
        DepthStencilStates[rf] = depthState;

        // no-test  write
        dsDcr.DepthEnable = FALSE;
        dsDcr.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
        hr = device->CreateDepthStencilState(&dsDcr,&depthState);
        if(FAILED(hr))
        {
            const WCHAR *wstr =  DXGetErrorString( hr);
            wprintf(L"%s\n",wstr);
            return;			 
        }
        rf = RenderFlags::DisableDepthTest;
        DepthStencilStates[rf] = depthState;

        // no-test no-write
        dsDcr.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ZERO;        
        hr = device->CreateDepthStencilState(&dsDcr,&depthState);
        if(FAILED(hr))
        {
            const WCHAR *wstr =  DXGetErrorString( hr);
            wprintf(L"%s\n",wstr);
            return;			 
        }
        rf = (RenderFlagsEnum)(RenderFlags::DisableDepthTest | RenderFlags::DisableDepthWrite);        
        DepthStencilStates[rf] = depthState;  


        // create blend states.

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

        m_defBsDcr = blendDcr;



        // crate alphaToCoverage
        blendDcr.AlphaToCoverageEnable = TRUE;
        hr = device->CreateBlendState(&blendDcr,&m_alphaToCoverage);
        if(FAILED(hr))
        {
            const WCHAR *wstr =  DXGetErrorString( hr);
            wprintf(L"%s\n",wstr);
            return;			 
        }        

        // create default blendstate 
        blendDcr.AlphaToCoverageEnable = FALSE;
        ID3D11BlendState* blendState;
        hr = device->CreateBlendState(&blendDcr,&blendState);
        if(FAILED(hr))
        {
            const WCHAR *wstr =  DXGetErrorString( hr);
            wprintf(L"%s\n",wstr);
            return;			 
        }        
        rf = (RenderFlagsEnum)0;        
        BlendStates[rf] = blendState;

        // enable alpha blend
        rtblendDcr.BlendEnable = TRUE;
        rtblendDcr.SrcBlend = D3D11_BLEND_SRC_ALPHA;
        rtblendDcr.DestBlend = D3D11_BLEND_INV_SRC_ALPHA;
        blendDcr.RenderTarget[0] = rtblendDcr;
        hr = device->CreateBlendState(&blendDcr,&blendState);
        if(FAILED(hr))
        {
            const WCHAR *wstr =  DXGetErrorString( hr);
            wprintf(L"%s\n",wstr);
            return;			 
        }
        rf = RenderFlags::AlphaBlend;
        BlendStates[rf] = blendState;    



        // create default sampler state.   
        // D3D11_SAMPLER_DESC
        SecureZeroMemory( &m_defSampler, sizeof(m_defSampler));
        m_defSampler.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
        m_defSampler.AddressU = D3D11_TEXTURE_ADDRESS_WRAP;
        m_defSampler.AddressV = D3D11_TEXTURE_ADDRESS_WRAP;
        m_defSampler.AddressW = D3D11_TEXTURE_ADDRESS_WRAP;
        m_defSampler.MipLODBias = 0.0f;
        m_defSampler.MaxAnisotropy = 1;
        m_defSampler.ComparisonFunc = D3D11_COMPARISON_ALWAYS;        
        m_defSampler.MinLOD = 0;
        m_defSampler.MaxLOD = D3D11_FLOAT32_MAX;
    }
    RenderStateCache::~RenderStateCache()
    {
        for(auto it = DepthStencilStates.begin(); it != DepthStencilStates.end(); it++)
        {
            it->second->Release();         
        }
        DepthStencilStates.clear();
        for(auto it = RasterStates.begin(); it != RasterStates.end(); it++)
        {
            it->second->Release();         
        }
        RasterStates.clear();

        for(auto it = BlendStates.begin(); it != BlendStates.end(); it++)
        {
            it->second->Release();         
        }
        BlendStates.clear();

        SAFE_RELEASE(m_alphaToCoverage);
    }

};
