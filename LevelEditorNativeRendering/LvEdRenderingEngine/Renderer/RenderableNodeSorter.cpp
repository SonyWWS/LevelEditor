//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    RenderableNodeSorter.cpp

****************************************************************************/

#include "RenderableNodeSorter.h"
#include "ShaderLib.h"
#include <algorithm>
#include "Model.h"
#include "RenderContext.h"


using namespace LvEdEngine;

#define GetBucketKey( _RenderFlagsEnum_, _ShadersEnum_ )   (( _RenderFlagsEnum_ << 10 ) | _ShadersEnum_ )


//---------------------------------------------------------------------------
RenderableNodeSorter::RenderableNodeSorter()
{
}

//---------------------------------------------------------------------------
RenderableNodeSorter::~RenderableNodeSorter()
{
}

//---------------------------------------------------------------------------
void RenderableNodeSorter::ClearLists()
{
    for ( auto it=m_buckets.begin(); it != m_buckets.end(); ++it )
    {
        it->second.renderables.clear();
    }
    ClearBounds();
}

//---------------------------------------------------------------------------
unsigned int RenderableNodeSorter::GetBucketCount()
{
    return (unsigned int)m_bucketKeys.size();
}

//---------------------------------------------------------------------------
RenderableNodeSorter::Bucket* RenderableNodeSorter::GetBucket(uint32_t index)
{
    assert(index < m_bucketKeys.size());
    unsigned int key = m_bucketKeys[index];
    return &m_buckets[key];
}

//---------------------------------------------------------------------------
static bool BucketKeySorting(const uint32_t n1, const uint32_t n2)
{
    return n1 < n2;
}

//---------------------------------------------------------------------------
RenderableNodeSorter::Bucket& RenderableNodeSorter::GetOrMakeBucket( uint32_t rf, ShadersEnum shaderId )
{
    uint32_t bucketKey = GetBucketKey(rf, shaderId );

    BucketMap::iterator it = m_buckets.find( bucketKey );
    if ( it == m_buckets.end() )
    {
        std::pair<uint32_t,Bucket> newVal;
        newVal.first = bucketKey;
        newVal.second.renderFlags  = (RenderFlagsEnum)rf;
        newVal.second.shaderId     = shaderId;

        std::pair<BucketMap::iterator, bool> insRslt = m_buckets.insert( newVal );
        it = insRslt.first;
        m_bucketKeys.push_back(bucketKey);
        std::sort(m_bucketKeys.begin(), m_bucketKeys.end(), BucketKeySorting);
    }
    return it->second;
}

//---------------------------------------------------------------------------
void RenderableNodeSorter::Add(RenderableNode& r, RenderFlagsEnum rf, ShadersEnum shaderId)
{
    
    assert(shaderId != Shaders::NONE);
    RenderContext* context = RenderContext::Inst();
    bool selected = ( context->selection.find( r.objectId ) != context->selection.end() );    
         
    PrimitiveTypeEnum primtype = r.mesh->primitiveType;
    GlobalRenderFlagsEnum gflags = this->GetFlags();
    bool triPrim = primtype == PrimitiveType::TriangleList || primtype == PrimitiveType::TriangleStrip;  
    
    uint32_t mask =(uint32_t) ~(RenderFlags::Textured | RenderFlags::Lit);        
    uint32_t flags = rf & (mask | gflags);
    flags |=  (gflags & GlobalRenderFlags::RenderBackFace);
    
    bool wireflagset = (gflags & GlobalRenderFlags::WireFrame) != 0;    
    if(triPrim)
    {
         if(gflags & GlobalRenderFlags::Solid) 
         {
             Bucket& bucket = GetOrMakeBucket(flags, shaderId);
             bucket.renderables.push_back( r );
             if(r.GetFlag(RenderableNode::kShadowCaster))
                 m_bounds.Extend(r.bounds);    
         }

         if(selected || wireflagset)
         {
             RenderableNode node = r;
             node.diffuse = selected ? context->State()->GetSelectionColor() : context->State()->GetWireframeColor();
             flags &= ~RenderFlags::AlphaBlend;             
             Bucket& bucket = GetOrMakeBucket(flags, Shaders::WireFrameShader );
             bucket.renderables.push_back( node );
         }         
    }
    else
    {
        flags &= ~RenderFlags::AlphaBlend;
        Bucket& bucket = GetOrMakeBucket(flags, shaderId);

        if(selected)
        {
            RenderableNode node = r;
            node.diffuse = context->State()->GetSelectionColor();
            bucket.renderables.push_back( node);
        }
        else
        {
            bucket.renderables.push_back( r );
        }               
    }
}

