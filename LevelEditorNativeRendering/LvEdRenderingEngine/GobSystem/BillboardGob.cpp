//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "BillboardGob.h"


#include "../Renderer/ShapeLib.h"
#include "../Renderer/RenderBuffer.h"
#include "../Renderer/Model.h"
#include "../VectorMath/Camera.h"


namespace LvEdEngine
{

// ---------------------------------------------------------------------------------------------
// push Renderable nodes
//virtual 
void BillboardGob::GetRenderables(RenderableNodeCollector* collector, RenderContext* context)
{
		if (!IsVisible(context->Cam().GetFrustum()))
			return;

		super::GetRenderables(collector, context);
    RenderableNode renderable;
    SetupRenderable(&renderable, context);

    //  // test     
    //Mesh* quad = ShapeLibGetMesh(RenderShape::Quad);
    //m_localBounds = quad->bounds;
    //m_bounds = m_localBounds;
    //m_bounds.Transform(m_world);
    //renderable.bounds = m_bounds;  
    //renderable.mesh = quad;
    //renderable.WorldXform = m_world;
    //// end of test


    RenderFlagsEnum flags = RenderFlags::Textured;
    collector->Add( renderable, flags, Shaders::BillboardShader );
}

// ---------------------------------------------------------------------------------------------
void BillboardGob::SetupRenderable(RenderableNode* r, RenderContext* context)
{
    Matrix billboard = m_world;
    {        
        float sx = length( float3(&m_world.M11) );
        float sy = length( float3(&m_world.M21) );
        float sz = length( float3(&m_world.M31) );
        Matrix scaleM = Matrix::CreateScale(sx,sy,sz);

        float3 objectPos = &m_world.M41;
        Camera& cam = context->Cam();
        Matrix b = Matrix::CreateBillboard(objectPos,cam.CamPos(),cam.CamUp(),cam.CamLook());        
        billboard = scaleM * b;
    }
   
    PrimitiveShapeGob::SetupRenderable(r, context);    
    r->WorldXform = billboard;
    r->diffuse = float4(m_intensity, m_intensity, m_intensity, m_intensity);

    // special case compute AABB
    m_localBounds = AABB(float3(-0.5f,-0.5f,-0.5f),float3(0.5f,0.5f,0.5f));
    m_bounds = m_localBounds;
    m_bounds.Transform(billboard);
    r->bounds = m_bounds;    
}

}; // namespace
