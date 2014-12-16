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
    public class ScaleManipulator : Manipulator
    {
        public ScaleManipulator()
        {
            ManipulatorInfo = new ManipulatorInfo("Scale".Localize(),
                                         "Scale manipulator".Localize(),
                                         LevelEditorCore.Resources.ScaleImage,
                                         Keys.R);
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

            float centerCubeScale = s * CenterCubeSize;
            Matrix4F centerCubeXform = new Matrix4F();
            centerCubeXform.Scale(centerCubeScale);
            centerCubeXform.Invert(centerCubeXform);
            Ray3F ray = rayL;
            ray.Transform(centerCubeXform);
            if (box.Intersect(ray))
            {
                m_hitRegion = HitRegion.CenterCube;
                return true;
            }

            Matrix4F boxScale = new Matrix4F();
            Matrix4F boxTrans = new Matrix4F();
            Matrix4F BoxMtrx = new Matrix4F();

            float handleScale = s * AxisHandle;
            // X axis

            boxScale.Scale(new Vec3F(s, handleScale, handleScale));
            boxTrans.Translation = new Vec3F(s / 2, 0, 0);
            BoxMtrx = boxScale * boxTrans;
            ray = rayL;
            BoxMtrx.Invert(BoxMtrx);
            ray.Transform(BoxMtrx);

            if (box.Intersect(ray))
            {
                m_hitRegion = HitRegion.XAxis;
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

            return false;
        }

        public override void Render(ViewControl vc)
        {
                                               
            Matrix4F normWorld = GetManipulatorMatrix();
            if (normWorld == null) return;

            
            Vec3F pos = normWorld.Translation;
            float s = Util.CalcAxisScale(vc.Camera, pos, AxisLength, vc.Height);
            
            Color xcolor = (m_hitRegion == HitRegion.XAxis || m_hitRegion == HitRegion.CenterCube ) ? Color.Gold : XAxisColor;
            Color ycolor = (m_hitRegion == HitRegion.YAxis || m_hitRegion == HitRegion.CenterCube ) ? Color.Gold : YAxisColor;
            Color zcolor = (m_hitRegion == HitRegion.ZAxis || m_hitRegion == HitRegion.CenterCube ) ? Color.Gold : ZAxisColor;
            Color centerCubeColor = (m_hitRegion == HitRegion.CenterCube) ? Color.Gold : Color.White;

           
            Vec3F sv = new Vec3F(s, s, s);
            Vec3F axscale = new Vec3F(s * AxisThickness, s, s * AxisThickness);

            Matrix4F scale = new Matrix4F();
            axscale.Y = Math.Abs(s * m_scale.X);
            scale.Scale(axscale);
            Matrix4F rot = new Matrix4F();
            rot.RotZ(-MathHelper.PiOver2);
            Matrix4F xform = scale * rot * normWorld;           
            Util3D.DrawCylinder(xform, xcolor);

            axscale.Y = Math.Abs(s * m_scale.Y);
            scale.Scale(axscale);
            xform = scale * normWorld;           
            Util3D.DrawCylinder(xform, ycolor);
            rot.RotX(MathHelper.PiOver2);
            axscale.Y = Math.Abs(s * m_scale.Z);
            scale.Scale(axscale);
            xform = scale * rot * normWorld;
            Util3D.DrawCylinder(xform, zcolor);
            
            Vec3F centerCubeScale = sv * CenterCubeSize;
            scale.Scale(centerCubeScale);            
            Matrix4F centerCubeXform = scale * normWorld;
            Util3D.DrawCube(centerCubeXform, centerCubeColor);


            Vec3F handleScale = new Vec3F(Math.Abs(s * m_scale.X), Math.Abs(s * m_scale.Y), Math.Abs(s * m_scale.Z));
            Vec3F handle = sv * AxisHandle;
            float handleWidth = handle.X / 2;
            scale.Scale(handle);
            Matrix4F trans = new Matrix4F();
            trans.Translation = new Vec3F(handleScale.X - handleWidth, 0, 0);
            xform = scale * trans * normWorld;

            Util3D.DrawCube(xform, xcolor);

            trans.Translation = new Vec3F(0, handleScale.Y - handleWidth, 0);
            xform = scale * trans * normWorld;
            Util3D.DrawCube(xform, ycolor);

            trans.Translation = new Vec3F(0, 0, handleScale.Z - handleWidth);
            xform = scale * trans * normWorld;
            Util3D.DrawCube(xform, zcolor);
        }

        public override void OnBeginDrag()
        {
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
            

            m_originalValues = new Vec3F[NodeList.Count];
            int k = 0;
            foreach (ITransformable node in NodeList)
            {
                m_originalValues[k++] = node.Scale;
            }

            if(NodeList.Count > 0)
                transactionContext.Begin("Scale".Localize());                
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
            Ray3F rayV = vc.GetRay(scrPt,vc.Camera.ProjectionMatrix);
            

            Vec3F xAxis = wv.XAxis;
            Vec3F yAxis = wv.YAxis;
            Vec3F zAxis = wv.ZAxis;
            Vec3F origin = wv.Translation;

            //Vec3F pos;
            m_scale = new Vec3F(1, 1, 1);
            float scale = 1;
            float a1, a2;
                  
            switch (m_hitRegion)
            {
                case HitRegion.XAxis:
                    {
                        a1 = Math.Abs(Vec3F.Dot(HitRayV.Direction, yAxis));
                        a2 = Math.Abs(Vec3F.Dot(HitRayV.Direction, zAxis));
                        Vec3F axis = (a1 > a2 ? yAxis : zAxis);
                        Vec3F p0 = HitRayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        Vec3F p1 = rayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        float dragAmount = Vec3F.Dot((p1 - p0), xAxis);                                                
                        m_scale.X = 1.0f + dragAmount / m_hitScale;
                        scale = m_scale.X;
                    }

                    break;
                case HitRegion.YAxis:
                    {
                        a1 = Math.Abs(Vec3F.Dot(HitRayV.Direction, zAxis));
                        a2 = Math.Abs(Vec3F.Dot(HitRayV.Direction, xAxis));
                        Vec3F axis = (a1 > a2 ? zAxis : xAxis);
                        Vec3F p0 = HitRayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        Vec3F p1 = rayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        float dragAmount = Vec3F.Dot((p1 - p0), yAxis);
                        m_scale.Y = 1.0f + dragAmount / m_hitScale;
                        scale = m_scale.Y;
                    }
                    break;
                case HitRegion.ZAxis:
                    {
                        a1 = Math.Abs(Vec3F.Dot(HitRayV.Direction, xAxis));
                        a2 = Math.Abs(Vec3F.Dot(HitRayV.Direction, yAxis));
                        Vec3F axis = (a1 > a2 ? xAxis : yAxis);
                        Vec3F p0 = HitRayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        Vec3F p1 = rayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        float dragAmount = Vec3F.Dot((p1 - p0), zAxis);
                        m_scale.Z = 1.0f + dragAmount / m_hitScale;
                        scale = m_scale.Z;
                    }
                    break;
                case HitRegion.CenterCube:
                    {

                        Vec3F axis = new Vec3F(0, 0, 1);
                        Vec3F p0 = HitRayV.IntersectPlane(axis, -origin.Z);
                        Vec3F p1 = rayV.IntersectPlane(axis, -origin.Z);
                        Vec3F dragVec = p1 - p0;

                        float dragAmount = 1.0f + dragVec.X / m_hitScale;
                        m_scale.X = dragAmount ;
                        m_scale.Y = dragAmount ;
                        m_scale.Z = dragAmount ;
                        scale = m_scale.X;
                   
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
                ITransformable transformable = NodeList[i];
                transformable.Scale = Vec3F.Mul(m_originalValues[i], m_scale);
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
            m_originalValues = null;
            m_hitRegion = HitRegion.None;
            m_scale = new Vec3F(1, 1, 1);
        }

        private HitRegion m_hitRegion = HitRegion.None;

        
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

                
        private bool m_isUniformScaling;        
        private Vec3F[] m_originalValues;        
        private float m_hitScale;
        private Vec3F m_scale;        
        private const float CenterCubeSize = 1.0f / 6.0f;        
        private enum HitRegion
        {
            None,
            XAxis,
            YAxis,
            ZAxis,
            CenterCube,
        }
    }
}
