#include "GpuResourceFactory.h"

using namespace LvEdEngine;

static ID3D11Device* s_device = NULL;
void GpuResourceFactory::SetDevice(ID3D11Device* device) { s_device = device; }
static uint32_t GetSizeInBytes(VertexFormatEnum vf);

VertexBuffer* GpuResourceFactory::CreateVertexBuffer(void* data, VertexFormatEnum vf, uint32_t count, uint32_t bufferUsage)
{    
    uint32_t vertexSize = GetSizeInBytes(vf);

    HRESULT hr = S_OK;
    UINT cpuAccess = 0;
    if(bufferUsage == BufferUsage::DYNAMIC )
    {
        cpuAccess = D3D11_CPU_ACCESS_WRITE;
    }

	D3D11_BUFFER_DESC bufDcr;
	SecureZeroMemory( &bufDcr, sizeof(bufDcr));
    bufDcr.Usage = (D3D11_USAGE) bufferUsage;
    bufDcr.ByteWidth =   vertexSize * count;
    bufDcr.BindFlags = D3D11_BIND_VERTEX_BUFFER;
	bufDcr.CPUAccessFlags = cpuAccess;
    bufDcr.MiscFlags = 0;
    bufDcr.StructureByteStride = 0;

    D3D11_SUBRESOURCE_DATA bufData;
	SecureZeroMemory( &bufData, sizeof(bufData) );
    bufData.pSysMem = data;
    D3D11_SUBRESOURCE_DATA* pbufData = (data)? &bufData : NULL;

    ID3D11Buffer* gpuBuffer = NULL;     
    hr = s_device->CreateBuffer( &bufDcr, pbufData, &gpuBuffer );
	
    if (Logger::IsFailureLog(hr, L"CreateBuffer"))
        return NULL;

    return new VertexBuffer(gpuBuffer, vertexSize);    
}

IndexBuffer* GpuResourceFactory::CreateIndexBuffer(void* data, uint32_t count,uint32_t bufferFormat, uint32_t bufferUsage)
{
    HRESULT hr = S_OK;
    uint32_t cpuAccess = 0;
    uint32_t elmSize = bufferFormat == IndexBufferFormat::U16 ? 2 : 4;

    if(bufferUsage == D3D11_USAGE_DYNAMIC )
    {
        cpuAccess = D3D11_CPU_ACCESS_WRITE;
    }

	D3D11_BUFFER_DESC bufDcr;
    SecureZeroMemory( &bufDcr, sizeof(bufDcr));
    bufDcr.Usage = (D3D11_USAGE) bufferUsage;
    bufDcr.ByteWidth = elmSize * count;
    bufDcr.BindFlags = D3D11_BIND_INDEX_BUFFER;
	bufDcr.CPUAccessFlags = cpuAccess;
    bufDcr.MiscFlags = 0;
    bufDcr.StructureByteStride = 0;

    D3D11_SUBRESOURCE_DATA bufData;
	SecureZeroMemory( &bufData, sizeof(bufData) );
    bufData.pSysMem = data;

    D3D11_SUBRESOURCE_DATA* pbufData = data ? &bufData : NULL;
    ID3D11Buffer* gpuBuffer = NULL; 
    hr = s_device->CreateBuffer(&bufDcr, pbufData, &gpuBuffer );
    if (Logger::IsFailureLog(hr, L"CreateBuffer"))
        return NULL;
    
    return new IndexBuffer(gpuBuffer, elmSize);    
}

ConstantBuffer* GpuResourceFactory::CreateConstantBuffer(uint32_t sizeInBytes)
{
    HRESULT hr = S_OK;        
    D3D11_BUFFER_DESC desc;
    SecureZeroMemory( &desc, sizeof(desc));
	desc.Usage = D3D11_USAGE_DYNAMIC;
	desc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
	desc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	desc.MiscFlags = 0;  
	desc.ByteWidth = sizeInBytes;
	desc.StructureByteStride = 0;

    ID3D11Buffer* buffer = NULL;
    hr = s_device->CreateBuffer(&desc, 0, &buffer );
    if(LvEdEngine::Logger::IsFailureLog(hr,L"Failed to create constant buffer"))
        return NULL;
    return new ConstantBuffer(buffer);  
}

