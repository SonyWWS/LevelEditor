//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <vector>
#include "GameObject.h"

namespace LvEdEngine
{    

    class ResourceReference;
    class Model;
    
    class Locator : public GameObject
    {
    public:
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "Locator";}

        Locator();
        virtual ~Locator();

        virtual bool GetRenderables(RenderableNodeCollector* collector, RenderContext* context);

        void AddResource(ResourceReference * r, int index);
        void RemoveResource(ResourceReference * r);

        void Update(float dt);       
    protected:
        void BuildRenderables();

        ResourceReference* m_resource;
        std::vector<Matrix> m_modelTransforms;
        

        RenderNodeList m_renderables;
    };
}
