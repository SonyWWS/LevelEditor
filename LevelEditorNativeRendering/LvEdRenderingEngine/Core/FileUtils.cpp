//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "WinHeaders.h"
#include <WinBase.h>
#include "FileUtils.h"

namespace LvEdEngine
{

// ----------------------------------------------------------------------------------------------
// ----------------------------------------------------------------------------------------------

// ----------------------------------------------------------------------------------------------
bool FileUtils::Exists(const WCHAR* filename)
{
    if(filename == NULL || wcslen(filename) == 0) return false;
    return GetFileAttributes(filename) != 0xFFFFFFFF;    
}

// ----------------------------------------------------------------------------------------------
BYTE* FileUtils::LoadFile(const WCHAR* filename, UINT* sizeOut)
{
    BYTE * data = NULL;
    UINT cBytes = 0;

    // Open the file
    HANDLE fileHandle = CreateFile(filename, FILE_READ_DATA, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_FLAG_SEQUENTIAL_SCAN, NULL);
    if (fileHandle != INVALID_HANDLE_VALUE)
    {
        // Get the file size
        LARGE_INTEGER FileSize;
        GetFileSizeEx(fileHandle, &FileSize);
        cBytes = FileSize.LowPart;

        // Allocate memory and NULL terminate
        data = new BYTE[ cBytes + 1 ];        
        if (data)
        {
            // Read in the file
            data[cBytes] = NULL;
            DWORD dwBytesRead=0;
            ReadFile(fileHandle, data, cBytes, &dwBytesRead, NULL);
            if (dwBytesRead != cBytes)
            {
                delete[] data;
                data = NULL;
            }
        }
        CloseHandle(fileHandle);
    }
    *sizeOut =cBytes;
    return data;
}

// ----------------------------------------------------------------------------------------------
// Get file extension in lower case.
std::wstring FileUtils::GetExtensionLower(const WCHAR* filename)
{
    wchar_t ext[255];
    const wchar_t* str = wcsrchr(filename, L'.');
    if (str != NULL)
    {
        ::wcscpy_s(ext,(sizeof(ext)/sizeof(ext[0])),str);        
        int c = 0;
        while(ext[c]) {ext[c] = towlower(ext[c]); c++; }        
        return std::wstring(ext);
    }
    return std::wstring();
}

// ----------------------------------------------------------------------------------------------
const WCHAR* FileUtils::Name(const WCHAR* filename)
{
    const WCHAR* lastSlash = filename;
    while(*filename)
    {
        if (*filename == L'/' || *filename == L'\\')
        {
            lastSlash = filename + 1;
        }
        ++filename;
    }
    return lastSlash;
}

}; // namespace
