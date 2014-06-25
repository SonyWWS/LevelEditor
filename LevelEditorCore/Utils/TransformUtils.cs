//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;
using LevelEditorCore.VectorMath;

using AxisSystemType = Sce.Atf.Rendering.Dom.AxisSystemType;

namespace LevelEditorCore
{
    /// <summary>
    /// 3D Transformation Utilities
    /// </summary>
    public static class TransformUtils
    {

        /// <summary>
        /// Decomposes the given matrix to translation, scale, 
        /// and rotation and set them to given Transformable node.        
        /// </summary>        
        public static void SetTransform(ITransformable xform, Matrix4F mtrx)
        {
            xform.Translation = mtrx.Translation;
            xform.Scale = mtrx.GetScale();
            Vec3F rot = new Vec3F();
            mtrx.GetEulerAngles(out rot.X, out rot.Y, out rot.Z);
            xform.Rotation = rot;
            xform.UpdateTransform();
        }

        /// <summary>
        /// Computes world transformation matrix for the given 
        /// Transformable node.</summary>        
        public static Matrix4F ComputeWorldTransform(ITransformable xform)
        {
            Matrix4F world = new Matrix4F();
            DomNode node = xform.As<DomNode>();
            foreach (DomNode n in node.Lineage)
            {
                ITransformable xformNode = n.As<ITransformable>();
                if (xformNode != null)
                {
                    world.Mul(world, xformNode.Transform);
                }
            }
            return world;
        }

        /// <summary>
        /// Calculates the world space matrix of the given path</summary>
        /// <param name="path">The path</param>
        /// <param name="start">Starting index</param>
        /// <param name="M">the world matrix</param>        
        public static void CalcPathTransform(Matrix4F M, Path<DomNode> path, int start)
        {           
            for (int i = start; i >= 0; i--)
            {
                if (path[i] != null)
                {
                    ITransformable renderable =
                        path[i].As<ITransformable>();

                    if (renderable != null)
                    {
                        M.Mul(M, renderable.Transform);
                    }
                }
            }            
        }

        /// <summary>
        /// Calculates the world space matrix of the given path
        /// </summary>
        /// <param name="path">The path</param>
        /// <param name="start">Starting index</param>
        /// <returns>The world space matrix</returns>
        public static Matrix4F CalcPathTransform(Path<DomNode> path, int start)
        {
            Matrix4F M = new Matrix4F();

            for (int i = start; i >= 0; i--)
            {
                if (path[i] != null)
                {
                    ITransformable renderable =
                        path[i].As<ITransformable>();

                    if (renderable != null)
                    {
                        M.Mul(M, renderable.Transform);
                    }
                }
            }

            return M;
        }

