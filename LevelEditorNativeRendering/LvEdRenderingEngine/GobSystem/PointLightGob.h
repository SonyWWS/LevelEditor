//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "LightGob.h"

namespace LvEdEngine
{
    class PointLight;
    class PointLightGob : public LightGob
    {
    public:
        PointLightGob();
        virtual ~PointLightGob();
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "PointLightGob";}

        void SetAmbient(int color);
        void SetDiffuse(int color);
        void SetSpecular(int color);
        void SetAttenuation(const float3& atten);
        void SetRange(float r);
        virtual void Update(const FrameTime& fr, UpdateTypeEnum updateType);
    protected:
        PointLight* m_light;
    private:        
        typedef LightGob super;
    };
}
