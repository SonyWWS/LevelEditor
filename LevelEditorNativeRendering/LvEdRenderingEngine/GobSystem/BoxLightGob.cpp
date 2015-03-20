//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "BoxLightGob.h"

#include "../Renderer/RenderBuffer.h"
#include "../Renderer/Model.h"
#include "../Renderer/LineRenderer.h"
#include "../Renderer/TextureLib.h"
#include "../Renderer/ShapeLib.h"
#include "GameObjectComponent.h"

namespace LvEdEngine
{

BoxLightGob::BoxLightGob()
{
    // creates and registers a box light with the rendering sub-sytem.
    m_light = LightingState::Inst()->CreateBoxLight();
    assert(m_light != NULL);
    m_localBounds = AABB(float3(-0.5f,-0.5f,-0.5f),float3(0.5f,0.5f,0.5f));
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

void BoxLightGob::GetRenderables(RenderableNodeCollector* collector, RenderContext* context)
{     
    
	if (!IsVisible(context->Cam().GetFrustum())) return;

	// No need to call super::GetRenderables	
	//super::GetRenderables(collector, context);
	// need to call GetRenderables() for each component.
	for (auto it = m_components.begin(); it != m_components.end(); ++it)
		(*it)->GetRenderables(collector, context);
	
    RenderableNode renderable;
    GameObject::SetupRenderable(&renderable,context);
    renderable.mesh = m_mesh;
    renderable.textures[TextureType::DIFFUSE] =  TextureLib::Inst()->GetByName(L"Light.png");
    
    float3 objectPos = &m_world.M41;
    Camera& cam = context->Cam();    
    Matrix billboard = Matrix::CreateBillboard(objectPos,cam.CamPos(),cam.CamUp(),cam.CamLook());       
    
    Matrix scale = Matrix::CreateScale(0.4f);
    renderable.WorldXform = scale * billboard;
    
    RenderFlagsEnum flags = RenderFlags::Textured;
    collector->Add( renderable, flags, Shaders::BillboardShader );
}

void BoxLightGob::Update(const FrameTime& fr, UpdateTypeEnum updateType)
{    
    super::Update(fr,updateType);
    m_light->min = m_bounds.Min();
    m_light->max = m_bounds.Max();
}

};
