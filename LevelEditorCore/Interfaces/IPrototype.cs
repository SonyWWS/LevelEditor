//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;

namespace LevelEditorCore
{
    /// <summary>
    /// Interface for prototype</summary>
    public interface IPrototype : IResource
    {
        /// <summary>
        /// Creates new instance of this prototype.</summary>
        /// <returns></returns>
        IGameObject CreateInstance();
    }
}
