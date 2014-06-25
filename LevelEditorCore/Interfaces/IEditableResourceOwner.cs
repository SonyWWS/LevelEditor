//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace LevelEditorCore
{
    /// <summary>
    /// Can be implemented by objects that references 
    /// resource that can be editted by 
    /// </summary>
    public interface IEditableResourceOwner
    {
        /// <summary>
        /// Return true if one or more resource is dirty</summary>
        bool Dirty { get; }

        /// <summary>
        /// Save dirty resources</summary>
        void Save();
    }
}
