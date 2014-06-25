//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include <string>
#include "../Core/Object.h"

#include "../Renderer/Resource.h"
#include "FontTypes.h"

namespace LvEdEngine
{
    class DeviceManager;

    namespace LvEdFonts
    {
        //-------------------------------------------------------------------
        //  FontAtlasSizeInfo
        //-------------------------------------------------------------------
        class FontAtlasSizeInfo : public NonCopyable
        {
        public:
            UINT atlasTextureWidth;
            UINT atlasTextureHeight;
            int  characterRowHeight;
            int  spaceCharacterWidth;
            int  monoSpaceCharacterWidth;
            int  gapBetweenChars;

            static const UINT kTextureWidth = 1024;

            FontAtlasSizeInfo() :
                atlasTextureWidth(kTextureWidth), atlasTextureHeight(0),
                characterRowHeight(0), spaceCharacterWidth(0), monoSpaceCharacterWidth(0), gapBetweenChars(0) {}
        };
        
        //-------------------------------------------------------------------
        //  Font
        //-------------------------------------------------------------------
        class Font : public Object
        {
        public:
            virtual const char* ClassName() const {return StaticClassName();}
            static const char* StaticClassName(){return "Font";}
            
            static Font*                CreateNewInstance(ID3D11Device* device, 
                                                const WCHAR* fontName, 
                                                float pixelFontSize, 
                                                FontStyleFlags fontStyles, 
                                                bool antiAliased=true);
            virtual ~Font();

            const WCHAR*                GetFontName()               { return m_fontName.c_str(); }
            float                       GetFontSize()               { return m_pixelFontSize; }
            FontStyleFlags              GetFontStyleFlags()         { return m_fontStyles; }
            bool                        GetAntiAliased()            { return m_antiAliased; }
            void                        SetMonospace( bool bSet );
            bool                        GetMonospace()              { return m_monoSpace; }

                                            // For treating the scren as a series of rows
                                            // of this font's height.
            int                         RowAsScrnY( int row )       { return ( row * m_sizeInfo.characterRowHeight ); }

            const ScreenRect&           CharRect( WCHAR aChar );
            const FontAtlasSizeInfo&    SizeInfo()                  { return m_sizeInfo; }
            ID3D11ShaderResourceView*   GetShaderResourceView()     { return m_d3dShaderResourceView; }

        private:
            Font();

            HRESULT                     InitTextureAndShaderObjects( ID3D11Device* device, 
                                                const WCHAR* fontName, 
                                                float pixelFontSize, 
                                                FontStyleFlags fontStyles, 
                                                bool antiAliased );

            std::wstring                m_fontName;
            float                       m_pixelFontSize;
            FontStyleFlags              m_fontStyles;
            bool                        m_antiAliased;
            bool                        m_monoSpace;

            ID3D11Texture2D*            m_d3dTexture;
	        ID3D11ShaderResourceView*   m_d3dShaderResourceView;
            ScreenRect*                 m_characterLookup;
            FontAtlasSizeInfo           m_sizeInfo;
        };


    };  // namespace LvEdFonts
};  // namespace LvEdEngine
