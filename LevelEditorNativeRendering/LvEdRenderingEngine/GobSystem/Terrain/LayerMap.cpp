//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "LayerMap.h"
#include "../../Core/Utils.h"
#include "../../Core/StringUtils.h"
#include "../../Core/ImageData.h"
#include "../../Renderer/Texture.h"
#include "../../Renderer/RenderContext.h"
#include "../../ResourceManager/ResourceManager.h"
#include "../../Renderer/TextureLib.h"



namespace LvEdEngine
{
    LayerMap::LayerMap() : m_lodTexture(NULL),
                           m_mask(NULL),
                           m_textureScale(10)
    {
    }

    LayerMap::~LayerMap()
    {
        SAFE_RELEASE(m_lodTexture);
        SAFE_DELETE(m_mask);
    }

    void LayerMap::Invoke(wchar_t* fn, const void* arg, void** retVal)
    {
        TerrainMap::Invoke(fn,arg,retVal);

        if(StrUtils::Equal(fn,L"ApplyDirtyRegion"))
        {

            m_tmpBrushdata.clear();
            ImageData* img = GetMaskData();

             bool valid = arg != NULL; 
             assert(valid);
             if(!valid) return;
             Bound2di box = *(Bound2di*)arg;

             for(int32_t y = box.y1; y < box.y2; y++)
             {
                 for(int32_t x = box.x1; x < box.x2; x++)
                 {
                     uint8_t val = *(uint8_t*)img->PixelAt( x,y);
                     m_tmpBrushdata.push_back(val);                   
                 }
             }

             auto cntx = RenderContext::Inst()->Context();
             D3D11_BOX destRegion;
             destRegion.left = box.x1;
             destRegion.right = box.x2;
             destRegion.top = box.y1;
             destRegion.bottom = box.y2;
             destRegion.front = 0;
             destRegion.back = 1;
             uint32_t rowPitch = (destRegion.right - destRegion.left) * img->GetBytesPerPixel();
             cntx->UpdateSubresource( m_mask->GetTex(), 0, &destRegion, &m_tmpBrushdata[0], rowPitch, 0 ); 
           
        }
        
    }

    void LayerMap::SetMask(wchar_t* mask)
    {
        SAFE_DELETE(m_mask);
        TerrainMap::SetMask(mask);
        
        ImageData* img = GetMaskData();

        if(img != NULL)
        {
            assert(img->GetFormat() == DXGI_FORMAT_R8_UNORM);
            // create texture to store height
            D3D11_TEXTURE2D_DESC desc;
            desc.Width = img->GetWidth();
            desc.Height = img->GetHeight();
            desc.MipLevels = 1;
            desc.ArraySize = 1;
            desc.SampleDesc.Count = 1;
            desc.SampleDesc.Quality = 0;
            desc.Format = (DXGI_FORMAT)img->GetFormat();
            desc.Usage = D3D11_USAGE_DEFAULT;
            desc.BindFlags = D3D11_BIND_SHADER_RESOURCE;
            desc.CPUAccessFlags = 0;
            desc.MiscFlags = 0;    
            D3D11_SUBRESOURCE_DATA initData;
            initData.pSysMem = img->GetBufferPointer();
            initData.SysMemPitch  =  img->GetRowPitch();
            initData.SysMemSlicePitch =  initData.SysMemPitch * img->GetHeight();
            auto device = RenderContext::Inst()->Device();

            ID3D11Texture2D *tex;
            device->CreateTexture2D( &desc, &initData, &tex );
            assert(tex);
            m_mask = new Texture(tex,true);
        }
    }

    void LayerMap::SetLodTexture(wchar_t* lodTexture)
    {
        SAFE_RELEASE(m_lodTexture);
        if(lodTexture == NULL || wcslen(lodTexture) == 0) return;
        m_lodTexture = (Texture*) ResourceManager::Inst()->LoadImmediate(lodTexture,NULL); 
    }

    const Texture* LayerMap::GetMask() const 
    {         
         return m_mask ? m_mask : TextureLib::Inst()->GetDefault(TextureType::BlankMask);
    }

}