//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "PrimitiveShapeGob.h"
#include "../Renderer/Texture.h"
#include "../Renderer/Model.h"

using namespace LvEdEngine;

//---------------------------------------------------------------------------
PrimitiveShapeGob::PrimitiveShapeGob( RenderShapeEnum shape )
{
    m_mesh = ShapeLibGetMesh( shape );  
}

//---------------------------------------------------------------------------
PrimitiveShapeGob::~PrimitiveShapeGob()
{
}

//---------------------------------------------------------------------------
// push Renderable nodes
//virtual 
bool PrimitiveShapeGob::GetRenderables(RenderableNodeCollector* collector, RenderContext* context)
{
    if ( IsVisible(context->Cam().GetFrustum()) )
    {
        RenderableNode r;
        SetupRenderable(&r, context);
        
        RenderFlagsEnum flags = (RenderFlagsEnum) (RenderFlags::Textured | RenderFlags::Lit);
        if(r.diffuse.w < 0.996f)   
        {
            r.SetFlag(RenderableNode::kShadowCaster,false);
            flags  = (RenderFlagsEnum)(flags | RenderFlags::AlphaBlend | RenderFlags::DisableDepthWrite);
        }

        collector->Add( r, flags, Shaders::TexturedShader );
        return true;
    }
    return false;
}


void PrimitiveShapeGob::Update(float dt)
{
    UpdateWorldTransform();
    if(m_boundsDirty)
    {
        m_localBounds = m_mesh->bounds;        
        UpdateWorldAABB();
    }
}

//---------------------------------------------------------------------------
// virtual
void PrimitiveShapeGob::SetupRenderable(RenderableNode* r, RenderContext* context)
{
    GameObject::SetupRenderable(r, context);
    r->mesh = m_mesh;
    ConvertColor(m_color, &r->diffuse);
    r->TextureXForm = m_textureTransform;
    r->textures[TextureType::DIFFUSE] = (Texture*)m_diffuse.GetTarget();
    r->textures[TextureType::NORMAL] = (Texture*)m_normal.GetTarget();

}

