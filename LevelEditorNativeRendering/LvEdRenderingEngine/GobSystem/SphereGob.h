//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "PrimitiveShapeGob.h"

namespace LvEdEngine
{
    class SphereGob : public PrimitiveShapeGob
    {
    public: 
        SphereGob() : PrimitiveShapeGob( RenderShape::Sphere ) {}     
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "SphereGob";}
    private:
        typedef PrimitiveShapeGob super;        
     };
}; // namespace
