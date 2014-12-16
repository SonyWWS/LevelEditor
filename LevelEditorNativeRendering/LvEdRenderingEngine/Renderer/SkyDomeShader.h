//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include "Shader.h"
#include "RenderBuffer.h"

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
        struct CbPerFrame
        {
	        Matrix view;
	        Matrix proj;            
        };

        ID3D11VertexShader*         m_vertexShader;
        ID3D11PixelShader*          m_pixelShader;
        ID3D11InputLayout*          m_vertexLayout;
        TConstantBuffer<CbPerFrame> m_cbPerFrame;        
        RenderContext*              m_rc;

    };

}
