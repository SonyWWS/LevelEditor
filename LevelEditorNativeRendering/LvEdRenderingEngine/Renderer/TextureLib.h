//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "RenderEnums.h"
#include <D3D11.h>
#include "../Core/NonCopyable.h"
#include <stdint.h>

namespace LvEdEngine
{

class Texture;

// holds list of textures, 
class TextureLib : public NonCopyable
{
public:
    static void         InitInstance(ID3D11Device* device);
    static void         DestroyInstance(void);
    static TextureLib*  Inst() { return s_Inst; }

    Texture* GetDefault(TextureTypeEnum texture);
    Texture* GetWhite();
    Texture* GetByName(const wchar_t* name);
    static Texture* CreateSolidTexture2D(ID3D11Device* device, int w, int h, uint32_t color);

private:
    
    TextureLib();
    ~TextureLib();

    static TextureLib* s_Inst;
    class Imple;
    Imple*  m_pImple;

};


}
