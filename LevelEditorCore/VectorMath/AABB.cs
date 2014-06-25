//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using Sce.Atf.VectorMath;

namespace LevelEditorCore.VectorMath
{
    /// <summary>
    ///  Axis aligned bounding box </summary>    
    public struct AABB
    {

        //public const int CornerCount = 8;
        //public static AABB CreateFromSphere(BoundingSphere sphere);
        //public static AABB CreateFromPoints(IEnumerable<Vector3> points);

        
         /// <summary>
        /// Minima of extents</summary>
        public Vec3F Min;

        /// <summary>
        /// Maxima of extents</summary>
        public Vec3F Max;

        /// <summary>
        /// Constructor with min and max</summary>
        /// <param name="min">Minima of extents</param>
        /// <param name="max">Maxima of extents</param>
        public AABB(Vec3F min, Vec3F max)
        {
            Min = min;
            Max = max;
            m_initialized = true;
        }

        /// <summary>
        /// Gets a value indicating if this box has zero-volume</summary>
        public bool IsEmpty
        {
            get { return (Min == Max); }
        }

        public Vec3F Radius
        {
            get { return (0.5f * (Max - Min)); }
        }
        /// <summary>
        /// Gets the centroid (geometrical center) of the box</summary>
        /// <remarks>Returns the origin for an empty box</remarks>
        public Vec3F Center
        {
            get { return (Min + Max)/2.0f; }
        }

        /// <summary>
        /// Extends box to contain the given point, or if this box is uninitialized, the box is
        /// initialized from the point</summary>
        /// <param name="p">Point</param>
        /// <returns>Extended box</returns>
        public void Extend(Vec3F p)
        {
           
            if (m_initialized == false)
            {
                Min = Max = p;
                m_initialized = true;
            }
            else
            {
                Min.X = Math.Min(Min.X, p.X);
                Min.Y = Math.Min(Min.Y, p.Y);
                Min.Z = Math.Min(Min.Z, p.Z);

                Max.X = Math.Max(Max.X, p.X);
                Max.Y = Math.Max(Max.Y, p.Y);
                Max.Z = Math.Max(Max.Z, p.Z);
            }           
        }

        /// <summary>
        /// Extends box to contain given array, interpreted as 3D points. 
        /// If this box is currently uninitialized, the box is initialized to the first point.</summary>
        /// <param name="v">Floats representing 3D points</param>
        /// <returns>Extended box</returns>
        public void Extend(IList<float> v)
        {
            if (v.Count >= 3)
            {
                if (!m_initialized)
                {
                    Max.X = Min.X = v[0];
                    Max.Y = Min.Y = v[1];
                    Max.Z = Min.Z = v[2];

                    m_initialized = true;
                }

                for (int i = 0; i < v.Count; i += 3)
                {
                    Min.X = Math.Min(Min.X, v[i]);
                    Min.Y = Math.Min(Min.Y, v[i + 1]);
                    Min.Z = Math.Min(Min.Z, v[i + 2]);

                    Max.X = Math.Max(Max.X, v[i]);
                    Max.Y = Math.Max(Max.Y, v[i + 1]);
                    Max.Z = Math.Max(Max.Z, v[i + 2]);
                }
            }            
        }

        /// <summary>
        /// Extends box to contain sphere, initializing this box if it is currently uninitialized</summary>
        /// <param name="sphere">Input sphere</param>
        /// <returns>The extended box</returns>
        public void Extend(Sphere3F sphere)
        {
            float r = sphere.Radius;
            Extend(sphere.Center + new Vec3F(r, r, r));
            Extend(sphere.Center - new Vec3F(r, r, r));            
        }

        /// <summary>
        /// Extends the box to contain the given box.
        /// If this box is currently uninitialized, sets this box to be the other box.</summary>
        /// <param name="other">The given box</param>
        /// <returns>The extended box</returns>
        public void Extend(AABB other)
        {
            if (!other.IsEmpty)
            {
                Extend(other.Min);
                Extend(other.Max);
            }            
        }

