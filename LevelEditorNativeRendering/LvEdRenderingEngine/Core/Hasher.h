//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <map>
#include <string>
#include "WinHeaders.h"


// -------------------------------------------------------------------------
namespace LvEdEngine
{
    typedef uint32_t hash32_t;
    static const hash32_t Hash32InitialValue = 0x811c9dc5; // FNV1 initial value

    // -------------------------------------------------------------------------
    // generate an hash for a string
    hash32_t Hash32(const char * string);
    hash32_t HashLowercase32(const char * string);
};
