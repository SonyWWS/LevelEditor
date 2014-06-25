//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace LevelEditorCore
{
    /// <summary>
    /// Interface for game object folders</summary>
    public interface IGameObjectFolder : IHierarchical,INameable,IVisible, IListable, ILockable
    {
        /// <summary>
        /// Gets the list of game objects</summary>
        IList<IGameObject> GameObjects
        {
            get;
        }
         

        /// <summary>
        /// Get the list of child folders</summary>
        IList<IGameObjectFolder> GameObjectFolders
        {
            get;
        }
    }
}
