//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "GameObject.h"
#include "ControlPointGob.h"
#include "../Renderer/Resource.h"
#include "../Renderer/Model.h"

namespace LvEdEngine
{
    class CurveGob : public GameObject
    {
    public:
        enum InterpolationType
        {
            // NOTE: This enum corresponds to the enum used in level_editor.xsd for the editing of the enumeration
            // any changes here need to be reflected there.
            Linear = 0,
            CatmullRom = 1,
            Bezier = 2,
        };

        CurveGob();        
        virtual ~CurveGob();

        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "CurveGob";}

        // push Renderable nodes
        virtual void GetRenderables(RenderableNodeCollector* collector, RenderContext* context);    
        virtual void Update(const FrameTime& fr, UpdateTypeEnum updateType);
        

        void SetColor(int color) { m_color = color; };
        void SetClosed(bool closed);
        void SetSteps(int steps);
        void SetInterpolationType(int type);
        int GetColor(){return m_color;}

        void AddPoint(ControlPointGob* point, int index);
        void RemovePoint(ControlPointGob* point);
        virtual void InvalidateWorld();

    protected:

        bool m_needsRebuild;
        bool m_closed;
        int m_steps;
        int m_color;
        InterpolationType m_type;

        std::vector<ControlPointGob*> m_points;       
        Mesh m_mesh;

    private:
        typedef GameObject super;
    };
}
