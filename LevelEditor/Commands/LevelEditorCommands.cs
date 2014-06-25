//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Applications;

namespace LevelEditor.Commands
{
    /// <summary>
    /// This class is provided as a tutorial for registering and managing your own ATF commands</summary>
    /// <remarks>When creating a new Commands class don't forget to register it in LevelEditorApplication class.</remarks>
    [Export(typeof(IInitializable))]
    [Export(typeof(LevelEditorCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class LevelEditorCommands : ICommandClient, IInitializable
    {
        /// <summary>
        /// Importing constructor receiving registered MEF services
        /// </summary>
        /// <param name="commandService"></param>
        /// <param name="contextRegistry"></param>
        [ImportingConstructor]
        public LevelEditorCommands(ICommandService commandService, IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
        }

        #region IInitializable Members

        /// <summary>
        /// Register commands in the Initialize method</summary>
        void IInitializable.Initialize()
        {
            //CommandServices.RegisterCommand(
            //    m_commandService,
            //    Command.RemoveGroup,
            //    null,
            //    StandardCommandGroup.EditGroup,
            //    "Remove/Group",
            //    "Removes the Group",
            //    Keys.None,
            //    null,
            //    CommandVisibility.Menu,
            //    this);

            //CommandServices.RegisterCommand(
            //    m_commandService,
            //    Command.RemoveTrack,
            //    null,
            //    StandardCommandGroup.EditGroup,
            //    "Remove/Track",
            //    "Removes the track",
            //    Keys.None,
            //    null,
            //    CommandVisibility.Menu,
            //    this);

            //CommandServices.RegisterCommand(
            //    m_commandService,
            //    Command.RemoveEmptyGroupsAndTracks,
            //    null,
            //    StandardCommandGroup.EditGroup,
            //    "Remove/Empty Groups and Tracks",
            //    "Removes empty Groups and Tracks",
            //    Keys.None,
            //    null,
            //    CommandVisibility.Menu,
            //    this);

            //CommandServices.RegisterCommand(
            //    m_commandService,
            //    Command.ToggleSplitMode,
            //    StandardMenu.Edit,
            //    null,
            //    "Interval Splitting Mode",
            //    "Toggles the interval splitting mode",
            //    Keys.S,
            //    null,
            //    CommandVisibility.Menu,
            //    this);

            //m_commandService.RegisterCommand(StandardCommand.ViewZoomExtents, CommandVisibility.All, this);
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Can the client do the command?</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True, iff client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            return DoCommand(commandTag, false);
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public void DoCommand(object commandTag)
        {
            DoCommand(commandTag, true);
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <remarks>This is used e.g. to set the check next to a menu command or 
        /// to show a toolbar button as pressed</remarks>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        public void UpdateCommand(object commandTag, CommandState commandState)
        {
            //GameDocument document = m_contextRegistry.GetActiveContext<GameDocument>();
            //if (document == null)
            //    return;

            //if (commandTag is Command)
            //{
            //    switch ((Command)commandTag)
            //    {
            //        case Command.ToggleSplitMode:
            //            commandState.Check = document.SplitManipulator.Active;
            //            break;
            //    }
            //}
        }

        #endregion

        /// <summary>
        /// Private member used by both DoCommand() and CanDoCommand()</summary>
        /// <param name="commandTag">Command to be performed</param>
        /// <param name="doing">True if called by Do(), false if called by CanDo()</param>
        /// <returns>True iff the command can be done</returns>
        /// <remarks>Use of this pattern is optional. It makes sense iff a lot of
        /// code would otherwise duplicated between the Do() and CanDo() methods.</remarks>
        private bool DoCommand(object commandTag, bool doing)
        {
            return false;
        }

        /// <summary>
        /// Command enumeration, used as command tags to distinguish between commands</summary>
        private enum Command
        {
            //RemoveGroup,
            //RemoveTrack,
            //RemoveEmptyGroupsAndTracks,
            //ToggleSplitMode,
        }

        private readonly ICommandService m_commandService;
        private readonly IContextRegistry m_contextRegistry;
    }
}
