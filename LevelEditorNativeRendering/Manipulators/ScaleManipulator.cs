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
            float s;
            Util.CalcAxisLengths(camera, HitMatrix.Translation, out s);
                        
            Matrix4F vp = camera.ViewMatrix * camera.ProjectionMatrix;
            Matrix4F wvp = HitMatrix * vp;
            
            // get ray in object space  space.
            Ray3F rayL = vc.GetRay(scrPt, wvp);
            
            m_scale = new Vec3F(1, 1, 1);            
            m_hitScale = s;
                                               
            float rectScale = s*FreeRectRatio;                                               
            Vec3F topRight    = rectScale * (new Vec3F(1, 1, 0));
            Vec3F topLeft     = rectScale * (new Vec3F(-1, 1, 0));
            Vec3F bottomLeft  = rectScale * (new Vec3F(-1, -1, 0));
            Vec3F bottomRight = rectScale * (new Vec3F(1, -1, 0));
            Matrix4F planeXform = Util.CreateBillboard(HitMatrix.Translation, camera.WorldEye, camera.Up, camera.LookAt);
            Matrix4F wvpPlane = planeXform * vp;

            // create ray in plane's local space.
            Ray3F rayP = vc.GetRay(scrPt, wvpPlane);

            Plane3F plane = new Plane3F(topRight,topLeft,bottomLeft);
            Vec3F p;

            bool intersect = rayP.IntersectPlane(plane, out p);            
            if(intersect)
            {                
                bool inside = p.X > topLeft.X
                              && p.X < topRight.X
                              && p.Y > bottomLeft.Y
                              && p.Y < topLeft.Y;
                if (inside)
                {
                    m_hitRegion = HitRegion.FreeRect;
                    return true;
                }                    
            }

            Vec3F min = new Vec3F(-0.5f, -0.5f, -0.5f);
            Vec3F max = new Vec3F(0.5f, 0.5f, 0.5f);
            AABB box = new AABB(min, max);
            Matrix4F boxScale = new Matrix4F();
            Matrix4F boxTrans = new Matrix4F();
            Matrix4F BoxMtrx = new Matrix4F();

            float handleScale = s * HandleRatio;
            // X axis

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
            BasicRendererFlags solid = BasicRendererFlags.Solid
                | BasicRendererFlags.DisableDepthTest;
            BasicRendererFlags wire = BasicRendererFlags.WireFrame
                               | BasicRendererFlags.DisableDepthTest;

                       
            Matrix4F normWorld = GetManipulatorMatrix();
            if (normWorld == null) return;

            Camera camera = vc.Camera;
            Vec3F pos = normWorld.Translation;
            float s;
            Util.CalcAxisLengths(vc.Camera, pos, out s);

            Vec3F sv = new Vec3F(s, s, s);
            Vec3F axscale = new Vec3F( Math.Abs(s*m_scale.X), Math.Abs(s*m_scale.Y), Math.Abs(s*m_scale.Z));

            Color xcolor = (m_hitRegion == HitRegion.XAxis || m_hitRegion == HitRegion.FreeRect ) ? Color.Gold : Color.Red;
            Color ycolor = (m_hitRegion == HitRegion.YAxis || m_hitRegion == HitRegion.FreeRect ) ? Color.Gold : Color.Green;
            Color Zcolor = (m_hitRegion == HitRegion.ZAxis || m_hitRegion == HitRegion.FreeRect ) ? Color.Gold : Color.Blue;
            Color freeRect = (m_hitRegion == HitRegion.FreeRect) ? Color.Gold : Color.White;


            Matrix4F scale = new Matrix4F();
            scale.Scale(axscale);
            Matrix4F xform = scale * normWorld;
            Util3D.RenderFlag = wire;
            Util3D.DrawX(xform, xcolor);
            Util3D.DrawY(xform, ycolor);
            Util3D.DrawZ(xform, Zcolor);

            Vec3F rectScale = sv*FreeRectRatio;
            scale.Scale(rectScale);
            Matrix4F b = Util.CreateBillboard(pos, camera.WorldEye, camera.Up, camera.LookAt);
            Matrix4F recXform = Matrix4F.Multiply(scale, b);
            Util3D.DrawRect(recXform, freeRect);

            Vec3F handle = sv*HandleRatio;
            float handleWidth = handle.X/2;
            scale.Scale(handle);
            Matrix4F trans = new Matrix4F();
            trans.Translation = new Vec3F(axscale.X - handleWidth, 0, 0);
            xform = scale * trans * normWorld;

            Util3D.RenderFlag = solid;

            Util3D.DrawCube(xform, xcolor);

            trans.Translation = new Vec3F(0, axscale.Y - handleWidth, 0);
            xform = scale * trans * normWorld;
            Util3D.DrawCube(xform, ycolor);

            trans.Translation = new Vec3F(0, 0, axscale.Z - handleWidth);
            xform = scale * trans * normWorld;
            Util3D.DrawCube(xform, Zcolor);
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
                case HitRegion.FreeRect:
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
        private const float FreeRectRatio = 1.0f / 7.0f;
        private const float HandleRatio = 1.0f / 8.0f;
        private enum HitRegion
        {
            None,
            XAxis,
            YAxis,
            ZAxis,
            FreeRect,
        }
    }
}
