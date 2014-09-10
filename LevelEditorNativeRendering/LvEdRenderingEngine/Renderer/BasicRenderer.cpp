//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include <D3Dcompiler.h>
#include <d3d11.h>
#include "BasicRenderer.h"
#include "RenderBuffer.h"
#include "Texture.h"
#include "RenderUtil.h"
#include "../Core/Logger.h"


bool UseRightHand = true; // for testing.


void BasicRenderer::Begin(ID3D11DeviceContext* d3dContext, const Matrix& view, const Matrix& proj)
{
	if(m_context) return;
		
	this->m_context = d3dContext;
    
	ConstantBufferPerFrame cb;
    Matrix v,p;    
    Matrix::Transpose(view,v);
    Matrix::Transpose(proj,p);    
	cb.viewXform = v;
    cb.projXform = p;    
	d3dContext->UpdateSubresource(m_pConstantBufferPerFrame,0,NULL,	&cb,0,0);
	
    // set per call buffer.
	d3dContext->VSSetConstantBuffers(0,1,&m_pConstantBufferPerFrame);    
    d3dContext->VSSetConstantBuffers(1,1,&m_pConstantBufferPerDraw);    
    m_context->PSSetConstantBuffers(1,1,&m_pConstantBufferPerDraw);	

    m_context->VSSetShader(m_pVertexShaderP,NULL,0);
    m_context->PSSetShader(m_pPixelShaderP,NULL,0);
    m_context->IASetInputLayout( m_pVertexLayout );
	
}
void BasicRenderer::End()
{
	if(!m_context) return;	
    m_context->RSSetState(NULL);
    m_context->OMSetDepthStencilState(NULL,0);
    m_context = NULL;    
}

void BasicRenderer::DrawPrimitive(PrimitiveTypeEnum pt, 
                       ObjectGUID vbufferId,
                       uint32_t StartVertex,
                       uint32_t vertexCount,
                       float* color, 
                       float* xform,
                       BasicRendererFlagsEnum renderFlags)
{
    if(!m_context) return;  

	ConstantBufferPerDraw cb;
    Matrix m;
    Matrix::Transpose(xform,m);        
	cb.worldXform = m;    
	cb.color = color;
	m_context->UpdateSubresource(m_pConstantBufferPerDraw,0,NULL,&cb,0,0);	
	
	
	// Set primitive topology
    m_context->IASetPrimitiveTopology( (D3D11_PRIMITIVE_TOPOLOGY)pt );	

    VertexBuffer* vb = reinterpret_cast<VertexBuffer*>(vbufferId);    
    UINT stride = vb->GetStride();
    UINT offset = 0;
    ID3D11Buffer* buffer = vb->GetBuffer();
    m_context->IASetVertexBuffers( 0, 1, &buffer, &stride, &offset );
        
    // todo use  RenderStateCache to set depth and raster state blocks
    if( (renderFlags & BasicRendererFlags::DisableDepthTest) 
        && (renderFlags & BasicRendererFlags::DisableDepthWrite))
    {
        m_context->OMSetDepthStencilState(m_dsNoTestNoWrite,0);
    }
    else if(renderFlags & BasicRendererFlags::DisableDepthTest) 
    {
        m_context->OMSetDepthStencilState(m_dsNoTestWrite,0);
    }
    else if(renderFlags & BasicRendererFlags::DisableDepthWrite)
    {
        m_context->OMSetDepthStencilState(m_dsTestNoWrite,0);
    }
    else
    {
        m_context->OMSetDepthStencilState(m_dsTestWrite,0);
    }

    if(renderFlags & BasicRendererFlags::Solid)
    {        
        m_context->RSSetState(m_pSolidRS);        
    }
    else if(renderFlags & BasicRendererFlags::WireFrame)
    {                
        m_context->RSSetState(m_pWireFrameRS);        
    }	
    m_context->Draw(vertexCount , StartVertex);	
}

 
void BasicRenderer::DrawIndexedPrimitive(PrimitiveTypeEnum pt,                                                             
                        ObjectGUID vbId, 
                        ObjectGUID ibId,
                        uint32_t startIndex,
                        uint32_t indexCount,
                        uint32_t startVertex,                        
                        float* color,
                        float* xform,
                        BasicRendererFlagsEnum renderFlags)
 {
    if(!m_context) return;    
	ConstantBufferPerDraw cb;
    Matrix m(xform);
    m.Transpose();        
	cb.worldXform = m;    
    cb.color =  color;	
	m_context->UpdateSubresource(m_pConstantBufferPerDraw,0,NULL,&cb,0,0);
	
	// Set primitive topology
    m_context->IASetPrimitiveTopology( (D3D11_PRIMITIVE_TOPOLOGY)pt );	

    VertexBuffer* vb = reinterpret_cast<VertexBuffer*>(vbId);
    IndexBuffer* ib = reinterpret_cast<IndexBuffer*>(ibId);

    UINT stride = vb->GetStride();
    UINT offset = 0;
    ID3D11Buffer* buffer = vb->GetBuffer();
    m_context->IASetVertexBuffers( 0, 1, &buffer, &stride, &offset );
    m_context->IASetIndexBuffer(ib->GetBuffer(),DXGI_FORMAT_R32_UINT,0);

     // todo use  RenderStateCache to set depth and raster state blocks
    if( (renderFlags & BasicRendererFlags::DisableDepthTest) 
        && (renderFlags & BasicRendererFlags::DisableDepthWrite))
    {
        m_context->OMSetDepthStencilState(m_dsNoTestNoWrite,0);
    }
    else if(renderFlags & BasicRendererFlags::DisableDepthTest) 
    {
        m_context->OMSetDepthStencilState(m_dsNoTestWrite,0);
    }
    else if(renderFlags & BasicRendererFlags::DisableDepthWrite)
    {
        m_context->OMSetDepthStencilState(m_dsTestNoWrite,0);
    }
    else
    {
        m_context->OMSetDepthStencilState(m_dsTestWrite,0);
    }

    if(renderFlags & BasicRendererFlags::Solid)
    {        
        m_context->RSSetState(m_pSolidRS);        
    }
    else if(renderFlags & BasicRendererFlags::WireFrame)
    {                
        m_context->RSSetState(m_pWireFrameRS);        
    }	

    m_context->DrawIndexed(indexCount,startIndex,startVertex);	
   
 }


