//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "../Core/NonCopyable.h"
#include "TextureLib.h"
#include "RenderUtil.h"
#include "Texture.h"
#include "../Core/ResUtil.h"
#include "../Core/FileUtils.h"
#include "../Core/Logger.h"
#include "GpuResourceFactory.h"
#include "../DirectX/WICTextureLoader/WICTextureLoader.h"
#include <map>

using namespace DirectX;
namespace LvEdEngine
{

typedef std::map<std::wstring, Texture*> TextureMap;
class TextureLib::Imple : public NonCopyable
{
    public:
        Texture* m_defaultTextures[TextureType::MAX];
        Texture* m_whiteTexture;
        TextureMap m_textures;
};


TextureLib * TextureLib::s_Inst = NULL;

//-------------------------------------------------------------------------------------------------
Texture* TextureLib::GetDefault(TextureTypeEnum texture)
{
    return m_pImple->m_defaultTextures[texture];
}

//-------------------------------------------------------------------------------------------------
Texture* TextureLib::GetWhite()
{
    return m_pImple->m_whiteTexture;
}

Texture* TextureLib::GetByName(const wchar_t* name)
{
    Texture* res = NULL;
    auto it = m_pImple->m_textures.find(name);
    if( it != m_pImple->m_textures.end())
    {
        res = it->second;
    }
    return res;
}


TextureLib::TextureLib()
{
    m_pImple = new Imple();
    for(int i = TextureType::MIN; i < TextureType::MAX; ++i)
        m_pImple->m_defaultTextures[i] = NULL;
    
}
TextureLib::~TextureLib()
{
    for(int i = TextureType::MIN; i < TextureType::MAX; ++i)
    {
        SAFE_DELETE(m_pImple->m_defaultTextures[i]);
    }
    SAFE_DELETE(m_pImple->m_whiteTexture);

    for(auto it = m_pImple->m_textures.begin(); it != m_pImple->m_textures.end(); it++)
    {
        Texture* tex = it->second;
        SAFE_DELETE(tex);
    }
    m_pImple->m_textures.clear();

    SAFE_DELETE(m_pImple);
}


static Texture* CreateCheckerboardTexture2D(ID3D11Device* device, int w, int h, uint32_t color1,  uint32_t color2, bool cubemap = false, bool useSRGBView = false);


void TextureLib::InitInstance(ID3D11Device* device)
{
    s_Inst = new TextureLib();
    Imple* pImple = s_Inst->m_pImple;
    
    
    pImple->m_defaultTextures[TextureType::DIFFUSE] = CreateCheckerboardTexture2D(device, 128, 128, 0xFF404040, 0xFF808080, false,true);
    pImple->m_defaultTextures[TextureType::DIFFUSE]->SetTextureType(TextureType::DIFFUSE);

    pImple->m_defaultTextures[TextureType::Cubemap] = CreateCheckerboardTexture2D(device, 128, 128, 0xff000040, 0xff000080, true);
    pImple->m_defaultTextures[TextureType::Cubemap]->SetTextureType(TextureType::Cubemap);

    pImple->m_defaultTextures[TextureType::NORMAL] = CreateSolidTexture2D(device, 8, 8, 0xFFFF8080);
    pImple->m_defaultTextures[TextureType::NORMAL]->SetTextureType(TextureType::NORMAL);

    pImple->m_defaultTextures[TextureType::LIGHT] = CreateSolidTexture2D(device, 8, 8, 0xFFFFFFFF);
    pImple->m_defaultTextures[TextureType::LIGHT]->SetTextureType(TextureType::LIGHT);

    pImple->m_defaultTextures[TextureType::SPEC] = CreateSolidTexture2D(device, 8, 8, 0xFF000000);    
    pImple->m_defaultTextures[TextureType::SPEC]->SetTextureType(TextureType::SPEC);

    pImple->m_defaultTextures[TextureType::BlankMask] = CreateSolidTexture2D(device, 4, 4, 0x00);
    pImple->m_defaultTextures[TextureType::BlankMask]->SetTextureType(TextureType::BlankMask);

    pImple->m_defaultTextures[TextureType::FullMask] = CreateSolidTexture2D(device, 4, 4, 0xFFFFFFFF);
    pImple->m_defaultTextures[TextureType::FullMask]->SetTextureType(TextureType::FullMask);
    
    pImple->m_whiteTexture = CreateSolidTexture2D(device, 8, 8, 0xFFFFFFFF);

    typedef std::pair<std::wstring, Texture*> NameTexPair;

    const wchar_t* resName = L"Light.png";
    const wchar_t* resType = L"Texture";
    HRESULT hr = E_FAIL;

    Texture* tex = NULL;
    ID3D11Resource* dxresource = NULL;
    ID3D11ShaderResourceView* dxTexView = NULL;
    uint32_t  resSize = 0;
    uint8_t* data = (uint8_t*)ResUtil::LoadResource(resType, resName,&resSize);
    hr = CreateWICTextureFromMemory( device,
                                             NULL,
                                             data,
                                             resSize,
                                             &dxresource,
                                             &dxTexView);
   free(data);
   if (!Logger::IsFailureLog(hr, L"Error loading %s\n", resName))
   {
       D3D11_RESOURCE_DIMENSION resDim = D3D11_RESOURCE_DIMENSION_UNKNOWN;
       dxresource->GetType( &resDim );
       assert( resDim == D3D11_RESOURCE_DIMENSION_TEXTURE2D);
       ID3D11Texture2D* dxTex = NULL;
       hr = dxresource->QueryInterface( __uuidof(ID3D11Texture2D), (void**) &dxTex );
       dxresource->Release();
       assert(dxTex);
       tex = new Texture(dxTex,dxTexView);
       auto insertResult = pImple->m_textures.insert(NameTexPair(resName,tex));
       assert(insertResult.second);
   }
}

 void TextureLib::DestroyInstance(void)
 {
     SAFE_DELETE(s_Inst);
 }
 

// ----------------------------------------------------------------------------------------------
static Texture* CreateCheckerboardTexture2D(ID3D11Device* device, int w, int h, uint32_t color1,  uint32_t color2,bool cubemap, bool useSRGBView)
{
    int slicePitch = w * h;
    int numslices = cubemap? 6 : 1;
    uint32_t* buf = new uint32_t[numslices * slicePitch];
    // create the checkerboard pattern
    for(int c = 0; c < numslices; c++)
    {
    for(int i=0;i<h;i++)
    {
        for(int j=0;j<w;j++)
        {
            if((i&16)==(j&16))  buf[c*slicePitch+i*w+j] = color1;
            else                buf[c*slicePitch+i*w+j] = color2;
        }
    }
    }

    ID3D11Texture2D* tex = GpuResourceFactory::CreateDxTexture2D(buf, w, h, cubemap);
    ID3D11ShaderResourceView* view = GpuResourceFactory::CreateTextureView(tex);
    assert(tex);
    assert(view);
    delete[] buf;
    Texture* texture =  new Texture(tex, view);    
    return texture;
}


// ----------------------------------------------------------------------------------------------
Texture* TextureLib::CreateSolidTexture2D(ID3D11Device* device, int w, int h, uint32_t color)
{
    return CreateCheckerboardTexture2D(device, w, h, color, color);
}
    
}; // namespace
