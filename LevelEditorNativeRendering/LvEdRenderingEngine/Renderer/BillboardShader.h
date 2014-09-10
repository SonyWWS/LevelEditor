//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "../Core/WinHeaders.h"
#include "RenderEnums.h"
#include "Renderable.h"
#include "Shader.h"

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
    void Initialize(ID3D11Device* device); 
    void Draw(const RenderableNode& r);  

    ID3D11VertexShader*     m_vertexShader;
    ID3D11PixelShader*      m_pixelShader;
    ID3D11InputLayout*      m_vertexLayout;
    ID3D11Buffer*           m_constantBufferPerFrame;
    ID3D11Buffer*           m_constantBufferPerDraw;
    
    ID3D11RasterizerState*  m_rasterStateSolid;
    ID3D11SamplerState*     m_samplerState;

    RenderFlagsEnum         m_renderFlags;
    RenderContext*          m_rc;

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


 };

}; // namespace LvEdEngine

