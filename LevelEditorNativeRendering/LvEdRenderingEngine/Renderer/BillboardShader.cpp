//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "BillboardShader.h"
#include "../Core/Utils.h"
#include "RenderBuffer.h"
#include "Texture.h"
#include "RenderContext.h"
#include "RenderState.h"
#include "RenderUtil.h"
#include "Lights.h"
#include "Model.h"
#include "TextureLib.h"
#include "ShaderLib.h"
#include "GpuResourceFactory.h"



#define Float4Align __declspec(align(16))

namespace LvEdEngine
{


static const bool UseRightHand = true; // for testing.

// --------------------------------------------------------------------------------------------------
BillboardShader::BillboardShader(ID3D11Device* device)
  : Shader( Shaders::BillboardShader ), m_rc(NULL)
{
    assert(device);
    m_cbPerFrame.Construct(device);
    m_cbPerDraw.Construct(device);
    
    // create shaders
    ID3DBlob* pVSBlob = CompileShaderFromResource(L"Billboard.hlsl", "VSMain","vs_4_0", NULL);
    ID3DBlob* pPSBlob = CompileShaderFromResource(L"Billboard.hlsl", "PSMain","ps_4_0", NULL);
    assert(pVSBlob && pPSBlob);
        
    m_vertexShader = GpuResourceFactory::CreateVertexShader(pVSBlob);
    m_pixelShader  =  GpuResourceFactory::CreatePixelShader(pPSBlob);
    assert(m_vertexShader && m_pixelShader);
    
    m_vertexLayout = GpuResourceFactory::CreateInputLayout(pVSBlob, VertexFormat::VF_PNTT);
    assert(m_vertexLayout);

    // release blob memory
    pVSBlob->Release();
    pPSBlob->Release();    
}


// --------------------------------------------------------------------------------------------------
BillboardShader::~BillboardShader()
{
    SAFE_RELEASE(m_vertexShader);
    SAFE_RELEASE(m_pixelShader);
    SAFE_RELEASE(m_vertexLayout);    
}

// --------------------------------------------------------------------------------------------------
void BillboardShader::SetRenderFlag(RenderFlagsEnum rf)
{
    m_renderFlags = rf;

    ID3D11DeviceContext* dc = m_rc->Context();
        
    // if solid and wireframe bit are set then choose solid.
    CullModeEnum cullmode = (rf & RenderFlags::RenderBackFace) ? CullMode::NONE : CullMode::BACK;
    auto rasterState = RSCache::Inst()->GetRasterState( FillMode::Solid, cullmode );
    dc->RSSetState(rasterState);

    // set blend state 
    auto blendState = RSCache::Inst()->GetBlendState(rf);
    float blendFactor[4] = {1.0f};
    dc->OMSetBlendState(blendState, blendFactor, 0xffffffff);        

    // set depth stencil state
    auto depthState  = RSCache::Inst()->GetDepthStencilState(rf);
    dc->OMSetDepthStencilState(depthState,0);

}
// --------------------------------------------------------------------------------------------------
void BillboardShader::Begin(RenderContext* rc)
{
    m_rc = rc;
    ID3D11DeviceContext* dc = rc->Context();
    
    // update cbuffer
    Matrix::Transpose(rc->Cam().View(),m_cbPerFrame.Data.viewXform);
    Matrix::Transpose(rc->Cam().Proj(),m_cbPerFrame.Data.projXform);
    m_cbPerFrame.Update(dc);
    

    // set per call buffer.
    auto perframeCb = m_cbPerFrame.GetBuffer();
    auto perDrawCb = m_cbPerDraw.GetBuffer();
    dc->VSSetConstantBuffers(0,1,&perframeCb);
    dc->PSSetConstantBuffers(0,1,&perframeCb);
    dc->VSSetConstantBuffers(1,1,&perDrawCb);
    dc->PSSetConstantBuffers(1,1,&perDrawCb);


    // set vs, ps and layout.
    dc->VSSetShader(m_vertexShader,NULL,0);
    dc->PSSetShader(m_pixelShader,NULL,0);
    dc->IASetInputLayout(m_vertexLayout);

    // setup samplers
    auto sampState = RSCache::Inst()->LinearWrap();
    dc->PSSetSamplers( 0, 1, &sampState);    
}

// --------------------------------------------------------------------------------------------------
void BillboardShader::End()
{
    m_rc = NULL;
}

// --------------------------------------------------------------------------------------------------
void BillboardShader::DrawNodes(const RenderNodeList& renderNodes)
{
    for(auto it = renderNodes.begin(); it != renderNodes.end(); it++)
    {
        const RenderableNode& renderable = (*it);
        Draw( renderable );
    }
}

// --------------------------------------------------------------------------------------------------
void BillboardShader::Draw(const RenderableNode& r)
{
    ID3D11DeviceContext* dc = m_rc->Context();

    // verify lighting
    assert(r.lighting.numDirLights <= MAX_DIR_LIGHTS);
    assert(r.lighting.numBoxLights <= MAX_BOX_LIGHTS);
    assert(r.lighting.numPointLights <= MAX_POINT_LIGHTS);

    Matrix::Transpose(r.WorldXform,m_cbPerDraw.Data.worldXform);
    Matrix::Transpose(r.TextureXForm, m_cbPerDraw.Data.textureXForm);
    m_cbPerDraw.Data.color = r.diffuse;
    m_cbPerDraw.Update(dc);
    
    ID3D11ShaderResourceView* diffuseMap[1] = {NULL};    
    if( (m_renderFlags & RenderFlags::Textured) && r.textures[TextureType::DIFFUSE])
    {
        diffuseMap[0] = r.textures[TextureType::DIFFUSE]->GetView();
    }
    else
    {
        diffuseMap[0] = TextureLib::Inst()->GetWhite()->GetView();
    }
   
    dc->PSSetShaderResources( 0, 1, diffuseMap );

    uint32_t stride = r.mesh->vertexBuffer->GetStride();
    uint32_t offset = 0;    
    uint32_t indexCount = r.mesh->indexBuffer->GetCount();
    ID3D11Buffer* d3dvb = r.mesh->vertexBuffer->GetBuffer();
    ID3D11Buffer* d3dib = r.mesh->indexBuffer->GetBuffer();

    dc->IASetPrimitiveTopology( (D3D11_PRIMITIVE_TOPOLOGY)r.mesh->primitiveType );    
    dc->IASetVertexBuffers( 0, 1, &d3dvb, &stride, &offset );
    dc->IASetIndexBuffer(d3dib, (DXGI_FORMAT) r.mesh->indexBuffer->GetFormat(), 0);
    dc->DrawIndexed(indexCount,0,0 );
}


}; //namespace

