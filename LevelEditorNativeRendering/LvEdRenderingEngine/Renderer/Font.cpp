//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    FontSource.cpp

****************************************************************************/
#include "d3d11.h"
#include <assert.h>
#include <Gdiplus.h>
#include "DeviceManager.h"
#include "../Core\Utils.h"
#include "Font.h"

using namespace LvEdEngine;
using namespace LvEdFonts;

static const WCHAR s_StartChar      = 33;
static const WCHAR s_EndChar        = 127;
static const WCHAR s_NumChars       = ( s_EndChar - s_StartChar ); // doesn't include s_EndChar

static Gdiplus::FontStyle 
                FlagsToGdiStyleEnum( FontStyleFlags fontStyles );
static void     GetCharacterSizes(Gdiplus::Font& font, Gdiplus::Graphics& charGraphics, FontAtlasSizeInfo* info /*OUT*/);
static void     CreateAndIndexAtlasBitmap(Gdiplus::Font& font, Gdiplus::Graphics& charGraphics, Gdiplus::Bitmap& charBitmap, 
                    Gdiplus::Graphics& fontSheetGraphics, FontAtlasSizeInfo* pSizeInfo,
                    ScreenRect* pCharacterMap );
static HRESULT  CreateAtlasTexture(ID3D11Device* device, Gdiplus::Bitmap& fontSheetBitmap, const FontAtlasSizeInfo* info,
                    ID3D11Texture2D** ppD3dTexture, ID3D11ShaderResourceView** ppD3dShaderResourceView);
static int      GetCharMinX(Gdiplus::Bitmap& charBitmap);
static int      GetCharMaxX(Gdiplus::Bitmap& charBitmap);


//---------------------------------------------------------------------------
Font::Font()
  : m_pixelFontSize( 0.0f ),
    m_fontStyles( 0 ),
    m_antiAliased( false ),
    m_monoSpace( false ),
    m_d3dTexture( NULL ),
    m_d3dShaderResourceView( NULL )
{
    m_characterLookup = new ScreenRect[s_NumChars]; 
}

//---------------------------------------------------------------------------
Font::~Font()
{
	SAFE_RELEASE( m_d3dTexture );
	SAFE_RELEASE( m_d3dShaderResourceView );
    SAFE_DELETE_ARRAY( m_characterLookup );
}

//---------------------------------------------------------------------------
const ScreenRect& Font::CharRect(WCHAR aChar)
{
    assert( aChar < s_EndChar );
    assert( aChar >= s_StartChar );
    return m_characterLookup[aChar - s_StartChar];
}

//---------------------------------------------------------------------------
// STATIC
Font* Font::CreateNewInstance(ID3D11Device* device, 
                            const WCHAR* fontName, 
                            float pixelFontSize, 
                            FontStyleFlags fontStyles, 
                            bool antiAliased)
{
    using namespace Gdiplus;

    LvEdFonts::Font* pFont = new Font;

    pFont->m_fontName          = fontName;
    pFont->m_pixelFontSize     = pixelFontSize;
    pFont->m_fontStyles        = fontStyles;
    pFont->m_antiAliased       = antiAliased;

	HRESULT hr = S_OK;

	ULONG_PTR token = NULL;
	GdiplusStartupInput startupInput(NULL, TRUE, TRUE);
	GdiplusStartupOutput startupOutput;
	GdiplusStartup(&token, &startupInput, &startupOutput);

    hr = pFont->InitTextureAndShaderObjects(device, fontName, pixelFontSize, fontStyles, antiAliased );

	GdiplusShutdown(token);
    if ( FAILED( hr ) )
    {
        SAFE_DELETE( pFont );
    }
    return pFont;
}

//---------------------------------------------------------------------------
HRESULT Font::InitTextureAndShaderObjects(
                            ID3D11Device* device, 
                            const WCHAR* fontName, 
                            float pixelFontSize, 
                            FontStyleFlags fontStyles, 
                            bool antiAliased )
{
    using namespace Gdiplus;

   	HRESULT hr = S_OK;

	Gdiplus::Font font( fontName, pixelFontSize, FlagsToGdiStyleEnum( fontStyles), UnitPixel);

	TextRenderingHint hint = antiAliased ? TextRenderingHintAntiAlias : TextRenderingHintSystemDefault;

	//
	// Bitmap for drawing a single char.
	//
	int tempSize = (int)(pixelFontSize * 2);
	Bitmap charBitmap(tempSize, tempSize, PixelFormat32bppARGB);
	Graphics charGraphics(&charBitmap);
	charGraphics.SetPageUnit(UnitPixel);
	charGraphics.SetTextRenderingHint(hint);
	
    //
	// Grab some size information.
    //
	GetCharacterSizes(font, charGraphics, &m_sizeInfo);

	//
	// Bitmap for storing all the char sprites on a sprite sheet.
	//
	Bitmap fontSheetBitmap(m_sizeInfo.atlasTextureWidth, m_sizeInfo.atlasTextureHeight, PixelFormat32bppARGB);
	Graphics fontSheetGraphics(&fontSheetBitmap);
	fontSheetGraphics.SetCompositingMode(CompositingModeSourceCopy);
	fontSheetGraphics.Clear(Color(0, 0, 0, 0));

	CreateAndIndexAtlasBitmap(font, charGraphics, charBitmap, fontSheetGraphics,
            &m_sizeInfo, m_characterLookup );

	CreateAtlasTexture(device, fontSheetBitmap, &m_sizeInfo, &m_d3dTexture, &m_d3dShaderResourceView );
    return hr;
}

