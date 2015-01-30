//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <vector>
#include "../Core/WinHeaders.h"
#include "../Core/Utils.h"
#include "../VectorMath/V3dMath.h"
#include "../VectorMath/CollisionPrimitives.h"
#include "RenderEnums.h"
#include <d3d11.h>

namespace LvEdEngine
{    
    // SHADERS
    ID3DBlob* CompileShaderFromResource(LPCWSTR resourceName, LPCSTR szEntryPoint, LPCSTR szShaderModel, const D3D_SHADER_MACRO *shaderMacros);   
};
