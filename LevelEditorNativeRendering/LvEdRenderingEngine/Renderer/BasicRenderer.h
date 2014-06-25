//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <D3DX11.h>
#include "../Core/WinHeaders.h"
#include "../Core/typedefs.h"
#include "../Core/NonCopyable.h"
#include "../VectorMath/V3dMath.h"
#include "RenderEnums.h"

using namespace LvEdEngine;

// Basic renderer,
// used for simple rendering.
// this class is internal to this DLL.
// it is implemented using DirectX 11
// A game engine can resuse this renderer as long as it has 
// d3d rendering context.
class BasicRenderer : public NonCopyable
{
public:

	BasicRenderer(ID3D11Device*);
	~BasicRenderer();	
    

	// call begin before drawing.
	// begin sets view and project matrices and also set the 
    // a simple vertex and pixel shaders.
    // the passed ID3D11DeviceContext is used until End() call 
    void Begin(ID3D11DeviceContext* d3dContext, const Matrix& view, const Matrix& proj);
    		
    void End();
    
    ObjectGUID CreateVertexBuffer(VertexFormatEnum vf, void* buffer, uint32_t vertexCount);
    ObjectGUID CreateIndexBuffer(uint32_t* buffer, uint32_t indexCount);
    void DrawPrimitive(PrimitiveTypeEnum pt, 
                       ObjectGUID vb,
                       uint32_t StartVertex,
                       uint32_t vertexCount,
                       float* color, float* xform, BasicRendererFlagsEnum renderFlags);

    void DrawIndexedPrimitive(PrimitiveTypeEnum pt,                                                             
                                ObjectGUID vb, 
                                ObjectGUID ib,
                                uint32_t startIndex,
                                uint32_t indexCount,
                                uint32_t startVertex,                        
                                float* color,
                                float* xform,
                                BasicRendererFlagsEnum renderFlags);
   
    // delete vertex or index buffer.
    void DeleteBuffer(ObjectGUID buffer);


	
private:
	
	void CreateBuffers();// create vb, ib, and constant buffer.
	ID3D11Device* m_pd3dDevice;
	ID3D11VertexShader*     m_pVertexShaderP;
	ID3D11PixelShader*      m_pPixelShaderP;
	ID3D11InputLayout*      m_pVertexLayout;

	ID3D11Buffer*           m_pConstantBufferPerFrame;
	ID3D11Buffer*           m_pConstantBufferPerDraw;
	ID3D11RasterizerState*  m_pWireFrameRS; // raster state for wireframe.
    ID3D11RasterizerState*  m_pSolidRS; // raster state for solid.
    
    ID3D11DepthStencilState*  m_dsTestWrite;
    ID3D11DepthStencilState*  m_dsTestNoWrite;
    ID3D11DepthStencilState*  m_dsNoTestWrite;
    ID3D11DepthStencilState*  m_dsNoTestNoWrite;
            
	ID3D11DeviceContext* m_context;

    struct ConstantBufferPerFrame
    {
        Matrix viewXform;
        Matrix projXform;    
    };

    struct ConstantBufferPerDraw
    {
        Matrix worldXform;   
        float4 color;
    };


 };
