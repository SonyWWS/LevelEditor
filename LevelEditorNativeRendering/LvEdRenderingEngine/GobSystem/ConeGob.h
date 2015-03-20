//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "PrimitiveShapeGob.h"

namespace LvEdEngine
{
    class ConeGob : public PrimitiveShapeGob
    {
    public:
        ConeGob() : PrimitiveShapeGob( RenderShape::Cone ) {}
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "ConeGob";}

    private:
        typedef PrimitiveShapeGob super;
    };
}
