//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


#include "TexturedShader.h"
#include "RenderUtil.h"
#include "RenderState.h"
#include "RenderContext.h"
#include "Texture.h"
#include "Model.h"
#include "GpuResourceFactory.h"

using namespace LvEdEngine;

//---------------------------------------------------------------------------
TexturedShader::TexturedShader(ID3D11Device* device)
  : Shader( Shaders::TexturedShader),
    m_rc( NULL ),    
    m_shaderSceneRenderVS( NULL ),
    m_shaderSceneRenderPS( NULL ),    
    m_pVertexLayoutMesh( NULL )    
{
    
    //  compile and create Vertex shader
    ID3DBlob* m_shaderSceneRenderVSBlob =  CompileShaderFromResource(L"TexturedShader.hlsl","VSMain","vs_4_0", NULL);
    assert(m_shaderSceneRenderVSBlob);
    m_shaderSceneRenderVS = GpuResourceFactory::CreateVertexShader(m_shaderSceneRenderVSBlob);
    assert(m_shaderSceneRenderVS);
    
    ID3DBlob* m_shaderSceneRenderPSBlob =  CompileShaderFromResource(L"TexturedShader.hlsl","PSMain","ps_4_0", NULL);
    assert(m_shaderSceneRenderPSBlob);
    m_shaderSceneRenderPS = GpuResourceFactory::CreatePixelShader(m_shaderSceneRenderPSBlob);
    assert(m_shaderSceneRenderPS);

    SAFE_RELEASE( m_shaderSceneRenderPSBlob );

    // create layout.
    m_pVertexLayoutMesh = GpuResourceFactory::CreateInputLayout(m_shaderSceneRenderVSBlob,VertexFormat::VF_PNTT);
    SAFE_RELEASE( m_shaderSceneRenderVSBlob );
    
    // create constant buffers.
    m_perFrameCb.Construct(device);
    m_perDrawCb.Construct(device);
    m_renderStateCb.Construct(device);    
}

//---------------------------------------------------------------------------
TexturedShader::~TexturedShader()
{
    SAFE_RELEASE(m_shaderSceneRenderVS);
    SAFE_RELEASE(m_shaderSceneRenderPS);    
    SAFE_RELEASE( m_pVertexLayoutMesh );
}


//---------------------------------------------------------------------------
void TexturedShader::SetRenderFlag(RenderFlagsEnum rf)
{
   
    ID3D11DeviceContext*  d3dcontext = m_rc->Context();
        
    m_renderStateCb.Data.cb_textured   = (rf & RenderFlags::Textured) != 0;
    m_renderStateCb.Data.cb_lit        = (rf & RenderFlags::Lit) != 0;
    m_renderStateCb.Data.cb_shadowed   = ShadowMaps::Inst()->IsEnabled();
    m_renderStateCb.Update(d3dcontext);
        
    // if solid and wireframe bit are set then choose solid.
    CullModeEnum cullmode = (rf & RenderFlags::RenderBackFace) ? CullMode::NONE : CullMode::BACK;
    auto rasterState = RSCache::Inst()->GetRasterState( FillMode::Solid, cullmode );
    d3dcontext->RSSetState(rasterState);

    // set blend state 
    auto blendState = RSCache::Inst()->GetBlendState(rf);
    float blendFactor[4] = {1.0f};
    d3dcontext->OMSetBlendState(blendState, blendFactor, 0xffffffff);        

    // set depth stencil state
    auto depthState  = RSCache::Inst()->GetDepthStencilState(rf);
    d3dcontext->OMSetDepthStencilState(depthState,0);
   
}

