//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace LevelEditorCore
{
    public interface ISnapSettings
    {
        /// <summary>
        /// When dragging an object around, should it snap to the nearest vertex of another object
        /// that the cursor is over?</summary>
        bool SnapVertex
        {
            get;
            set;
        }

        /// <summary>
        /// When dragging an object around and snaping it to a surface, should the object be rotated
        /// so that its local up-axis is aligned with the surface normal at that point?</summary>
        bool RotateOnSnap
        {
            get;
            set;
        }

        /// <summary>
        /// Snap to angle when using rotation manipulator.
        /// The Angle is in radians</summary>        
        float SnapAngle
        {
            get;
            set;
        }


        /// <summary>
        /// If 'true', then the translate manipulator should use the object's local coordinate system,
        /// otherwise, the translate manipulator control is aligned to the world coordinate system.</summary>
        bool ManipulateLocalAxis
        {
            get;
            set;
        }

        /// <summary>
        /// This property is the current snap-from mode. Manipulators use this mode to
        /// determine the offset from the object's origin that the object should be snapped with
        /// when snapping to some other object. See TransformUtils.CalcSnapFromOffset() and
        /// TransformUtils.SnapFromModes.</summary>
        SnapFromMode SnapFrom
        {
            get;
            set;
        }
    }


    /// <summary>
    /// Enums that correspond to the strings in TransformUtils.SnapFromModes</summary>
    public enum SnapFromMode
    {
        Pivot,
        Origin,
        TopCenter,
        BottomCenter,
        FrontCenter,
        BackCenter,
        LeftCenter,
        RightCenter,
    }
}
