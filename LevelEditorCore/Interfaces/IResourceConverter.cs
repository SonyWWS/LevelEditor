//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;

namespace LevelEditorCore
{
    /// <summary>
    /// Converts IResource to IGameObject</summary>
    /// </summary>
    public interface IResourceConverter
    {        
        /// <summary>
        /// Converts resource to gameObject.
        /// </summary>
        /// <param name="resource">resource to be converted</param>
        /// <returns>GameObject or null if convertion failed</returns>
        IGameObject Convert(IResource resource);

    }
}
