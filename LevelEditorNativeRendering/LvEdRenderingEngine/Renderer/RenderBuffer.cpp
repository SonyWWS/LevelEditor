//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "RenderBuffer.h"
#include "RenderUtil.h"

namespace LvEdEngine
{

// ------------------------------------------------------------------------------------------------
GpuBuffer::GpuBuffer(ID3D11Buffer* buffer, uint32_t count) 
    : m_buffer(buffer)
    , m_count(count)
{
}
    
// ------------------------------------------------------------------------------------------------
GpuBuffer::~GpuBuffer()
{
    SAFE_RELEASE(m_buffer);    
}

// ------------------------------------------------------------------------------------------------
void GpuBuffer::SetDebugName(const char* name)
{
    m_buffer->SetPrivateData( WKPDID_D3DDebugObjectName, lstrlenA(name), name );
}


// ------------------------------------------------------------------------------------------------
VertexBuffer::VertexBuffer(ID3D11Buffer* buffer, uint32_t numVerts, VertexFormatEnum vf)
    : GpuBuffer(buffer, numVerts)
    , m_format(vf)
{        
    m_stride = GetSizeInBytes(vf);
}

// ------------------------------------------------------------------------------------------------
IndexBuffer::IndexBuffer(ID3D11Buffer* buffer, uint32_t numindices)
    : GpuBuffer(buffer, numindices)
{        
}

}; // namespace 

