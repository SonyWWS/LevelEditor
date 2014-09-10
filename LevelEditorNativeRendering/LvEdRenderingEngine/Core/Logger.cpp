//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    Logger.cpp

    A singleton class which sends log text to the front end

****************************************************************************/


#include "Logger.h"
#include <stdio.h>

namespace LvEdEngine
{
    LogCallbackType Logger::s_callback = NULL;

    // -------------------------------------------------------------------------
    void Logger::LogVA(OutputMessageType::OutputMessageType type, const WCHAR * fmt, va_list args)
    {
        wchar_t wbuffer[c_bufferSize];
        _vsnwprintf_s(wbuffer, c_bufferSize, _TRUNCATE, fmt, args);           
        if(s_callback)
            s_callback((int)type, wbuffer);
        else
            wprintf(wbuffer);
    }

    // -------------------------------------------------------------------------
    void Logger::Log(OutputMessageType::OutputMessageType type, const WCHAR * fmt, ...)
    {        
        va_list args;
        va_start(args, fmt);
        LogVA(type, fmt, args);
        va_end(args);
    }

    // -------------------------------------------------------------------------
    void Logger::LogVA(OutputMessageType::OutputMessageType type, const char * fmt, va_list args)
    {        
        char buffer[c_bufferSize];
        wchar_t wbuffer[c_bufferSize];
        _vsnprintf_s(buffer, c_bufferSize, _TRUNCATE, fmt, args);        
        MultiByteToWideChar(0, 0, buffer, -1, wbuffer, c_bufferSize);
        if(s_callback)
            s_callback((int)type, wbuffer);
        else
            wprintf(wbuffer);
        
    }

    // -------------------------------------------------------------------------
    void Logger::Log(OutputMessageType::OutputMessageType type, const char * fmt, ...)
    {        
        va_list args;
        va_start(args, fmt);
        LogVA(type, fmt, args);
        va_end(args);
    }

    // -------------------------------------------------------------------------
    bool Logger::IsFailureLog(HRESULT hr, const wchar_t * fmt, ...)
    {

        if (SUCCEEDED(hr))
            return false;

        LPWSTR wstr;
        FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM |
				FORMAT_MESSAGE_IGNORE_INSERTS 	 |
				FORMAT_MESSAGE_ALLOCATE_BUFFER,
				NULL,
				hr,
				MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
				(LPTSTR) &wstr,
				0,
				NULL);

       
        if (!fmt)
        {
            Log(OutputMessageType::Error, L"HRESULT: '%s'\n", wstr);
        }
        else
        {
            wchar_t wbuffer[c_bufferSize];
            va_list args;
            va_start(args, fmt);
            _vsnwprintf_s(wbuffer, c_bufferSize, _TRUNCATE, fmt, args);           
            Log(OutputMessageType::Error, L"HRESULT: '%s' in context '%s'\n", wstr, wbuffer);
            va_end(args);
        }

        return true;
    }

}
