//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace LevelEditorCore
{
    /// <summary>
    /// Game Context interface.    
    /// </summary>
    public interface IGameContext
    {
        /// <summary>
        /// Gets true if this is master context.
        /// </summary>
        bool IsMasterContext { get; }

        /// <summary>
        /// Gets root game object folder.
        /// </summary>
        IGameObjectFolder RootGameObjectFolder { get; }

    }
}
