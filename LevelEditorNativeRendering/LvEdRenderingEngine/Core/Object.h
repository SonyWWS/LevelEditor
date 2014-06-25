//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include "typedefs.h"
#include "NonCopyable.h"
#include <stdint.h>

namespace LvEdEngine
{    
    // base class for all object types, GameLevel, GameObject, etc.
    class Object : public NonCopyable
    {
    public:
        virtual const char* ClassName()  const  = 0;
        ObjectGUID GetInstanceId() const
        {
            return (ObjectGUID)this;
        }
        virtual ~Object(void){}

        virtual void Invoke(wchar_t* /*fn*/, const void* /*arg*/, void** /*retVal*/) {}
    };
}
