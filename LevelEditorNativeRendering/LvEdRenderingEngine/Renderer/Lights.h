//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "../VectorMath/V3dMath.h"
#include "../VectorMath/CollisionPrimitives.h"
#include "../Core/NonCopyable.h"
#include <set>

namespace LvEdEngine
{
    static const unsigned int MAX_DIR_LIGHTS = 2;
    static const unsigned int MAX_BOX_LIGHTS = 2;
    static const unsigned int MAX_POINT_LIGHTS = 4;

    class RenderableNode;

    struct ExpFog
    {
        int    enabled;
        float  density;
        float  range;  
        float  fpad;  
        float4 Color;     
    };

    class Light
    {
    public:
        float3 ambient;
        float pad1;
        float3 diffuse;
        float pad2;
        float3 specular;
        float pad3;
    };

    // same struct defined in lighting.shh shader.
    class DirLight : public Light
    {
    public:
        float3 dir;
        float pad4;
    };

    // same struct defined in lighting.shh shader.
    class BoxLight : public DirLight
    {
    public:
        float3 min;
        float pad5;
        float3 max;
        float pad6;
        float4 attenuation;
    };

    // same struct defined in lighting.shh shader.
    class PointLight: public Light
    {
    public:
        float4 attenuation;
        float4 position; //(xyz, radius)
    };


    // this defines the entire lighting available for per object rendering.
    // See the related struct, LightEnvironment defined in Lighting.shh shader.
    struct LightEnvironment
    {
        DirLight   dir[ MAX_DIR_LIGHTS ];
        BoxLight   box[ MAX_BOX_LIGHTS ];
        PointLight point[ MAX_POINT_LIGHTS ];
        uint32_t numDirLights;
        uint32_t numBoxLights;
        uint32_t numPointLights;
        uint32_t pad1;
    };

    class LightingState : public NonCopyable
    {
    public:
        static LightingState* Inst();

        DirLight*   CreateDirLight();
        void        DestroyDirLight(DirLight* light);
        DirLight*   ProminentDirLight();
        DirLight*   DefaultDirLight()   { return &m_defaultDirLight; }

        BoxLight*   CreateBoxLight();
        void        DestroyBoxLight(BoxLight* light);

        PointLight* CreatePointLight();
        void        DestroyPointLight(PointLight* light);

        void        UpdateLightEnvironment( RenderableNode& r );
        void        UpdateLightEnvironment(LightEnvironment& env, const AABB& bounds);

    private:
        LightingState();

        DirLight                m_defaultDirLight;
        std::set<DirLight*>     m_dirLights;
        std::set<BoxLight*>     m_boxLights;
        std::set<PointLight*>   m_pointLights;

        DirLight                m_noDirLight;
        BoxLight                m_noBoxLight;
        PointLight              m_noPointLight;
    };
}
