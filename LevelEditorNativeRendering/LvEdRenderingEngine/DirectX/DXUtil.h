//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <stdint.h>
#include <D3D11.h>
#include "../Core/WinHeaders.h"
#include "../Core/Utils.h"
#include "../core/Logger.h"
#include "DirectXTex/DirectXTex.h"
#include <assert.h>

class DXUtil
{
public:

    // load texture  memory.
    static HRESULT LoadTexture(const wchar_t* file, DirectX::TexMetadata* metadata, DirectX::ScratchImage& image);
   
    // Gets wic codec from file extension
    // extension must be in lower case.
    static REFGUID GetWICCodecFromFileExtension(const wchar_t* extension);
    
    inline static void SetDebugName( ID3D11DeviceChild* pObj, const CHAR* pstrName )
    {
        #if defined(PROFILE) || defined(DEBUG) || defined(_DEBUG)
        if ( pObj )
            pObj->SetPrivateData( WKPDID_D3DDebugObjectName, lstrlenA(pstrName), pstrName );
        #endif
    }
};


