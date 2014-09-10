//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    ShadowMapGen.h

****************************************************************************/
#pragma once

#include "../Core/NonCopyable.h"
#include "ShadowMaps.h"
#include "RenderSurface.h"

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
        
        void                        DrawRenderable(const RenderableNode& r);
                
        RenderContext*              m_rc;
        RenderSurface*              m_pSurface;        
        ID3D11VertexShader*         m_shaderShadowMapsVS;                
        ID3D11Buffer*               m_pConstantBufferPerFrame;
        ID3D11Buffer*               m_pConstantBufferPerDraw;
        ID3D11InputLayout*          m_pVertexLayoutMesh;  
        ::ID3D11RasterizerState*    m_rasterStateShadow;

        
        struct ConstantBufferShadowMapGenPerFrame
        {
            Matrix  view;
            Matrix  proj;
        };

        struct ConstantBufferShadowMapGenPerDraw
        {
            Matrix  world;    
        };

    };

} // namespace LvEdEngine
