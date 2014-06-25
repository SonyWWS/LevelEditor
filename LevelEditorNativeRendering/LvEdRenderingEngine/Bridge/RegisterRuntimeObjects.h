//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "GobBridge.h"

namespace LvEdEngine
{
    // register object that exist in runtime and C# side
    // and not defined in schema.    
    void RegisterRuntimeObjects( GobBridge& bridge);
};

