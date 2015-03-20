//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "GameObject.h"
#include <vector>

namespace LvEdEngine
{    
    class GameObjectGroup : public GameObject
    {
    public:
        GameObjectGroup();
        virtual ~GameObjectGroup();
        virtual const char* ClassName()  const  {return StaticClassName();}
        static const char* StaticClassName(){return "GameObjectGroup";}

        virtual void GetRenderables(RenderableNodeCollector* collector, RenderContext* context);
        void AddChild(GameObject* child, int index);
        void RemoveChild(GameObject* child);

        virtual void Update(const FrameTime& fr, UpdateTypeEnum updateType);        
        virtual void InvalidateWorld();

        virtual void Query(QueryFunctor& func)
        {
            if(func(this))
            {
                for(auto iter = m_children.begin(); iter != m_children.end(); iter++)
                    if(!func(*iter)) return;
            }
        }

    protected:
        std::vector<GameObject*> m_children;
    private:
        typedef GameObject super;
    };
}
