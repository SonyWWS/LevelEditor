
//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

//#include <D3Dcompiler.h>
#include <d3d11.h>
#include "BasicRenderer.h"
#include "RenderBuffer.h"
#include "GpuResourceFactory.h"
#include "RenderState.h"
#include "Texture.h"
#include "RenderUtil.h"
#include "../Core/Logger.h"
#include "RenderSurface.h"

using namespace LvEdEngine;

void BasicRenderer::Begin(ID3D11DeviceContext* d3dContext,RenderSurface* surface, const Matrix& view, const Matrix& proj)
{
	if(m_context) return;
    m_context = d3dContext;
    m_surface = surface;    
    m_primaryDepthBufferActive = true;
    m_clearForegroundDepthBuffer = true;
    Camera cam;
    cam.SetViewProj(view,proj);

    Matrix v,p;    
    Matrix::Transpose(view,v);
    Matrix::Transpose(proj,p); 
        
	m_cbPerFrame.Data.viewXform = v;
    m_cbPerFrame.Data.projXform = p; 
    m_cbPerFrame.Data.camPosW = cam.CamPos();
    m_cbPerFrame.Data.pad = 0.0f;
    //m_light.dir = normalize(cam.CamLook() - cam.CamUp());    
    m_light.dir = cam.CamLook();
    m_cbPerFrame.Data.dirlight = m_light;
    m_cbPerFrame.Update(d3dContext);
   
    // set per call buffer.

    ID3D11Buffer* perframeBuffer = m_cbPerFrame.GetBuffer();
    ID3D11Buffer* perDrawBuffer  = m_cbPerDraw.GetBuffer();

    ID3D11Buffer* cbuffers[] = {perframeBuffer,perDrawBuffer};
    m_context->VSSetConstantBuffers(0,ARRAY_SIZE(cbuffers),cbuffers);
    m_context->PSSetConstantBuffers(0,ARRAY_SIZE(cbuffers),cbuffers);
   
    m_context->VSSetShader(m_pVertexShaderP,NULL,0);
    m_context->PSSetShader(m_pPixelShaderP,NULL,0);
    m_context->GSSetShader(NULL,NULL,0);

    m_context->IASetInputLayout( m_pVertexLayout );
	
}
void BasicRenderer::End()
{
	if(!m_context) return;	
    if(!m_primaryDepthBufferActive)
    {
        m_primaryDepthBufferActive = true;
        SetDepthBuffer(m_surface->GetDepthStencilView());       
    }

    m_context->VSSetConstantBuffers(0,0,NULL);    
    m_context->VSSetConstantBuffers(1,0,NULL);    
    m_context->PSSetConstantBuffers(1,0,NULL);	
    m_context->VSSetShader(NULL,NULL,0);
    m_context->PSSetShader(NULL,NULL,0);
    m_context->IASetInputLayout( NULL );

    m_context->RSSetState(NULL);
    m_context->OMSetDepthStencilState(NULL,0);
    m_context = NULL; 
    m_surface = NULL;
}


void BasicRenderer::SetRendererFlag(BasicRendererFlagsEnum renderFlags)
{
    m_renderFlags = renderFlags;
    RSCache* states = RSCache::Inst();
    
    if( (renderFlags & BasicRendererFlags::DisableDepthTest) 
        && (renderFlags & BasicRendererFlags::DisableDepthWrite))
    {
        
        m_context->OMSetDepthStencilState(states->DepthNoTestNoWrite(),0);
    }
    else if(renderFlags & BasicRendererFlags::DisableDepthTest) 
    {
        m_context->OMSetDepthStencilState(states->DepthWriteOnly(),0);
    }
    else if(renderFlags & BasicRendererFlags::DisableDepthWrite)
    {
        m_context->OMSetDepthStencilState(states->DepthTestOnly(),0);
    }
    else
    {
        m_context->OMSetDepthStencilState(states->DepthTestAndWrite(),0);
    }

    if(renderFlags & BasicRendererFlags::WireFrame)
        m_context->RSSetState(states->WireCullBack());        
    else
        m_context->RSSetState(states->SolidCullBack());        

    if(m_surface->GetDepthStencilViewFg())
    {
        if(renderFlags & BasicRendererFlags::Foreground)
        {
            if(m_primaryDepthBufferActive)
            {
                m_primaryDepthBufferActive = false;
                // set foreground depth buffer.                
                SetDepthBuffer(m_surface->GetDepthStencilViewFg());
                if(m_clearForegroundDepthBuffer)
                {
                    m_clearForegroundDepthBuffer = false;
                    m_context->ClearDepthStencilView( m_surface->GetDepthStencilViewFg(), D3D11_CLEAR_DEPTH, 1.0f, 0 );
                }
            }
        }
        else if(!m_primaryDepthBufferActive)
        {
            m_primaryDepthBufferActive = true;
            // set primary depth buffer.
            SetDepthBuffer(m_surface->GetDepthStencilView());            
        }
    }
}

void BasicRenderer::SetDepthBuffer(ID3D11DepthStencilView* dv)
{
    ID3D11RenderTargetView* rt  = m_surface->GetRenderTargetView();    
    m_context->OMSetRenderTargets(1, &rt, dv);        
}


