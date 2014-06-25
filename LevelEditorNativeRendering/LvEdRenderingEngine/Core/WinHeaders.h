//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

// Including SDKDDKVer.h defines the highest available Windows platform.

// If you wish to build your application for a previous Windows platform, include WinSDKVer.h and
// set the _WIN32_WINNT macro to the platform you wish to support before including SDKDDKVer.h.

#include <SDKDDKVer.h>


#define WIN32_LEAN_AND_MEAN    // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>
#include <stdint.h>

// this message is also defined in LevelEditor
// it is used to invalidate all the view panels.
#define InvalidateViews   (WM_USER + 0x1)
// returns module handle for this dll.
HMODULE GetDllModuleHandle();
