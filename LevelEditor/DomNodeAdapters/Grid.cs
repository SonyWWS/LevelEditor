//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Rendering;
using Sce.Atf.VectorMath;
using Sce.Atf.Dom;

using LevelEditorCore;

namespace LevelEditor.DomNodeAdapters
{
    /// <summary>
    /// Grid, to help with placing objects</summary>
    public class Grid : DomNodeAdapter, IGrid 
    {        
        #region IGrid Members

        /// <summary>
        /// Gets or sets the size of the grid</summary>
        public float Size
        {
            get { return GetAttribute<float>(Schema.gridType.sizeAttribute); }
            set { SetAttribute(Schema.gridType.sizeAttribute, value);}
        }

        /// <summary>
        /// Gets or sets the number of sub-divisions</summary>
        public int Subdivisions
        {
            get { return GetAttribute<int>(Schema.gridType.subdivisionsAttribute); }
            set { SetAttribute(Schema.gridType.subdivisionsAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the grid's height (along the world's up vector)</summary>
        public float Height
        {
            get { return GetAttribute<float>(Schema.gridType.heightAttribute); }
            set { SetAttribute(Schema.gridType.heightAttribute, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether grid is visible</summary>
        public bool Visible
        {
            get { return GetAttribute<bool>(Schema.gridType.visibleAttribute); }
            set { SetAttribute(Schema.gridType.visibleAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the grid's axis system</summary>
        public Matrix4F AxisSystem
        {
            get { return m_axisSystem; }
            set
            {
                m_axisSystem = value;
                m_invAxisSystem.Invert(m_axisSystem);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to snap all objects to the grid</summary>
        public bool Snap
        {
            get { return GetAttribute<bool>(Schema.gridType.snapAttribute); }
            set { SetAttribute(Schema.gridType.snapAttribute, value); }
        }

        /// <summary>
        /// Snaps the given point to the nearest grid vertex</summary>
        /// <param name="pt">Point to snap, in world space</param>
        /// <returns>Point, from given point, snapped to grid, in world space</returns>
        public Vec3F SnapPoint(Vec3F pt)
        {
            float segment = Size / (float)Subdivisions;
            Vec3F snap = new Vec3F((int)(pt.X / segment), 0, (int)(pt.Z / segment));
            snap = snap * segment;
            snap.Y = Height;
            return snap;
        }

        /// <summary>
        /// Projects the specified x and y, in normalized window coordinates, onto the grid,
        /// and snaps it to the nearest grid vertex if necessary.
        /// Normalized window coordinates are in the range [-0.5,0.5] with +x pointing to the
        /// right and +y pointing up.</summary>
        /// <param name="x">Window x in normalized window coords</param>
        /// <param name="y">Window y in normalized window coords</param>
        /// <param name="camera">Camera</param>
        /// <returns>Projection of x and y onto the grid, in world space.</returns>
        public Vec3F Project(float x, float y, Camera camera)
        {
            Ray3F ray = camera.CreateRay(x, y);

            Matrix4F V = new Matrix4F(camera.ViewMatrix);
            V.Mul(m_invAxisSystem, V);

            if (camera.Frustum.IsOrtho)
            {
                V = new Matrix4F(m_V);
                V.Translation = camera.ViewMatrix.Translation;
            }

            // origin
            Vec3F delta = new Vec3F(0, Height, 0);
            V.Transform(delta, out delta);
            Vec3F o = delta;

            // Up vec
            Vec3F axis = V.YAxis;
            Vec3F projPt = ray.IntersectPlane(axis, -Vec3F.Dot(o, axis));

            // Transform back into world space
            Matrix4F Inv = new Matrix4F();
            Inv.Invert(camera.ViewMatrix);
            Inv.Transform(projPt, out projPt);

            if (Snap)
            {
                projPt = SnapPoint(projPt);
            }
            return projPt;
        }

        #endregion
        
        private Matrix4F m_V = new Matrix4F();        
        private Matrix4F m_axisSystem = new Matrix4F();
        private Matrix4F m_invAxisSystem = new Matrix4F();
    }
}
