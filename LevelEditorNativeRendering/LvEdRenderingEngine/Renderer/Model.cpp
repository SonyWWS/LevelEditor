//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "Model.h"

#include "../Core/Utils.h"
#include "../Core/Logger.h"
#include "RenderBuffer.h"
#include "Texture.h"
#include "RenderUtil.h"
#include "TextureLib.h"
#include "CustomDataAttribute.h"
#include "../ResourceManager/ResourceManager.h"
#include "GpuResourceFactory.h"

namespace LvEdEngine
{

void Mesh::ComputeTangents()
{
    int triangleCount = (int)indices.size();
    int vertexCount = (int)pos.size();
    if (triangleCount == 0 || vertexCount == 0 || nor.size() == 0)
    {
        return;
    }

    if (!SizeCheck(tex.size(), vertexCount, "tex", "pos") || !SizeCheck(nor.size(), vertexCount, "nor", "pos"))
    {
        return;
    }

    std::vector<float3> tan1;
    tan1.resize(vertexCount);
    tan.clear();
    tan.resize(vertexCount);

    for (long a = 0; a + 2 < triangleCount; a += 3)
    {
        long i1 = indices[a + 0];
        long i2 = indices[a + 1];
        long i3 = indices[a + 2];

        if (!BoundsCheck(i1, vertexCount) || !BoundsCheck(i2, vertexCount) || !BoundsCheck(i3, vertexCount))
        {
            return;
        }

        float3 v1 = pos[i1];
        float3 v2 = pos[i2];
        float3 v3 = pos[i3];

        float2 w1 = tex[i1];
        float2 w2 = tex[i2];
        float2 w3 = tex[i3];

        // position differences
        float x1 = v2.x - v1.x;
        float x2 = v3.x - v1.x;
        float y1 = v2.y - v1.y;
        float y2 = v3.y - v1.y;
        float z1 = v2.z - v1.z;
        float z2 = v3.z - v1.z;

        // texcoord differences
        float s1 = w2.x - w1.x;
        float s2 = w3.x - w1.x;
        float t1 = w2.y - w1.y;
        float t2 = w3.y - w1.y;

        // ratio
        float r = 1.0f / (s1 * t2 - s2 * t1);

        // vector in the s direction ( tangent )
        float3 sdir = float3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);

        //accumulate tangents for each triangle
        tan1[i1] += sdir;
        tan1[i2] += sdir;
        tan1[i3] += sdir;
    }

    // loop through and ortho normalize the tangents
    for (long a = 0; a < vertexCount; ++a)
    {
        float3 n = nor[a];
        float3 t = tan1[a];
        OrthoNormalize(&n,&t);
        tan[a].x = t.x;
        tan[a].y = t.y;
        tan[a].z = t.z;
    }
}

void Mesh::Construct(ID3D11Device* d3dDevice)
{
    if (pos.size() == 0)
    {
        return;
    }

    // validate sizes are the same
    if (!SizeCheck(nor.size(), pos.size(), "nor", "pos") || !SizeCheck(tex.size(), pos.size(), "tex", "pos"))
    {
        return;
    }

    // if tangents exist, then validate they are the same size as pos
    if (!SizeCheck(tan.size(), pos.size(), "tan", "pos"))
    {
        return;
    }

    SAFE_DELETE(vertexBuffer);
    SAFE_DELETE(indexBuffer);

    // create index buffer
    if(indices.size() > 0)
    {
        indexBuffer = GpuResourceFactory::CreateIndexBuffer(&indices[0], (uint32_t)indices.size() );
        indexBuffer->SetDebugName(name.c_str());
    }

    if(tan.size() > 0)
    {
        std::vector<VertexPNTT> verts;
        verts.reserve(pos.size());
        for(unsigned int i = 0; i < pos.size(); ++i)
        {
            VertexPNTT vert;
            vert.Position = pos[i];
            vert.Normal = nor[i];
            vert.Tangent = tan[i];
            vert.Tex = tex[i];
            verts.push_back(vert);
        }
        vertexBuffer = GpuResourceFactory::CreateVertexBuffer(&verts[0], VertexFormat::VF_PNTT, (uint32_t)verts.size());
    }
    else if (tex.size() > 0)
    {
        std::vector<VertexPNT> verts;
        verts.reserve(pos.size());
        for(unsigned int i = 0; i < pos.size(); ++i)
        {
            VertexPNT vert;
            vert.Position = pos[i];
            vert.Normal = nor[i];
            vert.Tex = tex[i];
            verts.push_back(vert);
        }
        vertexBuffer = GpuResourceFactory::CreateVertexBuffer(&verts[0],VertexFormat::VF_PNT, (uint32_t)verts.size());
    }
    else if (nor.size() > 0)
    {
        std::vector<VertexPN> verts;
        verts.reserve(pos.size());
        for(unsigned int i = 0; i < pos.size(); ++i)
        {
            VertexPN vert;
            vert.Position = pos[i];
            vert.Normal = nor[i];
            verts.push_back(vert);
        }
        vertexBuffer = GpuResourceFactory::CreateVertexBuffer(&verts[0],VertexFormat::VF_PN, (uint32_t)verts.size());
    }
    else
    {
        vertexBuffer = GpuResourceFactory::CreateVertexBuffer(&pos[0],VertexFormat::VF_P, (uint32_t)pos.size());        
    }

    FreeVectorMemory(tex);
    FreeVectorMemory(tan);    
}

