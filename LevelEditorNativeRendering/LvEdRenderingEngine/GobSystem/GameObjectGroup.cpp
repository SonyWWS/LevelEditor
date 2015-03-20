//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "GameObjectGroup.h"
#include <algorithm>
#include "../Renderer/LineRenderer.h"

namespace LvEdEngine
{

    GameObjectGroup::GameObjectGroup()
    {
    }
    
    //virtual 
    GameObjectGroup::~GameObjectGroup()
    {
        for(auto it = m_children.begin(); it != m_children.end(); ++it)
        {
            delete (*it);
        }
        m_children.clear();
    }

    //virtual 
    void GameObjectGroup::GetRenderables(RenderableNodeCollector* collector, RenderContext* context)
    {
		if (!IsVisible(context->Cam().GetFrustum()))
			return;

		super::GetRenderables(collector, context);

          for(auto it = m_children.begin(); it != m_children.end(); ++it)
          {
              (*it)->GetRenderables(collector,context);
          }

         // draw a line from the center of this group to the center of each child.
        // float3  from = m_bounds.GetCenter();
        // float4 color = float4(1.0f,0,0,1.0f);
        // for( auto it = m_children.begin(); it != m_children.end(); ++it)
       //  {              
       //      float3 to =  (*it)->GetBounds().GetCenter();
        //     LineRenderer::Inst()->DrawLine(from,to,color);
        // }   
         
    }

    void GameObjectGroup::AddChild(GameObject* child, int index)
    {
        if(child)
        {
            child->SetParent(this);
            if(index == -1)
            {
                m_children.push_back(child);
            }
            else
            {
                m_children.insert(m_children.begin() + index, child);
            }
        }
    }

    void GameObjectGroup::RemoveChild(GameObject* child)
    {
        if(child)
        {
            child->SetParent(NULL);
            auto it = std::find(m_children.begin(), m_children.end(), child);
            m_children.erase(it);
        }
    }
   
    // ----------------------------------------------------------------------------------
    void GameObjectGroup::InvalidateWorld()
    {
        // if the groups world is dirty, so are all the children's
        m_worldDirty = true;
        for( auto it = m_children.begin(); it != m_children.end(); ++it)
        {
            (*it)->InvalidateWorld();
        }
    }


    void GameObjectGroup::Update(const FrameTime& fr, UpdateTypeEnum updateType)
    {
        bool boundDirty = m_boundsDirty;
        super::Update(fr,updateType);
        m_boundsDirty = boundDirty;
        for( auto it = m_children.begin(); it != m_children.end(); ++it)
        {
            (*it)->Update(fr,updateType);
        }


        // Update bounds
        if(m_boundsDirty)
        {            
            bool usechildbounds = false;
            AABB childbounds; // default ctor will set initial value of min and max.
            // merge bounds for the the visibile children.
            for (auto it = m_children.begin(); it != m_children.end(); ++it)
            {
                if( (*it)->IsVisible())
                {                    
                    AABB local = (*it)->GetLocalBounds();
                    local.Transform((*it)->GetTransform());
                    childbounds.Extend( local );
                    usechildbounds = true;
                }
            }
                 
            // set local bounds 
            m_localBounds = usechildbounds ? childbounds : AABB(float3(-0.5f,-0.5f,-0.5f), float3(0.5f,0.5f,0.5f));
            UpdateWorldAABB();                      
        }
    }
};
