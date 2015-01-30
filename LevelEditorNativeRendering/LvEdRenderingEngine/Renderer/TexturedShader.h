//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    TexturedShader.h

****************************************************************************/
#pragma once

#include "Shader.h"
#include "ShadowMaps.h"
#include "RenderSurface.h"
#include "Lights.h"
#include "RenderBuffer.h"

namespace LvEdEngine 
{
  
class RenderContext;


//---------------------------------------------------------------------------
//  TexturedShader
//---------------------------------------------------------------------------
class TexturedShader : public Shader
{
public:
    //-------------------------------------------------------------------
    // Shader interface
    //-------------------------------------------------------------------
    TexturedShader(ID3D11Device* device);
    virtual ~TexturedShader();

    //  Called begin before drawing.
    //  Set up sampler states, shaders, connect constant buffers, etc.
    virtual void Begin(RenderContext* rc );

    //  Accept render options
    virtual void SetRenderFlag(RenderFlagsEnum rf);

    //  Do the drawing.
    //  Connect resources, vertex and index buffers, and draw the world.
    virtual void DrawNodes(const RenderNodeList& renderNodes);

    //  Called after drawing.
    //  Perform any needed post-drawing cleanup.
    virtual void End();
private:
    
    void                        DrawRenderable(const RenderableNode& r);
    RenderContext*              m_rc;        

    ID3D11VertexShader*         m_shaderSceneRenderVS;
    ID3D11PixelShader*          m_shaderSceneRenderPS;
    ID3D11InputLayout*          m_pVertexLayoutMesh;
    
    struct PerFrameCb
    {    
        Matrix cb_view;
        Matrix cb_proj;
        ExpFog cb_fog;
        float3 cb_camPosW;
        float  cb_pad;
    };    
    struct PerDrawCb
    {
        Matrix           cb_world;
        Matrix           cb_worldInvTrans;
        Matrix           cb_textureTrans;
        float4           cb_matEmissive;
        float4           cb_matDiffuse;
        float4           cb_matSpecular;
        LightEnvironment cb_lighting;
        int              cb_hasDiffuseMap;
        int              cb_hasNormalMap;
        int              cb_hasSpecularMap;
        int              cb_pad;
    };    
    struct RenderStateCb
    {
        int   cb_textured;
        int   cb_lit;
        int   cb_shadowed;
        int   cb_pad;
    };

    TConstantBuffer<PerFrameCb>    m_perFrameCb;
    TConstantBuffer<PerDrawCb>     m_perDrawCb;
    TConstantBuffer<RenderStateCb> m_renderStateCb;
};

}; // namespace LvEdEngine
