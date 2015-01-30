//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    TerrainShader.h

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
class TerrainGob;


#define MaxNumLayers 8


//---------------------------------------------------------------------------
//  TerrainShader
//---------------------------------------------------------------------------
class TerrainShader : public Shader
{
public:
    //-------------------------------------------------------------------
    // Shader interface
    //-------------------------------------------------------------------
    TerrainShader(ID3D11Device* device);
    virtual ~TerrainShader();

    //  Called begin before drawing.
    //  Set up sampler states, shaders, connect constant buffers, etc.
    virtual void Begin(RenderContext* rc );
       

    //  Accept render options
    virtual void SetRenderFlag(RenderFlagsEnum rf);

    void RenderTerrain(TerrainGob* terrain);

    //  Do the drawing.
    //  Connect resources, vertex and index buffers, and draw the world.
    virtual void DrawNodes(const RenderNodeList& renderNodes);

    //  Called after drawing.
    //  Perform any needed post-drawing cleanup.
    virtual void End();

private:

    void                        Initialize(ID3D11Device* device);    
    RenderContext*              m_rc;    
    
    ID3D11VertexShader*         m_vertexShader;
    ID3D11PixelShader*          m_pixelShader;
    ID3D11InputLayout*          m_vertexLayout;

    // wireframe rendering
    ID3D11VertexShader*    m_VSSolidWire;
    ID3D11GeometryShader*  m_GSSolidWire;
    ID3D11PixelShader*     m_PSSolidWire;    
    ID3D11InputLayout*     m_layoutSolidWire;
    ID3D11DepthStencilState* m_dpLessEqual; 

    // rendering normals
    ID3D11VertexShader*    m_VSNormals;
    ID3D11GeometryShader*  m_GSNormals;
    ID3D11PixelShader*     m_PSNormals;
    

    // decoration map rendering
    
    ID3D11VertexShader* m_VSDeco;    
    ID3D11PixelShader*  m_PSDeco;  
    ID3D11InputLayout*  m_vertLayoutDeco;

    ID3D11VertexShader*   m_VSDecoBB;    
    ID3D11GeometryShader* m_GSDecoBB;
    ID3D11InputLayout*    m_vertLayoutDecoBB;
    

     struct PerFrameCb
     {
         Matrix view;
         Matrix proj;
         float4 viewport;
         float3 camPosW;
         float  pad;
     };
     TConstantBuffer<PerFrameCb> m_perFrameCb;


     struct RenderStateCb
     {
         int textured;
         int lit;
         int shadowed;
         int pad;
     };
     TConstantBuffer<RenderStateCb> m_renderStateCb;


     struct PerTerrainCb
     {
         float4  layerTexScale[MaxNumLayers];
         float4  wirecolor;
         float3  terrainTrans;
         float   cellSize;
         float2  hmSize;                // size of hiehgt map in texels 
         float2  hmTexelsize;        // texel-size of the height texture.         
         ExpFog  fog;
         float   numLayers;	
         float   pad[3];
         
     }; 
     TConstantBuffer<PerTerrainCb> m_perTerrainCb;

     struct PerPatchCb
     {
         float3              patchTrans; // patch translation.		
         float               pad;
         LightEnvironment    lightEnv;    
     };
     TConstantBuffer<PerPatchCb> m_perPatchCb;    

     TConstantBuffer<float4> m_perDecomapCb;
      

    //__declspec(align(16))
};

}; // namespace LvEdEngine


