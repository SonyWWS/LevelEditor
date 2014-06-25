//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include "Renderable.h"
#include "RenderableNodeCollector.h"
#include "Shader.h"
#include <hash_map>

namespace LvEdEngine
{
    class RenderableNodeSorter : public RenderableNodeCollector
    {
    public:
        RenderableNodeSorter();
        virtual ~RenderableNodeSorter();

        virtual void ClearLists();

        // Adds the given node to the appropriate bucket
        // using the combination of the global render flags and
        // the given render flags.
        // this method assumes the this class have access to global render flags.
        virtual void Add( const RenderNodeList::iterator& listBegin, const RenderNodeList::iterator& listEnd, RenderFlagsEnum rf, ShadersEnum shaderIdPref);
        virtual void Add(RenderableNode& r, RenderFlagsEnum rf, ShadersEnum shaderIdPref);

        virtual void Debug_GetStats( uint32_t& numBuckets, uint32_t& numItems );

        class Bucket
        {
        public:
            ShadersEnum     shaderId;
            RenderFlagsEnum renderFlags;
            RenderNodeList  renderables;

            Bucket() : renderFlags((RenderFlagsEnum)0) {}
        };

        uint32_t GetBucketCount();
        Bucket* GetBucket(uint32_t index);

    private:
        typedef std::hash_map<uint32_t,Bucket> BucketMap;
        typedef std::vector<uint32_t> BucketKeys;
        BucketMap       m_buckets;
        BucketKeys      m_bucketKeys;
        Bucket&         GetOrMakeBucket( uint32_t rf, ShadersEnum shaderId);
    };

}
