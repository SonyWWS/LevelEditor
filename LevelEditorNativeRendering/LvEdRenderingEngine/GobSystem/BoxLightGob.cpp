//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "BoxLightGob.h"

#include "../Renderer/RenderBuffer.h"
#include "../Renderer/RenderUtil.h"
#include "../Renderer/Model.h"
#include "../Renderer/LineRenderer.h"

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
    if(LightGob::GetRenderables(collector,context))
    {
        LineRenderer::Inst()->DrawAABB(m_bounds,m_light->diffuse);
        return true;
    }
    return false;        
}



void BoxLightGob::Update(float dt)
{
    UpdateWorldTransform();
    if(m_boundsDirty)
    {
        m_localBounds = m_mesh->bounds;        
        UpdateWorldAABB();
    }
    m_light->min = m_bounds.Min();
    m_light->max = m_bounds.Max();
}


};
