//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "RenderContext.h"
#include "RenderState.h"
#include "..\Core\Utils.h"

namespace LvEdEngine
{

RenderContext*  RenderContext::s_inst = NULL;

//---------------------------------------------------------------------------
void RenderContext:: InitInstance(ID3D11Device* device)
{
    if(s_inst == NULL)
        s_inst = new RenderContext();

    s_inst->m_device = device;
    s_inst->m_rsCache = new RenderStateCache(device);
    s_inst->LightEnvDirty = true;
}

RenderContext::~RenderContext()
{
    SAFE_DELETE(m_rsCache);
}

void RenderContext::DestroyInstance()
{
   SAFE_DELETE(s_inst);
   
}

}
