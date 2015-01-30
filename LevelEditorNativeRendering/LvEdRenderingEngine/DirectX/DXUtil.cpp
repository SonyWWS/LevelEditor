//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "DXUtil.h"
#include <D3D11.h>
#include "../Core/Utils.h"
#include "../Core/FileUtils.h"
#include "../Core/Logger.h"

using namespace DirectX;
using namespace LvEdEngine;

HRESULT DXUtil::LoadTexture(const wchar_t* file, TexMetadata* metadata, DirectX::ScratchImage& image)
{
    HRESULT hr = S_OK;
    std::wstring ext = FileUtils::GetExtensionLower(file);
    if(!FileUtils::Exists(file)) return E_INVALIDARG;

    if(ext == L".dds")
    {
        hr = LoadFromDDSFile( file, DDS_FLAGS_FORCE_RGB,metadata, image );

    }
    else if(ext == L".tga")
    {
        hr = LoadFromTGAFile(file,metadata,image);
    }
    else if(ext == L".bmp"
         || ext == L".png"
         || ext == L".jpg" || ext == L".jpeg"
         || ext == L".tif" || ext == L".tiff")
    {
        hr = LoadFromWICFile(file,WIC_FLAGS_FORCE_RGB,metadata,image);
    }
    else
    {
        hr = E_INVALIDARG;
    }


    return hr;
}

// Gets wic codec from file extension
REFGUID DXUtil::GetWICCodecFromFileExtension(const wchar_t* extension)
{
    int  wicCodec = (DirectX::WIC_CODEC_ICO+1);
    if(_wcsnicmp(extension,L".bmp",MAX_PATH) == 0)
    {
        wicCodec = DirectX::WIC_CODEC_BMP;            
    }
    else if(_wcsnicmp(extension,L".png",MAX_PATH) == 0)
    {
        wicCodec = DirectX::WIC_CODEC_PNG;
    } 
    else if(_wcsnicmp(extension,L".jpg",MAX_PATH) == 0
        || _wcsnicmp(extension,L".jpeg",MAX_PATH) == 0)
    {
        wicCodec = DirectX::WIC_CODEC_JPEG;
    }
    else if(_wcsnicmp(extension,L".tif",MAX_PATH) == 0
        || _wcsnicmp(extension,L".tiff",MAX_PATH) == 0)
    {
        wicCodec = DirectX::WIC_CODEC_TIFF;
    }        
    return DirectX::GetWICCodec((DirectX::WICCodecs)wicCodec);
}


