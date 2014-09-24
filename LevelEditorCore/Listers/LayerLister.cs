//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls;

namespace LevelEditorCore
{
    /// <summary>
    /// Editor that presents an ILayeringContext using a TreeControl</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(LayerLister))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class LayerLister : TreeControlEditor, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service for opening right-click context menus</param>
        [ImportingConstructor]
        public LayerLister(ICommandService commandService)
            : base(commandService)
        {
            Configure(out m_controlInfo);
            TreeControl.NodeCheckStateEdited += treeControl_NodeCheckStateEdited;
        }

        /// <summary>
        /// Configures the LayerLister</summary>
        /// <param name="controlInfo">Information about the control for the hosting service</param>
        protected virtual void Configure(
            out ControlInfo controlInfo)
        {
            controlInfo = new ControlInfo(
                "Layers".Localize(),
                "Edits document layers".Localize(),
                StandardControlGroup.Hidden,
                s_layerImage);

            TreeControl.ShowRoot = false;
            TreeControl.AllowDrop = true;
            TreeControl.SelectionMode = SelectionMode.MultiExtended;
            TreeControl.Text = "Drag items from the Project Lister and drop them here to create layers whose visibility can be controlled by clicking on a check box.".Localize();
        }

        /// <summary>
        /// Gets the control info instance, which determines the appearance and
        /// initial location of the control in the application</summary>
        public ControlInfo ControlInfo
        {
            get { return m_controlInfo; }
        }

       
        /// <summary>
        /// Called when the underlying tree control raises the MouseUp event</summary>
        /// <param name="e">Event args from the tree control's MouseUp event</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            // disable base method.
            // Let laying command process mouse-up event.
            // It is not possible to identify LayringCommands
            // LayerLister must only operate on LayringCommands, not other 
            // IContextMenuCommandProviders           
        }

        [Import]
        private IControlHostService m_controlHostService;

        [Import]
        private IContextRegistry m_contextRegistry;

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
            m_controlHostService.RegisterControl(TreeControl, m_controlInfo, null);
        }

        #endregion

        /// <summary>
        /// Performs custom actions after the LastHitChanged event</summary>
        /// <param name="e">Event args</param>
        protected override void OnLastHitChanged(EventArgs e)
        {
            var layeringContext = TreeView.As<ILayeringContext>();
            layeringContext.SetActiveItem(LastHit);
            base.OnLastHitChanged(e);
        }

        private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            // Note: obtain ITreeView from ILayeringContext not directly from GetActiveContext().
            var context = m_contextRegistry.GetActiveContext<ILayeringContext>();
            var treeView = context.As<ITreeView>();

            // The TreeView property guards again setting same value
            // but it still reloads the context.            
            // so this check is required to prevent the tree to unnecessarily reload 
            // the context.
            if (TreeView != treeView)
                TreeView = treeView;
        }

        private void treeControl_NodeCheckStateEdited(object sender, TreeControl.NodeEventArgs e)
        {
            ShowLayer(e.Node.Tag, e.Node.CheckState == CheckState.Checked);
        }

        public void ShowLayer(object layer, bool show)
        {
            var layeringContext = TreeView.As<ILayeringContext>();
            var transactionContext = TreeView.As<ITransactionContext>();
            transactionContext.DoTransaction(delegate
            {
                layeringContext.SetVisible(layer, show);
            },
            "Show/Hide Layer".Localize());
        }
        
        private readonly ControlInfo m_controlInfo;
        private static readonly Image s_layerImage = ResourceUtil.GetImage16(Sce.Atf.Resources.LayerImage);
    }
}
