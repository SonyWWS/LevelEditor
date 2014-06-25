//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace LevelEditorCore.Commands
{
    [Export(typeof(IInitializable))]    
    [Export(typeof(IContextMenuCommandProvider))]    
    [Export(typeof(ShowCommands))]    
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShowCommands : IInitializable, ICommandClient, IContextMenuCommandProvider
    {

        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public ShowCommands(
            ICommandService commandService, 
            IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(CommandInfo.ViewHide, this);
            m_commandService.RegisterCommand(CommandInfo.ViewShow, this);
            m_commandService.RegisterCommand(CommandInfo.ViewShowLast, this);
            m_commandService.RegisterCommand(CommandInfo.ViewShowAll, this);
            m_commandService.RegisterCommand(CommandInfo.ViewIsolate, this);

            if (m_scriptingService != null)
                m_scriptingService.SetVariable("showCommands", this);
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks if the client can do the command</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True if client can do the command</returns>
        bool ICommandClient.CanDoCommand(object commandTag)
        {
            if (commandTag is StandardCommand)
            {
                ISelectionContext selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
                IEnumerableContext enumerableContext = m_contextRegistry.GetActiveContext<IEnumerableContext>();
                
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.ViewHide:
                        return (selectionContext != null && selectionContext.SelectionCount > 0);
                    case StandardCommand.ViewShow:
                        return (selectionContext != null && selectionContext.SelectionCount > 0);
                    case StandardCommand.ViewShowLast:
                        return (m_hideStack != null && m_hideStack.Count > 0);
                    case StandardCommand.ViewShowAll:
                        return (enumerableContext != null);
                    case StandardCommand.ViewIsolate:
                        return (selectionContext != null && selectionContext.SelectionCount > 0);
                }
            }

            return false;
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="commandTag">Command</param>
        void ICommandClient.DoCommand(object commandTag)
        {
            if (commandTag is StandardCommand)
            {
                ISelectionContext selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
                IEnumerableContext enumerableContext = m_contextRegistry.GetActiveContext<IEnumerableContext>();
                IVisibilityContext visibilityContext = m_contextRegistry.GetActiveContext<IVisibilityContext>();

                IEnumerable<object> selection = null;
                if (selectionContext != null)
                    selection = selectionContext.Selection;
                
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.ViewHide:
                        HideSelection(selection, visibilityContext);
                        break;
                    case StandardCommand.ViewShow:
                        ShowSelection(selection, visibilityContext);
                        break;
                    case StandardCommand.ViewShowLast:
                        ShowLastHidden(visibilityContext);
                        break;
                    case StandardCommand.ViewShowAll:
                        ShowAll(visibilityContext, enumerableContext);
                        break;
                    case StandardCommand.ViewIsolate:
                        IsolateSelection(selection, visibilityContext, enumerableContext);
                        break;
                }
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command state to update</param>
        void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion

        private void HideSelection(IEnumerable<object> selection, IVisibilityContext visibilityContext)
        {
            if (selection != null && visibilityContext != null)
            {
                List<WeakReference> hidden = new List<WeakReference>();
                foreach (Path<object> path in selection)
                    hidden.Add(new WeakReference(path.Last));

                foreach (object item in selection)
                    visibilityContext.SetVisible(item, false);

                m_hideStack.Push(hidden);

                Refresh();
            }
           
        }

        private void ShowSelection(IEnumerable<object> selection, IVisibilityContext visibilityContext)
        {
            if (selection != null && visibilityContext != null)
            {
                foreach (object item in selection)
                    visibilityContext.SetVisible(item, true);

                Refresh();
            }
        }

        /// <summary>
        /// Shows all objects</summary>
        /// <param name="visibilityContext">Visibility context</param>
        /// <param name="enumerableContext">Enumerable context</param>
        public void ShowAll(IVisibilityContext visibilityContext, IEnumerableContext enumerableContext)
        {
            if (visibilityContext != null && enumerableContext != null)
            {
                foreach (object item in enumerableContext.Items)
                    visibilityContext.SetVisible(item, true);

                Refresh();
            }
        }

        private void ShowLastHidden(IVisibilityContext visibilityContext)
        {
            if (m_hideStack.Count > 0)
            {
                IList<WeakReference> hidden = m_hideStack.Pop();
                foreach (WeakReference wr in hidden)
                {
                    object obj = wr.Target;
                    if (obj != null)
                        visibilityContext.SetVisible(obj, true);
                }

                Refresh();
            }
        }

        private void IsolateSelection(IEnumerable<object> selection, IVisibilityContext visibilityContext, IEnumerableContext enumerableContext)
        {
            if (selection != null && visibilityContext != null && enumerableContext != null)
            {
                foreach (object item in enumerableContext.Items)
                    visibilityContext.SetVisible(item, false);

                foreach (Path<object> path in selection)
                {
                    foreach (object item in path)
                    {
                        visibilityContext.SetVisible(item, true);                        
                    }

                    DomNode lastNode = path.Last.As<DomNode>();
                    SetVisibility(lastNode, true, visibilityContext);
                }
                    
                Refresh();
            }
        }

        private void SetVisibility(DomNode node, bool visibility,IVisibilityContext visibilityContext)
        {
            if(visibilityContext.CanSetVisible(node))
            {
                visibilityContext.SetVisible(node, visibility);
                foreach (DomNode child in node.Children)
                {                    
                    SetVisibility(child, visibility, visibilityContext);
                }                
            }
        }

        private void Refresh()
        {
            m_designView.InvalidateViews();
        }

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Obtains enumerator for commands</summary>
        /// <param name="context">Context</param>
        /// <param name="target">Target</param>
        /// <returns></returns>
        public virtual IEnumerable<object> GetCommands(object context, object target)
        {            
            bool contextTest = (context is IGameContext)  || (context is FilteredTreeView);            
            bool targetTest = (target is DesignViewControl) || (Adapters.Is<IGameObject>(target));

            if (contextTest && targetTest)
            {                
                yield return StandardCommand.ViewHide;
                yield return StandardCommand.ViewShow;
                yield return StandardCommand.ViewShowLast;
                yield return StandardCommand.ViewShowAll;
                yield return StandardCommand.ViewIsolate;
            }            
        }

        #endregion

        [Import(AllowDefault = false)]
        private IDesignView m_designView = null;

        [Import(AllowDefault = false)]
        private ScriptingService m_scriptingService = null;

        private readonly Stack<IList<WeakReference>> m_hideStack = new Stack<IList<WeakReference>>();
        private readonly ICommandService m_commandService;
        private readonly IContextRegistry m_contextRegistry;

    }
}
