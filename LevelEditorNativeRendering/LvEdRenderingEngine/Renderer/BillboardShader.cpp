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



#define Float4Align __declspec(align(16))

namespace LvEdEngine
{


static const bool UseRightHand = true; // for testing.

// --------------------------------------------------------------------------------------------------
BillboardShader::BillboardShader(ID3D11Device* device)
  : Shader( Shaders::BillboardShader ), m_rc(NULL)
{
    Initialize( device );
}

// --------------------------------------------------------------------------------------------------
void BillboardShader::Initialize(ID3D11Device* device)
{
    assert(device);
    // create constant buffers
    m_constantBufferPerFrame = CreateConstantBuffer(device, sizeof(ConstantBufferPerFrame));
    m_constantBufferPerDraw = CreateConstantBuffer(device, sizeof(ConstantBufferPerDraw));

    assert(m_constantBufferPerFrame);
    assert(m_constantBufferPerDraw);


    // create shaders
    ID3DBlob* pVSBlob = CompileShaderFromResource(L"Billboard.hlsl", "VSMain","vs_4_0", NULL);
    ID3DBlob* pPSBlob = CompileShaderFromResource(L"Billboard.hlsl", "PSMain","ps_4_0", NULL);
    assert(pVSBlob);
    assert(pPSBlob);

    m_vertexShader = CreateVertexShader(device, pVSBlob);
    m_pixelShader = CreatePixelShader(device, pPSBlob);
    assert(m_vertexShader);
    assert(m_pixelShader);

    m_vertexLayout = CreateInputLayout(device, pVSBlob, VertexFormat::VF_PNTT);
    assert(m_vertexLayout);

    // create raster state
    m_rasterStateSolid = CreateRasterState(device, FillMode::Solid);
    assert(m_rasterStateSolid);

    // create sampler state
    m_samplerState = CreateSamplerState(device);
    assert(m_samplerState);

   // RenderStateCache* rscache = RenderContext::Inst()->GetRenderStateCache();
  //  D3D11_SAMPLER_DESC smpDescr = rscache->GetDefaultSampler();
  //  smpDescr.Filter = D3D11_FILTER_MIN_MAG_MIP_POINT;
  //  smpDescr.AddressU = D3D11_TEXTURE_ADDRESS_CLAMP;
  //  smpDescr.AddressV = D3D11_TEXTURE_ADDRESS_CLAMP;
  //  device->CreateSamplerState(&smpDescr, &m_samplerState);
 //   assert(m_samplerState);



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
    SAFE_RELEASE(m_constantBufferPerFrame);
    SAFE_RELEASE(m_constantBufferPerDraw);
    SAFE_RELEASE(m_rasterStateSolid);
    SAFE_RELEASE(m_samplerState);
}

// --------------------------------------------------------------------------------------------------
void BillboardShader::SetRenderFlag(RenderFlagsEnum rf)
{
    m_renderFlags = rf;

     ID3D11DeviceContext*  d3dcontext = m_rc->Context();
        
    // if solid and wireframe bit are set then choose solid.
    CullModeEnum cullmode = (rf & RenderFlags::RenderBackFace) ? CullMode::NONE : CullMode::BACK;
    auto rasterState = m_rc->GetRenderStateCache()->GetRasterState( FillMode::Solid, cullmode );
    d3dcontext->RSSetState(rasterState);

    // set blend state 
    auto blendState = m_rc->GetRenderStateCache()->GetBlendState(rf);
    float blendFactor[4] = {1.0f};
    d3dcontext->OMSetBlendState(blendState, blendFactor, 0xffffffff);        

    // set depth stencil state
    auto depthState  = m_rc->GetRenderStateCache()->GetDepthStencilState(rf);
    d3dcontext->OMSetDepthStencilState(depthState,0);

}
// --------------------------------------------------------------------------------------------------
void BillboardShader::Begin(RenderContext* rc)
{
    m_rc = rc;
    ID3D11DeviceContext*  d3dcontext = rc->Context();
    Matrix invView = rc->Cam().View();
    invView.Invert();

    ConstantBufferPerFrame cb;
    Matrix::Transpose(rc->Cam().View(),cb.viewXform);
    Matrix::Transpose(rc->Cam().Proj(),cb.projXform);    
    UpdateConstantBuffer(d3dcontext,m_constantBufferPerFrame,&cb,sizeof(cb));

    // set per call buffer.
    d3dcontext->VSSetConstantBuffers(0,1,&m_constantBufferPerFrame);
    d3dcontext->PSSetConstantBuffers(0,1,&m_constantBufferPerFrame);

    // set vs, ps and layout.
    d3dcontext->VSSetShader(m_vertexShader,NULL,0);
    d3dcontext->PSSetShader(m_pixelShader,NULL,0);

    // setup samplers
    d3dcontext->PSSetSamplers( 0, 1, &m_samplerState );

    // raster state
    d3dcontext->RSSetState(m_rasterStateSolid);

    // depth stencil state
    ID3D11DepthStencilState* depth = m_rc->GetRenderStateCache()->GetDepthStencilState(RenderFlags::None);
    d3dcontext->OMSetDepthStencilState(depth, 0);

    d3dcontext->VSSetConstantBuffers(1,1,&m_constantBufferPerDraw);
    d3dcontext->PSSetConstantBuffers(1,1,&m_constantBufferPerDraw);
    d3dcontext->IASetPrimitiveTopology( D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST );
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
    ID3D11DeviceContext*  d3dcontext = m_rc->Context();

    // verify lighting
    assert(r.lighting.numDirLights <= MAX_DIR_LIGHTS);
    assert(r.lighting.numBoxLights <= MAX_BOX_LIGHTS);
    assert(r.lighting.numPointLights <= MAX_POINT_LIGHTS);

    ConstantBufferPerDraw cb;
    Matrix::Transpose(r.WorldXform,cb.worldXform);
    Matrix::Transpose(r.TextureXForm, cb.textureXForm);
    cb.intensity = r.diffuse.x; // convention to use this    
    UpdateConstantBuffer(d3dcontext,m_constantBufferPerDraw,&cb,sizeof(cb));

    ID3D11ShaderResourceView* diffuseMap[1] = {NULL};
    diffuseMap[0] = TextureLib::Inst()->GetWhite()->GetView();
    if( (m_renderFlags & RenderFlags::Textured) && r.textures[TextureType::DIFFUSE])
    {
        diffuseMap[0] = r.textures[TextureType::DIFFUSE]->GetView();
    }
   
    d3dcontext->PSSetShaderResources( 0, 1, diffuseMap );

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


}; //namespace

