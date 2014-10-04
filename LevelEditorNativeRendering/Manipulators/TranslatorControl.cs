//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

using Sce.Atf.VectorMath;
using LevelEditorCore.VectorMath;

namespace RenderingInterop
{
    /// <summary>
    /// Reusable 3D Translator control</summary>
    public class TranslatorControl
    {
        public enum HitRegion
        {
            None,
            XAxis,
            YAxis,
            ZAxis,
            XYSquare,
            YZSquare,
            XZSquare,
        }

        public HitRegion Pick(Matrix4F world, Matrix4F view, Ray3F rayL, Ray3F rayV, float s)
        {
            m_hitRegion = HitRegion.None;
            m_hitRayV = rayV;

            m_hitMatrix.Set(world);
            m_hitWorldView = world * view;
            
            float sl = s * SquareLength;
           

            // test xy square.            
            Vec3F p1 = new Vec3F(0, 0, 0);
            Vec3F p2 = new Vec3F(sl, 0, 0);
            Vec3F p3 = new Vec3F(sl, sl, 0);
            Vec3F p4 = new Vec3F(0, sl, 0);
            Plane3F plane = new Plane3F(p1, p2, p3);
            Vec3F p;
            if (rayL.IntersectPlane(plane, out p))
            {
                // test point in 2d rectangle.
                if (p.X > p1.X && p.X < p2.X
                    && p.Y > p1.Y && p.Y < p4.Y)
                {
                    m_hitRegion = HitRegion.XYSquare;
                    return m_hitRegion;
                }

            }

            // test xz square
            p1 = new Vec3F(0, 0, 0);
            p2 = new Vec3F(sl, 0, 0);
            p3 = new Vec3F(sl, 0, sl);
            p4 = new Vec3F(0, 0, sl);
            plane = new Plane3F(p1, p2, p3);
            if (rayL.IntersectPlane(plane, out p))
            {
                // test point in 2d rectangle.
                if (p.X > p1.X && p.X < p2.X
                    && p.Z > p1.Z && p.Z < p4.Z)
                {
                    m_hitRegion = HitRegion.XZSquare;
                    return m_hitRegion;
                }

            }


            // test yz square
            p1 = new Vec3F(0, 0, 0);
            p2 = new Vec3F(0, 0, sl);
            p3 = new Vec3F(0, sl, sl);
            p4 = new Vec3F(0, sl, 0);
            plane = new Plane3F(p1, p2, p3);
            if (rayL.IntersectPlane(plane, out p))
            {
                // test point in 2d rectangle.
                if (p.Z > p1.Z && p.Z < p2.Z
                    && p.Y > p1.Z && p.Y < p4.Y)
                {
                    m_hitRegion = HitRegion.YZSquare;
                    return m_hitRegion;
                }

            }

            Vec3F min = new Vec3F(-0.5f, -0.5f, -0.5f);
            Vec3F max = new Vec3F(0.5f, 0.5f, 0.5f);
            AABB box = new AABB(min, max);
            Matrix4F boxScale = new Matrix4F();
            Matrix4F boxTrans = new Matrix4F();
            Matrix4F BoxMtrx = new Matrix4F();

            // X axis
            boxScale.Scale(new Vec3F(s, s * br, s * br));
            boxTrans.Translation = new Vec3F(s / 2, 0, 0);
            BoxMtrx = boxScale * boxTrans;

            Ray3F ray = rayL;
            BoxMtrx.Invert(BoxMtrx);
            ray.Transform(BoxMtrx);            
            if (box.Intersect(ray))
            {
                m_hitRegion = HitRegion.XAxis;
                return m_hitRegion;
            }

            // y axis
            boxScale.Scale(new Vec3F(s * br, s, s * br));
            boxTrans.Translation = new Vec3F(0, s / 2, 0);
            BoxMtrx = boxScale * boxTrans;
            ray = rayL;
            BoxMtrx.Invert(BoxMtrx);
            ray.Transform(BoxMtrx);
            if (box.Intersect(ray))
            {
                m_hitRegion = HitRegion.YAxis;
                return m_hitRegion;
            }

            // z axis
            boxScale.Scale(new Vec3F(s * br, s * br, s));
            boxTrans.Translation = new Vec3F(0, 0, s / 2);
            BoxMtrx = boxScale * boxTrans;

            ray = rayL;
            BoxMtrx.Invert(BoxMtrx);
            ray.Transform(BoxMtrx);
            if (box.Intersect(ray))
            {
                m_hitRegion = HitRegion.ZAxis;
            }

            return m_hitRegion;
        }


