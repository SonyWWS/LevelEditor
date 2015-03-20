//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "LightGob.h"

namespace LvEdEngine
{
    class DirLight;

    class DirLightGob : public LightGob
    {
    public:
        DirLightGob();
        virtual ~DirLightGob();
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "DirLightGob";}

        void SetAmbient(int color);
        void SetDiffuse(int color);
        void SetSpecular(int color);
        void SetDirection(const float3& v);

    protected:
        DirLight * m_light;
    private:
        typedef LightGob super;
    };
}