        public Sphere3F ToSphere()
        {
            Vec3F center = Center;
            float r = (Max - center).Length;
            return new Sphere3F(Center, r);                
        }
        /// <summary>
        /// Gets the object's string representation</summary>
        /// <returns>Object's string representation</returns>
        public override string ToString()
        {
            return Min + Environment.NewLine + Max;
        }

        /// <summary>
        /// Sets box to extents of original box, transformed by the given matrix</summary>
        /// <param name="M">Transformation matrix</param>
        public void Transform(Matrix4F xform)
        {
            Vec3F newMin = new Vec3F(xform.M41, xform.M42, xform.M43);
            Vec3F newMax = newMin;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    float e = Min[j] * xform[j, i];
                    float f = Max[j] * xform[j, i];
                    if (e < f)
                    {
                        newMin[i] += e;
                        newMax[i] += f;

                    }
                    else
                    {
                        newMin[i] += f;
                        newMax[i] += e;
                    }
                }
            }

            Min = newMin;
            Max = newMax;      
            m_initialized = true;
        }

        /// <summary>
        /// Returns an array of six floats representing this box</summary>
        /// <returns></returns>
        public float[] ToArray()
        {
            float[] temp = new float[6];
            temp[0] = Min.X;
            temp[1] = Min.Y;
            temp[2] = Min.Z;

            temp[3] = Max.X;
            temp[4] = Max.Y;
            temp[5] = Max.Z;
            return temp;
        }

        

        public bool Intersect(Ray3F r)
        {
            float tmin;
            float tmax;
            Vec3F pos;
            Vec3F norm;
            bool result = Intersect(r, out tmin,out tmax, out pos, out norm);
            return result;
        }
        public bool Intersect(Ray3F r, 
            out float out_tmin,
            out float out_tmax,
            out Vec3F out_pos, 
            out Vec3F out_nor)
        {
            out_pos = Vec3F.ZeroVector;
            out_nor = Vec3F.ZeroVector;
            out_tmin = 0.0f;
            out_tmax = 0.0f;

            Vec3F p = r.Origin;
            Vec3F d = r.Direction;
            float tmin = float.MinValue;
            float tmax = float.MaxValue;
            

            // check vs. all three 'slabs' of the aabb
            for (int i = 0; i < 3; ++i)
            {
                if (Math.Abs(d[i]) < float.Epsilon)
                {   // ray is parallel to slab, no hit if origin not within slab
                    if (p[i] < Min[i] || p[i] > Max[i])
                    {
                        return false;
                    }
                }
                else
                {
                    // compute intersection t values of ray with near and far plane of slab
                    float ood = 1.0f / d[i];
                    float t1 = (Min[i] - p[i]) * ood;
                    float t2 = (Max[i] - p[i]) * ood;
                    tmin = Math.Max(tmin, Math.Min(t1, t2));
                    tmax = Math.Min(tmax, Math.Max(t1, t2));

                    // exit with no collision as soon as slab intersection becomes empty
                    if (tmin > tmax)
                    {
                        return false;
                    }
                }
            }

            if (tmax < 0.0f)
            {
                // entire bounding box is behind us
                return false;
            }
            else if (tmin < 0.0f)
            {
                // we are inside the bounding box
                out_tmin = 0.0f;
                out_tmax = tmax;
                out_pos = p + d * tmax;
                out_nor = Vec3F.Normalize(Center - out_pos); // use 'sphere' type normal calculation to approximate.
                return true;
            }
            else
            {
                // ray intersects all 3 slabs. return point and normal of intersection
                out_tmin = tmin;
                out_pos = p + d * tmin;
                out_nor = Vec3F.Normalize(Center - out_pos); // use 'sphere' type normal calculation to approximate.
                return true;
            }
        }

       

        private bool m_initialized; //has been set with valid data; the box may still have zero-volume.
    }
}
