//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#pragma once
#include "../Core/WinHeaders.h"
#include "Shader.h"
#include "RenderEnums.h"
#include "Renderable.h"



namespace LvEdEngine 
{
    class BasicShader : public Shader
    {
    public:
        BasicShader(ID3D11Device* device);
        virtual ~BasicShader();

        virtual void Begin(RenderContext* rc);
        virtual void End();
        virtual void SetRenderFlag(RenderFlagsEnum rf);
        virtual void DrawNodes(const RenderNodeList& renderNodes);


    private:
        ID3D11Buffer*          m_cbPerFrame;
        ID3D11Buffer*          m_cbPerObject;
        ID3D11VertexShader*    m_vsShader;
        ID3D11PixelShader*     m_psShader;
        ID3D11InputLayout*     m_vertexLayout;
        RenderContext*         m_rc; // render context

        struct BasicCbPerFrame
        {
            Matrix viewXform;
            Matrix projXform;   
        };

        struct BasicCbPerObject
        {
            Matrix worldXform; 
            float4 color;
        };
    };
}