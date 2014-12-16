//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include "WinHeaders.h"
#include <vector>
#include "NonCopyable.h"

namespace LvEdEngine
{

// util for loading and enumerating  embedded resources 
// of type RT_RCDATA
class ResUtil : public NonCopyable
{
public:   
   // user need to to free the returned pointer.
   static void* LoadResource(const wchar_t* resType, const wchar_t* resName, uint32_t* outSize);        
};

}
