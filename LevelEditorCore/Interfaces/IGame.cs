//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf;

namespace LevelEditorCore
{    
    /// <summary>
    /// Represents game (Level).    
    /// </summary>
    public interface IGame : IListable, IHierarchical, INameable
    {
        /// <summary>
        /// Gets grid.
        /// </summary>
        IGrid Grid { get; }

        /// <summary>
        /// Gets the Reference<IGame> that references this game</summary>
        IReference<IGame> Parent
        {
            get;
        }

        /// <summary>
        /// Gets game references
        /// </summary>
        IEnumerable<IReference<IGame>> GameReferences { get; }

        /// <summary>
        /// Gets root game object folder.
        /// </summary>
        IGameObjectFolder RootGameObjectFolder { get; }
       
        /// <summary>
        /// Creates new ResourceReference instance.</summary>        
        IReference<IResource> CreateResourceReference(IResource resource);

        /// <summary>
        /// Creates new GameObjectGroup instance</summary>        
        IGameObjectGroup CreateGameObjectGroup();

        /// <summary>
        /// Creates new GameObjectFolder instance.</summary>        
        IGameObjectFolder CreateGameObjectFolder();
    }
    
}
