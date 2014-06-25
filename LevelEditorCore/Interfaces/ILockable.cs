//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace LevelEditorCore
{
    /// <summary>
    /// ILockable interface</summary>
    public interface ILockable
    {
        /// <summary>
        /// Gets or sets whether the item is locked.</summary>
        bool IsLocked
        {
            get;
            set;
        }
    }
}
