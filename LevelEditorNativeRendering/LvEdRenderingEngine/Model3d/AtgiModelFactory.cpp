//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "../VectorMath/V3dMath.h"
#include "../VectorMath/CollisionPrimitives.h"
#include "../Renderer/Model.h"
#include "../Renderer/CustomDataAttribute.h"
#include "../Core/Utils.h"
#include "../Core/Logger.h"
#include "../ResourceManager/ResourceManager.h"
#include "Model3dBuilder.h"
#include "rapidxmlhelpers.h"
#include "XmlModelFactory.h"
#include "AtgiModelFactory.h"

namespace LvEdEngine
{


// ------------------------------------------------------------------------------------------------
void AtgiModelFactory::ProcessTexture(Model3dBuilder * builder, xml_node* texNode)
{
    // get and verify 'name' and 'uri'
    const char * name = GetAttributeText(texNode, "name", true);
    const char * uri = GetAttributeText(texNode, "uri", true);
    if (!name) { return; }
    if (!uri) { return; }

    // validate 'name' is unique
    if (builder->m_material.image2file.find(name) != builder->m_material.image2file.end())
    {
        ParseError("<texture> has non-unique name, '%s'\n", name);
        return;
    }

    // save 'uri' in map
    builder->m_material.image2file[name] = uri;
}

// ------------------------------------------------------------------------------------------------
xml_node* AtgiModelFactory::GetBinding(xml_node* shaderNode, const char* bindingtype, const char* name, const char* partial)
{
    xml_node* found = NULL;
    for(xml_node* binding = FindChildByName(shaderNode, "binding"); binding != NULL; binding = FindNextByName(binding, "binding"))
    {
        // get and validate 'type' and 'tag'
        const char * type = GetAttributeText(binding, "type", true);
        const char * tag = GetAttributeText(binding, "tag", true);
        if (!type) { continue; }
        if (!tag) { continue; }

        // check if this is the binding we are looking for
        if(strcmp(type, bindingtype)==0)
        {
            if(name)
            {
                if(strcmp(tag, name)==0)
                {
                    found = binding;
                    break;
                }
            }
            else if(partial)
            {
                if(NULL !=strstr(tag, partial))
                {
                    found = binding;
                    break;
                }
            }
            else
            {
                found = binding;
                break;
            }
        }
    }
    return found;
}


// ------------------------------------------------------------------------------------------------
void AtgiModelFactory::GetColor(xml_node* shaderNode, const char* name, float4* out)
{   
    xml_node * binding = GetBinding(shaderNode, "constant", NULL, name);
    if(binding)
    {
        if (!ParseVector4(binding, out))
        {
            ParseError("'%s' 'constant' binding has fewer than required 4 floats\n", name);
        }
    }
}

// ------------------------------------------------------------------------------------------------
void AtgiModelFactory::GetPower(xml_node* shaderNode, const char* name, float* out)
{
    *out = 1.0f;
    xml_node * binding = GetBinding(shaderNode, "constant", NULL, name);
    if(binding)
    {
        ParseFloat(binding, out);
    }
}

// ------------------------------------------------------------------------------------------------
/// The diffuse texture is determined by the following preference:
/// 1. A MaterialBinding whose type is "texture" and whose Tag is "diffuseTexture"
/// http://wiki.ship.scea.com/confluence/display/SCEEATGDOCS/Materials+in+.atgi
/// 2. The first binding whose type is "texture"
// ------------------------------------------------------------------------------------------------
void AtgiModelFactory::ProcessShader(Model3dBuilder * builder, xml_node* shaderNode)
{
    const char * name = GetAttributeText(shaderNode, "name", true);
    if (!name)
    {
        name = "!missing-name!";
    }
    Material * material = builder->m_model->CreateMaterial(name);

    // To extract color information it seems that we should examine the customData attributes
    // (See wws_mayatoatgi/Source/HandlersATG/ShaderHandlerATG.cpp)
    //
    // I think the following would be correct, but it would be best to have an example and examine
    // the details:
    //
    //  material->diffuse = color(vec3) * diffuse(float)
    //  material->ambient = ambientColor
    //  material->specular = specularColor
    //  material->emissive, maerial->power ... glowIntensity? incandescence?

    // Extract source texture names
    xml_node* texNodes[TextureType::MAX];
    for(int t = TextureType::MIN; t < TextureType::MAX; t++)
        texNodes[t] = NULL;

    texNodes[TextureType::LIGHT] = GetBinding(shaderNode, "texture", NULL, "light");
    texNodes[TextureType::SPEC]  = GetBinding(shaderNode, "texture", NULL, "spec");
    texNodes[TextureType::NORMAL]  = GetBinding(shaderNode, "texture", NULL, "norm");

    // look for '*color*' and then '*diffuse*'
    texNodes[TextureType::DIFFUSE]  = GetBinding(shaderNode, "texture", NULL, "color");
    if (!texNodes[TextureType::DIFFUSE])
    {
        texNodes[TextureType::DIFFUSE]  = GetBinding(shaderNode, "texture", NULL, "diffuse");
    }

    // lookup texture filename
    for(unsigned int i = 0; i < TextureType::MAX; ++i)
    {
        if(NULL != texNodes[i])
        {
            const char * tex = GetAttributeText(texNodes[i], "source", true);
            if (!tex)
            {
                continue;
            }

            // skip | at start of name, if it exists -- '|' is optional
            if(*tex == '|')
            {
                tex++;
            }

            // find tex in image2file map -- and validate it exists
            auto it = builder->m_material.image2file.find(tex);
            if (it == builder->m_material.image2file.end())
            {
                ParseError("<binding> 'source' attribute, '%s', is missing target material\n", tex);
                continue;
            }

            std::string & texFilename = it->second;
            material->texNames[i] = texFilename;
        }
    }
}

// ------------------------------------------------------------------------------------------------
void AtgiModelFactory::ProcessSources(Model3dBuilder * builder, xml_node* vertexArray)
{
    xml_node* pos = NULL;
    xml_node* nor = NULL;
    xml_node* tex = NULL;

    // first setup our 'sources'
    for(xml_node* source = FindChildByName(vertexArray, "array"); source != NULL; source=FindNextByName(source, "array"))
    {
      const char * name = GetAttributeText(source, "name", true);
      if (!name)                        { continue; }
      if(strcmp(name, "position")==0)   { pos = source; }
      if(strcmp(name, "normal")==0)     { nor = source; }
      if(strcmp(name, "map1")==0)       { tex = source; }
    }
    // now we have a pos,nor,tex sources, so load them into the builder
    ParseVector3Array(pos, &builder->m_mesh.source.pos);
    ParseVector3Array(nor, &builder->m_mesh.source.nor);
    ParseVector2Array(tex, &builder->m_mesh.source.tex);
}

// ------------------------------------------------------------------------------------------------
void AtgiModelFactory::ProcessPrimitivesFeatures(Model3dBuilder * builder, xml_node* xmlPrim)
{
  UINT stride=0;
  builder->Mesh_ResetPolyInfo();

  for(xml_node* input = FindChildByName(xmlPrim, "binding"); input != NULL; input = FindNextByName(input, "binding"))
  {
    const char * name = GetAttributeText(input, "source", true);
    if (!name) { continue; }

    if(strcmp(name, "position")==0)
    {
      builder->m_mesh.poly.posOffset = stride;
      builder->m_mesh.poly.hasPos = true;
    }
    if(strcmp(name, "normal")==0)
    {
      builder->m_mesh.poly.norOffset = stride;
      builder->m_mesh.poly.hasNor = true;
    }
    if(strcmp(name, "map1")==0)
    {
      builder->m_mesh.poly.texOffset = stride;
      builder->m_mesh.poly.hasTex = true;
    }
    ++stride;
  }

  builder->m_mesh.poly.stride = stride; // the number of indices per vertex in the poly list
}
// ------------------------------------------------------------------------------------------------
void AtgiModelFactory::ProcessPrimitives(Model3dBuilder * builder, xml_node* xmlPrim, Node * node)
{
    // for each primitive, we create a Geometry with it's own Mesh
    const char * name = GetAttributeText(xmlPrim, "name", false);
    const char * matName = GetAttributeText(xmlPrim, "shader", true);
    const char * type = GetAttributeText(xmlPrim, "type", true);
    if (!type)      { return; }
    if (!matName)   { matName = "!missing-mat!"; }
    if (!name)      { name = matName; }

    // skip | at start of name, if it exists -- '|' is optional
    if(*matName == '|')
    { 
        ++matName;
    }


    std::string geoName = node->name + name;
    Geometry * geo = builder->m_model->CreateGeometry(geoName);
    node->geometries.push_back(geo);
    geo->material = builder->m_model->GetMaterial(matName);

    // now create the mesh for this geo from the poly/indicie/source data
    builder->Mesh_Begin(geo->name.c_str());

    builder->Mesh_SetPrimType(type);

    // setup index offsets
    ProcessPrimitivesFeatures(builder, xmlPrim);

    // get the poly index list and vcount per poly.
    ParseUINTArray(FindChildByName(xmlPrim, "sizes"), &builder->m_mesh.poly.vcount);
    ParseUINTArray(FindChildByName(xmlPrim, "indices"), &builder->m_mesh.poly.indices);

    builder->Mesh_AddPolys();
    geo->mesh = builder->m_mesh.mesh;
    builder->Mesh_End();
}

// ------------------------------------------------------------------------------------------------
void AtgiModelFactory::PushCustomDataValue(CustomDataAttribute * customDataAttribute, const char * type, xml_node * xmlValue)
{
    // handle "string" which uses cdata
    if (strcmp(type, "string")==0)
    {
        const char * value = xmlValue ? xmlValue->value() : "";
        customDataAttribute->PushValueAsString(value);
    }
    else
    {
        // get 'value' attribute
        const char * value = xmlValue ? GetAttributeText(xmlValue, "value", true) : "0";
        if (!value)
        {
            value = "0";
        }

        // handle normal cases of 'bool', 'int', 'float'
        if      (strcmp(type, "bool")==0)   { customDataAttribute->PushValueAsBool(ConvertToBool(value)); }
        else if (strcmp(type, "int")==0)    { customDataAttribute->PushValueAsInt(atoi(value)); }
        else if (strcmp(type, "float")==0)  { customDataAttribute->PushValueAsFloat((float)atof(value)); }
        else if (strcmp(type, "enum")==0)
        {
            // handle enum by looking up name associated with 'value'
            xml_node* field;
            // find the 'field' that matches the 'value'
            for (field = FindChildByName(xmlValue, "field"); field != NULL; field = FindNextByName(field, "field"))
            {
                const char * index = GetAttributeText(field, "index", true);
                if (index && strcmp(index, value) == 0)
                {
                    // use the 'name' associated with the 'value' matched with 'index'
                    const char * name = GetAttributeText(field, "name", true);
                    if (!name)
                    {
                        name = "!missing-enum-name!";
                    }
                    customDataAttribute->PushValueAsString(name);
                    break;
                }
            }
            // in case we didn't find any matches, just use the raw value
            if (field == NULL)
            {
                customDataAttribute->PushValueAsString(value);
                ParseError("Custom data attribute, '%s', has enum value, '%s', with no matching field\n",
                        customDataAttribute->GetName(), value);
            }
        }
        else
        {
            // error log for unexpected attribute type
            ParseError("Unexpected custom attribute type, '%s', for attribute, '%s'\n",
                    type, customDataAttribute->GetName());
        }
    }
}
 
// ------------------------------------------------------------------------------------------------
void AtgiModelFactory::ProcessCustomDataAttributes(xml_node* xmlParent, Node * node)
{
    /* example data:
          <customData>
            <attribute name="stringArray" type="string" isArray="true">
              <value index="1">stringValue</value>
            </attribute>
            <attribute name="boolValue" type="bool" default="false" value="false"/>
            <attribute name="floatValue" type="float" default="0.0" value="100.0"/>
            <attribute name="stringValue" type="string">this is my string</attribute>
            <attribute name="meshType" type="enum" default="0" value="2">
              <field name="visualLod0" index="0" />
              <field name="visualLod1" index="1" />
              <field name="collision" index="2" />
            </attribute>
          </customData>
    */

    xml_node* customData = FindChildByName(xmlParent, "customData");
    for(  xml_node* attribute = FindChildByName(customData, "attribute");
        attribute != NULL;
        attribute = FindNextByName(attribute, "attribute"))
    {
        // get name and type
        CustomDataAttribute * customDataAttribute = new CustomDataAttribute();
        const char * name = GetAttributeText(attribute, "name", true);
        const char * type = GetAttributeText(attribute, "type", true);
        if (!name) { continue; }
        if (!type) { continue; }

        // set name
        customDataAttribute->SetName(name);

        // handle base case and 'isArray' case
        bool isArray = ConvertToBool(GetAttributeText(attribute, "isArray", false));
        if (!isArray)
        {
            PushCustomDataValue(customDataAttribute, type, attribute);
        }
        else
        {
            // set 'isArray'
            customDataAttribute->SetIsArray(isArray);

            // loop through all the child 'value's
            for (   xml_node * xmlValue = FindChildByName(attribute, "value");
                    xmlValue != NULL;
                    xmlValue = FindNextByName(xmlValue, "value"))
            {
                // fill in default for missing indices
                const char * index = GetAttributeText(xmlValue, "index", false);
                for (int i=customDataAttribute->NumValues(); i<atoi(index); i++)
                {
                    PushCustomDataValue(customDataAttribute, type, NULL);
                }

                PushCustomDataValue(customDataAttribute, type, xmlValue);
            }
        }

        // set the attribute
        node->attributes[customDataAttribute->GetName()] = customDataAttribute;
    }
}

// ------------------------------------------------------------------------------------------------
void AtgiModelFactory::ProcessMesh(Model3dBuilder * builder, xml_node* xmlMesh, Node * node)
{
  // an ATGI mesh does not have a model3d equivalent. Instead, we create a 'geometry' for every 'primitives' element.
  // and each geometry will have it's own unique mesh.
  xml_node* vertexArray = FindChildByName(xmlMesh, "vertexArray");
  builder->Mesh_Reset();
  ProcessSources(builder, vertexArray);
  for(xml_node* xmlPrim = FindChildByName(vertexArray, "primitives"); xmlPrim != NULL; xmlPrim = FindNextByName(xmlPrim, "primitives"))
  {
    ProcessPrimitives(builder, xmlPrim, node);
  }
}


// ------------------------------------------------------------------------------------------------
static Matrix CalcTransform(
float3 translation,
float3 rotation,
float3 scale,
float3 scalePivot,
float3 scalePivotTranslate,
float3 rotatePivot,
float3 rotatePivotTranslate)
{
    Matrix M = Matrix::CreateTranslation(-scalePivot);
    M = M * Matrix::CreateScale(scale);
    M = M * Matrix::CreateTranslation(scalePivot + scalePivotTranslate - rotatePivot);
    M = M * Matrix::CreateRotationX(rotation.x);
    M = M * Matrix::CreateRotationY(rotation.y);
    M = M * Matrix::CreateRotationZ(rotation.z);
    M = M * Matrix::CreateTranslation(rotatePivot + rotatePivotTranslate + translation);
    return M;
}

// ------------------------------------------------------------------------------------------------
void AtgiModelFactory::GetTransform(xml_node* node, Matrix * out )
{
  float3 t;
  float3 s(1,1,1);
  float3 sp;
  float3 spt;
  float3 r;
  float3 rp;
  float3 rpt;

  for(xml_node* child = GetChildEle(node); child != NULL; child=GetNextEle(child))
  {
    const char * name = (const char*)child->name();
    bool parsedSuccessful = true;
    if(strcmp(name, "translate")==0)
    {
      parsedSuccessful = ParseVector3(child, &t);
    }
    else if(strcmp(name, "scale")==0)
    {
      parsedSuccessful = ParseVector3(child, &s);
    }
    else if(strcmp(name, "scalePivot")==0)
    {
      parsedSuccessful = ParseVector3(child, &sp);
    }
    else if(strcmp(name, "scalePivotTranslation")==0)
    {
      parsedSuccessful = ParseVector3(child, &spt);
    }
    else if(strcmp(name, "rotEul")==0)
    {
      parsedSuccessful = ParseVector3(child, &r);
    }
    else if(strcmp(name, "rotatePivot")==0)
    {
      parsedSuccessful = ParseVector3(child, &rp);
    }
    else if(strcmp(name, "rotatePivotTranslation")==0)
    {
      parsedSuccessful = ParseVector3(child, &rpt);
    }
    else
    {
      // normal condition for node that we just don't deal with 
    }
    if (!parsedSuccessful)
    {
        ParseError("Failed to parse '%s' transform (expected 3 floats)'\n", name);
    }
  }

  *out = CalcTransform(t,r,s,sp,spt,rp,rpt);

}

// ------------------------------------------------------------------------------------------------
void AtgiModelFactory::ProcessInstance(Model3dBuilder * builder, xml_node* xmlInstance, Node * node)
{
    const char* target = GetAttributeText(xmlInstance, "target", true);
    if (!target) { return; }

    // skip | at start of name, if it exists -- '|' is optional
    if(*target == '|')
    {
        ++target;
    }
    Node* toClone = builder->FindInstance(target);
    if (!toClone)
    {
        ParseError("Could not find instance:target, '%s'\n", target);
        return;
    }

    node->geometries.insert(node->geometries.begin(), toClone->geometries.begin(), toClone->geometries.end());
}


// ------------------------------------------------------------------------------------------------
void AtgiModelFactory::ProcessNode(Model3dBuilder * builder, xml_node* xmlNode, Node * parent)
{
    // we only care about this node if it has either a mesh or child nodes
    xml_node* mesh = FindChildByName(xmlNode, "mesh");
	xml_node* hasJoint = FindChildByName(xmlNode, "joint");
    xml_node* hasNodeChildren = FindChildByName(xmlNode, "node");
    xml_node* hasLodChildren = FindChildByName(xmlNode, "lodgroup");
    xml_node* instanceNode = FindChildByName(xmlNode, "instance");
    const char * nodeName = GetAttributeText(xmlNode, "name", true);
    if (!nodeName)
    {
        nodeName = "!missing-node-name!";
    }
    if(mesh || hasJoint || hasNodeChildren || instanceNode || hasLodChildren)
    {
		std::string fullNodeName = parent->name + std::string(":") + std::string(nodeName);
        Node * node = builder->m_model->CreateNode(fullNodeName);
        ProcessCustomDataAttributes(mesh, node);
        GetTransform(xmlNode, &node->transform);
        ParseFloatArray(xmlNode->first_node("thresholds"), &node->thresholds);
        node->parent = parent;
        parent->children.push_back(node);
        
		if(mesh)
        {
            ProcessMesh(builder, mesh, node);
        }
        if(instanceNode)
        {
            ProcessInstance(builder, instanceNode, node);
        }
  
		// child joint nodes
        for(xml_node* xmlChild = FindChildByName(xmlNode, "joint"); xmlChild!=NULL; xmlChild=FindNextByName(xmlChild, "joint"))
        {
            // used for debug. Can be added to a watch for quick reference...  GJL
			//const char * childName = GetAttributeText(xmlChild, "name", true);
			ProcessNode(builder, xmlChild, node);
        }
        // child nodes
        for(xml_node* xmlChild = FindChildByName(xmlNode, "node"); xmlChild!=NULL; xmlChild=FindNextByName(xmlChild, "node"))
        {
            // used for debug. Can be added to a watch for quick reference...  GJL
			//const char * childName = GetAttributeText(xmlChild, "name", true);
			ProcessNode(builder, xmlChild, node);
        }
        // child lodgroups - treat them the same as nodes.
        for(xml_node* xmlChild = FindChildByName(xmlNode, "lodgroup"); xmlChild!=NULL; xmlChild=FindNextByName(xmlChild, "lodgroup"))
        {
            ProcessNode(builder, xmlChild, node);
        }
    }
}

// ------------------------------------------------------------------------------------------------
void AtgiModelFactory::ProcessXml(xml_node * atg, Model3dBuilder * builder)
{
    // textures
    for(xml_node* node = FindChildByName(atg, "texture"); node != NULL; node = FindNextByName(node, "texture"))
    {
        ProcessTexture(builder, node);
    }
    // shaders
    for(xml_node* node = FindChildByName(atg, "shader"); node != NULL; node = FindNextByName(node, "shader"))
    {
        ProcessShader(builder, node);
    }
    //materials
    for(xml_node* node = FindChildByName(atg, "material"); node != NULL; node = FindNextByName(node, "material"))
    {
        ProcessShader(builder, node);
    }

    // instance meshes
    for(xml_node* scene = FindChildByName(atg, "scene"); scene != NULL; scene = FindNextByName(scene, "scene"))
    {
        for(xml_node* mesh = FindChildByName(scene, "mesh"); mesh != NULL; mesh = FindNextByName(mesh, "mesh"))
        {
            const char* name = GetAttributeText(mesh, "name", true);
            if (!name)
            {
                name = "!missing-name!";
            }
            Node* node = new Node();
            node->name = name;
            ProcessCustomDataAttributes(mesh, node);
            ProcessMesh(builder, mesh, node);
            builder->AddInstance(node);
        }
    }
    

    // scene
    {
        xml_node* scene = FindChildByName(atg, "scene" );
        assert(scene);
        Node * sceneNode = builder->m_model->CreateNode("ATGISceneNode");
        builder->m_model->SetRoot(sceneNode);

        // nodes
        for(xml_node* node = FindChildByName(scene, "node"); node != NULL; node = FindNextByName(node, "node"))
        {
            ProcessNode(builder, node, sceneNode);
        }

        // lod groups, treat them the same as nodes.
        for(xml_node* node = FindChildByName(scene, "lodgroup"); node != NULL; node = FindNextByName(node, "lodgroup"))
        {
            ProcessNode(builder, node, sceneNode);
        }
    }
}

// ----------------------------------------------------------------------------------------------
AtgiModelFactory::AtgiModelFactory(ID3D11Device* device) : XmlModelFactory(device)
{
}

// ----------------------------------------------------------------------------------------------
Resource* AtgiModelFactory::CreateResource(Resource* def)
{
    UNREFERENCED_VARIABLE(def);
    return new Model();
}

}; // namespace LvEdEngine


