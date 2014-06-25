//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include "Shader.h"

namespace LvEdEngine 
{

    class SkyDomeShader : public Shader
    {
    public:
        SkyDomeShader(ID3D11Device* device);
        virtual ~SkyDomeShader();

        virtual void Begin(RenderContext* rc);
        virtual void End();
        virtual void SetRenderFlag(RenderFlagsEnum rf);
        virtual void DrawNodes(const RenderNodeList& renderNodes);
        void Draw( const RenderableNode& r );  

    private:
        ID3D11VertexShader*     m_vertexShader;
        ID3D11PixelShader*      m_pixelShader;
        ID3D11InputLayout*      m_vertexLayout;
        ID3D11Buffer*           m_constantBufferPerFrame;
        
        ID3D11SamplerState*     m_samplerState;
        RenderContext*          m_rc;

        
        // -------------------------------------------------------------------
        // NOTE: Must match constant buffer in HLSL
        struct cbPerFrame
        {
	        Matrix view;
	        Matrix proj;            
        };
    };

}
