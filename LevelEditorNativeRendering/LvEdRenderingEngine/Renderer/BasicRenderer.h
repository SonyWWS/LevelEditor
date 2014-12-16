//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "../Core/WinHeaders.h"
#include "../Core/typedefs.h"
#include "../Core/NonCopyable.h"
#include "../VectorMath/V3dMath.h"
#include "RenderEnums.h"
#include "Lights.h"
#include "RenderBuffer.h"

namespace LvEdEngine
{
    class RenderSurface;

// Basic renderer,
// used for simple rendering.
// this class is internal to this DLL.
// it is implemented using DirectX 11
// A game engine can resuse this renderer as long as it has 
// d3d rendering context.
class BasicRenderer : public NonCopyable
{
public:

	BasicRenderer(ID3D11Device* device);
	~BasicRenderer();	
    
	// call begin before drawing.
	// begin sets view and project matrices and also set the 
    // a simple vertex and pixel shaders.
    // the passed ID3D11DeviceContext is used until End() call 
    void Begin(ID3D11DeviceContext* d3dContext,RenderSurface* surface, const Matrix& view, const Matrix& proj);
    void SetRendererFlag(BasicRendererFlagsEnum renderFlags);
    void End();
    
    ObjectGUID CreateVertexBuffer(VertexFormatEnum vf, void* buffer, uint32_t vertexCount);
    ObjectGUID CreateIndexBuffer(uint32_t* buffer, uint32_t indexCount);
    void DrawPrimitive(PrimitiveTypeEnum pt, 
                       ObjectGUID vb,
                       uint32_t StartVertex,
                       uint32_t vertexCount,
                       float* color, float* xform);

    void DrawIndexedPrimitive(PrimitiveTypeEnum pt,                                                             
                                ObjectGUID vb, 
                                ObjectGUID ib,
                                uint32_t startIndex,
                                uint32_t indexCount,
                                uint32_t startVertex,                        
                                float* color,
                                float* xform);
                                
   
    // delete vertex or index buffer.
    void DeleteBuffer(ObjectGUID buffer);	
private:	    
    
    void UpdateCbPerDraw(const Matrix& xform, const float4& color); // update constant buffer per draw
    bool m_clearForegroundDepthBuffer;
    bool m_primaryDepthBufferActive;
    void SetDepthBuffer(ID3D11DepthStencilView* dv);
    DirLight m_light;
    BasicRendererFlagsEnum m_renderFlags;
		
	ID3D11VertexShader*     m_pVertexShaderP;
	ID3D11PixelShader*      m_pPixelShaderP;
	ID3D11InputLayout*      m_pVertexLayout;
    
	ID3D11DeviceContext* m_context;
    RenderSurface* m_surface;

    struct ConstantBufferPerFrame
    {
        Matrix viewXform;
        Matrix projXform;  
        float3   camPosW;
        float pad;
        DirLight dirlight;
    };    
    struct ConstantBufferPerDraw
    {
        Matrix worldXform;           
        float4x4 worldInvTrans;                                                      
        float4 color;
        float4   specular; // xyz = specular color and  w = specular power
        int lit;
        int pad[3];
    };

    TConstantBuffer<ConstantBufferPerFrame>  m_cbPerFrame;
	TConstantBuffer<ConstantBufferPerDraw>   m_cbPerDraw;
 };

}
