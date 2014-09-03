//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

using LevelEditorCore;
using LevelEditor.DomNodeAdapters;


namespace LevelEditor
{
    /// <summary>
    /// Bookmark editor.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(BookmarkLister))]
    [Export(typeof(IContextMenuCommandProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class BookmarkLister : TreeControlEditor, 
        ICommandClient, 
        IContextMenuCommandProvider,
        IControlHostClient, 
        IInitializable
    {
         /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service for opening right-click context menus</param>
        [ImportingConstructor]
        public BookmarkLister(ICommandService commandService)
            : base(commandService)
        {
            Configure(out m_controlInfo);
            m_commandService = commandService;

        }

        /// <summary>
        /// Configures the BookmarkLister</summary>
        /// <param name="controlInfo">Information about the control for the hosting service</param>
        protected virtual void Configure(
            out ControlInfo controlInfo)
        {
            Image bkmImage = ResourceUtil.GetImage16(LevelEditorCore.Resources.BookmarkImage);
            controlInfo = new ControlInfo(
                "Bookmarks".Localize(),
                "Edits document bookmarks".Localize(),
                StandardControlGroup.Hidden,
                bkmImage);

            TreeControl.ShowRoot = false;
            TreeControl.AllowDrop = false;
            TreeControl.SelectionMode = SelectionMode.One;
            TreeControl.MouseDown += new MouseEventHandler(TreeControl_MouseDown);
        }

        private void TreeControl_MouseDown(object sender, MouseEventArgs e)
        {
            Bookmark bkm = LastHit.As<Bookmark>();
            if (e.Button == MouseButtons.Left && bkm != null)
            {
                m_designView.ActiveView.Camera.Init(bkm.Camera);                
            }                                             
        }

        /// <summary>
        /// Gets the control info instance, which determines the appearance and
        /// initial location of the control in the application.</summary>
        public ControlInfo ControlInfo
        {
            get { return m_controlInfo; }
        }
       
        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
            m_controlHostService.RegisterControl(TreeControl, m_controlInfo, this);

            m_commandService.RegisterCommand(
             new CommandInfo(
                 Commands.AddBookmark,
                 null,
                 null,
                 "Add Bookmark".Localize(),
                 "Adds a new bookmark".Localize()),
             this);
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the control gets focus, or a parent "host" control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        void IControlHostClient.Activate(Control control)
        {
            if (m_bookmarkContext != null)
                m_contextRegistry.ActiveContext = m_bookmarkContext;
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another control or "host" control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        void IControlHostClient.Deactivate(Control control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control.</summary>
        /// <param name="control">Client control to be closed</param>
        /// <returns>true if the control can close, or false to cancel.</returns>
        bool IControlHostClient.Close(Control control)
        {
            return true;
        }

        #endregion

        #region ICommandClient Members

        bool ICommandClient.CanDoCommand(object commandTag)
        {
            return Commands.AddBookmark.Equals(commandTag)
                 && (LastHit.Is<BookmarkingContext>() || LastHit.Is<Bookmark>());           
        }

        void ICommandClient.DoCommand(object commandTag)
        {
            if (Commands.AddBookmark.Equals(commandTag))
            {                
                IList<Bookmark> bookmarkList = null;
                Bookmark parentBookmark = Adapters.As<Bookmark>(LastHit);
                if (parentBookmark != null)
                {
                    bookmarkList = parentBookmark.Bookmarks;
                }
                else
                {
                    BookmarkingContext bookmarkContext = Adapters.As<BookmarkingContext>(LastHit);
                    if (bookmarkContext != null)
                        bookmarkList = bookmarkContext.Bookmarks;
                    else if (Context != null)
                        bookmarkList = Context.Bookmarks;
                }

                if (bookmarkList != null)
                {
                    Bookmark newBookmark = new DomNode(Schema.bookmarkType.Type).As<Bookmark>();
                    newBookmark.Name = "New bookmark".Localize();
                    newBookmark.Camera = m_designView.ActiveView.Camera;

                    ITransactionContext transactionContext = Adapters.As<ITransactionContext>(m_bookmarkContext);
                    TransactionContexts.DoTransaction(
                        transactionContext,
                        delegate
                        {
                            bookmarkList.Add(newBookmark);
                        },
                        "Add bookmark".Localize());
                }
            }
        }

        void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion


        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object target)
        {           
            if (Adapters.Is<BookmarkingContext>(context) && TreeControl.Focused)
            {                
                yield return Commands.AddBookmark;
            }
        }

        #endregion
        
        private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            Context = m_contextRegistry.GetMostRecentContext<BookmarkingContext>();
            TreeControl.Text = (Context != null) ?
                "Right click to create a new camera bookmark that stores the current camera settings." +
                " Click on a camera bookmark to set the camera to the stored settings.".Localize()
               : null;
        }

        public enum Commands
        {
            AddBookmark,
        }

        /// <summary>
        /// Gets or sets the BookmarkingContext to be viewed and edited by the user</summary>
        public BookmarkingContext Context
        {
            get { return m_bookmarkContext; }
            private set
            {
                if (m_bookmarkContext != value)
                {
                    m_bookmarkContext = value;
                    TreeView = m_bookmarkContext;
                }
            }
        }

        [Import(AllowDefault = false)]
        private IDesignView m_designView = null;
        
        [Import]
        private IControlHostService m_controlHostService = null;

        [Import]
        private IContextRegistry m_contextRegistry = null;

        private ICommandService m_commandService;
        private readonly ControlInfo m_controlInfo;
        private BookmarkingContext m_bookmarkContext;        
    }

}
