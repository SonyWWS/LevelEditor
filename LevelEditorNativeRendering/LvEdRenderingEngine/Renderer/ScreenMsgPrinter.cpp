//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    LvEdUtils.cpp

****************************************************************************/
#include <stdarg.h>
#include <d3d11.h>
#include "FontRenderer.h"
#include "..\Core\Utils.h"
#include "ScreenMsgPrinter.h"

using namespace LvEdEngine;
using namespace LvEdEngine::LvEdFonts;

//===========================================================================
//
//
//  ScreenMsgPrinter
//
//
//===========================================================================

//---------------------------------------------------------------------------
void ScreenMsgPrinter::AddMsg( int scrnX, int scrnY, const WCHAR* fmt, ... )
{
    va_list args;
    va_start(args, fmt);     /* Initialize variable arguments. */
    GotoXY( scrnX, scrnY );
    AddMsg( fmt, args );
}

//---------------------------------------------------------------------------
void ScreenMsgPrinter::AddMsg( const WCHAR* fmt, ... )
{
    va_list args;
    va_start(args, fmt);     /* Initialize variable arguments. */
    AddMsg( fmt, args );
}

//---------------------------------------------------------------------------
void ScreenMsgPrinter::AddMsg( const WCHAR* fmt, va_list args )
{
    va_list argStart = args;

    int buffSz = 0;
    {
        va_list args = argStart;
        buffSz = _vscwprintf( fmt, args ) + 1;
        va_end(args);
    }

    WCHAR* buffer = (WCHAR*)_alloca( sizeof(WCHAR) * buffSz );
    {
        va_list args = argStart;
        vswprintf_s( buffer, (size_t)buffSz, fmt, args );
        va_end(args);
    }

    int newLineCount = 0;
    for ( const WCHAR* p = buffer; *p != '\0'; p++ )
    {
        if ( *p == '\n' )
        {
            newLineCount++;
        }
    }

    FontRenderer::Inst()->DrawText( m_font, buffer, m_currScrnX, m_currScrnY, m_colorRGBA );

    m_currScrnY += ( newLineCount * m_font->SizeInfo().characterRowHeight );
}
