//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace LevelEditorCore
{   
    /// <summary>
    /// Interface for game objects</summary>
    public interface IGameObject : ITransformable, INameable, IVisible,ILockable, IListable
    { 
         /// <summary>  
         /// Gets the game that owns this game object.</summary>  
         /// <returns>The game that owns this game object, or null if this object isn't owned.</returns>  
         IGame GetGame();
    }
}
