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

	class RenderStateCache : public Object
    {
    public:
        RenderStateCache(ID3D11Device* device);
        ~RenderStateCache();
        
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "RenderStateCache";}

        D3D11_RASTERIZER_DESC     GetDefaultRsDcr(){return m_defRsDcr;}
        D3D11_DEPTH_STENCIL_DESC  GetDefaultDpDcr(){return m_defDpDcr;}
        D3D11_BLEND_DESC          GetDefaultBsDcr(){return m_defBsDcr;}
        D3D11_SAMPLER_DESC        GetDefaultSampler() {return m_defSampler;}
        ID3D11RasterizerState* GetRasterState(FillModeEnum fillMode, CullModeEnum cullMode);
        ID3D11DepthStencilState* GetDepthStencilState(RenderFlagsEnum renderflag);
        ID3D11BlendState* GetBlendState(RenderFlagsEnum renderflag);

        ID3D11BlendState* GetAlphaToCoverageState() { return m_alphaToCoverage;}



    private:        
        std::map<uint32_t,ID3D11RasterizerState*> RasterStates;
        std::map<uint32_t,ID3D11DepthStencilState*> DepthStencilStates;        
        std::map<uint32_t,ID3D11BlendState*> BlendStates;
        ID3D11BlendState*  m_alphaToCoverage;
        D3D11_RASTERIZER_DESC     m_defRsDcr;
        D3D11_DEPTH_STENCIL_DESC  m_defDpDcr;
        D3D11_BLEND_DESC          m_defBsDcr;
        D3D11_SAMPLER_DESC        m_defSampler;

    };

    class ShadowMaps;
         
}; // namespace
