//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once


// used for toggling global render states.
namespace GlobalRenderFlags
{
    enum GlobalRenderFlags 
    {
        None           = 0,        
        Textured       = 1 << 0, // turn on/off textured rendering
        Lit            = 1 << 1, // turn on/off all lights
        RenderBackFace = 1 << 2, // turn on/off backface rendering.
        Solid          = 1 << 3, // turn on/off solid rendering
        WireFrame      = 1 << 4, // turn on/off wireframe rendering
        Shadows        = 1 << 5, // turn on/off shadow rendering
        RenderNormals  = 1 << 6, // turn on/off normals rendering.
        Force32bit          = 0xffffffffUL,
    };    
}
typedef enum GlobalRenderFlags::GlobalRenderFlags GlobalRenderFlagsEnum;

// per object render flags
namespace RenderFlags
{
    enum RenderFlags 
    {
        None                = 0,        
        Textured            = 1 << 0,
        Lit                 = 1 << 1,
        RenderBackFace      = 1 << 2,
        DisableDepthTest    = 1 << 3,
        DisableDepthWrite   = 1 << 4,        
        AlphaBlend          = 1 << 5,         
        Force32bit          = 0x7FFFFFFF 
    };
}
typedef enum RenderFlags::RenderFlags RenderFlagsEnum;

// Flags used by Basic Renderer
namespace BasicRendererFlags
{
    enum BasicRendererFlags 
    {
        None                = 0,
        Lit                 = 1 << 0,
        WireFrame           = 1 << 1,
        DisableDepthTest    = 1 << 2,
        DisableDepthWrite   = 1 << 3,
        Foreground          = 1 << 4,   // foreground layer.
        Force32bit          = 0x7fffffff 
    };   
}
typedef enum BasicRendererFlags::BasicRendererFlags BasicRendererFlagsEnum;

namespace PrimitiveType
{
    // Defines how data in a vertex stream is interpreted during a draw call. Reference    
    enum PrimitiveType
    {
        Undefined = 0,

        // Renders the verticies as a series of isolated points.
        PointList = 1,

        // Renders the vertices as a list of isolated straight line segments; the count
        // may be any positive integer.
        LineList = 2, // = PrimitiveTopology.LineList,
        
        // Renders the vertices as a single polyline; the count may be any positive
        // integer.
        LineStrip = 3, // = PrimitiveTopology.LineStrip,
        
        // Renders the specified vertices as a sequence of isolated triangles. Each
        // group of three vertices defines a separate triangle. Back-face culling is
        // affected by the current winding-order render state.
        TriangleList = 4, // = PrimitiveTopology.TriangleList,
        
        // Renders the vertices as a triangle strip. The back-face culling flag is flipped
        // automatically on even-numbered triangles.
        TriangleStrip = 5, // = PrimitiveTopology.TriangleStrip,      
    };
}

typedef PrimitiveType::PrimitiveType PrimitiveTypeEnum;

namespace VertexFormat
{
    enum VertexFormat
    {
        VF_P,       // position
        VF_PC,      // position + color (rgba float4)
        VF_PN,      // position + normal
		VF_PT,      // position + texcoord
        VF_PTC,     // position + texcoord + color
        VF_PNT,     // position + normal + texcoord
        VF_PNTT,    // position + normal + tangent + texcoord
        VF_T,       // 2d position or 2d tex.
        VF_MAX,     // always last
    };
}
typedef VertexFormat::VertexFormat VertexFormatEnum;

namespace TextureType
{
    enum TextureType
    {
        // if you modify this enum
        // make sure MIN and MAX are updated.
        DIFFUSE,
        NORMAL,
        LIGHT,
        SPEC,   
        BlankMask,
        FullMask,
        Cubemap,
        Unknown,
        MAX,     // always last
        MIN = DIFFUSE,
    };
}
typedef TextureType::TextureType TextureTypeEnum;



namespace FillMode
{
    enum FillMode
    {	
        Wireframe = 2,
        Solid = 3
    };
}
typedef FillMode::FillMode FillModeEnum;


namespace CullMode
{
    enum CullMode
    {	
        NONE	= 1,
        FRONT	= 2,
        BACK	= 3,        
    };
}
typedef CullMode::CullMode CullModeEnum;



namespace ResourceType
{
    enum ResourceType
    {
        Unknown,
        Model,
        Texture,
        Material
    };

    inline const wchar_t* ToWString(ResourceType restype)
    {
        switch(restype)
        {
        case Unknown : return L"Unknown";
        case Model : return L"Model";
        case Texture : return L"Texture";
        case Material : return L"Material";
        default: return L"";
        }
    }

}
typedef enum ResourceType::ResourceType ResourceTypeEnum;

