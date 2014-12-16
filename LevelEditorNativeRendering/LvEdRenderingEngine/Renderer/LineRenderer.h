//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "../Core/WinHeaders.h"
#include "../Core/typedefs.h"
#include "../Core/NonCopyable.h"
#include "../VectorMath/V3dMath.h"
#include "../VectorMath/CollisionPrimitives.h"
#include "RenderBuffer.h"
#include <vector>

namespace LvEdEngine
{

class RenderContext;
class LineRenderer : public NonCopyable
{
public:

    // set color for draw functions that don't take color arg.
    void SetColor(const float4& color) {m_color = color;}
    
    void DrawLine(const float3& v1, const float3& v2)
    {
        m_vertsPC.push_back(VertexPC(v1,m_color));
        m_vertsPC.push_back(VertexPC(v2,m_color));
    }
    void DrawLine(const float3& v1, const float3& v2, const float4& color)
    {
        m_vertsPC.push_back(VertexPC(v1,color));
        m_vertsPC.push_back(VertexPC(v2,color));
    }
    void DrawLine(const float3& v1, const float3& v2, const float4& color1, const float4& color2)
    {
        m_vertsPC.push_back(VertexPC(v1,color1));
        m_vertsPC.push_back(VertexPC(v2,color2));
    }

    void DrawAABB(const AABB& aabb, const float4& color);

    void DrawFrustum(const Frustum& frustum, const float4& color);

    // render all the requested draw calls,
    // ideally this function should be called once per-frame.
    void RenderAll(RenderContext* rc);

    static void            InitInstance(ID3D11Device* device);
    static void            DestroyInstance();
    static LineRenderer*   Inst() { return s_inst; }

private:
    LineRenderer(ID3D11Device* device);
    ~LineRenderer();
    static LineRenderer*   s_inst;

    float4 m_color;
    std::vector<VertexPC> m_vertsPC;
    VertexBuffer* m_vbPC; // vertex buffer.
    
    ID3D11InputLayout*     m_vertexLayoutPC;
    ID3D11VertexShader*    m_vsShader;
    ID3D11PixelShader*     m_psShader;

    TConstantBuffer<Matrix> m_perframeCB;
};



}