        public static Vec3F CalcSnapFromOffset(ITransformable node, SnapFromMode snapFrom)
        {            
            
            // AABB in local space.
            AABB box = node.As<IBoundable>().LocalBoundingBox;

            Vec3F offsetLocal = box.Center;
            switch (snapFrom)
            {
                case SnapFromMode.Pivot:
                    offsetLocal = node.Pivot;
                    break;
                case SnapFromMode.Origin:
                    offsetLocal = box.Center;
                    break;
                case SnapFromMode.TopCenter:
                    offsetLocal.Y = box.Max.Y;
                    break;
                case SnapFromMode.BottomCenter:
                    offsetLocal.Y = box.Min.Y;
                    break;
                case SnapFromMode.FrontCenter:
                    offsetLocal.Z = box.Max.Z;
                    break;
                case SnapFromMode.BackCenter:
                    offsetLocal.Z = box.Min.Z;
                    break;
                case SnapFromMode.LeftCenter:
                    offsetLocal.X = box.Min.X;
                    break;
                case SnapFromMode.RightCenter:
                    offsetLocal.X = box.Max.X;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Invalid snap-from node");
            }

            Path<DomNode> path = new Path<DomNode>(node.Cast<DomNode>().GetPath());
            Matrix4F toWorld = TransformUtils.CalcPathTransform(path, path.Count - 1);

            Vec3F offset;            
            toWorld.TransformVector(offsetLocal, out offset); //local-to-world           
            return offset; //world
        }

     
        /// <summary>
        /// Given an object's current Euler angles and a surface normal, will calculate
        ///  the Euler angles necessary to rotate the object so that it's up-vector is
        ///  aligned with the surface normal.
        /// </summary>
        /// <param name="originalEulers">From an IRenderableNode.Rotation, for example.</param>
        /// <param name="surfaceNormal">a unit vector that the object should be rotate-snapped to</param>
        /// <param name="upAxis">y or z is up?</param>
        /// <returns>The resulting angles to be assigned to IRenderableNode.Rotation</returns>
        /// <remarks>
        /// Note that QuatF was attempted to be used, but I could not get it to work reliably
        ///  with the Matrix3F.GetEulerAngles(). Numerical instability? The basis vector
        ///  method below works well except for when the target surface is 90 degrees different
        ///  than the starting up vector in which case the rotation around the up vector is lost
        ///  (gimbal lock) but the results are always valid in the sense that the up vector
        ///  is aligned with the surface normal. --Ron Little
        /// </remarks>
        public static Vec3F RotateToVector(Vec3F originalEulers, Vec3F surfaceNormal, AxisSystemType upAxis)
        {
            // get basis vectors for the current rotation
            Matrix3F rotMat = new Matrix3F();
            rotMat.Rotation(originalEulers);
            Vec3F a1 = rotMat.XAxis;
            Vec3F a2 = rotMat.YAxis;
            Vec3F a3 = rotMat.ZAxis;

            // calculate destination basis vectors
            Vec3F b1, b2, b3;
            if (upAxis == AxisSystemType.YIsUp)
            {
                // a2 is the current up vector. b2 is the final up vector.
                // now, find either a1 or a3, whichever is most orthogonal to surface
                b2 = new Vec3F(surfaceNormal);
                float a1DotS = Vec3F.Dot(a1, surfaceNormal);
                float a3DotS = Vec3F.Dot(a3, surfaceNormal);
                if (Math.Abs(a1DotS) < Math.Abs(a3DotS))
                {
                    b1 = new Vec3F(a1);
                    b3 = Vec3F.Cross(b1, b2);
                    b1 = Vec3F.Cross(b2, b3);
                }
                else
                {
                    b3 = new Vec3F(a3);
                    b1 = Vec3F.Cross(b2, b3);
                    b3 = Vec3F.Cross(b1, b2);
                }
            }
            else
            {
                // a3 is the current up vector. b3 is the final up vector.
                // now, find either a1 or a2, whichever is most orthogonal to surface
                b3 = new Vec3F(surfaceNormal);
                float a1DotS = Vec3F.Dot(a1, surfaceNormal);
                float a2DotS = Vec3F.Dot(a2, surfaceNormal);
                if (Math.Abs(a1DotS) < Math.Abs(a2DotS))
                {
                    b1 = new Vec3F(a1);
                    b2 = Vec3F.Cross(b3, b1);
                    b1 = Vec3F.Cross(b2, b3);
                }
                else
                {
                    b2 = new Vec3F(a2);
                    b1 = Vec3F.Cross(b2, b3);
                    b2 = Vec3F.Cross(b3, b1);
                }
            }

            // in theory, this isn't necessary, but in practice...
            b1.Normalize();
            b2.Normalize();
            b3.Normalize();

            // construct new rotation matrix and extract euler angles
            rotMat.XAxis = b1;
            rotMat.YAxis = b2;
            rotMat.ZAxis = b3;

            Vec3F newEulers = new Vec3F();
            rotMat.GetEulerAngles(out newEulers.X, out newEulers.Y, out newEulers.Z);
            return newEulers;
        }
             
        /// <summary>
        /// Calculates the transformation matrix corresponding to the given Renderable node</summary>
        /// <param name="node">Renderable node</param>
        /// <returns>transformation matrix corresponding to the node's transform components</returns>
        public static Matrix4F CalcTransform(ITransformable node)
        {
            return CalcTransform(
                node.Translation,
                node.Rotation,
                node.Scale,
                node.Pivot);
        }

        /// <summary>
        /// Calculates the transformation matrix corresponding to the given transform components
        /// </summary>
        /// <param name="translation">Translation</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="scale">Scale</param>
        /// <param name="scalePivot">Translation to origin of scaling</param>
        /// <param name="scalePivotTranslate">Translation after scaling</param>
        /// <param name="rotatePivot">Translation to origin of rotation</param>
        /// <param name="rotatePivotTranslate">Translation after rotation</param>
        /// <returns>transformation matrix corresponding to the given transform components</returns>
        public static Matrix4F CalcTransform(
            Vec3F translation,
            Vec3F rotation,
            Vec3F scale,
            Vec3F pivot)
        {
            
            Matrix4F M = new Matrix4F();
            Matrix4F temp = new Matrix4F();

            M.Set(-pivot);

            temp.Scale(scale);
            M.Mul(M, temp);

            if (rotation.X != 0)
            {
                temp.RotX(rotation.X);
                M.Mul(M, temp);
            }

            if (rotation.Y != 0)
            {
                temp.RotY(rotation.Y);
                M.Mul(M, temp);
            }

            if (rotation.Z != 0)
            {
                temp.RotZ(rotation.Z);
                M.Mul(M, temp);
            }

            temp.Set(pivot + translation);
            M.Mul(M, temp);

            return M;
        }
        
    }

}
