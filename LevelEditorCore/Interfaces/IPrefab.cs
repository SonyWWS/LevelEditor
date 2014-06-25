//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf;

namespace LevelEditorCore
{
    /// <summary>
    /// Prefab interface</summary>
    public interface IPrefab : IResource
    {
        /// <summary>
        /// Gets all the gameobjects in this prefab</summary>
        IEnumerable<IGameObject> GameObjects { get; }

        /// <summary>
        /// Gets name of the prefab </summary>
        string Name { get; }
    }
}
