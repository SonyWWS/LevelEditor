//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <D3D11.h>
#include "../VectorMath/V3dMath.h"
#include "../VectorMath/CollisionPrimitives.h"
#include "../Core/Object.h"
#include "RenderEnums.h"
#include <cstdint>
#include <map>
#include "Lights.h"
#include "../VectorMath/Camera.h"
#include <hash_set>

typedef std::hash_set<ObjectGUID> Selection;

namespace LvEdEngine
{
    class ShadowMaps;

    class RenderState : public Object
    {
    public:
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "RenderState";}

        GlobalRenderFlagsEnum GetGlobalRenderFlags(){return m_renderflags;}
        void SetGlobalRenderFlags(GlobalRenderFlagsEnum renderflags) {m_renderflags = renderflags; }

        void SetWireframeColor(const float4& color){ m_wirecolor= color;}
        const float4 GetWireframeColor() const {return m_wirecolor;}

        void SetSelectionColor(const float4& color){ m_selectionColor= color;}
        const float4 GetSelectionColor() const {return m_selectionColor;}

    private:                
        GlobalRenderFlagsEnum m_renderflags;
        float4 m_wirecolor;
        float4 m_selectionColor;

    };


    // Render state cache.
	class RSCache : public NonCopyable
    {
    public:

        static void              InitInstance(ID3D11Device* device);
        static void              DestroyInstance();
        static RSCache* Inst() { return s_inst; }
        
        D3D11_RASTERIZER_DESC     GetDefaultRsDcr(){return m_defRsDcr;}
        D3D11_DEPTH_STENCIL_DESC  GetDefaultDpDcr(){return m_defDpDcr;}
        D3D11_BLEND_DESC          GetDefaultBsDcr(){return m_defBsDcr;}
        D3D11_SAMPLER_DESC        GetDefaultSampler() {return m_defSampler;}
        ID3D11RasterizerState* GetRasterState(FillModeEnum fillMode, CullModeEnum cullMode);
        ID3D11DepthStencilState* GetDepthStencilState(RenderFlagsEnum renderflag);
        ID3D11BlendState* GetBlendState(RenderFlagsEnum renderflag);
        ID3D11BlendState* GetAlphaToCoverageState() { return m_alphaToCoverage;}

        //// render states accessors        
        //ID3D11BlendState*  Opaque() const;
        //ID3D11BlendState*  AlphaBlend() const;
        //ID3D11BlendState*  Additive() const;
        //ID3D11BlendState*  NonPremultiplied() const;

        // Raster states
        ID3D11RasterizerState*  SolidCullNone() const { return m_solidCullNone; }
        ID3D11RasterizerState*  SolidCullBack() const { return m_solidCullBack; }
        ID3D11RasterizerState*  SolidCullFront() const {return m_solidCullFront;}
        ID3D11RasterizerState*  WireCullNone()  const { return m_wireCullNone; }
        ID3D11RasterizerState*  WireCullBack()  const { return m_wireCullBack; }
        ID3D11RasterizerState*  WireCullFront() const { return m_wireCullFront; }
         
        // Depth state.
        ID3D11DepthStencilState*  DepthTestAndWrite() const { return m_depthTestAndWrite;  }
        ID3D11DepthStencilState*  DepthTestOnly() const { return m_depthTestOnly; }
        ID3D11DepthStencilState*  DepthWriteOnly() const { return m_depthWriteOnly; }
        ID3D11DepthStencilState*  DepthNoTestNoWrite() const { return m_depthNoTestNoWrite; }

        // Sampler states.
        ID3D11SamplerState* PointWrap() const { return m_pointWrap; }
        ID3D11SamplerState* PointClamp() const { return m_pointClamp; }
        ID3D11SamplerState* LinearWrap() const { return m_linearWrap; }
        ID3D11SamplerState* LinearClamp() const { return m_linearClamp; }
        ID3D11SamplerState* AnisotropicWrap() const { return m_anisotropicWrap; }
        ID3D11SamplerState* AnisotropicClamp() const { return m_anisotropicClamp; }

    private:  

        RSCache(ID3D11Device* device);
        ~RSCache();
        static RSCache* s_inst;

        // list of all the resources.       
        std::vector<IUnknown*> m_resources;


        std::map<uint32_t,ID3D11RasterizerState*> RasterStates;
        std::map<uint32_t,ID3D11DepthStencilState*> DepthStencilStates;        
        std::map<uint32_t,ID3D11BlendState*> BlendStates;
        ID3D11BlendState*  m_alphaToCoverage;
        D3D11_RASTERIZER_DESC     m_defRsDcr;
        D3D11_DEPTH_STENCIL_DESC  m_defDpDcr;
        D3D11_BLEND_DESC          m_defBsDcr;
        D3D11_SAMPLER_DESC        m_defSampler;

        // Raster states
        ID3D11RasterizerState*  m_solidCullNone;
        ID3D11RasterizerState*  m_solidCullBack;
        ID3D11RasterizerState*  m_solidCullFront;
        ID3D11RasterizerState*  m_wireCullNone;
        ID3D11RasterizerState*  m_wireCullBack;
        ID3D11RasterizerState*  m_wireCullFront;
        
        // Depth stencil states
        ID3D11DepthStencilState*  m_depthTestAndWrite;
        ID3D11DepthStencilState*  m_depthTestOnly;
        ID3D11DepthStencilState*  m_depthWriteOnly;
        ID3D11DepthStencilState*  m_depthNoTestNoWrite;
        
        // Sampler states.
        ID3D11SamplerState* m_pointWrap;
        ID3D11SamplerState* m_pointClamp;
        ID3D11SamplerState* m_linearWrap;
        ID3D11SamplerState* m_linearClamp;
        ID3D11SamplerState* m_anisotropicWrap;
        ID3D11SamplerState* m_anisotropicClamp;
    };
         
}; // namespace
