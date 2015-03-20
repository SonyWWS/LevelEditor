//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "ControlPointGob.h"
#include "CurveGob.h"


#include "../Renderer/ShapeLib.h"
#include "../Renderer/RenderBuffer.h"
#include "../Renderer/Model.h"

namespace LvEdEngine
{

// push Renderable nodes
//virtual
void ControlPointGob::GetRenderables(RenderableNodeCollector* collector, RenderContext* context)
{  
	super::GetRenderables(collector, context);

    Mesh* mesh = ShapeLibGetMesh(RenderShape::QuadLineStrip);
    m_localBounds = mesh->bounds;

    const float pointSize = 8; // control point size in pixels
    float upp = context->Cam().ComputeUnitPerPixel(float3(&m_world.M41),
        context->ViewPort().y);
    float scale = pointSize * upp;        
    Matrix scaleM = Matrix::CreateScale(scale);
    float3 objectPos = float3(m_world.M41,m_world.M42,m_world.M43);
    Matrix b = Matrix::CreateBillboard(objectPos,context->Cam().CamPos(),context->Cam().CamUp(),context->Cam().CamLook());
    Matrix billboard = scaleM * b;

    // calculate bounds for screen facing quad
    float3 transformed, min, max;
    transformed = mesh->pos[0];
    transformed.Transform(billboard);
    min = max = transformed;
    for (auto it = mesh->pos.begin(); it != mesh->pos.end(); ++it)
    {
        transformed = (*it);
        transformed.Transform(billboard);
        min = minimize(min, transformed);
        max = maximize(max, transformed);
    }
    m_bounds = AABB(min,max);

    // give it same color as curve
    int color = 0xFFFF0000;
    CurveGob* curve = (CurveGob*)m_parent;
    if(curve != NULL)
    {
        color = curve->GetColor();
    }

    // set renderable
    RenderableNode r;
    r.mesh = mesh;
    ConvertColor(color, &r.diffuse);
    r.objectId = GetInstanceId();
    r.bounds = m_bounds;
    r.WorldXform = billboard;
    r.SetFlag(RenderableNode::kTestAgainstBBoxOnly, true);
    r.SetFlag(RenderableNode::kShadowCaster, false);
    r.SetFlag(RenderableNode::kShadowReceiver, false);    
    collector->Add(r, RenderFlags::None, Shaders::BasicShader);    
}


}; // namespace
