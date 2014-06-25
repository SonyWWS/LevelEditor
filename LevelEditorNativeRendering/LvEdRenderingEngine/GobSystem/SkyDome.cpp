//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "SkyDome.h"
#include "../Core/Utils.h"
#include "../Renderer/ShapeLib.h"
#include "../Renderer/ShaderLib.h"
#include "../Renderer/SkyDomeShader.h"
#include "../Renderer/Texture.h"
#include "../Renderer/TextureLib.h"
#include "../ResourceManager/ResourceManager.h"

#define SkyCubeMap
namespace LvEdEngine
{
    // ----------------------------------------------------------------------------------
    SkyDome::SkyDome()
    {
       m_texture = NULL;
    }
    
    SkyDome::~SkyDome()
    {
        SAFE_RELEASE(m_texture);
        ResourceManager::Inst()->GarbageCollect();
    }
    void SkyDome::SetCubeMap(wchar_t* filename)
    {
        SAFE_RELEASE(m_texture);
        ResourceManager::Inst()->GarbageCollect();
        if(filename && wcslen(filename) > 0)
            m_texture = (Texture*) ResourceManager::Inst()->LoadImmediate(filename,NULL);

    }

    void SkyDome::Render( RenderContext* context)
    {
        if(IsVisible() == false) 
            return;
       
        RenderableNode r;        
        r.mesh = ShapeLibGetMesh(RenderShape::Sphere);
        r.objectId = GetInstanceId();    
        r.textures[TextureType::Cubemap] = m_texture ? m_texture : TextureLib::Inst()->GetDefault(TextureType::Cubemap);       
        r.SetFlag( RenderableNode::kShadowCaster, false );
        r.SetFlag( RenderableNode::kShadowReceiver, false );

        SkyDomeShader* pShader =(SkyDomeShader*) ShaderLib::Inst()->GetShader(Shaders::SkyDomeShader); 
        pShader->Begin( context);
        pShader->SetRenderFlag( RenderFlags::None );   // call this *after* Begin()        
        pShader->Draw(r);
        
        pShader->End();
    }
    
};
