//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "GameObjectComponent.h"
#include "../Renderer/Model.h"

namespace LvEdEngine
{    
    class MeshComponent : public RenderComponent
    {
    public:
        const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "MeshComponent";}
        void Update(const FrameTime& fr, UpdateTypeEnum updateType);  // override
        void SetRef(const wchar_t* path);

    private:
        typedef RenderComponent super;        
        Model* m_model;
    };
}