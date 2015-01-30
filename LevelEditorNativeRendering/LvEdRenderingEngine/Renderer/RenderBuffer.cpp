//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "RenderBuffer.h"


namespace LvEdEngine
{
 
// ------------------------------------------------------------------------------------------------
GpuBuffer::GpuBuffer(ID3D11Buffer* buffer) 
    : m_buffer(buffer)
    
{
    assert(m_buffer);
    D3D11_BUFFER_DESC bufDescr;
    m_buffer->GetDesc(&bufDescr);
    m_size = bufDescr.ByteWidth;
    m_stride = m_size; // derived class sets m_stride
    m_writable = bufDescr.CPUAccessFlags == D3D11_CPU_ACCESS_WRITE
        && bufDescr.Usage == D3D11_USAGE_DYNAMIC;
    // note CPUAccessFlags must be exactly D3D11_CPU_ACCESS_WRITE
}
    
// ------------------------------------------------------------------------------------------------
GpuBuffer::~GpuBuffer()
{
    SAFE_RELEASE(m_buffer);  
    m_size = 0;
}

// ------------------------------------------------------------------------------------------------
void GpuBuffer::SetDebugName(const char* name)
{    
    m_buffer->SetPrivateData( WKPDID_D3DDebugObjectName, lstrlenA(name), name );
}


void GpuBuffer::Update(ID3D11DeviceContext* dc, void* data,uint32_t count)
{
    D3D11_MAPPED_SUBRESOURCE mappedResource;
    HRESULT hr = dc->Map(m_buffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
    if(Logger::IsFailureLog(hr,L"Buffer updating failed.")) return;
    uint32_t size = m_stride * count;
    CopyMemory(mappedResource.pData, data, size);          
    dc->Unmap(m_buffer, 0);
}


// ------------------------------------------------------------------------------------------------
VertexBuffer::VertexBuffer(ID3D11Buffer* buffer,  uint32_t stride)
    : GpuBuffer(buffer)
    
{        
    m_count = GetSize() / stride;
    m_stride = stride;
}

// ------------------------------------------------------------------------------------------------
IndexBuffer::IndexBuffer(ID3D11Buffer* buffer, uint32_t stride)
    : GpuBuffer(buffer)    
{        
    m_count = GetSize() / stride;
    m_stride = stride;
}

uint32_t IndexBuffer::GetFormat() const
{
    if(m_stride == 4)
        return (uint32_t) DXGI_FORMAT_R32_UINT;
    if(m_stride == 2)
        return (uint32_t) DXGI_FORMAT_R16_UINT;
    assert(0);
    return   (uint32_t)DXGI_FORMAT_UNKNOWN;
}




// ========== TConstantBufferBase imple ====

void TConstantBufferBase::Construct(ID3D11Device* device, uint32_t sizeInBytes)
{
    assert(m_buffer == NULL);
    if(m_buffer) return;

    HRESULT hr = S_OK;        
    D3D11_BUFFER_DESC desc;
    SecureZeroMemory( &desc, sizeof(desc));
	desc.Usage = D3D11_USAGE_DYNAMIC;
	desc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
	desc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	desc.MiscFlags = 0;  
	desc.ByteWidth = sizeInBytes;
	desc.StructureByteStride = 0;	

    hr = device->CreateBuffer(&desc, 0, &m_buffer );
    if(!Logger::IsFailureLog(hr,L"Failed to create constant buffer"))
    {
        m_bufSize = sizeInBytes;
    }     
}


void TConstantBufferBase::Update(ID3D11DeviceContext* dc, void* data)
{
    assert(m_buffer);
    if(!m_buffer) return;
    D3D11_MAPPED_SUBRESOURCE mappedResource;
    HRESULT hr = dc->Map(m_buffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
    if(Logger::IsFailureLog(hr,L"failed map cb")) return;    
    CopyMemory(mappedResource.pData, data, m_bufSize);          
    dc->Unmap(m_buffer, 0);
}


}; // namespace 

