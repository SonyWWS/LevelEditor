//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "../Core/WinHeaders.h"
#include "RenderEnums.h"
#include "Renderable.h"
#include "Shader.h"
#include "RenderBuffer.h"

namespace LvEdEngine
{
    class Texture;    
    class RenderContext;


//Initial advanced renderer.
//This class will be refactored and improved overtime.
class BillboardShader : public Shader
{
public:

	BillboardShader(ID3D11Device* device);
	virtual ~BillboardShader();	
    
    // call begin before drawing.
    virtual void Begin(RenderContext* rc);
    virtual void End();

    // temp solution
    // set fill mode
    virtual void SetRenderFlag(RenderFlagsEnum rf);    

    virtual void DrawNodes(const RenderNodeList& renderNodes);
    
private:    
    void Draw(const RenderableNode& r);  

    // -------------------------------------------------------------------
    struct ConstantBufferPerFrame
    {
         Matrix viewXform;
         Matrix projXform;
    };

    // -------------------------------------------------------------------
    struct ConstantBufferPerDraw
    {
        Matrix worldXform;
        Matrix textureXForm;
        float intensity;
        float3 pad;
    };

    ID3D11VertexShader*     m_vertexShader;
    ID3D11PixelShader*      m_pixelShader;
    ID3D11InputLayout*      m_vertexLayout;

    TConstantBuffer<ConstantBufferPerFrame> m_cbPerFrame;
    TConstantBuffer<ConstantBufferPerDraw> m_cbPerDraw;    
    
    RenderFlagsEnum         m_renderFlags;
    RenderContext*          m_rc;
 };

}; // namespace LvEdEngine

