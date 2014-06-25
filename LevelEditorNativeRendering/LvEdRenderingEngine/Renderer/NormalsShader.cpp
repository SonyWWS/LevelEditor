//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "../Core/NonCopyable.h"
#include "NormalsShader.h"
#include <D3D11.h>
#include "Renderable.h"
#include "RenderBuffer.h"
#include "RenderUtil.h"
#include "../Core/Utils.h"
#include "RenderContext.h"
#include "RenderState.h"
#include "Model.h"
#include <DxErr.h>

using namespace LvEdEngine;

void NormalsShader::Begin(RenderContext* context)
{
    if(m_rcntx) return;		
	m_rcntx = context;
    
    ID3D11DeviceContext* d3dContext = context->Context();

	CbPerFrame cb;   
    cb.ViewPort = m_rcntx->ViewPort();

    Matrix vp = m_rcntx->Cam().View() * m_rcntx->Cam().Proj();
    Matrix::Transpose(vp,cb.ViewProj);
    cb.color = float4(1,0,0,1);
	
    UpdateConstantBuffer(d3dContext,m_cbPerFrame,&cb,sizeof(cb));		
    d3dContext->VSSetConstantBuffers(1,1,&m_cbPerObject);
    d3dContext->GSSetConstantBuffers(0,1,&m_cbPerFrame);    
    d3dContext->VSSetShader(m_vsShader,NULL,0);
    d3dContext->GSSetShader(m_gsShader,NULL,0);
    d3dContext->PSSetShader(m_psShader,NULL,0);
    d3dContext->IASetInputLayout( m_layoutP);
    d3dContext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_POINTLIST);  // D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST
                                       

    RenderStateCache* rscache = m_rcntx->GetRenderStateCache();
    // set state-blocks ( raster, depth, and blend states)     
    d3dContext->RSSetState(rscache->GetRasterState(FillMode::Wireframe,CullMode::BACK));    
    d3dContext->OMSetDepthStencilState(NULL,0);    
    d3dContext->OMSetBlendState( NULL, NULL, 0xFFFFFFFF );
}

void NormalsShader::End()
{
    m_rcntx->Context()->GSSetShader(NULL,NULL,0);
    m_rcntx = NULL;
}


void NormalsShader::DrawNodes(const RenderNodeList& renderNodes)
{
    CbPerObject cb;
    ID3D11DeviceContext* d3dContext = m_rcntx->Context();
    for ( auto it = renderNodes.begin(); it != renderNodes.end(); ++it )
    {
        const RenderableNode& r = (*it);
        
        if(r.mesh->nor.size() == 0) continue;
        Matrix::Transpose(r.WorldXform,cb.worldXform);    

        Matrix w = r.WorldXform;        
        w.M41 = w.M42 = w.M43 = 0; w.M44 = 1;
        Matrix::Invert(w,cb.worldInvTrans);
        
        UpdateConstantBuffer(d3dContext,m_cbPerObject,&cb,sizeof(cb));

        uint32_t stride = r.mesh->vertexBuffer->GetStride();
        uint32_t offset = 0;       
        uint32_t startVertex = 0;
        ID3D11Buffer* d3dvb  = r.mesh->vertexBuffer->GetBuffer();
       
        d3dContext->IASetVertexBuffers( 0, 1, &d3dvb, &stride, &offset );              
        d3dContext->Draw( r.mesh->vertexBuffer->GetCount(),startVertex);

    }
}

NormalsShader::NormalsShader(ID3D11Device* device)
    : Shader( Shaders::NormalsShader)
{    
    m_rcntx = NULL;
    m_gsShader = NULL;
    // create cbuffers.
    m_cbPerFrame  = CreateConstantBuffer(device, sizeof(CbPerFrame));
    m_cbPerObject = CreateConstantBuffer(device, sizeof(CbPerObject));
        
    ID3DBlob* vsBlob = CompileShaderFromResource(L"NormalsShader.hlsl", "VS","vs_4_0", NULL);    
    ID3DBlob* gsBlob = CompileShaderFromResource(L"NormalsShader.hlsl", "GS","gs_4_0", NULL);    
    ID3DBlob* psBlob = CompileShaderFromResource(L"NormalsShader.hlsl", "PS","ps_4_0", NULL);

    assert(vsBlob);
    assert(gsBlob);
    assert(psBlob);
    

    m_vsShader = CreateVertexShader(device, vsBlob);
    m_gsShader = CreateGeometryShader(device, gsBlob);
    m_psShader = CreatePixelShader(device, psBlob);
    
    assert(m_vsShader);
    assert(m_gsShader);
    assert(m_psShader);
      
    // create input layout    
    m_layoutP = CreateInputLayout(device,vsBlob,VertexFormat::VF_PN);
    assert(m_layoutP);

    // release the blobs
    vsBlob->Release();
    gsBlob->Release();
    psBlob->Release();
        
}

// --------------------------------------------------------------------------------------------------
NormalsShader::~NormalsShader()
{
    SAFE_RELEASE(m_cbPerFrame);
    SAFE_RELEASE(m_cbPerObject);    
    SAFE_RELEASE(m_layoutP);
    SAFE_RELEASE(m_vsShader);
    SAFE_RELEASE(m_psShader);
    SAFE_RELEASE(m_gsShader);    
}

void NormalsShader::SetRenderFlag(RenderFlagsEnum /*rf*/)
{   
}
