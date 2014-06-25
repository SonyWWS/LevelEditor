//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Rendering;

namespace LevelEditorCore.Commands
{
    /// <summary>
    /// Commands for switching the DesignView's active camera</summary>
    [Export(typeof(CameraCommands))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CameraCommands : ICommandClient, IInitializable
    {
        #region IInitializable Implementation

        void IInitializable.Initialize()
        {
            // register a custom menu without any commands, so it won't appear in the main menu bar
            var cameraMenu = CommandService.RegisterMenu(this, "Camera", "Camera");
            ToolStrip strip = cameraMenu.GetToolStrip();


            string camera = "Camera".Localize();

            CommandInfo cmdInfo =
                CommandService.RegisterCommand(
                    Command.Arcball,
                    StandardMenu.View,
                    StandardCommandGroup.ViewCamera,
                    camera + "/" + "Arcball".Localize(),
                    "Arcball".Localize(),
                    Keys.None,
                    Resources.ArcballImage,
                    CommandVisibility.Menu,
                    this);
            strip.Items.Add(cmdInfo.GetButton());

            cmdInfo =
                CommandService.RegisterCommand(
                    Command.Maya,
                    StandardMenu.View,
                    StandardCommandGroup.ViewCamera,
                    camera + "/" + "Maya".Localize(),
                    "Maya style camera".Localize(),
                    Keys.None,
                    Resources.MayaImage,
                    CommandVisibility.Menu,
                    this);
            strip.Items.Add(cmdInfo.GetButton());

            cmdInfo =
                CommandService.RegisterCommand(
                    Command.Fly,
                    StandardMenu.View,
                    StandardCommandGroup.ViewCamera,
                    camera + "/" + "Fly".Localize(),
                    "Fly:  WASD + Middle Mouse navigation, mouse-wheel to adjust speed".Localize(),
                    Keys.None,
                    Resources.FlyImage,
                    CommandVisibility.Menu,
                    this);
            strip.Items.Add(cmdInfo.GetButton());

            cmdInfo =
                CommandService.RegisterCommand(
                    Command.Walk,
                    StandardMenu.View,
                    StandardCommandGroup.ViewCamera,
                    camera + "/" + "Walk".Localize(),
                    "Walk: WASD + Middle Mouse, press Alt+MiddleMouse for height, mouse wheel to adjust walk speed".Localize(),
                    Keys.None,
                    Resources.WalkImage,
                    CommandVisibility.Menu,
                    this);
            strip.Items.Add(cmdInfo.GetButton());

            SettingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(this, () => CameraMode, "CameraMode", null, null));
        }

        #endregion

        #region ICommandClient Implementation

        /// <summary>
        /// Checks whether the client can do the command if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>true, if client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return false;

            if (m_designView == null)
                return false;

            var designControl = m_designView.ActiveView;

            var cmd = (Command)commandTag;
            switch (cmd)
            {
                case Command.Arcball:
                case Command.Maya:
                    return true;

                case Command.Walk:
                case Command.Fly:
                    {
                        return designControl.ViewType == ViewTypes.Perspective;                        
                    }
            }

            return false;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public void DoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return;

            if (m_designView == null)
                return;

            m_cameraMode = (Command)commandTag;

            var designControl = m_designView.ActiveView;
            
            switch (m_cameraMode)
            {
                case Command.Arcball:
                    {
                        if (!(designControl.CameraController is ArcBallCameraController))
                            designControl.CameraController = new ArcBallCameraController();
                    }
                    break;

                case Command.Maya:
                    {
                        if (!(designControl.CameraController is MayaStyleCameraController))
                            designControl.CameraController = new MayaStyleCameraController();
                    }
                    break;

                case Command.Walk:
                    {
                        if (!(designControl.CameraController is WalkCameraController) &&
                            (designControl.ViewType == ViewTypes.Perspective))
                        {
                            designControl.CameraController = new WalkCameraController();
                        }
                    }
                    break;

                case Command.Fly:
                    {
                        if (!(designControl.CameraController is FlyCameraController) &&
                            (designControl.ViewType == ViewTypes.Perspective))
                        {
                            designControl.CameraController = new FlyCameraController();
                        }
                    }
                    break;
            }

            designControl.Invalidate();
        }

        /// <summary>
        /// Updates command state for given command. Only called if CanDoCommand is true.</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, CommandState state)
        {
            if (!(commandTag is Command))
                return;

            if (m_designView == null)
                return;

            var designControl = m_designView.ActiveView;

            switch ((Command)commandTag)
            {
                case Command.Arcball:
                    state.Check = (designControl.CameraController is ArcBallCameraController);
                    break;

                case Command.Maya:
                    state.Check = (designControl.CameraController is MayaStyleCameraController);
                    break;

                case Command.Walk:
                    state.Check = (designControl.CameraController is WalkCameraController);
                    break;

                case Command.Fly:
                    state.Check = (designControl.CameraController is FlyCameraController);
                    break;
            }
        }

        #endregion

        /// <summary>
        /// Gets and sets the currently selected camera mode. This string is
        /// persisted in the user's settings file.
        /// </summary>
        public string CameraMode
        {
            get { return m_cameraMode.ToString(); }
            set
            {
                // Make sure that 'value' is valid first, in case the the names have changed
                //  or the settings file is otherwise invalid.
                if (string.Compare(Command.Arcball.ToString(), value) == 0)
                    m_cameraMode = Command.Arcball;
                else if (string.Compare(Command.Maya.ToString(), value) == 0)
                    m_cameraMode = Command.Maya;
                else if (string.Compare(Command.Walk.ToString(), value) == 0)
                    m_cameraMode = Command.Walk;
                else if (string.Compare(Command.Fly.ToString(), value) == 0)
                    m_cameraMode = Command.Fly;

                DoCommand(m_cameraMode);
            }
        }

        /// <summary>
        /// Gets or sets the command service to use.</summary>
        [Import]
        public ICommandService CommandService { get; set; }

        /// <summary>
        /// Gets or sets the settings service to use.</summary>
        [Import]
        public ISettingsService SettingsService { get; set; }

        /// <summary>
        /// Camera modes</summary>
        protected enum Command
        {
            /// <summary>
            /// Standard "Arcball" camera
            /// </summary>
            Arcball,

            /// <summary>
            /// Maya camera
            /// </summary>
            Maya,

            /// <summary>
            /// Fly "WASD" camera
            /// </summary>
            Fly,

            /// <summary>
            /// Walk "WASD" camera
            /// </summary>
            Walk,
        }

        [Import(AllowDefault = false)]
        private IDesignView m_designView = null;

        private Command m_cameraMode = Command.Maya;
    }
}