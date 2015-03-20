//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "Locator.h"
#include "../Core/Utils.h"
#include "../Renderer/RenderBuffer.h"
#include "../Renderer/Texture.h"
#include "../Renderer/RenderUtil.h"
#include "../Renderer/Model.h"
#include "../ResourceManager/ResourceManager.h"
#include "../Renderer/DeviceManager.h"

namespace LvEdEngine
{

    // ----------------------------------------------------------------------------------
    Locator::Locator()
    {
        m_resource = NULL;
    }

    // ----------------------------------------------------------------------------------
    Locator::~Locator()
    {
        SAFE_DELETE(m_resource);
    }

    // ----------------------------------------------------------------------------------
	void Locator::GetRenderables(RenderableNodeCollector* collector, RenderContext* context)
    {  
		if (!IsVisible(context->Cam().GetFrustum()))
			return;
		super::GetRenderables(collector, context);

        RenderFlagsEnum flags = (RenderFlagsEnum)(RenderFlags::Textured | RenderFlags::Lit);
        collector->Add( m_renderables.begin(), m_renderables.end(), flags, Shaders::TexturedShader );
    }

    void Locator::BuildRenderables()
    {
        m_renderables.clear();
        Model* model = NULL;
        assert(m_resource);
        model = (Model*)m_resource->GetTarget();
        
        assert(model && model->IsReady());

        const NodeDict& nodes = model->Nodes();
        for(auto nodeIt = nodes.begin(); nodeIt != nodes.end(); ++nodeIt)
        {
            Node* node = nodeIt->second;
            assert(m_modelTransforms.size() >= node->index);
            const Matrix& world = m_modelTransforms[node->index]; // transform array holds world matricies already, not local
            for(auto geoIt = node->geometries.begin(); geoIt != node->geometries.end(); ++geoIt)
            {
                Geometry* geo = (*geoIt);
                Material* mat = geo->material;
                RenderableNode renderNode;
                renderNode.mesh = geo->mesh;
                renderNode.WorldXform = world;
                renderNode.bounds = geo->mesh->bounds;
                renderNode.bounds.Transform(renderNode.WorldXform);
                renderNode.objectId = GetInstanceId();
                renderNode.diffuse =  mat->diffuse;
                renderNode.specular = mat->specular.xyz();
                renderNode.specPower = mat->power;
                renderNode.SetFlag( RenderableNode::kShadowCaster, GetCastsShadows() );
                renderNode.SetFlag( RenderableNode::kShadowReceiver, GetReceivesShadows() );

                LightingState::Inst()->UpdateLightEnvironment(renderNode);

                for(unsigned int i = TextureType::MIN; i < TextureType::MAX; ++i)
                {
                    renderNode.textures[i] = geo->material->textures[i];
                }
                m_renderables.push_back(renderNode);
            }
        }
    }


    // ----------------------------------------------------------------------------------
    void Locator::AddResource(ResourceReference* r, int /*index*/)
    {
        m_resource = r;
        m_modelTransforms.clear();
        m_renderables.clear();
        InvalidateBounds();
        InvalidateWorld();        
    }

    // ----------------------------------------------------------------------------------
    void Locator::RemoveResource(ResourceReference* /*r*/)
    {
        AddResource(NULL, -1);
    }

    void Locator::Update(const FrameTime& fr, UpdateTypeEnum updateType)
    {               
        super::Update(fr,updateType);
		bool updatedBound = m_worldXformUpdated;

        Model* model = m_resource ? (Model*)m_resource->GetTarget() : NULL;                     
        if( model && model->IsReady())
        {
            if(m_modelTransforms.empty() || m_worldXformUpdated)
            {
                 const MatrixList& matrices = model->AbsoluteTransforms();
                 m_modelTransforms.resize(matrices.size());
                 for( unsigned int i = 0; i < m_modelTransforms.size(); ++i)
                 {
                     m_modelTransforms[i] = matrices[i] * m_world; // transform matrix array now holds complete world transform.
                 }
                 BuildRenderables(); 
				 updatedBound = true;
            }                               
        }

        m_boundsDirty = updatedBound;
        if(m_boundsDirty)        
        {                  
            if(!m_modelTransforms.empty())
            {                
               // assert(model && model->IsReady());
                m_localBounds = model->GetBounds();                                
                if(m_parent) m_parent->InvalidateBounds();                
            }
            else
            {
                m_localBounds = AABB(float3(-0.5f,-0.5f,-0.5f), float3(0.5f,0.5f,0.5f));                
            }      
            this->UpdateWorldAABB();            
        }

        if(RenderContext::Inst()->LightEnvDirty)
        {
            // update light env.
            for(auto  renderNode = m_renderables.begin(); renderNode != m_renderables.end(); renderNode++)
            {
                LightingState::Inst()->UpdateLightEnvironment(*renderNode);
            }
        }         
    }
}
