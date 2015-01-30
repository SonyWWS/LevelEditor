//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "TerrainMap.h"
#include "../../Core/ImageData.h"
#include "../../Core/Utils.h"
#include "../../Core/FileUtils.h"
#include "../../Core/StringUtils.h"
#include "../../DirectX/DXUtil.h"
#include "../../Renderer/Texture.h"
#include "../../ResourceManager/ResourceManager.h"
#include "../../Renderer/TextureLib.h"
#include "../../Renderer/RenderContext.h"


namespace LvEdEngine
{
    // helper functions.
    void LoadTexture(wchar_t* file, Texture** tex)
    {        
        SAFE_RELEASE(*tex);
        if(file == NULL || wcslen(file) == 0) return;
         *tex = (Texture*) ResourceManager::Inst()->LoadImmediate(file,NULL); 
    }
    
    TerrainMap::TerrainMap() 
        : m_minHeight(0),
        m_maxHeight(0),
        m_diffuse(NULL),
        m_normal(NULL),
        m_specular(NULL),        
        m_maskData(NULL)
    {          
    }

    TerrainMap::~TerrainMap()
    {
        SAFE_RELEASE(m_diffuse);
        SAFE_RELEASE(m_normal);
        SAFE_RELEASE(m_specular);     
        SAFE_DELETE(m_maskData);
    }

    
    void TerrainMap::Invoke(wchar_t* fn, const void* arg, void** retVal)
    {
        if( StrUtils::Equal(fn,L"GetMaskMapInstanceId"))
        {
            static ObjectGUID hmInstId = 0;
            hmInstId = m_maskData ? m_maskData->GetInstanceId() : 0;
            *retVal = &hmInstId;
        }       
    }

     void TerrainMap::SetDiffuse(wchar_t* diffuse)
     {
         SAFE_RELEASE(m_diffuse);
         if(diffuse == NULL || wcslen(diffuse) == 0) 
             return;
         m_diffuse = (Texture*) ResourceManager::Inst()->LoadImmediate(diffuse,
             TextureLib::Inst()->GetDefault(TextureType::DIFFUSE));
     }

     void TerrainMap::SetNormal(wchar_t* normal)
     {
         LoadTexture(normal,&m_normal);
     }

     void TerrainMap::SetSpecular(wchar_t* specular)
     {
         LoadTexture(specular,&m_specular);
     }

     void TerrainMap::SetMask(wchar_t* mask)
     {         
         SAFE_DELETE(m_maskData);         
         if(!FileUtils::Exists(mask)) return;
         
         m_maskData = new ImageData();
         m_maskData->LoadFromFile(mask);
         m_maskData->Convert(DXGI_FORMAT_R8_UNORM);
         if(m_maskData->GetBufferPointer() == NULL)
         {
             SAFE_DELETE(m_maskData);          
         }                  

     } 

     const Texture* TerrainMap::GetDiffuse() const 
     { 
         return  m_diffuse ? m_diffuse : TextureLib::Inst()->GetDefault(TextureType::DIFFUSE);
     }
     const Texture* TerrainMap::GetNormal() const  
     {
         return m_normal ? m_normal : TextureLib::Inst()->GetDefault(TextureType::NORMAL);
     }
     const Texture* TerrainMap::GetSpecular() const 
     {
         return m_specular ? m_specular : TextureLib::Inst()->GetDefault(TextureType::SPEC);
     }
}