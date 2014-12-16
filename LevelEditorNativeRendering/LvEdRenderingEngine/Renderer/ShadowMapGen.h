//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    ShadowMapGen.h

****************************************************************************/
#pragma once

#include "../Core/NonCopyable.h"
#include "ShadowMaps.h"
#include "RenderSurface.h"
#include "RenderBuffer.h"

namespace LvEdEngine
{
    class ShadowMapGen : public NonCopyable
    {
    public:
        ShadowMapGen( ID3D11Device* device);
        virtual ~ShadowMapGen();

        //  Called begin before drawing.
        //  Set up sampler states, shaders, connect constant buffers, etc.
        void Begin(RenderContext* rc, RenderSurface* pSurface, const AABB& bounds );

        //  Do the drawing.
        //  Connect resources, vertex and index buffers, and draw the world.
        void DrawNodes(const RenderNodeList& renderNodes);

        //  Called after drawing.
        //  Perform any needed post-drawing cleanup.
        virtual void End();

    private:
        
        struct CbPerFrame
        {
            Matrix  view;
            Matrix  proj;
        };

        TConstantBuffer<CbPerFrame> m_cbPerFrame;
        TConstantBuffer<Matrix>     m_cbPerDraw;
                
        RenderContext*              m_rc;
        RenderSurface*              m_pSurface;

        ID3D11VertexShader*         m_vertexShader;                        
        ID3D11InputLayout*          m_layoutP;
        ID3D11RasterizerState*      m_rasterState;

        void DrawRenderable(const RenderableNode& r);

    };

} // namespace LvEdEngine
