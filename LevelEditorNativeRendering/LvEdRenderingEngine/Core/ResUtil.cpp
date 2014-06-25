//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "ResUtil.h"
#include <assert.h>


namespace LvEdEngine
{

 bool ResUtil::s_inited = false;
 std::vector<std::wstring> ResUtil::s_resourceNames;
 

 uint32_t ResUtil::ResourceCount()
 {
     if(!s_inited) Initialize();
     return (uint32_t)s_resourceNames.size();
 }

 const wchar_t* ResUtil::GetResourceName(uint32_t index)
 {
     if(!s_inited) Initialize();
     assert(index < s_resourceNames.size());

     if(index < s_resourceNames.size())
         return s_resourceNames[index].c_str();
     else
         return L"";   
 }

 
 uint8_t* ResUtil::LoadResource( const wchar_t* resName, uint32_t* size)
 {     

     uint8_t* buffer = NULL;
     HMODULE handle = GetDllModuleHandle();      

     HRSRC resInfo =  FindResource(handle, resName,RT_RCDATA);
     assert(resInfo);

     DWORD resLen =  SizeofResource(handle, resInfo);
     *size = resLen;
     if(resLen > 0)
     {
         HGLOBAL hRes = ::LoadResource(handle, resInfo);
         assert(hRes);
         const uint8_t* data = static_cast<const uint8_t*>(LockResource(hRes));
         buffer = new uint8_t[resLen];
         CopyMemory(buffer,data,resLen);
     }

     return buffer;
 }



 BOOL CALLBACK ResUtil::EnumResourceCalbk(
        HMODULE /*hModule*/,
        LPCTSTR /*lpszType*/,
        LPTSTR lpszName,
        LONG_PTR /*lParam*/)
 {     
     s_resourceNames.push_back(lpszName);
     return TRUE;
 }
 void ResUtil::Initialize()
 {
     if(s_inited ) return;

     HMODULE handle = GetDllModuleHandle();
     EnumResourceNames(handle, RT_RCDATA, &ResUtil::EnumResourceCalbk, NULL);

     s_inited = true;
 }

}
