//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <vector>
#include "../Core/WinHeaders.h"
#include "../Core/Utils.h"
#include "../VectorMath/V3dMath.h"
#include "../VectorMath/CollisionPrimitives.h"
#include "RenderEnums.h"
#include <d3d11.h>

namespace LvEdEngine
{
    class VertexBuffer;
    class IndexBuffer;
    class Mesh;

      
    // SHAPES
    namespace RenderShape
    {
        enum RenderShape
        {
            // if you modify this enum, make sure
            // MIN and MAX are updated.
            QuadLineStrip,
            Quad,
            AsteriskQuads,
            Cube,
            Sphere,
            Cylinder,
            Torus,
            Cone,

            MAX,
            MIN = QuadLineStrip
        };
    }
    typedef RenderShape::RenderShape RenderShapeEnum;

    // Startup the render library
    void ShapeLibStartup(ID3D11Device* device);
    
    // Shutdown the render library
    void ShapeLibShutdown();

    // Get a mesh for one of the shape enum's.
    Mesh* ShapeLibGetMesh(RenderShapeEnum shape);    
};