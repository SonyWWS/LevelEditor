//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "PrimitiveShapeGob.h"
#include "../VectorMath/V3dMath.h"

namespace LvEdEngine
{
    class BillboardGob : public PrimitiveShapeGob // note: in the schema, billboard deriveds from game object...but we want to re-use some code from ShapeGob.
    {
    public:
        BillboardGob() : PrimitiveShapeGob( RenderShape::Quad ) {m_intensity=1.0f;}    
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "BillboardGob";}

        // push Renderable nodes
		virtual void GetRenderables(RenderableNodeCollector* collector, RenderContext* context);
        virtual void SetupRenderable(RenderableNode* r, RenderContext* context);
        void SetIntensity(float intensity)
        {
            m_intensity = clamp(intensity, 0.0f, 1.0f);
        };
    protected:        
        float m_intensity;
    private:
        typedef PrimitiveShapeGob super;
    };
}
