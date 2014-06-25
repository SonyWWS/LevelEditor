//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    ShadowMapGen.cpp


****************************************************************************/
#include "ShadowMapGen.h"
#include <DxErr.h>
#include <D3DX10math.h>
#include "RenderUtil.h"
#include "RenderEnums.h"
#include "RenderState.h"
#include "Texture.h"
#include "Model.h"
#include "RenderBuffer.h"
#include "ScreenMsgPrinter.h"
#include "Lights.h"
#include "..\GobSystem\GameLevel.h"
#include "FontRenderer.h"
#include "..\LvEdUtils.h"
#include "../Core/Logger.h"

using namespace LvEdEngine;
using namespace LvEdEngine::LvEdFonts;


//---------------------------------------------------------------------------
ShadowMapGen::~ShadowMapGen()
{
    SAFE_RELEASE( m_shaderShadowMapsVS );    
    SAFE_RELEASE( m_pVertexLayoutMesh );
    SAFE_RELEASE( m_pConstantBufferPerFrame );
    SAFE_RELEASE( m_pConstantBufferPerDraw );    
    SAFE_RELEASE( m_rasterStateShadow );        
}

//---------------------------------------------------------------------------
ShadowMapGen::ShadowMapGen( ID3D11Device* device ) : 
    m_rc( NULL ),
    m_pSurface( NULL ),    
    m_shaderShadowMapsVS( NULL ),    
    m_pVertexLayoutMesh( NULL ),    
    m_pConstantBufferPerFrame( NULL ),
    m_pConstantBufferPerDraw( NULL),
    m_rasterStateShadow(NULL)
{

    // compile and create vertex shader.
    ID3DBlob* shaderShadowMapsVSBlob =  CompileShaderFromResource(L"ShadowMapGen.hlsl","VSMain","vs_4_0", NULL);
    assert(shaderShadowMapsVSBlob);
    m_shaderShadowMapsVS = CreateVertexShader(device, shaderShadowMapsVSBlob);
    assert(m_shaderShadowMapsVS);

    // create input layout
    m_pVertexLayoutMesh = CreateInputLayout(device,shaderShadowMapsVSBlob,VertexFormat::VF_P);
    assert(m_pVertexLayoutMesh);
    SAFE_RELEASE(shaderShadowMapsVSBlob);

    // create raster state.
    D3D11_RASTERIZER_DESC rsDcr;
    SecureZeroMemory( &rsDcr, sizeof(rsDcr));
    rsDcr.DepthClipEnable = TRUE;
    rsDcr.CullMode =  D3D11_CULL_BACK; //D3D11_CULL_NONE;
    rsDcr.FillMode =  D3D11_FILL_SOLID;
    rsDcr.FrontCounterClockwise = TRUE;
    rsDcr.AntialiasedLineEnable = FALSE;
    rsDcr.MultisampleEnable = FALSE;    
    rsDcr.DepthBias = 25000;
    rsDcr.SlopeScaledDepthBias = 1.0f;
    rsDcr.DepthBiasClamp = 0.f;
    HRESULT hr = device->CreateRasterizerState( &rsDcr, &m_rasterStateShadow );
    Logger::IsFailureLog(hr, L"CreateRasterizerState");
    assert(m_rasterStateShadow);


    // create constant buffers.
    D3D11_BUFFER_DESC Desc;
    Desc.Usage = D3D11_USAGE_DYNAMIC;
    Desc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
    Desc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
    Desc.MiscFlags = 0;

    m_pConstantBufferPerFrame = CreateConstantBuffer(device,sizeof(ConstantBufferShadowMapGenPerFrame));
    assert(m_pConstantBufferPerFrame);

    m_pConstantBufferPerDraw = CreateConstantBuffer(device,sizeof(ConstantBufferShadowMapGenPerDraw));
    assert(m_pConstantBufferPerFrame);      
}

