//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <string>
#include <vector>
#include <map>
#include <D3D11.h>

#include "../Core/WinHeaders.h"
#include "../Core/NonCopyable.h"
#include "../VectorMath/V3dMath.h"
#include "../VectorMath/CollisionPrimitives.h"
#include "../Renderer/RenderEnums.h"
#include "../Renderer/Resource.h"

namespace LvEdEngine
{
    class ResourceInfo;
    class ResourceManager;
    class VertexBuffer;
    class IndexBuffer;
    class Texture;


// ------------------------------------------------------------------------------------------------
static const int MODEL_MAX_NAME=100;
static const int MODEL_MAX_VERTEX_ELEMENTS=32;
static const int MODEL_MAX_VERTEX_STREAMS=16;
static const UINT MODEL_INVALID_ID=(UINT)-1;

class Mesh;
class Material;
class Geometry;
class Node;
class Model;
class CustomDataAttribute;

typedef std::map<std::string, Mesh*> MeshDict;
typedef std::map<std::string, Material*> MaterialDict;
typedef std::map<std::string, Node*> NodeDict;
typedef std::map<std::string, Geometry*> GeometryDict;
typedef std::vector<Node*> NodeArray;
typedef std::vector<Geometry*> GeoArray;
typedef std::vector<float> FloatArary;
typedef std::map<std::string, CustomDataAttribute*> CustomDataAttributeMap;
typedef std::vector<Matrix> MatrixList;


// ------------------------------------------------------------------------------------------------
class Mesh : public NonCopyable
{
    
public:
    std::string name;
    std::vector<float3> pos;
    std::vector<float3> nor;
    std::vector<float3> tan;
    std::vector<float2> tex;
    std::vector<unsigned int> indices;
    AABB bounds;
    VertexBuffer* vertexBuffer;       // from RenderBuffer.h
    IndexBuffer* indexBuffer;         // from RenderBuffer.h
    PrimitiveTypeEnum primitiveType;
    Mesh()
    {
        primitiveType = PrimitiveType::TriangleList;
        vertexBuffer = NULL;
        indexBuffer = NULL;
        bounds = AABB(float3(-0.5f,-0.5f,-0.5f),float3(0.5f,0.5f,0.5f));
    }
    ~Mesh();    

    // compute tangents
    void ComputeBound();
    void ComputeTangents();
    void Construct(ID3D11Device* d3dDevice);

private:
    bool BoundsCheck(long index, long max);
    bool SizeCheck(size_t s1, size_t s2, const char * n1, const char * n2);
};

// ------------------------------------------------------------------------------------------------
class Material : public NonCopyable
{
public:
  std::string name;
  float4 diffuse;
  float4 ambient;
  float4 specular;
  float4 emissive;
  float power;
  std::string texNames[TextureType::MAX];
  Texture* textures[TextureType::MAX];

  Material();

  static Material * MissingMaterial();
};

// ------------------------------------------------------------------------------------------------
// NOTE: The same geometry may occur in multiple Nodes, for example when mesh instancing is used.
class Geometry : public NonCopyable
{
public:
  std::string name;
  Material * material;
  Mesh * mesh;
  Geometry()
  {
    material = NULL;
    mesh = NULL;
  }
};

// ------------------------------------------------------------------------------------------------
class Node : public NonCopyable
{
public:
    std::string name;
    Node* parent;
    NodeArray children;
    GeoArray geometries;                  // geometries may be shared among multiple nodes.
    Matrix transform;                     // local to parent transform
    uint32_t index;                       // index into matrix array
    FloatArary thresholds;                // LOD information
    CustomDataAttributeMap attributes;    // custom data attributes

    Node()
    {
        parent = NULL;
        transform.MakeIdentity();
    }
    ~Node();
};

// ------------------------------------------------------------------------------------------------
class Model : public Resource
{
public:
  //  Model(Model* model);
    Model();

    // the destructor should only get called when the resource is released
    // by the resource manager. don't delete this directly, release them instead.
    virtual ~Model();

    //IResource methods
    virtual ResourceTypeEnum GetType(){return ResourceType::Model;}

    Node * CreateNode(const std::string& name);
    Geometry * CreateGeometry(const std::string& name);
    Material * CreateMaterial(const std::string& name);
    Mesh * CreateMesh(const std::string& name);


    Node * GetNode(const std::string& name);
    Geometry * GetGeometry(const std::string& name);
    Material * GetMaterial(const std::string& name);
    Mesh * GetMesh(const std::string& name);

    void SetRoot(Node* node);
    Node* GetRoot(){return m_root;}
    void SetSourceFileName(const std::wstring& name);
    const std::wstring & GetSourceFileName() { return m_source; }
    const GeometryDict& Geometries(){return m_geometries;}
    const MaterialDict& Materials(){return m_materials;}
    const MeshDict& Meshes(){return m_meshes;}
    const NodeDict& Nodes(){return m_nodes;}


    uint32_t GetNodeCount(){return (uint32_t)m_nodes.size();}
   
    bool IsConstructed() { return m_constructed;}
    HRESULT Construct(ID3D11Device* d3dDevice, ResourceManager* manager);
    void Destroy();

    
    const AABB& GetBounds(){return m_bounds;}

    const MatrixList& AbsoluteTransforms() { return m_nodeTransforms;}
    

protected:
    void UpdateBounds();
    void ComputeAbsoluteTransforms();
    MatrixList m_nodeTransforms;
    bool m_constructed;
    std::wstring m_source;
    std::wstring m_path;

    MeshDict m_meshes;
    MaterialDict m_materials;
    GeometryDict m_geometries;
    NodeDict m_nodes;

    AABB m_bounds;
    Node* m_root;

};

}; // namespace LvEdEngine

