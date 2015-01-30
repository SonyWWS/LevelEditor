//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf;

namespace LevelEditorCore
{
    public interface IGameDocument : IDocument
    {
        /// <summary>
        /// Gets root game object folder.
        /// </summary>
        IGameObjectFolder RootGameObjectFolder { get; }

        /// <summary>
        /// Gets whether this instance is master document.
        /// </summary>
        bool IsMasterGameDocument { get; }

        /// <summary>
        /// Gets game document references.
        /// </summary>
        IEnumerable<IReference<IGameDocument>> GameDocumentReferences { get; }


        /// <summary>
        /// Used by document child of IEditableResourceOwner type
        /// to notify parent document that one or more resources is changed </summary>
        /// <param name="resOwner">the resource owner</param>
        void NotifyEditableResourceOwnerDirtyChanged(IEditableResourceOwner resOwner);
        
        
        /// <summary>
        /// Event that is raised when Dirty property of any 
        /// child of type IEditableResourceOwner changes</summary>
        event EventHandler<ItemChangedEventArgs<IEditableResourceOwner>> EditableResourceOwnerDirtyChanged;

    }
}