static void SetLayout(D3D11_INPUT_ELEMENT_DESC * desc, LPCSTR name, UINT index, DXGI_FORMAT format, UINT slot, UINT offset, D3D11_INPUT_CLASSIFICATION slotClass, UINT rate)
{
    desc->SemanticName = name;
    desc->SemanticIndex = index;
    desc->Format = format;
    desc->InputSlot = slot;
    desc->AlignedByteOffset = offset;
    desc->InputSlotClass = slotClass;
    desc->InstanceDataStepRate = rate;
}

ID3D11InputLayout* GpuResourceFactory::CreateInputLayout(void* code, uint32_t codeSize, VertexFormatEnum vf)
{
    D3D11_INPUT_ELEMENT_DESC elements[10];
    uint32_t numelements = 0;
    switch(vf)
    {    
    case VertexFormat::VF_P:
        SetLayout(&elements[0], "POSITION",  0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        numelements = 1;
        break;
    case VertexFormat::VF_PC:
        SetLayout(&elements[0], "POSITION",  0, DXGI_FORMAT_R32G32B32_FLOAT, 0,  0,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&elements[1], "COLOR",     0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        numelements = 2;
        break;
    case VertexFormat::VF_PTC:
        SetLayout(&elements[0], "POSITION",  0, DXGI_FORMAT_R32G32B32_FLOAT, 0,  0,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&elements[1], "TEXCOORD",  0, DXGI_FORMAT_R32G32_FLOAT,    0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&elements[2], "COLOR",     0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        numelements = 3;
        break;
    case VertexFormat::VF_PN:
        SetLayout(&elements[0], "POSITION",  0, DXGI_FORMAT_R32G32B32_FLOAT, 0,  0,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&elements[1], "NORMAL",    0, DXGI_FORMAT_R32G32B32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        numelements = 2;
        break;
    case VertexFormat::VF_PT:
        SetLayout(&elements[0], "POSITION",  0, DXGI_FORMAT_R32G32B32_FLOAT, 0,  0,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&elements[1], "TEXCOORD",  0, DXGI_FORMAT_R32G32_FLOAT,    0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        numelements = 2;
        break;
    case VertexFormat::VF_PNT:
        SetLayout(&elements[0], "POSITION",  0, DXGI_FORMAT_R32G32B32_FLOAT, 0,  0,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&elements[1], "NORMAL",    0, DXGI_FORMAT_R32G32B32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&elements[2], "TEXCOORD",  0, DXGI_FORMAT_R32G32_FLOAT,    0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        numelements = 3;
        break;
    case VertexFormat::VF_PNTT:
        SetLayout(&elements[0], "POSITION",  0, DXGI_FORMAT_R32G32B32_FLOAT, 0,  0,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&elements[1], "NORMAL",    0, DXGI_FORMAT_R32G32B32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&elements[2], "TEXCOORD",  0, DXGI_FORMAT_R32G32_FLOAT,    0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&elements[3], "TANGENT",   0, DXGI_FORMAT_R32G32B32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        numelements = 4;
        break;   

    case VertexFormat::VF_T:
        SetLayout(&elements[0], "POSITION",  0, DXGI_FORMAT_R32G32_FLOAT, 0, 0,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        numelements = 1;
        break;
    default:
        assert(0);
        break;
    }

    ID3D11InputLayout* layout = NULL;
    HRESULT hr = S_OK;
    if(numelements > 0)
    {        
        hr = s_device->CreateInputLayout( elements, numelements, code, codeSize, &layout );
        Logger::IsFailureLog(hr, L"CreateInputLayout");
    }
    return layout;
}

// ========================== shader creation functions =========================
ID3D11VertexShader*   GpuResourceFactory::CreateVertexShader(void* code, uint32_t codeSize)
{
    assert(code && codeSize > 0);
    ID3D11VertexShader * shader = NULL;
    if(code && codeSize > 0)
    {
        HRESULT hr = s_device->CreateVertexShader( code, codeSize, NULL, &shader );
        Logger::IsFailureLog(hr, L"CreateVertexShader");
    }
    return shader;
}

ID3D11PixelShader*    GpuResourceFactory::CreatePixelShader(void* code, uint32_t codeSize)
{
    assert(code && codeSize > 0);
    ID3D11PixelShader * shader = NULL;
    if(code && codeSize > 0)
    {
        HRESULT hr = s_device->CreatePixelShader( code, codeSize, NULL, &shader );
        Logger::IsFailureLog(hr, L"CreatePixelShader");
    }
    return shader;
}

ID3D11GeometryShader* GpuResourceFactory::CreateGeometryShader(void* code, uint32_t codeSize)
{
    assert(code && codeSize > 0);
    ID3D11GeometryShader* shader = NULL;
    if(code && codeSize > 0)
    {
        HRESULT hr = s_device->CreateGeometryShader( code, codeSize, NULL, &shader );
        Logger::IsFailureLog(hr, L"CreateGeometryShader");
    }
    return shader;

}


// ==================== texture creation functions. ===========

ID3D11Texture2D* GpuResourceFactory::CreateDxTexture2D(void* buff, int w, int h, bool cubemap)
{    
    assert(buff);
    ID3D11Texture2D *tex = NULL;       
    D3D11_TEXTURE2D_DESC tdesc;
    tdesc.Width = w;
    tdesc.Height = h;
    tdesc.MipLevels = 1;
    tdesc.ArraySize = 1;
    tdesc.SampleDesc.Count = 1;
    tdesc.SampleDesc.Quality = 0;
    tdesc.Usage = D3D11_USAGE_DEFAULT;
    tdesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
    //
    tdesc.BindFlags = D3D11_BIND_SHADER_RESOURCE;
    tdesc.CPUAccessFlags = 0;
    tdesc.MiscFlags = 0;

    if(cubemap)
    {
         tdesc.MiscFlags |= D3D11_RESOURCE_MISC_TEXTURECUBE;
         tdesc.ArraySize = 6;
    }

    int numSubRC = tdesc.ArraySize * tdesc.MipLevels;
    D3D11_SUBRESOURCE_DATA*  subData = new D3D11_SUBRESOURCE_DATA[numSubRC];
    uint8_t* ptr = (uint8_t*)buff;
    uint32_t rowPitch = w * 4;
    uint32_t slicePitch = rowPitch * h;

    for(int index = 0; index <numSubRC; index++)
    {        
        subData[index].pSysMem = ptr;
        subData[index].SysMemPitch = rowPitch;
        subData[index].SysMemSlicePitch = slicePitch;
        ptr += slicePitch;
    }
    
    HRESULT hr = s_device->CreateTexture2D(&tdesc,subData,&tex);
    delete[] subData;    
    Logger::IsFailureLog(hr, L"CreateTexture2D");
    return tex;

}

//
//
//static DXGI_FORMAT GetSRGBFormat(DXGI_FORMAT format)
//{
//
//    switch(format)
//    {
//    case DXGI_FORMAT_R8G8B8A8_TYPELESS:
//    case DXGI_FORMAT_R8G8B8A8_UNORM:
//        return DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;
//
//    case DXGI_FORMAT_BC1_TYPELESS:
//    case DXGI_FORMAT_BC1_UNORM:
//        return DXGI_FORMAT_BC1_UNORM_SRGB;
//
//    case DXGI_FORMAT_BC2_TYPELESS:
//    case DXGI_FORMAT_BC2_UNORM:
//        return DXGI_FORMAT_BC2_UNORM_SRGB;
//
//    case DXGI_FORMAT_BC7_TYPELESS:
//    case DXGI_FORMAT_BC7_UNORM:
//        return DXGI_FORMAT_BC7_UNORM_SRGB;
//
//    case DXGI_FORMAT_BC3_TYPELESS:
//    case DXGI_FORMAT_BC3_UNORM:
//        return DXGI_FORMAT_BC3_UNORM_SRGB;
//
//    case DXGI_FORMAT_B8G8R8A8_TYPELESS:
//    case DXGI_FORMAT_B8G8R8A8_UNORM:
//        return DXGI_FORMAT_B8G8R8A8_UNORM_SRGB;
//
//    case DXGI_FORMAT_B8G8R8X8_TYPELESS:
//    case DXGI_FORMAT_B8G8R8X8_UNORM:
//        return DXGI_FORMAT_B8G8R8X8_UNORM_SRGB;
//
//    }
//    return format;
//}


ID3D11ShaderResourceView* GpuResourceFactory::CreateTextureView(ID3D11Texture2D *tex)
{
    ID3D11ShaderResourceView* view = NULL;
    assert(tex);

    if(tex)
    {
        D3D11_TEXTURE2D_DESC desc;
        D3D11_RESOURCE_DIMENSION type;
        tex->GetType(&type);
        tex->GetDesc(&desc);
        bool isCubeMap =  (desc.MiscFlags & D3D11_RESOURCE_MISC_TEXTURECUBE) != 0;
        
        D3D11_SHADER_RESOURCE_VIEW_DESC SRVDesc;
        memset( &SRVDesc, 0, sizeof( SRVDesc ) );
        SRVDesc.Format = desc.Format;
        

        if (isCubeMap)
        {

            if (desc.ArraySize > 6)
            {
                SRVDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURECUBEARRAY;
                SRVDesc.TextureCubeArray.MipLevels = desc.MipLevels;                
                SRVDesc.TextureCubeArray.NumCubes = ( desc.ArraySize / 6 );
            }
            else
            {
                SRVDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURECUBE;
                SRVDesc.TextureCube.MipLevels = desc.MipLevels;
            }
         }
         else if (desc.ArraySize > 1)
         {
             SRVDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2DARRAY;
             SRVDesc.Texture2DArray.MipLevels = desc.MipLevels;
             SRVDesc.Texture2DArray.ArraySize = desc.ArraySize;
         }
         else
         {
             SRVDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D;
             SRVDesc.Texture2D.MipLevels = desc.MipLevels;
         }
         HRESULT hr = s_device->CreateShaderResourceView( tex, &SRVDesc, &view );
         Logger::IsFailureLog(hr, L"CreateShaderResourceView");
    }
    return view;
}



//-------------------------------------------------------------------------------------------------
static uint32_t GetSizeInBytes(VertexFormatEnum vf)
{   
    uint32_t size = 0;
    switch(vf)
    {    
    case VertexFormat::VF_P:
        size = sizeof(float3);
        break;

    case VertexFormat::VF_PC:
        size = sizeof(float3) + sizeof(float4);
        break;

    case VertexFormat::VF_PN:
        size = sizeof(float3) + sizeof(float3);
        break;  

    case VertexFormat::VF_PT:
		size = sizeof(float3) + sizeof(float2);
        break; 

    case VertexFormat::VF_PTC:
		size = sizeof(float3) + sizeof(float2) + sizeof(float4);
        break; 

    case VertexFormat::VF_PNT:
        size = sizeof(float3) + sizeof(float3) + sizeof(float2);
        break;    

    case VertexFormat::VF_PNTT:  // pos, norm, tex0, tangent
        size = sizeof(float3) + sizeof(float3) + sizeof(float2) + sizeof(float3);
        break;  

    case VertexFormat::VF_T:
        size = sizeof(float2);
        break;  

     default: assert(0); break;
    }
    return size;
}

