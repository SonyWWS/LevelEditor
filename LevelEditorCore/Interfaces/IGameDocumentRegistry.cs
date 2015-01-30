//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf;
using Sce.Atf.Dom;

namespace LevelEditorCore
{
    public interface IGameDocumentRegistry
    {
        /// <summary>
        /// Gets master game document. </summary>
        IGameDocument MasterDocument { get; }

        /// <summary>
        /// Gets all sub documents.</summary>
        IEnumerable<IGameDocument> SubDocuments { get; }

        /// <summary>
        /// Gets all documents including master.</summary>
        IEnumerable<IGameDocument> Documents { get; }

        /// <summary>
        /// Finds document by uri</summary>        
        IGameDocument FindDocument(Uri ur);
        

        /// <summary>
        /// Finds object of the of type T in all documents.</summary>
        /// <typeparam name="T">The type to search for</typeparam>
        /// <returns>Enumerable of objects of type T</returns>
        IEnumerable<T> FindAll<T>() where T : class;
        
        /// <summary>
        /// Checks if any document is dirty.</summary>
        bool AnyDocumentDirty { get; }

        /// <summary>
        /// Checks if any resource of any document is dirty</summary>
        bool AnyEditableResourceOwnerDirty { get; }

        /// <summary>
        /// Checks if GameDocumentRegistry contains a GameDocument</summary>
        /// <param name="doc">GameDocument to search for</param>
        /// <returns>True if GameDocument is in GameDocumentRegistry</returns>
        bool Contains(IGameDocument doc);

        /// <summary>
        /// Event that is raised after a document is added</summary>
        event EventHandler<ItemInsertedEventArgs<IGameDocument>> DocumentAdded;

        /// <summary>
        /// Event that is raised after a document is removed</summary>
        event EventHandler<ItemRemovedEventArgs<IGameDocument>> DocumentRemoved;

        /// <summary>
        /// Event that is raised after dirty flag changes in any document</summary>
        event EventHandler<ItemChangedEventArgs<IGameDocument>> DocumentDirtyChanged;

        /// <summary>
        /// Event that is raised after the Uri of any document changes</summary>
        event EventHandler<ItemChangedEventArgs<IGameDocument>> DocumentUriChanged;

        /// <summary>
        /// Event that is raised after Dirty property of any IEditableResourceOwner changes</summary>
        event EventHandler<ItemChangedEventArgs<IEditableResourceOwner>> EditableResourceOwnerDirtyChanged;



    }
}
