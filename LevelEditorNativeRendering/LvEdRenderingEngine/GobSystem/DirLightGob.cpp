//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "DirLightGob.h"

#include "../Renderer/RenderBuffer.h"
#include "../Renderer/RenderUtil.h"
#include "../Renderer/Model.h"

namespace LvEdEngine
{

DirLightGob::DirLightGob()
{
    // creates and registers a box light with the rendering sub-sytem.
    m_light = LightingState::Inst()->CreateDirLight();
    assert(m_light != NULL);
}

DirLightGob::~DirLightGob()
{
    assert(m_light != NULL);
    LightingState::Inst()->DestroyDirLight(m_light);
    m_light = NULL;
}


void DirLightGob::SetAmbient(int color)
{
    ConvertColor(color, &m_light->ambient);
}

void DirLightGob::SetDiffuse(int color)
{
    ConvertColor(color, &m_light->diffuse);
}

void DirLightGob::SetSpecular(int color)
{
    ConvertColor(color, &m_light->specular);
}
void DirLightGob::SetDirection(const float3& v)
{
    m_light->dir = normalize(v);
}

}
