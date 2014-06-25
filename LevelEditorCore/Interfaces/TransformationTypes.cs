//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace LevelEditorCore
{
    /// <summary>
    /// Transformation types</summary>
    [Flags]
    public enum TransformationTypes
    {
        /// <summary>
        /// None Transformable.</summary>
        None = 0,

        /// <summary>
        /// Translation</summary>
        Translation = 1,

        /// <summary>
        /// Scale</summary>
        Scale = 2,

        /// <summary>
        /// Rotation</summary>
        Rotation = 4,

        /// <summary>
        /// Pivot around which scale and rotation are applied</summary>
        Pivot = 8,

        /// <summary>
        /// Uniform scale</summary>
        UniformScale = 16,
    }
}
