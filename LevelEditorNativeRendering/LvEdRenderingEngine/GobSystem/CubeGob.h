//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "PrimitiveShapeGob.h"

namespace LvEdEngine
{
    class CubeGob : public PrimitiveShapeGob
    {
    public:
        CubeGob() : PrimitiveShapeGob( RenderShape::Cube ) {}    
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "CubeGob";}
    private:
        typedef PrimitiveShapeGob super;
    };
}
