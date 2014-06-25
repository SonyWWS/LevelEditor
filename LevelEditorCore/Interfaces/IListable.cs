//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Applications;

namespace LevelEditorCore
{
    /// <summary>
    /// Interface for objects that are visible to the user in list and tree controls</summary>    
    public interface IListable
    {
        /// <summary>
        /// Gets display info for the object</summary>
        /// <param name="info">Item info, to be filled out</param>
        void GetInfo(ItemInfo info);
    }
}
