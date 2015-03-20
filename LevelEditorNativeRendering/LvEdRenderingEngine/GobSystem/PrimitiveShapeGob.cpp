//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "PrimitiveShapeGob.h"
#include "../Renderer/Texture.h"
#include "../Renderer/TextureLib.h"
#include "../Renderer/Model.h"

using namespace LvEdEngine;

//---------------------------------------------------------------------------
PrimitiveShapeGob::PrimitiveShapeGob( RenderShapeEnum shape )
{
    m_mesh = ShapeLibGetMesh( shape );
    m_color = float4(1,1,1,1);
    m_emissive = float3(0,0,0);
    m_specular = float3(0,0,0);
    m_specPower = 1;
}

void PrimitiveShapeGob::SetDiffuse(wchar_t* filename)
{
    Texture* def  = TextureLib::Inst()->GetDefault(TextureType::DIFFUSE);
    m_diffuse.SetTarget(filename, def);
}        

void PrimitiveShapeGob::SetNormal(wchar_t* filename)
{
    Texture* def  = TextureLib::Inst()->GetDefault(TextureType::NORMAL);
    m_normal.SetTarget(filename,def);
}

//---------------------------------------------------------------------------
PrimitiveShapeGob::~PrimitiveShapeGob()
{
}

//---------------------------------------------------------------------------
// push Renderable nodes
//virtual 
void PrimitiveShapeGob::GetRenderables(RenderableNodeCollector* collector, RenderContext* context)
{
    if ( IsVisible(context->Cam().GetFrustum()) )
    {
		super::GetRenderables(collector, context);

        RenderableNode r;
        SetupRenderable(&r, context);
        
        RenderFlagsEnum flags = (RenderFlagsEnum) (RenderFlags::Textured | RenderFlags::Lit);
        if(r.diffuse.w < 0.996f)   
        {
            r.SetFlag(RenderableNode::kShadowCaster,false);
            flags  = (RenderFlagsEnum)(flags | RenderFlags::AlphaBlend | RenderFlags::DisableDepthWrite);
        }

        collector->Add( r, flags, Shaders::TexturedShader );        
    }    
}


void PrimitiveShapeGob::Update(const FrameTime& fr, UpdateTypeEnum updateType)
{
    bool boundDirty = m_boundsDirty;
    super::Update(fr,updateType);
    m_boundsDirty |= m_worldBoundUpdated;    
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
    r->diffuse = m_color;  
    r->specPower = m_specPower;
    r->emissive = m_emissive;
    r->specular = m_specular;
    r->TextureXForm = m_textureTransform;
    r->textures[TextureType::DIFFUSE] = (Texture*)m_diffuse.GetTarget();
    r->textures[TextureType::NORMAL] = (Texture*)m_normal.GetTarget();
}

