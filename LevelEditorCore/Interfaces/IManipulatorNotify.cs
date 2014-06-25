//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace LevelEditorCore
{
    /// <summary>
    /// An optional interface that can be implemented by
    /// any ITransformable object. The object will receive notification 
    /// when it is been manipulated.
    /// </summary>
    public interface IManipulatorNotify
    {
        /// <summary>
        /// Begin drag
        /// called one time before dragging the object</summary>
        void OnBeginDrag();

        /// <summary>
        /// called once after dragging ends.</summary>
        void OnEndDrag();

    }
}
