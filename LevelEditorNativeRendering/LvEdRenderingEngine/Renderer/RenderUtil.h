//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <vector>
#include "../Core/WinHeaders.h"
#include "../Core/Utils.h"
#include "../VectorMath/V3dMath.h"
#include "../VectorMath/CollisionPrimitives.h"
#include "RenderEnums.h"
#include <d3d11.h>

namespace LvEdEngine
{
    class Texture;
    class VertexBuffer;
    class IndexBuffer;
    class Mesh;
    class BoxLight;
    class PointLight;
    class DirLight;
    struct LightEnvironment;


    // COLOR
    void ConvertColor( int color, float4* out );
    void ConvertColor( int color, float3* out );

    // BUFFERS
    uint32_t GetSizeInBytes(VertexFormatEnum vf);
    uint32_t GetVerticesPerPrimitive(PrimitiveTypeEnum pt);

    // SHADERS
    ID3DBlob* CompileShaderFromResource(LPCWSTR resourceName, LPCSTR szEntryPoint, LPCSTR szShaderModel, const D3D_SHADER_MACRO *shaderMacros);    
    ID3D11VertexShader* CreateVertexShader(ID3D11Device* device, ID3DBlob* pVSBlob);
    ID3D11GeometryShader* CreateGeometryShader(ID3D11Device* device, ID3DBlob* blob);
    ID3D11PixelShader* CreatePixelShader(ID3D11Device* device, ID3DBlob* pVSBlob);

    // BUFFERS
    ID3D11Buffer* CreateConstantBuffer(ID3D11Device* device, uint32_t sizeInBytes);
    IndexBuffer* CreateIndexBuffer(ID3D11Device* device, uint32_t* buffer, uint32_t indexCount, D3D11_USAGE usage = D3D11_USAGE_DEFAULT);
    
    VertexBuffer* CreateVertexBuffer(ID3D11Device* device, VertexFormatEnum vf, void* buffer, uint32_t vertexCount, D3D11_USAGE usage = D3D11_USAGE_DEFAULT);
    bool UpdateIndexBuffer(ID3D11DeviceContext* context, IndexBuffer* buffer, uint32_t* data, uint32_t count);
    bool UpdateVertexBuffer(ID3D11DeviceContext* context, VertexBuffer* buffer, void* data, uint32_t count);
    void UpdateConstantBuffer(ID3D11DeviceContext* dc, ID3D11Buffer* buffer,void *data, uint32_t size);

    // MISC
    ID3D11InputLayout* CreateInputLayout(ID3D11Device* device, ID3DBlob* shaderBlob, VertexFormatEnum vf);
    ID3D11InputLayout* CreateInputLayout(ID3D11Device* device, ID3DBlob* shaderBlob, const D3D11_INPUT_ELEMENT_DESC* elements, uint32_t numElements);
    ID3D11RasterizerState* CreateRasterState(ID3D11Device* device, FillModeEnum fillMode);

    // TEXTURES
    
    ID3D11Texture2D * CreateDxTexture2D(ID3D11Device* device, uint32_t* buff, int w, int h, bool cubemap = false );
    ID3D11ShaderResourceView* CreateTextureView(ID3D11Device* device, ID3D11Texture2D *tex);
    ID3D11SamplerState* CreateSamplerState(ID3D11Device* device);
       

     // Use DXUT_SetDebugName() to attach names to D3D objects for use by 
    // SDKDebugLayer, PIX's object table, etc.
    #if defined(PROFILE) || defined(DEBUG) || defined(_DEBUG)  
    inline void Render_SetDebugName( ID3D11DeviceChild* pObj, const CHAR* pstrName )
    {
        if ( pObj )
            pObj->SetPrivateData( WKPDID_D3DDebugObjectName, lstrlenA(pstrName), pstrName );
    }
    #else
    #define Render_SetDebugName( pObj, pstrName )
    #endif
};
