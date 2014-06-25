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
#include <DxErr.h>

using namespace LvEdEngine;

// ------------------------------------------------------------------------------------------------
void BasicShader::Begin(RenderContext* context)
{
    m_rc = context;
    ID3D11DeviceContext*  d3dcontext = m_rc->Context();

    // setup per frame buffer
	BasicCbPerFrame cb;
    Matrix::Transpose(m_rc->Cam().View(),cb.viewXform);
    Matrix::Transpose(m_rc->Cam().Proj(),cb.projXform);   	
    UpdateConstantBuffer(d3dcontext,m_cbPerFrame,&cb,sizeof(cb));
    d3dcontext->VSSetConstantBuffers(0,1,&m_cbPerFrame);
    d3dcontext->PSSetConstantBuffers(0,1,&m_cbPerFrame);	
    
	// set vs, ps and layout.
	d3dcontext->VSSetShader(m_vsShader,NULL,0);
	d3dcontext->PSSetShader(m_psShader,NULL,0);

    // raster state
    ID3D11RasterizerState* rasterState = m_rc->GetRenderStateCache()->GetRasterState(FillMode::Solid,CullMode::BACK);
    d3dcontext->RSSetState(rasterState);

    // depth stencil state
    ID3D11DepthStencilState* depth = m_rc->GetRenderStateCache()->GetDepthStencilState(RenderFlags::None);
    d3dcontext->OMSetDepthStencilState(depth, 0);

    // setup per object buffer
    d3dcontext->VSSetConstantBuffers(1,1,&m_cbPerObject);
	d3dcontext->PSSetConstantBuffers(1,1,&m_cbPerObject);	
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
    BasicCbPerObject cb;

    for ( auto it = renderNodes.begin(); it != renderNodes.end(); ++it )
    {
        const RenderableNode& r = (*it);

        Matrix::Transpose(r.WorldXform,cb.worldXform);
        cb.color = r.diffuse;        
        UpdateConstantBuffer(d3dContext,m_cbPerObject,&cb,sizeof(cb));

        uint32_t stride = r.mesh->vertexBuffer->GetStride();
        uint32_t offset = 0;    
        uint32_t startVertex = 0;
        ID3D11Buffer* d3dvb  = r.mesh->vertexBuffer->GetBuffer();
        d3dContext->IASetPrimitiveTopology( (D3D11_PRIMITIVE_TOPOLOGY)r.mesh->primitiveType );
        d3dContext->IASetInputLayout(m_vertexLayout);
        d3dContext->IASetVertexBuffers( 0, 1, &d3dvb, &stride, &offset );

        if(r.mesh->indexBuffer)
        {
            uint32_t startIndex  = 0;
            uint32_t indexCount  = r.mesh->indexBuffer->GetCount();;
            ID3D11Buffer* d3dib  = r.mesh->indexBuffer->GetBuffer();
            d3dContext->IASetIndexBuffer(d3dib,DXGI_FORMAT_R32_UINT,0);    
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
    m_cbPerFrame  = CreateConstantBuffer(device, sizeof(BasicCbPerFrame));
    m_cbPerObject = CreateConstantBuffer(device, sizeof(BasicCbPerObject));

    // compile shaders
    ID3DBlob* vsBlob = CompileShaderFromResource(L"BasicShader.hlsl", "VS","vs_4_0", NULL);    
    ID3DBlob* psBlob = CompileShaderFromResource(L"BasicShader.hlsl", "PS","ps_4_0", NULL);
    assert(vsBlob);
    assert(psBlob);
    m_vsShader = CreateVertexShader(device, vsBlob);
    m_psShader = CreatePixelShader(device, psBlob);
    assert(m_vsShader);
    assert(m_psShader);

    // create input layout
    m_vertexLayout = CreateInputLayout(device, vsBlob, VertexFormat::VF_P);
    assert(m_vertexLayout);

    // release the blobs
    vsBlob->Release();
    psBlob->Release();
}

// --------------------------------------------------------------------------------------------------
BasicShader::~BasicShader()
{
    SAFE_RELEASE(m_cbPerFrame);
    SAFE_RELEASE(m_cbPerObject);
    SAFE_RELEASE(m_vertexLayout);
    SAFE_RELEASE(m_vsShader);
    SAFE_RELEASE(m_psShader);
}

// --------------------------------------------------------------------------------------------------
void BasicShader::SetRenderFlag(RenderFlagsEnum rf)
{
    ID3D11DeviceContext*  d3dcontext = m_rc->Context();

    // set blend state 
    auto blendState = m_rc->GetRenderStateCache()->GetBlendState(rf);
    float blendFactor[4] = {1.0f};
    d3dcontext->OMSetBlendState(blendState, blendFactor, 0xffffffff);        

    // set depth stencil state
    auto depthState  = m_rc->GetRenderStateCache()->GetDepthStencilState(rf);
    d3dcontext->OMSetDepthStencilState(depthState,0);

    CullModeEnum cullmode = (rf & RenderFlags::RenderBackFace) ? CullMode::NONE : CullMode::BACK;
    auto rasterState = m_rc->GetRenderStateCache()->GetRasterState( FillMode::Solid, cullmode );
    d3dcontext->RSSetState(rasterState);

}
