//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include "../Renderer/RenderUtil.h"
#include "../Renderer/ShapeLib.h"
#include "../Renderer/Resource.h"
#include "GameObject.h"

namespace LvEdEngine
{

    class PrimitiveShapeGob : public GameObject
    {
    public:
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "PrimitiveShapeGob";}
        PrimitiveShapeGob( RenderShapeEnum shape );
        virtual ~PrimitiveShapeGob();

        void SetColor(int color) { ConvertColor(color, &m_color); }
        void SetEmissive(int color) { ConvertColor(color, &m_emissive); }
        void SetSpecular(int color) { ConvertColor(color, &m_specular); }
        void SetSpecularPower(float specPower) { m_specPower = specPower; }

        void SetDiffuse(wchar_t* filename);
        void SetNormal(wchar_t* filename);
        void SetTextureTransform(const Matrix& xform){m_textureTransform = xform;}

        virtual void Update(const FrameTime& fr, UpdateTypeEnum updateType);
      
        // push Renderable nodes
		virtual void GetRenderables(RenderableNodeCollector* collector, RenderContext* context);
        virtual void SetupRenderable(RenderableNode* r, RenderContext* context);

    protected:
        float4 m_color;
        float3 m_emissive;
        float3 m_specular;
        float m_specPower;
        ResourceReference m_diffuse;
        ResourceReference m_normal;
        Matrix m_textureTransform;
        Mesh* m_mesh;
    private:
        typedef GameObject super;
      
    };

}
