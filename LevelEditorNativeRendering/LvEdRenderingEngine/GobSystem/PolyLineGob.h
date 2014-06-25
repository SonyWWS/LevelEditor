#pragma once
#include "GameObject.h"
#include "CurveGob.h"

namespace LvEdEngine
{
    // todo: might not want to inherite from CurveGob, but instead be its own class
    class PolyLineGob : public CurveGob
    {
    public:
        PolyLineGob()
        {
            m_steps = 0;
        }
        virtual const char* ClassName(){return StaticClassName();}
        static const char* StaticClassName(){return "PolyLineGob";}
    };
}
