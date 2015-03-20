//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "PrimitiveShapeGob.h"

namespace LvEdEngine
{
    class TorusGob : public PrimitiveShapeGob
    {
    public: 
        TorusGob() : PrimitiveShapeGob( RenderShape::Torus ) {}     
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "TorusGob";}
    private:
        typedef PrimitiveShapeGob super;
     };
}; // namespace
