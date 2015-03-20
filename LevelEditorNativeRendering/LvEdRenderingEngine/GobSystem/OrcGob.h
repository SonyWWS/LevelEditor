//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "GameObject.h"
#include "../Renderer/Resource.h"

namespace LvEdEngine
{
    class OrcGob : public GameObject
    {
    public:
       OrcGob();
        virtual ~OrcGob();

        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "OrcGob";}

        // push Renderable nodes
		virtual void GetRenderables(RenderableNodeCollector* collector, RenderContext* context);
        virtual void Update(const FrameTime& fr, UpdateTypeEnum updateType);

        // orc functions
        void SetWeight(float w);
        void SetEmotion(int e);
        void SetGoals(int g);
        void SetColor(int c);
        void SetToeColor(int tc);

        void AddAnimation(ResourceReference * ref, int index);
        void RemoveAnimation(ResourceReference * ref);

        void AddGeometry(ResourceReference * ref, int index);
        void RemoveGeometry(ResourceReference * ref);

        void AddFriends(GameObjectReference * ref, int index);
        void RemoveFriends(GameObjectReference * ref);

        void AddTarget(GameObjectReference * ref, int index);
        void RemoveTarget(GameObjectReference * ref);

        void AddChildren(OrcGob * child, int index);
        void RemoveChildren(OrcGob * child);

    protected:
        void BuildRenderables();

        ResourceReference* m_geometry;
        ResourceReference* m_animation;
        GameObjectReference* m_target;
        RenderNodeList m_renderables;

        std::vector<GameObjectReference*> m_friends;
        std::vector<OrcGob*> m_children;
        float m_weight;
        int m_emotion;
        int m_goal;
        int m_color;
        int m_toeColor;
        std::vector<Matrix> m_modelTransforms;
    private:
        typedef GameObject super;
    };
}
