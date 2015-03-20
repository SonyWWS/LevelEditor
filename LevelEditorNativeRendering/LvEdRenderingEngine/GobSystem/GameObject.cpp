//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include <D3D11.h>
#include "GameObject.h"
#include "GameObjectComponent.h"
#include <algorithm>

namespace LvEdEngine
{

    // ----------------------------------------------------------------------------------
    GameObject::GameObject()
    {
        m_parent = NULL;
        m_visible = true;
        m_castsShadows = true;
        m_receivesShadows = true;

        m_localBounds = AABB(float3(-0.5f,-0.5f,-0.5f), float3(0.5f,0.5f,0.5f));
        m_bounds = m_localBounds;
    }

    // ----------------------------------------------------------------------------------
    //virtual
    GameObject::~GameObject()
    {
         for(auto it = m_components.begin(); it != m_components.end(); it++)
         {
             delete (*it);
         }
         m_components.clear();
    }

    // ----------------------------------------------------------------------------------
    void GameObject::SetName(const wchar_t *name)
    {
        if(name)
            m_name = name;
        else
            m_name = L"";
    }

    // ----------------------------------------------------------------------------------
    const wchar_t* const GameObject::GetName() const
    {
        return m_name.c_str();
    }

    // ----------------------------------------------------------------------------------
    void GameObject::SetTransform(const Matrix& xform)
    {
        m_local = xform;
        InvalidateWorld();
        InvalidateBounds();
    }

    // ----------------------------------------------------------------------------------
    const Matrix& GameObject::GetTransform() const
    {
        return m_local;
    }

    // ----------------------------------------------------------------------------------
    const AABB& GameObject::GetBounds() const
    {
        return m_bounds;
    }

    const AABB& GameObject::GetLocalBounds() const
    {
        return m_localBounds;
    }


    void GameObject::UpdateWorldTransform()
    {
         // update world transform
        if(m_worldDirty)
        {
            if(m_parent)
            {
                m_world = m_local * m_parent->m_world;
            }
            else
            {
                m_world = m_local;
            }
            m_worldDirty = false;
            m_worldXformUpdated = true;
        
        }
    }

    void GameObject::UpdateWorldAABB()
    {
         // update bounds based on world matrix ( updated in BeginUpdate )
        if(m_boundsDirty)
        {            
            m_bounds = m_localBounds;
            m_bounds.Transform(m_world);            
            m_boundsDirty = false;
            m_worldBoundUpdated = true;
        }
    }

    void GameObject::Update(const FrameTime& fr, UpdateTypeEnum updateType)
    {        
        m_worldXformUpdated = false;
        m_worldBoundUpdated = false;

        for( auto it = m_components.begin(); it != m_components.end(); ++it)
        {
            (*it)->Update(fr,updateType);
        }
        UpdateWorldTransform();
        UpdateWorldAABB();
    }
    

    // ----------------------------------------------------------------------------------
    void GameObject::InvalidateBounds()
    {
        m_boundsDirty = true;
        if(m_parent)
        {
            m_parent->InvalidateBounds();
        }
    }

    // ----------------------------------------------------------------------------------
    void GameObject::InvalidateWorld()
    {
        m_worldDirty = true;
        InvalidateBounds();
    }

    // ----------------------------------------------------------------------------------
    void GameObject::SetParent(GameObject* parent)
    {
        InvalidateBounds(); // mark 'old' ancestors as dirty.
        m_parent = parent;
        InvalidateBounds(); // mark 'new' ancestors as dirty.
        InvalidateWorld();  // mark world as dirty.
    }

    // ----------------------------------------------------------------------------------
    bool GameObject::IsVisible() const
    {
        return m_visible;
    }

    // ----------------------------------------------------------------------------------
    bool GameObject::IsVisible(const Frustum& frustum) const
    {
        bool visible = m_visible;
        if(m_visible)
        {
            visible = TestFrustumAABB(frustum, m_bounds);
        }
        return visible;
    }


    // ----------------------------------------------------------------------------------
    void GameObject::SetVisible(bool visible)
    {
        InvalidateBounds(); // mark 'old' ancestors as dirty.
        m_visible = visible;
    }

   
    // ----------------------------------------------------------------------------------
    //virtual
	void GameObject::GetRenderables(RenderableNodeCollector* collector, RenderContext* context)
    {
		if (!IsVisible(context->Cam().GetFrustum()))
			return;	
		for (auto it = m_components.begin(); it != m_components.end(); ++it)
		{
			(*it)->GetRenderables(collector, context);
		}
    }

    // -----------------------------------------------------------------------------------------------
    void GameObject::SetupRenderable(RenderableNode* r, RenderContext* /*context*/)
    {
        r->objectId = GetInstanceId();
        r->bounds = m_bounds;
        r->WorldXform = m_world;
        r->SetFlag( RenderableNode::kShadowCaster, GetCastsShadows() );
        r->SetFlag( RenderableNode::kShadowReceiver, GetReceivesShadows() );
        LightingState::Inst()->UpdateLightEnvironment( *r );
    }


    void GameObject::AddComponent(GameObjectComponent* component, int index)
    {
        if(component)
        {
            component->SetOwner(this);
            if(index == -1)
            {
                m_components.push_back(component);
            }
            else
            {
                m_components.insert(m_components.begin() + index, component);
            }
        }


    }
    void GameObject::RemoveComponent(GameObjectComponent* component)
    {
        if(component)
        {
            component->SetOwner(NULL);
            auto it = std::find(m_components.begin(), m_components.end(), component);
            m_components.erase(it);
        }

    }

    //============================ GameObjectReference imple ========================


    // -----------------------------------------------------------------------------------------------
    GameObjectReference::GameObjectReference()
    {
        m_target = NULL;
    }

    // -----------------------------------------------------------------------------------------------
    GameObjectReference::GameObjectReference(GameObject* r)
    {
        m_target = r;
    }

    // -----------------------------------------------------------------------------------------------
    GameObjectReference::~GameObjectReference()
    {
    }

    // -----------------------------------------------------------------------------------------------
    GameObject * GameObjectReference::GetTarget()
    {
        return m_target;
    }

    // -----------------------------------------------------------------------------------------------
    void GameObjectReference::SetTarget(GameObject* r, int /*size*/)
    {
        m_target = r;
    }
}
