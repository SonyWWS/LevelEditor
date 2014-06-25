//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;

using LevelEditorCore;

namespace LevelEditor.Commands
{

    /// <summary>
    /// Component that adds LevelEditor file commands: File/New, File/Open, File/Save,
    /// File/SaveAs, File/Close. This component requires an
    /// IFileDialogService for file dialogs; ATF provides a default implementation,
    /// FileDialogService. Use this, customize it, or provide your own implementation.</summary>
    [Export(typeof(IDocumentService))]
    [Export(typeof(StandardFileCommands))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class LevelEditorFileCommands : StandardFileCommands
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="documentRegistry">Document registry</param>
        /// <param name="fileDialogService">File dialog service</param>
        [ImportingConstructor]
        public LevelEditorFileCommands(
            ICommandService commandService,
            IDocumentRegistry documentRegistry,
            IFileDialogService fileDialogService) : base(commandService,documentRegistry,fileDialogService)
        {
            RegisterCommands = (RegisterCommands & ~(CommandRegister.FileSaveAll | CommandRegister.FileClose));
        }


        /// <summary>
        /// Saves the document under its current name</summary>
        /// <param name="document">Document to save</param>
        /// <returns>true, if document was successfully saved and false if the user cancelled
        /// or there was some kind of problem</returns>
        /// <remarks>All exceptions are caught and are reported via OnSaveException(). The
        /// IDocumentClient is responsible for the save operation and ensuring that any
        /// existing files are not corrupted if the save fails. Only Documents that are dirty
        /// are really saved.</remarks>
        public override bool Save(IDocument document)
        {
            GameDocument gameDcument = document as GameDocument;
            if (gameDcument == null)
                return base.Save(document);

            if (IsUntitled(document))
                return SaveAs(document);

            if (!gameDcument.AnyDirty)
                return true;           

            return SafeSave(document, DocumentEventType.Saved);
        }

        protected override bool ConfirmClose(IDocument document)
        {

            GameDocument gameDcument = document as GameDocument;
            if (gameDcument == null)
                return base.ConfirmClose(document);

            bool dirty = false;
            foreach (IGameDocument doc in m_gameDocumentRegistry.Documents)
            {
                dirty |= doc.Dirty;
            }

            // external resources.
            foreach (var obj in Util.FindAll<IEditableResourceOwner>())
            {
                dirty |= obj.Dirty;                
            }

            bool closeConfirmed = true;
            if (dirty)
            {
                string message = "One or more level and/or external resource is dirty"
                    + Environment.NewLine + "Save Changes?";
                               
                FileDialogResult result = FileDialogService.ConfirmFileClose(message);
                if (result == FileDialogResult.Yes)
                {
                    closeConfirmed = Save(document);
                }
                else if (result == FileDialogResult.Cancel)
                {
                    closeConfirmed = false;
                }
            }
            return closeConfirmed;
        }

        [Import(AllowDefault = false)] 
        private IGameDocumentRegistry m_gameDocumentRegistry = null;
    }
}
