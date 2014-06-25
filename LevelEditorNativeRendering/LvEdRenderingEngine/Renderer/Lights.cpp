//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "Lights.h"
#include <set>
#include "../VectorMath/V3dMath.h"
#include "../VectorMath/CollisionPrimitives.h"
#include "Renderable.h"

namespace LvEdEngine
{

//-------------------------------------------------------------------------------------------------
LightingState::LightingState()
{
    //
    // The HLSL shader does not use light counts (i.e., it doesn't loop through the array),
    // so we point any unused slots at these "empty" light structures.
    //

    m_noDirLight.ambient = m_noDirLight.diffuse = m_noDirLight.specular = float3(0,0,0);
    m_noDirLight.dir = float3(0,-1,0);

    m_noBoxLight.ambient = m_noBoxLight.diffuse = m_noBoxLight.specular = float3(0,0,0);
    m_noBoxLight.min = float3(FLT_MAX, FLT_MAX, FLT_MAX);
    m_noBoxLight.max = float3(-FLT_MAX, -FLT_MAX, -FLT_MAX);

    m_noPointLight.ambient = m_noPointLight.diffuse = m_noPointLight.specular = float3(0,0,0);
    m_noPointLight.position = float4(0,0,0,0);
}

//-------------------------------------------------------------------------------------------------
LightingState * LightingState::Inst()
{
    static LightingState s_inst;
    return &s_inst;
}

//-------------------------------------------------------------------------------------------------
DirLight* LightingState::ProminentDirLight()
{
    DirLight* light = DefaultDirLight();
    if(m_dirLights.size())
    {
        light = *(m_dirLights.begin());
    }
    return light;
}

//-------------------------------------------------------------------------------------------------
DirLight* LightingState::CreateDirLight()
{
    DirLight * light = new DirLight();
    m_dirLights.insert(light);
    return light;
}

//-------------------------------------------------------------------------------------------------
BoxLight* LightingState::CreateBoxLight()
{
    BoxLight * light = new BoxLight();
    m_boxLights.insert(light);
    return light;
}

//-------------------------------------------------------------------------------------------------
PointLight* LightingState::CreatePointLight()
{
    PointLight * light = new PointLight();
    m_pointLights.insert(light);
    return light;
}

//-------------------------------------------------------------------------------------------------
void LightingState::DestroyDirLight(DirLight* light)
{
    m_dirLights.erase(light);
    delete light;
}

//-------------------------------------------------------------------------------------------------
void LightingState::DestroyBoxLight(BoxLight* light)
{
    m_boxLights.erase(light);
    delete light;
}

//-------------------------------------------------------------------------------------------------
void LightingState::DestroyPointLight(PointLight* light)
{
    m_pointLights.erase(light);
    delete light;
}

void LightingState::UpdateLightEnvironment( RenderableNode& r )
{    
    UpdateLightEnvironment(r.lighting,r.bounds);
}

//-------------------------------------------------------------------------------------------------
void LightingState::UpdateLightEnvironment(LightEnvironment& env, const AABB& bounds)
{
    
    env.numDirLights = 0;
    env.numBoxLights = 0;
    env.numPointLights = 0;

    // gather dir lights
    if(m_dirLights.size() > 0)
    {
        for(auto it = m_dirLights.begin(); it != m_dirLights.end(); ++it)
        {                        
            env.dir[env.numDirLights++] = *(*it);
            if(env.numDirLights >= MAX_DIR_LIGHTS)
            {
                break;
            }
        }
    }
    else
    {   // default lighting
        env.dir[env.numDirLights++] = m_defaultDirLight;
    }

    // gather box lights
    for(auto it = m_boxLights.begin(); it != m_boxLights.end(); ++it)
    {
        BoxLight  light = *(*it);
        AABB lightBounds(light.min, light.max);

        if(TestAABBAABB(bounds, lightBounds))
        {
            env.box[env.numBoxLights++] = light;
            if(env.numBoxLights >= MAX_BOX_LIGHTS )
            {
                break;
            }
        }
    }

    // gather point lights
    for(auto it = m_pointLights.begin(); it != m_pointLights.end(); ++it)
    {
        PointLight  light = *(*it);

        // build an AABB for the sphere and test.
        float3 pos = float3(light.position.x, light.position.y, light.position.z);
        float radius = light.position.w;
        float3 ll(pos.x - radius, pos.y - radius, pos.z - radius);
        float3 ur(pos.x + radius, pos.y + radius, pos.z + radius);
        AABB sphereBounds(ll, ur);
        if(TestAABBAABB(bounds, sphereBounds))
        {
            env.point[env.numPointLights++] = light;
            if(env.numPointLights >= MAX_POINT_LIGHTS)
            {
                break;
            }
        }
    }

    //
    //  Set any unused lights slots to the empty light structs. The HLSL shader
    //  expects the entire array to be properly initialized.
    //
    for(unsigned int i = env.numDirLights; i < MAX_DIR_LIGHTS; ++i)
    {
        env.dir[i] = m_noDirLight;
    }
    for(unsigned int i = env.numBoxLights; i < MAX_BOX_LIGHTS; ++i)
    {
        env.box[i] = m_noBoxLight;
    }
    for(unsigned int i = env.numPointLights; i < MAX_POINT_LIGHTS; ++i)
    {
        env.point[i] = m_noPointLight;
    }

}


} // namespace
