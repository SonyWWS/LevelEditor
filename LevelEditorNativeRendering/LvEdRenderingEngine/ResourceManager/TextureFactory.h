//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

namespace LvEdEngine
{
    //--------------------------------------------------
    class TextureFactory : public ResourceFactory
    {
    public:
        TextureFactory(ID3D11Device* device);
        virtual Resource* CreateResource(Resource* def);
        virtual bool LoadResource(Resource* resource, const WCHAR * filename);
    private:
        ID3D11Device* m_device;        
    };

};
