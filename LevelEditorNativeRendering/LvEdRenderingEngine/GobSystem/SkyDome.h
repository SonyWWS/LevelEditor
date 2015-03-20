//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "GameObject.h"
#include "DirLightGob.h"
#include "../Renderer/Resource.h"

struct ID3D11VertexShader;
struct ID3D11Buffer;
struct ID3D11PixelShader;
struct ID3D11InputLayout;
struct ID3D11SamplerState;
struct ID3D11DepthStencilState;

namespace LvEdEngine
{    
	
    //sky color = 150, 178 220
    //sun color =  221 188 151

    // sky gradient:      8 25  60        24 61 132   


    class VertexBuffer;
    class IndexBuffer;

	class SkyDome : public GameObject
	{
	public:
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "SkyDome";}
        SkyDome();
        ~SkyDome();
		       
        void SetCubeMap(wchar_t* filename);
        void Render( RenderContext* context);
	private:
        Texture* m_texture;
        typedef GameObject super;
	};

};
