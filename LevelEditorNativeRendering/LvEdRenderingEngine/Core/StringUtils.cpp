//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "StringUtils.h"

namespace StrUtils
{

bool Equal(wchar_t* str1, wchar_t* str2)
{
    if(str1 == 0 || str2 == 0) return false;
    return wcscmp(str1,str2) == 0;
}

}