//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

namespace LvEdEngine
{
    //--------------------------------------------------
    class ColladaModelFactory : public XmlModelFactory
    {
    public:
        ColladaModelFactory(ID3D11Device* device);
        virtual Resource* CreateResource(Resource* def);
        virtual void ProcessXml(xml_node * root, Model3dBuilder * builder);
    private:
        struct Source
        {
            const char * id;
            std::vector<float> float_array;
            int stride;
        };

        typedef std::map<std::string, std::string> ControllerToGeo;


        template <class T>
        bool CopyFloatsFromSource(Model3dBuilder * builder, const char * sourceName,
                                                const std::vector<Source> * sources, std::vector<T> * dst);
        void ProcessInput(Model3dBuilder * builder, xml_node* input, UINT offset, std::vector<Source> * sources);
        void ProcessPolyListFeatures(Model3dBuilder * builder, xml_node* polylist, std::vector<Source> * sources);
        void ProcessSources(std::vector<Source> * sources, Model3dBuilder * builder, xml_node* mesh);
        void ProcessGeo(Model3dBuilder * builder, xml_node* geo);
        void ProcessMaterial(Model3dBuilder * builder, xml_node* node);
        void ParseEffectColor(xml_node* node, const char * name, float4 * out);
        void ParseEffectFloat(xml_node* node, const char * name, float * out);
        void ParseEffectTexture( xml_node* node, const char * name, std::string * out);
        const std::string & FindAndValidateString(std::map<std::string, std::string> * stringMap,
                                                                                    const std::string & index);
        const std::string& GetTextureFileFromSampler(Model3dBuilder* builder, const std::string& samplerName);
        void ProcessEffect(Model3dBuilder * builder, xml_node* node);
        void ProcessImage(Model3dBuilder * builder, xml_node* node);
        void GetTransform(xml_node* node, Matrix * out );
        void ProcessController(xml_node * xmlController, ColladaModelFactory::ControllerToGeo * controllerToGeo);
        Node * ProcessSceneNode(Model3dBuilder * builder, xml_node* node, Node * parent,
                                                                                    ControllerToGeo * controllerToGeo);
        void ProcessChildNodes(Model3dBuilder * builder, xml_node* xmlParent, Node * parent,
                                                                                    ControllerToGeo * controllerToGeo);
    };
};
