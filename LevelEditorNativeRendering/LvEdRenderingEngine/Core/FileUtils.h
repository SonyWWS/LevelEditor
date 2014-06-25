//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "WinHeaders.h"
#include <string>

namespace LvEdEngine
{
    class FileUtils
    {
    public:
        static bool Exists(const WCHAR* filename);
        static BYTE* LoadFile(const WCHAR* filename, UINT * sizeOut);
        static std::wstring GetExtensionLower(const WCHAR* filename);
        static const WCHAR* Name(const WCHAR* filename);
    };
}
