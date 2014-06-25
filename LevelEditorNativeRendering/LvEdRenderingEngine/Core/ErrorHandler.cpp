//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "ErrorHandler.h"
#include <stdio.h>
#include <windows.h>
#include "Utils.h"



using namespace LvEdEngine;

static ErrorDescription s_errorDescr;

// -------------------------------------------------------------------------
void ErrorHandler::ClearError()
{
    ErrorDescription* descr = GetError();
    descr->errorType = ErrorType::NoError;        
    ::wcscpy_s( descr->errorText, ARRAY_SIZE(descr->errorText),L"");            
}

// -------------------------------------------------------------------------
void ErrorHandler::SetError(ErrorType::ErrorType type, const char * fmt, ...)
{
    va_list args;
    va_start(args, fmt);

    // sprintf into format and convert to wchar
    char buffer[errorTextSize];
    wchar_t wbuffer[errorTextSize];
    _vsnprintf_s(buffer, errorTextSize, _TRUNCATE, fmt, args);           
    MultiByteToWideChar(0, 0, buffer, -1, wbuffer, errorTextSize);

    // set the description
    ErrorDescription* errorDescription = GetError();
    errorDescription->errorType = type;    
    ::wcscpy_s( errorDescription->errorText, ARRAY_SIZE(errorDescription->errorText),wbuffer);            

    va_end(args);
}

// -------------------------------------------------------------------------
void ErrorHandler::SetError(ErrorType::ErrorType type, const wchar_t * fmt, ...)
{
    va_list args;
    va_start(args, fmt);

    // sprintf into format
    wchar_t wbuffer[errorTextSize];
    _vsnwprintf_s(wbuffer, errorTextSize, _TRUNCATE, fmt, args);           

    // set the description
    ErrorDescription * errorDescription = GetError();
    errorDescription->errorType = type;
    ::wcscpy_s( errorDescription->errorText, ARRAY_SIZE(errorDescription->errorText),wbuffer);            

    va_end(args);
}

// -------------------------------------------------------------------------
ErrorDescription* ErrorHandler::GetError()
{
    ErrorDescription* errorDescription = &s_errorDescr;   
    return errorDescription;
}

