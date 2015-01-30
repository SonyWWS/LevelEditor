//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "DecorationMap.h"
#include "TerrainGob.h"
#include "../../Core/ImageData.h"
#include "../../Core/StringUtils.h"
#include "../../Renderer/RenderBuffer.h"
#include "../../Renderer/GpuResourceFactory.h"

namespace LvEdEngine
{
int32_t DecorationMap::s_instId = 0;

DecorationMap::DecorationMap() : m_scale(1),   
                                 m_vertexcount(),
                                 m_lodDistance(1200),
                                 m_genVB(true),
                                 m_decoDynVB(NULL),
                                 m_numOfDecoratorsPerTexel(1)

{
    s_instId++;
    m_instId = s_instId;
}

DecorationMap::~DecorationMap()
{
    ReleaseResources();
    SAFE_DELETE(m_decoDynVB);
}


 void DecorationMap::Invoke(wchar_t* fn, const void* arg, void** retVal)
 {
     TerrainMap::Invoke(fn,arg,retVal);

     if(StrUtils::Equal(fn,L"ApplyDirtyRegion"))
     {         
         bool valid = arg != NULL; 
         assert(valid);
         if(!valid) return;
         
         Bound2di box = *(Bound2di*)arg;
         ImageData* imgdata = GetMaskData();
         m_tmpPatchSet.clear();
         const TerrainGob* terrain = this->GetParent();    

         const TerrainPatchList& patches = terrain->Patches();
         if(m_listOfVBList.size() == 0 || patches.size() == 0  || imgdata == NULL) return;

         int32_t patchCell = terrain->GetPatchDim() - 1;
         int32_t numPatchX  = (terrain->GetNumCols()-1) / patchCell;
         int32_t numPatchY  = (terrain->GetNumRows()-1) / patchCell;    
         float  dimx = (float)imgdata->GetWidth() / numPatchX;
         float  dimy = (float)imgdata->GetHeight() / numPatchY;         
         m_tmpPatchSet.clear();
         for(int32_t y = box.y1; y < box.y2; y++)
         {
             for(int32_t x = box.x1; x < box.x2; x++)
             {                 
                 int32_t patchIndex = (int32_t)(y / dimy) * numPatchX + (int32_t)( x / dimx);
                 m_tmpPatchSet.insert(patchIndex);                 
             }
         }

         for(auto it = m_tmpPatchSet.begin(); it != m_tmpPatchSet.end(); it++)
         {             
             GenVBuffers(*it);
         }     
         m_vertexcount = ComputeTotalVertexCount();
     }
 }


uint32_t DecorationMap::ComputeTotalVertexCount()
{
    size_t vertcount = 0;
    for(auto it = m_listOfVBList.begin(); it != m_listOfVBList.end(); it++)
    {
        vertcount += it->size();
    }
    return (uint32_t)vertcount;
}
void DecorationMap::ReleaseResources()
{    
    for(auto it = m_listOfVBList.begin(); it != m_listOfVBList.end(); it++)
    {
       it->clear();
    }    
    m_vertexcount = 0;
}

const VertexBuffer* DecorationMap::GetVB(RenderContext* rc, uint32_t& vertCount)
{
    vertCount = 0;
    GenVBuffers();
    if(m_vertexcount == 0) return NULL;
    
    auto d3dcontext = rc->Context();
    vertCount = 0;
        

    if( m_decoDynVB == NULL || m_decoDynVB->GetCount() < m_vertexcount)

    {
        SAFE_DELETE(m_decoDynVB);
        m_decoDynVB = GpuResourceFactory::CreateVertexBuffer(NULL, VertexFormat::VF_T,
            m_vertexcount,BufferUsage::DYNAMIC); 
        assert(m_decoDynVB);        
    }

    if(!m_decoDynVB) return NULL;
    D3D11_MAPPED_SUBRESOURCE mappedResource;
    const TerrainPatchList& patchlist = this->GetParent()->GetVisiblePatches();
    
    HRESULT hr = d3dcontext->Map(m_decoDynVB->GetBuffer(), 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
    if(FAILED(hr)) return NULL;

    // update dynamic instance  buffer        
    // for each visible terrain patch.
    
    for(auto it = patchlist.begin(); it != patchlist.end(); it++)
    {
        const VBList* pos2d = &m_listOfVBList[it->patchId];
        float dist = length( it->bounds.GetCenter() - rc->Cam().CamPos());            
            if( dist > m_lodDistance || pos2d->size() == 0) continue;            
            // update instance buffer.
            
            uint8_t* destPtr =  ( (uint8_t*)mappedResource.pData + (vertCount * m_decoDynVB->GetStride()));
            CopyMemory(destPtr, &pos2d->front(), m_decoDynVB->GetStride() * pos2d->size());
            vertCount += (uint32_t)pos2d->size();
    }
    d3dcontext->Unmap(m_decoDynVB->GetBuffer(), 0);       
    return m_decoDynVB;
}

void DecorationMap::GenVBuffers(int32_t patchId)
{
       
    const TerrainGob* terrain = this->GetParent();        
    ImageData* imgdata = GetMaskData();
    int32_t patchCell = terrain->GetPatchDim() - 1;
    int32_t numPatchX  = (terrain->GetNumCols()-1) / patchCell;
    int32_t numPatchY  = (terrain->GetNumRows()-1) / patchCell;    
    float dimx = (float)imgdata->GetWidth() / numPatchX;
    float dimy = (float)imgdata->GetHeight() / numPatchY;

    Bound2di box;

    int32_t px = patchId % numPatchX;
    int32_t py = patchId / numPatchX;
    box.x1 = (int32_t) (px * dimx);
    box.x2 = box.x1 + (int32_t)dimx;
    box.y1 =  (int32_t) (py * dimy);
    box.y2 = box.y1 + (int32_t)dimy;      
        
    float ycol = (float)imgdata->GetHeight();
    float xcol = (float)imgdata->GetWidth();
    float halfu = 1.0f / ( 2.0f * xcol);
    float halfv = 1.0f / (2.0f * ycol);
        
    auto& list = m_listOfVBList[patchId];
    list.clear();    
    srand(7353 + patchId * m_instId);
    for(int32_t y = box.y1; y < box.y2; y++)
    {            
        float v = y / ycol;
        for(int32_t x = box.x1; x < box.x2; x++)
        {
            float u = x / xcol;
            uint8_t r =  *((uint8_t*)imgdata->PixelAt(x,y));
            if(r > 0)
            {                      
                for(int32_t k = 0; k < m_numOfDecoratorsPerTexel; k++)
                {
                    float nu = GetRandomFloat(u-halfu, u+halfu);
                    float nv = GetRandomFloat(v-halfv,v+halfv);                            
                    float2 posn(nu,nv);                        
                    list.push_back(posn);                          
                }                
            }
        }
    }//// for(int32_t y = box.y1; y < box.y2; y++)
}
void DecorationMap::GenVBuffers()
{
    if(m_genVB == false) return;

    ReleaseResources();

    ImageData* imgdata = GetMaskData();
    const TerrainGob* terrain = this->GetParent();                
    const auto& patchlist = terrain->Patches();

    if(!imgdata || patchlist.size() == 0) return;

    int32_t patchCell = terrain->GetPatchDim() - 1;
    int32_t numPatchX  = (terrain->GetNumCols()-1) / patchCell;
    int32_t numPatchY  = (terrain->GetNumRows()-1) / patchCell;    
    int32_t numPatches = numPatchX * numPatchY; 
    int32_t dimx = imgdata->GetWidth() / numPatchX;
    int32_t dimy = imgdata->GetHeight() / numPatchY;
            
    if(numPatches != (int32_t) patchlist.size())
    {
        Logger::Log(OutputMessageType::Error,"DecoMap: wrong number of terrain patches");
        assert(false);   
        return;
    }
     
    if(dimx < 2 || dimy < 2 ) // remX != 0 || remY != 0
    {
        Logger::Log(OutputMessageType::Error,L"%s decoration map mask have wrong dimention", this->GetName());
        assert(false);
        return;
    }

    m_vertexcount = 0;    
    m_listOfVBList.resize(numPatches);         
    for(int32_t patchId = 0; patchId < numPatches; patchId++)
    { 
        GenVBuffers(patchId);        
    }
         
    m_vertexcount = ComputeTotalVertexCount();
    m_genVB = false;    
}

}