//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.VectorMath;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Applications;
using LevelEditorCore;

namespace RenderingInterop
{
    /// <summary>
    /// Abstract base class for 3d Manipulators.</summary>
    public abstract class Manipulator : IManipulator
    {
        public Manipulator()
        {
            NodeList = new List<ITransformable>();
            HitMatrix = new Matrix4F();
            DesignView = null;
        }

        #region IManipulator Members

        public virtual bool Pick(ViewControl vc, Point scrPt)
        {
            Matrix4F normWorld = GetManipulatorMatrix();
            if (normWorld == null) return false;
            HitRayV = vc.GetRay(scrPt, vc.Camera.ProjectionMatrix);
            HitPoint = scrPt;
            HitMatrix.Set(normWorld);            
            return true;
        }
        public abstract void Render(ViewControl vc);
        public abstract void OnBeginDrag();
        public abstract void OnDragging(ViewControl vc, Point scrPt);
        public abstract void OnEndDrag(ViewControl vc, Point scrPt);

        public ManipulatorInfo ManipulatorInfo
        {
            get;
            protected set;
        }

        #endregion
        protected abstract Matrix4F GetManipulatorMatrix();
        
        protected ITransformable GetManipulatorNode(TransformationTypes xformType)
        {
            ITransformable manipNode = null;
            var selectionCntx = DesignView.Context.As<ISelectionContext>();
            var visibilityContext = DesignView.Context.As<IVisibilityContext>();
            if (selectionCntx.LastSelected != null)
            {
                Path<object> path = selectionCntx.LastSelected.As<Path<object>>();
                foreach (object obj in path)
                {
                    DomNode pathnode = obj.As<DomNode>();
                    if (pathnode == null) break;
                    object item = Util.AdaptDomPath(pathnode);
                    if (selectionCntx.SelectionContains(item))
                    {
                        var xformable = pathnode.As<ITransformable>();
                        if (xformable != null 
                            && (xformable.TransformationType & xformType) != 0
                            && visibilityContext.IsVisible(pathnode))
                        {
                            manipNode = xformable;                           
                        }
                        break;
                    }
                }
            }
            return manipNode;
        }

        protected List<ITransformable> NodeList
        {
            get;
            private set;

        }

        protected Matrix4F HitMatrix
        {
            get;
            private set;
        }

        [Import(AllowDefault = false)]
        protected IDesignView DesignView
        {
            get;
            private set;
        }
        protected Point HitPoint; // hit point in screen space.
        protected Ray3F HitRayV;  // hit ray in view space.

       
    }
}
