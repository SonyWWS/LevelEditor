//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    TexturedShader.cpp

****************************************************************************/
#include <d3dx11.h>
#include <DxErr.h>
#include <D3DX10math.h>
#include "../Core/NonCopyable.h"
#include "RenderUtil.h"
#include "RenderState.h"
#include "Texture.h"
#include "Model.h"
#include "RenderBuffer.h"
#include "TexturedShader.h"
#include "ScreenMsgPrinter.h"
#include "Lights.h"
#include "../GobSystem/GameLevel.h"
#include "FontRenderer.h"
#include "../LvEdUtils.h"
#include "../Core/Logger.h"

using namespace LvEdEngine;


//---------------------------------------------------------------------------
TexturedShader::TexturedShader(ID3D11Device* device)
  : Shader( Shaders::TexturedShader),
    m_rc( NULL ),        
    m_ShadowMapState( ShadowMaps::Inst() ),
    m_shaderSceneRenderVS( NULL ),
    m_shaderSceneRenderPS( NULL ),    
    m_pVertexLayoutMesh( NULL ),
    m_pSceneSamplerState( NULL ),
    m_pConstantBufferPerFrame( NULL ),
    m_pConstantBufferRenderState( NULL ),
    m_pConstantBufferPerDraw( NULL)
{
    
    //  compile and create Vertex shader
    ID3DBlob* m_shaderSceneRenderVSBlob =  CompileShaderFromResource(L"TexturedShader.hlsl","VSMain","vs_4_0", NULL);
    assert(m_shaderSceneRenderVSBlob);
    m_shaderSceneRenderVS = CreateVertexShader(device, m_shaderSceneRenderVSBlob);
    assert(m_shaderSceneRenderVS);
    
    ID3DBlob* m_shaderSceneRenderPSBlob =  CompileShaderFromResource(L"TexturedShader.hlsl","PSMain","ps_4_0", NULL);
    assert(m_shaderSceneRenderPSBlob);
    m_shaderSceneRenderPS = CreatePixelShader(device, m_shaderSceneRenderPSBlob);
    assert(m_shaderSceneRenderPS);

    SAFE_RELEASE( m_shaderSceneRenderPSBlob );

    // create layout.
    m_pVertexLayoutMesh = CreateInputLayout(device,m_shaderSceneRenderVSBlob,VertexFormat::VF_PNTT);
    SAFE_RELEASE( m_shaderSceneRenderVSBlob );
    
    // create sampler state for diffuse and normal
    m_pSceneSamplerState = CreateSamplerState( device );
    
    // create constant buffers.    
    m_pConstantBufferPerFrame = CreateConstantBuffer(device,sizeof( ConstantBufferPerFrame ));
    m_pConstantBufferPerDraw = CreateConstantBuffer(device,sizeof( ConstantBufferPerDraw ));
    m_pConstantBufferRenderState = CreateConstantBuffer(device,sizeof( ConstantBufferRenderState ));

}

//---------------------------------------------------------------------------
TexturedShader::~TexturedShader()
{
    SAFE_RELEASE(m_shaderSceneRenderVS);
    SAFE_RELEASE(m_shaderSceneRenderPS);    
    SAFE_RELEASE(m_pSceneSamplerState);    
    SAFE_RELEASE( m_pVertexLayoutMesh );
    SAFE_RELEASE( m_pConstantBufferPerFrame );
    SAFE_RELEASE( m_pConstantBufferRenderState );
    SAFE_RELEASE( m_pConstantBufferPerDraw );
}


