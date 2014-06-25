//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

// basic types and structures
typedef unsigned __int64 ObjectGUID;
typedef unsigned __int32 ObjectTypeGUID;
typedef unsigned __int32 ObjectPropertyUID;
typedef unsigned __int32 ObjectListUID;

typedef void (__stdcall * LogCallbackType)(int messageType, wchar_t * text);
