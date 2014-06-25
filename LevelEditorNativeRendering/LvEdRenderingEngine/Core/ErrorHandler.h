//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    ErrorHandler.h

    A singleton class which sends log text to the front end

****************************************************************************/
#pragma once

#include "WinHeaders.h"
#include "typedefs.h"
#include <string>

namespace ErrorType
{
    enum ErrorType
    {
        NoError         = 0,
        UnknownError    = 1,
    };
}

namespace LvEdEngine
{
#define errorTextSize  2048
    struct ErrorDescription
    {
        ErrorType::ErrorType errorType;
        wchar_t errorText[errorTextSize];
    };

    class ErrorHandler
    {
    public:        
        static void ClearError();
        static void SetError(ErrorType::ErrorType type, const char * description, ...);
        static void SetError(ErrorType::ErrorType type, const wchar_t * description, ...);
        static ErrorDescription * GetError();
    private:
        
    };
}

