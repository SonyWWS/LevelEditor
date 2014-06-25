//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <vector>
#include <deque>
#include "../Core/WinHeaders.h"
#include "../Core/typedefs.h"
#include "../VectorMath/V3dMath.h"
#include "../VectorMath/CollisionPrimitives.h"
#include "../Core/StringBlob.h"
#include "RenderEnums.h"
#include "Lights.h"
#include "D3DCommon.h"

namespace LvEdEngine
{    
    class VertexBuffer;
    class IndexBuffer;
    class Texture;
    class Mesh;
              
    class RenderableNode
    {
    public:
        enum Flags
        {
            None                    = 0,
            kShadowCaster           = 1 << 0,
            kShadowReceiver         = 1 << 1,
            kTestAgainstBBoxOnly    = 1 << 2,   // hit test: should the hit test against the mesh?
            kNotPickable            = 1 << 3  // this node is not pickable
        };

        RenderableNode()
        {
            flags = 0;
            SetFlag( kShadowCaster, true );
            SetFlag( kShadowReceiver, true );            
            
            objectId = 0;
            diffuse.x = diffuse.y = diffuse.z = diffuse.w = 1.0f;
            specular.x = specular.y = specular.z = 0.0f;
            emissive = float4(0,0,0,0);
            specPower = 1;
            for(int t = TextureType::MIN; t < TextureType::MAX; t++)
                textures[t] = NULL;
            
            mesh = NULL;
            lighting.numDirLights = 0;
            lighting.numBoxLights = 0;
            lighting.numPointLights = 0;
        }

        // The mesh to draw.
        Mesh* mesh;
        
        // world transform matrix
        Matrix WorldXform;
        Matrix TextureXForm;
        AABB bounds;
        float4 emissive;
        float4 diffuse;
        float3 specular;
        float specPower;
        uint32_t flags;
        LightEnvironment lighting;

        // the handle of the game object that created this node.
        ObjectGUID objectId;

        float Distance;

        Texture*        textures[TextureType::MAX];

        void    SetFlag( Flags flagBit, bool bON )      { if ( bON ) { flags |= flagBit; } else { flags &= ~flagBit; } }
        bool    GetFlag( Flags flagBit ) const          { return (( flags & flagBit ) != 0 ); }
    };
    typedef std::vector<RenderableNode> RenderNodeList;
}

