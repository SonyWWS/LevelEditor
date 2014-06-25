//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    SkyDomeShader.cpp

****************************************************************************/
#include "SkyDomeShader.h"
#include "../Core/Utils.h"
#include "RenderBuffer.h"
#include "Texture.h"
#include "RenderContext.h"
#include "RenderState.h"
#include "RenderUtil.h"
#include "Lights.h"
#include "Model.h"

using namespace LvEdEngine;



//---------------------------------------------------------------------------
SkyDomeShader::SkyDomeShader(ID3D11Device* device)
  : Shader( Shaders::SkyDomeShader )
{
    assert(device);    
    // create constant buffers
    m_constantBufferPerFrame = CreateConstantBuffer(device, sizeof(cbPerFrame));
    assert(m_constantBufferPerFrame);

    
    // create shaders        
    ID3DBlob* pVSBlob = CompileShaderFromResource(L"SkySphere.hlsl", "VS_Render","vs_4_0", NULL);    
    ID3DBlob* pPSBlob = CompileShaderFromResource(L"SkySphere.hlsl", "PS_Render","ps_4_0", NULL);
    assert(pVSBlob);
    assert(pPSBlob);

    m_vertexShader = CreateVertexShader(device, pVSBlob);
    m_pixelShader = CreatePixelShader(device, pPSBlob);
    assert(m_vertexShader);
    assert(m_pixelShader);

    m_vertexLayout = CreateInputLayout(device, pVSBlob, VertexFormat::VF_P);
    assert(m_vertexLayout);

    // create sampler state
    // TODO: Get this from cache.
    D3D11_SAMPLER_DESC sampDesc;
    SecureZeroMemory( &sampDesc, sizeof(sampDesc) );
    sampDesc.Filter = D3D11_FILTER_MIN_MAG_LINEAR_MIP_POINT;
    sampDesc.AddressU = D3D11_TEXTURE_ADDRESS_CLAMP;// 
    sampDesc.AddressV = D3D11_TEXTURE_ADDRESS_CLAMP;
    sampDesc.AddressW = D3D11_TEXTURE_ADDRESS_CLAMP;
    sampDesc.ComparisonFunc = D3D11_COMPARISON_NEVER;
    sampDesc.MinLOD = 0;
    sampDesc.MaxLOD = D3D11_FLOAT32_MAX;		
    device->CreateSamplerState( &sampDesc, &m_samplerState );
    assert(m_samplerState);

    // release blob memory
    pVSBlob->Release();
    pPSBlob->Release();
}


//---------------------------------------------------------------------------
SkyDomeShader::~SkyDomeShader()
{
    SAFE_RELEASE(m_vertexShader);
    SAFE_RELEASE(m_pixelShader);
    SAFE_RELEASE(m_vertexLayout);
    SAFE_RELEASE(m_constantBufferPerFrame);
    SAFE_RELEASE(m_samplerState);
}

//---------------------------------------------------------------------------
void SkyDomeShader::Begin(RenderContext* rc)
{
    m_rc = rc;
    ID3D11DeviceContext* d3dcontext = rc->Context();

    // Update Constant Buffers
    float4 vp = rc->ViewPort();
    float aspect = vp.x / vp.y; // width / height.
    Matrix proj = Matrix::CreatePerspectiveFieldOfView(PiOver4,aspect,0.1f,1.0f);
    Matrix view = rc->Cam().View();
    view.M41 = 0;
    view.M42 = 0;
    view.M43 = 0;
    
    cbPerFrame cb;	            
    Matrix::Transpose(view,cb.view);
    Matrix::Transpose(proj,cb.proj);    
    UpdateConstantBuffer(d3dcontext,m_constantBufferPerFrame,&cb,sizeof(cb));
    	
    // set per frame buffer.
	d3dcontext->VSSetConstantBuffers(0,1,&m_constantBufferPerFrame);
        
	// set vs, ps and layout.
	d3dcontext->VSSetShader(m_vertexShader,NULL,0);
	d3dcontext->PSSetShader(m_pixelShader,NULL,0);

    // sampler state
    d3dcontext->PSSetSamplers( 0, 1, &m_samplerState );

    // raster state
    ID3D11RasterizerState* rasterState = rc->GetRenderStateCache()->GetRasterState(FillMode::Solid, CullMode::NONE);
	d3dcontext->RSSetState(rasterState);                

    // depth stencil state
    ID3D11DepthStencilState* depthState = m_rc->GetRenderStateCache()->GetDepthStencilState((RenderFlagsEnum)(RenderFlags::DisableDepthTest | RenderFlags::DisableDepthWrite) );
    d3dcontext->OMSetDepthStencilState(depthState, 0);

    // default topology
    d3dcontext->IASetPrimitiveTopology( D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST );
}


//---------------------------------------------------------------------------
void SkyDomeShader::End()
{
    m_rc = NULL;
}

//---------------------------------------------------------------------------
void SkyDomeShader::SetRenderFlag(RenderFlagsEnum /*rf*/)
{
}

//---------------------------------------------------------------------------
void SkyDomeShader::DrawNodes(const RenderNodeList& nodes)
{
    for(auto it = nodes.begin(); it != nodes.end(); it++)
    {
        const RenderableNode& renderable = (*it);            
        Draw( renderable );
    }
}

// --------------------------------------------------------------------------------------------------
void SkyDomeShader::Draw(const RenderableNode& r)
{	
    ID3D11DeviceContext*  d3dcontext = m_rc->Context();
    
    ID3D11ShaderResourceView* cubemap[1] = {NULL};
    if(r.textures[TextureType::Cubemap])
    {
        cubemap[0] =r.textures[TextureType::Cubemap]->GetView();               
    }   
    d3dcontext->PSSetShaderResources( 0, 1, cubemap );    

    uint32_t stride = r.mesh->vertexBuffer->GetStride();
    uint32_t offset = 0;
    uint32_t startIndex = 0;
    uint32_t startVertex= 0;
    uint32_t indexCount = r.mesh->indexBuffer->GetCount();
    ID3D11Buffer* d3dvb = r.mesh->vertexBuffer->GetBuffer();
    ID3D11Buffer* d3dib = r.mesh->indexBuffer->GetBuffer();

    d3dcontext->IASetPrimitiveTopology( (D3D11_PRIMITIVE_TOPOLOGY)r.mesh->primitiveType );
    d3dcontext->IASetInputLayout(m_vertexLayout);
    d3dcontext->IASetVertexBuffers( 0, 1, &d3dvb, &stride, &offset );
    d3dcontext->IASetIndexBuffer(d3dib, DXGI_FORMAT_R32_UINT, 0);
    
    d3dcontext->DrawIndexed(indexCount, startIndex, startVertex);
}

