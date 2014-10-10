//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Drawing;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

using LevelEditorCore;
using Camera = Sce.Atf.Rendering.Camera;

namespace RenderingInterop
{

    [Export(typeof(IManipulator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RotateManipulator : Manipulator
    {

        public RotateManipulator()
        {
            ManipulatorInfo = new ManipulatorInfo("Rotate".Localize(),
                                         "Activate rotation manipulator".Localize(),
                                         LevelEditorCore.Resources.RotateImage,
                                         Keys.E);
        }


        public override bool Pick(ViewControl vc, Point scrPt)
        {
            m_hitRegion = HitRegion.None;
            if (base.Pick(vc, scrPt) == false)
                return false;

            Camera camera = vc.Camera;
            float rad;
            Util.CalcAxisLengths(camera, HitMatrix.Translation, out rad);
            float tolerance = rad / 10.0f;

            // compute ray in object space  space.
            Matrix4F vp = camera.ViewMatrix * camera.ProjectionMatrix;
            Matrix4F wvp = HitMatrix * vp;            
            Ray3F rayL = vc.GetRay(scrPt, wvp);

            Plane3F xplane = new Plane3F(Vec3F.XAxis, Vec3F.ZeroVector);
            Plane3F yplane = new Plane3F(Vec3F.YAxis,Vec3F.ZeroVector);
            Plane3F zplane = new Plane3F(Vec3F.ZAxis,Vec3F.ZeroVector);

            Vec3F pt;
            float xdelta = float.MaxValue;            
            float ydelta = float.MaxValue;
            float zdelta = float.MaxValue;
            if(rayL.IntersectPlane(xplane,out pt))
            {
                xdelta = Math.Abs(pt.Length - rad); 
            }

            if (rayL.IntersectPlane(yplane, out pt))
            {
                ydelta = Math.Abs(pt.Length - rad);              
            }

            if (rayL.IntersectPlane(zplane, out pt))
            {
                zdelta = Math.Abs(pt.Length - rad);
            }

            if(xdelta < tolerance && xdelta < ydelta && xdelta < zdelta)
            {
                m_hitRegion = HitRegion.XAxis;                
            }
            else if(ydelta < tolerance && ydelta < zdelta)
            {
                m_hitRegion = HitRegion.YAxis;
            }
            else if(zdelta < tolerance)
            {
                m_hitRegion = HitRegion.ZAxis;
            }

            return m_hitRegion != HitRegion.None;
        }


        public override void Render(ViewControl vc)
        {                                                                           
            Matrix4F normWorld = GetManipulatorMatrix();
            if (normWorld == null) return;

            Util3D.RenderFlag = BasicRendererFlags.WireFrame | BasicRendererFlags.DisableDepthTest;
            Color xcolor = (m_hitRegion == HitRegion.XAxis ) ? Color.LightSalmon : Color.Red;
            Color ycolor = (m_hitRegion == HitRegion.YAxis ) ? Color.LightGreen : Color.Green;
            Color Zcolor = (m_hitRegion == HitRegion.ZAxis ) ? Color.LightBlue : Color.Blue;

            float s;
            Util.CalcAxisLengths(vc.Camera, normWorld.Translation, out s);
            Vec3F axScale = new Vec3F(s, s, s);

            Matrix4F scale = new Matrix4F();
            scale.Scale(axScale);
            Matrix4F xform = scale*normWorld;
            Util3D.DrawCircle(xform, Zcolor);

            Matrix4F rot = new Matrix4F();
            rot.RotY(MathHelper.PiOver2);
            xform = scale * rot * normWorld;
            Util3D.DrawCircle(xform, xcolor);

            rot.RotX(MathHelper.PiOver2);
            xform = scale * rot * normWorld;
            Util3D.DrawCircle(xform,ycolor);

        }

        public override void OnBeginDrag()
        {
            if (m_hitRegion == HitRegion.None)
                return;
            var selectionCntx = DesignView.Context.As<ISelectionContext>();
            var selection = selectionCntx.Selection;
            var transactionContext = DesignView.Context.As<ITransactionContext>();

            NodeList.Clear();

            IEnumerable<DomNode> rootDomNodes = DomNode.GetRoots(selection.AsIEnumerable<DomNode>());
            foreach (DomNode domNode in rootDomNodes)
            {
                ITransformable node = domNode.As<ITransformable>();
                                
                if (node == null || (node.TransformationType & TransformationTypes.Rotation) == 0)
                    continue;

                IVisible vn = node.As<IVisible>();
                if (!vn.Visible) continue;

                ILockable lockable = node.As<ILockable>();
                if (lockable.IsLocked) continue;
                
                NodeList.Add(node);
                IManipulatorNotify notifier = node.As<IManipulatorNotify>();
                if (notifier != null) notifier.OnBeginDrag();
            }

            m_rotations = new Matrix4F[NodeList.Count];            
            for (int k = 0; k < NodeList.Count; k++)
            {
                ITransformable node = NodeList[k];                
                Matrix4F m = new Matrix4F(node.Transform);
                m.Translation = new Vec3F(0, 0, 0);
                m.Normalize(m);
                m_rotations[k] = m;
            }

            if(NodeList.Count > 0)
                transactionContext.Begin("Rotate".Localize());
        }

        public override void OnDragging(ViewControl vc, Point scrPt)
        {
            if (m_hitRegion == HitRegion.None || NodeList.Count == 0)
                return;
            Camera cam = vc.Camera;

            Matrix4F view = cam.ViewMatrix;
            Matrix4F mtrx = cam.ProjectionMatrix;

            Matrix4F axisMtrx = HitMatrix * view;
            Ray3F hitRay = HitRayV;
            Ray3F dragRay = vc.GetRay(scrPt, mtrx);
            
            Vec3F xAxis = axisMtrx.XAxis;
            Vec3F yAxis = axisMtrx.YAxis;
            Vec3F zAxis = axisMtrx.ZAxis;
            Vec3F origin = axisMtrx.Translation;
            
            Vec3F rotAxis = new Vec3F();            
            float theta = 0;

            float snapAngle = ((ISnapSettings)DesignView).SnapAngle;
            switch (m_hitRegion)
            {                
                case HitRegion.XAxis:
                    {
                        Plane3F xplane = new Plane3F(xAxis, origin);
                        theta = CalcAngle(origin, xplane, hitRay, dragRay, snapAngle);
                        rotAxis = HitMatrix.XAxis;                        
                    }
                    break;
                case HitRegion.YAxis:
                    {
                        Plane3F yplane = new Plane3F(yAxis, origin);
                        theta = CalcAngle(origin, yplane, hitRay, dragRay, snapAngle);
                        rotAxis = HitMatrix.YAxis;                        
                    }
                    break;
                case HitRegion.ZAxis:
                    {
                        Plane3F zplane = new Plane3F(zAxis, origin);
                        theta = CalcAngle(origin, zplane, hitRay, dragRay, snapAngle);
                        rotAxis = HitMatrix.ZAxis;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            AngleAxisF axf = new AngleAxisF(-theta, rotAxis);
            Matrix4F deltaMtrx = new Matrix4F(axf);                                   
            Matrix4F rotMtrx = new Matrix4F();
           
            for (int i = 0; i < NodeList.Count; i++)
            {                                
                ITransformable node = NodeList[i];                              
                rotMtrx.Mul(m_rotations[i], deltaMtrx);                
                float ax, ay, az;
                rotMtrx.GetEulerAngles(out ax, out ay, out az);                                
                node.Rotation = new Vec3F(ax, ay, az);      
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
            m_hitRegion = HitRegion.None;                        
            NodeList.Clear();            
            m_rotations = null;
        }
        
        private static float CalcAngle(Vec3F origin, Plane3F plane, Ray3F ray0, Ray3F ray1, float snapAngle)
        {
            float theta = 0;            
            Vec3F p0;
            Vec3F p1;

            if( ray0.IntersectPlane(plane, out p0)
                && ray1.IntersectPlane(plane, out p1))
            {
                Vec3F v0 = Vec3F.Normalize(p0 - origin);
                Vec3F v1 = Vec3F.Normalize(p1 - origin);
                theta = CalcAngle(v0, v1, plane.Normal, snapAngle);
            }
            return theta;
        }

        private static float CalcAngle(Vec3F v0, Vec3F v1, Vec3F axis, float snapAngle)
        {
            const float twoPi = (float)(2.0 * Math.PI);
            float angle = Vec3F.Dot(v0, v1);
            if (angle > 1.0f)
                angle = 1.0f;
            else if (angle <= -1.0f)
                angle = 1.0f;
            angle = (float)Math.Acos(angle);

            // Check if v0 is left of v1
            Vec3F cross = Vec3F.Cross(v0, v1);
            if (Vec3F.Dot(cross, axis) < 0)
                angle = twoPi - angle;
            // round to nearest multiple of preference
            if (snapAngle > 0)
            {
                if (angle >= 0)
                {
                    int n = (int)((angle + snapAngle * 0.5f) / snapAngle);
                    angle = n * snapAngle;
                }
                else
                {
                    int n = (int)((angle - snapAngle * 0.5f) / snapAngle);
                    angle = n * snapAngle;
                }
            }            
           // const float epsilone = (float)1e-7;
           // if (twoPi - angle <= epsilone) angle = 0.0f;          
            return angle;
        }

        
        protected override Matrix4F GetManipulatorMatrix()
        {
            ITransformable node = GetManipulatorNode(TransformationTypes.Rotation);
            if (node == null ) return null;
            
            Path<DomNode> path = new Path<DomNode>(node.Cast<DomNode>().GetPath());
            Matrix4F localToWorld = TransformUtils.CalcPathTransform(path, path.Count - 1);

            // local transform
            Matrix4F toworld = new Matrix4F(localToWorld);

            // Offset by rotate pivot
            Matrix4F P = new Matrix4F();
            P.Translation = node.Pivot;
            toworld.Mul(P, toworld);

            // Normalize            
            toworld.Normalize(toworld);

            return toworld;
        }

        private HitRegion m_hitRegion = HitRegion.None;        
        private Matrix4F[] m_rotations;        
        private enum HitRegion
        {
            None,
            XAxis,
            YAxis,
            ZAxis,          
        }
    }
}
