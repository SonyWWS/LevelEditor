//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    FontRenderer.cpp

****************************************************************************/

#include "FontRenderer.h"
#include "Font.h"

#include <assert.h>
#include "../Core/Utils.h"
#include "../Core/Logger.h"
#include "DeviceManager.h"
#include "RenderState.h"
#include "RenderUtil.h"
#include "../VectorMath/V3dMath.h"
#include "RenderContext.h"
#include "GpuResourceFactory.h"

using namespace LvEdEngine;
using namespace LvEdEngine::LvEdFonts;

// Not that these will ever change, but it makes the code a little clearer.
static const size_t kVerticesPerQuad    = 4;
static const UINT kIndexElementsPerQuad = 6;

static ID3D11RasterizerState*   CreateFontRasterState(ID3D11Device* device);
static ID3D11BlendState*        CreateTransparentBlendState(ID3D11Device* device);
static ID3D11Buffer*            CreateFontVertexBuffer(ID3D11Device* device, UINT maxBatches, UINT stride);
static ID3D11Buffer*            CreateFontIndexBuffer(ID3D11Device* device, UINT maxBatches );

//---------------------------------------------------------------------------
// Static
static void GetScreenSize( RenderContext* rc, int* pWidth, int* pHeight )
{    
    float4 vp = rc->ViewPort();

    *pWidth  = (int)vp.x;
    *pHeight = (int)vp.y;
}

//---------------------------------------------------------------------------
// Static
static float3 ScrnPtToNDC( RenderContext* rc, int x, int y, int z )
{
    float3 ndc;
    int scrnW, scrnH;

    GetScreenSize( rc, &scrnW, &scrnH );    // maybe we need to cache this...

    ndc.x = ((2.0f * ( (float)x / (float)scrnW )) - 1.0f );
    ndc.y = ( 1.0f - (2.0f * ( (float)y / (float)scrnH )) );
    ndc.z = (float)z;

    return ndc;
}


//===========================================================================
//
//
//  FontRenderer
//
//
//===========================================================================

FontRenderer* FontRenderer::s_Inst = NULL;
size_t          FontRenderer::s_maxBatchSize = 256;        // somewhat arbitrary...


//---------------------------------------------------------------------------
FontRenderer::FontRenderer()
  : m_deviceManager(NULL),
    m_vertexBuffer(NULL),
    m_indexBuffer(NULL),
    m_vertexShader(NULL),
    m_pixelShader(NULL),
    m_vertexLayout(NULL),
    m_samplerState(NULL),
    m_rasterState(NULL),
    m_blendState(NULL),
    m_requestedFont(NULL),
    m_activeFont(NULL),
    m_leftMargin(-1),
    m_targetX(0),
    m_targetY(0),
    m_currX(0),
    m_currY(0),
    m_scale(1.0f)
{
}

//---------------------------------------------------------------------------
FontRenderer::~FontRenderer()
{
    SAFE_RELEASE( m_vertexBuffer );
    SAFE_RELEASE( m_indexBuffer );
    SAFE_RELEASE( m_vertexShader );
    SAFE_RELEASE( m_pixelShader );
    SAFE_RELEASE( m_vertexLayout );
    SAFE_RELEASE( m_rasterState );
    SAFE_RELEASE( m_blendState );
    SAFE_RELEASE( m_samplerState );
}

//---------------------------------------------------------------------------
void FontRenderer::InitInstance( DeviceManager* pDeviceManager )
{
    // Don't initialize a second time...
    assert( s_Inst == NULL );
    s_Inst = new FontRenderer;
    s_Inst->Init( pDeviceManager );
}

//---------------------------------------------------------------------------
void FontRenderer::DestroyInstance( void )
{
    SAFE_DELETE( s_Inst );
}

