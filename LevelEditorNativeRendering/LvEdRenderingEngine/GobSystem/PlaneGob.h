//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "PrimitiveShapeGob.h"

namespace LvEdEngine
{
    class PlaneGob : public PrimitiveShapeGob
    {
    public:
        PlaneGob() : PrimitiveShapeGob( RenderShape::Quad) {}
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "PlaneGob";}
    private:
        typedef PrimitiveShapeGob super;
    };
}
