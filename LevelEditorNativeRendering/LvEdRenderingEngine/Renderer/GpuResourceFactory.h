//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "RenderBuffer.h"

namespace LvEdEngine
{


namespace BufferUsage
{
enum BufferUsage
{
    DEFAULT	= D3D11_USAGE_DEFAULT,
    IMMUTABLE	= D3D11_USAGE_IMMUTABLE,
    DYNAMIC	= D3D11_USAGE_DYNAMIC, 
    BufferUsage_ForceUint32 = 0xffffffff
};
}

namespace IndexBufferFormat
{
enum IndexBufferFormat
{
    U16 = DXGI_FORMAT_R16_UINT,
    U32 = DXGI_FORMAT_R32_UINT,
    IndexBufferFormat_ForceUint32 = 0xffffffff
};
}



// GpuResourceFactory a static class used for 
// creating Gpu resources.
// This factory doesn't manage object's life time.
// It is upto caller to delete resources created by this factory.
// There are other service that caches resource.
class GpuResourceFactory
{

public:
    // set the device used for creating resources.
    static void SetDevice(ID3D11Device* device);

    // Create vertex buffer
    // data:  source data it can be null if the buffer usage is dynamic.
    // vf  : see VertexFormatEnum
    // count: number of vertex to create it can be zero if the data is null
    // bufferUsage: see enum BufferUsage
    static VertexBuffer* CreateVertexBuffer(void* data, VertexFormatEnum vf, uint32_t count, uint32_t bufferUsage = BufferUsage::DEFAULT);
    
    // Create index buffer
    // data: source data, it can be null if the buffer usage is dynamic.
    // bufferFormat: see enum IndexBufferFormat 
    // count: number of indices it can be zero if the data is null.
    // bufferUsage:  see BufferUsage enum
    static IndexBuffer* CreateIndexBuffer(void* data, uint32_t count, uint32_t bufferFormat = IndexBufferFormat::U32 , uint32_t bufferUsage = BufferUsage::DEFAULT);

    // Create constant buffer
    // sizeInBytes: size of the buffer in bytes
    static ConstantBuffer* CreateConstantBuffer(uint32_t sizeInBytes);

    // Create Input layout
    // code : compiled shader code with With input signature
    // codeSize: code size in bytes
    // vf: vertex format
    static ID3D11InputLayout* CreateInputLayout(void* code, uint32_t codeSize, VertexFormatEnum vf);
    static ID3D11InputLayout* CreateInputLayout(ID3DBlob* blob, VertexFormatEnum vf)
    {
        if(blob) return CreateInputLayout(blob->GetBufferPointer(), (uint32_t)blob->GetBufferSize(),vf);
        return NULL;
    }

    // create shader functions    

    // create vertex shader
    // code : compiled shader code
    // codeSize: code size in bytes
    static ID3D11VertexShader*   CreateVertexShader(void* code, uint32_t codeSize);    
    static ID3D11VertexShader*   CreateVertexShader(ID3DBlob* blob)
    {
        if(blob) return CreateVertexShader(blob->GetBufferPointer(), (uint32_t)blob->GetBufferSize());
        return NULL;
    }

    // create pixel shader
    // code : compiled shader code
    // codeSize: code size in bytes
    static ID3D11PixelShader*    CreatePixelShader(void* code, uint32_t codeSize);
    static ID3D11PixelShader*    CreatePixelShader(ID3DBlob* blob)
    {
        if(blob) return CreatePixelShader(blob->GetBufferPointer(), (uint32_t)blob->GetBufferSize());
        return NULL;
    }


    // create geometry shader
    // code : compiled shader code
    // codeSize: code size in bytes
    static ID3D11GeometryShader* CreateGeometryShader(void* code, uint32_t codeSize);
    static ID3D11GeometryShader* CreateGeometryShader(ID3DBlob* blob)
    {
        if(blob) return CreateGeometryShader(blob->GetBufferPointer(), (uint32_t)blob->GetBufferSize());
        return NULL;
    }

    // ======= texture creation functions.
    static ID3D11ShaderResourceView* CreateTextureView(ID3D11Texture2D *tex);
    static ID3D11Texture2D* CreateDxTexture2D(void* buff, int w, int h, bool cubemap = false);

private:
    
    // revoke all the default functions
      GpuResourceFactory() {}
      ~GpuResourceFactory() {}   
      GpuResourceFactory( const GpuResourceFactory& );
      GpuResourceFactory& operator=( const GpuResourceFactory& );
};


}