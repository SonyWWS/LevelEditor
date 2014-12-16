//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "ResUtil.h"
#include <assert.h>


namespace LvEdEngine
{ 
 void* ResUtil::LoadResource(const wchar_t* resType, const wchar_t* resName, uint32_t* outSize)
 {     
     void* buffer = NULL;
     HMODULE handle = GetDllModuleHandle();      
     HRSRC resInfo =  FindResource(handle, resName,resType);
     assert(resInfo);

     DWORD resLen =  SizeofResource(handle, resInfo);
     *outSize = resLen;
     if(resLen > 0)
     {
         HGLOBAL hRes = ::LoadResource(handle, resInfo);
         assert(hRes);
         void* data = LockResource(hRes);
         buffer = malloc(resLen);
         if(buffer)
             CopyMemory(buffer,data,resLen);
     }
     return buffer;
 }
}