void Mesh::ComputeBound()
{
     // update bounds
    if(pos.size() > 0)
    {
        float3 first = pos[0];
        float3 vmin(first);
        float3 vmax(first);
        for(auto it = pos.begin(); it != pos.end(); ++it)
        {
            float3 pos = (*it);
            vmin = minimize(vmin, pos);
            vmax = maximize(vmax, pos);
        }
        AABB aabb(vmin, vmax);
        bounds = aabb;
    }
}

// ------------------------------------------------------------------------------------------------
Mesh::~Mesh()
{
    SAFE_DELETE(vertexBuffer);
    SAFE_DELETE(indexBuffer);
}

// ------------------------------------------------------------------------------------------------
bool Mesh::BoundsCheck(long index, long max)
{
    if (index < max)
    {
        return true;
    }

    Logger::Log(OutputMessageType::Error, "index, '%d', greater than max, '%d', for mesh, '%s'\n", index, max, name.c_str());
    return false;
}

bool Mesh::SizeCheck(size_t s1, size_t s2, const char * n1, const char * n2)
{
    if (s1 == 0 || s2 == 0 || s1 == s2)
    {
        return true;
    }

    Logger::Log(OutputMessageType::Error, "size of '%s' and '%s' arrays not equal for mesh, '%s'\n", n1, n2, name.c_str());
    return false;
}


// ------------------------------------------------------------------------------------------------
Model::Model()
{
    m_root = NULL;
    m_constructed = false;    
}

// ------------------------------------------------------------------------------------------------
Model::~Model()
{
    Destroy();
}

// ------------------------------------------------------------------------------------------------
Node * Model::CreateNode(const std::string& name)
{
    if (m_nodes.find(name) != m_nodes.end())
    {
        Logger::Log(OutputMessageType::Error, "node, '%s', is non-unique in model, '%ls'\n", name.c_str(), m_source.c_str());
    }

    Node * node = new Node();
    assert(node);
    node->name = name;
    m_nodes[name] = node;
    return node;
}

// ------------------------------------------------------------------------------------------------
Geometry * Model::CreateGeometry(const std::string& name)
{
    if (m_geometries.find(name) != m_geometries.end())
    {
        Logger::Log(OutputMessageType::Error, "geometry, '%s', is non-unique, '%ls'\n", name.c_str(), m_source.c_str());
    }

    Geometry * geo = new Geometry();
    assert(geo);
    geo->name = name;
    m_geometries[name] = geo;
    return geo;
}

// ------------------------------------------------------------------------------------------------
Material * Model::CreateMaterial(const std::string& name)
{
    if (m_materials.find(name) != m_materials.end())
    {
        Logger::Log(OutputMessageType::Error, "material, '%s', is non-unique, '%ls'\n", name.c_str(), m_source.c_str());
    }

    Material * mat = new Material();
    assert(mat);
    mat->name = name;
    m_materials[name] = mat;
    return mat;
}

