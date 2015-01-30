//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    FontRenderer.h

    A singleton class which can be used to draw text in screen space.

****************************************************************************/
#pragma once

#include <d3d11.h>
#include <vector>
#include "FontTypes.h"
#include "../Core/StringBlob.h"
#include "../Core/NonCopyable.h"

namespace LvEdEngine
{
    class DeviceManager;
    class RenderContext;

    namespace LvEdFonts
    {
        //-------------------------------------------------------------------
        //  FontRenderer
        //-------------------------------------------------------------------
        class FontRenderer : public NonCopyable
        {
        public:
            static void             InitInstance(DeviceManager* pDeviceManager);
            static void             DestroyInstance(void);
            static FontRenderer*    Inst() { return s_Inst; }

            // Draw deferred text items. Call this once per frame.
            void                    FlushPrintRequests( RenderContext* rc );
            uint32_t                PrintReqCacheSize() { return (uint32_t)m_printRequests.size(); }

            //---------------------------------------------------------------
            //
            //  Basic drawing:
            //
            //  1. Pass all of the arguments in the DrawText() call.
            //
            //  NOTE: Caches a request. Drawing does not occur until
            //  FlushPrintRequests() is called.
            //---------------------------------------------------------------
                    // Specify destination in screen space.
            void    DrawText( Font* aFont, const WCHAR* aString, int scrnTopX, int scrnTopY,
                        const float4& colorRGBA, float scale=1.0f );

            // NOTE: Not cached. Draws the text immediately
            void    DrawTextImmediate( RenderContext* rc, Font* aFont, const WCHAR* aString, int scrnTopX, int scrnTopY,
                        const float4& colorRGBA, float scale=1.0f );

            //---------------------------------------------------------------
            //
            // Using the renderer state:
            //
            //  1.  Call the "set" methods to prepare the state of the renderer.
            //  2.  Use the simple DrawText() overload to display text using the
            //      current state. Updates a virtual cursor like printf
            //      output to a console.
            //
            //---------------------------------------------------------------

            void        SetFont( Font* aFont )                  { m_requestedFont = aFont; }
            Font*       GetFont()                               { return m_requestedFont; }

            void        SetColor( const float4& colorRGBA )     { m_colorRGBA = colorRGBA; }
            float4      GetColor()                              { return m_colorRGBA; }

            void        SetScale( float scale )                 { m_scale = scale; }
            float       GetScale()                              { return m_scale; }

                        // Sets the x position for the *next* draw when a line
                        // contains '\n'.
            void        SetLeftMargin( int x )                  { m_leftMargin = x; }
            int         GetLeftMargin()                         { return m_leftMargin; }

            void        SetX( int x )                           { m_targetX = x; }
            int         GetX()                                  { return m_targetX; }

            void        SetY( int y )                           { m_targetY = y; }
            int         GetY()                                  { return m_targetY; }

            void        MoveTo( int x, int y, bool bSetLeftMargin );

                        //
                        // Draw text using the current state.
                        //
                        //  If the string passed to DrawText() ends with a '\n', the x position
                        //  is reset to either the left margin, if one was set, or to the starting
                        //  x position of the last DrawText() call.
                        //
                        //  Here is an example where setting the left margin is necessary:
                        //
                        //      DrawText( "Hello " );
                        //      DrawText( "World!" );   <--- No '\n' in the previous line, so we continue where we left off.
                        //      DrawText( "\n" );       <--- No left margin set, so 'x' is set to the start of "World", the previous call.
                        //      DrawText( "Where am I?" );
                        //
                        //      Output:
                        //          Hello World!
                        //                Where am I?    <--- We want this to be flush left with "Hello World!", but it isn't. :(
                        //
                        //  In a case like that, set a left margin:
                        //
                        //      SetLeftMargin( 20 );
                        //      DrawText( "Hello " );   <--- Text drawing starts at the left margin.
                        //      DrawText( "World!" );   <--- No '\n' in the previous line, so we continue where we left off.
                        //      DrawText( "\n" );       <--- 'x' is set to position 20, the explicitly set left margin.
                        //      DrawText( "Where am I?" );
                        //
                        //      Output:
                        //          Hello World!
                        //          Where am I?         <--- Ahhh, that's better.
                        //
                        //  NOTE: Caches a request. Drawing does not occur until FlushPrintRequests() is called.
                        //
            void        DrawText( RenderContext* rc, const WCHAR* aString );

