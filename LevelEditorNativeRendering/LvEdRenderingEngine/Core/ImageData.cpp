//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "ImageData.h"
#include <stdlib.h>
#include <memory.h>
#include <assert.h>
#include "../DirectX/DXUtil.h"
#include "../DirectX/DirectXTex/DirectXTex.h"
#include "FileUtils.h"
#include "StringUtils.h"


using namespace DirectX;

namespace LvEdEngine
{

ImageData::ImageData()
{
    m_buffer = NULL;
    Reset();    
}


void ImageData::Invoke(wchar_t* fn, const void* arg, void** retVal)
{
     if(fn == NULL || wcslen(fn) == 0) return;
     if( wcscmp(fn,L"CreateNew") == 0)
     {
        // size_t argsize = 3 * sizeof(int32_t);
         assert(arg);
         int32_t* args = (int32_t*) arg;
         CreateNew(args[0],args[1],(uint32_t)args[2]);
     }
     else if(StrUtils::Equal(fn,L"SaveToFile"))
     {
         assert(arg);
         wchar_t* file = (wchar_t*)arg;
         SaveToFile(file);
     }
}

static const uint64_t szLimit  = 16i64 * 1024i64 * 1024i64 * 1024i64; 

void ImageData::CreateNew(int32_t width, int32_t height, uint32_t format)
{
     

    //validate args
    int32_t bytesPP  = (int32_t) DirectX::BitsPerPixel((DXGI_FORMAT)format) / 8;
    if( width == 0 || height == 0 || bytesPP == 0)
        return;        
    Reset();
    
    uint64_t size = height * width * bytesPP;
    assert(size < szLimit);
    if( size >= szLimit) return;
   
    uint8_t* buffer = (uint8_t*)malloc( (size_t) size);
    assert(buffer);
    if ( !buffer ) return;

    SecureZeroMemory(buffer,(size_t) size);
    m_width = width;
    m_height = height;
    m_bytesPerPixel = bytesPP;
    m_rowPitch = bytesPP * width;
    m_format = format;
    m_size = size;
    m_buffer = buffer;
}



//
//
//void ImageData::ApplyImage(const ImageData* img, 
//                    int32_t x, 
//                    int32_t y,   
//                    int32_t brushOp,
//                    Bound2di& outRect)
//{
//    outRect.x1 = 0;
//    outRect.x2 = 0;
//    outRect.y1 = 0;
//    outRect.y2 = 0;
//
//    bool valid = img && img->m_format == m_format
//        && m_buffer && img->m_buffer;
//    assert(valid);
//    if(!valid) return;
//    
//
//    Bound2di kernelRect;
//    kernelRect.x1 = x -  img->GetWidth()/2;   
//    kernelRect.y1 = y -  img->GetHeight()/2;
//    kernelRect.x2 = x +  img->GetWidth()/2;   
//    kernelRect.y2 = y +  img->GetHeight()/2;
//    if((kernelRect.x2 - kernelRect.x1) < img->GetWidth())  kernelRect.x2++;
//    if((kernelRect.y2 - kernelRect.y1) < img->GetHeight()) kernelRect.y2++;
//
//        
//    Bound2di imgRect;
//    imgRect.x1 = 0;
//    imgRect.y1 = 0;
//    imgRect.x2 = m_width;
//    imgRect.y2 = m_height;
//    
//    if(!Bound2di::Intersect(kernelRect,imgRect,outRect)) return;
//      
//    for(int32_t cy = outRect.y1; cy < outRect.y2; cy++)
//    {
//        for(int32_t cx = outRect.x1; cx <outRect.x2; cx++)
//        {
//            const float* scrPixel = (float*)img->PixelAt(cx-outRect.x1,cy-outRect.y1);            
//            float* destPixel = (float*) GetPixelAt(cx,cy);
//            *destPixel = *destPixel + *scrPixel;
//        }        
//    }
//
//}
void ImageData::Extend(int32_t dx, int32_t dy)
{

    if(m_buffer == NULL || (dx == 0 && dy == 0)) return;
    int32_t width = m_width + dx;
    int32_t height = m_height + dy;
    int32_t rowPitch = m_rowPitch +  dx * m_bytesPerPixel;

    
    uint64_t size = height * rowPitch;
    uint8_t* buffer = (uint8_t*)malloc( (size_t)size);
    assert(buffer);
    if ( !buffer ) return;

    // copy images.
    
    uint8_t* desPtr = buffer;
    uint8_t* srcPtr = m_buffer;
    {
    int32_t count = m_width * m_bytesPerPixel;
    for(int32_t y = 0;  y < m_height; y++)
    {
        uint8_t* desline = desPtr + y * rowPitch;
        uint8_t* srcline = srcPtr + y * m_rowPitch;        
        memcpy_s(desline,count,srcline,count);
    }
    }
    
   
    if(dx > 0)
    {// add cols.
        for(int32_t y = 0;  y < m_height; y++)
        {
            uint8_t* srcline  = desPtr + y * rowPitch + (m_width-1) * m_bytesPerPixel;
            uint8_t* destline = desPtr + y * rowPitch + m_width * m_bytesPerPixel;            
            for(int32_t x = 0;  x < dx; x++)
            {
                memcpy_s(destline,m_bytesPerPixel,srcline,m_bytesPerPixel);
                destline += m_bytesPerPixel;
            }
        }
    }

    if(dy > 0)
    {// add rows        
        uint8_t* srcline = desPtr + (m_height-1) * rowPitch;
        int32_t count = width * m_bytesPerPixel;
        for(int32_t y = 0;  y < dy; y++)
        {
            // address of last row            
            uint8_t* desline = desPtr + (y+m_height) * rowPitch;                        
            memcpy_s(desline,count,srcline,count);
        }
    }

    DirectX::Image img;        
    img.width =(size_t) width;
    img.height = (size_t) height;
    img.format = (DXGI_FORMAT) m_format;
    img.rowPitch = (size_t) rowPitch;
    img.slicePitch = (size_t) size;
    img.pixels = buffer;        
    InitFrom(&img);
    free(buffer);
}

void ImageData::Convert(uint32_t format)
{
    if(format != m_format && m_buffer != NULL)
    {            
        Image img;
        img.width = (size_t) m_width;
        img.height = (size_t) m_height;
        img.format = (DXGI_FORMAT)m_format;
        img.rowPitch = (size_t) m_rowPitch;
        img.slicePitch = (size_t) m_size;
        img.pixels = m_buffer;

        ScratchImage scratchImg;
        HRESULT hr = DirectX::Convert(img,(DXGI_FORMAT)format,0,0,scratchImg);
        if(FAILED(hr)) return;
        const Image* image = scratchImg.GetImage(0,0,0);
        InitFrom(image);         
    }
}
void ImageData::LoadFromFile(wchar_t* file)
{    
    Reset();
    if(!FileUtils::Exists(file)) return;
        
    TexMetadata metadata;
    ScratchImage scratchImg;
    ScratchImage scratchImg2;
    HRESULT hr = DXUtil::LoadTexture(file,&metadata,scratchImg);
    if(FAILED(hr)) return;
    const Image* image = scratchImg.GetImage(0,0,0);
    if(IsCompressed(image->format))
    {               
        DXGI_FORMAT fmt = DXGI_FORMAT_UNKNOWN; //DXGI_FORMAT_B8G8R8A8_UNORM;       
        Decompress(*image,fmt,scratchImg2);
        if(FAILED(hr)) return;
        image = scratchImg2.GetImage(0,0,0);
    }
    InitFrom(image);    
}

void ImageData::SaveToFile(wchar_t* file)
{
    if(m_buffer == NULL  || file == NULL || wcslen(file) < 0) return;

    HRESULT hr = S_OK;
    std::wstring ext = FileUtils::GetExtensionLower(file);
        
    Image img;
    img.width = (size_t) m_width;
    img.height =(size_t)  m_height;
    img.format = (DXGI_FORMAT)m_format;
    img.rowPitch =(size_t) m_rowPitch;
    img.slicePitch =(size_t) m_size;
    img.pixels = m_buffer;

    if(ext == L".dds")
    {        
        hr = SaveToDDSFile(img,0,file);        
    }
    else // try wic
    {
        REFGUID wicCodec = DXUtil::GetWICCodecFromFileExtension(ext.c_str());
        hr = DirectX::SaveToWICFile( img, 0, wicCodec, file);    
    }
    Logger::IsFailureLog(hr,L"error saving %s\n",file);    
}

ImageData::~ImageData()
{
    Reset();
}

void ImageData::Reset()
{
    m_width = 0;
    m_height = 0;
    m_bytesPerPixel = 0;
    m_rowPitch = 0; 
    m_format = 0;
    m_size = 0;    
    if(m_buffer)
    {
       free(m_buffer);
       m_buffer = NULL;
    }
}


void ImageData::InitFrom(const ImageData* imgdata)
{
    assert(this != imgdata);

    Image img;
    img.width =  (size_t)  imgdata->m_width;
    img.height = (size_t)  imgdata->m_height;
    img.format = (DXGI_FORMAT)imgdata->m_format;
    img.rowPitch = (size_t)  imgdata->m_rowPitch;
    img.slicePitch = (size_t) imgdata->m_size;
    img.pixels = imgdata->m_buffer;
    this->InitFrom(&img);
}

void ImageData::InitFrom(const Image* image)
{
    assert(image);
    if(!image) return;

    Reset();
    uint64_t size = image->height * image->rowPitch;
    uint8_t* buffer = (uint8_t*)malloc( (size_t) size);
    assert(buffer);
    if ( !buffer ) return;

    
    m_buffer = buffer;
    m_width = (int32_t) image->width;
    m_height = (int32_t) image->height;
    m_bytesPerPixel = (int32_t) DirectX::BitsPerPixel(image->format) / 8;
    m_rowPitch = (int32_t) image->rowPitch;
    m_format = (uint32_t) image->format;
    m_size = (uint64_t) size;
    memcpy_s(m_buffer,m_size,image->pixels,m_size);      
}
}
