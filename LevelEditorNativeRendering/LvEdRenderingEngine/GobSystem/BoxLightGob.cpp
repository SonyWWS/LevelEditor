//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "BoxLightGob.h"

#include "../Renderer/RenderBuffer.h"
#include "../Renderer/RenderUtil.h"
#include "../Renderer/Model.h"
#include "../Renderer/LineRenderer.h"
#include "../Renderer/TextureLib.h"
#include "../Renderer/ShapeLib.h"

namespace LvEdEngine
{

BoxLightGob::BoxLightGob()
{
    // creates and registers a box light with the rendering sub-sytem.
    m_light = LightingState::Inst()->CreateBoxLight();
    assert(m_light != NULL);
}

BoxLightGob::~BoxLightGob()
{
    assert(m_light != NULL);
    LightingState::Inst()->DestroyBoxLight(m_light);
    m_light = NULL;
}


void BoxLightGob::SetAmbient(int color)
{
    ConvertColor(color, &m_light->ambient);
}

void BoxLightGob::SetDiffuse(int color)
{
    ConvertColor(color, &m_light->diffuse);
}

void BoxLightGob::SetSpecular(int color)
{
    ConvertColor(color, &m_light->specular);
}
void BoxLightGob::SetDirection(const float3& v)
{
    m_light->dir = normalize(v);
}

float3 BoxLightGob::GetDirection()
{
    return m_light->dir;
}

void BoxLightGob::SetAttenuation(const float3& atten)
{
    m_light->attenuation = float4(atten.x,atten.y,atten.z,1);
}

bool BoxLightGob::GetRenderables(RenderableNodeCollector* collector, RenderContext* context)
{     
    
    if (!IsVisible(context->Cam().GetFrustum())) return false;   

    RenderableNode renderable;
    GameObject::SetupRenderable(&renderable,context);
    renderable.mesh = m_mesh;
    renderable.textures[TextureType::DIFFUSE] =  TextureLib::Inst()->GetByName(L"LIGHT.PNG");
    
    float3 objectPos = &m_world.M41;
    Camera& cam = context->Cam();    
    Matrix billboard = Matrix::CreateBillboard(objectPos,cam.CamPos(),cam.CamUp(),cam.CamLook());       
    
    Matrix scale = Matrix::CreateScale(0.4f);
    renderable.WorldXform = scale * billboard;
    
    RenderFlagsEnum flags = RenderFlags::Textured;
    collector->Add( renderable, flags, Shaders::BillboardShader );

    if( context->selection.find(GetInstanceId()) != context->selection.end() )
    {
        /* RenderableNode cubeR;
         GameObject::SetupRenderable(&cubeR,context);
         cubeR.mesh = ShapeLibGetMesh( RenderShape::Cube);
         cubeR.SetFlag( RenderableNode::kShadowCaster, false );
         cubeR.SetFlag( RenderableNode::kShadowReceiver, false );
         cubeR.SetFlag( RenderableNode::kNotPickable,true);
         cubeR.diffuse = m_light->diffuse;         
         cubeR.diffuse.w = 0.2f;
         RenderFlagsEnum rflags  = (RenderFlagsEnum)(RenderFlags::Lit | RenderFlags::AlphaBlend | RenderFlags::DisableDepthWrite);         
         collector->Add( cubeR, rflags, Shaders::TexturedShader );*/
        LineRenderer::Inst()->DrawAABB(m_bounds,float4(1,1,1,1));
    }

    return true;    
}

void BoxLightGob::Update(float dt)
{
    UpdateWorldTransform();
    if(m_boundsDirty)
    {
        m_localBounds = AABB(float3(-0.5f,-0.5f,-0.5f),float3(0.5f,0.5f,0.5f));        
        UpdateWorldAABB();
    }
    m_light->min = m_bounds.Min();
    m_light->max = m_bounds.Max();
}


};