        private:
            FontRenderer();
            ~FontRenderer();

            void Init(DeviceManager* pDeviceManager);

            //---------------------------------------------------------------
            //  FontRenderer::FontTextVertex
            //---------------------------------------------------------------
            struct FontTextVertex
            {
                float3 Pos;
                float2 Tex;
                float4 ColorRGBA;
            };

            //---------------------------------------------------------------
            //  FontRenderer::FontDrawOp
            //---------------------------------------------------------------
            class FontDrawOp
            {
            public:
                ScreenRect m_fontPgSrcRect;
                ScreenRect m_scrnDestRect;

                    // Returns the number of items consumed (i.e., 4).
                void InitQuad( FontRenderer* pFontWriter, FontRenderer::FontTextVertex* quad ) const;
            };
            typedef std::vector<FontDrawOp> FontDrawOpArray;

            //---------------------------------------------------------------
            //  FontRenderer::VertexBufferAccessor
            //---------------------------------------------------------------
            class VertexBufferAccessor : public NonCopyable
            {
            public:
                VertexBufferAccessor( ID3D11DeviceContext* context, ID3D11Buffer* vertexBuffer );
                ~VertexBufferAccessor();
                FontRenderer::FontTextVertex* VertexData()  { return m_buffer; }
            private:
                ID3D11DeviceContext*    m_context;
                ID3D11Buffer*           m_vertexBuffer;
                FontTextVertex*         m_buffer;
            };

            void            FontDrawingBegin( RenderContext* rc );
            void            FontDrawingEnd();
            void            FontDrawOpsSetActiveFont( void );
            void            FontDrawOpsExecute( void );
            void            ProcessChar( WCHAR aChar );
            void            ProcessOneBatch( size_t startIndex, size_t numToProcess );
            size_t          GetMaxBatch( void ) { return s_maxBatchSize; }
            ID3D11SamplerState*
                            CreateFontSamplerState(ID3D11Device* device);

            DeviceManager*          m_deviceManager;
            RenderContext*          m_currentRC;   // transient

            ID3D11Buffer*           m_vertexBuffer;
            ID3D11Buffer*           m_indexBuffer;
            ID3D11VertexShader*     m_vertexShader;
            ID3D11PixelShader*      m_pixelShader;
            ID3D11InputLayout*      m_vertexLayout;
            ID3D11SamplerState*     m_samplerState;
            ID3D11RasterizerState*  m_rasterState;
            ID3D11BlendState*       m_blendState;
            Font*                   m_requestedFont;
            Font*                   m_activeFont;
            int                     m_leftMargin;
            int                     m_targetX;
            int                     m_targetY;
            int                     m_currX;
            int                     m_currY;
            float4                  m_colorRGBA;
            float                   m_scale;
            static size_t           s_maxBatchSize;

            FontDrawOpArray m_fontDrawOps;

            //
            //  Deferred Screen Drawing
            //
            //  Print requests are cached until FlushPrintRequests() is called
            //  at the end of the frame.
            //
            class FontPrintRequest
            {
            public:
                FontPrintRequest() : x(-1), y(-1), scale(1.0f), font(NULL), colorRGBA(float4()), m_msgIndex(0) {}
                int x;
                int y;
                float scale;
                LvEdFonts::Font* font;
                float4 colorRGBA;
                size_t m_msgIndex;       // To obtain the string from m_stringBlob.
            };
            typedef StringBlob<WCHAR,8192> StringBlob;
            typedef std::vector<FontPrintRequest> FontPrintRequestList;
            StringBlob m_stringBlob;
            FontPrintRequestList  m_printRequests;

            void    SendTextToScreen( const FontPrintRequest& printReq, const WCHAR* msgText );

            static FontRenderer*  s_Inst;

        };

    };  // namespace LvEdFonts
};  // namespace LvEdEngine

