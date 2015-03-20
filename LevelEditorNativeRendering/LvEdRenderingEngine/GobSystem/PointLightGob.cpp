//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "PointLightGob.h"

#include "../Renderer/RenderBuffer.h"
#include "../Renderer/RenderUtil.h"
#include "../Renderer/Model.h"

namespace LvEdEngine
{
PointLightGob::PointLightGob()
{
    // creates and registers a point light with the render sub-system.
    m_light = LightingState::Inst()->CreatePointLight();
    assert(m_light != NULL);
}

PointLightGob::~PointLightGob()
{
    assert(m_light != NULL);
    LightingState::Inst()->DestroyPointLight(m_light);
    m_light = NULL;
}

void PointLightGob::SetAmbient(int color)
{
    ConvertColor(color, &m_light->ambient);
}

void PointLightGob::SetDiffuse(int color)
{
    ConvertColor(color, &m_light->diffuse);
}

void PointLightGob::SetSpecular(int color)
{
    ConvertColor(color, &m_light->specular);
}

void PointLightGob::SetAttenuation(const float3& atten)
{
    m_light->attenuation = float4(atten.x,atten.y,atten.z,1);
}

void PointLightGob::SetRange(float r)
{
    m_light->position.w = r;
}


void PointLightGob::Update(const FrameTime& fr, UpdateTypeEnum updateType)
{
    bool boundDirty = m_boundsDirty;
    super::Update(fr,updateType);
    
    m_boundsDirty = boundDirty;
    if(m_boundsDirty)
    {
        m_localBounds = m_mesh->bounds;        
        UpdateWorldAABB();
    }

    float range = m_light->position.w;
    float3 pos(&m_world.M41);
    m_light->position = float4(pos, range);  
}

};
