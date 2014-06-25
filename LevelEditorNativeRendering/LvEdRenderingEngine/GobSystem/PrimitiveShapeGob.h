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

        void SetColor(int color) { m_color = color; };
        void SetDiffuse(wchar_t* filename){m_diffuse.SetTarget(filename);}
        void SetDiffuse(Resource* res){m_diffuse.SetTarget(res);}
        void SetNormal(wchar_t* filename){m_normal.SetTarget(filename);};
        void SetTextureTransform(const Matrix& xform){m_textureTransform = xform;}

        virtual void Update(float dt);
      
        // push Renderable nodes
        virtual bool GetRenderables(RenderableNodeCollector* collector, RenderContext* context);
        virtual void SetupRenderable(RenderableNode* r, RenderContext* context);

    protected:
        int m_color;
        ResourceReference m_diffuse;
        ResourceReference m_normal;
        Matrix m_textureTransform;
        Mesh* m_mesh;
    };

}
