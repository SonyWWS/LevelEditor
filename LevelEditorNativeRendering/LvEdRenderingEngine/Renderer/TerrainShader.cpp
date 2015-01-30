//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    TerrainShader.cpp

****************************************************************************/
#include "TerrainShader.h"
#include "../GobSystem/Terrain/TerrainGob.h"
#include "../Core/Utils.h"
#include "Lights.h"
#include "Model.h"
#include "RenderBuffer.h"
#include "RenderContext.h"
#include "RenderUtil.h"
#include "Texture.h"
#include "TextureLib.h"
#include "ShapeLib.h"
#include "GpuResourceFactory.h"


using namespace LvEdEngine;

//---------------------------------------------------------------------------
TerrainShader::TerrainShader(ID3D11Device* device)
  : Shader(Shaders::TerrainShader),
    m_rc(NULL),        
    m_vertexShader(NULL),
    m_pixelShader(NULL),
    m_vertexLayout(NULL),  
    m_VSSolidWire(NULL),
    m_PSSolidWire(NULL),
    m_GSSolidWire(NULL),
    m_VSNormals(NULL),
    m_GSNormals(NULL),
    m_PSNormals(NULL),
    m_layoutSolidWire(NULL),    
    m_dpLessEqual(NULL),    
    m_VSDeco(NULL),
    m_PSDeco(NULL),
    m_VSDecoBB(NULL),
    m_GSDecoBB(NULL),
    m_vertLayoutDecoBB(NULL),
    m_vertLayoutDeco(NULL)    
{
    Initialize(device);
}

//---------------------------------------------------------------------------
TerrainShader::~TerrainShader()
{
    SAFE_RELEASE(m_vertexShader);
    SAFE_RELEASE(m_pixelShader);    
    SAFE_RELEASE(m_vertexLayout);

    // solidware frame
    SAFE_RELEASE(m_VSSolidWire);
    SAFE_RELEASE(m_PSSolidWire);
    SAFE_RELEASE(m_GSSolidWire);
    SAFE_RELEASE(m_layoutSolidWire);
    SAFE_RELEASE(m_dpLessEqual);   

    // rendering normals.
    SAFE_RELEASE(m_VSNormals);
    SAFE_RELEASE(m_GSNormals);
    SAFE_RELEASE(m_PSNormals);

    //  decomap
    SAFE_RELEASE(m_VSDeco);
    SAFE_RELEASE(m_PSDeco);
    SAFE_RELEASE(m_vertLayoutDeco);    
    SAFE_RELEASE(m_VSDecoBB);  
    SAFE_RELEASE(m_GSDecoBB);  
    SAFE_RELEASE(m_vertLayoutDecoBB);  
}

