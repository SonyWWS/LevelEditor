//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "PrimitiveShapeGob.h"

namespace LvEdEngine
{
    class CylinderGob : public PrimitiveShapeGob
    {
    public:
        CylinderGob() : PrimitiveShapeGob( RenderShape::Cylinder ) {}
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "CylinderGob";}

    private:
        typedef PrimitiveShapeGob super;
    };
}
