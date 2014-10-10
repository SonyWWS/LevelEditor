//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;

namespace LevelEditorCore.Commands
{
    [Export(typeof(ManipulatorCommands))]
    [Export(typeof(IInitializable))]
    [Export(typeof(ICommandClient))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ManipulatorCommands : ICommandClient, IInitializable
    {
        [ImportingConstructor]
        public ManipulatorCommands(ICommandService commandService)
        {            
            m_commandService = commandService;
            
        }

        #region IInitializable Members

        public void Initialize()
        {
            RegisterCommands();

            foreach (IManipulator manipulator in m_manipulators)
            {
                ManipulatorInfo info = manipulator.ManipulatorInfo;
                m_commandService.RegisterCommand(
                   manipulator,
                   StandardMenu.Modify,
                   CommandGroup.Manipulators,
                   info.Name,
                   info.Description,
                   info.ShortCut,
                   info.Image,
                   CommandVisibility.All,
                   this);
            }

            if (m_settingsService != null)
            {
                m_settingsService.Reloaded += (sender, e) =>
                    {
                        ISnapSettings snapSettings = (ISnapSettings)m_designView;
                        m_snapFromModeComboBox.SelectedItem = snapSettings.SnapFrom;
                    };
            }
        }

        #endregion
        
        #region ICommandClient

        /// <summary>
        /// Checks whether the client can do the command if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>true, if client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            
            
            bool cando = (commandTag is IManipulator) || commandTag.Equals(Command.Select);
            if(!cando && commandTag is Command)
            {

                switch ((Command)commandTag)
                {
                    case Command.Select:                        
                    case Command.SnapToVertex:                        
                    case Command.RotateOnSnap:
                        cando = true;
                        break;
                }
            }
            return cando;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public void DoCommand(object commandTag)
        {

            ISnapSettings snapSettings = (ISnapSettings)m_designView;
            IManipulator manip = commandTag as IManipulator;
            if(manip != null)
            {
                m_designView.Manipulator = manip;
            }
            else if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                                     
                    case Command.Select:
                        m_designView.Manipulator = null; 
                        break;
                   
                    case Command.SnapToVertex:
                        snapSettings.SnapVertex = !snapSettings.SnapVertex;
                        break;

                    case Command.RotateOnSnap:
                        snapSettings.RotateOnSnap = !snapSettings.RotateOnSnap;
                        break;

                }
            }
        }

        /// <summary>
        /// Updates the view's commands</summary>
        /// <param name="commandTag">The command tag</param>
        /// <param name="state">State of the command</param>
        public void UpdateCommand(object commandTag, CommandState state)
        {

            ISnapSettings snapSettings = (ISnapSettings)m_designView;
            IManipulator manip = commandTag as IManipulator;
            if(manip != null)
            {
                state.Check = manip == m_designView.Manipulator;
            }
            else if (commandTag is Command)
            {                
                switch ((Command)commandTag)
                {                    
                    case Command.Select:
                        state.Check = m_designView.Manipulator == null;
                        break;
                   
                    case Command.SnapToVertex:
                        state.Check = snapSettings.SnapVertex;
                        break;

                    case Command.RotateOnSnap:
                        state.Check = snapSettings.RotateOnSnap;
                        break;

                }
            }
        }

        #endregion //ICommandClient

        private enum Command
        {
            Select,                        
            SnapToVertex,
            RotateOnSnap,
        }

        // The order of these enums determines the order of the command groups in a menu.
        private enum CommandGroup
        {
            Manipulators = 128,
        }

        private void RegisterCommands()
        {
            
            m_commandService.RegisterCommand(
                Command.Select,
                StandardMenu.Modify,
                CommandGroup.Manipulators,
                "Select".Localize(),
                "Activate Selection Manipulator".Localize(),
                Keys.Q,
                Sce.Atf.Resources.SelectionImage,
                CommandVisibility.All,
                this);

            m_commandService.RegisterCommand(
              Command.SnapToVertex,
              StandardMenu.Modify,
              CommandGroup.Manipulators,
              "SnapToVertex".Localize(),
              "Snap To Vertex".Localize(),
              Keys.None,
              Resources.VertexSnapImage,
              CommandVisibility.All,
              this);

            m_commandService.RegisterCommand(
                Command.RotateOnSnap,
                StandardMenu.Modify,
                CommandGroup.Manipulators,
                "RotateOnSnap".Localize(),
                "Rotate On Snap".Localize(),
                Keys.None,
                Resources.RotateOnSnapImage,
                CommandVisibility.All,
                this);

            
             // Register comboBoxes for snap-from-mode and reference-coordinate-system, in the edit
            //  menu's toolbar
            m_snapFromModeComboBox = new ToolStripComboBox();
            m_snapFromModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            m_snapFromModeComboBox.Name = "Snap From Mode".Localize();
            m_snapFromModeComboBox.ComboBox.DataSource = Enum.GetValues(typeof (SnapFromMode));            
            m_snapFromModeComboBox.SelectedIndexChanged +=new EventHandler(m_snapFromModeComboBox_SelectedIndexChanged);
            m_snapFromModeComboBox.ToolTipText = "Snap mode".Localize();
            
            
            m_referenceCoordinateSystemComboBox = new ToolStripComboBox();
            m_referenceCoordinateSystemComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            m_referenceCoordinateSystemComboBox.Name = "Reference Coordinate System";
            m_referenceCoordinateSystemComboBox.ToolTipText = "Reference Coordinate System";
            m_referenceCoordinateSystemComboBox.Items.AddRange(
                new string[] { "World", "Local"});
            m_referenceCoordinateSystemComboBox.SelectedIndex = 0;
            m_referenceCoordinateSystemComboBox.SelectedIndexChanged += referenceCoordinateSystemComboBox_SelectedIndexChanged;

            MenuInfo editMenuInfo = MenuInfo.Edit;
            editMenuInfo.GetToolStrip().Items.Add(m_referenceCoordinateSystemComboBox);
            editMenuInfo.GetToolStrip().Items.Add(m_snapFromModeComboBox);
        }

        void m_snapFromModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ISnapSettings snapSettings = (ISnapSettings)m_designView;
            snapSettings.SnapFrom = (SnapFromMode) m_snapFromModeComboBox.SelectedItem;
            m_designView.InvalidateViews();
        }

        // Change the currently active DesignView to reflect the new combo box item.
        private void referenceCoordinateSystemComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ISnapSettings snapSettings = (ISnapSettings)m_designView;            
            snapSettings.ManipulateLocalAxis = (m_referenceCoordinateSystemComboBox.SelectedIndex == 1);
            m_designView.InvalidateViews();
        }
       
        [Import(AllowDefault = false)]
        private IDesignView m_designView = null;

        [ImportMany]
        private IEnumerable<IManipulator> m_manipulators;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService = null;

        private readonly ICommandService m_commandService;        
        private ToolStripComboBox m_referenceCoordinateSystemComboBox;
        private ToolStripComboBox m_snapFromModeComboBox;
    }
}
