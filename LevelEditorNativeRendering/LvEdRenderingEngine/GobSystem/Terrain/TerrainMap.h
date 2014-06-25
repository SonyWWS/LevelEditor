//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <string>
#include "../../Core/Object.h"

namespace LvEdEngine
{

class Texture;
class ImageData;
class TerrainGob;
      
class TerrainMap : public Object
{
	public:
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "TerrainMap";}
        
        TerrainMap();
        ~TerrainMap();

        void Invoke(wchar_t* fn, const void* arg, void** retVal);
        void SetName(wchar_t* name)  { m_name = name ? name : L""; }
        const wchar_t* const GetName() const { return m_name.c_str();}
        void SetMinHeight(float minHeight) { m_minHeight = minHeight;}
        void SetMaxHeight(float maxHeight) { m_maxHeight = maxHeight;}
        void SetParent(TerrainGob* parent) {m_parent = parent;}
        void SetDiffuse(wchar_t* diffuse);
        void SetNormal(wchar_t* normal);
        void SetSpecular(wchar_t* specular);
        virtual void SetMask(wchar_t* mask);
        void SetVisible(bool visible) { m_visible = visible; }
        bool IsVisible() const {return m_visible;}
        const TerrainGob* GetParent() const {return m_parent;}
        const Texture* GetDiffuse() const;
        const Texture* GetNormal()  const;
        const Texture* GetSpecular() const;        
        ImageData* GetMaskData() { return m_maskData;}

private:    
    std::wstring m_name;
    float m_minHeight;
    float m_maxHeight;
    Texture* m_diffuse;
    Texture* m_normal;
    Texture* m_specular;        
    ImageData* m_maskData;
    TerrainGob* m_parent;
    bool m_visible;
};

}