void Font::SetMonospace( bool bSet )
{
    m_monoSpace = bSet;

    //
    //  For monospace font rendering, use the letter 'W' to determine the
    //  widest character we'll need to draw.
    //
    if ( m_monoSpace )
    {
        const ScreenRect& r = CharRect( 'W' );
        m_sizeInfo.monoSpaceCharacterWidth = ( r.Width() - m_sizeInfo.gapBetweenChars );    // looks better if we subtract 1 gap
    }
}

//---------------------------------------------------------------------------
void GetCharacterSizes(Gdiplus::Font& font, Gdiplus::Graphics& charGraphics, FontAtlasSizeInfo* info /*OUT*/)
{
    using namespace Gdiplus;

    //
    // Make a string comprising all characters we'll have in the font.
    //
	WCHAR allChars[s_NumChars + 1];
	for ( WCHAR i = 0; i < s_NumChars; ++i )
    {
		allChars[i] = ( s_StartChar + i );
    }
	allChars[s_NumChars] = 0;

    //
    //  Measure the entire character set.
    //
	RectF sizeRect;
	charGraphics.MeasureString(allChars, s_NumChars, &font, PointF(0, 0), &sizeRect);
	info->characterRowHeight = (int)(sizeRect.Height + 0.5f);  

    //
	//  The texture's width is fixed and already determined, so compute
    //  how many rows we'll need to place all of the characters on it,
    //  and how tall the texture will need to be.
    //
	int numRows = (int)(sizeRect.Width / info->atlasTextureWidth) + 1;  
	info->atlasTextureHeight  = (int)(numRows*info->characterRowHeight) + 1;

    //
    //  No use "drawing" a space character. Just remember how much of
    //  a horizontal shift is needed to represent a space on the screen.
    //
    {
	    WCHAR charString[2] = {' ', 0};
	    charGraphics.MeasureString(charString, 1, &font, PointF(0, 0), &sizeRect);
	    info->spaceCharacterWidth = (int)(sizeRect.Width + 0.5f);
    }
  
    //
    //  For the gap between characters, apply a factor to the size of
    //  a basic character width.
    //
    {
        static const float kCharGapFactor = 0.10f;   // portion of a space character
	    WCHAR charString[2] = {'x', 0};
	    charGraphics.MeasureString(charString, 1, &font, PointF(0, 0), &sizeRect);
	    info->gapBetweenChars = (int)(sizeRect.Width * kCharGapFactor );
    }
}

//---------------------------------------------------------------------------
void CreateAndIndexAtlasBitmap(Gdiplus::Font& font, Gdiplus::Graphics& charGraphics, Gdiplus::Bitmap& charBitmap,
            Gdiplus::Graphics& fontSheetGraphics, FontAtlasSizeInfo* pSizeInfo,
            ScreenRect* pCharacterMap )
{
    using namespace Gdiplus;

	WCHAR charString[2] = {' ', 0};
	SolidBrush whiteBrush(Color(255, 255, 255, 255));
	UINT fontSheetX = 0;
	UINT fontSheetY = 0;
	for(UINT i = 0; i < s_NumChars; ++i)
	{
		charString[0] = static_cast<WCHAR>(s_StartChar + i);
		charGraphics.Clear(Color(0, 0, 0, 0));
		charGraphics.DrawString(charString, 1, &font, PointF(0.0f, 0.0f), &whiteBrush);

		// Compute tight char horizontal bounds (ignoring empty space).
		int minX = GetCharMinX(charBitmap);
		int maxX = GetCharMaxX(charBitmap);
		int charWidth = maxX - minX + 1;

		// Move to next row of the font sheet?
		if(fontSheetX + charWidth >= pSizeInfo->atlasTextureWidth)
		{
			fontSheetX = 0;
			fontSheetY += static_cast<int>(pSizeInfo->characterRowHeight) + 1;
		}

		// Save the rectangle of this character on the texture atlas.
		pCharacterMap[i] = ScreenRect(fontSheetX, fontSheetY, fontSheetX + charWidth, fontSheetY + pSizeInfo->characterRowHeight);

		// The rectangle subset of the source image to copy.
		fontSheetGraphics.DrawImage(&charBitmap, fontSheetX, fontSheetY, 
			minX, 0, charWidth, pSizeInfo->characterRowHeight, UnitPixel);

		// next char
		fontSheetX += charWidth + 1;
	}
}

