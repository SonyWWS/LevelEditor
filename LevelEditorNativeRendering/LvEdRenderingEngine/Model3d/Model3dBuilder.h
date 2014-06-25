//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include <string>
#include <map>
#include <vector>
#include "../Core/WinHeaders.h"

#include "../VectorMath/V3dMath.h"

#include "../Renderer/Model.h"
#include "../Core/NonCopyable.h"


namespace LvEdEngine
{

namespace BuilderPrimitiveType
{
    enum PrimitiveType
    {
        POLYGONS,
        TRIANGLES,
        TRISTRIPS,
        TRIFANS,
    };
};

typedef enum BuilderPrimitiveType::PrimitiveType BuilderPrimitiveTypeEnum;

class Model;
class Mesh;
class Node;

  // ------------------------------------------------------------------------------------------------
struct UINT3
{
  UINT p,n,t;
};

// ------------------------------------------------------------------------------------------------
inline bool operator <(const UINT3& lhs, const UINT3& rhs)
{
  if(lhs.p == rhs.p ) 
  {
    if(lhs.n == rhs.n)
    {
      return lhs.t < rhs.t;
    }
    return lhs.n < rhs.n;
  }
  return lhs.p < rhs.p;
}

// ------------------------------------------------------------------------------------------------
class Model3dBuilder: public NonCopyable
{
public:

    struct MaterialData
    {
        std::map<std::string, std::string> material2effect;
        std::map<std::string, std::string> image2file;
        std::map<std::string, std::string> surface2image;
        std::map<std::string, std::string> sampler2surface;
    };

    struct MeshSourceData
    {
        // these arrays hold the 'source' data when constructing a ModelMesh
        std::vector<float3> pos;
        std::vector<float3> nor;
        std::vector<float2> tex;
    };

    struct MeshPolyData
    {
        // this is a mapping of p,n,t index tuples into the above 'source' arrays to their corresponding vertex index.
        // each tuple is a unique vertex.
        std::map<UINT3,UINT> vertexindices;
        UINT currentVertexIndex; // increments every time we add a unique tuple, this is the current vertex index
        

        // these hold the data from the current primitives
        BuilderPrimitiveTypeEnum primType; // type of primitives
        std::vector<UINT> vcount;
        std::vector<UINT> indices;

        // keep track of the current polylist 'features'
        bool hasPos;
        bool hasNor;
        bool hasTex;
        UINT posOffset;
        UINT norOffset;
        UINT texOffset;
        UINT stride;
    };

    struct MeshData
    {
        MeshSourceData source;
        MeshPolyData poly;
        Mesh * mesh;
    };

    MaterialData      m_material;
    MeshData          m_mesh;
    Model *  m_model;

    void Begin();
    void End();

    void Mesh_Reset();
    void Mesh_ResetSourceInfo();
    void Mesh_ResetPolyInfo();
    void Mesh_Begin(const char * name);
    void Mesh_SetPrimType(const char * name);


    // call this after setting up the MeshData structure
    // it will add all the vertex/indices to the mesh based on the poly information
    void Mesh_AddPolys();

    // call this after setting up the MeshData structure
    // it will add all the vertex/indices to the mesh based on the poly information
    void Mesh_AddTriangles();

    void Mesh_AddTriStrips();

    void Mesh_AddTriFans();

    void Mesh_End();

    // Instances
    void AddInstance(Node* node);
    Node* FindInstance(const char* name);

private:

    NodeDict m_instances;

    // add a single vertex using the index tuple. 
    // this adds a index tuble the the mapping and returns the 
    // index for this vertex.
    UINT Mesh_AddVertex(UINT p, UINT n, UINT t);

    // p1,p2,p3 are the poly indices in the current 'indices' data
    // it takes these indices to build a vertex and index
    // from the independent indices for position,normal,texcood.
    void Mesh_AddTriangle(UINT p1, UINT p2, UINT p3);

    // Calculateds tangents for all meshes if they need them.
    void CalculateTangents();


};

}; // namespace LvEdEngine

 
