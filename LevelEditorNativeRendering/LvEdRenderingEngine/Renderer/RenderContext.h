//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <D3D11.h>
#include "../VectorMath/V3dMath.h"
#include "../VectorMath/CollisionPrimitives.h"
#include "../Core/Object.h"
#include "RenderEnums.h"
#include <cstdint>
#include <map>
#include "Lights.h"
#include "../VectorMath/Camera.h"
#include <hash_set>

typedef std::hash_set<ObjectGUID> Selection;

namespace LvEdEngine
{
    class RenderState;
    
     class RenderContext : public NonCopyable
     {
     public:
        
         static void    InitInstance(ID3D11Device* device);
         static void    DestroyInstance(void);
         static RenderContext*   Inst() { return s_inst; }
        
        // Getting Context State
        ID3D11Device* Device() const {return m_device;}
        ID3D11DeviceContext* Context() const {return m_context;}        
        RenderState* State() const {return m_currentState; }
        Camera& Cam()  {return m_cam;}
        
        const ExpFog& GlobalFog() const {return m_fog;}
        const float4& ViewPort(){return m_viewPort;}
        void SetState(RenderState* state){ m_currentState = state; }
        void SetViewPort(float4 vp) { m_viewPort = vp; }
        void  SetFog(ExpFog fog) { m_fog = fog;}
        // Setting Context State        
        void SetContext(ID3D11DeviceContext* context){m_context = context;}
        // Object Ids of any items currently selected.
        Selection selection;
        bool LightEnvDirty;

    private:
        RenderContext() {}
        ~RenderContext();
        Camera m_cam;
        ID3D11Device* m_device;
        ID3D11DeviceContext* m_context;        
        float4 m_viewPort;
        ExpFog  m_fog;
        RenderState* m_currentState;
        static RenderContext*   s_inst;
    };
}
