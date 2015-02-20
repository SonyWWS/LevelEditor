//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "../VectorMath/V3dMath.h"
#include "../Renderer/Model.h"
#include "../Core/Utils.h"
#include "../Core/Logger.h"
#include "../ResourceManager/ResourceManager.h"
#include "Model3dBuilder.h"
#include "rapidxmlhelpers.h"
#include "XmlModelFactory.h"
#include "ColladaModelFactory.h"

namespace LvEdEngine
{

// ------------------------------------------------------------------------------------------------
template <class T>
bool ColladaModelFactory::CopyFloatsFromSource(Model3dBuilder * builder, const char * sourceName, const std::vector<Source> * sources, std::vector<T> * dst)
{
    // find the correct source
    for (auto it = sources->begin(); it != sources->end(); it++)
    {
        if (strcmp(it->id, sourceName) == 0 && it->float_array.size() > 0)
        {
            // copy (source stride might be different than dst)
            const float* src = &it->float_array[0];            
            for (uint32_t i = 0; i < it->float_array.size(); i += it->stride)
            {
                T value(src + i);
                dst->push_back(value);
            }
            return true;
        }
    }

    // no source found, log an error
    ParseError("Could not find 'source', '%s', for mesh, '%s'\n", sourceName, builder->m_mesh.mesh->name.c_str());
    return false;
}

// ------------------------------------------------------------------------------------------------
void ColladaModelFactory::ProcessInput(Model3dBuilder * builder, xml_node* input, UINT offset, std::vector<Source> * sources)
{
    const char * semantic = GetAttributeText(input, "semantic", true);
    const char * sourceName = GetAttributeText(input, "source", true);
    if (!semantic)  return; 
    if (!sourceName) return;
    if (*sourceName!='#')
    {
        ParseError("source, '%s', expected to start with '#'\n", sourceName);
        return;
    }
    sourceName++;

    if(builder->m_mesh.poly.hasPos == false && strcmp(semantic, "POSITION")==0)
    {
        if (CopyFloatsFromSource(builder, sourceName, sources, &builder->m_mesh.source.pos))
        {
            builder->m_mesh.poly.posOffset = offset;
            builder->m_mesh.poly.hasPos = true;
        }
    }
    if(builder->m_mesh.poly.hasNor == false && strcmp(semantic, "NORMAL")==0)
    {
        if (CopyFloatsFromSource(builder, sourceName, sources, &builder->m_mesh.source.nor))
        {
            builder->m_mesh.poly.norOffset = offset;
            builder->m_mesh.poly.hasNor = true;
        }
    }
    if(builder->m_mesh.poly.hasTex == false && strcmp(semantic, "TEXCOORD")==0)
    {
        if (CopyFloatsFromSource(builder, sourceName, sources, &builder->m_mesh.source.tex))
        {
            builder->m_mesh.poly.texOffset = offset;
            builder->m_mesh.poly.hasTex = true;
        }
    }
}

// ------------------------------------------------------------------------------------------------
void ColladaModelFactory::ProcessPolyListFeatures(Model3dBuilder * builder, xml_node* polylist, std::vector<Source> * sources)
{
    UINT stride=0;
    builder->Mesh_ResetSourceInfo();
    builder->Mesh_ResetPolyInfo();

    for(xml_node* input = FindChildByName(polylist, "input"); input != NULL; input = FindNextByName(input, "input"))
    {
        const char * semantic = GetAttributeText(input, "semantic", true);
        if (!semantic) { continue; }

        UINT offset = GetAttributeUINT(input, "offset");
        //++stride;
        stride = stride > offset ? stride:offset;
        if(strcmp(semantic, "VERTEX")==0)
        {
            // the 'VERTEX' semantic defines multiple-inputs using the same indicie offset.
            xml_node* mesh = polylist->parent();
            xml_node* vertex = FindChildByName(mesh, "vertices");
            xml_node* vinput;
            for(vinput = FindChildByName(vertex, "input"); vinput != NULL; vinput = FindNextByName(vinput, "input"))
            {
                ProcessInput(builder, vinput, offset, sources);
            }
        }
        else
        {
            ProcessInput(builder, input, offset, sources);
        }
    }
    // stride is one greater than the largest offset.
    ++stride;

    builder->m_mesh.poly.stride = stride; // the number of indices per vertex in the poly list
}

// ------------------------------------------------------------------------------------------------
void ColladaModelFactory::ProcessSources(std::vector<Source> * sources, Model3dBuilder * /*builder*/, xml_node* mesh)
{
    // first setup our 'sources'
    for(xml_node* xmlSource = FindChildByName(mesh, "source"); xmlSource != NULL; xmlSource=FindNextByName(xmlSource, "source"))
    {
        // get and validate id
        const char * id = GetAttributeText(xmlSource, "id", true);
        if (!id) { continue; }

        // get and validate stride
        std::vector<xml_node*> accessors;
        FindAllByName(xmlSource, "accessor", true, &accessors);
        if (accessors.size() != 1)
        {
            ParseError("'%s' source should have only one 'accessor' descendant\n", id);
            continue;
        }
        const char * stride = GetAttributeText(accessors[0], "stride", true);
        if (!stride) { continue; }

        sources->resize(sources->size() + 1);
        Source & source = sources->back();

        source.id = id;
        source.stride = atoi(stride);
       

        // get values
        xml_node * float_array = FindChildByName(xmlSource, "float_array");
        if (!float_array)
        {
            ParseError("'%s' source has no 'float_array' child\n", id);
            continue;
        }
        ParseFloatArray(float_array, &source.float_array);
    }
}

// ------------------------------------------------------------------------------------------------
void ColladaModelFactory::ProcessGeo(Model3dBuilder * builder, xml_node* geo)
{
  // process meshes
  for(xml_node* mesh = FindChildByName(geo, "mesh"); geo != NULL; geo=FindNextByName(geo, "mesh"))
  {
    const char * id = GetAttributeText(geo, "id", true);
    if (!id)
    {
        id = "!missing-id!";
    }

    //start a new mesh
    builder->Mesh_Reset();
    builder->Mesh_Begin(id);

    std::vector<Source> sources;

    // setup our 'source' arrays
    ProcessSources(&sources, builder, mesh);
    // process 'polylists'
    for(xml_node* polylist = FindChildByName(mesh, "polylist"); polylist != NULL; polylist = FindNextByName(polylist, "polylist"))
    {
      // process the inputs to determine index stride & offsets
      ProcessPolyListFeatures(builder, polylist, &sources);

      // get the poly index list and vcount per poly.
      ParseUINTArray(FindChildByName(polylist, "vcount"), &builder->m_mesh.poly.vcount);
      ParseUINTArray(FindChildByName(polylist, "p"), &builder->m_mesh.poly.indices);
      
      // tell the builder to process the poly/indices data to build vertex/index data
      builder->Mesh_SetPrimType("POLYGONS");
      builder->Mesh_AddPolys();
    }
    // process 'triangles'
    for(xml_node* triangles = FindChildByName(mesh, "triangles"); triangles != NULL; triangles = FindNextByName(triangles, "triangles"))
    {
      ProcessPolyListFeatures(builder, triangles, &sources);
      ParseUINTArray(FindChildByName(triangles, "p"), &builder->m_mesh.poly.indices);
      builder->Mesh_SetPrimType("TRIANGLES");
      builder->Mesh_AddTriangles();
    }
    // process 'tristrips'
    for(xml_node* tristrips = FindChildByName(mesh, "tristrips"); tristrips != NULL; tristrips = FindNextByName(tristrips, "tristrips"))
    {
      ProcessPolyListFeatures(builder, tristrips, &sources);
      ParseUINTArray(FindChildByName(tristrips, "p"), &builder->m_mesh.poly.indices);
      builder->Mesh_SetPrimType("TRISTRIPS");
      builder->Mesh_AddTriStrips();
    }
    // process 'trifans'
    for(xml_node* trifans = FindChildByName(mesh, "trifans"); trifans != NULL; trifans = FindNextByName(trifans, "trifans"))
    {
      ProcessPolyListFeatures(builder, trifans, &sources);
      ParseUINTArray(FindChildByName(trifans, "p"), &builder->m_mesh.poly.indices);
      builder->Mesh_SetPrimType("TRIFANS");
      builder->Mesh_AddTriFans();
    }
    builder->Mesh_End();
  }
}

// ------------------------------------------------------------------------------------------------
void ColladaModelFactory::ProcessMaterial(Model3dBuilder * builder, xml_node* node)
{
    const char * matId = GetAttributeText(node, "id", true);
    if (!matId) { return; }

    xml_node* effect = FindChildByName(node, "instance_effect");
    if (effect)
    {
        const char * effectId = GetAttributeText(effect, "url", true);
        if (!effectId) { return; }
        effectId++; // strip off the #
        builder->m_material.material2effect[matId] = effectId;
    }

}

// ------------------------------------------------------------------------------------------------
void ColladaModelFactory::ParseEffectColor(xml_node* node, const char* name, float4* out)
{
    if(node)
    {
        // get 'color' descendant
        xml_node* parentNode = FindFirstByName(node, name);
        xml_node* colorNode = FindChildByName(parentNode, "color");
        if (colorNode)
        {                
            // get 4 floats
            if (!ParseVector4(colorNode, out))
            {
                ParseError("'%s' 'color' element has fewer than required 4 floats\n", name);
            }
        }
    }
}
// ------------------------------------------------------------------------------------------------
void ColladaModelFactory::ParseEffectFloat(xml_node* node, const char * name, float * out)
{
    if(node)
    {
        // get 'float' descendant
        xml_node* parentNode = FindFirstByName(node, name);
        xml_node* floatNode = FindFirstByName(parentNode, "float");
        if (floatNode)
        {
            // get single floats
            if (!ParseFloat(floatNode, out))
            {
                ParseError("'%s' 'float' element has no floats\n", name);
            }
        }
    }
}

// ------------------------------------------------------------------------------------------------
void ColladaModelFactory::ParseEffectTexture( xml_node* node, const char * name, std::string * out)
{
    if(node)
    {
        // get 'texture' descendant
		xml_node* childNode = FindFirstByName(node, name);
		xml_node* textureNode = FindFirstByName(childNode, "texture");
        const char * val = GetAttributeText(textureNode, "texture", false);
        if(val)
        {
              *out = val;
        }
    }
}

// ------------------------------------------------------------------------------------------------
const std::string & ColladaModelFactory::FindAndValidateString(std::map<std::string, std::string> * stringMap, const std::string & index)
{
    // if empty, just return another empty string
    if (index.empty())
    {
        return index;
    }

    // look up 'index' in map
    auto it = stringMap->find(index);
    if (it == stringMap->end())
    {
        static const std::string c_emptyString;
        ParseError("could not find '%s'\n", index.c_str());
        return c_emptyString;
    }
    return it->second;
}

// ------------------------------------------------------------------------------------------------
const std::string& ColladaModelFactory::GetTextureFileFromSampler(Model3dBuilder* builder, const std::string& samplerName)
{
    const std::string & surfaceName = FindAndValidateString(&builder->m_material.sampler2surface, samplerName);
    const std::string & imageName = FindAndValidateString(&builder->m_material.surface2image, surfaceName);
    return FindAndValidateString(&builder->m_material.image2file, imageName);
}

// ------------------------------------------------------------------------------------------------
void ColladaModelFactory::ProcessEffect(Model3dBuilder * builder, xml_node* node)
{
    const char * id = GetAttributeText(node, "id", true);
    if (!id) { return; }

    // create a material
    Material * mat = builder->m_model->CreateMaterial(id);

    { // process surfaces, track surface name to image name
        std::vector<xml_node*> nodeSet;
        FindAllByName(node->first_node(), "surface", true, &nodeSet);
        for( UINT i = 0; i < nodeSet.size(); ++i)
        {
            xml_node* surface = nodeSet[i];

            const char * surfaceName = GetAttributeText(surface->parent(), "sid", true);
            if (!surfaceName) { continue; }

            //ie: <surface><init_from>filename</init_from></surface>
            // get and validate 'init_from'
            xml_node * init_from = FindChildByName(surface, "init_from");
            if (!init_from)
            {
                ParseError("<surface> missing child <init_from>\n");
                continue;
            }   
			const char * imageName = init_from->value();
            builder->m_material.surface2image[surfaceName] = imageName;
        }
    }
  
    { // process samplers, track sampler name to surface name
        std::vector<xml_node*> nodeSet;
        FindAllByName(node->first_node(), "sampler2D", true, &nodeSet);
        for( UINT i = 0; i < nodeSet.size(); ++i)
        {
            xml_node* sampler = nodeSet[i];

            const char * samplerName = GetAttributeText(sampler->parent(), "sid", true);
            if (!samplerName) { continue; }

            //ie: <sampler2D><source>surfacename</source></sampler2D>
            // get and validate 'source'
            xml_node * source = FindChildByName(sampler, "source");
            if (!source)
            {
                ParseError("<sampler2D> missing child <source>\n");
                continue;
            }

			const char* surfaceName = source->value();
			if (!surfaceName)
			{
				ParseError("<sampler2D><source>  missing value   </source></sampler2D>\n");
				continue;
			}			
            builder->m_material.sampler2surface[samplerName] = surfaceName;
        }
    }

    // get the 1st technique
    std::string samplerName;
	xml_node* technique = FindChildByName(node->first_node(), "technique");	
    if(technique)
    {   // process 'standard' technique		
        ParseEffectColor(technique, "diffuse", &mat->diffuse);
        ParseEffectColor(technique, "ambient", &mat->ambient);
        ParseEffectColor(technique, "specular", &mat->specular);
        ParseEffectColor(technique, "emission", &mat->emissive);
        ParseEffectFloat(technique, "shininess", &mat->power);
        ParseEffectTexture(technique, "diffuse", &samplerName);
    }
	mat->texNames[TextureType::DIFFUSE] = GetTextureFileFromSampler(builder, samplerName);

	// find normal map
	xml_node* extra = FindChildByName(technique, "extra");
	xml_node* bump = FindFirstByName(extra, "bump");
	xml_node* texture = FindChildByName(bump, "texture");
	const char* sampName = GetAttributeText(texture, "texture", false);
    if(sampName)
        mat->texNames[TextureType::NORMAL] = GetTextureFileFromSampler(builder, sampName);
	

    // process any 'bind' params
    std::vector<xml_node*> bindNodes;
    FindAllByName(technique, "bind", true, &bindNodes);
    for(auto it = bindNodes.begin(); it != bindNodes.end(); ++it)
    {		
        xml_node* bind = (*it);

        const char* symbolName = GetAttributeText(bind, "symbol", true);
        const char* symbolValue = GetAttributeText(bind->first_node("param"), "ref", true);
        if (!symbolName) { continue; }
        if (!symbolValue) { continue; }

        if(strcmp(symbolName, "ColorSampler")==0)
        {            
            mat->texNames[TextureType::DIFFUSE] = GetTextureFileFromSampler(builder, std::string(symbolValue));
        }
        if(strcmp(symbolName, "NormalSampler")==0)
        {
            mat->texNames[TextureType::NORMAL] = GetTextureFileFromSampler(builder, std::string(symbolValue));
        }
    }


        
}

// ------------------------------------------------------------------------------------------------
void ColladaModelFactory::ProcessImage(Model3dBuilder * builder, xml_node* node)
{
  const char * imageId = GetAttributeText(node, "id", true);
  if (!imageId) { return; }

  xml_node* source = FindChildByName(node, "init_from");
  if(source)
  {
    const char * filepath = (const char*)source->first_node()->value();
    builder->m_material.image2file[imageId] = filepath;
  }
}

// ------------------------------------------------------------------------------------------------
void ColladaModelFactory::GetTransform(xml_node* node, Matrix * out )
{
    Matrix rotateX,rotateY,rotateZ,translate,scale;
    rotateX.MakeIdentity();
    rotateY.MakeIdentity();
    rotateZ.MakeIdentity();
    translate.MakeIdentity();
    scale.MakeIdentity();
  

    for(xml_node* child = GetChildEle(node); child != NULL; child=GetNextEle(child))
    {
        const char * name = (const char*)child->name();
        const char * sid = GetAttributeText(child, "sid", false);
        if (!name) { continue; }
        if (!sid) { continue; }

        if(strcmp(name, "rotate")==0)
        {
            if(strcmp(sid, "rotateX")==0)
            {
                float4 temp;
                if (!ParseVector4(child, &temp))
                {
                    ParseError("Failed to parse '%s' transform (expected 4 floats)'\n", sid);
                } 
                else 
                {
                    rotateX = Matrix::CreateRotationX(temp.w);                    
                }
            }
            else if(strcmp(sid, "rotateY")==0)
            {
                float4 temp;
                if (!ParseVector4(child, &temp)) 
                {
                    ParseError("Failed to parse '%s' transform (expected 4 floats)'\n", sid);
                } 
                else 
                {
                    rotateY = Matrix::CreateRotationY(temp.w);                
                }
            }
            else if(strcmp(sid, "rotateZ")==0)
            {
                float4 temp;
                if (!ParseVector4(child, &temp)) 
                {
                    ParseError("Failed to parse '%s' transform (expected 4 floats)'\n", sid);
                } 
                else
                {
                    rotateZ = Matrix::CreateRotationZ(temp.w);                    
                }
            }
        }
        else if(strcmp(name, "translate")==0)
        {
            float3 temp;
            if (!ParseVector3(child, &temp)) 
            {
                ParseError("Failed to parse '%s' transform (expected 3 floats)'\n", name);
            } 
            else 
            {
                translate = Matrix::CreateTranslation(temp);                
            }
        }
        else if(strcmp(name, "scale")==0)
        {
            float3 temp;
            if (!ParseVector3(child, &temp)) 
            {
                ParseError("Failed to parse '%s' transform (expected 3 floats)'\n", name);
            } 
            else
            {
                scale = Matrix::CreateScale(temp);                
            }
        }
        // unhandled transform node
        else
        {
            Logger::Log(OutputMessageType::Warning, "Unexpected transform node, '%s'\n", name);
        }
    }

    Matrix tmp = scale * rotateX;
    tmp = tmp * rotateY;
    tmp = tmp * rotateZ;
    tmp = tmp * translate;
    *out = tmp;
}

// ------------------------------------------------------------------------------------------------
void ColladaModelFactory::ProcessController(xml_node * xmlController, ColladaModelFactory::ControllerToGeo * controllerToGeo)
{
    const char * id = GetAttributeText(xmlController, "id", true);
    if (!id) { return; }

    xml_node* skin = FindChildByName(xmlController, "skin");
    if (skin)
    {
        const char * skinSource = GetAttributeText(skin, "source", true);
        if (!skinSource) { return; }

        // skip #
        ++skinSource;

        (*controllerToGeo)[id] = skinSource;
    }
}


// ------------------------------------------------------------------------------------------------
Node* ColladaModelFactory::ProcessSceneNode(Model3dBuilder * builder, xml_node* node, Node*, ColladaModelFactory::ControllerToGeo * controllerToGeo)
{
    xml_node* instanceGeo = FindChildByName(node, "instance_geometry");
    xml_node* instanceController = FindChildByName(node, "instance_controller");

    const char * nodeName = GetAttributeText(node, "id", true);
    if (!nodeName)
    {
        nodeName = "!missing-id!";
    }

    Node * modelNode = builder->m_model->CreateNode(nodeName);
    GetTransform(node, &modelNode->transform);

    // not an error condition
    xml_node* instance = instanceGeo ? instanceGeo : instanceController;
    if (!instance)
    {
        return modelNode;
    }

    xml_node* instanceMat = FindFirstByName(instance, "instance_material");

    // validate url/geoName
    const char * geoName = GetAttributeText(instance, "url", true);
    if (!geoName)
    {
        return modelNode;
    }
    if (*geoName!='#')
    {
        ParseError("attribute 'url', '%s', expected to start with '#'\n", geoName);
        return modelNode;
    }
    // skip the # prefix
    ++geoName;

    // validate target/matName
    const char * matName = GetAttributeText(instanceMat, "target", true);
    if (!matName)
    {
        return modelNode;
    }
    if (*matName!='#')
    {
        ParseError("target, '%s', expected to start with '#'\n", matName);
        return modelNode;
    }
    // skip the # prefix
    ++matName;

    if (instanceController)
    {
        auto it = controllerToGeo->find(geoName);
        if (it == controllerToGeo->end())
        {
            ParseError("Could not find controller, '%s', referenced from node, '%s'\n",
                                                                                            geoName, nodeName);
            return modelNode;
        }
        else
        {
            geoName = it->second.c_str();
        }
    }

    // look up mesh from the library (error reported in GetMesh function)
    Mesh * mesh = builder->m_model->GetMesh(geoName);
    if (!mesh)
    {
        return modelNode;
    }

    // from the material name, get the 'effect' name, which is the actual material.
    const std::string & effectName = builder->m_material.material2effect[matName];
    std::string actualGeoName = effectName + "_Geo";
    Geometry * geo = builder->m_model->CreateGeometry(actualGeoName);
    modelNode->geometries.push_back(geo);

    geo->material = builder->m_model->GetMaterial(effectName);
    geo->mesh = mesh;
    return modelNode;
}

// ------------------------------------------------------------------------------------------------
void ColladaModelFactory::ProcessChildNodes(Model3dBuilder * builder, xml_node* xmlParent, Node * parent, ControllerToGeo * controllerToGeo)
{
    for(xml_node* node = FindChildByName(xmlParent, "node"); node != NULL; node=FindNextByName(node, "node"))
    {
        Node * modelNode = ProcessSceneNode(builder, node, parent, controllerToGeo);

        // register with parent and model
        parent->children.push_back(modelNode);
        modelNode->parent = parent;

        // process child nodes
        ProcessChildNodes(builder, node, modelNode, controllerToGeo);
    }
}

// ------------------------------------------------------------------------------------------------
void ColladaModelFactory::ProcessXml(xml_node * rootXml, Model3dBuilder * builder)
{
    // process images
    xml_node* images = FindChildByName(rootXml, "library_images");
    for(xml_node* node = FindChildByName(images, "image"); node != NULL; node=FindNextByName(node, "image"))
    {
        ProcessImage(builder, node);
    }

    // process materials
    xml_node* mats = FindChildByName(rootXml, "library_materials");
    for(xml_node* node = FindChildByName(mats, "material"); node != NULL; node=FindNextByName(node, "material"))
    {
        ProcessMaterial(builder, node);
    }

    // process effects
    xml_node* effects = FindChildByName(rootXml, "library_effects");
    for(xml_node* node = FindChildByName(effects, "effect"); node != NULL; node=FindNextByName(node, "effect"))
    {
        ProcessEffect(builder, node);
    }

    // process geometries
    xml_node* geos = FindChildByName(rootXml, "library_geometries");
    for(xml_node* node = FindChildByName(geos, "geometry"); node != NULL; node=FindNextByName(node, "geometry"))
    {
        ProcessGeo(builder, node);
    }

    // process controllers
    ControllerToGeo controllerToGeo;
    xml_node* controllers = FindChildByName(rootXml, "library_controllers");
    for(xml_node* node = FindChildByName(controllers, "controller"); node != NULL; node=FindNextByName(node, "controller"))
    {
        ProcessController(node, &controllerToGeo);
    }

    // process visual scene
    xml_node* scenes = FindChildByName(rootXml, "library_visual_scenes");
    xml_node* scene = FindChildByName(scenes, "visual_scene");  // just grab the 1st visual scene
    if(scene)
    {
        const char * nodeName = GetAttributeText(scene, "id", true);
        if (!nodeName)
        {
            nodeName = "!missing-id!";
        }
        Node * rootNode = builder->m_model->CreateNode(nodeName);

        xml_node* asset = rootXml->first_node("asset");
        xml_node* upaxisNode =  asset->first_node("up_axis");        
        
        if(upaxisNode && strcmpi(upaxisNode->value(),"Z_UP") == 0)
        {
            rootNode->transform = Matrix::CreateRotationX(-PiOver2);
        }

        builder->m_model->SetRoot(rootNode);
        ProcessChildNodes(builder, scene, rootNode, &controllerToGeo);
    }
}

// ----------------------------------------------------------------------------------------------
ColladaModelFactory::ColladaModelFactory(ID3D11Device* device) : XmlModelFactory(device)
{
}
 
// ----------------------------------------------------------------------------------------------
Resource* ColladaModelFactory::CreateResource(Resource* def)
{
    UNREFERENCED_VARIABLE(def);
    return new Model();
}

}; // namespace LvEdEngine

