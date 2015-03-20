//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <string>
#include "../Core/Object.h"
#include "../VectorMath/V3dMath.h"
#include "../VectorMath/CollisionPrimitives.h"
#include "../Renderer/Renderable.h"
#include "../Renderer/RenderContext.h"
#include "../Renderer/RenderState.h"
#include "../Renderer/RenderableNodeSorter.h"
#include "../FrameTime.h"


namespace LvEdEngine
{
    
    class GameObjectComponent;
    class QueryFunctor
    {
    public:
        virtual bool operator() (const class GameObject* gob) = 0;
    };

	class GameObject : public Object
	{
	public:
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "GameObject";}

        GameObject();
        virtual ~GameObject();

        void SetName(const wchar_t *name);
        const wchar_t* const GetName() const;
		void SetTransform(const Matrix& xform);
		const Matrix& GetTransform() const;        
        const Matrix& GetWorldTransform() const  { return m_world; }
        const AABB& GetBounds() const;
        const AABB& GetLocalBounds() const;
        bool IsVisible() const;
        bool IsVisible(const Frustum& frustum) const;
        void SetVisible(bool visible);
        bool GetVisible(){return m_visible;}
        bool GetCastsShadows(){return m_castsShadows;}
        void SetCastsShadows(bool castsShadows){m_castsShadows = castsShadows;}
        bool GetReceivesShadows(){return m_receivesShadows;}
        void SetReceivesShadows(bool receivesShadows){m_receivesShadows = receivesShadows;}
        GameObject* Parent(){return m_parent;}

        void AddComponent(GameObjectComponent* component, int index);
        void RemoveComponent(GameObjectComponent* component);

        // push Renderable nodes
        virtual void GetRenderables(RenderableNodeCollector* collector, RenderContext* context);
        virtual void SetupRenderable(RenderableNode* r, RenderContext* context);


        void UpdateWorldTransform();
        void UpdateWorldAABB();
        virtual void Update(const FrameTime& fr, UpdateTypeEnum updateType);
        virtual void InvalidateBounds();
        virtual void InvalidateWorld();

        void SetParent(GameObject* parent);
        virtual void Query(QueryFunctor& func) { func(this);}
    protected:

        GameObject * m_parent;
		Matrix m_local;		
		Matrix m_world;
        AABB m_bounds;  // AABB in world space.
        AABB m_localBounds; // AABB in local space.
        std::wstring m_name;
        bool m_boundsDirty;
        bool m_worldDirty;

        // temp solution.
        bool m_worldXformUpdated;
        bool m_worldBoundUpdated;

		std::vector<GameObjectComponent*> m_components;

    private:
        bool m_visible;
        bool m_castsShadows;
        bool m_receivesShadows;
        
        typedef Object super;
    };


    //--------------------------------------------------
    class GameObjectReference : public Object
    {
    public:
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "GameObjectReference";}
        GameObjectReference();
        GameObjectReference(GameObject* r);
        ~GameObjectReference();
        GameObject * GetTarget();
        void SetTarget(GameObject* r, int size=0);

    protected:
        GameObject* m_target;
    };

}