//---------------------------------------------------------------------------
void FontRenderer::Init( DeviceManager* pDeviceManager )
{
    m_deviceManager = pDeviceManager;
    m_fontDrawOps.reserve( GetMaxBatch() );

    ID3DBlob* pVSBlob = CompileShaderFromResource(L"FontShader.hlsl", "VS","vs_4_0", NULL);
    assert(pVSBlob);

    ID3DBlob* pPSBlob = CompileShaderFromResource(L"FontShader.hlsl", "PS","ps_4_0", NULL);
    assert(pPSBlob);

    m_vertexShader = GpuResourceFactory::CreateVertexShader(pVSBlob);
    assert(m_vertexShader);

    m_pixelShader = GpuResourceFactory::CreatePixelShader(pPSBlob);
    assert(m_pixelShader);

    m_vertexLayout = GpuResourceFactory::CreateInputLayout(pVSBlob, VertexFormat::VF_PTC);
    assert(m_vertexLayout);

    m_vertexBuffer = CreateFontVertexBuffer( m_deviceManager->GetDevice(), (UINT)GetMaxBatch(), sizeof(FontTextVertex) );
    assert(m_vertexBuffer);

    m_indexBuffer = CreateFontIndexBuffer( m_deviceManager->GetDevice(), (UINT)GetMaxBatch() );
    assert(m_indexBuffer);

    m_samplerState  = CreateFontSamplerState( m_deviceManager->GetDevice() );

    m_rasterState   = CreateFontRasterState( m_deviceManager->GetDevice() );
    assert(m_rasterState);

    m_blendState    = CreateTransparentBlendState( m_deviceManager->GetDevice() );
    assert(m_blendState);
}

//-------------------------------------------------------------------------------------------------
ID3D11Buffer* CreateFontVertexBuffer(ID3D11Device* device, UINT maxBatches, UINT stride )
{
    HRESULT hr = S_OK;

    D3D11_BUFFER_DESC vbd;
    vbd.ByteWidth = ( maxBatches * kVerticesPerQuad * stride );
    vbd.Usage = D3D11_USAGE_DYNAMIC;
    vbd.BindFlags = D3D11_BIND_VERTEX_BUFFER;
    vbd.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
    vbd.MiscFlags = 0;
    vbd.StructureByteStride = 0;

    ID3D11Buffer* buff = NULL;
    hr = device->CreateBuffer(&vbd, 0, &buff);
    if (Logger::IsFailureLog(hr))
    {
        return NULL;
    }

    return buff;
}

//-------------------------------------------------------------------------------------------------
ID3D11Buffer* CreateFontIndexBuffer(ID3D11Device* device, UINT maxBatches )
{
    HRESULT hr = S_OK;
    
    std::vector<uint32_t> indices( maxBatches * kIndexElementsPerQuad );
    uint32_t  idxBufferOfs = 0;
    uint32_t  vertexBufferOfs = 0;
    for(uint32_t  i = 0; i < maxBatches; ++i)
    {
        indices[ idxBufferOfs++ ] = ( vertexBufferOfs + 0 );
        indices[ idxBufferOfs++ ] = ( vertexBufferOfs + 1 );
        indices[ idxBufferOfs++ ] = ( vertexBufferOfs + 2 );

        indices[ idxBufferOfs++ ] = ( vertexBufferOfs + 0 );
        indices[ idxBufferOfs++ ] = ( vertexBufferOfs + 2 );
        indices[ idxBufferOfs++ ] = ( vertexBufferOfs + 3 );

        vertexBufferOfs += kVerticesPerQuad;
    }

    D3D11_SUBRESOURCE_DATA indexData = {0};
    indexData.pSysMem = &indices[0];

    D3D11_BUFFER_DESC ibd;
    ibd.ByteWidth = ( maxBatches * kIndexElementsPerQuad * sizeof(indices[0]));
    ibd.Usage = D3D11_USAGE_IMMUTABLE;
    ibd.BindFlags = D3D11_BIND_INDEX_BUFFER;
    ibd.CPUAccessFlags = 0;
    ibd.MiscFlags = 0;
    ibd.StructureByteStride = 0;

    ID3D11Buffer* buff = NULL;
    hr = device->CreateBuffer(&ibd, &indexData, &buff);
    if (Logger::IsFailureLog(hr))
    {
        return NULL;
    }

    return buff;
}

//-------------------------------------------------------------------------------------------------
ID3D11RasterizerState* CreateFontRasterState(ID3D11Device* device)
{
    HRESULT hr = S_OK;
    ID3D11RasterizerState* rasterState = NULL;
    D3D11_RASTERIZER_DESC rsDcr;
    SecureZeroMemory( &rsDcr, sizeof(rsDcr));
    rsDcr.CullMode =  D3D11_CULL_NONE;
    rsDcr.FillMode =  D3D11_FILL_SOLID;
    rsDcr.FrontCounterClockwise = false;
    rsDcr.DepthClipEnable = true;
    rsDcr.AntialiasedLineEnable = false;
    rsDcr.MultisampleEnable = true;

    device->CreateRasterizerState(&rsDcr,  &rasterState);
    if (Logger::IsFailureLog(hr))
    {
        return NULL;
    }

    return rasterState;
}