//---------------------------------------------------------------------------
void ShadowMapGen::Begin(RenderContext* rc, RenderSurface* pSurface, const AABB& bounds )
{
    m_rc = rc;
    m_pSurface = pSurface;
    ID3D11DeviceContext*  dc = m_rc->Context();

    // set depth-stencil state to default.
    dc->OMSetDepthStencilState(NULL,0);
    
    ShadowMaps::Inst()->UpdateLightCamera(dc, LightingState::Inst()->ProminentDirLight(), bounds);
    ShadowMaps::Inst()->SetAndClear(dc);
        
    dc->RSSetState( m_rasterStateShadow );
    
    const Camera& lightCam = ShadowMaps::Inst()->GetCamera();    

    // update per frame cb    
    ConstantBufferShadowMapGenPerFrame constBuffer;
    Matrix::Transpose(lightCam.View() ,constBuffer.view);
    Matrix::Transpose(lightCam.Proj(),constBuffer.proj);
    UpdateConstantBuffer(dc,m_pConstantBufferPerFrame,&constBuffer,sizeof(constBuffer));
    
    // set imput layout.
    dc->IASetInputLayout( m_pVertexLayoutMesh );

    // set shaders
    dc->VSSetShader( m_shaderShadowMapsVS, NULL, 0 );
    dc->PSSetShader( NULL, NULL, 0 );
    dc->GSSetShader( NULL, NULL, 0 );

    ID3D11Buffer* constantBuffers[] = {
        m_pConstantBufferPerFrame,
        m_pConstantBufferPerDraw
    };

    // set const buffers for vertex shader.
    dc->VSSetConstantBuffers( 0, ARRAY_SIZE(constantBuffers), constantBuffers );

}

//---------------------------------------------------------------------------
void ShadowMapGen::DrawNodes(const RenderNodeList& renderNodes)
{  
    // Render the scene into the shadow map
    for(auto it = renderNodes.begin(); it != renderNodes.end(); it++)
    {        
        const RenderableNode& renderable = (*it);
        DrawRenderable(renderable);        
    }        
}

//---------------------------------------------------------------------------
void ShadowMapGen::End()
{
    ID3D11DeviceContext* dc = m_rc->Context();

    // Restore the render targets
    ID3D11RenderTargetView* rt  = m_pSurface->GetRenderTargetView();
    ID3D11DepthStencilView* dsv = m_pSurface->GetDepthStencilView();
    dc->OMSetRenderTargets(1, &rt, dsv);    
    dc->RSSetViewports( 1, &m_pSurface->GetViewPort() );
    
    m_rc = NULL;
}

//---------------------------------------------------------------------------
void ShadowMapGen::DrawRenderable(const RenderableNode& r)
{
    if (!r.GetFlag( RenderableNode::kShadowCaster ) )
        return;

    ID3D11DeviceContext* dc = m_rc->Context();

    ConstantBufferShadowMapGenPerDraw constBuffer;
    Matrix::Transpose(r.WorldXform, constBuffer.world );    
    UpdateConstantBuffer(dc,m_pConstantBufferPerDraw,&constBuffer,sizeof(constBuffer));
    
    uint32_t stride = r.mesh->vertexBuffer->GetStride();
    uint32_t offset = 0;
    uint32_t startIndex = 0;
    uint32_t startVertex = 0;
    uint32_t indexCount = r.mesh->indexBuffer->GetCount();
    ID3D11Buffer* d3dvb = r.mesh->vertexBuffer->GetBuffer();
    ID3D11Buffer* d3dib = r.mesh->indexBuffer->GetBuffer();

    dc->IASetPrimitiveTopology( (D3D11_PRIMITIVE_TOPOLOGY)r.mesh->primitiveType );
    dc->IASetVertexBuffers( 0, 1, &d3dvb, &stride, &offset );
    dc->IASetIndexBuffer(d3dib, DXGI_FORMAT_R32_UINT, 0);

    dc->DrawIndexed(indexCount, startIndex, startVertex);
}
