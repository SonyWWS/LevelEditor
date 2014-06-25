//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include "TerrainMap.h"
#include <vector>

namespace LvEdEngine
{
   
class LayerMap : public TerrainMap
{
public:
    virtual const char* ClassName() const {return StaticClassName();}
    static const char* StaticClassName(){return "LayerMap";}
    LayerMap();
    ~LayerMap();
    void Invoke(wchar_t* fn, const void* arg, void** retVal);

    void SetLodTexture(wchar_t* lodTexture);
    void SetMask(wchar_t* mask);
    void SetTextureScale(float textureScale) 
    {
        if(m_textureScale < 1.0f) m_textureScale = 1.0f;
        m_textureScale = textureScale;
    }
    float GetTextureScale() { return m_textureScale;}
    const Texture* GetMask() const;
    
private:
    Texture* m_lodTexture;
    Texture* m_mask;
    float m_textureScale;

    std::vector<uint8_t> m_tmpBrushdata;
    
};


}


//LayerMap