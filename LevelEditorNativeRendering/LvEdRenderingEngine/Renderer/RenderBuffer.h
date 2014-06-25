//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <D3D11.h>
#include "../Core/Utils.h"
#include "../Core/WinHeaders.h"
#include "../Core/NonCopyable.h"
#include "../Core/Logger.h"
#include "../VectorMath/V3dMath.h"

#include "RenderEnums.h"

namespace LvEdEngine
{

    
// --------------------------------------------------------------------------------------------------
// vertex with position, normal, tangent, texcoord0
class VertexPNTT
{
public:
    VertexPNTT(){};
    VertexPNTT(float x, float y, float z, float nx, float ny, float nz, float tx, float ty, float tz, float u, float v)
    : Position(x,y,z)
    , Normal(nx,ny,nz)
    , Tangent(tx,ty,tz)
    , Tex(u,v)
    {
    }
    VertexPNTT(float3 p, float3 n, float3 t, float2 tx)
    : Position(p)
    , Normal(n)
    , Tangent(t)
    , Tex(tx)
    {
    }
    float3 Position;
    float3 Normal;
    float2 Tex;
    float3 Tangent;
};

// --------------------------------------------------------------------------------------------------
// vertex with position, normal, texcoord0
class VertexPNT
{
public:
    VertexPNT(){};
    VertexPNT(float x, float y, float z, float nx, float ny, float nz, float u, float v)
    : Position(x,y,z)
    , Normal(nx,ny,nz)
    , Tex(u,v)
    {
    }
    VertexPNT(float3 p, float3 n, float2 t)
    : Position(p)
    , Normal(n)
    , Tex(t)
    {
    }
    float3 Position;
    float3 Normal;
    float2 Tex;
};

// --------------------------------------------------------------------------------------------------
// vertex with position, normal
class VertexPN
{
public:
    VertexPN(){};
    VertexPN(float x, float y, float z, float nx, float ny, float nz)
    : Position(x,y,z)
    , Normal(nx,ny,nz)
    {
    }
    VertexPN(float3 p, float3 n)
    : Position(p)
    , Normal(n)
    {
    }
    float3 Position;
    float3 Normal;
};



// --------------------------------------------------------------------------------------------------
// vertex with position, texcoord0
class VertexPT
{
public:
    VertexPT(){};
    VertexPT(float x, float y, float z, float u, float v)
    : Position(x,y,z)    
    , Tex(u,v)
    {
    }
    VertexPT(float3 p, float2 t)
    : Position(p)    
    , Tex(t)
    {
    }
    float3 Position;    
    float2 Tex;
};

// --------------------------------------------------------------------------------------------------
// vertex with position, color
class VertexPC
{
public:
    VertexPC(){};
    VertexPC(const float3& pos, const float4& c)
    : Position(pos)
    , Color(c)
    {
    }   
    float3 Position;    
    float4 Color;
};


// --------------------------------------------------------------------------------------------------
class GpuBuffer : public NonCopyable
{
public:
    
    GpuBuffer(ID3D11Buffer* buffer, uint32_t count);
    virtual ~GpuBuffer();
    ID3D11Buffer* GetBuffer() const {return m_buffer;}
    uint32_t GetCount() const {return m_count;}

    void SetDebugName(const char* name);

private:
    ID3D11Buffer* m_buffer;
    uint32_t m_count;
};

// --------------------------------------------------------------------------------------------------
class VertexBuffer : public GpuBuffer
{
public:
    VertexBuffer(ID3D11Buffer* buffer, uint32_t numVerts, VertexFormatEnum vf);
    VertexFormatEnum GetFormat() const { return m_format;}
    uint32_t GetStride() const {return m_stride;}

private:
    VertexFormatEnum m_format;
    uint32_t m_stride;
};

// --------------------------------------------------------------------------------------------------
class IndexBuffer : public GpuBuffer
{
public:
    IndexBuffer(ID3D11Buffer* buffer, uint32_t numindices);
};

inline void UpdateConstantBuffer(ID3D11DeviceContext* dc, ID3D11Buffer* buffer,void *data, uint32_t size)
{
    D3D11_MAPPED_SUBRESOURCE mappedResource;
    HRESULT hr = dc->Map(buffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
    Logger::IsFailureLog(hr,L"failed map cb");
    CopyMemory(mappedResource.pData, data, size);          
    dc->Unmap(buffer, 0);
}

}; // namespace LvEdEngine

