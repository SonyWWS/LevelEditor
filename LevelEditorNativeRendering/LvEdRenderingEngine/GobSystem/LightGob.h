//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "GameObject.h"

namespace LvEdEngine
{
    class LightGob : public GameObject
    {
    public:
        LightGob();
        virtual ~LightGob();               
        // push Renderable nodes
		virtual void GetRenderables(RenderableNodeCollector* collector, RenderContext* context);

    protected:        
        Mesh* m_mesh;
    private:
        typedef GameObject super;
    };
}
