//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace LevelEditorCore
{
    public interface ILinear 
    {
        /// <summary>
        /// Attempts to insert a new control point</summary>
        /// <returns>true if control point inserted, false otherwise</returns>
        IControlPoint InsertPoint(uint index, float x, float y, float z);
    }
}
