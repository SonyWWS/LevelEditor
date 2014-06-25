//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include <stdint.h>
#include "..\VectorMath\V3dMath.h"

namespace LvEdEngine
{
    namespace LvEdFonts
    {
        class Font;

        //-------------------------------------------------------------------
        //  FontKey
        //-------------------------------------------------------------------
        typedef int FontKey;
        static const FontKey    kInvalidFontKey = (FontKey)-1;
        static const FontKey    kDefaultFontKey = (FontKey)0;

        //-------------------------------------------------------------------
        //  eFontStyle
        //-------------------------------------------------------------------
        enum eFontStyle
        {
            kFontStyleNORMAL    = ( 1 << 1 ),
            kFontStyleBOLD      = ( 1 << 2 ),
            kFontStyleITALIC    = ( 1 << 3 ),
            kFontStyleUNDERLINE = ( 1 << 4 ),
            kFontStyleSTRIKEOUT = ( 1 << 5 )
        };
        typedef unsigned int FontStyleFlags;    // eFontStyle values OR'd together

        //-------------------------------------------------------------------
        //  ScreenRect
        //-------------------------------------------------------------------
        class ScreenRect
        {
        public:
            ScreenRect() : topX(0), topY(0), bottX(0), bottY(0) {}
            ScreenRect( int scrnRectTopX, int scrnRectTopY, int scrnRectBottX, int scrnRectBottY )
              : topX(scrnRectTopX), topY( scrnRectTopY ), bottX( scrnRectBottX ), bottY( scrnRectBottY ) {}

            int Width() const  { return ( bottX - topX ); }
            int Height() const { return ( bottY - topY ); }

            int topX;   // where 0 is left edge of screen
            int topY;   // where 0 is top edge of screen
            int bottX;
            int bottY;
        };

    };
};  // namespace LvEdEngine
    
