//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "TerrainMap.h"
#include <vector>
#include "../../VectorMath/CollisionPrimitives.h"
#include "../../Core/Utils.h"
#include <vector>
#include <hash_set>

namespace LvEdEngine
{

class VertexBuffer;
class RenderContext;
typedef std::vector<float2> VBList;
typedef std::vector<VBList> ListOfVBList;
class DecorationMap : public TerrainMap
{
public:
    virtual const char* ClassName() const {return StaticClassName();}
    static const char* StaticClassName(){return "DecorationMap";}
    DecorationMap();
    ~DecorationMap();
    void Invoke(wchar_t* fn, const void* arg, void** retVal);

    void SetUseBillboard(bool useBillboard){ m_useBillboard = useBillboard;}
    void SetScale(float scale)  { m_scale = scale; }            
    void SetLodDistance(float lodDist) { m_lodDistance = lodDist;}
    void SetNumOfDecorators(int numOfDecorators) 
    {
        m_numOfDecoratorsPerTexel = numOfDecorators;
        m_genVB = true;        
    }   

    const VertexBuffer* GetVB(RenderContext* rc, uint32_t& vertCount);
    uint32_t GetVertexcount() const {return m_vertexcount;}
    const ListOfVBList& GetListOfVBList()
    {
        GenVBuffers();
        return m_listOfVBList;
    }
    
    float GetScale() const { return m_scale;}
    float GetLodDistance() const {return m_lodDistance;}
    bool  GetUseBillboard() const { return m_useBillboard;}
    
private:

    bool  m_useBillboard;
    bool  m_genVB;
    float m_scale;        
    int32_t m_numOfDecoratorsPerTexel; // number of decorators will be created 
                                   // for each ON texel in the mask. default is one.    
    uint32_t m_vertexcount;            // total position count equivalent to total number of decorators.
    float  m_lodDistance;
    ListOfVBList m_listOfVBList;  // list of all the decorators for each terrain patch.    
    void ReleaseResources();
    void GenVBuffers();
    void GenVBuffers(int32_t patchId);
    uint32_t ComputeTotalVertexCount();
    VertexBuffer*  m_decoDynVB; // dynamic vertex buffer used for rendering decoration maps.

    // temp to holds terrain patch ids that need to be udpated.
    std::hash_set<int32_t> m_tmpPatchSet;
    //std::hash_set<int32_t> temp;
    static int32_t s_instId;
    int32_t m_instId;
};

}
