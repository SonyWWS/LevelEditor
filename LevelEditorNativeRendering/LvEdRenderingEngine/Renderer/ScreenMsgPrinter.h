//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include <D3D11.h>
#include "Font.h"
#include "FontRenderer.h"
#include "../Core/NonCopyable.h"

namespace LvEdEngine
{
    //-----------------------------------------------------------------------
    //  ScreenMsgPrinter
    //-----------------------------------------------------------------------
    class ScreenMsgPrinter : public NonCopyable
    {
    public:
        ScreenMsgPrinter() : 
            m_font(NULL), m_leftMargin(-1), m_topMargin(-1), 
            m_colorRGBA(float4()), m_currScrnX(0), m_currScrnY(0) {}
        ScreenMsgPrinter( LvEdFonts::Font* aFont, const float4& colorRGBA ) :
            m_font(aFont), m_leftMargin(-1), m_topMargin(-1), 
            m_colorRGBA(colorRGBA), m_currScrnX(0), m_currScrnY(0) {}

        // User must set this
        LvEdFonts::Font* m_font;

        void GotoXY( int x, int y ) { m_currScrnX = x; m_currScrnY = y; }

                // The message is cached until RenderToScreen() is called.
        void    AddMsg( const WCHAR* fmt, ... );
        void    AddMsg( int scrnX, int scrnY, const WCHAR* fmt, ... );

        // User can set these
        int m_leftMargin; 
        int m_topMargin;
        float4 m_colorRGBA;

        // User can set this; ScreenMsgPrinter updates it.
        int m_currScrnX;  // -1 means, "use current"
        int m_currScrnY;  // -1 means, "use current"

    private:
        void AddMsg( const WCHAR* fmt, va_list args );
    };

}
