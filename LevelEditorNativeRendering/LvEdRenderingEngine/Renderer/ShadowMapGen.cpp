//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    ShadowMapGen.cpp


****************************************************************************/
#include "ShadowMapGen.h"
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
#include "GpuResourceFactory.h"

using namespace LvEdEngine;
using namespace LvEdEngine::LvEdFonts;


//---------------------------------------------------------------------------
ShadowMapGen::~ShadowMapGen()
{
    SAFE_RELEASE( m_vertexShader);    
    SAFE_RELEASE( m_layoutP );    
    SAFE_RELEASE( m_rasterState );        
}

//---------------------------------------------------------------------------
ShadowMapGen::ShadowMapGen( ID3D11Device* device ) : 
    m_rc( NULL ),
    m_pSurface( NULL ),    
    m_vertexShader( NULL ),    
    m_layoutP( NULL ),        
    m_rasterState(NULL)
{

    //GpuResourceFactory
    // compile and create vertex shader.
    ID3DBlob* vsBlob =  CompileShaderFromResource(L"ShadowMapGen.hlsl","VSMain","vs_4_0", NULL);
    assert(vsBlob);
    m_vertexShader = GpuResourceFactory::CreateVertexShader(vsBlob);
    assert(m_vertexShader);

    // create input layout
    m_layoutP = GpuResourceFactory::CreateInputLayout(vsBlob, VertexFormat::VF_P);
    assert(m_layoutP);
    vsBlob->Release();
    
    // create raster state.
    D3D11_RASTERIZER_DESC rsDcr = RSCache::Inst()->GetDefaultRsDcr();       
    rsDcr.AntialiasedLineEnable = FALSE;
    rsDcr.MultisampleEnable = FALSE;    
    rsDcr.DepthBias = 25000;
    rsDcr.SlopeScaledDepthBias = 1.0f;
    rsDcr.DepthBiasClamp = 0.f;

    HRESULT hr = device->CreateRasterizerState( &rsDcr, &m_rasterState );
    Logger::IsFailureLog(hr, L"CreateRasterizerState");
    assert(m_rasterState);

    // create constant buffers.    
    m_cbPerFrame.Construct(device);
    m_cbPerDraw.Construct(device);    
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
        
    dc->RSSetState( m_rasterState );
    
    const Camera& lightCam = ShadowMaps::Inst()->GetCamera();    

    // update per frame cb         
    Matrix::Transpose(lightCam.View() ,m_cbPerFrame.Data.view);
    Matrix::Transpose(lightCam.Proj(),m_cbPerFrame.Data.proj);
    m_cbPerFrame.Update(dc);
    
    // set imput layout.
    dc->IASetInputLayout( m_layoutP );

    // set shaders
    dc->VSSetShader( m_vertexShader, NULL, 0 );
    dc->PSSetShader( NULL, NULL, 0 );
    dc->GSSetShader( NULL, NULL, 0 );

    ID3D11Buffer* constantBuffers[] = {
        m_cbPerFrame.GetBuffer(),
        m_cbPerDraw.GetBuffer() };

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
    m_pSurface = NULL;
}

//---------------------------------------------------------------------------
void ShadowMapGen::DrawRenderable(const RenderableNode& r)
{
    if (!r.GetFlag( RenderableNode::kShadowCaster ) )
        return;

    ID3D11DeviceContext* dc = m_rc->Context();
        
    Matrix::Transpose(r.WorldXform, m_cbPerDraw.Data);    
    m_cbPerDraw.Update(dc);
        
    uint32_t stride = r.mesh->vertexBuffer->GetStride();
    uint32_t offset = 0;
    uint32_t startIndex = 0;
    uint32_t startVertex = 0;
    uint32_t indexCount = r.mesh->indexBuffer->GetCount();
    ID3D11Buffer* d3dvb = r.mesh->vertexBuffer->GetBuffer();
    ID3D11Buffer* d3dib = r.mesh->indexBuffer->GetBuffer();

    dc->IASetPrimitiveTopology( (D3D11_PRIMITIVE_TOPOLOGY)r.mesh->primitiveType );
    dc->IASetVertexBuffers( 0, 1, &d3dvb, &stride, &offset );
    dc->IASetIndexBuffer(d3dib, (DXGI_FORMAT) r.mesh->indexBuffer->GetFormat(), 0);

    dc->DrawIndexed(indexCount, startIndex, startVertex);
}
