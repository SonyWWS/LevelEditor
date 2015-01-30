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

        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(                
                Command.SelectAllInstances,
                StandardMenu.Edit,
                StandardCommandGroup.EditSelectAll,
                "Select all instances".Localize(),
                "Selects all instances of the selected type".Localize(),
                Keys.None,
                null,
                CommandVisibility.ContextMenu,
                this);

            m_commandService.RegisterCommand(                
                Command.ShowAllInstances,
                StandardMenu.View,
                StandardCommandGroup.ViewShow,
                "Show all instances".Localize(),
                "Show all instances of the selected type".Localize(),
                Keys.None,
                null,
                CommandVisibility.ContextMenu,
                this);

            m_commandService.RegisterCommand(                
                Command.HideAllInstances,
                StandardMenu.View,
                StandardCommandGroup.ViewShow,
                "Hide all instances".Localize(),
                "Hide all instances of the selected type".Localize(),
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
            if (context == m_paletteService.TreeView && target.Is<DomNodeType>())                
            {
                foreach (Command command in Enum.GetValues(typeof(Command)))
                    yield return command;                
            }            
        }

        #endregion

        #region ICommandClient Members

        bool ICommandClient.CanDoCommand(object commandTag)
        {            
            switch ((Command)commandTag)
            {
                case Command.SelectAllInstances:
                    return GetSelectionContext() != null;                       
                case Command.ShowAllInstances:
                case Command.HideAllInstances:
                    return GetVisibilityContext() != null;
            }
            return false;
        }

        void ICommandClient.DoCommand(object commandTag)
        {

            var nodeType = (DomNodeType)m_paletteService.TreeControlAdapter.LastHit;
            switch ((Command)commandTag)
            {
                case Command.SelectAllInstances:
                    SelectAllInstances(nodeType);
                    break;
                case Command.ShowAllInstances:
                    SetAllVisible(nodeType, true);
                    break;
                case Command.HideAllInstances:
                    SetAllVisible(nodeType, false);
                    break;
            }
            m_designView.InvalidateViews();
        }

        void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion

        private void SelectAllInstances(DomNodeType nodeType)
        {
            ISelectionContext sc = GetSelectionContext();
            if (sc != null)
            {
                List<object> nodes = new List<object>();
                foreach (DomNode node in Util.FindAll(nodeType, true))
                {
                    nodes.Add(Util.AdaptDomPath(node));
                }
                if (nodes.Count > 0)
                    sc.SetRange(nodes);
                else
                    sc.Clear();
            }           
        }

        private void SetAllVisible(DomNodeType nodeType, bool visible)
        {
            IVisibilityContext vc = GetVisibilityContext();
            if (vc != null)            
                foreach (DomNode node in Util.FindAll(nodeType, true))
                    vc.SetVisible(node, visible);
        }

        private ISelectionContext GetSelectionContext()
        {
            object context = m_contextRegistry.GetActiveContext<IGameContext>();
            return context.As<ISelectionContext>();
        }

        private IVisibilityContext GetVisibilityContext()
        {
            object context = m_contextRegistry.GetActiveContext<IGameContext>();
            return context.As<IVisibilityContext>();
        }
              
        [Import(AllowDefault = false)]
        private IDesignView m_designView = null;

        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;
        private PaletteService m_paletteService;

        private enum Command
        {
            SelectAllInstances,
            ShowAllInstances,
            HideAllInstances,
        }
    }
}
