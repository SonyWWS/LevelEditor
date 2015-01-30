//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "Texture.h"
#include "RenderUtil.h"
#include "../Core/Utils.h"

namespace LvEdEngine
{

Texture::Texture(ID3D11Texture2D* tex, bool createView)
{    
    m_tex = NULL;
    m_view = NULL;
    m_texType = TextureType::Unknown;

    ID3D11ShaderResourceView* texView = NULL;
    if(tex && createView)
    {
        ID3D11Device* pd3dDevice = NULL;
        tex->GetDevice(&pd3dDevice);        
        pd3dDevice->CreateShaderResourceView( tex, NULL, &texView);
        pd3dDevice->Release();
    }
    Set(tex,texView);
}

// ----------------------------------------------------------------------------------------------
Texture::Texture(ID3D11Texture2D* tex, ID3D11ShaderResourceView* view)
{    
    m_tex = NULL;
    m_view = NULL;
    m_texType = TextureType::Unknown;
    Set(tex,view);
}



// ----------------------------------------------------------------------------------------------
Texture::Texture()
{
    m_tex = NULL;
    m_view = NULL;
    m_texType = TextureType::Unknown;
}

// ----------------------------------------------------------------------------------------------
Texture::Texture(Texture* tex)
{           
    if(tex)
    {
        m_tex = tex->m_tex;
        m_view = tex->m_view;
        m_texType = tex->GetTextureType();
        SAFE_ADDREF(m_tex);
        SAFE_ADDREF(m_view);
    }
    else
    {
        m_tex = NULL;
        m_view = NULL;
        m_texType = TextureType::Unknown;
    }
}

// ----------------------------------------------------------------------------------------------
//virtual 
// this should only get called by the resource manager when items are being released.
Texture::~Texture()
{
    SAFE_RELEASE(m_tex);
    SAFE_RELEASE(m_view);
}

// ----------------------------------------------------------------------------------------------
void Texture::Set(ID3D11Texture2D* tex, ID3D11ShaderResourceView* view)
{
    SAFE_RELEASE(m_tex);
    SAFE_RELEASE(m_view);
    m_tex = tex;
    m_view = view;
    if((NULL != m_tex) && (NULL != m_view))
    {
        SetReady();
    }
}

};
