//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include <d3dx11.h>
#include <DxErr.h>
#include <D3Dcompiler.h>


#include "../Core/NonCopyable.h"
#include "RenderUtil.h"
#include "RenderBuffer.h"
#include "Texture.h"
//#include <strsafe.h>
#include <set>
#include "Model.h"
#include "Lights.h"
#include "../Core/Logger.h"

namespace LvEdEngine
{

    // loads embedded shader
    // shaderName is the embedeed resource name
    // the caller have to free the returned pointer.
    static void LoadEmbeddedShader(const wchar_t* shaderName, const void **ppData, UINT* pBytes);
   
    // custom include handler 
    // helps shader compiler to  resolve #include as embedded resources 
    class IncludeHandler : public ID3DInclude, public NonCopyable
    {
        public:
        STDMETHOD(Open)(
            D3D_INCLUDE_TYPE /*IncludeType*/,
            LPCSTR pFileName,
            LPCVOID /*pParentData*/,
            LPCVOID *ppData,
            UINT *pBytes)
        {                       
            WCHAR wfile[MAX_PATH];
            MultiByteToWideChar(0, 0, pFileName, MAX_PATH, wfile, MAX_PATH );
            LoadEmbeddedShader(wfile,ppData,pBytes);                                 
            return (*pBytes > 0)? S_OK : E_INVALIDARG;

        }
        STDMETHOD(Close)( LPCVOID pData)
        {
            if(pData)
            {                
                free((void*)pData);
            }          
            return S_OK;
        }
    };

//-------------------------------------------------------------------------------------------------
void ConvertColor(int color, float4* out)
{
    *out = float4((float)((color>>16) & 0xFF)/255.0f, (float)((color>>8) & 0xFF)/255.0f, (float)((color>>0) & 0xFF)/255.0f, (float)((color>>24) & 0xFF)/255.0f );
}

//-------------------------------------------------------------------------------------------------
void ConvertColor( int color , float3* out)
{
    float4 lcolor;
    ConvertColor(color, &lcolor);
    *out = float3(lcolor.x, lcolor.y, lcolor.z);
}

//-------------------------------------------------------------------------------------------------
uint32_t GetSizeInBytes(VertexFormatEnum vf)
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


//-------------------------------------------------------------------------------------------------
uint32_t GetVerticesPerPrimitive(PrimitiveTypeEnum pt)
{
    // todo use array instead of multipe if-statements.
    uint32_t vertexCount = 0;
    switch(pt)
    {
    default: 
        assert(0); 
        break;
    case PrimitiveType::LineList:
        vertexCount = 2;
        break;
    case PrimitiveType::LineStrip:
        vertexCount = 1;
        break;
    case PrimitiveType::TriangleList:
        vertexCount = 3;
        break;
    case PrimitiveType::TriangleStrip:
        vertexCount = 1;
        break;
    }

    return vertexCount;
}

//-----------------------------------------------------

ID3DBlob* CompileShaderFromResource(LPCWSTR resourceName, LPCSTR szEntryPoint, LPCSTR szShaderModel, const D3D_SHADER_MACRO *shaderMacros)
{
    wchar_t ResName[MAX_PATH];
    size_t len = wcslen(resourceName);
    
    for(unsigned int c =0; c < len; c++) { ResName[c] = towupper(resourceName[c]); }
    ResName[len] = L'\0';

    HMODULE handle = GetDllModuleHandle();      
    HRSRC resInfo =  FindResource(handle, ResName,L"SHADER");
    if(resInfo == NULL)
    {       
        Logger::Log(OutputMessageType::Error, L"Could not find specified shader in resource.rc: %s\n", resourceName);
        return NULL;
    }

    HGLOBAL hRes = LoadResource(handle, resInfo);
    if(hRes == NULL)
    {
        Logger::Log(OutputMessageType::Error, L"Failed to load specified shader from resource.rc: %s\n", resourceName);
        return NULL;
    }
    DWORD resLen =  SizeofResource(handle, resInfo);
    const char* data = static_cast<const char*>(LockResource(hRes));
    char* code = new char[resLen+1];
    CopyMemory(code,data,resLen);
    
    code[resLen] = 0;


    char shaderName[MAX_PATH];    
    WideCharToMultiByte(
                       CP_ACP,       //__in UINT     CodePage,
                       0,            //__in DWORD    dwFlags,
                       resourceName, // __in_ecount(cchWideChar) LPCWSTR  lpWideCharStr,
                        -1,          // __in int      cchWideChar,
                        shaderName,  // __out_bcount_opt(cbMultiByte) __transfer(lpWideCharStr) LPSTR   lpMultiByteStr,
                        MAX_PATH,    //__in int      cbMultiByte,
                        NULL,        //__in_opt LPCSTR   lpDefaultChar,
                        NULL         //__out_opt LPBOOL  lpUsedDefaultChar
                        );        
    ID3DBlob* blob = CompileShaderFromString(shaderName,code, szEntryPoint, szShaderModel, shaderMacros);
    delete[] code;
    return blob;
}

//-------------------------------------------------------------------------------------------------
ID3DBlob* CompileShaderFromString(const char * shaderName, const char* szCode, LPCSTR szEntryPoint, LPCSTR szShaderModel, const D3D_SHADER_MACRO *shaderMacros)
{
	HRESULT hr = S_OK;

	 DWORD32 dwShaderFlags = D3DCOMPILE_ENABLE_STRICTNESS;
#if defined( DEBUG ) || defined( _DEBUG )  
    dwShaderFlags |= D3DCOMPILE_DEBUG;
#else
     dwShaderFlags |= D3DCOMPILE_OPTIMIZATION_LEVEL3;     
#endif


    
    ID3DBlob* pErrorBlob = NULL;
    ID3DBlob* pCompiledCode = NULL;
    IncludeHandler incHandler;
	hr = D3DCompile(
                (void*)szCode,   //__in   LPCVOID pSrcData,
				strlen(szCode),  //__in   SIZE_T SrcDataSize,
				shaderName,	      //__in   LPCSTR pSourceName,
				shaderMacros,     //  __in   const D3D_SHADER_MACRO *pDefines,
				&incHandler,   // __in   ID3DInclude pInclude,
				szEntryPoint,  // __in   LPCSTR pEntrypoint,
				szShaderModel, //  __in   LPCSTR pTarget,
				dwShaderFlags,  //  __in   UINT Flags1,
				0,              //  __in   UINT Flags2,
				&pCompiledCode,     //  __out  ID3DBlob *ppCode,
				&pErrorBlob//  __out  ID3DBlob *ppErrorMsgs
				);

	
    if( FAILED(hr) )
    {
        if( pErrorBlob != NULL )
			Logger::Log(OutputMessageType::Error, "Shader Error: %s\n",(char*)pErrorBlob->GetBufferPointer());
    }
    else
    {
        if( pErrorBlob != NULL )
			Logger::Log(OutputMessageType::Warning, "Shader Warning: %s\n",(char*)pErrorBlob->GetBufferPointer());
    }

    if( pErrorBlob ) pErrorBlob->Release();
	return pCompiledCode;
}

//-------------------------------------------------------------------------------------------------
ID3D11VertexShader* CreateVertexShader(ID3D11Device* device, ID3DBlob* blob)
{
    assert(blob);
    ID3D11VertexShader * shader = NULL;
    if(blob)
    {
        HRESULT hr = device->CreateVertexShader( blob->GetBufferPointer(), blob->GetBufferSize(), NULL, &shader );
        Logger::IsFailureLog(hr, L"CreateVertexShader");
    }
    return shader;
}

ID3D11GeometryShader* CreateGeometryShader(ID3D11Device* device, ID3DBlob* blob)
{
    assert(blob);
    ID3D11GeometryShader* shader = NULL;
    if(blob)
    {
        HRESULT hr = device->CreateGeometryShader( blob->GetBufferPointer(), blob->GetBufferSize(), NULL, &shader );
        Logger::IsFailureLog(hr, L"CreateGeometryShader");
    }
    return shader;
}
//-------------------------------------------------------------------------------------------------
ID3D11PixelShader* CreatePixelShader(ID3D11Device* device, ID3DBlob* blob)
{
    assert(blob);
    ID3D11PixelShader * shader = NULL;
    if(blob)
    {
        HRESULT hr = device->CreatePixelShader( blob->GetBufferPointer(), blob->GetBufferSize(), NULL, &shader );
        Logger::IsFailureLog(hr, L"CreatePixelShader");
    }
    return shader;
}


//-------------------------------------------------------------------------------------------------
IndexBuffer* CreateIndexBuffer(ID3D11Device* device, uint32_t* buffer, uint32_t indexCount, D3D11_USAGE usage)
{
    HRESULT hr = S_OK;

    UINT cpuAccess = 0;
    if(usage == D3D11_USAGE_DYNAMIC )
    {
        cpuAccess = D3D11_CPU_ACCESS_WRITE;
    }

	D3D11_BUFFER_DESC bufDcr;
    SecureZeroMemory( &bufDcr, sizeof(bufDcr));
    bufDcr.Usage = usage;
    bufDcr.ByteWidth = sizeof( DWORD32 ) * indexCount;
    bufDcr.BindFlags = D3D11_BIND_INDEX_BUFFER;
	bufDcr.CPUAccessFlags = cpuAccess;
    bufDcr.MiscFlags = 0;
    bufDcr.StructureByteStride = 0;

    D3D11_SUBRESOURCE_DATA bufData;
	SecureZeroMemory( &bufData, sizeof(bufData) );
    bufData.pSysMem = buffer;

    ID3D11Buffer* gpuBuffer = NULL; 
    hr = device->CreateBuffer( &bufDcr, &bufData, &gpuBuffer );
    if (Logger::IsFailureLog(hr, L"CreateBuffer"))
    {
        return 0;
    }

    IndexBuffer* ib = new IndexBuffer(gpuBuffer, indexCount);
    return ib;
}

//
//-------------------------------------------------------------------------------------------------
VertexBuffer* CreateVertexBuffer(ID3D11Device* device, VertexFormatEnum vf, void* buffer, uint32_t vertexCount, D3D11_USAGE usage)
{
    uint32_t vertexSize = GetSizeInBytes(vf);

    HRESULT hr = S_OK;
    UINT cpuAccess = 0;
    if(usage == D3D11_USAGE_DYNAMIC )
    {
        cpuAccess = D3D11_CPU_ACCESS_WRITE;
    }

	D3D11_BUFFER_DESC bufDcr;
	SecureZeroMemory( &bufDcr, sizeof(bufDcr));
    bufDcr.Usage = usage;
    bufDcr.ByteWidth =   vertexSize * vertexCount;
    bufDcr.BindFlags = D3D11_BIND_VERTEX_BUFFER;
	bufDcr.CPUAccessFlags = cpuAccess;
    bufDcr.MiscFlags = 0;
    bufDcr.StructureByteStride = 0;

    D3D11_SUBRESOURCE_DATA bufData;
	SecureZeroMemory( &bufData, sizeof(bufData) );
    bufData.pSysMem = buffer;
    D3D11_SUBRESOURCE_DATA* pbufData = (buffer)? &bufData : NULL;

    ID3D11Buffer* gpuBuffer = NULL;     
    hr = device->CreateBuffer( &bufDcr, pbufData, &gpuBuffer );
	
    if (Logger::IsFailureLog(hr, L"CreateBuffer"))
	{
		return 0;	 
	}
    VertexBuffer* vb = new VertexBuffer(gpuBuffer, vertexCount, vf);
    return vb;
}

//-------------------------------------------------------------------------------------------------
ID3D11Buffer* CreateConstantBuffer(ID3D11Device* device, uint32_t sizeInBytes)
{
    HRESULT hr = S_OK;    
    ID3D11Buffer* buffer = NULL;
    D3D11_BUFFER_DESC desc;
    SecureZeroMemory( &desc, sizeof(desc));
	desc.Usage = D3D11_USAGE_DYNAMIC;
	desc.BindFlags = D3D11_BIND_CONSTANT_BUFFER;
	desc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	desc.MiscFlags = 0;  
	desc.ByteWidth = sizeInBytes;
	desc.StructureByteStride = 0;	
    hr = device->CreateBuffer(&desc, 0, &buffer );
    LvEdEngine::Logger::IsFailureLog(hr,L"Failed to create constant buffer");	
    return buffer;    
}

//-------------------------------------------------------------------------------------------------
bool UpdateIndexBuffer(ID3D11DeviceContext* context, IndexBuffer* buffer, uint32_t* data, uint32_t count)
{
    HRESULT result;
    D3D11_MAPPED_SUBRESOURCE mappedResource;
    if(buffer->GetCount() != count)
    {
        return false;
    }
    result = context->Map(buffer->GetBuffer(), 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
    if( FAILED(result) )
    {
        return false;
    }

    CopyMemory(mappedResource.pData, (void*)data, (sizeof(uint32_t) * count));
    context->Unmap(buffer->GetBuffer(), 0);
    return true;
}

bool UpdateVertexBuffer(ID3D11DeviceContext* context, VertexBuffer* buffer, void* data, uint32_t count)
{    
    assert(count <= buffer->GetCount());
    HRESULT hr;
    D3D11_MAPPED_SUBRESOURCE mappedResource;
    hr = context->Map(buffer->GetBuffer(), 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
    if(SUCCEEDED(hr))
    {
        CopyMemory(mappedResource.pData, data, buffer->GetStride() * count);
        context->Unmap(buffer->GetBuffer(), 0);
        return true;
    }
    return false;
}
//-------------------------------------------------------------------------------------------------
bool UpdateVertexBuffer(ID3D11DeviceContext* context, VertexBuffer* buffer, VertexFormatEnum vf, void* data, uint32_t count)
{
    HRESULT result;
    D3D11_MAPPED_SUBRESOURCE mappedResource;
    if(buffer->GetFormat() != vf)
    {
        return false;
    }
    if(buffer->GetCount() != count)
    {
        return false;
    }
    buffer->GetStride();
    result = context->Map(buffer->GetBuffer(), 0, D3D11_MAP_WRITE_DISCARD, 0, &mappedResource);
    if( FAILED(result) )
    {
        return false;
    }


    CopyMemory(mappedResource.pData, (void*)data, (buffer->GetStride() * count));
    context->Unmap(buffer->GetBuffer(), 0);
    return true;
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

ID3D11InputLayout* CreateInputLayout(ID3D11Device* device, ID3DBlob* shaderBlob, VertexFormatEnum vf)
{
    D3D11_INPUT_ELEMENT_DESC layout[10];
    uint32_t numelements = 0;
    switch(vf)
    {    
    case VertexFormat::VF_P:
        SetLayout(&layout[0], "POSITION",  0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        numelements = 1;
        break;
    case VertexFormat::VF_PC:
        SetLayout(&layout[0], "POSITION",  0, DXGI_FORMAT_R32G32B32_FLOAT, 0,  0,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&layout[1], "COLOR",     0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        numelements = 2;
        break;
    case VertexFormat::VF_PTC:
        SetLayout(&layout[0], "POSITION",  0, DXGI_FORMAT_R32G32B32_FLOAT, 0,  0,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&layout[1], "TEXCOORD",  0, DXGI_FORMAT_R32G32_FLOAT,    0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&layout[2], "COLOR",     0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        numelements = 3;
        break;
    case VertexFormat::VF_PN:
        SetLayout(&layout[0], "POSITION",  0, DXGI_FORMAT_R32G32B32_FLOAT, 0,  0,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&layout[1], "NORMAL",    0, DXGI_FORMAT_R32G32B32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        numelements = 2;
        break;
    case VertexFormat::VF_PT:
        SetLayout(&layout[0], "POSITION",  0, DXGI_FORMAT_R32G32B32_FLOAT, 0,  0,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&layout[1], "TEXCOORD",  0, DXGI_FORMAT_R32G32_FLOAT,    0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        numelements = 2;
        break;
    case VertexFormat::VF_PNT:
        SetLayout(&layout[0], "POSITION",  0, DXGI_FORMAT_R32G32B32_FLOAT, 0,  0,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&layout[1], "NORMAL",    0, DXGI_FORMAT_R32G32B32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&layout[2], "TEXCOORD",  0, DXGI_FORMAT_R32G32_FLOAT,    0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        numelements = 3;
        break;
    case VertexFormat::VF_PNTT:
        SetLayout(&layout[0], "POSITION",  0, DXGI_FORMAT_R32G32B32_FLOAT, 0,  0,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&layout[1], "NORMAL",    0, DXGI_FORMAT_R32G32B32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&layout[2], "TEXCOORD",  0, DXGI_FORMAT_R32G32_FLOAT,    0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        SetLayout(&layout[3], "TANGENT",   0, DXGI_FORMAT_R32G32B32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT,  D3D11_INPUT_PER_VERTEX_DATA, 0);
        numelements = 4;
        break;   

    default:
        assert(0);
        break;
    }

    return CreateInputLayout(device, shaderBlob, layout, numelements);
}


//-------------------------------------------------------------------------------------------------
ID3D11InputLayout* CreateInputLayout(ID3D11Device* device, ID3DBlob* shaderBlob, const D3D11_INPUT_ELEMENT_DESC* elements, uint32_t numElements)
{
    HRESULT hr = S_OK;
    ID3D11InputLayout* layout = NULL;
    hr = device->CreateInputLayout( elements, numElements, shaderBlob->GetBufferPointer(), shaderBlob->GetBufferSize(), &layout );
    Logger::IsFailureLog(hr, L"CreateInputLayout");
    return layout;
}

//-------------------------------------------------------------------------------------------------
ID3D11RasterizerState* CreateRasterState(ID3D11Device* device, FillModeEnum fillMode)
{
    HRESULT hr = S_OK;
    ID3D11RasterizerState* rasterState = NULL;
	D3D11_RASTERIZER_DESC rsDcr;
	SecureZeroMemory( &rsDcr, sizeof(rsDcr));
	rsDcr.CullMode =  D3D11_CULL_BACK;
	rsDcr.FillMode =  fillMode==FillMode::Wireframe ? D3D11_FILL_WIREFRAME : D3D11_FILL_SOLID;
	rsDcr.FrontCounterClockwise = true;
    rsDcr.DepthClipEnable = true;
	rsDcr.AntialiasedLineEnable = false;
	rsDcr.MultisampleEnable = true;
    if(fillMode == FillMode::Wireframe)
    {
         rsDcr.DepthBias = -1;
         rsDcr.DepthBiasClamp = -0.000005f;
    }
		
	device->CreateRasterizerState(&rsDcr,  &rasterState);
    Logger::IsFailureLog(hr, L"CreateRasterizerState");
    return rasterState;
}


// ----------------------------------------------------------------------------------------------
ID3D11Texture2D * CreateDxTexture2D(ID3D11Device* device, uint32_t* buff, int w, int h, bool cubemap)
{
    assert(device);
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
    uint32_t slicePitch = w * 4 * h;

    for(int index = 0; index <numSubRC; index++)
    {        
        subData[index].pSysMem = ptr;
        subData[index].SysMemPitch = rowPitch;
        subData[index].SysMemSlicePitch = slicePitch;
        ptr += slicePitch;
    }
    
    HRESULT hr = device->CreateTexture2D(&tdesc,subData,&tex);
    delete[] subData;
    //device->createtex
    Logger::IsFailureLog(hr, L"CreateTexture2D");
    return tex;

}

// ----------------------------------------------------------------------------------------------
ID3D11ShaderResourceView * CreateTextureView(ID3D11Device* device, ID3D11Texture2D *tex)
{
    ID3D11ShaderResourceView * view = NULL;
    assert(device);
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
         HRESULT hr = device->CreateShaderResourceView( tex, &SRVDesc, &view );
         Logger::IsFailureLog(hr, L"CreateShaderResourceView");
    }

    return view;
}



// ----------------------------------------------------------------------------------------------
ID3D11SamplerState* CreateSamplerState(ID3D11Device* device)
{
    // Create a sampler state
    ID3D11SamplerState* sampler = NULL;
    D3D11_SAMPLER_DESC SamDesc;
    SamDesc.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
    SamDesc.AddressU = D3D11_TEXTURE_ADDRESS_WRAP;
    SamDesc.AddressV = D3D11_TEXTURE_ADDRESS_WRAP;
    SamDesc.AddressW = D3D11_TEXTURE_ADDRESS_WRAP;
    SamDesc.MipLODBias = 0.0f;
    SamDesc.MaxAnisotropy = 1;
    SamDesc.ComparisonFunc = D3D11_COMPARISON_ALWAYS;
    SamDesc.BorderColor[0] = SamDesc.BorderColor[1] = SamDesc.BorderColor[2] = SamDesc.BorderColor[3] = 0;
    SamDesc.MinLOD = 0;
    SamDesc.MaxLOD = D3D11_FLOAT32_MAX;
    HRESULT hr = device->CreateSamplerState( &SamDesc, &sampler );
    Logger::IsFailureLog(hr, L"CreateSamplerState");
    return sampler;
}

// ----------------------------------------------------------------------------------------------
static void LoadEmbeddedShader(const wchar_t* shaderName, const void **ppData, UINT* pBytes)
{
    *ppData = NULL;
    *pBytes = 0;

    wchar_t ResName[MAX_PATH];
    size_t len = wcslen(shaderName);

    for(unsigned int c =0; c < len; c++) { ResName[c] = towupper(shaderName[c]); }
    ResName[len] = L'\0';

    HMODULE handle = GetDllModuleHandle();      
    HRSRC resInfo =  FindResource(handle, ResName,L"SHADER");
    if(resInfo == NULL)
    {
        // print error msg.
        return;
    }

    HGLOBAL hRes = LoadResource(handle, resInfo);
    if(hRes == NULL)
    {
        // use GetLastError() to print errror msg
        return;
    }

    DWORD resLen =  SizeofResource(handle, resInfo);
    *pBytes = resLen;
    const char* data = static_cast<const char*>(LockResource(hRes));
    char* buffer = (char*)malloc(resLen+1);
    ::CopyMemory(buffer,data,resLen);
    buffer[resLen] = '\0';
    *ppData = buffer;        
}
    
}; // namespace
