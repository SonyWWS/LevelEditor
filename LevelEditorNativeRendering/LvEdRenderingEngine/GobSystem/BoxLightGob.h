//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "LightGob.h"

namespace LvEdEngine
{
    class BoxLight;

    class BoxLightGob : public LightGob
    {
    public:
        BoxLightGob();
        virtual ~BoxLightGob();
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "BoxLightGob";}

        void SetAmbient(int color);
        void SetDiffuse(int color);
        void SetSpecular(int color);
        void SetDirection(const float3& v);
        float3 GetDirection();
        void SetAttenuation(const float3& atten);

        virtual void Update(float dt);

        virtual bool GetRenderables(RenderableNodeCollector* collector, RenderContext* context);
     
    protected:
        BoxLight * m_light;
    };
}
