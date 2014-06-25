//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using LevelEditorCore.VectorMath;

namespace LevelEditorCore
{
    /// <summary>
    /// Interface for objects having a bounding box</summary>
    public interface IBoundable
    {
        /// <summary>
        /// Gets a bounding box in world space</summary>
        AABB BoundingBox
        {
            get;
        }

        /// <summary>
        /// Gets a bounding box in local space.
        /// </summary>
        AABB LocalBoundingBox
        {
            get;
        }
    }
}