// ------------------------------------------------------------------------------------------------
Mesh * Model::CreateMesh(const std::string& name)
{
    if (m_meshes.find(name) != m_meshes.end())
    {
        Logger::Log(OutputMessageType::Error, "mesh, '%s', is non-unique, '%ls'\n", name.c_str(), m_source.c_str());
    }

    Mesh * mesh = new Mesh();
    assert(mesh);
    mesh->name = name;
    m_meshes[name] = mesh;
    return mesh;
}

// ------------------------------------------------------------------------------------------------
Node * Model::GetNode(const std::string& name)
{
    auto it = m_nodes.find(name);
    if (it == m_nodes.end())
    {
        Logger::Log(OutputMessageType::Error, "Could not find node, '%s', while processing, '%ls'\n",
                                                                                name.c_str(), m_source.c_str());
        return NULL;
    }
    return it->second;
}

// ------------------------------------------------------------------------------------------------
Geometry * Model::GetGeometry(const std::string& name)
{
    auto it = m_geometries.find(name);
    if (it == m_geometries.end())
    {
        Logger::Log(OutputMessageType::Error, "Could not find geometry, '%s', while processing, '%ls'\n",
                                                                                name.c_str(), m_source.c_str());
        return NULL;
    }
    return it->second;
}

// ------------------------------------------------------------------------------------------------
Material * Model::GetMaterial(const std::string& name)
{
    auto it = m_materials.find(name);
    if (it == m_materials.end())
    {
        Logger::Log(OutputMessageType::Error, "Could not find material, '%s', while processing, '%ls'\n",
                                                                                name.c_str(), m_source.c_str());
        return Material::MissingMaterial();
    }
    return it->second;
}

// ------------------------------------------------------------------------------------------------
Mesh * Model::GetMesh(const std::string& name)
{
    auto it = m_meshes.find(name);
    if (it == m_meshes.end())
    {
        Logger::Log(OutputMessageType::Error, "Could not find mesh, '%s', while processing, '%ls'\n",
                                                                                name.c_str(), m_source.c_str());
        return NULL;
    }
    return it->second;
}

// ------------------------------------------------------------------------------------------------
void Model::SetRoot(Node* node)
{
    assert(m_root==NULL);
    m_root = node;
}

// ------------------------------------------------------------------------------------------------
void Model::SetSourceFileName(const std::wstring& name)
{
    m_source = name;
    m_path = name;
    // trim the filename from the path
    size_t lastSlash =  m_path.find_last_of('\\');
    if(lastSlash != std::wstring::npos)
    {
        m_path = m_path.substr(0, lastSlash+1);
    }
}

// ------------------------------------------------------------------------------------------------
HRESULT Model::Construct(ID3D11Device* d3dDevice, ResourceManager* manager)
{
    if(m_constructed)
    {
        return S_OK; // already done.
    }
    m_constructed = true;

    // setup node indexes
    std::vector<Node*> nodeStack;
    if(m_root)
    {
        nodeStack.push_back(m_root);
    }
    uint32_t currentIndex = 0;
    while(nodeStack.size())
    {
        Node * node = nodeStack.back();
        nodeStack.pop_back();
        node->index = currentIndex++;
        for(NodeArray::iterator it = node->children.begin(); it != node->children.end(); ++it)
        {
            nodeStack.push_back(*it);
        }
    }

    // fixup meshes & m_materials.
    for(MeshDict::iterator it = m_meshes.begin(); it != m_meshes.end(); ++it)
    {
        Mesh * mesh = it->second;
        assert(mesh);
        mesh->Construct(d3dDevice);       
    }

    // create D3D11 texture resource views
    for(MaterialDict::iterator it = m_materials.begin(); it != m_materials.end(); ++it)
    {
        Material * mat = it->second;
        assert(mat);
        for(unsigned int i = TextureType::MIN; i < TextureType::MAX; ++i)
        {
            if(mat->texNames[i].length())
            {
                WCHAR strPath[MAX_PATH];
                // initialize wTexName to wchar_t version of texNames[i]
                std::wstring wTexName(mat->texNames[i].begin(), mat->texNames[i].end());
                swprintf_s(strPath, MAX_PATH, L"%ls%ls", m_path.c_str(), wTexName.c_str());
                mat->textures[i] = (Texture*)manager->LoadAsync(strPath, TextureLib::Inst()->GetDefault((TextureTypeEnum)i) );                
                //mat->textures[i] = (Texture*)manager->LoadImmediate(m_strPathW, TextureLibGetDefault((TextureTypeEnum)i) );
            }
        }
    }


    ComputeAbsoluteTransforms();
    UpdateBounds();
    return S_OK;
}

