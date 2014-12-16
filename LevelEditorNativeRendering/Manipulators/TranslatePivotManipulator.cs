//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

using LevelEditorCore;

using Camera = Sce.Atf.Rendering.Camera;

namespace RenderingInterop
{
    using HitRegion = TranslatorControl.HitRegion;

    [Export(typeof(IManipulator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TranslatePivotManipulator : Manipulator
    {
        public TranslatePivotManipulator()
        {
            ManipulatorInfo = new ManipulatorInfo("MovePivot".Localize(),
                                         "Activate move-pivot manipulator".Localize(),
                                         LevelEditorCore.Resources.TranslatePivotImage,
                                         Keys.P);
            m_translatorControl = new TranslatorControl();
        }

        #region Implementation of IManipulator

        public override bool Pick(ViewControl vc, Point scrPt)
        {
            m_hitRegion = HitRegion.None;
            if (base.Pick(vc, scrPt) == false)
                return false;

            m_node = GetManipulatorNode(TransformationTypes.Pivot);

            Camera camera = vc.Camera;            
            Matrix4F view = camera.ViewMatrix;
            Matrix4F vp = view * camera.ProjectionMatrix;
            Matrix4F wvp = HitMatrix * vp;

            Ray3F rayL = vc.GetRay(scrPt, wvp);
            m_hitRegion = m_translatorControl.Pick(vc, HitMatrix, view, rayL, HitRayV);
            bool picked = m_hitRegion != HitRegion.None;
            return picked;
        }

        public override void Render(ViewControl vc)
        {
            Matrix4F normWorld = GetManipulatorMatrix();
            if (normWorld == null) return;            
            m_translatorControl.Render(vc, normWorld);                                
        }

        public override void OnBeginDrag()
        {
            if (m_hitRegion == HitRegion.None || !CanManipulate(m_node))
                return;                       
            var transactionContext = DesignView.Context.As<ITransactionContext>();           
            transactionContext.Begin("Move".Localize());
            m_originalPivot = m_node.Pivot;

            Path<DomNode> path = new Path<DomNode>(m_node.Cast<DomNode>().GetPath());
            Matrix4F localToWorld = TransformUtils.CalcPathTransform(path, path.Count - 1);
            m_worldToLocal = new Matrix4F();
            m_worldToLocal.Invert(localToWorld);
        }

        public override void OnDragging(ViewControl vc, Point scrPt)
        {
            if (m_hitRegion == HitRegion.None || !CanManipulate(m_node))
                return;

            Matrix4F proj = vc.Camera.ProjectionMatrix;
            // create ray in view space.            
            Ray3F rayV = vc.GetRay(scrPt, proj);
                             
            Vec3F translate = m_translatorControl.OnDragging(rayV);
                      
            Vec3F localTranslation;
            m_worldToLocal.TransformVector(translate, out localTranslation);
            m_node.Pivot = m_originalPivot + localTranslation;

        }

        public override void OnEndDrag(ViewControl vc, Point scrPt)
        {
            if (CanManipulate(m_node))
            {
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
            m_node = null;
        }

        #endregion
          
        
        private bool CanManipulate(ITransformable node)
        {
            bool result = node != null
                  && (node.TransformationType & TransformationTypes.Pivot) != 0
                  && node.Cast<IVisible>().Visible
                  && node.Cast<ILockable>().IsLocked == false;
            return result;
        }

        protected override Matrix4F GetManipulatorMatrix()
        {
            ITransformable node = GetManipulatorNode(TransformationTypes.Pivot);
            if (node == null) return null;

            Path<DomNode> path = new Path<DomNode>(node.Cast<DomNode>().GetPath());
            Matrix4F localToWorld = TransformUtils.CalcPathTransform(path, path.Count - 1);

            // Offset by pivot
            Matrix4F Pv = new Matrix4F();
            Pv.Set(node.Pivot);
            localToWorld.Mul(Pv, localToWorld);
            localToWorld.OrthoNormalize(localToWorld);
            return new Matrix4F(localToWorld.Translation);
        }
        
        private TranslatorControl m_translatorControl;
        private HitRegion m_hitRegion = HitRegion.None;
        private Matrix4F m_worldToLocal;
        private Vec3F m_originalPivot;
        private ITransformable m_node;
        
        
    }
}
