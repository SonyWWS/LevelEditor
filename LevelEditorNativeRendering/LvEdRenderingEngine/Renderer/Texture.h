//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <D3D11.h>
#include "Resource.h"
#include <stdint.h>
namespace LvEdEngine
{
    //---------------------------------------------------------------------------------------------
    class Texture : public Resource
    {
    public:
        Texture();
        Texture(ID3D11Texture2D* tex, bool createView);
        Texture(ID3D11Texture2D* tex, ID3D11ShaderResourceView* view);
        
        
        Texture(Texture* tex);

         virtual const char* ClassName() const {return StaticClassName();}
         static const char* StaticClassName(){return "Texture";}

        // the destructor should only get called when the resource is released
        // by the resource manager. don't delete this directly, release them instead.
        virtual ~Texture();

        //IResource
        virtual ResourceTypeEnum GetType(){return ResourceType::Texture;}


        ID3D11Texture2D* GetTex() const {return m_tex;}
        ID3D11ShaderResourceView* GetView()const {return m_view;}

        void Set(ID3D11Texture2D* tex, ID3D11ShaderResourceView* view);
                      
    private:
        ID3D11Texture2D* m_tex;
        ID3D11ShaderResourceView* m_view;
        
    };
};
