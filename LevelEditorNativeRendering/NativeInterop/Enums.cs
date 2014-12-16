//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace RenderingInterop
{

    

    /// <summary>
    /// Vertex format.</summary>
    public enum VertexFormat
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
    }


    // Defines how data in a vertex stream is interpreted during a draw call. Reference    
    public enum PrimitiveType
    {
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
    }


    [Flags]
    public enum BasicRendererFlags : uint
    {
        None              = 0,
        Lit               = 1 << 0,
        WireFrame         = 1 << 1,
        DisableDepthTest  = 1 << 2,
        DisableDepthWrite = 1 << 3,
        Foreground        = 1 << 4,   // foreground layer.
    };   

    [Flags]
    public enum GlobalRenderFlags : uint
    {
        None           = 0,
        Textured       = 1 << 0, // turn on/off textured rendering
        Lit            = 1 << 1, // turn on/off all lights
        RenderBackFace = 1 << 2, // turn on/off backface rendering.
        Solid          = 1 << 3, // turn on/off solid rendering
        WireFrame      = 1 << 4, // turn on/off wireframe rendering
        Shadows        = 1 << 5, // turn on/off shadow rendering
        RenderNormals  = 1 << 6, // turn on/off normals rendering.
    }


    //-------------------------------------------------------------------
    //  eFontStyle
    //-------------------------------------------------------------------
    [Flags]
    public enum FontStyle
    {
        NORMAL = (1 << 1),
        BOLD = (1 << 2),
        ITALIC = (1 << 3),
        UNDERLINE = (1 << 4),
        STRIKEOUT = (1 << 5),
    };
}