//-------------------------------------------------------------------------------------------------
ID3D11BlendState* CreateTransparentBlendState(ID3D11Device* device)
{
    HRESULT hr = S_OK;

    D3D11_BLEND_DESC fontBlendDesc = {0};
    fontBlendDesc.AlphaToCoverageEnable = false;
    fontBlendDesc.IndependentBlendEnable = false;

    fontBlendDesc.RenderTarget[0].BlendEnable = true;
    fontBlendDesc.RenderTarget[0].SrcBlend              = D3D11_BLEND_SRC_ALPHA;
    fontBlendDesc.RenderTarget[0].DestBlend             = D3D11_BLEND_INV_SRC_ALPHA;
    fontBlendDesc.RenderTarget[0].BlendOp               = D3D11_BLEND_OP_ADD;
    fontBlendDesc.RenderTarget[0].SrcBlendAlpha         = D3D11_BLEND_ONE;
    fontBlendDesc.RenderTarget[0].DestBlendAlpha        = D3D11_BLEND_ZERO;
    fontBlendDesc.RenderTarget[0].BlendOpAlpha          = D3D11_BLEND_OP_ADD;
    fontBlendDesc.RenderTarget[0].RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL;

    ID3D11BlendState* bs = NULL;
    hr = device->CreateBlendState(&fontBlendDesc, &bs);
    if (Logger::IsFailureLog(hr))
    {
        return NULL;
    }

    return bs;
}

// ----------------------------------------------------------------------------------------------
ID3D11SamplerState* FontRenderer::CreateFontSamplerState(ID3D11Device* device)
{
    //
    // Create a sampler state
    //
    //  From the MSDN documentation:
    //      "If an application attempts to create a sampler-state interface with
    //       the same state as an existing interface, the same interface will be
    //       returned and the total number of unique sampler state objects will
    //       stay the same."
    //
    //  So we may get back a ID3D11SamplerState with a ref count > 1.
    //
    ID3D11SamplerState* sampler = NULL;
    D3D11_SAMPLER_DESC SamDesc;
    SamDesc.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
    SamDesc.AddressU = D3D11_TEXTURE_ADDRESS_WRAP;
    SamDesc.AddressV = D3D11_TEXTURE_ADDRESS_WRAP;
    SamDesc.AddressW = D3D11_TEXTURE_ADDRESS_WRAP;
    SamDesc.MipLODBias = 0.0f;
    SamDesc.MaxAnisotropy = 1;
    SamDesc.ComparisonFunc = D3D11_COMPARISON_ALWAYS;
    SamDesc.BorderColor[0] = SamDesc.BorderColor[1] = SamDesc.BorderColor[2] = SamDesc.BorderColor[3] = 0;
    SamDesc.MinLOD = 0;
    SamDesc.MaxLOD = D3D11_FLOAT32_MAX;
    HRESULT hr = device->CreateSamplerState( &SamDesc, &sampler );
    if (Logger::IsFailureLog(hr))
    {
        return NULL;
    }

    return sampler;
}

//---------------------------------------------------------------------------
void FontRenderer::FontDrawingBegin(  RenderContext* rc )
{
    assert( m_vertexBuffer != NULL );
    assert( m_indexBuffer != NULL );

    m_currentRC = rc;

    m_fontDrawOps.clear();

    ID3D11DeviceContext* dc = m_currentRC->Context();

    UINT viewportCount = 1;
    D3D11_VIEWPORT vp;
    dc->RSGetViewports(&viewportCount, &vp);

    UINT stride = sizeof(FontTextVertex);
    UINT offset = 0;
    dc->IASetInputLayout(m_vertexLayout);
    dc->IASetIndexBuffer(m_indexBuffer, DXGI_FORMAT_R32_UINT, 0);
    dc->IASetVertexBuffers(0, 1, &m_vertexBuffer, &stride, &offset);
    dc->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

    dc->VSSetShader( m_vertexShader, 0, 0 );
    dc->PSSetShader( m_pixelShader, 0, 0 );
    dc->PSSetSamplers( 0, 1, &m_samplerState );

    dc->RSSetState(m_rasterState);

    float blendFactor[4] = {1.0f};
    dc->OMSetBlendState(m_blendState, blendFactor, 0xffffffff);

    m_activeFont = NULL;    // needs to be set!
}

//---------------------------------------------------------------------------
void FontRenderer::FontDrawOpsSetActiveFont()
{
    ID3D11ShaderResourceView* fontAtlas[1] = {NULL};
    fontAtlas[0] = m_requestedFont->GetShaderResourceView();
    m_currentRC->Context()->PSSetShaderResources( 0, 1, fontAtlas );

    m_activeFont = m_requestedFont;
}

