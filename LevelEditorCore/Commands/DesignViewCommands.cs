//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Rendering;

namespace LevelEditorCore.Commands
{
    [Export(typeof(DesignViewCommands))]
    [Export(typeof(IInitializable))]
    [Export(typeof(ICommandClient))]
    [Export(typeof(IContextMenuCommandProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DesignViewCommands : ICommandClient, IContextMenuCommandProvider, IInitializable
    {
        [ImportingConstructor]
        public DesignViewCommands(
            IContextRegistry contextRegistry,
            ICommandService commandService)
        {
            m_contextRegistry = contextRegistry;
            m_commandService = commandService;
            m_contextMenuCommands = new object[]
                                        {
                                            ViewTypes.Perspective,
                                            ViewTypes.Top,
                                            ViewTypes.Bottom,
                                            ViewTypes.Right,
                                            ViewTypes.Left,
                                            ViewTypes.Front,
                                            ViewTypes.Back
                                        };

        }

        #region IInitializable Members

        public void Initialize()
        {
            RegisterCommands();
        }

        #endregion

        #region ICommandClient

        /// <summary>
        /// Checks whether the client can do the command if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>true, if client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {

            bool result = ((commandTag is Command) || (commandTag is ViewTypes));
                
            return result;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public void DoCommand(object commandTag)
        {
            
            if (commandTag is Command)                       
            {
                switch ((Command)commandTag)
                {
                   
                    case Command.SingleView:

                        m_designView.ViewMode = ViewModes.Single;
                        break;

                    case Command.QuadView:
                        m_designView.ViewMode = ViewModes.Quad;
                        break;

                    case Command.DualHorizontalView:
                        m_designView.ViewMode = ViewModes.DualHorizontal;
                        break;

                    case Command.DualVerticalView:
                        m_designView.ViewMode = ViewModes.DualVertical;
                        break;
                }
            }
            else if(commandTag is ViewTypes)
            {
                m_designView.ActiveView.ViewType = (ViewTypes)commandTag;
            }
        }

        /// <summary>
        /// Updates the view's commands</summary>
        /// <param name="commandTag">The command tag</param>
        /// <param name="state">State of the command</param>
        public void UpdateCommand(object commandTag, CommandState state)
        {
            
            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                   
                    case Command.SingleView:
                        state.Check = m_designView.ViewMode == ViewModes.Single;
                        break;

                    case Command.QuadView:
                        state.Check = m_designView.ViewMode == ViewModes.Quad;
                        break;

                    case Command.DualHorizontalView:
                        state.Check = m_designView.ViewMode == ViewModes.DualHorizontal;
                        break;

                    case Command.DualVerticalView:
                        state.Check = m_designView.ViewMode == ViewModes.DualVertical;
                        break;
                }
            }
        }

        #endregion //ICommandClient

        #region IContextMenuCommandProvider Members

        public IEnumerable<object> GetCommands(object context, object target)
        {
            if (m_designView.Context == context)
            {
                return m_contextMenuCommands;
            }                
            return EmptyEnumerable<object>.Instance;
        }

        #endregion

        private enum Command
        {
            SingleView,                     //per Design View
            QuadView,                       //per Design View
            DualHorizontalView,             //per Design View
            DualVerticalView,               //per Design View         
        }

        // The order of these enums determines the order of the command groups in a menu.
        private enum CommandGroup
        {
            Manipulators = 128, 
            Layouts,
            ProjectionModes,
            Display,
            Bookmarks,
        }

        private void RegisterCommands()
        {
            string layouts = "Layouts".Localize();
            m_commandService.RegisterCommand(
                Command.SingleView,
                StandardMenu.View,
                CommandGroup.Layouts,
                layouts + "/" + "Single View".Localize(),
                "Switch to Single View".Localize(),
                Keys.None,
                Resources.SingleViewImage,
                CommandVisibility.All,
                this);

            m_commandService.RegisterCommand(
                Command.QuadView,
                StandardMenu.View,
                CommandGroup.Layouts,
                layouts + "/" + "Quad View".Localize(),
                "Switch To Quad View".Localize(),
                Keys.None,
                Resources.QuadViewImage,
                CommandVisibility.All,
                this);

            m_commandService.RegisterCommand(
                Command.DualHorizontalView,
                StandardMenu.View,
                CommandGroup.Layouts,
                layouts + "/" + "Dual Horizontal View".Localize(),
                "Switch To Dual Horizontal View".Localize(),
                Keys.None,
                Resources.HorizSplitViewImage,
                CommandVisibility.All,
                this);

            m_commandService.RegisterCommand(
                Command.DualVerticalView,
                StandardMenu.View,
                CommandGroup.Layouts,
                layouts + "/" + "Dual Vertical View".Localize(),
                "Switch To Dual Vertical View".Localize(),
                Keys.None,
                Resources.VertSplitViewImage,
                CommandVisibility.All,
                this);


            string projection = "Projection".Localize();

            m_commandService.RegisterCommand(
                ViewTypes.Perspective,
                StandardMenu.View,
                CommandGroup.ProjectionModes,
                projection + "/" + "Perspective".Localize(),
                "Perspective projection".Localize(),
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(
                ViewTypes.Top,
                StandardMenu.View,
                CommandGroup.ProjectionModes,
                projection + "/" + "Top".Localize(),
                "Top projection ".Localize(),
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(
                ViewTypes.Bottom,
                StandardMenu.View,
                CommandGroup.ProjectionModes,
                projection + "/" + "Bottom".Localize(),
                "Bottom projection".Localize(),
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);


            m_commandService.RegisterCommand(
                ViewTypes.Right,
                StandardMenu.View,
                CommandGroup.ProjectionModes,
                projection + "/" + "Right".Localize(),
                "Right projeciton".Localize(),
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);


            m_commandService.RegisterCommand(
                ViewTypes.Left,
                StandardMenu.View,
                CommandGroup.ProjectionModes,
                projection + "/" + "Left".Localize(),
                "Left projeciton".Localize(),
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);


            m_commandService.RegisterCommand(
                ViewTypes.Front,
                StandardMenu.View,
                CommandGroup.ProjectionModes,
                projection + "/" + "Front".Localize(),
                "Front projeciton".Localize(),
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            m_commandService.RegisterCommand(
                ViewTypes.Back,
                StandardMenu.View,
                CommandGroup.ProjectionModes,
                projection + "/" + "Back".Localize(),
                "Back projeciton".Localize(),
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);
        }

        

        private object[] m_contextMenuCommands;      

        [Import(AllowDefault = false)]
        private IDesignView m_designView = null;
        
        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;
    }
}
