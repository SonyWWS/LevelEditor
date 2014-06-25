//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    ShaderLib.cpp

****************************************************************************/
#include <stdlib.h>
#include "../Core/Utils.h"
#include "BasicShader.h"
#include "BillboardShader.h"
#include "ShaderLib.h"
#include "SkyDomeShader.h"
#include "TexturedShader.h"
#include "TerrainShader.h"
#include "WireFrameShader.h"
#include "NormalsShader.h"

using namespace LvEdEngine;

ShaderLib*  ShaderLib::s_Inst = NULL;

//---------------------------------------------------------------------------
ShaderLib::ShaderLib()
{
}

//---------------------------------------------------------------------------
ShaderLib::~ShaderLib()
{
    ReleaseIShaders();
}

//---------------------------------------------------------------------------
void ShaderLib::InitInstance(ID3D11Device* device)
{
    s_Inst = new ShaderLib;

    s_Inst->InitIShaders(device);
}

//---------------------------------------------------------------------------
void ShaderLib::DestroyInstance( void )
{
    SAFE_DELETE( s_Inst );
}

//---------------------------------------------------------------------------
Shader* ShaderLib::GetShader(ShadersEnum shader)
{
    assert(shader < Shaders::COUNT );
    return m_shaders[shader];
}

//---------------------------------------------------------------------------
void ShaderLib::InitIShaders( ID3D11Device* device)
{
    m_shaders[Shaders::SkyDomeShader]        = new SkyDomeShader( device );
    m_shaders[Shaders::TexturedShader]       = new TexturedShader( device );
    m_shaders[Shaders::BillboardShader]      = new BillboardShader( device );
    m_shaders[Shaders::WireFrameShader]      = new WireFrameShader( device );
    m_shaders[Shaders::BasicShader]          = new BasicShader( device );
    m_shaders[Shaders::TerrainShader]        = new TerrainShader( device );
    m_shaders[Shaders::NormalsShader]        = new NormalsShader( device );
    
}

//---------------------------------------------------------------------------
void ShaderLib::ReleaseIShaders( void )
{
    for ( int i=0; i < Shaders::COUNT; i++ )
    {
        SAFE_DELETE( m_shaders[i] );
    }
}
