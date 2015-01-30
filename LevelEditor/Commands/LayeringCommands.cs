//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using LevelEditorCore;

using LayerLister = LevelEditorCore.LayerLister;

namespace LevelEditor.Commands
{
    /// <summary>
    /// Component to add "Add Layer" command to app. Command is accessible only
    /// by right click (context menu).</summary>
    [Export(typeof(IInitializable))]   
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class LayeringCommands :  IInitializable
    {        
        public LayeringCommands()
        {
            m_contextMenuStrip = new ContextMenuStrip();
            m_contextMenuStrip.AutoClose = true;
            
            m_addLayer = new ToolStripMenuItem("New Layer".Localize());
            m_addLayer.ToolTipText = "Creates a new layer folder".Localize();
            m_addLayer.Click += (sender, e) => AddNewLayer();
            
            m_deleteLayer = new ToolStripMenuItem("Delete".Localize());
            m_deleteLayer.Click += (sender, e) => Delete();
            m_deleteLayer.ShortcutKeys = Keys.Delete;
            m_deleteLayer.ShortcutKeyDisplayString = KeysUtil.KeysToString(Keys.Delete, true);
            m_deleteLayer.Image = ResourceUtil.GetImage16(CommandInfo.EditDelete.ImageName);

            m_contextMenuStrip.Items.Add(m_addLayer);
            m_contextMenuStrip.Items.Add(m_deleteLayer);
        }

        
        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_layerLister.TreeControl.MouseUp += TreeControl_MouseUp;            
            m_commandService.ProcessingKey += (sender, e) =>
                {
                    if (e.KeyData == Sce.Atf.Input.Keys.Delete
                        && m_controlRegistry.ActiveControl == m_layerLister.ControlInfo)
                    {
                        e.Handled = Delete();                        
                    }
                };
        }

        #endregion

        private void TreeControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {            
            if (e.Button == MouseButtons.Right)
            {
                var instancingContext = m_layerLister.TreeView.As<IInstancingContext>();
                m_deleteLayer.Enabled = instancingContext.CanDelete();
                m_deleteLayer.Visible = instancingContext.CanDelete();
                SkinService.ApplyActiveSkin(m_contextMenuStrip);
                m_contextMenuStrip.Show(m_layerLister.TreeControl, e.X, e.Y);                
            }
        }

        /// <summary>
        /// Delete selected objects</summary>
        private bool Delete()
        {
            var instancingContext = m_layerLister.TreeView.As<IInstancingContext>();
            if (instancingContext.CanDelete())
            {
                var transactionContext = m_layerLister.TreeView.As<ITransactionContext>();
                transactionContext.DoTransaction(
                        delegate
                        {
                            instancingContext.Delete();
                            ISelectionContext selectionContext = m_layerLister.TreeView.As<ISelectionContext>();
                            if (selectionContext != null)
                                selectionContext.Clear();

                        },
                        m_deleteLayer.Text);
                return true;
            }
            return false;
        }

        private void AddNewLayer()
        {
            object lastHit = m_layerLister.LastHit;
            ILayer newLayer = new DomNode(Schema.layerType.Type).As<ILayer>();
            newLayer.Name = "New Layer".Localize();

            IList<ILayer> layerList = null;
            var layer = lastHit.As<ILayer>();
            if (layer != null)
            {
                layerList = layer.Layers;
            }
            else
            {
                LayeringContext layeringContext = m_layerLister.TreeView.As<LayeringContext>();
                    if (layeringContext != null)
                        layerList = layeringContext.Layers;
            }                                
            if (layerList != null)
            {
                var transactionContext = m_layerLister.TreeView.As<ITransactionContext>();
                transactionContext.DoTransaction(
                    delegate
                    {
                        layerList.Add(newLayer);
                    },
                    m_addLayer.Text);
            }
        }

        [Import(AllowDefault = false)]
        private LayerLister m_layerLister;

        [Import(AllowDefault = false)]
        private IControlRegistry m_controlRegistry;

        [Import(AllowDefault = false)]
        private ICommandService m_commandService;

        private ContextMenuStrip m_contextMenuStrip;
        private ToolStripMenuItem m_addLayer;
        private ToolStripMenuItem m_deleteLayer;
    }
}