//---------------------------------------------------------------------------
void TerrainShader::Initialize(ID3D11Device* device)
{
    // load and compile shaders
    ID3DBlob* pGSBlob = NULL;
    ID3DBlob* pVSBlob = CompileShaderFromResource(L"TerrainShader.hlsl", "VSMain","vs_4_0", NULL);
    ID3DBlob* pPSBlob = CompileShaderFromResource(L"TerrainShader.hlsl", "PSMain","ps_4_0", NULL);
    assert(pVSBlob);
    assert(pPSBlob);

    // create shaders and state
    m_vertexShader = GpuResourceFactory::CreateVertexShader(pVSBlob);
    m_pixelShader = GpuResourceFactory::CreatePixelShader(pPSBlob);
    m_vertexLayout = GpuResourceFactory::CreateInputLayout(pVSBlob, VertexFormat::VF_P);
    SAFE_RELEASE(pVSBlob);
    SAFE_RELEASE(pPSBlob);    
    assert(m_vertexShader);
    assert(m_pixelShader);
    assert(m_vertexLayout);

    // create shaders for rendering solid wireframe.
    pVSBlob = CompileShaderFromResource(L"TerrainShader.hlsl", "VSSolidWire","vs_4_0", NULL);
    pGSBlob = CompileShaderFromResource(L"TerrainShader.hlsl", "GSSolidWire","gs_4_0", NULL);
    pPSBlob = CompileShaderFromResource(L"TerrainShader.hlsl", "PSSolidWire","ps_4_0", NULL);
    assert(pVSBlob && pGSBlob && pPSBlob);
    
    m_VSSolidWire     = GpuResourceFactory::CreateVertexShader(pVSBlob);
    m_GSSolidWire     = GpuResourceFactory::CreateGeometryShader(pGSBlob);
    m_PSSolidWire     = GpuResourceFactory::CreatePixelShader(pPSBlob);
    assert(m_VSSolidWire && m_GSSolidWire && m_PSSolidWire);
    SAFE_RELEASE(pVSBlob);
    SAFE_RELEASE(pGSBlob);
    SAFE_RELEASE(pPSBlob);

    // create shaders for normals rendering.
    pVSBlob = CompileShaderFromResource(L"TerrainShader.hlsl", "VSNormals","vs_4_0", NULL);
    pGSBlob = CompileShaderFromResource(L"TerrainShader.hlsl", "GSNormals","gs_4_0", NULL);
    pPSBlob = CompileShaderFromResource(L"TerrainShader.hlsl", "PSNormals","ps_4_0", NULL);
    assert(pVSBlob && pGSBlob && pPSBlob);
    
    m_VSNormals = GpuResourceFactory::CreateVertexShader(pVSBlob);
    m_GSNormals = GpuResourceFactory::CreateGeometryShader(pGSBlob);
    m_PSNormals = GpuResourceFactory::CreatePixelShader(pPSBlob);
    assert(m_VSNormals && m_GSNormals && m_PSNormals);
    SAFE_RELEASE(pVSBlob);
    SAFE_RELEASE(pGSBlob);
    SAFE_RELEASE(pPSBlob);
    

    // create shaders for rendering decoration maps
    pVSBlob = CompileShaderFromResource(L"TerrainShader.hlsl", "VSDeco","vs_4_0", NULL);    
    pPSBlob = CompileShaderFromResource(L"TerrainShader.hlsl", "PSDeco","ps_4_0", NULL);
    assert(pVSBlob && pPSBlob);
    
    // Define the input layout
    D3D11_INPUT_ELEMENT_DESC layoutpntp[] =
    {
        { "POSITION", 0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },        
        { "NORMAL",   0, DXGI_FORMAT_R32G32B32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0 },        
        { "TEXCOORD", 0, DXGI_FORMAT_R32G32_FLOAT,    0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0},
        { "INSPOS",   0, DXGI_FORMAT_R32G32_FLOAT, 1, 0,  D3D11_INPUT_PER_INSTANCE_DATA, 1}
    };
    UINT numElements = ARRAYSIZE( layoutpntp );

    // Create the input layout
    device->CreateInputLayout( layoutpntp, numElements, pVSBlob->GetBufferPointer(),
                                          pVSBlob->GetBufferSize(), &m_vertLayoutDeco );
    assert(m_vertLayoutDeco);
         
    m_VSDeco = GpuResourceFactory::CreateVertexShader(pVSBlob);    
    m_PSDeco = GpuResourceFactory::CreatePixelShader(pPSBlob);
    assert(m_VSDeco && m_PSDeco);
    SAFE_RELEASE(pVSBlob);    
    SAFE_RELEASE(pPSBlob);

    // decoratin rendering using billboards    
    pVSBlob = CompileShaderFromResource(L"TerrainShader.hlsl", "VSDecoBB","vs_4_0", NULL);
    pGSBlob = CompileShaderFromResource(L"TerrainShader.hlsl", "GSDecoBB","gs_4_0", NULL);    
    assert(pVSBlob && pGSBlob);

    m_VSDecoBB = GpuResourceFactory::CreateVertexShader(pVSBlob);
    m_GSDecoBB = GpuResourceFactory::CreateGeometryShader(pGSBlob);
    assert(m_VSDecoBB && m_GSDecoBB);


    // Define the input layout
    D3D11_INPUT_ELEMENT_DESC layoutT[] =
    {
        { "POSITION", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 }        
    };
    UINT numElementsT = ARRAYSIZE( layoutT );

    // Create the input layout
    HRESULT hr = device->CreateInputLayout( layoutT, numElementsT, pVSBlob->GetBufferPointer(),
                                          pVSBlob->GetBufferSize(), &m_vertLayoutDecoBB );
    Logger::IsFailureLog(hr,L"Create input layout for decorationmap");

    assert(m_vertLayoutDecoBB);
    SAFE_RELEASE(pVSBlob);
    SAFE_RELEASE(pGSBlob);
    

    // create constant buffers
    m_perFrameCb.Construct(device);
    m_perTerrainCb.Construct(device);
    m_renderStateCb.Construct(device);
    m_perPatchCb.Construct(device);
    m_perDecomapCb.Construct(device);


    //m_hnSampler  point clamp

    
    // create depth state for rendering wireframe overlay.
    D3D11_DEPTH_STENCIL_DESC depthDescr = RSCache::Inst()->GetDefaultDpDcr();
    depthDescr.DepthFunc = D3D11_COMPARISON_LESS_EQUAL;
    device->CreateDepthStencilState(&depthDescr,&m_dpLessEqual);
    assert(m_dpLessEqual);  
}

