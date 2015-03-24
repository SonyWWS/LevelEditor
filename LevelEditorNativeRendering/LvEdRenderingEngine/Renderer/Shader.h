//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    Shader

    Renderer interface for Level Editor "advanced renderers".
****************************************************************************/

#pragma once

#include "RenderEnums.h"
#include "RenderState.h"
#include "Renderable.h"
#include "RenderSurface.h"
#include "../Core/NonCopyable.h"
#include "../FrameTime.h"

namespace Shaders
{
    enum Shaders
    {
        SkyDomeShader,
        TexturedShader,
        TerrainShader,
        BillboardShader,        
        WireFrameShader,        
        BasicShader,    
        NormalsShader,
        //-----------------
        COUNT,
        NONE
    };
}
typedef Shaders::Shaders ShadersEnum;

struct ID3D11Device;

namespace LvEdEngine
{    
    class RenderContext;

    //-----------------------------------------------------------------------
    //  Shader
    //-----------------------------------------------------------------------
    class Shader : public NonCopyable
    {
    public:
        Shader( ShadersEnum shaderEnum): m_shaderEnum( shaderEnum )
        {}

        virtual ~Shader() {}

        ShadersEnum ShaderType() { return m_shaderEnum; }
            
		// Update any time based effects.		
		virtual void Update(const FrameTime& fr, UpdateTypeEnum updateType) {}
			
	    //  Called begin before drawing.
        //  Set up sampler states, shaders, connect constant buffers, etc.
        virtual void Begin(RenderContext* rc ) = 0;

        //  Accept render options
        virtual void SetRenderFlag(RenderFlagsEnum rf) = 0;

        //  Do the drawing.
        //  Connect resources, vertex and index buffers, and draw the world.
        virtual void DrawNodes(const RenderNodeList& renderNodes) = 0;

        //  Called after drawing.
        //  Perform any needed post-drawing cleanup.
        virtual void End() = 0;

    private:
        ShadersEnum         m_shaderEnum;        
    };

};