//---------------------------------------------------------------------------
HRESULT CreateAtlasTexture(ID3D11Device* device, Gdiplus::Bitmap& fontSheetBitmap, const FontAtlasSizeInfo* pSizeInfo,
    ID3D11Texture2D** ppD3dTexture, ID3D11ShaderResourceView** ppD3dShaderResourceView )
{
    using namespace Gdiplus;

	HRESULT hr = S_OK;

	// Lock the bitmap for direct memory access
	BitmapData bmData;
    Rect rect(0, 0, pSizeInfo->atlasTextureWidth, pSizeInfo->atlasTextureHeight );
	fontSheetBitmap.LockBits(&rect,ImageLockModeRead, PixelFormat32bppARGB, &bmData);  

	// Copy into a texture.
	D3D11_TEXTURE2D_DESC texDesc;
	texDesc.Width  = pSizeInfo->atlasTextureWidth;
	texDesc.Height = pSizeInfo->atlasTextureHeight;
	texDesc.MipLevels = 1;
	texDesc.ArraySize = 1;
	texDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
	texDesc.SampleDesc.Count = 1;
	texDesc.SampleDesc.Quality = 0;
	texDesc.Usage = D3D11_USAGE_IMMUTABLE;
	texDesc.BindFlags = D3D11_BIND_SHADER_RESOURCE;
	texDesc.CPUAccessFlags = 0;
	texDesc.MiscFlags = 0;

	D3D11_SUBRESOURCE_DATA data;        
	data.pSysMem = bmData.Scan0;
	data.SysMemPitch = pSizeInfo->atlasTextureWidth * 4;
	data.SysMemSlicePitch = 0;

	hr = device->CreateTexture2D(&texDesc, &data, ppD3dTexture );
	if(FAILED(hr))
		return hr;

	D3D11_SHADER_RESOURCE_VIEW_DESC srvDesc;
	srvDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
	srvDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D;
	srvDesc.Texture2D.MipLevels = 1;
	srvDesc.Texture2D.MostDetailedMip = 0;

	hr = device->CreateShaderResourceView(*ppD3dTexture, &srvDesc, ppD3dShaderResourceView );
	if(FAILED(hr))
		return hr;

	fontSheetBitmap.UnlockBits(&bmData);  

	return hr;
}

//---------------------------------------------------------------------------
int GetCharMinX(Gdiplus::Bitmap& charBitmap)
{
    using namespace Gdiplus;

	int width  = charBitmap.GetWidth();
	int height = charBitmap.GetHeight();
	
	for(int x = 0; x < width; ++x)
	{
		for(int y = 0; y < height; ++y)
		{
			Color color;
			charBitmap.GetPixel(x, y, &color);
			if(color.GetAlpha() > 0)
			{
				 return x;
			}
		}
	}

	return 0;
}

//---------------------------------------------------------------------------
int GetCharMaxX(Gdiplus::Bitmap& charBitmap)
{
    using namespace Gdiplus;

	int width  = charBitmap.GetWidth();
	int height = charBitmap.GetHeight();

	for(int x = width-1; x >= 0; --x)
	{
		for(int y = 0; y < height; ++y)
		{
			Color color;
			charBitmap.GetPixel(x, y, &color);
			if(color.GetAlpha() > 0)
			{
				 return x;
			}
		}
	}

	return width-1;
}


//---------------------------------------------------------------------------
Gdiplus::FontStyle FlagsToGdiStyleEnum( FontStyleFlags fontStyles )
{
    switch ( fontStyles )
    {
        case kFontStyleNORMAL :
            return Gdiplus::FontStyleRegular;

        case kFontStyleBOLD :
            return Gdiplus::FontStyleBold;

        case kFontStyleITALIC :
            return Gdiplus::FontStyleItalic;

        case ( kFontStyleBOLD | kFontStyleITALIC ):
            return Gdiplus::FontStyleBoldItalic;

        case kFontStyleUNDERLINE :
            return Gdiplus::FontStyleUnderline;

        case kFontStyleSTRIKEOUT :
            return Gdiplus::FontStyleStrikeout;
    }

    assert( false );
    return Gdiplus::FontStyleRegular;
}
