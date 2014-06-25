//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Linq;

using Sce.Atf;
using Sce.Atf.Applications;
using PropertyGrid = Sce.Atf.Controls.PropertyEditing.PropertyGrid;

namespace LevelEditorCore
{
    /// <summary>
    /// Component to edit resource meta-data.
    /// </summary>
    [Export(typeof(IInitializable))]    
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ResourceMetadataEditor : IInitializable
    {
        public ResourceMetadataEditor()
        {
            m_propertyGrid = new PropertyGrid();
            m_controlInfo = new ControlInfo(
                "Resource Metadata".Localize(),
                "Edits selected resource metadata".Localize(),
                StandardControlGroup.Hidden);
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            if (m_resourceLister == null || m_resourceMetadataService == null)
                return;

            m_resourceLister.SelectionChanged += resourceLister_SelectionChanged;
            m_controlHostService.RegisterControl(m_propertyGrid, m_controlInfo, null);

        }

        #endregion

        private void resourceLister_SelectionChanged(object sender, EventArgs e)
        {            
            Uri resUri = m_resourceLister.LastSelected;
            object[] mdatadata = m_resourceMetadataService.GetMetadata(m_resourceLister.Selection).ToArray();
            m_propertyGrid.Bind(mdatadata);            
        }

        [Import(AllowDefault = true)]
        private ResourceLister m_resourceLister = null;

        [Import(AllowDefault = true)]
        private IResourceMetadataService m_resourceMetadataService = null;

        [Import(AllowDefault = false)]
        private ControlHostService m_controlHostService = null;

        private readonly ControlInfo m_controlInfo;
        private readonly PropertyGrid m_propertyGrid;
    }
}
