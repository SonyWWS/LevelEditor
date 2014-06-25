//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf;

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
        /// Event that is raised after a document is added</summary>
        event EventHandler<ItemInsertedEventArgs<IGameDocument>> DocumentAdded;

        /// <summary>
        /// Event that is raised after a document is removed</summary>
        event EventHandler<ItemRemovedEventArgs<IGameDocument>> DocumentRemoved;
    }
}