//---------------------------------------------------------------------------
void TexturedShader::Begin(RenderContext* rc)
{
    m_rc = rc;    
    
    ID3D11DeviceContext*  d3dcontext = m_rc->Context();

    // depth stencil state
    ID3D11DepthStencilState* depth = RSCache::Inst()->GetDepthStencilState(RenderFlags::None);
    d3dcontext->OMSetDepthStencilState(depth, 0);

    // update per frame cb    
    Matrix::Transpose(m_rc->Cam().View(),m_perFrameCb.Data.cb_view);
    Matrix::Transpose(m_rc->Cam().Proj(),m_perFrameCb.Data.cb_proj);         
    m_perFrameCb.Data.cb_camPosW = m_rc->Cam().CamPos(); 
    m_perFrameCb.Data.cb_fog = m_rc->GlobalFog();    
    m_perFrameCb.Update(d3dcontext);

    // set input layout.
    d3dcontext->IASetInputLayout( m_pVertexLayoutMesh );

    // set texture samplers
    ID3D11SamplerState* samplers[] = 
    {        
        RSCache::Inst()->LinearWrap(),
        ShadowMaps::Inst()->GetSamplerState()
    };

    d3dcontext->PSSetSamplers( 0, ARRAY_SIZE(samplers), samplers);
    
    d3dcontext->GSSetShader( NULL, NULL, 0 );
    d3dcontext->VSSetShader( m_shaderSceneRenderVS, NULL, 0 );
    d3dcontext->PSSetShader( m_shaderSceneRenderPS, NULL, 0 );

    ID3D11ShaderResourceView* srv = ShadowMaps::Inst()->GetShaderResourceView();
    d3dcontext->PSSetShaderResources( 3,1, &srv );

    ID3D11Buffer* constantBuffers[] = {
        m_perFrameCb.GetBuffer(),
        m_renderStateCb.GetBuffer(),
        m_perDrawCb.GetBuffer(),
        ShadowMaps::Inst()->GetShadowConstantBuffer()
    };
    
    d3dcontext->VSSetConstantBuffers( 0, ARRAY_SIZE(constantBuffers), constantBuffers);
    d3dcontext->PSSetConstantBuffers( 0, ARRAY_SIZE(constantBuffers), constantBuffers);    
}

// --------------------------------------------------------------------------------------------------
void TexturedShader::End()
{    
    ID3D11DeviceContext*  d3dcontext = m_rc->Context();
    ID3D11ShaderResourceView* texviews[] = {NULL, NULL, NULL, NULL };
    d3dcontext->PSSetShaderResources(0, ARRAY_SIZE(texviews) , texviews);
    m_rc = NULL;
}

//---------------------------------------------------------------------------
void TexturedShader::DrawNodes(const RenderNodeList& renderNodes)
{               
    for(auto it = renderNodes.begin(); it != renderNodes.end(); it++)
    {        
        const RenderableNode& renderable = (*it);
        DrawRenderable( renderable );
    }
}

//---------------------------------------------------------------------------
void TexturedShader::DrawRenderable(const RenderableNode& r)
{

    // update per draw cb.
    ID3D11DeviceContext*  dc = m_rc->Context();
    ID3D11ShaderResourceView* textures[] = {NULL, NULL, NULL};
    
    Matrix::Transpose(r.WorldXform, m_perDrawCb.Data.cb_world );
    m_perDrawCb.Data.cb_hasDiffuseMap = 0;
    m_perDrawCb.Data.cb_hasNormalMap = 0;
    m_perDrawCb.Data.cb_hasSpecularMap = 0;
    m_perDrawCb.Data.cb_lighting =  r.lighting;

    Matrix w = r.WorldXform;        
    w.M41 = w.M42 = w.M43 = 0; w.M44 = 1;
    Matrix::Invert(w,m_perDrawCb.Data.cb_worldInvTrans);
    Matrix::Transpose(r.TextureXForm, m_perDrawCb.Data.cb_textureTrans);
    m_perDrawCb.Data.cb_matDiffuse     = r.diffuse;
    m_perDrawCb.Data.cb_matEmissive    = r.emissive;
    m_perDrawCb.Data.cb_matSpecular    = float4(r.specular.x,r.specular.y, r.specular.z, r.specPower);

    if(r.textures[TextureType::DIFFUSE])
    {
        m_perDrawCb.Data.cb_hasDiffuseMap = 1;
        textures[0] = r.textures[TextureType::DIFFUSE]->GetView();
    }

    if(r.textures[TextureType::NORMAL])
    {
        m_perDrawCb.Data.cb_hasNormalMap = 1;
        textures[1] = r.textures[TextureType::NORMAL]->GetView();
    }
        
    m_perDrawCb.Update(dc);
    
    dc->PSSetShaderResources( 0, ARRAY_SIZE(textures), textures );
            
    uint32_t stride = r.mesh->vertexBuffer->GetStride();
    uint32_t offset = 0;
    uint32_t startIndex  = 0;
    uint32_t startVertex = 0;    
    uint32_t indexCount = r.mesh->indexBuffer->GetCount();        
    ID3D11Buffer* d3dvb = r.mesh->vertexBuffer->GetBuffer();
    ID3D11Buffer* d3dib = r.mesh->indexBuffer->GetBuffer();
    dc->IASetPrimitiveTopology( (D3D11_PRIMITIVE_TOPOLOGY)r.mesh->primitiveType );    
    dc->IASetVertexBuffers( 0, 1, &d3dvb, &stride, &offset );
    dc->IASetIndexBuffer(d3dib, (DXGI_FORMAT)r.mesh->indexBuffer->GetFormat(), 0);
    dc->DrawIndexed(indexCount, startIndex, startVertex);
}