//---------------------------------------------------------------------------
void TerrainShader::SetRenderFlag(RenderFlagsEnum rf)
{    
    ID3D11DeviceContext*  d3dcontext = m_rc->Context();
    m_renderStateCb.Data.textured   = (rf & RenderFlags::Textured) != 0;
    m_renderStateCb.Data.lit        = (rf & RenderFlags::Lit) != 0;
    m_renderStateCb.Data.shadowed   =  ShadowMaps::Inst()->IsEnabled();
    m_renderStateCb.Update(d3dcontext);
    

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
void TerrainShader::Begin(RenderContext* rc)
{
    m_rc = rc;    
    
    ID3D11DeviceContext*  d3dcontext = m_rc->Context();
        
    // update per frame CB    
    Matrix::Transpose(rc->Cam().View(),m_perFrameCb.Data.view);
    Matrix::Transpose(rc->Cam().Proj(),m_perFrameCb.Data.proj);                
    m_perFrameCb.Data.camPosW = rc->Cam().CamPos();
    m_perFrameCb.Data.viewport = rc->ViewPort();
    m_perFrameCb.Update(d3dcontext);
        
    ID3D11SamplerState* samplers[] = 
    {           
        RSCache::Inst()->PointClamp(),
        ShadowMaps::Inst()->GetSamplerState(),
        RSCache::Inst()->LinearWrap(),
        RSCache::Inst()->LinearClamp(),
        RSCache::Inst()->AnisotropicWrap()
    };

    // set vertex shader samplers
    d3dcontext->VSSetSamplers( 0, ARRAY_SIZE(samplers), samplers);
    d3dcontext->PSSetSamplers( 0, ARRAY_SIZE(samplers), samplers);
    
    
    // set shaders
    d3dcontext->GSSetShader(NULL, NULL, 0);
    d3dcontext->VSSetShader(m_vertexShader, NULL, 0);
    d3dcontext->PSSetShader(m_pixelShader, NULL, 0);

    // set shadow map.
    ID3D11ShaderResourceView* srv = ShadowMaps::Inst()->GetShaderResourceView();
    d3dcontext->PSSetShaderResources(0, 1, &srv);

    // set constant buffers
    ID3D11Buffer* constantBuffers[] = 
    {
        m_perFrameCb.GetBuffer(),
        m_renderStateCb.GetBuffer(),
        m_perTerrainCb.GetBuffer(),
        m_perPatchCb.GetBuffer(),
        ShadowMaps::Inst()->GetShadowConstantBuffer(),
        m_perDecomapCb.GetBuffer()
    };

    d3dcontext->VSSetConstantBuffers(0, ARRAY_SIZE(constantBuffers), constantBuffers);
    d3dcontext->PSSetConstantBuffers(0, ARRAY_SIZE(constantBuffers), constantBuffers);    
    d3dcontext->GSSetConstantBuffers(0, ARRAY_SIZE(constantBuffers), constantBuffers);    
    
}

void TerrainShader::RenderTerrain(TerrainGob* terrain)
{       
    assert(m_rc);
    ID3D11DeviceContext* d3dcontext = m_rc->Context();
      
    const TerrainPatchList& patches = terrain->GetVisiblePatches(m_rc);
    const LayerMapList& layermaps = terrain->GetLayerMaps();
    if(patches.size() == 0) return;

    // compute render flag.
    uint32_t gflags = (uint32_t)m_rc->State()->GetGlobalRenderFlags();
    uint32_t flags = 0;
    flags =  (gflags & GlobalRenderFlags::Textured) ? RenderFlags::Textured : 0;
    flags |= (gflags & GlobalRenderFlags::Lit) ? RenderFlags::Lit : 0;
    flags |= (gflags & GlobalRenderFlags::RenderBackFace) ? RenderFlags::RenderBackFace : 0;
    
    bool selected = ( m_rc->selection.find( terrain->GetInstanceId() ) != m_rc->selection.end() );    
    bool wireframe = selected || (gflags & GlobalRenderFlags::WireFrame);
              
    Matrix world = terrain->GetWorldTransform();
    m_perTerrainCb.Data.terrainTrans = float3(&world.M41);
    m_perTerrainCb.Data.cellSize = terrain->GetCellSize();
    m_perTerrainCb.Data.wirecolor =  selected ? m_rc->State()->GetSelectionColor() : float4(0,1,1,1);    
    m_perTerrainCb.Data.fog = m_rc->GlobalFog();    
    m_perTerrainCb.Data.hmSize.x = (float) terrain->GetNumCols();
    m_perTerrainCb.Data.hmSize.y = (float) terrain->GetNumRows();
    m_perTerrainCb.Data.hmTexelsize.x = 1.0f / (float) terrain->GetNumCols();
    m_perTerrainCb.Data.hmTexelsize.y = 1.0f / (float) terrain->GetNumRows();
    
               
    // set hight map views
    ID3D11ShaderResourceView* view[] = {terrain->GetVTex()->GetView()};
    d3dcontext->VSSetShaderResources(1,1,view);
    d3dcontext->PSSetShaderResources(1,1,view);

    // mask bump, and diffuse  resource views
    ID3D11ShaderResourceView* layervews[MaxNumLayers ];
    ID3D11ShaderResourceView* maskvews[MaxNumLayers ];
    ID3D11ShaderResourceView* bumpmapviews[MaxNumLayers ];
    
    int k = 0;
    for(int i = 0; i < layermaps.size(); i++)
    {
        LayerMap* layermap = layermaps[i];
        if(!layermap->IsVisible()) continue;

        m_perTerrainCb.Data.layerTexScale[k] = float4(layermap->GetTextureScale(),0,0,0);
        layervews[k] = layermap->GetDiffuse()->GetView();
        maskvews[k] = layermap->GetMask()->GetView();
        bumpmapviews[k] = layermap->GetNormal()->GetView();
        k++;
    }
    int32_t numlayers = k;
    m_perTerrainCb.Data.numLayers = (float) min(MaxNumLayers, numlayers);
    m_perTerrainCb.Update(d3dcontext);
    
    // replace mask of the first layer to full-mask.
    maskvews[0]  = TextureLib::Inst()->GetDefault(TextureType::FullMask)->GetView();

    d3dcontext->PSSetShaderResources(2, numlayers,layervews);
    d3dcontext->PSSetShaderResources(2+ MaxNumLayers, numlayers,maskvews);
    d3dcontext->PSSetShaderResources(2+ MaxNumLayers + MaxNumLayers, numlayers,bumpmapviews);

    d3dcontext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);    
    // set input layout.
    d3dcontext->IASetInputLayout(m_vertexLayout);
   
    // render patches.
    const VertexBuffer* vb = terrain->SharedVB();
    const IndexBuffer* ib = terrain->SharedIB();
    uint32_t stride = vb->GetStride();
    uint32_t offset = 0;
    uint32_t startIndex = 0;
    uint32_t startVertex= 0;
    uint32_t indexCount = ib->GetCount();
    ID3D11Buffer* d3dvb = vb->GetBuffer();
    ID3D11Buffer* d3dib = ib->GetBuffer();
    
    d3dcontext->IASetVertexBuffers(0, 1, &d3dvb, &stride, &offset);
    d3dcontext->IASetIndexBuffer(d3dib, (DXGI_FORMAT)ib->GetFormat(), 0);

    for(int i = 0; i < 3; i++)
    {
        if(gflags & GlobalRenderFlags::Solid) 
        {            
            // render solid.
            d3dcontext->GSSetShader(NULL, NULL, 0);
            d3dcontext->VSSetShader(m_vertexShader, NULL, 0);
            d3dcontext->PSSetShader(m_pixelShader, NULL, 0);
            SetRenderFlag((RenderFlagsEnum)flags);
            gflags &=  ~(GlobalRenderFlags::Solid);
        }
        else if( wireframe)
        {// render wireframe.            
            d3dcontext->VSSetShader(m_VSSolidWire, NULL, 0);
            d3dcontext->GSSetShader(m_GSSolidWire, NULL, 0);
            d3dcontext->PSSetShader(m_PSSolidWire, NULL, 0);
            uint32_t flagalpha = RenderFlags::AlphaBlend;            
            SetRenderFlag((RenderFlagsEnum)flagalpha);
            d3dcontext->OMSetDepthStencilState(m_dpLessEqual,0);
            wireframe = false;
        }
        else if( gflags & GlobalRenderFlags::RenderNormals)
        {            
            d3dcontext->VSSetShader(m_VSNormals, NULL, 0);
            d3dcontext->GSSetShader(m_GSNormals, NULL, 0);
            d3dcontext->PSSetShader(m_PSNormals, NULL, 0);
            d3dcontext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_POINTLIST);    
            SetRenderFlag((RenderFlagsEnum)flags);
            gflags &=  ~(GlobalRenderFlags::RenderNormals);
        }
        else
        {
            continue;
        }
           
        
        for(auto it = patches.begin(); it != patches.end(); it++)
        {
             m_perPatchCb.Data.lightEnv = it->lighting;
             m_perPatchCb.Data.patchTrans = float3((float)it->x,0,(float)it->y);
             m_perPatchCb.Update(d3dcontext);             
             d3dcontext->DrawIndexed(indexCount, startIndex, startVertex);
        }
    }
    
    
    d3dcontext->PSSetShader(m_PSDeco, NULL, 0);
    
    Mesh* decomesh = ShapeLibGetMesh(RenderShape::AsteriskQuads);
    const DecorationMapList& decolist = terrain->GetDecorationMaps();
    
    UINT strides[2] = {decomesh->vertexBuffer->GetStride(), sizeof(float2)};
    UINT offsets[2] = {0,0};
    ID3D11Buffer* vbs[2] = {decomesh->vertexBuffer->GetBuffer(), NULL};

    d3dcontext->IASetIndexBuffer(decomesh->indexBuffer->GetBuffer(), (DXGI_FORMAT)decomesh->indexBuffer->GetFormat(), 0);
    flags |= RenderFlags::RenderBackFace;
    SetRenderFlag((RenderFlagsEnum)flags);
     // set blend state 
    auto blendState = RSCache::Inst()->GetAlphaToCoverageState();
    float blendFactor[4] = {1.0f};
    d3dcontext->OMSetBlendState(blendState, blendFactor, 0xffffffff);        
    
    for(auto it = decolist.begin(); it != decolist.end(); it++)
    {   
        DecorationMap* map = *it;
        if(!map->IsVisible()) continue;

        uint32_t vertCount = 0;
        const VertexBuffer* dynvb = map->GetVB(m_rc,vertCount);
        if( vertCount == 0)  continue;
                        
        const Texture* diffuse = map->GetDiffuse();
        D3D11_TEXTURE2D_DESC desc;
        diffuse->GetTex()->GetDesc(&desc);
        
        float h =(float) desc.Height;
        float w =(float) desc.Width;
        float3 scale =  (w >= h) ? float3(1,h/w,1) : float3(w/h,1,w/h);
        m_perDecomapCb.Data = scale * map->GetScale();                       
        m_perDecomapCb.Update(d3dcontext);
                        
        layervews[0] = diffuse->GetView();
        bumpmapviews[0] = map->GetNormal()->GetView();
        d3dcontext->PSSetShaderResources(2,1,layervews);        
        d3dcontext->PSSetShaderResources(2+ MaxNumLayers + MaxNumLayers, 1,bumpmapviews);
        
        if(map->GetUseBillboard())
        {            
            d3dcontext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_POINTLIST);        
            d3dcontext->IASetInputLayout(m_vertLayoutDecoBB);
            d3dcontext->VSSetShader(m_VSDecoBB, NULL, 0);
            d3dcontext->GSSetShader(m_GSDecoBB, NULL, 0);
            vbs[0] = dynvb->GetBuffer();
            strides[0] = dynvb->GetStride();
            d3dcontext->IASetVertexBuffers(0, 1, vbs, strides, offsets);
            d3dcontext->Draw(vertCount,0);
        }
        else
        {
            d3dcontext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);        
            d3dcontext->IASetInputLayout(m_vertLayoutDeco);
            d3dcontext->VSSetShader(m_VSDeco, NULL, 0);
            d3dcontext->GSSetShader(NULL, NULL, 0);
            vbs[1] = dynvb->GetBuffer();
            d3dcontext->IASetVertexBuffers(0, 2, vbs, strides, offsets);
            d3dcontext->DrawIndexedInstanced(decomesh->indexBuffer->GetCount(), vertCount, 0, 0, 0);		
        }
    }
}

// --------------------------------------------------------------------------------------------------
void TerrainShader::End()
{
    ID3D11DeviceContext*  d3dcontext = m_rc->Context();

    ID3D11ShaderResourceView* nv[] = {NULL, NULL, NULL, NULL, NULL, NULL, NULL,NULL,NULL,NULL};
    d3dcontext->PSSetShaderResources(0, 10, nv);         
    d3dcontext->VSSetShaderResources(0, 10, nv);         
    d3dcontext->GSSetShader(NULL, NULL, 0);
    d3dcontext->VSSetShader(NULL, NULL, 0);
    d3dcontext->PSSetShader(NULL, NULL, 0);
    m_rc = NULL;
}

//---------------------------------------------------------------------------
void TerrainShader::DrawNodes(const RenderNodeList& /*renderNodes*/)
{
    // use RenderTerrain() instead of this function.
    assert(0);   
}
