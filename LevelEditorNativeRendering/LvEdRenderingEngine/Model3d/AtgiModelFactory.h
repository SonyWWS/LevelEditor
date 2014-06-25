//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

namespace LvEdEngine
{
    //--------------------------------------------------
    class AtgiModelFactory : public XmlModelFactory
    {
    public:
        AtgiModelFactory(ID3D11Device* device);
        virtual Resource* CreateResource(Resource* def);
        virtual void ProcessXml(xml_node * root, Model3dBuilder * builder);
    private:
        void ProcessTexture(Model3dBuilder * builder, xml_node* texNode);
        xml_node* GetBinding(xml_node* shaderNode, const char* bindingtype, const char* name, const char* partial);
        void GetColor(xml_node* shaderNode, const char* name, float4* out);
        void GetPower(xml_node* shaderNode, const char* name, float* out);
        void ProcessShader(Model3dBuilder * builder, xml_node* shaderNode);
        void ProcessSources(Model3dBuilder * builder, xml_node* vertexArray);
        void ProcessPrimitivesFeatures(Model3dBuilder * builder, xml_node* xmlPrim);
        void ProcessPrimitives(Model3dBuilder * builder, xml_node* xmlPrim, Node * node);
        void PushCustomDataValue(CustomDataAttribute * customDataAttribute, const char * type, xml_node * xmlValue);
        void ProcessCustomDataAttributes(xml_node* xmlParent, Node * node);
        void ProcessMesh(Model3dBuilder * builder, xml_node* xmlMesh, Node * node);
        void GetTransform(xml_node* node, Matrix * out );
        void ProcessInstance(Model3dBuilder * builder, xml_node* xmlInstance, Node * node);
        void ProcessNode(Model3dBuilder * builder, xml_node* xmlNode, Node * parent);
    };
};