//---------------------------------------------------------------------------
void FontRenderer::FontDrawingEnd()
{
    m_currentRC = NULL;
}

//---------------------------------------------------------------------------
void FontRenderer::FontDrawOpsExecute( void )
{
    assert( m_activeFont != NULL );

    size_t batchStart=0;
    size_t numRemaining = m_fontDrawOps.size();
    while ( numRemaining > 0 )
    {
        size_t numToBatch = min( numRemaining, GetMaxBatch() );
        ProcessOneBatch( batchStart, numToBatch );
        batchStart += numToBatch;
        numRemaining -= numToBatch;
    }
    m_fontDrawOps.clear();
}

//---------------------------------------------------------------------------
void FontRenderer::MoveTo( int x, int y, bool bSetLeftMargin )
{
    SetX(x);
    SetY(y);
    if ( bSetLeftMargin )
    {
        SetLeftMargin(x);
    }
}

//---------------------------------------------------------------------------
void FontRenderer::DrawText( Font* aFont, const WCHAR* aString,
            int scrnTopX, int scrnTopY,
            const float4& colorRGBA, float scale )
{
    FontPrintRequest printReq;
    printReq.font       = aFont;
    printReq.m_msgIndex = m_stringBlob.Store( aString, wcslen( aString ) );
    printReq.x          = scrnTopX;
    printReq.y          = scrnTopY;
    printReq.colorRGBA = colorRGBA;
    printReq.scale      = scale;

    m_printRequests.push_back( printReq );
}

//---------------------------------------------------------------------------
void FontRenderer::DrawTextImmediate( RenderContext* rc, Font* aFont, const WCHAR* aString, int scrnTopX, int scrnTopY,
                        const float4& colorRGBA, float scale )
{
    FontPrintRequest printReq;
    printReq.colorRGBA = colorRGBA;
    printReq.font      = aFont;
    printReq.x          = scrnTopX;
    printReq.y          = scrnTopY;
    printReq.scale      = scale;

    FontDrawingBegin( rc );
    FontDrawOpsSetActiveFont();
    SendTextToScreen( printReq, aString );
    FontDrawingEnd();
}

 //---------------------------------------------------------------------------
void FontRenderer::SendTextToScreen( const FontPrintRequest& printReq, const WCHAR* msgText )
{
    if (( printReq.x != -1 ) || ( printReq.y != -1 ))
    {
        MoveTo(
            (( printReq.x != -1 ) ? printReq.x : GetX() ),
            (( printReq.y != -1 ) ? printReq.y : GetY() ),
            false );
    }
    SetFont( printReq.font );
    SetColor( printReq.colorRGBA );
    SetScale( printReq.scale );
    SetLeftMargin(-1);  // cancel it

    m_currX = GetX();
    m_currY = GetY();

    //
    // Even though the text can be stored in the printReq as a std::wstr, we receive msgText as a
    // pointer here. The caller may have the text as a WCHAR* and we don't want to force a copy
    // into the FontPrintRequest's vector in that case.
    //
    for ( const WCHAR* p = msgText; *p != 0; p++ )
    {
        ProcessChar( *p );
    }

    FontDrawOpsExecute();

    SetX( m_currX );
    SetY( m_currY );
}

//---------------------------------------------------------------------------
void FontRenderer::ProcessChar( WCHAR aChar )
{
    switch ( aChar )
    {
        case ' ' :
            m_currX += ( m_activeFont->GetMonospace() ) ?
                            m_activeFont->SizeInfo().monoSpaceCharacterWidth :
                            m_activeFont->SizeInfo().spaceCharacterWidth;
            m_currX += m_activeFont->SizeInfo().gapBetweenChars;
            break;
        case '\n' :
            m_currX = ( m_leftMargin >= 0 ) ? m_leftMargin : m_targetX;
            m_currY += m_activeFont->SizeInfo().characterRowHeight;
            break;
        default :
        {
            // The real work.
            FontDrawOp op;
            op.m_fontPgSrcRect  = m_activeFont->CharRect( aChar );
            op.m_scrnDestRect = ScreenRect( m_currX, m_currY, m_currX + op.m_fontPgSrcRect.Width(), m_currY + op.m_fontPgSrcRect.Height() );
            m_fontDrawOps.push_back( op );
            m_currX += ( m_activeFont->GetMonospace() ) ?
                            m_activeFont->SizeInfo().monoSpaceCharacterWidth :
                            op.m_fontPgSrcRect.Width();
            m_currX += m_activeFont->SizeInfo().gapBetweenChars;
            break;
        }
    }
}

