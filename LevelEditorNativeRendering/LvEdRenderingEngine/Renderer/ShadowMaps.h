//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    ShadowMaps.h

****************************************************************************/
#pragma once
#include <string>
#include "Lights.h"
#include "Shader.h"
#include "RenderBuffer.h"

namespace LvEdEngine
{
    class DirLight;

    //-----------------------------------------------------------------------
    //  ShadowMaps
    //
    //  Global singleton for sharing shadow map data.
    //-----------------------------------------------------------------------
    class ShadowMaps : public NonCopyable
    {
    public:

        static void InitInstance(ID3D11Device* device, uint32_t dim);
        static void DestroyInstance();
        static      ShadowMaps*  Inst() { return s_inst; }

        
        ID3D11SamplerState*  GetSamplerState(){ return m_samplerState; }        
        ID3D11ShaderResourceView*   GetShaderResourceView() { return m_resourceView; }
        const Camera& GetCamera() {return m_lightCamera;}
        const D3D11_VIEWPORT& GetViewPort() {return m_viewport;}
        float MapSize() {return m_viewport.Width;}
        // sets and clears depth stencil buffer and set render target to null.
        void SetAndClear(ID3D11DeviceContext* dc);
        void UpdateLightCamera(ID3D11DeviceContext* dc, const DirLight* light, const AABB& renderedArea );
        ID3D11Buffer* GetShadowConstantBuffer() { return m_cbShadow.GetBuffer(); }        
        bool IsEnabled() {return m_enabled;}
        void SetEnabled(bool enabled) { m_enabled = enabled;}
        
    private:

        __declspec(align(16))
        struct CbShadow
        {
            Matrix xform;    
            float  texelSize; // Shadow map texel size.
            float pad[3];
        };

        static ShadowMaps*   s_inst;
        ShadowMaps(ID3D11Device* device, uint32_t dim);
        ~ShadowMaps();

        ID3D11DepthStencilView*     m_depthStencilView;        
        ID3D11SamplerState*         m_samplerState;
        ID3D11ShaderResourceView*   m_resourceView;
        TConstantBuffer<CbShadow>   m_cbShadow;
        D3D11_VIEWPORT m_viewport;
        bool m_enabled;
        Camera                      m_lightCamera;        
    };

}   // namespace LvEdEngine
