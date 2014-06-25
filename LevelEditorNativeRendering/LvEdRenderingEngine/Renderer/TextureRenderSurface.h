//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include "../Core/WinHeaders.h"
#include "RenderSurface.h"


namespace LvEdEngine
{

class TextureRenderSurface : public RenderSurface
{
public:
    TextureRenderSurface(ID3D11Device* pd3dDevice,
        int width,
        int height,
        bool hasDepthBuffer,
        bool hasColorBuffer,        
        DXGI_FORMAT colorBufferFormat,
        uint32_t sampleCount,
        uint32_t sampleQuality);


  /*  void Begin(ID3D11DeviceContext* dc);
    void End();*/
        
       
    virtual ~TextureRenderSurface(void);
    virtual const char* ClassName() const {return StaticClassName();}
    static const char* StaticClassName(){return "TextureRenderSurface";}

    void Resize(int w, int h);
    SurfaceType GetType() {return kTexture2d;}

};

}// namespace LvEdEngine
