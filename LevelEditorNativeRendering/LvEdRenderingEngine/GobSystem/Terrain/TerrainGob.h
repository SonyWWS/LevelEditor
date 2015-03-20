//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include "../GameObject.h"
#include "LayerMap.h"
#include "DecorationMap.h"
#include <vector>
#include <hash_set>
namespace LvEdEngine
{  


class LayerMap;    
class DecorationMap;
class ImageData;
class TerrainShader;

class TerrainPatch
{
public:
        
    int32_t patchId;
    int32_t  x,y;  // patch address.        
    AABB boundsTr;  // bounds in terrain space.
    AABB bounds; // bound in world space.    
    LightEnvironment lighting;
};

typedef std::vector<TerrainPatch> TerrainPatchList;
typedef std::vector<LayerMap*> LayerMapList;
typedef std::vector<DecorationMap*> DecorationMapList;
//typedef std::hash_set<int32_t> PatchSet;
class TerrainGob : public GameObject
{
public:
     virtual const char* ClassName() const {return StaticClassName();}
     static const char* StaticClassName(){return "TerrainGob";}
     
     TerrainGob();
     ~TerrainGob();
     
     void Invoke(wchar_t* fn, const void* arg, void** retVal);

     
     void SetCellSize(float cellSize) 
     { 
         assert(cellSize > 0);
         m_cellSize = cellSize;
         BuildPatches();
     } 
             
     void SetHeightMap(wchar_t* file);     
     void AddLayerMap(LayerMap* map, int index);
     void RemoveLayerMap(LayerMap* map);
     void AddDecorationMap(DecorationMap* map, int index);
     void RemoveDecorationMap(DecorationMap* map);
     
     // overrides     
     virtual void Update(const FrameTime& fr, UpdateTypeEnum updateType);
         
     bool RayPick(const Ray& rayw, float3& hitpos, float3& norm, float3& nearestVertex);
     float GetHeightAt(float u, float v) const;     
     float TerrainGob::GetHeightAt(float2 posT) const;
     int32_t GetPatchIdAt(float u, float v) const;
     int32_t GetNumOfPatches() const {return (int)m_renderableNodes.size();}

    // functions used by TerrainShader
    const Texture* GetVTex() const { return m_hn;}
    const VertexBuffer* SharedVB() const { return m_sharedVB;}
    const IndexBuffer* SharedIB() const { return m_sharedIB;}
    const TerrainPatchList& GetVisiblePatches( RenderContext* context);
    const TerrainPatchList& GetVisiblePatches() const {return m_visibleList;}
    const LayerMapList& GetLayerMaps() const { return m_layerMaps;}
    const DecorationMapList& GetDecorationMaps() const {return m_decorationMaps;}
    const TerrainPatchList& Patches() const { return m_renderableNodes; }
    float GetCellSize() const {return m_cellSize;}
    float2 GetSize() const { return float2(m_cols * m_cellSize, m_rows * m_cellSize); }    
    int32_t  GetNumCols() const {return m_cols;}
    int32_t  GetNumRows() const {return m_rows;}
    int32_t  GetPatchDim() const {return m_patchDim;}
    
private:    
    typedef GameObject super;

    int32_t m_cols;
    int32_t m_rows;
    int32_t m_patchDim;
    float m_cellSize;
    
    // resources    
    ImageData* m_heightMap;
    LayerMapList m_layerMaps;
    DecorationMapList m_decorationMaps;
    

    // rendering 
    VertexBuffer* m_sharedVB;       // shared vertex buffer.
    IndexBuffer*  m_sharedIB;       // shared index buffer.
    std::vector<uint32_t> m_indices;
    Texture* m_hn; // stores height.    
    TerrainPatchList m_renderableNodes;
    TerrainPatchList m_visibleList; // visible list of renderable node.
    void BuildPatches();

    // scratch lists used by RayPick(..) function.
    std::vector<TerrainPatch*> m_pickPatchlist;
    std::vector<float3> m_pickPosT;

    std::vector<float> tempBrushdata;
    std::hash_set<int32_t> m_tmpPatchSet;
    
    // Apply dirty region to heightmap texture.
    void ApplyDirtyRegion(const Bound2di& box);

    // draw brush 
    // img  brush to draw.
    // posw   position of the brush in world.
    // falloff brush fall off.
    void DrawBrush(float radx, float rady, float falloff, float3 posw);

};

}