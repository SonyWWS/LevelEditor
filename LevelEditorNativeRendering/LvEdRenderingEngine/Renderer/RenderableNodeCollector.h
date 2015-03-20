//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include "Renderable.h"
#include "Shader.h"
#include "../Core/NonCopyable.h"
#include "../VectorMath/CollisionPrimitives.h"

namespace LvEdEngine
{
    class RenderableNodeCollector : public NonCopyable
    {
    public:
        RenderableNodeCollector()
          : m_globalRenderFlags(GlobalRenderFlags::None)  {}
        virtual ~RenderableNodeCollector() {}

        virtual void    Add( RenderableNode& r, RenderFlagsEnum rf, ShadersEnum shaderIdPref) = 0;
        virtual void    Add( const RenderNodeList::iterator& listBegin, const RenderNodeList::iterator& listEnd, RenderFlagsEnum rf, ShadersEnum shaderIdPref) = 0;

        // Remove any renderables we have stored.
        virtual void ClearLists() = 0;

        GlobalRenderFlagsEnum GetFlags()                          { return m_globalRenderFlags; }
        void            SetFlags( GlobalRenderFlagsEnum flags )   { m_globalRenderFlags = flags; }

        AABB&           GetBounds()                         { return m_bounds; }
        void            ClearBounds()                       { m_bounds = AABB(); }

    protected:
        AABB            m_bounds;

    private:
        GlobalRenderFlagsEnum m_globalRenderFlags;
    };
}
