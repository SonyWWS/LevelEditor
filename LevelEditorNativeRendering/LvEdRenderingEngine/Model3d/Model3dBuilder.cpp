//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "Model3dBuilder.h"
#include "../Renderer/Model.h"
#include "../Core/Logger.h"
#include "rapidxmlhelpers.h"
#include <sstream>
#include <stdexcept>


namespace LvEdEngine
{


// ------------------------------------------------------------------------------------------------
void Model3dBuilder::Begin()
{
}

// ------------------------------------------------------------------------------------------------
void Model3dBuilder::End()
{
    CalculateTangents();
}


// ------------------------------------------------------------------------------------------------
// uses the 'vcount' and 'indices' array to add vertex/index data into the current mesh.
void Model3dBuilder::Mesh_AddPolys()
{
  switch(m_mesh.poly.primType)
  {
  default:
      Logger::Log(OutputMessageType::Error, "Unsupported primitive type, %d\n", m_mesh.poly.primType);
      break;
  case BuilderPrimitiveType::POLYGONS:
  case BuilderPrimitiveType::TRIFANS:
  case BuilderPrimitiveType::TRIANGLES:
      {
          UINT currentIndex = 0;
          UINT stride = m_mesh.poly.stride;
          for(UINT i = 0; i < m_mesh.poly.vcount.size(); ++i)
          {
            UINT vcount = m_mesh.poly.vcount[i];
            UINT p0 = currentIndex;
            UINT p1 = currentIndex + stride;
            UINT p2 = currentIndex + stride + stride;

            UINT temp = vcount;
            while(temp>=3)
            {
                Mesh_AddTriangle(p0, p1, p2);
                p1 += stride;
                p2 += stride;
                --temp;
            }
            currentIndex += m_mesh.poly.stride*vcount;
          }
      }
      break;
  case BuilderPrimitiveType::TRISTRIPS:
      {
          UINT currentIndex = 0;
          UINT stride = m_mesh.poly.stride;
          for(UINT i = 0; i < m_mesh.poly.vcount.size(); ++i)
          {
            UINT vcount = m_mesh.poly.vcount[i];
            UINT p0 = currentIndex;
            UINT p1 = currentIndex + stride;
            UINT p2 = currentIndex + stride + stride;

            UINT temp = vcount;
            bool even = true;
            while(temp>=3)
            {
                if(even)
                {
                    Mesh_AddTriangle(p0, p1, p2);
                }
                else
                {
                    Mesh_AddTriangle(p2, p1, p0);
                }
                even = !even;
                p0 += stride;
                p1 += stride;
                p2 += stride;
                --temp;
            }
            currentIndex += m_mesh.poly.stride*vcount;
          }
      }
      break;
  }

}

// ------------------------------------------------------------------------------------------------
void Model3dBuilder::Mesh_SetPrimType(const char * name)
{
    if(strcmp(name, "POLYGONS")==0)
    {
        m_mesh.poly.primType = BuilderPrimitiveType::POLYGONS;
    }
    else if(strcmp(name, "TRIANGLES")==0)
    {
        m_mesh.poly.primType = BuilderPrimitiveType::TRIANGLES;
    }
    else if(strcmp(name, "TRISTRIPS")==0)
    {
        m_mesh.poly.primType = BuilderPrimitiveType::TRISTRIPS;
    }
    else if(strcmp(name, "TRIFANS")==0)
    {
        m_mesh.poly.primType = BuilderPrimitiveType::TRIFANS;
    }
    
}



// ------------------------------------------------------------------------------------------------
// uses the 'indices' array to add vertex/index data into the current mesh.
void Model3dBuilder::Mesh_AddTriangles()
{
    UINT currentIndex = 0;
    UINT stride = m_mesh.poly.stride;

    while(currentIndex < m_mesh.poly.indices.size())
    {
        UINT vcount = 3;
        UINT p0 = currentIndex;
        UINT p1 = currentIndex + stride;
        UINT p2 = currentIndex + stride + stride;
        Mesh_AddTriangle(p0, p1, p2);
        currentIndex += m_mesh.poly.stride*vcount;
    }
}

// ------------------------------------------------------------------------------------------------
// uses the 'indices' array to add vertex/index data into the current mesh.
void Model3dBuilder::Mesh_AddTriStrips()
{
    UINT currentIndex = 0;
    UINT stride = m_mesh.poly.stride;
    bool even = true;
    while(currentIndex < m_mesh.poly.indices.size())
    {
        UINT p0 = currentIndex;
        UINT p1 = currentIndex + stride;
        UINT p2 = currentIndex + stride + stride;
        currentIndex += m_mesh.poly.stride;
        if(even)
        {
            Mesh_AddTriangle(p0, p1, p2);
        }
        else
        {
            Mesh_AddTriangle(p2, p1, p0);
        }
        even = !even;
    }
}

// ------------------------------------------------------------------------------------------------
// uses the 'indices' array to add vertex/index data into the current mesh.
void Model3dBuilder::Mesh_AddTriFans()
{
    UINT currentIndex = 0;
    UINT stride = m_mesh.poly.stride;
    UINT p0 = currentIndex;
    while(currentIndex < m_mesh.poly.indices.size())
    {
        UINT p1 = currentIndex + stride;
        UINT p2 = currentIndex + stride + stride;
        Mesh_AddTriangle(p0, p1, p2);
        currentIndex += m_mesh.poly.stride;
    }
}


// ------------------------------------------------------------------------------------------------
void Model3dBuilder::Mesh_Reset()
{
  Mesh_ResetSourceInfo();
  Mesh_ResetPolyInfo();
  m_mesh.mesh = NULL;
}

// ------------------------------------------------------------------------------------------------
void Model3dBuilder::Mesh_ResetSourceInfo()
{
  // this will not free the associated vector memory, but it is probably better to leave the associated
  // memory so that they will be reused without having to allocate
  m_mesh.source.pos.clear();
  m_mesh.source.nor.clear();
  m_mesh.source.tex.clear();
}

// ------------------------------------------------------------------------------------------------
void Model3dBuilder::Mesh_ResetPolyInfo()
{
  m_mesh.poly.stride = 0;
  m_mesh.poly.hasPos = false;
  m_mesh.poly.hasNor = false;
  m_mesh.poly.hasTex = false;
  m_mesh.poly.posOffset = 0;
  m_mesh.poly.norOffset = 0;
  m_mesh.poly.texOffset = 0;

  // this will not free the associated vector memory, but it is probably better to leave the associated
  // memory so that they will be reused without having to allocate
  m_mesh.poly.vertexindices.clear();
  m_mesh.poly.vcount.clear();
  m_mesh.poly.indices.clear();
}


// ------------------------------------------------------------------------------------------------
void Model3dBuilder::Mesh_Begin(const char * name)
{
  m_mesh.mesh = m_model->CreateMesh(name);
}

// ------------------------------------------------------------------------------------------------
template <class T>
static void CheckIndex(bool hasIndex, T * vec, UINT index, const char * checkName)
{
    if (hasIndex && index >= vec->size())
    {
        std::stringstream ss;
        ss << checkName << " index (" << index << ") out of range (0-" << vec->size() << ")";
        throw std::runtime_error(ss.str());
    }
}

// ------------------------------------------------------------------------------------------------
UINT Model3dBuilder::Mesh_AddVertex(UINT p, UINT n, UINT t)
{
    UINT index = (UINT)m_mesh.mesh->pos.size();
    UINT3 tuple;
    tuple.p = p;
    tuple.n = n;
    tuple.t = t;

    // only add tuple if it does not already exist
    auto it = m_mesh.poly.vertexindices.find(tuple);
    if(it == m_mesh.poly.vertexindices.end())
    {
        // map tuple to index in pos vector, and update 'it'
        m_mesh.poly.vertexindices[tuple] = index;
        it = m_mesh.poly.vertexindices.find(tuple);

        // add zero pos, nor, tex
        float3 zero;
        zero.x = zero.y = zero.z = 0.0f;
        m_mesh.mesh->pos.push_back(zero);
        m_mesh.mesh->nor.push_back(zero);
        m_mesh.mesh->tex.push_back(*(float2*)&zero);

        // validate indices in bounds
        CheckIndex(m_mesh.poly.hasPos, &m_mesh.source.pos, tuple.p, "vertex position");
        CheckIndex(m_mesh.poly.hasNor, &m_mesh.source.nor, tuple.n, "vertex normal");
        CheckIndex(m_mesh.poly.hasTex, &m_mesh.source.tex, tuple.t, "vertex texture coordinate");

        // if provided, set pos, nor, tex
        if(m_mesh.poly.hasPos) m_mesh.mesh->pos.back() = (m_mesh.source.pos[tuple.p]);
        if(m_mesh.poly.hasNor) m_mesh.mesh->nor.back() = (m_mesh.source.nor[tuple.n]);
        if(m_mesh.poly.hasTex) m_mesh.mesh->tex.back() = (m_mesh.source.tex[tuple.t]);

        assert(m_mesh.mesh->pos.size() == m_mesh.mesh->nor.size());
        assert(m_mesh.mesh->pos.size() == m_mesh.mesh->tex.size());
    }

    // return index from 
    assert(it != m_mesh.poly.vertexindices.end());
    return it->second;
}

// ------------------------------------------------------------------------------------------------
void Model3dBuilder::Mesh_End()
{
    if(m_mesh.mesh )
    {
        m_mesh.mesh->ComputeBound();
    }   
    m_mesh.mesh = NULL;
}

// ------------------------------------------------------------------------------------------------
void Model3dBuilder::CalculateTangents()
{

    // check to see if any materials have a normal map, if anyone does, 
    // we generate tangents for the entire model.
    bool needsTangents = false;
    for(auto it = m_model->Materials().begin(); it != m_model->Materials().end(); ++it)
    {
        Material * mat = it->second;
        if(mat->texNames[TextureType::NORMAL].size() > 0 )
        {
            needsTangents = true;
        }
    }

    if(!needsTangents)
    {
        return;
    }

    for(auto it = m_model->Meshes().begin(); it != m_model->Meshes().end(); ++it)
    {
        Mesh * mesh = it->second;
        mesh->ComputeTangents();
    }
}


// ------------------------------------------------------------------------------------------------
void Model3dBuilder::Mesh_AddTriangle(UINT p0, UINT p1, UINT p2)
{
    UINT posOffset = m_mesh.poly.posOffset;
    UINT norOffset = m_mesh.poly.norOffset;
    UINT texOffset = m_mesh.poly.texOffset;

    UINT p = 0;
    UINT n = 0; 
    UINT t = 0;

    // validate inbounds for position
    CheckIndex(m_mesh.poly.hasPos, &m_mesh.poly.indices, p0+posOffset, "triangle position");
    CheckIndex(m_mesh.poly.hasPos, &m_mesh.poly.indices, p1+posOffset, "triangle position");
    CheckIndex(m_mesh.poly.hasPos, &m_mesh.poly.indices, p2+posOffset, "triangle position");

    // validate inbounds for normal
    CheckIndex(m_mesh.poly.hasNor, &m_mesh.poly.indices, p0+norOffset, "triangle normal");
    CheckIndex(m_mesh.poly.hasNor, &m_mesh.poly.indices, p1+norOffset, "triangle normal");
    CheckIndex(m_mesh.poly.hasNor, &m_mesh.poly.indices, p2+norOffset, "triangle normal");

    // validate inbounds for texture coordinate
    CheckIndex(m_mesh.poly.hasTex, &m_mesh.poly.indices, p0+texOffset, "triangle texture-coordinate");
    CheckIndex(m_mesh.poly.hasTex, &m_mesh.poly.indices, p1+texOffset, "triangle texture-coordinate");
    CheckIndex(m_mesh.poly.hasTex, &m_mesh.poly.indices, p2+texOffset, "triangle texture-coordinate");

    // each vertex is a tuple of indices.
    if(m_mesh.poly.hasPos)   p  = m_mesh.poly.indices[p0+posOffset];
    if(m_mesh.poly.hasNor)   n  = m_mesh.poly.indices[p0+norOffset];
    if(m_mesh.poly.hasTex)   t  = m_mesh.poly.indices[p0+texOffset];
    UINT v1 = Mesh_AddVertex(p,n,t);

    // vertex2
    if(m_mesh.poly.hasPos)   p  = m_mesh.poly.indices[p1+posOffset];
    if(m_mesh.poly.hasNor)   n  = m_mesh.poly.indices[p1+norOffset];
    if(m_mesh.poly.hasTex)   t  = m_mesh.poly.indices[p1+texOffset];
    UINT v2 = Mesh_AddVertex(p,n,t);

    // vertex3
    if(m_mesh.poly.hasPos)   p  = m_mesh.poly.indices[p2+posOffset];
    if(m_mesh.poly.hasNor)   n  = m_mesh.poly.indices[p2+norOffset];
    if(m_mesh.poly.hasTex)   t  = m_mesh.poly.indices[p2+texOffset];
    UINT v3 = Mesh_AddVertex(p,n,t);

    // add the triangle indices
    m_mesh.mesh->indices.push_back(v1);
    m_mesh.mesh->indices.push_back(v2);
    m_mesh.mesh->indices.push_back(v3);

}


// Instances
void Model3dBuilder::AddInstance(Node* node)
{
    m_instances[node->name] = node;
}

Node* Model3dBuilder::FindInstance(const char* name)
{
    assert(name != NULL);
    auto it = m_instances.find(std::string(name));
    if(it != m_instances.end())
    {
        return it->second;
    }
    return NULL;
}



}; // namespace LvEdEngine
