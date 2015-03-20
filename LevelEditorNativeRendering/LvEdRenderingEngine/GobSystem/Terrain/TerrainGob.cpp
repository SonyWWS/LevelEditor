//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "TerrainGob.h"
#include <algorithm>
#include "../../Core/Utils.h"
#include "../../Core/FileUtils.h"
#include "../../Core/StringUtils.h"
#include "../../Core/ImageData.h"
#include "../../VectorMath/MeshUtil.h"
#include "../../Renderer/Texture.h"
#include "../../Renderer/ShaderLib.h"
#include "../../Renderer/TerrainShader.h"
#include "../../Renderer/RenderBuffer.h"
#include "../../Renderer/LineRenderer.h"
#include "../../VectorMath/CollisionPrimitives.h"
#include "../../Renderer/GpuResourceFactory.h"

namespace LvEdEngine
{


    static const int segs = 40;    
    static float step = TwoPi / (float)segs;
    static std::vector<float2> sincos;

    

TerrainGob::TerrainGob() :  m_hn(NULL),                            
                            m_cols(0),
                            m_rows(0),
                            m_cellSize(0),                        
                            m_patchDim(65),                         
                            m_heightMap(NULL)
{
    // create shared vertex buffer.    
    std::vector<float3> pos;
    for(int32_t z = 0; z < m_patchDim; z++)
    {
        for(int32_t x = 0; x < m_patchDim; x++)
        {
            pos.push_back(float3((float)x,0,(float)z));
        }
    }
    m_sharedVB = GpuResourceFactory::CreateVertexBuffer(&pos[0],VertexFormat::VF_P, (int32_t)pos.size());
    m_sharedVB->SetDebugName("TerrainPatch shared vertex buffer");


    // create index buffer.
    // A--------B    
    // | \      |
    // |   \    |
    // |     \  |
    // C--------D
    // each cell has two tri  ACD and ADB

       
    int32_t patchDim = m_patchDim;
    int32_t patchCell = m_patchDim - 1;    
    int32_t numPatchIndices =   patchCell * patchCell * 6;
    m_indices.resize(numPatchIndices);    
    int32_t k = 0;
    for (int32_t zc = 0; zc < patchCell; zc++)
    {
        for (int32_t xc = 0; xc < patchCell; xc++)
        {
            // front faces are CCW like OpenGL            
            m_indices[k++] = zc * patchDim + xc;            // A
            m_indices[k++] = (zc + 1) * patchDim + xc;      // C
            m_indices[k++] = (zc + 1) * patchDim + xc + 1;  // D

            m_indices[k++] = zc * patchDim + xc;            // A
            m_indices[k++] = (zc + 1) * patchDim + xc + 1;  // D            
            m_indices[k++] = zc * patchDim + xc + 1;        // B
        }
    }    
    m_sharedIB = GpuResourceFactory::CreateIndexBuffer(&m_indices[0], (uint32_t)m_indices.size());
    m_sharedIB->SetDebugName("TerrainPatch shared index buffer");

    // only init one time.
    if(sincos.size() == 0)
    {
        float theta = 0;
        for(int i = 0; i < segs; i++)
        {            
            sincos.push_back(float2( sin(theta), cos(theta)));
            theta += step;
        }
    }
}



struct DrawBrushArgs
{
    float3 posW;    
    float radius;
    float falloff;   
    float2 scale;
};
struct RayPickRetVal
{
    int8_t picked;
    int8_t pad[3];
    float3 hitpos;
    float3 norm;
    float3 nvert;
};

void TerrainGob::Invoke(wchar_t* fn, const void* arg, void** retVal)
{
    if(StrUtils::Equal(fn,L"RayPick"))
    {        
        bool valid = arg && retVal;                     
        assert(valid);
        if(!valid) return;

        static RayPickRetVal retdata;
        
        retdata.picked = RayPick( *(Ray*)arg, retdata.hitpos, retdata.norm, retdata.nvert);
        *retVal =  &retdata;
        
    }        
    else if( StrUtils::Equal(fn,L"DrawBrush"))
    {
        bool valid = arg != NULL;            
        assert(valid);
        if(!valid) return;
        DrawBrushArgs* data = (DrawBrushArgs*)arg;        

        DrawBrush(data->radius * data->scale.x
                 ,data->radius * data->scale.y
                 ,data->falloff
                 ,data->posW);

    }
    else if( StrUtils::Equal(fn,L"ApplyDirtyRegion"))
    {
        bool valid = arg != NULL;            
        assert(valid);
        if(!valid) return;
        Bound2di box = *(Bound2di*)arg;
        ApplyDirtyRegion(box);

    }    
    else if( StrUtils::Equal(fn,L"GetHeightMapInstanceId"))
    {
        static ObjectGUID hmInstId = 0;
        hmInstId = m_heightMap ? m_heightMap->GetInstanceId() : 0;
        *retVal = &hmInstId;
    }
}


void TerrainGob::ApplyDirtyRegion(const Bound2di& box)
{     
    
    bool boxValid =  ((box.x2 - box.x1) > 0) && ((box.y2 - box.y1) > 0);
  
    if(this->m_hn && boxValid)
    {              
        tempBrushdata.clear();
        m_tmpPatchSet.clear();

        int32_t patchCell = m_patchDim - 1;
        int32_t stride = (m_cols-1) / patchCell;
        
         
        for(int32_t y = box.y1; y < box.y2; y++)
        {
            for(int32_t x = box.x1; x < box.x2; x++)
            {
                const float* val = (float*) m_heightMap->PixelAt( x,y);
                tempBrushdata.push_back(*val);                   
                int32_t patchIndex = ((y-1)/patchCell)  * stride + (x-1)/patchCell;
                assert(patchIndex < m_renderableNodes.size());
                m_tmpPatchSet.insert(patchIndex);
            }
        }
      
        auto cntx = RenderContext::Inst()->Context();
        D3D11_BOX destRegion;
        destRegion.left = box.x1;
        destRegion.right = box.x2;
        destRegion.top = box.y1;
        destRegion.bottom = box.y2;
        destRegion.front = 0;
        destRegion.back = 1;
        uint32_t rowPitch = (destRegion.right - destRegion.left) * 4;
        cntx->UpdateSubresource( m_hn->GetTex(), 0, &destRegion, &tempBrushdata[0], rowPitch, 0 );
      
        uint8_t* ptr = (uint8_t*)m_heightMap->GetBufferPointer();                        
        for(auto it = m_tmpPatchSet.begin(); it != m_tmpPatchSet.end(); it++)
        {            
            TerrainPatch* patch = &m_renderableNodes[*it];
            int xEnd = patch->x + patchCell;
            int yEnd = patch->y + patchCell;
        
            AABB bound = AABB();
            
            for(int y = patch->y;  y <= yEnd; y++)
            {
                float* scanline = (float*) (ptr + y * m_heightMap->GetRowPitch());
                for(int x = patch->x; x <= xEnd; x++)
                {    
                    float3  pos(x * m_cellSize, scanline[x], y * m_cellSize);                
                    bound.Extend(pos);
                }                
            }
            patch->boundsTr = bound;
        }        
        InvalidateBounds();        
    }
}


void TerrainGob::DrawBrush(float radx, float rady, float falloff, float3 posw)
{    
    bool valid = m_heightMap && m_heightMap->GetBufferPointer();                 
    assert(valid);

    // xform posW to heightmap space.
    float3 trans(&m_world.M41);    
    float3 posH = posw - trans; 
    posH.x /= m_cellSize;
    posH.z /= m_cellSize;    
    if(posH.x < 0 || posH.x > (m_cols-1) || posH.z < 0 || posH.z > (m_rows -1))
        return;

    falloff =  clamp<float>(falloff,0.0f,1.0f);
     
    LineRenderer::Inst()->SetColor(float4(1,1,0,1));
    float offset = 0.2f;
    for(int k = 0; k < 2; k++)
    {       
        if(k == 1)
        {
            radx = radx * (1.0f - falloff);
            rady = rady * (1.0f - falloff);
        }
               
        for(int i = 0; i < segs; i++)
        {
            float2 sc0 = sincos[i];
            float2 sc1 = sincos[ (i+1)%segs];

            float x0 = radx * sc0.y + posH.x;
            float z0 = rady * sc0.x + posH.z;
            float y0 = m_heightMap->BLinearSample<float>(x0,z0) + offset;
            x0 = x0 * m_cellSize + trans.x;
            z0 = z0 * m_cellSize + trans.z;
            float3 p0(x0,y0,z0);

            float x1 = radx * sc1.y + posH.x;
            float z1 = rady * sc1.x + posH.z;
            float y1 = m_heightMap->BLinearSample<float>(x1,z1)  + offset;
            x1 = x1 * m_cellSize + trans.x;
            z1 = z1 * m_cellSize + trans.z;
            float3 p1(x1,y1,z1);
            LineRenderer::Inst()->DrawLine(p0,p1);        
        }  
     
    }    
}



TerrainGob::~TerrainGob()
{    
    SAFE_DELETE(m_heightMap);
    SAFE_DELETE(m_hn);      
    SAFE_DELETE(m_sharedVB);
    SAFE_DELETE(m_sharedIB);
    
    for(auto it = m_layerMaps.begin(); it != m_layerMaps.end(); ++it)
    {
        delete (*it);
    }
    m_layerMaps.clear();

    for(auto it = m_decorationMaps.begin(); it != m_decorationMaps.end(); ++it)
    {
        delete (*it);
    }
    m_decorationMaps.clear();
    
}

// ray/terrain intersection.
bool TerrainGob::RayPick(const Ray& rayw, float3& hitpos, float3& norm, float3& nearestVertex)
{
    if(m_renderableNodes.size() == 0) return false;

    // transform ray to terrain space.
    Ray ray = rayw;
    float3 trans(&m_world.M41);
    ray.pos = ray.pos - trans;

    float t;
    float3 p,n, nearp;
               
    m_pickPatchlist.clear();
    for(int i = 0; i < m_renderableNodes.size(); i++)
    {
        TerrainPatch* it = &m_renderableNodes[i];
        if(IntersectRayAABB(ray, it->boundsTr, &t, &p, &n))
        {
            m_pickPatchlist.push_back(it);
        }        
    }
       
    //perform  ray/tri intersection for the terrain patchs.
    bool picked = false;
    if(m_pickPatchlist.size() > 0)
    {
        int patchCell = m_patchDim - 1;
        float minT = FLT_MAX;
        uint8_t* ptr = (uint8_t*)m_heightMap->GetBufferPointer();
        
        for(auto it = m_pickPatchlist.begin(); it != m_pickPatchlist.end(); it++)
        {
            m_pickPosT.clear();
            TerrainPatch* patch = *it;
          
            int yEnd = patch->y + patchCell;
            int xEnd = patch->x + patchCell;
            for(int y = patch->y;  y <= yEnd; y++)
            {
                float* scanline = (float*) (ptr + y * m_heightMap->GetRowPitch());
                for(int x = patch->x; x <= xEnd; x++)
                {                                       
                    m_pickPosT.push_back( float3(x * m_cellSize, scanline[x], y * m_cellSize));                
                }                
            }// for (int y = .....

            bool hit = MeshIntersects(ray,&m_pickPosT[0],
                           (uint32_t)m_pickPosT.size(),
                           &m_indices[0],
                           (uint32_t)m_indices.size(),
                           true,
                           &t,
                           &p,
                           &n,
                           &nearp);
            if(hit)
            {
                picked = true;
                if( t < minT)
                {                    
                    minT = t;
                    hitpos = p + trans;
                    norm = n;
                    nearestVertex = nearp + trans;
                }
            }
        }             
    } // if ( patchList.size() > 0)
    return picked;
}

float TerrainGob::GetHeightAt(float2 posT) const
{
    assert(m_heightMap);
    float2 dim = GetSize() - m_cellSize;
    float2 uv = posT / dim;
    return m_heightMap->BLinearSampleNorm<float>(uv.x,uv.y);    
}

int32_t TerrainGob::GetPatchIdAt(float u, float v) const
{
    int32_t patchCell = (m_patchDim - 1);
    int32_t cx =  (int32_t)( float(m_cols-1) * u) / patchCell;
    int32_t cy =  (int32_t)( float(m_rows-1) * v) / patchCell;
    int32_t stride = (int32_t)ceil( (float)(m_cols-1) / patchCell);
    return (cy * stride + cx);
}

float TerrainGob::GetHeightAt(float u, float v) const
{
    assert(m_heightMap);
    if(m_heightMap)
    {
        return m_heightMap->BLinearSampleNorm<float>(u,v);
    }
    return 0.0f;
}

// virtual

void TerrainGob::Update(const FrameTime& fr, UpdateTypeEnum updateType)
{    
    bool boundDirty = m_boundsDirty;
    super::Update(fr,updateType);
    m_boundsDirty = boundDirty || m_worldBoundUpdated;
    if(m_boundsDirty)    
    {                
        if(m_renderableNodes.size() > 0)
        {
            AABB local;
            for(auto it = m_renderableNodes.begin(); it != m_renderableNodes.end(); it++)
            {
                local.Extend(it->boundsTr);
                it->bounds = it->boundsTr;
                it->bounds.Transform(m_world);
            }            
            m_localBounds = local;
        }
        else
        {
            m_localBounds = AABB(float3(-0.5f,-0.5f,-0.5f),float3(0.5f,0.5f,0.5f));
        }

        UpdateWorldAABB();
    }
    
    if(RenderContext::Inst()->LightEnvDirty)
    {
        for(auto it = m_renderableNodes.begin(); it != m_renderableNodes.end(); it++)
        {
            LightingState::Inst()->UpdateLightEnvironment(it->lighting,it->bounds);
        }
    }

}

const TerrainPatchList& TerrainGob::GetVisiblePatches( RenderContext* context)
{               
    m_visibleList.clear();
    
    const Frustum& frustum = context->Cam().GetFrustum();    
    if (IsVisible(frustum)) 
    {
        for(auto it = m_renderableNodes.begin(); it != m_renderableNodes.end(); it++)
        {
            if (TestFrustumAABB(frustum, it->bounds))
            {
                m_visibleList.push_back(*it);                
            }
        }
    }       
    return m_visibleList;       
}

void TerrainGob::SetHeightMap(wchar_t* file)
{    
    SAFE_DELETE(m_heightMap);
    if(!FileUtils::Exists(file)) return;

    m_heightMap = new ImageData();
    m_heightMap->LoadFromFile(file);
    m_heightMap->Convert(DXGI_FORMAT_R32_FLOAT);
    if(m_heightMap->GetBufferPointer() == NULL)
    {
        SAFE_DELETE(m_heightMap);   
        return;
    }

    BuildPatches();

    
}

void TerrainGob::AddLayerMap(LayerMap* map, int index)
{
    if(map)
    {
        if(index == -1)
            m_layerMaps.push_back(map);        
        else
            m_layerMaps.insert(m_layerMaps.begin() + index, map);
        map->SetParent(this);
    }
}

void TerrainGob::RemoveLayerMap(LayerMap* map)
{
    if(map)
    {                
        auto it = std::find(m_layerMaps.begin(), m_layerMaps.end(), map);
        m_layerMaps.erase(it);
        map->SetParent(NULL);
    }
}

void TerrainGob::AddDecorationMap(DecorationMap* map, int index)
{    
    if(map)
    {
        if(index == -1)
            m_decorationMaps.push_back(map);        
        else
            m_decorationMaps.insert(m_decorationMaps.begin() + index, map);
        map->SetParent(this);
    }
}

void TerrainGob::RemoveDecorationMap(DecorationMap* map)
{
    if(map)
    {                
        auto it = std::find(m_decorationMaps.begin(), m_decorationMaps.end(), map);
        m_decorationMaps.erase(it);
        map->SetParent(NULL);
    }
}


//------------------------------------------------------
// private functions
//---------------------------------------

void TerrainGob::BuildPatches()
{
    if(m_cellSize == 0  ||  m_heightMap == NULL )       
        return; 

    int32_t patchCell = m_patchDim - 1;
    if(m_patchDim < 3 || !IsPowerOf2((uint32_t)patchCell))
    {
        Logger::Log(OutputMessageType::Error,"Terrain patch size must be greater than two and power of two plus one");
        return;
    }
    if( m_heightMap->GetWidth() < m_patchDim
        || m_heightMap->GetHeight() < m_patchDim)
    {
        Logger::Log(OutputMessageType::Error,"Base height map must be greater than %f",m_patchDim);
        return;
    }
    
    
    // ensure proper height map dimention extend if needed.
    int32_t c = m_heightMap->GetWidth();
    int32_t r = m_heightMap->GetHeight();
    int32_t nc = (int32_t) ceil((c-1.0f) / (float)patchCell);
    int32_t nr = (int32_t) ceil((r-1.0f) / (float)patchCell);
    
    int32_t dc = (nc * patchCell + 1) - c;
    int32_t dr = (nr * patchCell + 1) - r;
    if( dc > 0 || dr > 0)
    {// extend height.
        m_heightMap->Extend( dc, dr);        
    }

    m_cols =  m_heightMap->GetWidth();
    m_rows =  m_heightMap->GetHeight();
    int32_t numVerts = m_cols * m_rows;

    assert( (m_heightMap->GetWidth() * m_heightMap->GetBytesPerPixel()) == m_heightMap->GetRowPitch());
    
    std::vector<float3> pos;    
    pos.reserve(numVerts);
    uint8_t* pixelptr =(uint8_t*)m_heightMap->GetBufferPointer();
    for(int32_t z = 0; z < m_rows; z++)
    {
        float* line = (float*) ( pixelptr + z * m_heightMap->GetRowPitch());
        for(int32_t x = 0; x < m_cols; x++)
        {                         
            pos.push_back(float3(x * m_cellSize,*line++,z * m_cellSize));                        
        }
    }
   
   
    D3D11_TEXTURE2D_DESC desc;
    desc.Width = m_cols;
    desc.Height = m_rows;
    desc.MipLevels = 1;
    desc.ArraySize = 1;
    desc.SampleDesc.Count = 1;
    desc.SampleDesc.Quality = 0;
    desc.Format = DXGI_FORMAT_R32_FLOAT;
    desc.Usage = D3D11_USAGE_DEFAULT;
    desc.BindFlags = D3D11_BIND_SHADER_RESOURCE;
    desc.CPUAccessFlags = 0;
    desc.MiscFlags = 0;    
    
    D3D11_SUBRESOURCE_DATA initData;
    initData.pSysMem = m_heightMap->GetBufferPointer();
    initData.SysMemPitch  = (uint32_t) m_heightMap->GetRowPitch();
    initData.SysMemSlicePitch =  initData.SysMemPitch * m_rows;
    auto device = RenderContext::Inst()->Device();

    HRESULT hr;
    ID3D11Texture2D *tex;
    hr = device->CreateTexture2D( &desc, &initData, &tex );
    assert(tex);
    
    SAFE_DELETE(m_hn);
    m_hn = new Texture(tex,true);


    // create list of terrain patches.    
    m_renderableNodes.clear();    
    int32_t patchId = 0;
    for(int32_t zp = 0; zp < (m_rows - 1 ); zp+=patchCell)
    {
        for(int32_t xp = 0; xp < (m_cols - 1); xp+=patchCell)
        {
            TerrainPatch terpatch;
            terpatch.y = zp;
            terpatch.x = xp;              
            terpatch.patchId = patchId++;            
            m_renderableNodes.push_back(terpatch);
        }
    }
   
    // compute AABB in local space.
    for(auto it = m_renderableNodes.begin(); it != m_renderableNodes.end(); it++)
    {
        int32_t yEnd =  it->y+patchCell;
        int32_t xEnd =  it->x+patchCell;
        AABB bound;
        for(int32_t y = it->y;  y <= yEnd; y++)
        {
            for(int32_t x = it->x; x <= xEnd; x++)
            {
                bound.Extend( pos[ y * m_cols + x]);
            }
        }
        it->boundsTr = bound;
    }      
    InvalidateBounds();
}

}