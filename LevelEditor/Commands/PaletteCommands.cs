//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using LevelEditorCore;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace LevelEditor.Commands
{
    [Export(typeof(PaletteCommands))]
    [Export(typeof(IInitializable))]
    [Export(typeof(IContextMenuCommandProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PaletteCommands : ICommandClient, IInitializable, IContextMenuCommandProvider
    {
        /// <summary>
        /// MEF importing constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="paletteService">Palette service</param>
        [ImportingConstructor]
        public PaletteCommands(
            ICommandService commandService,
            IContextRegistry contextRegistry,
            PaletteService paletteService)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
            m_paletteService = paletteService;
        }

        private enum Command
        {
            SelectAllInstances,
            ShowAllInstances,
            HideAllInstances,
        }

        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(                
                Command.SelectAllInstances,
                StandardMenu.Edit,
                StandardCommandGroup.EditSelectAll,
                Localizer.Localize("Select all instances"),
                Localizer.Localize("Selects all instances of the selected type"),
                Keys.None,
                null,
                CommandVisibility.ContextMenu,
                this);

            m_commandService.RegisterCommand(                
                Command.ShowAllInstances,
                StandardMenu.View,
                StandardCommandGroup.ViewShow,
                Localizer.Localize("Show all instances"),
                Localizer.Localize("Show all instances of the selected type"),
                Keys.None,
                null,
                CommandVisibility.ContextMenu,
                this);

            m_commandService.RegisterCommand(                
                Command.HideAllInstances,
                StandardMenu.View,
                StandardCommandGroup.ViewShow,
                Localizer.Localize("Hide all instances"),
                Localizer.Localize("Hide all instances of the selected type"),
                Keys.None,
                null,
                CommandVisibility.ContextMenu,
                this);
        }

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object target)
        {            
            ICommandClient cmdclient = (ICommandClient) this;

            if( context == m_paletteService.TreeView)
            {
                foreach (Command command in Enum.GetValues(typeof(Command)))
                {
                    if (cmdclient.CanDoCommand(command))
                    {                        
                        yield return command;
                    }
                }                    
            }            
        }

        #endregion

        #region ICommandClient Members

        bool ICommandClient.CanDoCommand(object commandTag)
        {
            if (!m_paletteService.TreeControl.Focused)
                return false;

            switch ((Command)commandTag)
            {
                case Command.SelectAllInstances:
                    return SelectionContext != null
                        && GetRootNode(SelectionContext) != null
                        && SelectedNodeType != null;

                case Command.ShowAllInstances:
                case Command.HideAllInstances:
                    return VisibilityContext != null
                        && GetRootNode(VisibilityContext) != null
                        && SelectedNodeType != null;
            }

            return false;
        }

        void ICommandClient.DoCommand(object commandTag)
        {
            switch ((Command)commandTag)
            {
                case Command.SelectAllInstances:
                    SelectAllInstances();
                    break;
                case Command.ShowAllInstances:
                    SetAllVisible(true);
                    break;
                case Command.HideAllInstances:
                    SetAllVisible(false);
                    break;
            }
            m_designView.InvalidateViews();
        }

        void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion

        private void SelectAllInstances()
        {
            DomNodeType domNodeType = SelectedNodeType;
            if (domNodeType == null)
                return;

            ISelectionContext selectionContext = SelectionContext;
            if (selectionContext == null)
                return;

            DomNode rootNode = GetRootNode(selectionContext);
            if (rootNode == null)
                return;


            selectionContext.Clear();
            List<Path<object>> newSelection = new List<Path<object>>();
            foreach (DomNode domNode in GetNodesOfType(rootNode, domNodeType))
                newSelection.Add(Util.AdaptDomPath(domNode));
            if (newSelection.Count > 0)
                selectionContext.SetRange(newSelection);
        }

        private void SetAllVisible(bool visible)
        {
            DomNodeType domNodeType = SelectedNodeType;
            if (domNodeType == null)
                return;

            IVisibilityContext visibilityContext = VisibilityContext;
            if (visibilityContext == null)
                return;

            DomNode rootNode = GetRootNode(visibilityContext);
            if (rootNode == null)
                return;

            foreach (DomNode domNode in GetNodesOfType(rootNode, domNodeType))
                visibilityContext.SetVisible(domNode, visible);
        }

        private DomNodeType SelectedNodeType
        {
            get
            {
                object lastHit = m_paletteService.TreeControlAdapter.LastHit;

                DomNodeType domNodeType = Adapters.As<DomNodeType>(lastHit);
                if (domNodeType != null)
                    return domNodeType;

                NodeTypePaletteItem item = Adapters.As<NodeTypePaletteItem>(lastHit);
                if (item != null && item.NodeType != null)
                    return item.NodeType;

                return null;
            }
        }

        private ISelectionContext SelectionContext
        {
            get { return m_contextRegistry.GetActiveContext<ISelectionContext>(); }
        }

        private IVisibilityContext VisibilityContext
        {
            get { return m_contextRegistry.GetActiveContext<IVisibilityContext>(); }
        }

        private DomNode GetRootNode(object context)
        {
            DomNode domNode = Adapters.As<DomNode>(context);
            if (domNode != null)
                return domNode.GetRoot();
            return null;
        }

        private IEnumerable<DomNode> GetNodesOfType(DomNode rootNode, DomNodeType domNodeType)
        {
            foreach (DomNode domNode in rootNode.Subtree)
                if (domNodeType.IsAssignableFrom(domNode.Type))
                    yield return domNode;
        }

      
        [Import(AllowDefault = false)]
        private IDesignView m_designView = null;

        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;
        private PaletteService m_paletteService;
    }
}
