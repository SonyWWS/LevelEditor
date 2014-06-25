//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using Sce.Atf.Dom;


using LevelEditorCore;

namespace LevelEditor.DomNodeAdapters
{
    /// <summary>
    /// control point 
    /// </summary>
    public class ControlPoint : GameObject, IControlPoint, IManipulatorNotify
    {
        protected override void OnNodeSet()
        {
            base.OnNodeSet();            
            TransformationType = TransformationTypes.Translation;
            DomNode.AttributeChanged += DomNode_AttributeChanged;
            
        }

        #region IManipulatorNotify Members

        void IManipulatorNotify.OnBeginDrag()
        {
            m_manipulating = true;            
        }

        void IManipulatorNotify.OnEndDrag()
        {            
            Curve curve = GetParentAs<Curve>();
            if (curve != null)
                curve.ComputeTranslation();
            m_manipulating = false;
        }        
        #endregion

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            if (!m_manipulating && e.AttributeInfo.Equivalent(Schema.controlPointType.translateAttribute))
            {
                Curve curve = GetParentAs<Curve>();
                if (curve != null)
                    curve.ComputeTranslation();
            }
        }
        private bool m_manipulating;
    }
}
