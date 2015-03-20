//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <string>
#include "../Core/Object.h"
#include "../VectorMath/V3dMath.h"
#include "../FrameTime.h"

namespace LvEdEngine
{
    class GameObject;
	class RenderContext;
	class RenderableNodeCollector;
    // base class for game object components.
    class GameObjectComponent : public Object
    {
    public:
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "GameObjectComponent";}

		
		virtual void Update(const FrameTime& fr, UpdateTypeEnum updateType) {}

		// Allow component to push renderable nodes.
		// Non visual components don't need to override this function.
		virtual void GetRenderables(RenderableNodeCollector* collector, RenderContext* context) {}

        void SetName(const wchar_t* name)
        {
            if(name)
                m_name = name;
            else
                m_name = L"";
        }
        const wchar_t* const GetName() const;
        void SetActive(bool active) {m_active = active;}
        bool GetActive() const { return m_active;} 
        GameObject* GetOwner() {return m_owner;}
        
    private:
        typedef Object super;
        friend GameObject;
        void SetOwner(GameObject* gob) { m_owner = gob; }
        // m_owner is the game object owns this component.
        GameObject* m_owner;		
        std::wstring m_name; // component name.
        bool m_active;
    };

    // base class of all component that need to have transform.
    class TransformComponent : public GameObjectComponent
    {
    public:
        const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "TransformComponent";}

        void SetTranslation(const Vector3& trans) {m_translation = trans;}
        const Vector3& GetTranslation() const {return m_translation;}        
        void SetRotation(const Vector3& rot) {m_rotation = rot;}
        const Vector3& GetRotation() const {return m_rotation;}

        void SetScale(const Vector3& scale) {m_scale = scale;}
        const Vector3& GetScale() const {return m_scale;}

    private:
        typedef GameObjectComponent super;
        Vector3 m_translation;
        Vector3 m_rotation;
        Vector3 m_scale;
    };


    class RenderComponent : public TransformComponent
    {
    public:
        const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "RenderComponent";}

        void SetVisible(bool visible) { m_visible = visible;}
        bool GetVisible() const {return m_visible;}

        void SetCastShadow(bool castShadow) {m_castShadow = castShadow;}
        bool GetCastShadow() const {return m_castShadow;}

        void SetReceiveShadow(bool receiveShadow) {m_receiveShadow = receiveShadow;}
        bool GetReceiveShadow() const {return m_receiveShadow;}

        void SetDrawDistance(float drawDistance) 
        {
            if(drawDistance < 0.0f) drawDistance = 0.0f;
            m_drawDistance = drawDistance;
        }
        float GetDrawDistance() const {return m_drawDistance;}



    private:
        typedef TransformComponent super;
        bool m_visible;
        bool m_castShadow;
        bool m_receiveShadow;
        float m_drawDistance;
    };
    

   




}