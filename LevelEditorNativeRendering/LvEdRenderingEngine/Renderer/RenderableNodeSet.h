//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include "RenderableNodeCollector.h"

namespace LvEdEngine
{
    class RenderableNodeSet: public RenderableNodeCollector
    {
    public:
        RenderableNodeSet();
        virtual ~RenderableNodeSet();

        virtual void            Add( RenderableNode& r, RenderFlagsEnum rf, ShadersEnum shaderIdPref);
        virtual void            Add( const RenderNodeList::iterator& listBegin, const RenderNodeList::iterator& listEnd, RenderFlagsEnum rf, ShadersEnum shaderIdPref);

        RenderNodeList&         GetList( void ) { return m_renderNodes; }
        virtual void            ClearLists() { m_renderNodes.clear(); ClearBounds(); }        
        void SetSkipSelected(bool skipSelected) {m_skipSelected = skipSelected;}

    private:
        bool m_skipSelected;
        RenderNodeList  m_renderNodes;
    };
}
