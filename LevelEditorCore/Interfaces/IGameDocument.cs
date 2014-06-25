//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

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
    }
}