//---------------------------------------------------------------------------
void TexturedShader::SetRenderFlag(RenderFlagsEnum rf)
{
   
    ID3D11DeviceContext*  d3dcontext = m_rc->Context();

    ConstantBufferRenderState renderstateCb;    
    renderstateCb.cb_textured   = (rf & RenderFlags::Textured) != 0;
    renderstateCb.cb_lit        = (rf & RenderFlags::Lit) != 0;
    renderstateCb.cb_shadowed   = ShadowMaps::Inst()->IsEnabled();
    UpdateConstantBuffer(d3dcontext,m_pConstantBufferRenderState,&renderstateCb,sizeof(ConstantBufferRenderState));
    
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

//---------------------------------------------------------------------------
void TexturedShader::Begin(RenderContext* rc)
{
    m_rc = rc;    
    
    ID3D11DeviceContext*  d3dcontext = m_rc->Context();

    // depth stencil state
    ID3D11DepthStencilState* depth = m_rc->GetRenderStateCache()->GetDepthStencilState(RenderFlags::None);
    d3dcontext->OMSetDepthStencilState(depth, 0);

    // update per frame cb   
    ConstantBufferPerFrame constBuffer;  
    Matrix::Transpose(m_rc->Cam().View(),constBuffer.cb_view);
    Matrix::Transpose(m_rc->Cam().Proj(),constBuffer.cb_proj);         
    constBuffer.cb_camPosW = m_rc->Cam().CamPos(); 
    constBuffer.fog = m_rc->GlobalFog();    
    UpdateConstantBuffer(d3dcontext,m_pConstantBufferPerFrame,&constBuffer,sizeof(ConstantBufferPerFrame));


    // set input layout.
    d3dcontext->IASetInputLayout( m_pVertexLayoutMesh );

    // set texture samplers
    ID3D11SamplerState* samplers[] = 
    {        
        m_pSceneSamplerState,
        ShadowMapState()->GetSamplerState()
    };

    d3dcontext->PSSetSamplers( 0, ARRAY_SIZE(samplers), samplers);
    
    d3dcontext->GSSetShader( NULL, NULL, 0 );
    d3dcontext->VSSetShader( m_shaderSceneRenderVS, NULL, 0 );
    d3dcontext->PSSetShader( m_shaderSceneRenderPS, NULL, 0 );

    ID3D11ShaderResourceView* srv = ShadowMapState()->GetShaderResourceView();
    d3dcontext->PSSetShaderResources( 3,1, &srv );

    ID3D11Buffer* constantBuffers[] = {
        m_pConstantBufferPerFrame,
        m_pConstantBufferRenderState,
        m_pConstantBufferPerDraw,
        ShadowMaps::Inst()->GetShadowConstantBuffer()
    };
    
    d3dcontext->VSSetConstantBuffers( 0, ARRAY_SIZE(constantBuffers), constantBuffers);
    d3dcontext->PSSetConstantBuffers( 0, ARRAY_SIZE(constantBuffers), constantBuffers);    
}

// --------------------------------------------------------------------------------------------------
void TexturedShader::End()
{    
    ID3D11ShaderResourceView* depthmap[] = {NULL};
    m_rc->Context()->PSSetShaderResources( 3,1, depthmap );
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
           
    ConstantBufferPerDraw constBuff;

    Matrix::Transpose(r.WorldXform, constBuff.cb_world );
    constBuff.cb_hasDiffuseMap = 0;
    constBuff.cb_hasNormalMap = 0;
    constBuff.cb_hasSpecularMap = 0;
    constBuff.cb_lighting =  r.lighting;

    Matrix w = r.WorldXform;        
    w.M41 = w.M42 = w.M43 = 0; w.M44 = 1;
    Matrix::Invert(w,constBuff.cb_worldInvTrans);
    Matrix::Transpose(r.TextureXForm, constBuff.cb_textureTrans);
    constBuff.cb_matDiffuse     = r.diffuse;
    constBuff.cb_matEmissive    = r.emissive;
    constBuff.cb_matSpecular    = float4(r.specular.x,r.specular.y, r.specular.z, r.specPower);

    if(r.textures[TextureType::DIFFUSE])
    {
        constBuff.cb_hasDiffuseMap = 1;
        textures[0] = r.textures[TextureType::DIFFUSE]->GetView();
    }

    if(r.textures[TextureType::NORMAL])
    {
        constBuff.cb_hasNormalMap = 1;
        textures[1] = r.textures[TextureType::NORMAL]->GetView();
    }
        
    UpdateConstantBuffer(dc,m_pConstantBufferPerDraw,&constBuff, sizeof(constBuff));    
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
    dc->IASetIndexBuffer(d3dib, DXGI_FORMAT_R32_UINT, 0);
    dc->DrawIndexed(indexCount, startIndex, startVertex);
}