void BasicRenderer::UpdateCbPerDraw(const Matrix& xform, const float4& color)
{    
    Matrix m;
    Matrix::Transpose(xform,m);        
	m_cbPerDraw.Data.worldXform = m;    
	m_cbPerDraw.Data.color = color;	
    m_cbPerDraw.Data.lit =  (m_renderFlags & BasicRendererFlags::Lit) != 0;
    if(m_cbPerDraw.Data.lit)
    {
        m_cbPerDraw.Data.specular = float4(0.0f,0.0f,0.0f,1.0f);
        Matrix w = xform;
        w.M41 = w.M42 = w.M43 = 0; w.M44 = 1;
        Matrix::Invert(w,m_cbPerDraw.Data.worldInvTrans);
    }
    m_cbPerDraw.Update(m_context);   

}

void BasicRenderer::DrawPrimitive(PrimitiveTypeEnum pt, 
                       ObjectGUID vbufferId,
                       uint32_t StartVertex,
                       uint32_t vertexCount,
                       float* color, 
                       float* xform)
{
    if(!m_context) return;  

    UpdateCbPerDraw(xform,color);
		
	// Set primitive topology
    m_context->IASetPrimitiveTopology( (D3D11_PRIMITIVE_TOPOLOGY)pt );	

    VertexBuffer* vb = reinterpret_cast<VertexBuffer*>(vbufferId);    
    UINT stride = vb->GetStride();
    UINT offset = 0;
    ID3D11Buffer* buffer = vb->GetBuffer();
    m_context->IASetVertexBuffers( 0, 1, &buffer, &stride, &offset );           
    m_context->Draw(vertexCount , StartVertex);	
}
 
void BasicRenderer::DrawIndexedPrimitive(PrimitiveTypeEnum pt,                                                             
                        ObjectGUID vbId, 
                        ObjectGUID ibId,
                        uint32_t startIndex,
                        uint32_t indexCount,
                        uint32_t startVertex,                        
                        float* color,
                        float* xform)                        
 {
    if(!m_context) return; 
    UpdateCbPerDraw(xform,color);
	
	// Set primitive topology
    m_context->IASetPrimitiveTopology( (D3D11_PRIMITIVE_TOPOLOGY)pt );	

    // set vertex buffer
    VertexBuffer* vb = reinterpret_cast<VertexBuffer*>(vbId);
    UINT stride = vb->GetStride();    
    UINT Offset = 0;
    ID3D11Buffer* buffer = vb->GetBuffer();
    m_context->IASetVertexBuffers( 0, 1, &buffer, &stride, &Offset);

    // set index buffer
    IndexBuffer* ib = reinterpret_cast<IndexBuffer*>(ibId);
    m_context->IASetIndexBuffer(ib->GetBuffer(),(DXGI_FORMAT)ib->GetFormat(),0);

    m_context->DrawIndexed(indexCount,startIndex,startVertex);	
   
 }


ObjectGUID BasicRenderer::CreateVertexBuffer(VertexFormatEnum vf, void* buffer, uint32_t vertexCount)
{    
    VertexBuffer* vb =  GpuResourceFactory::CreateVertexBuffer(buffer, vf, vertexCount, BufferUsage::DEFAULT);
    return (ObjectGUID)vb;
}

ObjectGUID BasicRenderer::CreateIndexBuffer(uint32_t* buffer, uint32_t indexCount)
{    
    IndexBuffer* ib = GpuResourceFactory::CreateIndexBuffer(buffer,indexCount,IndexBufferFormat::U32,BufferUsage::DEFAULT);
    return (ObjectGUID)ib;
}

void BasicRenderer::DeleteBuffer(ObjectGUID bufferId)
{    
    GpuBuffer* buffer = reinterpret_cast<GpuBuffer*>(bufferId);
    delete buffer;
}

BasicRenderer::BasicRenderer(ID3D11Device* device) 	
{	    
    assert(device);
	
    m_context = NULL;
    m_primaryDepthBufferActive = true;
    m_light.ambient  = float3(0.3f,0.3f,0.3f);
    m_light.diffuse  = float3(0.8f,0.8f,0.8f);
    m_light.specular = float3(0.9f,0.9f,0.9f);
    m_renderFlags = BasicRendererFlags::None;
	
    m_cbPerFrame.Construct(device);
    m_cbPerDraw.Construct(device);

	// Compile the vertex shader
    ID3DBlob* pVSBlob = CompileShaderFromResource(L"BasicRenderer.hlsl", "VS", "vs_4_0", NULL);
    
    // Create the vertex shader
    m_pVertexShaderP = GpuResourceFactory::CreateVertexShader(pVSBlob);
    assert(m_pVertexShaderP);
          
    // create input layout
    m_pVertexLayout  = GpuResourceFactory::CreateInputLayout(pVSBlob,VertexFormat::VF_PN);    
    assert(m_pVertexLayout);    
    pVSBlob->Release();
    
    
    // Compile the pixel shader
    ID3DBlob* pPSBlob = CompileShaderFromResource(L"BasicRenderer.hlsl", "PS", "ps_4_0", NULL);
    // Create the pixel shader
    m_pPixelShaderP = GpuResourceFactory::CreatePixelShader(pPSBlob);    
    pPSBlob->Release();      

}

BasicRenderer::~BasicRenderer()
{    
    SAFE_RELEASE(m_pVertexShaderP);
    SAFE_RELEASE(m_pPixelShaderP);
    SAFE_RELEASE(m_pVertexLayout);    
}
