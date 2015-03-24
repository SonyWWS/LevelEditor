//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#pragma once
#include "../Core/WinHeaders.h"
#include "Shader.h"
#include "RenderEnums.h"
#include "Renderable.h"
#include "RenderBuffer.h"

struct ID3D11Device;
struct ID3D11VertexShader;
struct ID3D11PixelShader;
struct ID3D11GeometryShader;       
struct ID3D11Buffer;    
struct ID3D11InputLayout;
struct ID3D11RasterizerState;
struct ID3D11DepthStencilState;
struct ID3D11BlendState;

namespace LvEdEngine 
{
    class WireFrameShader : public Shader
    {
    public:
        WireFrameShader(ID3D11Device* device);
        virtual ~WireFrameShader();

		//overrde: 
		virtual void Update(const FrameTime& fr, UpdateTypeEnum updateType);

        virtual void Begin(RenderContext* rc);
        virtual void End();
        virtual void SetRenderFlag(RenderFlagsEnum rf);
        virtual void DrawNodes(const RenderNodeList& renderNodes);
    private:
		typedef Shader super;
        struct CbPerFrame
        {
            Matrix viewXform;
            Matrix projXform;   
            float4 viewport;
        };

        struct CbPerObject
        {
            Matrix worldXform;   
            float4 color;
        };

        void SetCullMode(CullModeEnum cullMode);
        TConstantBuffer<CbPerFrame>  m_cbPerFrame;
        TConstantBuffer<CbPerObject> m_cbPerObject;
                
        ID3D11VertexShader*    m_vsShader;
        ID3D11PixelShader*     m_psShader;
        ID3D11GeometryShader*  m_gsShader;
        ID3D11InputLayout*     m_layoutP;

        ID3D11RasterizerState*   m_rsFillCullNone;
        ID3D11RasterizerState*   m_rsFillCullBack;
        ID3D11RasterizerState*   m_rsFillCullFront;

        ID3D11DepthStencilState* m_dpLessEqual; 
        ID3D11BlendState*        m_bsBlending;

        RenderContext*         m_rcntx; // render context

		// pulsate wire-frame color for the selected object.
		float m_diffuseModulator;
		float m_theta;
    };
}