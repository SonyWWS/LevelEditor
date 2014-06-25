//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.VectorMath;

namespace LevelEditorCore
{
    /// <summary>
    /// Interface for objects that maintain 3D transformation information</summary>
    public interface ITransformable
    {

        /// <summary>
        /// Updates transformation matrix using the following transformable properties
        /// Translation, Rotation, Scale, and Pivot
        /// In most cases the Transform will be auto-updated        
        /// </summary>
        /// <remarks>You should manually call this method when creating new 
        /// GameObject and setting its Transformable properties</remarks>
        void UpdateTransform();

        /// <summary>
        /// Gets local transformation matrix.
        /// This is derived from the various
        /// components below.</summary>
        Matrix4F Transform
        {
            get;            
        }

        /// <summary>
        /// Gets and sets the node translation. Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector</remarks>
        Vec3F Translation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the node rotation. Check TransformationType before using 'set'</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector</remarks>
        Vec3F Rotation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the node scale. Check TransformationType before using 'set'</summary>
        /// <remarks>If unimplemented, 'get' should return new Vec3F{1,1,1}</remarks>
        Vec3F Scale
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the translation to origin of scaling. Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector</remarks>
        Vec3F Pivot
        {
            get;
            set;
        }
      
        /// <summary>
        /// Gets or sets what type of transformation this object can support. All of the transformation properties
        /// have a valid 'get', but check the appropriate flag before setting the property.</summary>
        TransformationTypes TransformationType
        {
            get;
            set;
        }
    }
}
