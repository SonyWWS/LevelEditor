//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "LightGob.h"
#include "..\Renderer\RenderableNodeSet.h"
#include "../Renderer/TextureLib.h"
#include "../Renderer/Model.h"
#include "../Renderer/ShapeLib.h"

namespace LvEdEngine
{
//---------------------------------------------------------------------------
LightGob::LightGob()
{
    SetCastsShadows( false );       // doesn't block the light
    SetReceivesShadows( false );    // no shadow is cast on it        
    m_mesh = ShapeLibGetMesh( RenderShape::Quad); 
    m_localBounds = AABB(float3(-0.5f,-0.5f,-0.5f), float3(0.5f,0.5f,0.5f));
}

//---------------------------------------------------------------------------
LightGob::~LightGob()
{   
}

// push Renderable nodes
//virtual 
void LightGob::GetRenderables(RenderableNodeCollector* collector, RenderContext* context)
{
	if (!IsVisible(context->Cam().GetFrustum()))
		return;
    
	super::GetRenderables(collector, context);

    RenderableNode renderable;
    GameObject::SetupRenderable(&renderable,context);
    renderable.mesh = m_mesh;
    renderable.textures[TextureType::DIFFUSE] =  TextureLib::Inst()->GetByName(L"Light.png");
    
    float3 objectPos = &m_world.M41;
    Camera& cam = context->Cam();    
    Matrix billboard = Matrix::CreateBillboard(objectPos,cam.CamPos(),cam.CamUp(),cam.CamLook());       
    float sx = length( float3(&m_local.M11) );
    float sy = length( float3(&m_local.M21) );
    float sz = length( float3(&m_local.M31) );    
    Matrix scale = Matrix::CreateScale(sx,sy,sz);
    renderable.WorldXform = scale * billboard;
    
    RenderFlagsEnum flags = RenderFlags::Textured;
    collector->Add( renderable, flags, Shaders::BillboardShader );
}

}