//---------------------------------------------------------------------------
void RenderableNodeSorter::Add( const RenderNodeList::iterator& listBegin, const RenderNodeList::iterator& listEnd,
                                    RenderFlagsEnum rf, ShadersEnum shaderId )
{
    if(listBegin == listEnd) return;
    // use the first renderable to detect if parent gob is selected
    // and also if the gob is shadow caster.
    assert(shaderId != Shaders::NONE);
    ObjectGUID gobId = listBegin->objectId;
    RenderContext* context = RenderContext::Inst();
    bool selected = ( context->selection.find( gobId ) != context->selection.end() );
    //bool isShadowCaster = listBegin->GetFlag( RenderableNode::kShadowCaster );
    GlobalRenderFlagsEnum gflags = this->GetFlags();

    uint32_t mask =(uint32_t) ~(RenderFlags::Textured | RenderFlags::Lit | RenderFlags::RenderBackFace);    
    uint32_t flags = rf & (mask | gflags);
    flags |=  (gflags & GlobalRenderFlags::RenderBackFace);

    PrimitiveTypeEnum primtype = listBegin->mesh->primitiveType;
    bool triPrim = primtype == PrimitiveType::TriangleList || primtype == PrimitiveType::TriangleStrip;  
   
    bool wireflagset = (gflags & GlobalRenderFlags::WireFrame) != 0;
    if(triPrim)
    {
         if(gflags & GlobalRenderFlags::Solid)
         {
             Bucket& bucket = GetOrMakeBucket(flags, shaderId);
             bucket.renderables.insert( bucket.renderables.end(), listBegin, listEnd );
             for ( auto it = listBegin; it != listEnd; ++it )      
             {
                 if(it->GetFlag(RenderableNode::kShadowCaster))
                     m_bounds.Extend(it->bounds);  
             }
         }

         if(selected || wireflagset)
         {
             float4 color = selected ? context->State()->GetSelectionColor() : context->State()->GetWireframeColor();
             flags &= ~RenderFlags::AlphaBlend;             
             Bucket& bucket = GetOrMakeBucket(flags, Shaders::WireFrameShader );

             for ( auto it = listBegin; it != listEnd; ++it )      
             {
                 RenderableNode node = (*it);
                 node.diffuse = color;
                 bucket.renderables.push_back(node);
             }
         }
    }
    else
    {
        flags &= ~RenderFlags::AlphaBlend;
        Bucket& bucket = GetOrMakeBucket(flags, shaderId );

        if(wireflagset || selected)
        {
            float4 color = context->State()->GetSelectionColor();
             for ( auto it = listBegin; it != listEnd; ++it )      
             {
                 RenderableNode node = *it;
                 node.diffuse = color;
                 bucket.renderables.push_back(node);
             }
        }
        else
        {
            bucket.renderables.insert( bucket.renderables.end(), listBegin, listEnd );      
        }        
    }
}

//---------------------------------------------------------------------------
void RenderableNodeSorter::Debug_GetStats( uint32_t& numBuckets, uint32_t& numItems )
{
    numBuckets = 0;
    numItems = 0;
    for ( auto it = m_buckets.begin(); it != m_buckets.end(); ++it )
    {
        uint32_t bucketItems = (uint32_t)it->second.renderables.size();
        numItems += bucketItems;
        if ( bucketItems > 0 )
        {
            numBuckets++;
        }
    }
}
