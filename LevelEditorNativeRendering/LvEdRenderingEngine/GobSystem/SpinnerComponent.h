//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "GameObjectComponent.h"

namespace LvEdEngine
{    
    class SpinnerComponent : public GameObjectComponent
    {
    public:
        const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "SpinnerComponent";}
        void Update(const FrameTime& fr, UpdateTypeEnum updateType);  // override
        void SetRPS(Vector3 rps){ m_rps = rps;}
    private:
        typedef GameObjectComponent super;        
        Vector3 m_rps; // revolution per second for x y and z.
		Vector3 m_rot;
    };
}