// ------------------------------------------------------------------------------------------------
void Model::Destroy()
{
    for(MeshDict::iterator it = m_meshes.begin(); it != m_meshes.end(); ++it)
    {
        Mesh * m = it->second;
        SAFE_DELETE(m);
    }
    for(MaterialDict::iterator it = m_materials.begin(); it != m_materials.end(); ++it)
    {
        Material * m = it->second;
        for(unsigned int i = TextureType::MIN; i < TextureType::MAX; ++i)
        {
            SAFE_RELEASE(m->textures[i]);
        }
        SAFE_DELETE(m);
    }
    for(GeometryDict::iterator it = m_geometries.begin(); it != m_geometries.end(); ++it)
    {
        Geometry * geo = it->second;
        SAFE_DELETE(geo);
    }
    for(NodeDict::iterator it = m_nodes.begin(); it != m_nodes.end(); ++it)
    {
        Node * node = it->second;
        SAFE_DELETE(node);
    }

    m_meshes.clear();
    m_materials.clear();
    m_geometries.clear();
    m_nodes.clear();
    m_root = NULL;
    m_nodeTransforms.clear();
}


void Model::ComputeAbsoluteTransforms()
{    
    m_nodeTransforms.resize(m_nodes.size());
    std::vector<Node*> nodeStack;
    nodeStack.push_back(m_root);
    assert(m_constructed==true);
    while(nodeStack.size())
    {
        Node * node = nodeStack.back();
        nodeStack.pop_back();
        Matrix world = node->transform;
        if(node->parent)
        {
            world = world * m_nodeTransforms[node->parent->index]; // already calculated full world matrix for parent, so reuse-it.
        }
        
        m_nodeTransforms[node->index] = world;
        for(NodeArray::iterator it = node->children.begin(); it != node->children.end(); ++it)
        {
            nodeStack.push_back(*it);
        }
    }
}


// ------------------------------------------------------------------------------------------------
void Model::UpdateBounds()
{
    assert(m_constructed==true);
    const GeometryDict& geos = Geometries();
    if(geos.size()==0)
    {
        return;
    }

    float3 min = float3(FLT_MAX, FLT_MAX, FLT_MAX);
    float3 max = float3(-FLT_MAX, -FLT_MAX, -FLT_MAX);
    const NodeDict& nodes = Nodes();
    for(auto nodeIt = nodes.begin(); nodeIt != nodes.end(); ++nodeIt)
    {
        Node* node = nodeIt->second;
        for(auto geoIt = node->geometries.begin(); geoIt != node->geometries.end(); ++geoIt)
        {
            Geometry * geo = (*geoIt);
            AABB bbox = geo->mesh->bounds;
            bbox.Transform(m_nodeTransforms[node->index]);
            min = minimize(min, bbox.Min());
            max = maximize(max, bbox.Max());
        }
    }
    // note: min & max already transformed by entire object world matrix
    m_bounds = AABB(min, max);
}

Material::Material()
{    
    diffuse.x = diffuse.y = diffuse.z = diffuse.w = 1.0f;    
    ambient.x = ambient.y = ambient.z = 0.3f;
    ambient.w = 1.0f;

    specular.x = specular.y = specular.z = 0;
    specular.w = 1;
    emissive.x = emissive.y = emissive.z = emissive.w = 0;
    power = 1;
    for(unsigned int i = TextureType::MIN; i < TextureType::MAX; ++i)
    {        
        textures[i] = NULL;
    }
}

Material * Material::MissingMaterial()
{
    static Material s_missingMaterial;
    return &s_missingMaterial;
}

Node::~Node()
{
    for (auto it = attributes.begin(); it != attributes.end(); it++)
    {
        SAFE_DELETE((*it).second);
    }
}

}; // namespace LvEdEngine

