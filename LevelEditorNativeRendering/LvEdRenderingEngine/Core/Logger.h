//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    Logger.h

    A singleton class which sends log text to the front end

****************************************************************************/
#pragma once

#include "WinHeaders.h"
#include "typedefs.h"
#include "NonCopyable.h"

#define __WIDEN(x) L##x
#define WIDEN(x) __WIDEN(x)
#define __WFUNCTION__ WIDEN(__FUNCTION__)

namespace OutputMessageType
{
    enum OutputMessageType
    {
        Error = 0,
        Warning = 1,
        Info = 2,
        Debug = 3,
    };
}


namespace LvEdEngine
{

    class Logger
    {
    public:
        static void SetLogCallback(LogCallbackType callback) { s_callback = callback; }
        static void Log(OutputMessageType::OutputMessageType type, const WCHAR * fmt, ...);
        static void Log(OutputMessageType::OutputMessageType type, const char * fmt, ...);
        static bool IsFailureLog(HRESULT hr, const wchar_t * fmt = NULL, ...);

        static void LogVA(OutputMessageType::OutputMessageType type, const WCHAR * fmt, va_list args);
        static void LogVA(OutputMessageType::OutputMessageType type, const char * fmt, va_list args);

    private:
        static const int c_bufferSize = 2048;
        static LogCallbackType s_callback;
    };
}