ObjectGUID BasicRenderer::CreateVertexBuffer(VertexFormatEnum vf, void* buffer, uint32_t vertexCount)
{
    VertexBuffer* vb =  LvEdEngine::CreateVertexBuffer(m_pd3dDevice, vf, buffer, vertexCount);
    return (ObjectGUID)vb;
}

ObjectGUID BasicRenderer::CreateIndexBuffer(uint32_t* buffer, uint32_t indexCount)
{
    IndexBuffer* ib = LvEdEngine::CreateIndexBuffer(m_pd3dDevice, buffer, indexCount);
    return (ObjectGUID)ib;
}

void BasicRenderer::DeleteBuffer(ObjectGUID bufferId)
{    
    GpuBuffer* buffer = reinterpret_cast<GpuBuffer*>(bufferId);
    delete buffer;
}

BasicRenderer::BasicRenderer(ID3D11Device* pd3dDevice) 	
{	    
    m_pd3dDevice =pd3dDevice;
	m_pVertexShaderP = NULL;
	m_pPixelShaderP = NULL;
	m_pVertexLayout = NULL;    
	m_pConstantBufferPerFrame = NULL;
	m_pConstantBufferPerDraw = NULL;	
	m_pWireFrameRS = NULL;  
    m_context = NULL;
	CreateBuffers();
}

BasicRenderer::~BasicRenderer()
{    
	m_pVertexShaderP->Release();
    m_pPixelShaderP->Release();
    m_pVertexLayout->Release();    

    // position and normal    
	m_pConstantBufferPerFrame->Release();
	m_pConstantBufferPerDraw->Release();
	m_pWireFrameRS->Release();
    m_pSolidRS->Release();

    m_dsTestWrite->Release();
    m_dsTestNoWrite->Release();
    m_dsNoTestWrite->Release();
    m_dsNoTestNoWrite->Release();
}