        public void Render(Matrix4F normWorld, float s)
        {
            BasicRendererFlags solid = BasicRendererFlags.Solid | BasicRendererFlags.DisableDepthTest;
            BasicRendererFlags wire = BasicRendererFlags.WireFrame | BasicRendererFlags.DisableDepthTest;

            Color xcolor = (m_hitRegion == HitRegion.XAxis || m_hitRegion == HitRegion.XYSquare || m_hitRegion == HitRegion.XZSquare) ? Color.Gold : Color.Red;
            Color ycolor = (m_hitRegion == HitRegion.YAxis || m_hitRegion == HitRegion.XYSquare || m_hitRegion == HitRegion.YZSquare) ? Color.Gold : Color.Green;
            Color Zcolor = (m_hitRegion == HitRegion.ZAxis || m_hitRegion == HitRegion.XZSquare || m_hitRegion == HitRegion.YZSquare) ? Color.Gold : Color.Blue;

            Color XYx = m_hitRegion == HitRegion.XYSquare ? Color.Gold : Color.Red;
            Color XYy = m_hitRegion == HitRegion.XYSquare ? Color.Gold : Color.Green;


            Color XZx = m_hitRegion == HitRegion.XZSquare ? Color.Gold : Color.Red;
            Color XZz = m_hitRegion == HitRegion.XZSquare ? Color.Gold : Color.Blue;


            Color YZy = m_hitRegion == HitRegion.YZSquare ? Color.Gold : Color.Green;
            Color YZz = m_hitRegion == HitRegion.YZSquare ? Color.Gold : Color.Blue;
            
            Vec3F axScale = new Vec3F(s, s, s);
            Matrix4F axisXform = ComputeAxis(normWorld, axScale);
            Util3D.RenderFlag = wire;
            Util3D.DrawX(axisXform, xcolor);
            Util3D.DrawY(axisXform, ycolor);
            Util3D.DrawZ(axisXform, Zcolor);

            Matrix4F arrowHead = ComputeXhead(normWorld, s);
            Util3D.RenderFlag = solid;

            Util3D.DrawCone(arrowHead, xcolor);

            arrowHead = ComputeYhead(normWorld, s);
            Util3D.DrawCone(arrowHead, ycolor);

            arrowHead = ComputeZhead(normWorld, s);
            Util3D.DrawCone(arrowHead, Zcolor);

            Util3D.RenderFlag = wire;
            // draw xy rect.
            Matrix4F scale = new Matrix4F();
            scale.Scale(axScale * SquareLength);
            Matrix4F trans = new Matrix4F();
            trans.Translation = new Vec3F(0, s * SquareLength, 0);
            Matrix4F squareXform = scale * trans * normWorld;
            Util3D.DrawX(squareXform, XYy);
            trans.Translation = new Vec3F(s * SquareLength, 0, 0);
            squareXform = scale * trans * normWorld;
            Util3D.DrawY(squareXform, XYx);

            // draw xz rect.
            trans.Translation = new Vec3F(0, 0, s * SquareLength);
            squareXform = scale * trans * normWorld;
            Util3D.DrawX(squareXform, XZz);

            trans.Translation = new Vec3F(s * SquareLength, 0, 0);
            squareXform = scale * trans * normWorld;
            Util3D.DrawZ(squareXform, XZx);

            // draw yz
            trans.Translation = new Vec3F(0, s * SquareLength, 0);
            squareXform = scale * trans * normWorld;
            Util3D.DrawZ(squareXform, YZy);

            trans.Translation = new Vec3F(0, 0, s * SquareLength);
            squareXform = scale * trans * normWorld;
            Util3D.DrawY(squareXform, YZz);

        }

        
        public Vec3F OnDragging(Ray3F rayV)
        {
            if (m_hitRegion == HitRegion.None)
                return Vec3F.ZeroVector;


            Vec3F xLocal = m_hitMatrix.XAxis;
            Vec3F yLocal = m_hitMatrix.YAxis;
            Vec3F zLocal = m_hitMatrix.ZAxis;

            Vec3F xAxis = m_hitWorldView.XAxis;
            Vec3F yAxis = m_hitWorldView.YAxis;
            Vec3F zAxis = m_hitWorldView.ZAxis;
            Vec3F origin = m_hitWorldView.Translation;

            Vec3F translate;
            float a1, a2;
            switch (m_hitRegion)
            {
                case HitRegion.XAxis:
                    {
                        a1 = Math.Abs(Vec3F.Dot(m_hitRayV.Direction, yAxis));
                        a2 = Math.Abs(Vec3F.Dot(m_hitRayV.Direction, zAxis));
                        Vec3F axis = (a1 > a2 ? yAxis : zAxis);
                        Vec3F p0 = m_hitRayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        Vec3F p1 = rayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        float dragAmount = Vec3F.Dot((p1 - p0), xAxis);
                        translate = dragAmount * xLocal;
                    }

                    break;
                case HitRegion.YAxis:
                    {
                        a1 = Math.Abs(Vec3F.Dot(m_hitRayV.Direction, zAxis));
                        a2 = Math.Abs(Vec3F.Dot(m_hitRayV.Direction, xAxis));
                        Vec3F axis = (a1 > a2 ? zAxis : xAxis);
                        Vec3F p0 = m_hitRayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        Vec3F p1 = rayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        float dragAmount = Vec3F.Dot((p1 - p0), yAxis);
                        translate = dragAmount * yLocal;
                    }
                    break;
                case HitRegion.ZAxis:
                    {
                        a1 = Math.Abs(Vec3F.Dot(m_hitRayV.Direction, xAxis));
                        a2 = Math.Abs(Vec3F.Dot(m_hitRayV.Direction, yAxis));
                        Vec3F axis = (a1 > a2 ? xAxis : yAxis);
                        Vec3F p0 = m_hitRayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        Vec3F p1 = rayV.IntersectPlane(axis, -Vec3F.Dot(axis, origin));
                        float dragAmount = Vec3F.Dot((p1 - p0), zAxis);
                        translate = dragAmount * zLocal;
                    }
                    break;
                case HitRegion.XYSquare:
                    {
                        Vec3F p0 = m_hitRayV.IntersectPlane(zAxis, -Vec3F.Dot(zAxis, origin));
                        Vec3F p1 = rayV.IntersectPlane(zAxis, -Vec3F.Dot(zAxis, origin));
                        Vec3F deltaLocal = p1 - p0;
                        float dragX = Vec3F.Dot(xAxis, deltaLocal);
                        float dragY = Vec3F.Dot(yAxis, deltaLocal);
                        translate = dragX * xLocal + dragY * yLocal;
                    }
                    break;
                case HitRegion.YZSquare:
                    {
                        Vec3F p0 = m_hitRayV.IntersectPlane(xAxis, -Vec3F.Dot(xAxis, origin));
                        Vec3F p1 = rayV.IntersectPlane(xAxis, -Vec3F.Dot(xAxis, origin));
                        Vec3F deltaLocal = p1 - p0;
                        float dragY = Vec3F.Dot(yAxis, deltaLocal);
                        float dragZ = Vec3F.Dot(zAxis, deltaLocal);
                        translate = dragY * yLocal + dragZ * zLocal;
                    }
                    break;
                case HitRegion.XZSquare:
                    {
                        Vec3F p0 = m_hitRayV.IntersectPlane(yAxis, -Vec3F.Dot(yAxis, origin));
                        Vec3F p1 = rayV.IntersectPlane(yAxis, -Vec3F.Dot(yAxis, origin));
                        Vec3F deltaLocal = p1 - p0;
                        float dragX = Vec3F.Dot(xAxis, deltaLocal);
                        float dragZ = Vec3F.Dot(zAxis, deltaLocal);
                        translate = dragX * xLocal + dragZ * zLocal;

                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return translate;
        }


        private Matrix4F ComputeAxis(Matrix4F normWorld, Vec3F axisScale)
        {
            Matrix4F scale = new Matrix4F();
            scale.Scale(axisScale);
            Matrix4F xform = scale * normWorld;
            return xform;
        }

        private Matrix4F ComputeXhead(Matrix4F normWorld, float s)
        {
            Matrix4F headrot = new Matrix4F();
            headrot.RotZ(-MathHelper.PiOver2);

            Matrix4F trans = new Matrix4F();
            trans.Translation = new Vec3F(s * (1 - hr), 0, 0);
            Matrix4F scale = new Matrix4F();
            scale.Scale(new Vec3F(s * br, s * hr, s * br));
            Matrix4F xform = scale * headrot * trans * normWorld;
            return xform;
        }

        private Matrix4F ComputeYhead(Matrix4F normWorld, float s)
        {
            Matrix4F trans = new Matrix4F();
            trans.Translation = new Vec3F(0, s * (1 - hr), 0);
            Matrix4F scale = new Matrix4F();
            scale.Scale(new Vec3F(s * br, s * hr, s * br));
            Matrix4F xform = scale * trans * normWorld;
            return xform;
        }

        private Matrix4F ComputeZhead(Matrix4F normWorld, float s)
        {
            Matrix4F headrot = new Matrix4F();
            headrot.RotX(MathHelper.PiOver2);

            Matrix4F trans = new Matrix4F();
            trans.Translation = new Vec3F(0, 0, s * (1 - hr));
            Matrix4F scale = new Matrix4F();
            scale.Scale(new Vec3F(s * br, s * hr, s * br));
            Matrix4F xform = scale * headrot * trans * normWorld;
            return xform;
        }


        private const float hr = 1.0f / 4.0f;
        private const float br = 1.0f / 15.0f;
        private const float SquareLength = 0.3f; //the ratio of square length to arrow length
        private Matrix4F m_hitWorldView = new Matrix4F();
        private Matrix4F m_hitMatrix = new Matrix4F();
        private Ray3F m_hitRayV;
        private HitRegion m_hitRegion = HitRegion.None;

    }

    
}
