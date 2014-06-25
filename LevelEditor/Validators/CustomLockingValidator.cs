//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace LevelEditor
{
    /// <summary>
    /// Allows setting visibility when the object is locked.</summary>
    public class CustomLockingValidator : LockingValidator
    {
        /// <summary>
        /// Performs initialization when the adapter's node is set</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            m_visibilityContext = this.Cast<IVisibilityContext>();
        }

        protected override void OnAttributeChanging(object sender, AttributeEventArgs e)
        {
            if (Validating)
            {
                m_visible = m_visibilityContext.IsVisible(e.DomNode);
            }

            base.OnAttributeChanging(sender, e);
        }

        protected override void OnAttributeChanged(object sender, AttributeEventArgs e)
        {
            if (Validating)
            {
                bool visible = m_visibilityContext.IsVisible(e.DomNode);
                if (m_visible == visible)
                {
                    base.OnAttributeChanged(sender, e);
                }            
            }
            else
            {
                base.OnAttributeChanged(sender, e);
            }

            
        }
        private IVisibilityContext m_visibilityContext;
        private bool m_visible;
    }
}
