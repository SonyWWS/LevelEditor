//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace LevelEditorCore
{   
    /// <summary>
    /// Interface for game objects</summary>
    public interface IGameObject : ITransformable, INameable, IVisible,ILockable, IListable
    {
        // todo, add IGame GetGame()
        // returns the game that owns this gameobject or null.
    }
}
