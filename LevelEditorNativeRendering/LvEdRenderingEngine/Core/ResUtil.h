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

   static uint32_t ResourceCount();
   static const wchar_t* GetResourceName(uint32_t index);
   
   
   // user need to to free the returned pointer.
   static uint8_t* LoadResource( const wchar_t* resName, uint32_t* size);

private:
    static void Initialize();
    static bool s_inited;
    static std::vector<std::wstring> s_resourceNames;
    
    // callback for enum resources
    static BOOL CALLBACK EnumResourceCalbk(
        HMODULE hModule,
        LPCTSTR lpszType,
        LPTSTR lpszName,
        LONG_PTR lParam);

};

}
