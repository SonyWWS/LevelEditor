//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    ShaderLib.h

****************************************************************************/
#pragma once

#include "Shader.h"
#include "../Core/NonCopyable.h"

namespace LvEdEngine
{

class ShaderLib : public NonCopyable
{
public:
    static void         InitInstance(ID3D11Device* device);
    static void         DestroyInstance(void);
    static ShaderLib*   Inst() { return s_Inst; }
    Shader*             GetShader(ShadersEnum shader);

	void Update(const FrameTime& fr, UpdateTypeEnum updateType)
	{
		for (int i = 0; i < Shaders::COUNT; i++)
			m_shaders[i]->Update(fr, updateType);		
	}

private:
    ShaderLib();
    ~ShaderLib();
    static ShaderLib*   s_Inst;

    void                InitIShaders( ID3D11Device* device);
    void                ReleaseIShaders();

    Shader*             m_shaders[Shaders::COUNT];
};

}   // namespace LvEdEngine
