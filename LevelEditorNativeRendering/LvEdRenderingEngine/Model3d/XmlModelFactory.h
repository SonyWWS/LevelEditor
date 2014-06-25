//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

class xml_node;
class Model3dBuilder;

// Use XmlModelFactory for files using xml format
namespace LvEdEngine
{
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