//---------------------------------------------------------------------------
void FontRenderer::ProcessOneBatch( size_t startIndex, size_t numToProcess )
{
    { // We want buffAccess to go out of scope before we call DrawIndexed().
        VertexBufferAccessor buffAccess( m_currentRC->Context(), m_vertexBuffer );

        size_t destOfs = 0;
        for( unsigned int i = 0; i < numToProcess; ++i)
        {
            const FontDrawOp& op = m_fontDrawOps[startIndex+i];
            op.InitQuad( this, buffAccess.VertexData() + destOfs );
            destOfs += kVerticesPerQuad;
        }
    }

    m_currentRC->Context()->DrawIndexed(((UINT)numToProcess * kIndexElementsPerQuad ), 0, 0);
}

//---------------------------------------------------------------------------
void FontRenderer::FlushPrintRequests( RenderContext* rc )
{
    FontDrawingBegin( rc );

    for ( auto it = m_printRequests.begin(); it != m_printRequests.end(); ++it )
    {
        FontPrintRequest& scrnMsg = (*it);
        if ( scrnMsg.font != m_activeFont )
        {
            SetFont( scrnMsg.font );
            FontDrawOpsSetActiveFont();
        }
        SendTextToScreen( scrnMsg, m_stringBlob[scrnMsg.m_msgIndex] );
    }

    m_printRequests.clear();
    m_stringBlob.clear();

    FontDrawingEnd();
}


//===========================================================================
//
//
//  FontRenderer::VertexBufferAccessor
//
//
//===========================================================================

//---------------------------------------------------------------------------
// Constructor makes buffer data available in VertexData()
FontRenderer::VertexBufferAccessor::VertexBufferAccessor( ID3D11DeviceContext* context, ID3D11Buffer* vertexBuffer )
    : m_context( context ), m_vertexBuffer( vertexBuffer )
{
    D3D11_MAPPED_SUBRESOURCE subResource;
    m_context->Map(m_vertexBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &subResource);
    m_buffer = reinterpret_cast<FontTextVertex*>(subResource.pData);
}

//---------------------------------------------------------------------------
// Destructor does the necessary cleanup.
FontRenderer::VertexBufferAccessor::~VertexBufferAccessor()
{
    m_context->Unmap(m_vertexBuffer, 0);
}


//===========================================================================
//
//
//  FontRenderer::FontDrawOp
//
//
//===========================================================================

//---------------------------------------------------------------------------
void FontRenderer::FontDrawOp::InitQuad( FontRenderer* pFontWriter, FontRenderer::FontTextVertex* quad ) const
{
    RenderContext* rc = pFontWriter->m_currentRC;

    //
    // Destination quad (screen space)
    //
    quad[0].Pos = ScrnPtToNDC( rc, m_scrnDestRect.topX,  m_scrnDestRect.bottY, 0 );
    quad[1].Pos = ScrnPtToNDC( rc, m_scrnDestRect.topX,  m_scrnDestRect.topY,  0 );
    quad[2].Pos = ScrnPtToNDC( rc, m_scrnDestRect.bottX, m_scrnDestRect.topY,  0 );
    quad[3].Pos = ScrnPtToNDC( rc, m_scrnDestRect.bottX, m_scrnDestRect.bottY, 0 );

    float fontTextureWidth  = (float)pFontWriter->m_activeFont->SizeInfo().atlasTextureWidth;
    float fontTextureHeight = (float)pFontWriter->m_activeFont->SizeInfo().atlasTextureHeight;

    //
    // Source rect (texture coordinates from the font atlas)
    //
    quad[0].Tex = float2((float)m_fontPgSrcRect.topX  / fontTextureWidth, (float)m_fontPgSrcRect.bottY / fontTextureHeight);
    quad[1].Tex = float2((float)m_fontPgSrcRect.topX  / fontTextureWidth, (float)m_fontPgSrcRect.topY  / fontTextureHeight);
    quad[2].Tex = float2((float)m_fontPgSrcRect.bottX / fontTextureWidth, (float)m_fontPgSrcRect.topY  / fontTextureHeight);
    quad[3].Tex = float2((float)m_fontPgSrcRect.bottX / fontTextureWidth, (float)m_fontPgSrcRect.bottY / fontTextureHeight);

    //
    // Set the quad's colors from the current opts
    //
    quad[0].ColorRGBA = pFontWriter->m_colorRGBA;
    quad[1].ColorRGBA = pFontWriter->m_colorRGBA;
    quad[2].ColorRGBA = pFontWriter->m_colorRGBA;
    quad[3].ColorRGBA = pFontWriter->m_colorRGBA;
}
