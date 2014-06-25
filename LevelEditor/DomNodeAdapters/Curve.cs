//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

using LevelEditorCore;


namespace LevelEditor.DomNodeAdapters
{
    /// <summary>
    /// DomNodeAdapter for game object groups</summary>
    public class Curve : GameObject, ILinear
    {
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            if (ControlPoints.Count == 0)
            {
                IControlPoint p0 = CreateControlPoint();                
                p0.Translation = new Vec3F(-1f, 0, 0);
                ControlPoints.Add(p0);
            }

            if(ControlPoints.Count < 2)
            {
                IControlPoint p1 = CreateControlPoint();
                p1.Translation = new Vec3F(1, 0, 0);
                ControlPoints.Add(p1);                
            }
            TransformationType = TransformationTypes.Translation;

            DomNode.ChildInserted += DomNode_HierarchyChanged;
            DomNode.ChildRemoved += DomNode_HierarchyChanged;                       
        }
        
        public IList<IControlPoint> ControlPoints
        {
            get { return GetChildList<IControlPoint>(Schema.curveType.pointChild); }
        }

        #region IListable Members

        /// <summary>
        /// Gets display info (label, icon, ...) for the ProjectLister and other controls</summary>
        /// <param name="info">Item info: passed in and modified by this method</param>
        public override void GetInfo(ItemInfo info)
        {
            info.ImageIndex = Util.GetTypeImageIndex(DomNode.Type, info.GetImageList());
            info.Label = Name;
            info.IsLeaf = ControlPoints.Count == 0;

            if (IsLocked)
                info.StateImageIndex = info.GetImageList().Images.IndexOfKey(Sce.Atf.Resources.LockImage);
        }

        #endregion
      
        #region ILinear Members

        public IControlPoint InsertPoint(uint index, float x, float y, float z)
        {            
            IControlPoint cpt = CreateControlPoint();
            int numSteps = GetAttribute<int>(Schema.curveType.stepsAttribute);
            int interpolationType = GetAttribute<int>(Schema.curveType.interpolationTypeAttribute);            
            if (interpolationType != 0 && numSteps > 0)
            {
                index = index / (uint)numSteps;
            }


            Path<DomNode> path = new Path<DomNode>(DomNode.GetPath());
            Matrix4F toworld = TransformUtils.CalcPathTransform(path, path.Count - 1);
            Matrix4F worldToLocal = new Matrix4F();
            worldToLocal.Invert(toworld);
            Vec3F pos = new Vec3F(x, y, z);
            worldToLocal.Transform(ref pos);
            cpt.Translation = pos;
            ControlPoints.Insert((int)index + 1, cpt);
            return cpt;
        }
        
        #endregion


        /// <summary>
        /// Creates a control point</summary>
        /// <returns>Control point</returns>
        public IControlPoint CreateControlPoint()
        {
            DomNode node = new DomNode(Schema.controlPointType.Type);
            node.InitializeExtensions();
            IControlPoint cpt = node.As<IControlPoint>();
            cpt.Name = "ControlPoint";            
            return cpt;
        }

        
        /// <summary>
        /// Compute new transform using 
        /// position of all the control points</summary>
        internal void ComputeTranslation()
        {
            if (m_computingTranslation) return;
            try
            {
                m_computingTranslation = true;
                var points = ControlPoints;
                if (points.Count == 0)
                    return;
                                
                // center in local space.
                Vec3F localcenter = points[0].Translation;

                ITransformable xformcurve = this.As<ITransformable>();
                Matrix4F localToParent = xformcurve.Transform;
                // center in parent space.
                Vec3F center;
                localToParent.Transform(localcenter, out center);                
                xformcurve.Translation = center;
                foreach (var cpt in points)
                {
                    cpt.Translation  -= localcenter;
                }                            
            }
            finally
            {
                m_computingTranslation = false;
            }
        }

        private void DomNode_HierarchyChanged(object sender, ChildEventArgs e)
        {
            if (e.Child.Is<IControlPoint>())
            {
                ComputeTranslation();
            }
        }

        private bool m_computingTranslation;
    }


    /// <summary>
    /// Shows how to custimize curve to force CatmullRom type.
    /// see the schema to show how to hide inherited property.</summary>    
    /// <remarks> 
    /// To avoid registering duplicate DomNodeAdapters
    /// do not derive your DomNodeAdapter from any other DomNodeAdapter that is already 
    /// registered.
    /// For example should not derive from Curve because it already registered
    /// Also should not derive from GameObject 
    /// because curve is registered and curve is a is a gameobject.
    /// </remarks>    
    public class CatmullRom : DomNodeAdapter
    {
        protected override void OnNodeSet()
        {            
            DomNode node = this.DomNode;
            SetAttribute(Schema.catmullRomType.interpolationTypeAttribute, 1);            
        }
    }

    /// <summary>
    /// piecewise Bezier curves with "C1" continuity
    /// </summary>
    public class BezierCurves : DomNodeAdapter
    {
        protected override void OnNodeSet()
        {
            DomNode node = this.DomNode;
            SetAttribute(Schema.catmullRomType.interpolationTypeAttribute, 2);
        }

        public BezierSpline CreateSpline()
        {
            IList<IControlPoint> points = GetChildList<IControlPoint>(Schema.bezierType.pointChild);
            BezierSpline spline = new BezierSpline();
            spline.IsClosed = GetAttribute<bool>(Schema.bezierType.isClosedAttribute);

            BezierPoint bpt = new BezierPoint();
            foreach (var cpt in points)
            {
                bpt.Position = cpt.Translation;
                spline.Add(bpt);
            }            
            return spline;
        }
    }
}
