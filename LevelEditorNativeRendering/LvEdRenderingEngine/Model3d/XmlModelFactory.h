//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "rapidxmlhelpers.h"



// Use XmlModelFactory for files using xml format
namespace LvEdEngine
{

    class Model3dBuilder;
    //--------------------------------------------------
    class XmlModelFactory : public ResourceFactory
    {
    public:
        XmlModelFactory(ID3D11Device* device);
        virtual bool LoadResource(Resource* resource, const WCHAR * filename);
        virtual void ProcessXml(xml_node * root, Model3dBuilder * builder) = 0;
    protected:
        ID3D11Device* m_device;
        void ParseError(const char * fmt, ...);
        int m_parseErrors;
    };
};
