//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace LevelEditorCore
{
    /// <summary>
    /// Client code can implement this interface
    /// to filter out objects that can't be used as a snap target.</summary>
    /// <remarks>The snap operation is performed when 
    /// drag/dropping objects onto designView or when moving objects
    /// using translate manipulator in both case snapping have to 
    /// activated using shortcut keys</remarks>
    public interface ISnapFilter
    {
        /// <summary>
        /// Can dragObj snaps to snapObj.
        /// </summary>
        /// <param name="dragObj">object that is being dragged</param>
        /// <param name="snapObj">object that the dragged object is trying to snap to</param>
        /// <returns>True if dragObj can be snapped to snapObject.</returns>
        bool CanSnapTo(object dragObj, object snapObj);        
    }
}