void BasicRenderer::CreateBuffers()
{	
	HRESULT hr = S_OK;
    	
    D3D11_BUFFER_DESC bufDcr;
    
	// Create the constant buffer
	SecureZeroMemory( &bufDcr, sizeof(bufDcr));
	bufDcr.Usage =  D3D11_USAGE_DEFAULT; //D3D11_USAGE_DYNAMIC;	
	bufDcr.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
	bufDcr.CPUAccessFlags = 0;  //D3D11_CPU_ACCESS_WRITE;
    
    bufDcr.ByteWidth = sizeof(ConstantBufferPerFrame);
    hr = m_pd3dDevice->CreateBuffer( &bufDcr, NULL, &m_pConstantBufferPerFrame );
    if (Logger::IsFailureLog(hr))
	{
		return;			 
	}
    	
    bufDcr.ByteWidth = sizeof(ConstantBufferPerDraw);
    hr = m_pd3dDevice->CreateBuffer( &bufDcr, NULL, &m_pConstantBufferPerDraw );
    if (Logger::IsFailureLog(hr))
	{
		return;			 
	}
    
	// Compile the vertex shader
    ID3DBlob* pVSBlob = CompileShaderFromResource(L"BasicShader.hlsl", "VS", "vs_4_0", NULL);
    
    // Create the vertex shader
    hr = m_pd3dDevice->CreateVertexShader( pVSBlob->GetBufferPointer(), pVSBlob->GetBufferSize(), NULL, &m_pVertexShaderP );
    if( FAILED( hr ) )
    {    
        pVSBlob->Release();
        return;
    }

    // Define the input layout
    D3D11_INPUT_ELEMENT_DESC layoutP[] =
    {
        { "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },        
    };
    UINT numElements = ARRAYSIZE( layoutP );

    // Create the input layout
    hr = m_pd3dDevice->CreateInputLayout( layoutP, numElements, pVSBlob->GetBufferPointer(),
                                          pVSBlob->GetBufferSize(), &m_pVertexLayout );
    pVSBlob->Release();
    pVSBlob = NULL;

    if( FAILED( hr ) )
        return;

    
    // Compile the pixel shader
    ID3DBlob* pPSBlob = CompileShaderFromResource(L"BasicShader.hlsl", "PS", "ps_4_0", NULL);
    // Create the pixel shader
    hr = m_pd3dDevice->CreatePixelShader( pPSBlob->GetBufferPointer(), pPSBlob->GetBufferSize(), NULL, &m_pPixelShaderP );
    pPSBlob->Release();
    pPSBlob = NULL;
  
    // create raster states.
	D3D11_RASTERIZER_DESC rsDcr;
	SecureZeroMemory( &rsDcr, sizeof(rsDcr));		
    rsDcr.DepthClipEnable = true;
	rsDcr.CullMode =  D3D11_CULL_BACK;
	rsDcr.FillMode =  D3D11_FILL_WIREFRAME;
	rsDcr.FrontCounterClockwise = UseRightHand; // front face is CCW for right hand	
	rsDcr.AntialiasedLineEnable = false;
	rsDcr.MultisampleEnable = true;
		
	m_pd3dDevice->CreateRasterizerState(&rsDcr,  &m_pWireFrameRS);
    
    rsDcr.FillMode =  D3D11_FILL_SOLID;
    m_pd3dDevice->CreateRasterizerState(&rsDcr,  &m_pSolidRS);
      
    D3D11_DEPTH_STENCIL_DESC dsDcr;
    SecureZeroMemory( &dsDcr, sizeof(dsDcr));		    
    dsDcr.DepthEnable = TRUE;
    dsDcr.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
    dsDcr.DepthFunc = D3D11_COMPARISON_LESS;
    dsDcr.StencilEnable = FALSE;
    dsDcr.StencilReadMask = D3D11_DEFAULT_STENCIL_READ_MASK;
    dsDcr.StencilWriteMask = D3D11_DEFAULT_STENCIL_WRITE_MASK;

    dsDcr.FrontFace.StencilFunc = D3D11_COMPARISON_ALWAYS;
    dsDcr.BackFace.StencilFunc = D3D11_COMPARISON_ALWAYS;
    
    dsDcr.FrontFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP; 
    dsDcr.BackFace.StencilDepthFailOp = D3D11_STENCIL_OP_KEEP;

    dsDcr.FrontFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;
    dsDcr.BackFace.StencilPassOp = D3D11_STENCIL_OP_KEEP;

    dsDcr.FrontFace.StencilFailOp = D3D11_STENCIL_OP_KEEP; 
    dsDcr.BackFace.StencilFailOp = D3D11_STENCIL_OP_KEEP;

    m_pd3dDevice->CreateDepthStencilState(&dsDcr,&m_dsTestWrite);

    dsDcr.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ZERO;
    m_pd3dDevice->CreateDepthStencilState(&dsDcr,&m_dsTestNoWrite);
        
    dsDcr.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
    dsDcr.DepthFunc = D3D11_COMPARISON_ALWAYS;
    m_pd3dDevice->CreateDepthStencilState(&dsDcr,&m_dsNoTestWrite);

    dsDcr.DepthEnable = FALSE;
    dsDcr.DepthFunc = D3D11_COMPARISON_NEVER;
    dsDcr.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ZERO;
    m_pd3dDevice->CreateDepthStencilState(&dsDcr,&m_dsNoTestNoWrite);
}


