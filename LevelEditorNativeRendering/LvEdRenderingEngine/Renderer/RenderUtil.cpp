//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "RenderUtil.h"
#include <D3Dcompiler.h>
#include "../Core/NonCopyable.h"
#include "RenderBuffer.h"
#include "Texture.h"
#include <set>
#include "Model.h"
#include "Lights.h"
#include "../Core/ResUtil.h"
#include "../Core/Logger.h"

namespace LvEdEngine
{
   
    // custom include handler 
    // helps shader compiler to resolve #include as embedded resources 
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
            *ppData = ResUtil::LoadResource(L"SHADER",wfile,pBytes);            
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

//-----------------------------------------------------

ID3DBlob* CompileShaderFromString(const char* shaderName,
    void* code,
    uint32_t codeSize,
    LPCSTR szEntryPoint, 
    LPCSTR szShaderModel, 
    const D3D_SHADER_MACRO *shaderMacros)
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
                code,   //__in   LPCVOID pSrcData,
				codeSize,  //__in   SIZE_T SrcDataSize,
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

ID3DBlob* CompileShaderFromResource(LPCWSTR resourceName, LPCSTR szEntryPoint, LPCSTR szShaderModel, const D3D_SHADER_MACRO *shaderMacros)
{    
    uint32_t resLen = 0;
    void* code = ResUtil::LoadResource(L"SHADER",resourceName,&resLen);

    
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

    ID3DBlob* blob = CompileShaderFromString(shaderName,
        code, 
        resLen,
        szEntryPoint,
        szShaderModel,
        shaderMacros);
    free(code);
    return blob;
}    
}; // namespace
