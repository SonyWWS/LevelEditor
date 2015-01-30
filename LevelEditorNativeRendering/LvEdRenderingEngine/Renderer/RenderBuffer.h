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
// Base class for Vertex,Index, and Constant buffer
class GpuBuffer : public NonCopyable
{
public:
            
    GpuBuffer(ID3D11Buffer* buffer);
    virtual ~GpuBuffer();
    ID3D11Buffer* GetBuffer() const {return m_buffer;}

    // Gets buffer size in bytes
    uint32_t GetSize() { return m_size;}
   
    void SetDebugName(const char* name);

    // update buffer from data
    // count number of elements     
    void Update(ID3D11DeviceContext* dc, void* data,uint32_t count = 1);


protected:
    uint32_t m_stride;
private:
    ID3D11Buffer* m_buffer;    
    uint32_t m_size;
    bool m_writable; // the buffer can be updated

};

// --------------------------------------------------------------------------------------------------
class VertexBuffer : public GpuBuffer
{
public:
    VertexBuffer(ID3D11Buffer* buffer, uint32_t stride);
    //VertexFormatEnum GetFormat() const { return m_format;}

    // Gets the size of one vertex in bytes.
    uint32_t GetStride() const {return m_stride;}

    //Gets number of vertices
    uint32_t GetCount() const {return m_count;}

private:
    //VertexFormatEnum m_format;    
    uint32_t m_count;       
};

// --------------------------------------------------------------------------------------------------
class IndexBuffer : public GpuBuffer
{
public:
    IndexBuffer(ID3D11Buffer* buffer, uint32_t stride);

    //Gets number of indices
    uint32_t GetCount() const {return m_count;}
    uint32_t GetFormat() const;
private:
    uint32_t m_count;    
};

class ConstantBuffer : public GpuBuffer
{
public:
    ConstantBuffer(ID3D11Buffer* buffer)
        :GpuBuffer(buffer){}

};

class TConstantBufferBase
{
public:
    TConstantBufferBase() : m_buffer(NULL), m_bufSize(0) {}
    ID3D11Buffer* GetBuffer() const {return m_buffer;}
    ~TConstantBufferBase() { SAFE_RELEASE(m_buffer); }
   	
protected:
    void Update(ID3D11DeviceContext* dc, void* data);
    void Construct(ID3D11Device* device, uint32_t sizeInBytes);   
private:
    uint32_t m_bufSize; // size of the buffer in bytes.
    ID3D11Buffer* m_buffer;
    
};
// templated version of Constant buffer.
template<typename T>
class TConstantBuffer : public TConstantBufferBase
{
public:

    TConstantBuffer() {}
	T Data;
    
    // creates constant buffer in gpu memory.
    void Construct(ID3D11Device* device)
    {     
        TConstantBufferBase::Construct(device,sizeof(T));
    }
    void Update(ID3D11DeviceContext* dc)
    {
        TConstantBufferBase::Update(dc, &Data);
    }	
private:
	TConstantBuffer(const TConstantBuffer&);
	TConstantBuffer& operator=(const TConstantBuffer&);
};

}; // namespace LvEdEngine

