//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    RenderableNodeSet.cpp

****************************************************************************/
#include "RenderableNodeSet.h"
#include "../Renderer/RenderContext.h"

using namespace LvEdEngine;

//---------------------------------------------------------------------------
RenderableNodeSet::RenderableNodeSet()
{
    m_skipSelected = false;
}

//---------------------------------------------------------------------------
RenderableNodeSet::~RenderableNodeSet()
{
}

void RenderableNodeSet::Add( RenderableNode& r, RenderFlagsEnum /*rf*/, ShadersEnum /*shaderIdPref*/) 
{ 
    if(r.GetFlag(RenderableNode::kNotPickable))
        return;
    if(m_skipSelected)
    {
        RenderContext* context = RenderContext::Inst();
        if( context->selection.find( r.objectId ) != context->selection.end() )
            return;        
    }

    m_renderNodes.push_back( r ); 
}

//---------------------------------------------------------------------------
void RenderableNodeSet::Add( const RenderNodeList::iterator& listBegin, const RenderNodeList::iterator& listEnd, RenderFlagsEnum /*rf*/, ShadersEnum /*shaderIdPref*/ )
{
    if(m_skipSelected && listBegin != listEnd)
    {
        ObjectGUID gobId = listBegin->objectId;
        RenderContext* context = RenderContext::Inst();
        if( context->selection.find( gobId ) != context->selection.end() )
            return;

    }
    for ( RenderNodeList::iterator i=listBegin; i != listEnd; i++ )
    {
        if((*i).GetFlag(RenderableNode::kNotPickable))
            continue;        
        m_bounds.Extend( (*i).bounds );
        m_renderNodes.push_back( *i );
    }
}
