//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "GameObject.h"
#include "../Renderer/Resource.h"

namespace LvEdEngine
{
    class ControlPointGob : public GameObject
    {
    public:
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "ControlPointGob";}
		
        // push Renderable nodes
        virtual bool GetRenderables(RenderableNodeCollector* collector, RenderContext* context);
        virtual void Update(float dt);
    };
}
