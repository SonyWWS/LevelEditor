//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "OrcGob.h"
#include <algorithm>

#include "../Renderer/RenderUtil.h"
#include "../Renderer/ShapeLib.h"
#include "../Renderer/RenderBuffer.h"
#include "../Renderer/Model.h"

namespace LvEdEngine
{


// orc functions
// ----------------------------------------------------------------------------------
void OrcGob::SetWeight(float w)
{
    m_weight = w;
}

// ----------------------------------------------------------------------------------
void OrcGob::SetEmotion(int e)
{
    m_emotion = e;
}

// ----------------------------------------------------------------------------------
void OrcGob::SetGoals(int g)
{
    m_goal = g;
}

// ----------------------------------------------------------------------------------
void OrcGob::SetColor(int c)
{
    m_color = c;
}

// ----------------------------------------------------------------------------------
void OrcGob::SetToeColor(int tc)
{
    m_toeColor = tc;
}

// ----------------------------------------------------------------------------------
void OrcGob::AddAnimation(ResourceReference* ref, int /*index*/)
{
    m_animation = ref;
}

// ----------------------------------------------------------------------------------
void OrcGob::RemoveAnimation(ResourceReference* /*ref*/)
{
    m_animation = NULL;
}

// ----------------------------------------------------------------------------------
void OrcGob::AddGeometry(ResourceReference* ref, int /*index*/)
{
    m_geometry = ref;
    m_modelTransforms.clear();
    m_renderables.clear();
    InvalidateBounds();
    InvalidateWorld();
}

// ----------------------------------------------------------------------------------
void OrcGob::RemoveGeometry(ResourceReference* /*ref*/)
{
    AddGeometry(NULL, -1);
}

// ----------------------------------------------------------------------------------
void OrcGob::AddFriends(GameObjectReference* ref, int index)
{

    if(index == -1)
    {
        m_friends.push_back(ref);
    }
    else
    {
        m_friends.insert(m_friends.begin() + index, ref);
    }

}

// ----------------------------------------------------------------------------------
void OrcGob::RemoveFriends(GameObjectReference* ref)
{
    auto it = std::find(m_friends.begin(), m_friends.end(), ref);
    m_friends.erase(it);
}

// ----------------------------------------------------------------------------------
void OrcGob::AddTarget(GameObjectReference* ref, int /*index*/)
{
    m_target = ref;
}

// ----------------------------------------------------------------------------------
void OrcGob::RemoveTarget(GameObjectReference* /*ref*/)
{
    m_target = NULL;
}

// ----------------------------------------------------------------------------------
void OrcGob::AddChildren(OrcGob* child, int index)
{
    assert(child->Parent() == NULL);
    child->SetParent(this);
    if(index == -1)
    {
        m_children.push_back(child);
    }
    else
    {
        m_children.insert(m_children.begin() + index, child);
    }
}

// ----------------------------------------------------------------------------------
void OrcGob::RemoveChildren(OrcGob* child)
{
    assert(child->Parent() == this);
    child->SetParent(NULL);
    auto it = std::find(m_children.begin(), m_children.end(), child);
    m_children.erase(it);
}

// ----------------------------------------------------------------------------------
OrcGob::OrcGob()
{
    m_geometry = NULL;
    m_animation = NULL;
    m_target = NULL;
}

// ----------------------------------------------------------------------------------
OrcGob::~OrcGob()
{
    SAFE_DELETE(m_geometry);
    SAFE_DELETE(m_animation);
    SAFE_DELETE(m_target);

    for(auto it = m_friends.begin(); it != m_friends.end(); ++it)
    {
        delete (*it);
    }
    m_friends.clear();

    for(auto it = m_children.begin(); it != m_children.end(); ++it)
    {
        delete (*it);
    }
    m_children.clear();
}

// ----------------------------------------------------------------------------------
void OrcGob::GetRenderables(RenderableNodeCollector* collector, RenderContext* context)
{   
	if (!IsVisible())
		return;

	super::GetRenderables(collector, context);

    RenderFlagsEnum flags = (RenderFlagsEnum) (RenderFlags::Textured | RenderFlags::Lit);

    if (!m_renderables.empty())
    {
        collector->Add(m_renderables.begin(), m_renderables.end(), flags, Shaders::TexturedShader);
    }
    else
    {
        Mesh* mesh = ShapeLibGetMesh(RenderShape::Cube);
               
        RenderableNode r;
        r.mesh = mesh;
        r.diffuse = float4(0.0f,0.3f,0,1);
        r.objectId = GetInstanceId();
        r.WorldXform = m_world;
        r.bounds = m_bounds;
        LightingState::Inst()->UpdateLightEnvironment(r);
        collector->Add(r, flags, Shaders::TexturedShader);
    }
}

// ----------------------------------------------------------------------------------
void OrcGob::BuildRenderables()
{
    m_renderables.clear();
    Model* model = NULL;
    assert(m_geometry);
    model = (Model*)m_geometry->GetTarget();

    // If !model or !IsReady, then UpdateBegin should have returned false and UpdateEnd|BuildRenderables
    // should not be called.  Assert this is the case.
    assert(model && model->IsReady());

    const NodeDict& nodes = model->Nodes();
    for(auto nodeIt = nodes.begin(); nodeIt != nodes.end(); ++nodeIt)
    {
        Node* node = nodeIt->second;
        assert(m_modelTransforms.size() >= node->index);
        const Matrix& world = m_modelTransforms[node->index]; // transform array holds world matricies already, not local
        for(auto geoIt = node->geometries.begin(); geoIt != node->geometries.end(); ++geoIt)
        {
            Geometry* geo = (*geoIt);
            Material * mat = geo->material;
            RenderableNode renderNode;
            renderNode.mesh = geo->mesh;
            renderNode.WorldXform = world;
            renderNode.bounds = geo->mesh->bounds;
            renderNode.bounds.Transform(renderNode.WorldXform);
            renderNode.objectId = GetInstanceId();
            renderNode.diffuse =  mat->diffuse;
            renderNode.specular = mat->specular.xyz();
            renderNode.specPower = mat->power;
            renderNode.SetFlag( RenderableNode::kShadowCaster, GetCastsShadows() );
            renderNode.SetFlag( RenderableNode::kShadowReceiver, GetReceivesShadows() );

            LightingState::Inst()->UpdateLightEnvironment(renderNode);

            for(unsigned int i = TextureType::MIN; i < TextureType::MAX; ++i)
            {
                renderNode.textures[i] = geo->material->textures[i];
            }
            m_renderables.push_back(renderNode);
        }
    }
}


void OrcGob::Update(const FrameTime& fr, UpdateTypeEnum updateType)
{
    bool boundDirty = m_boundsDirty;
    bool udpateXforms = m_worldDirty;
    super::Update(fr,updateType);
	udpateXforms |= m_worldXformUpdated;
    Model* model = m_geometry ? (Model*)m_geometry->GetTarget() : NULL;                     
    if( model && model->IsReady())
    {
        if(m_modelTransforms.empty() || udpateXforms)
        {
             const MatrixList& matrices = model->AbsoluteTransforms();
             m_modelTransforms.resize(matrices.size());
             for( unsigned int i = 0; i < m_modelTransforms.size(); ++i)
             {
                 m_modelTransforms[i] = matrices[i] * m_world; // transform matrix array now holds complete world transform.
             }

             BuildRenderables();
             boundDirty = true;
        }                               
    }

    m_boundsDirty = boundDirty;
    if(m_boundsDirty)        
    {                  
        if(!m_modelTransforms.empty())
        {                
           // assert(model && model->IsReady());
            m_localBounds = model->GetBounds();                                
            if(m_parent) m_parent->InvalidateBounds();                
        }
        else
        {
            m_localBounds = AABB(float3(-0.5f,-0.5f,-0.5f), float3(0.5f,0.5f,0.5f));                
        }      
        this->UpdateWorldAABB();            
    }

    if(RenderContext::Inst()->LightEnvDirty)
    {
        // update light env.
        for(auto  renderNode = m_renderables.begin(); renderNode != m_renderables.end(); renderNode++)
        {
            LightingState::Inst()->UpdateLightEnvironment(*renderNode);
        }
    }       

}

}; // namespace
