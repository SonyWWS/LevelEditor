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


    public static class ResourceTypes
    {
        public const string Geometry = "Geometry";
        public const string Texture  = "Texture";
    }
}
