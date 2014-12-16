//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "BasicShader.h"
#include <D3D11.h>
#include "Renderable.h"
#include "RenderBuffer.h"
#include "RenderUtil.h"
#include "../Core/Utils.h"
#include "RenderContext.h"
#include "RenderState.h"
#include "Model.h"
#include "GpuResourceFactory.h"

using namespace LvEdEngine;

// ------------------------------------------------------------------------------------------------
void BasicShader::Begin(RenderContext* context)
{
    m_rc = context;
    ID3D11DeviceContext*  d3dcontext = m_rc->Context();

    // update per frame cbuffer    
    Matrix::Transpose(m_rc->Cam().View(),m_cbPerFrame.Data.viewXform);
    Matrix::Transpose(m_rc->Cam().Proj(),m_cbPerFrame.Data.projXform);   	
    m_cbPerFrame.Update(d3dcontext);
    
    // set cbuffers
    ID3D11Buffer* nativePerFrameBuffer[] = {m_cbPerFrame.GetBuffer()};
    ID3D11Buffer* nativePerObjectBuffer[] = {m_cbPerObject.GetBuffer()};
    d3dcontext->VSSetConstantBuffers(0,1,nativePerFrameBuffer);
    d3dcontext->PSSetConstantBuffers(0,1,nativePerFrameBuffer);	    
    d3dcontext->VSSetConstantBuffers(1,1,nativePerObjectBuffer);
	d3dcontext->PSSetConstantBuffers(1,1,nativePerObjectBuffer);	
    
	// set vs, ps and layout.
	d3dcontext->VSSetShader(m_vsShader,NULL,0);
	d3dcontext->PSSetShader(m_psShader,NULL,0);
    d3dcontext->IASetInputLayout(m_vertexLayout);

    // raster state
    ID3D11RasterizerState* rasterState = RSCache::Inst()->GetRasterState(FillMode::Solid,CullMode::BACK);
    d3dcontext->RSSetState(rasterState);

    // depth stencil state
    ID3D11DepthStencilState* depth = RSCache::Inst()->GetDepthStencilState(RenderFlags::None);
    d3dcontext->OMSetDepthStencilState(depth, 0);

    
}

// ------------------------------------------------------------------------------------------------
void BasicShader::End()
{
    m_rc = NULL;
}

// ------------------------------------------------------------------------------------------------
void BasicShader::DrawNodes(const RenderNodeList& renderNodes)
{
    ID3D11DeviceContext* d3dContext = m_rc->Context();
    
    for ( auto it = renderNodes.begin(); it != renderNodes.end(); ++it )
    {
        const RenderableNode& r = (*it);
        
        Matrix::Transpose(r.WorldXform,m_cbPerObject.Data.worldXform);
        m_cbPerObject.Data.color = r.diffuse;        
        m_cbPerObject.Update(d3dContext);
        
        uint32_t stride = r.mesh->vertexBuffer->GetStride();
        uint32_t offset = 0;    
        uint32_t startVertex = 0;
        ID3D11Buffer* d3dvb  = r.mesh->vertexBuffer->GetBuffer();
        d3dContext->IASetPrimitiveTopology( (D3D11_PRIMITIVE_TOPOLOGY)r.mesh->primitiveType );
        
        d3dContext->IASetVertexBuffers( 0, 1, &d3dvb, &stride, &offset );

        if(r.mesh->indexBuffer)
        {
            uint32_t startIndex  = 0;
            uint32_t indexCount  = r.mesh->indexBuffer->GetCount();
            IndexBuffer* d3dib  = r.mesh->indexBuffer;
            d3dContext->IASetIndexBuffer(d3dib->GetBuffer(),(DXGI_FORMAT)d3dib->GetFormat(),0);    
            d3dContext->DrawIndexed(indexCount,startIndex,startVertex);
        }
        else
        {
            d3dContext->Draw(r.mesh->vertexBuffer->GetCount(),startVertex);
        }        
    }
}

// ------------------------------------------------------------------------------------------------
BasicShader::BasicShader(ID3D11Device* device)
    : Shader( Shaders::BasicShader )
{
    // create cbuffers.
    m_cbPerFrame.Construct(device);
    m_cbPerObject.Construct(device);

    // compile shaders
    ID3DBlob* vsBlob = CompileShaderFromResource(L"BasicShader.hlsl", "VS","vs_4_0", NULL);    
    ID3DBlob* psBlob = CompileShaderFromResource(L"BasicShader.hlsl", "PS","ps_4_0", NULL);
    assert(vsBlob && psBlob);
    m_vsShader = GpuResourceFactory::CreateVertexShader(vsBlob);     
    m_psShader = GpuResourceFactory::CreatePixelShader(psBlob);
    assert(m_vsShader && m_psShader);
    
    // create input layout
    m_vertexLayout = GpuResourceFactory::CreateInputLayout(vsBlob, VertexFormat::VF_P);
    assert(m_vertexLayout);

    // release the blobs
    vsBlob->Release();
    psBlob->Release();
}

// --------------------------------------------------------------------------------------------------
BasicShader::~BasicShader()
{    
    SAFE_RELEASE(m_vertexLayout);
    SAFE_RELEASE(m_vsShader);
    SAFE_RELEASE(m_psShader);
}

// --------------------------------------------------------------------------------------------------
void BasicShader::SetRenderFlag(RenderFlagsEnum rf)
{
    ID3D11DeviceContext*  d3dcontext = m_rc->Context();

    // set blend state 
    auto blendState = RSCache::Inst()->GetBlendState(rf);
    float blendFactor[4] = {1.0f};
    d3dcontext->OMSetBlendState(blendState, blendFactor, 0xffffffff);        

    // set depth stencil state
    auto depthState  = RSCache::Inst()->GetDepthStencilState(rf);
    d3dcontext->OMSetDepthStencilState(depthState,0);

    CullModeEnum cullmode = (rf & RenderFlags::RenderBackFace) ? CullMode::NONE : CullMode::BACK;
    auto rasterState = RSCache::Inst()->GetRasterState( FillMode::Solid, cullmode );
    d3dcontext->RSSetState(rasterState);
}
