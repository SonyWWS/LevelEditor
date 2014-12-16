//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.VectorMath;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Applications;
using LevelEditorCore;
using LevelEditorCore.VectorMath;

using Camera = Sce.Atf.Rendering.Camera;

namespace RenderingInterop
{
    [Export(typeof(IManipulator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ExtensionManipulator : Manipulator
    {
        public ExtensionManipulator()
        {
            ManipulatorInfo = new ManipulatorInfo("Extension".Localize(),
                                         "Extension manipulator".Localize(),
                                         LevelEditorCore.Resources.ExtensionImage,
                                         Keys.None);

            m_axisColor = new Color[Enum.GetValues(typeof(HitRegion)).Length];
            m_axisColor[(int)HitRegion.XAxis] = XAxisColor;
            m_axisColor[(int)HitRegion.YAxis] = YAxisColor;
            m_axisColor[(int)HitRegion.ZAxis] = ZAxisColor;
            m_axisColor[(int)HitRegion.NegXAxis] = ControlPaint.LightLight(XAxisColor);
            m_axisColor[(int)HitRegion.NegYAxis] = ControlPaint.LightLight(YAxisColor);
            m_axisColor[(int)HitRegion.NegZAxis] = ControlPaint.LightLight(ZAxisColor);
            m_highlightColor = Color.Gold;

        }


        public override bool Pick(ViewControl vc, Point scrPt)
        {
            m_hitRegion = HitRegion.None;
            if (base.Pick(vc, scrPt) == false)
                return false;

            Camera camera = vc.Camera;            
            float s = Util.CalcAxisScale(vc.Camera, HitMatrix.Translation, AxisLength, vc.Height);

            Matrix4F vp = camera.ViewMatrix * camera.ProjectionMatrix;
            Matrix4F wvp = HitMatrix * vp;

            // get ray in object space  space.
            Ray3F rayL = vc.GetRay(scrPt, wvp);

            m_scale = new Vec3F(1, 1, 1);            
            m_hitScale = s;
            

            Vec3F min = new Vec3F(-0.5f, -0.5f, -0.5f);
            Vec3F max = new Vec3F(0.5f, 0.5f, 0.5f);
            AABB box = new AABB(min, max);
            Matrix4F boxScale = new Matrix4F();
            Matrix4F boxTrans = new Matrix4F();
            Matrix4F BoxMtrx = new Matrix4F();

            float handleScale = s * AxisHandle;

            // +X axis
            boxScale.Scale(new Vec3F(s, handleScale, handleScale));
            boxTrans.Translation = new Vec3F(s / 2, 0, 0);
            BoxMtrx = boxScale * boxTrans;
            
            Ray3F ray = rayL;
            BoxMtrx.Invert(BoxMtrx);
            ray.Transform(BoxMtrx);

            if (box.Intersect(ray))
            {
                m_hitRegion = HitRegion.XAxis;
                return true;
            }

            // -X
            boxTrans.Translation = new Vec3F(-s / 2, 0, 0);
            BoxMtrx = boxScale * boxTrans;
            
            ray = rayL;
            BoxMtrx.Invert(BoxMtrx);
            ray.Transform(BoxMtrx);

            if (box.Intersect(ray))
            {
                m_hitRegion = HitRegion.NegXAxis;                
                return true;
            }

            // y axis
            boxScale.Scale(new Vec3F(handleScale, s, handleScale));
            boxTrans.Translation = new Vec3F(0, s / 2, 0);
            BoxMtrx = boxScale * boxTrans;
           
            ray = rayL;
            BoxMtrx.Invert(BoxMtrx);
            ray.Transform(BoxMtrx);
            if (box.Intersect(ray))
            {
                m_hitRegion = HitRegion.YAxis;
                return true;
            }

            // -Y
            boxTrans.Translation = new Vec3F(0, -s / 2, 0);
            BoxMtrx = boxScale * boxTrans;
            ray = rayL;
            BoxMtrx.Invert(BoxMtrx);
            ray.Transform(BoxMtrx);
            if (box.Intersect(ray))
            {
                m_hitRegion = HitRegion.NegYAxis;                
                return true;
            }


            // z axis
            boxScale.Scale(new Vec3F(handleScale, handleScale, s));
            boxTrans.Translation = new Vec3F(0, 0, s / 2);
            BoxMtrx = boxScale * boxTrans;

            ray = rayL;
            BoxMtrx.Invert(BoxMtrx);
            ray.Transform(BoxMtrx);
            if (box.Intersect(ray))
            {
                m_hitRegion = HitRegion.ZAxis;
                return true;
            }

            // -Z
            boxTrans.Translation = new Vec3F(0, 0, -s / 2);
            BoxMtrx = boxScale * boxTrans;

            ray = rayL;
            BoxMtrx.Invert(BoxMtrx);
            ray.Transform(BoxMtrx);
            if (box.Intersect(ray))
            {
                m_hitRegion = HitRegion.NegZAxis;                
                return true;
            }

            return false;
        }

        public override void Render(ViewControl vc)
        {
                       
            Matrix4F normWorld = GetManipulatorMatrix();
            if (normWorld == null) return;
            
            int axis = (int)m_hitRegion;

            // axis colors
            Color saveColor = m_axisColor[axis];
            m_axisColor[axis] = m_highlightColor;
            Color xcolor  = m_axisColor[(int)HitRegion.XAxis];
            Color ycolor  = m_axisColor[(int)HitRegion.YAxis];
            Color zcolor  = m_axisColor[(int)HitRegion.ZAxis];
            Color nxcolor = m_axisColor[(int)HitRegion.NegXAxis];
            Color nycolor = m_axisColor[(int)HitRegion.NegYAxis];
            Color nzcolor = m_axisColor[(int)HitRegion.NegZAxis];
            m_axisColor[axis] = saveColor;

          
            if (m_hitRegion != HitRegion.None)
            {
                normWorld.Translation = HitMatrix.Translation;                    
            }

            Vec3F pos = normWorld.Translation;            
            float s = Util.CalcAxisScale(vc.Camera, pos, AxisLength, vc.Height);

            Vec3F sv = new Vec3F(s, s, s);
            Vec3F axscale = new Vec3F(s*AxisThickness, s, s*AxisThickness);            
            bool negativeAxis = m_hitRegion == HitRegion.NegXAxis || m_hitRegion == HitRegion.NegYAxis || m_hitRegion == HitRegion.NegZAxis;
            Vec3F dragScale = new Vec3F(Math.Abs(m_scale.X),
                Math.Abs(m_scale.Y), Math.Abs(m_scale.Z));
          
            

            Matrix4F rot = new Matrix4F();
            Matrix4F scale = new Matrix4F();
            axscale.Y = negativeAxis ? s : s * dragScale.X;
            scale.Scale(axscale);           
            rot.RotZ(-MathHelper.PiOver2);
            Matrix4F xform = scale * rot * normWorld;
            Util3D.DrawCylinder(xform, xcolor);

            axscale.Y = negativeAxis ? s : s * dragScale.Y;
            scale.Scale(axscale);
            xform = scale * normWorld;
            Util3D.DrawCylinder(xform, ycolor);

            axscale.Y = negativeAxis ? s : s * dragScale.Z;
            scale.Scale(axscale);
            rot.RotX(MathHelper.PiOver2);
            xform = scale * rot * normWorld;
            Util3D.DrawCylinder(xform, zcolor);


            rot.RotZ(MathHelper.PiOver2);
            axscale.Y = negativeAxis ? s * dragScale.X : s;
            scale.Scale(axscale);
            xform = scale * rot * normWorld;
            Util3D.DrawCylinder(xform, nxcolor);

            rot.RotZ(MathHelper.Pi);
            axscale.Y = negativeAxis ? s * dragScale.Y : s;
            scale.Scale(axscale);
            xform = scale * rot * normWorld;
            Util3D.DrawCylinder(xform, nycolor);

            rot.RotX(-MathHelper.PiOver2);
            axscale.Y = negativeAxis ? s * dragScale.Z : s;
            scale.Scale(axscale);
            xform = scale * rot * normWorld;
            Util3D.DrawCylinder(xform, nzcolor);


            // draw center cube
            scale.Scale(s*(1.0f / 16.0f));
            xform = scale * normWorld;
            Util3D.DrawCube(xform, Color.White);
            
            Vec3F handle = sv*AxisHandle;
            float handleWidth = handle.X/2;
            scale.Scale(handle);
            Matrix4F trans = new Matrix4F();

            
            // X handle
            float drag = m_hitRegion == HitRegion.XAxis ? dragScale.X : 1.0f;
            trans.Translation = new Vec3F(drag * sv.X - handleWidth, 0, 0);            
            xform = scale * trans * normWorld;
            Util3D.DrawCube(xform, xcolor);

            // y handle
            drag = m_hitRegion == HitRegion.YAxis ? dragScale.Y : 1.0f;
            trans.Translation = new Vec3F(0, drag * sv.Y - handleWidth, 0);
            xform = scale * trans * normWorld;
            Util3D.DrawCube(xform, ycolor);

            // z handle
            drag = m_hitRegion == HitRegion.ZAxis ? dragScale.Z : 1.0f;
            trans.Translation = new Vec3F(0, 0, drag * sv.Z - handleWidth);
            xform = scale * trans * normWorld;
            Util3D.DrawCube(xform, zcolor);


            // -x handle
            drag = m_hitRegion == HitRegion.NegXAxis ? dragScale.X : 1.0f;
            trans.Translation = new Vec3F(-sv.X * drag + handleWidth, 0, 0);
            xform = scale * trans * normWorld;
            Util3D.DrawCube(xform, nxcolor);

            // -y handle
            drag = m_hitRegion == HitRegion.NegYAxis ? dragScale.Y : 1.0f;
            trans.Translation = new Vec3F(0, -sv.Y * drag + handleWidth, 0);
            xform = scale * trans * normWorld;
            Util3D.DrawCube(xform, nycolor);

            // -z handle
            drag = m_hitRegion == HitRegion.NegZAxis ? dragScale.Z : 1.0f;
            trans.Translation = new Vec3F(0, 0, -sv.Z * drag + handleWidth);
            xform = scale * trans * normWorld;
            Util3D.DrawCube(xform, nzcolor);
        }

        public override void OnBeginDrag()
        {
            if (m_hitRegion == HitRegion.None)
                return;

            var selection = DesignView.Context.As<ISelectionContext>().Selection;
            var transactionContext = DesignView.Context.As<ITransactionContext>();
            NodeList.Clear();

            m_isUniformScaling = false;

            IEnumerable<DomNode> rootDomNodes = DomNode.GetRoots(selection.AsIEnumerable<DomNode>());
            foreach (DomNode node in rootDomNodes)
            {
                ITransformable transNode = node.As<ITransformable>();
                if (transNode == null || (transNode.TransformationType & TransformationTypes.Scale) == 0)
                    continue;

                IVisible vn = node.As<IVisible>();
                if (!vn.Visible) continue;

                ILockable lockable = node.As<ILockable>();
                if (lockable.IsLocked) continue;

                // force uniform scaling if any node requires it
                if ((transNode.TransformationType & TransformationTypes.UniformScale) == TransformationTypes.UniformScale)
                    m_isUniformScaling = true;                

                NodeList.Add(transNode);

                IManipulatorNotify notifier = transNode.As<IManipulatorNotify>();
                if (notifier != null) notifier.OnBeginDrag();
            }


            // to compute offset use bounding box in local space.
            Vec3F offset = Vec3F.ZeroVector;// 0.5f; // use bounding box in local space 
            
            switch (m_hitRegion)
            {
                case HitRegion.XAxis:                  
                case HitRegion.YAxis:                  
                case HitRegion.ZAxis:   
                    offset = new Vec3F(-1, -1, -1);                    
                    break;
                case HitRegion.NegXAxis:                    
                case HitRegion.NegYAxis:                    
                case HitRegion.NegZAxis:  
                    offset = new Vec3F(1, 1, 1);                    
                    break;
                default:
                    break;

            }
          
            m_originalScales = new Vec3F[NodeList.Count];
            m_originalTranslations = new Vec3F[NodeList.Count];
            m_pivotOffset = new Vec3F[NodeList.Count];
            int k = 0;
            foreach (ITransformable node in NodeList)
            {
                IBoundable boundable = node.As<IBoundable>();
                Vec3F pivot = Vec3F.Mul(boundable.LocalBoundingBox.Radius, offset);
                m_pivotOffset[k] = pivot;
                
                m_originalScales[k] = node.Scale;

                Matrix4F mtrx = TransformUtils.CalcTransform(
                   Vec3F.ZeroVector,
                   node.Rotation,
                   node.Scale,
                   pivot
                   );

                m_originalTranslations[k] = node.Translation - mtrx.Translation;
                k++;
            }

            if(NodeList.Count > 0)
                transactionContext.Begin("Extend".Localize());                
        }

        public override void OnDragging(ViewControl vc, Point scrPt)
        {
            if (m_hitRegion == HitRegion.None || NodeList.Count == 0)
                return;

            Matrix4F view = vc.Camera.ViewMatrix;
            // compute world * view 
            Matrix4F wv = new Matrix4F();
            wv.Mul(HitMatrix, view);

            // create ray in view space.            
            Ray3F rayV = vc.GetRay(scrPt, vc.Camera.ProjectionMatrix);

            Vec3F xAxis = wv.XAxis;
            Vec3F yAxis = wv.YAxis;
            Vec3F zAxis = wv.ZAxis;
            Vec3F origin = wv.Translation;
 
            m_scale = new Vec3F(1, 1, 1);
            float scale = 1;
            float a1, a2;
            
            switch (m_hitRegion)
            {
                case HitRegion.XAxis:  
                case HitRegion.NegXAxis:
                    {
                        a1 = Math.Abs(Vec3F.Dot(HitRayV.Direction, yAxis));
                        a2 = Math.Abs(Vec3F.Dot(HitRayV.Direction, zAxis));
                        Vec3F axis = (a1 > a2 ? yAxis : zAxis);
                        Vec3F p0 = HitRayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        Vec3F p1 = rayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        float dragAmount = Vec3F.Dot((p1 - p0), xAxis);                        
                        if (m_hitRegion == HitRegion.NegXAxis)
                        {
                            dragAmount *= -1;                            
                        }
                        m_scale.X = 1.0f + dragAmount / m_hitScale;
                        scale = m_scale.X;                        
                        
                    }

                    break;
                case HitRegion.YAxis:
                case HitRegion.NegYAxis:
                    {
                        a1 = Math.Abs(Vec3F.Dot(HitRayV.Direction, zAxis));
                        a2 = Math.Abs(Vec3F.Dot(HitRayV.Direction, xAxis));
                        Vec3F axis = (a1 > a2 ? zAxis : xAxis);
                        Vec3F p0 = HitRayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        Vec3F p1 = rayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        float dragAmount = Vec3F.Dot((p1 - p0), yAxis);                        
                        if (m_hitRegion == HitRegion.NegYAxis)
                        {
                            dragAmount *= -1;
                        }
                        m_scale.Y = 1.0f + dragAmount / m_hitScale;
                        scale = m_scale.Y;                        
                        
                    }
                    break;
                case HitRegion.ZAxis:
                case HitRegion.NegZAxis:
                    {
                        a1 = Math.Abs(Vec3F.Dot(HitRayV.Direction, xAxis));
                        a2 = Math.Abs(Vec3F.Dot(HitRayV.Direction, yAxis));
                        Vec3F axis = (a1 > a2 ? xAxis : yAxis);
                        Vec3F p0 = HitRayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        Vec3F p1 = rayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        float dragAmount = Vec3F.Dot((p1 - p0), zAxis);                        
                        if (m_hitRegion == HitRegion.NegZAxis)
                        {
                            dragAmount *= -1;                            
                        }
                        m_scale.Z = 1.0f + dragAmount / m_hitScale;
                        scale = m_scale.Z;                        
                        
                    }
                    break;
               
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if(m_isUniformScaling)
                m_scale = new Vec3F(scale,scale,scale);

            
            // scale 
            for (int i = 0; i < NodeList.Count; i++)
            {
                ITransformable node = NodeList[i];               
                node.Scale = Vec3F.Mul(m_originalScales[i], m_scale);

                Matrix4F mtrx = TransformUtils.CalcTransform(
                   Vec3F.ZeroVector,
                   node.Rotation,
                   node.Scale,
                    m_pivotOffset[i]);                
                node.Translation = m_originalTranslations[i] + mtrx.Translation;
                
            }                                      
        }

        public override void OnEndDrag(ViewControl vc, Point scrPt)
        {
            if (NodeList.Count > 0)
            {
                for (int k = 0; k < NodeList.Count; k++)
                {
                    IManipulatorNotify notifier = NodeList[k].As<IManipulatorNotify>();
                    if (notifier != null) notifier.OnEndDrag();
                }

                var transactionContext = DesignView.Context.As<ITransactionContext>();
                try
                {
                    if (transactionContext.InTransaction)
                        transactionContext.End();
                }
                catch (InvalidTransactionException ex)
                {
                    if (transactionContext.InTransaction)
                        transactionContext.Cancel();

                    if (ex.ReportError)
                        Outputs.WriteLine(OutputMessageType.Error, ex.Message);
                }
            }
            
            NodeList.Clear();
            m_originalScales = null;
            m_originalTranslations = null;
            m_hitRegion = HitRegion.None;
            m_scale = new Vec3F(1, 1, 1);
        }

        
        protected override Matrix4F GetManipulatorMatrix()
        {
            ITransformable node = GetManipulatorNode(TransformationTypes.Scale);
            if (node == null ) return null;

            Path<DomNode> path = new Path<DomNode>(node.Cast<DomNode>().GetPath());
            Matrix4F localToWorld = TransformUtils.CalcPathTransform(path, path.Count - 1);

            // local transform
            Matrix4F toworld = new Matrix4F(localToWorld);

            // Offset by pivot
            Matrix4F P = new Matrix4F();
            P.Translation = node.Pivot;
            toworld.Mul(P, toworld);

            // Normalize            
            toworld.Normalize(toworld);

            return toworld;
        }

        private Color m_highlightColor;
        private Vec3F[] m_pivotOffset;
        private Color[] m_axisColor;
        private HitRegion m_hitRegion = HitRegion.None;        
        private bool m_isUniformScaling;                
        private Vec3F[] m_originalScales;
        private Vec3F[] m_originalTranslations;        
        private float m_hitScale;
        private Vec3F m_scale;                
        
        private enum HitRegion
        {            
            XAxis,
            YAxis,
            ZAxis,
            NegXAxis,
            NegYAxis,
            NegZAxis,
            None
        }
    }
}
