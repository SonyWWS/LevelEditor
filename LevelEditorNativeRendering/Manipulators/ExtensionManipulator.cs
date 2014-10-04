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
            m_axisColor[(int)HitRegion.XAxis] = Color.Red;
            m_axisColor[(int)HitRegion.YAxis] = Color.Green;
            m_axisColor[(int)HitRegion.ZAxis] = Color.Blue;
            m_axisColor[(int)HitRegion.NegXAxis] = ControlPaint.LightLight(Color.Red);
            m_axisColor[(int)HitRegion.NegYAxis] = ControlPaint.LightLight(Color.Green);
            m_axisColor[(int)HitRegion.NegZAxis] = ControlPaint.LightLight(Color.Blue);
            m_highlightColor = Color.Gold;

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
            

            Vec3F min = new Vec3F(-0.5f, -0.5f, -0.5f);
            Vec3F max = new Vec3F(0.5f, 0.5f, 0.5f);
            AABB box = new AABB(min, max);
            Matrix4F boxScale = new Matrix4F();
            Matrix4F boxTrans = new Matrix4F();
            Matrix4F BoxMtrx = new Matrix4F();

            float handleScale = s * HandleRatio;

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
            BasicRendererFlags solid = BasicRendererFlags.Solid
                | BasicRendererFlags.DisableDepthTest;
            BasicRendererFlags wire = BasicRendererFlags.WireFrame
                               | BasicRendererFlags.DisableDepthTest;

            Matrix4F normWorld = GetManipulatorMatrix();
            if (normWorld == null) return;
            Camera camera = vc.Camera;
                        
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

            Vec3F deltaTrans = Vec3F.ZeroVector;            
            if (m_hitRegion != HitRegion.None)
            {
                normWorld.Translation = HitMatrix.Translation;
                    
            }

            Vec3F pos = normWorld.Translation;
            float s;
            Util.CalcAxisLengths(vc.Camera, pos, out s);

            Vec3F sv = new Vec3F(s, s, s);
            Vec3F axscale = new Vec3F(s, s, s);
            Vec3F negAxscale = new Vec3F(-s, -s, -s);
            bool negativeAxis = m_hitRegion == HitRegion.NegXAxis || m_hitRegion == HitRegion.NegYAxis || m_hitRegion == HitRegion.NegZAxis;
            if (negativeAxis)
            {
                negAxscale.X *= Math.Abs(m_scale.X);
                negAxscale.Y *= Math.Abs(m_scale.Y);
                negAxscale.Z *= Math.Abs(m_scale.Z);
            }
            else
            {
                axscale.X *= Math.Abs(m_scale.X);
                axscale.Y *= Math.Abs(m_scale.Y);
                axscale.Z *= Math.Abs(m_scale.Z);                
            }
            
            Matrix4F scale = new Matrix4F();
            scale.Scale(axscale);
            Matrix4F xform = scale * normWorld;
            Util3D.RenderFlag = wire;

            Util3D.DrawX(xform, xcolor);
            Util3D.DrawY(xform, ycolor);
            Util3D.DrawZ(xform, zcolor);

            scale.Scale(negAxscale);
            xform = scale * normWorld;

            Util3D.DrawX(xform, nxcolor);
            Util3D.DrawY(xform, nycolor);
            Util3D.DrawZ(xform, nzcolor);

            Vec3F handle = sv*HandleRatio;
            float handleWidth = handle.X/2;
            scale.Scale(handle);
            Matrix4F trans = new Matrix4F();

            

            Util3D.RenderFlag = solid;

            // X handle
            trans.Translation = new Vec3F(axscale.X - handleWidth, 0, 0);
            xform = scale * trans * normWorld;
            Util3D.DrawCube(xform, xcolor);

            // y handle
            trans.Translation = new Vec3F(0, axscale.Y - handleWidth, 0);
            xform = scale * trans * normWorld;
            Util3D.DrawCube(xform, ycolor);

            // z handle
            trans.Translation = new Vec3F(0, 0, axscale.Z - handleWidth);
            xform = scale * trans * normWorld;
            Util3D.DrawCube(xform, zcolor);


            // -x handle
            trans.Translation = new Vec3F(negAxscale.X + handleWidth, 0, 0);
            xform = scale * trans * normWorld;
            Util3D.DrawCube(xform, nxcolor);

            // -y handle
            trans.Translation = new Vec3F(0, negAxscale.Y + handleWidth, 0);
            xform = scale * trans * normWorld;
            Util3D.DrawCube(xform, nycolor);

            // -z handle
            trans.Translation = new Vec3F(0, 0, negAxscale.Z + handleWidth);
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
        private const float HandleRatio = 1.0f / 8.0f;
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
