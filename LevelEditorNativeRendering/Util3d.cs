//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;


using Sce.Atf.VectorMath;
using Vector2 = Sce.Atf.VectorMath.Vec2F;
using Vector3 = Sce.Atf.VectorMath.Vec3F;
using Vector4 = Sce.Atf.VectorMath.Vec4F;

using LevelEditorCore.VectorMath;


namespace RenderingInterop
{
    // utility class for drawing simple 2d/3d shapes
    public static class Util3D
    {
        public static BasicRendererFlags RenderFlag
        {
            set { GameEngine.SetRendererFlag(value);}
        }

        public static void DrawSphere(Sphere3F sphere, Color c)
        {
            // create matrix from AABB            
            T1.Set(sphere.Center);
            float scale = 2*sphere.Radius;
            T1.M11 = scale;
            T1.M22 = scale;
            T1.M33 = scale;
            DrawSphere(T1, c);
            
        }

        public static ulong CaptionFont
        {
            get { return s_captionFont; }
        }

        public static void DrawPivot(Matrix4F xform, Color c)
        {
            GameEngine.DrawPrimitive(PrimitiveType.LineStrip,
              s_pivotVerts,
              0,
              s_pivotVertexCount,
              c,
              xform);            
        }
        public static void DrawAABB(AABB bound)
        {
            // create matrix from AABB
            Vec3F trans = bound.Center;
            Vec3F diag = bound.Max - bound.Min;            
            T1.Set(trans);
            T1.M11 = diag.X;
            T1.M22 = diag.Y;
            T1.M33 = diag.Z;

            GameEngine.DrawIndexedPrimitive(PrimitiveType.LineList,
               s_boxVertsId,
               s_boxIndicesId,
               0,
               s_boxIndicesCount,
               0,
               Color.White,
               T1);
        }

        public static void DrawCircle(Matrix4F xform, Color color)
        {            
            GameEngine.DrawPrimitive(PrimitiveType.LineStrip,
               s_circleVerts,
               0,
               s_circleVertexCount,
               color,
               xform);
        }
        public static void DrawUnitSquare(Matrix4F xform, Color color)
        {
            GameEngine.DrawPrimitive(PrimitiveType.LineList,
                s_linesVertId,
                s_unitSquareStartVertex,
                s_unitSquareVertexCount,
                color,
                xform);
        }

        public static void DrawRect(Matrix4F xform, Color color)
        {
            GameEngine.DrawPrimitive(PrimitiveType.LineList,
                s_linesVertId,
                s_rectStartVertex,
                s_rectVertexCount,
                color,
                xform);
            
        }
        
        public static void DrawAxis(Matrix4F xform, Color color)
        {
            GameEngine.DrawPrimitive(PrimitiveType.LineList,
                s_linesVertId,
                s_axisStartVertex,
                s_axisVertexCount,
                color,
                xform);
        }


        public static void DrawX(Matrix4F xform, Color color)
        {

            GameEngine.DrawPrimitive(PrimitiveType.LineList,
                                     s_linesVertId,
                                     s_axisStartVertex,
                                     2,
                                     color,
                                     xform);

        }

        public static void DrawY(Matrix4F xform, Color color)
        {
            GameEngine.DrawPrimitive(PrimitiveType.LineList,
                                     s_linesVertId,
                                     s_axisStartVertex + 2,
                                     2,
                                     color,
                                     xform);

        }

        public static void DrawZ(Matrix4F xform, Color color)
        {
            GameEngine.DrawPrimitive(PrimitiveType.LineList,
                                     s_linesVertId,
                                     s_axisStartVertex + 4,
                                     2,
                                     color,
                                     xform);

        }

        public static void DrawSphere(Matrix4F xform, System.Drawing.Color color)
        {            
            GameEngine.DrawIndexedPrimitive(PrimitiveType.TriangleList,
                s_sphereVertId,
                s_sphereIndexId,
                0,
                s_sphereIndexCount,
                0,
                color,
                xform);

        }


        // few constants used for creating Ring (a thin torus).
        public const float RingInnerRadias = 0.47f;
        public const float RingOuterRadias = 0.5f;
        public const float RingThickness = (RingOuterRadias - RingInnerRadias);
        public const float RingCenterRadias = 0.5f * (RingInnerRadias + RingOuterRadias);

        public static void DrawRing(Matrix4F xform, Color color)
        {            
            GameEngine.DrawIndexedPrimitive(PrimitiveType.TriangleList,
               s_torusVertId,
               s_torusIndexId,
               0,
               s_torusIndexCount,
               0,
               color,
               xform);
        }
        public static void DrawCylinder(Matrix4F xform, Color color)
        {
            GameEngine.DrawIndexedPrimitive(PrimitiveType.TriangleList,
               s_cylinderVertId,
               s_cylinderIndexId,
               0,
               s_cylinderIndexCount,
               0,
               color,
               xform);
        }

        public static void DrawCone(Matrix4F xform, Color color)
        {                                   
            GameEngine.DrawIndexedPrimitive(PrimitiveType.TriangleList,
               s_coneVertId,
               s_coneIndexId,
               0,
               s_coneIndexCount,
               0,
               color,
               xform);
        }
      
        public static void DrawCube(Matrix4F xform, Color color)
        {
            GameEngine.DrawIndexedPrimitive(PrimitiveType.TriangleList,
               s_cubeVertId,
               s_cubeIndexId,
               0,
               s_cubeIndexCount,
               0,
               color,
               xform);

        }

        public static void Init()
        {
            if (s_inited) return;


            List<VertexPN> vertList = new List<VertexPN>();


            List<Vector3> spherePositions = new List<Vec3F>();
            List<Vector3> sphereNormals = new List<Vec3F>();
            List<uint> sphereIndices = new List<uint>();                        
            GeometryHelper.CreateSphere(0.5f,FastSphereSlices,FastSphereStacks, spherePositions
                ,  sphereNormals, null, sphereIndices);


            GeometryHelper.AssembleVertexPN(spherePositions, sphereNormals, vertList);

            s_sphereVertId = GameEngine.CreateVertexBuffer(vertList.ToArray());
            s_sphereIndexId = GameEngine.CreateIndexBuffer(sphereIndices.ToArray());
            s_sphereIndexCount = (uint) sphereIndices.Count;
            
            List<Vec3F> conePos = new List<Vec3F>();
            List<uint> coneIndices = new List<uint>();
            List<Vec3F> coneNorms = new List<Vec3F>();
            GeometryHelper.CreateCylinder(1.0f, 0.0f, 1.0f, 16, 1, conePos, coneNorms, coneIndices);
            s_coneIndexCount = (uint)coneIndices.Count;

            GeometryHelper.AssembleVertexPN(conePos, coneNorms, vertList);
            s_coneVertId = GameEngine.CreateVertexBuffer(vertList.ToArray());
            s_coneIndexId = GameEngine.CreateIndexBuffer(coneIndices.ToArray());


            // create unit cylinder
            List<Vec3F> cyPos = new List<Vec3F>();
            List<uint> cyIndices = new List<uint>();
            List<Vec3F> cyNorms = new List<Vec3F>();
            GeometryHelper.CreateCylinder(0.5f, 0.5f, 1.0f, 16, 2, cyPos, cyNorms, cyIndices);
            GeometryHelper.AssembleVertexPN(cyPos, cyNorms, vertList);
            s_cylinderVertId = GameEngine.CreateVertexBuffer(vertList.ToArray());
            s_cylinderIndexId = GameEngine.CreateIndexBuffer(cyIndices.ToArray());
            s_cylinderIndexCount =(uint)cyIndices.Count;

            // create slim unit torus.
            List<Vec3F> torPos = new List<Vec3F>();            
            List<Vec3F> torNorms = new List<Vec3F>();
            List<uint> torIndices = new List<uint>();
            GeometryHelper.CreateTorus(RingInnerRadias, RingOuterRadias, 40, 6, torPos, torNorms, null, torIndices);
            GeometryHelper.AssembleVertexPN(torPos, torNorms, vertList);
            s_torusVertId = GameEngine.CreateVertexBuffer(vertList.ToArray());
            s_torusIndexId = GameEngine.CreateIndexBuffer(torIndices.ToArray());
            s_torusIndexCount = (uint)torIndices.Count;



            List<Vector3> cubePos = new List<Vector3>();
            List<Vector3> cubeNormals = new List<Vector3>();
            
            List<uint> cubeIndices = new List<uint>();

            GeometryHelper.CreateUnitCube(cubePos,cubeNormals,null,cubeIndices);
            GeometryHelper.AssembleVertexPN(cubePos, cubeNormals, vertList);

            s_cubeVertId = GameEngine.CreateVertexBuffer(vertList.ToArray());
            s_cubeIndexId = GameEngine.CreateIndexBuffer(cubeIndices.ToArray());
            s_cubeIndexCount = (uint) cubeIndices.Count;


            var linePos = new List<Vec3F>();

            linePos.Add(new Vector3(0, 0, 0));
            linePos.Add(new Vector3(1, 0, 0));

            linePos.Add(new Vector3(0, 0, 0));
            linePos.Add(new Vector3(0, 1, 0));

            linePos.Add(new Vector3(0, 0, 0));
            linePos.Add(new Vector3(0, 0, 1));

            linePos.Add(new Vector3(0, 0, 0));
            linePos.Add(new Vector3(-1, 0, 0));

            linePos.Add(new Vector3(0, 0, 0));
            linePos.Add(new Vector3(0, -1, 0));

            linePos.Add(new Vector3(0, 0, 0));
            linePos.Add(new Vector3(0, 0, -1));

            s_axisStartVertex = 0;
            s_axisVertexCount = (uint)linePos.Count;

            // rect pos;
            linePos.Add(new Vector3(1, 1, 0));
            linePos.Add(new Vector3(-1, 1, 0));

            linePos.Add(new Vector3(-1, 1, 0));
            linePos.Add(new Vector3(-1, -1, 0));

            linePos.Add(new Vector3(-1, -1, 0));
            linePos.Add(new Vector3(1, -1, 0));

            linePos.Add(new Vector3(1, -1, 0));
            linePos.Add(new Vector3(1, 1, 0));

            s_rectVertexCount = 8;
            s_rectStartVertex = (uint)linePos.Count - s_rectVertexCount;

            // unit square 
            linePos.Add(new Vector3(0.5f, 0.5f, 0));
            linePos.Add(new Vector3(-0.5f, 0.5f, 0));

            linePos.Add(new Vector3(-0.5f, 0.5f, 0));
            linePos.Add(new Vector3(-0.5f, -0.5f, 0));

            linePos.Add(new Vector3(-0.5f, -0.5f, 0));
            linePos.Add(new Vector3(0.5f, -0.5f, 0));

            linePos.Add(new Vector3(0.5f, -0.5f, 0));
            linePos.Add(new Vector3(0.5f, 0.5f, 0));

            s_unitSquareVertexCount = 8;
            s_unitSquareStartVertex = (uint) linePos.Count - s_unitSquareVertexCount;

            s_linesVertId = GameEngine.CreateVertexBuffer(linePos.ToArray());

            List<Vec3F> circlePos = new List<Vec3F>();
            GeometryHelper.CreateCircle(1.0f,32,circlePos);
            s_circleVertexCount = (uint) circlePos.Count;
            s_circleVerts = GameEngine.CreateVertexBuffer(circlePos.ToArray());
            
            List<Vec3F>  boxVerts = new List<Vec3F>();
            List<uint> boxIndices = new List<uint>();
            GeometryHelper.CreateUnitBox(boxVerts,boxIndices);
            s_boxVertsId = GameEngine.CreateVertexBuffer(boxVerts.ToArray());
            s_boxIndicesId = GameEngine.CreateIndexBuffer(boxIndices.ToArray());
            s_boxIndicesCount =(uint)boxIndices.Count;



            List<Vec3F> pivotVerts = new List<Vec3F>();
            GeometryHelper.CreateCircle(0.5f, 16, pivotVerts);
            GeometryHelper.CreateCircle(0.375f, 16, pivotVerts);
            GeometryHelper.CreateCircle(0.25f, 16, pivotVerts);
            GeometryHelper.CreateCircle(0.125f, 16, pivotVerts);
            s_pivotVerts = GameEngine.CreateVertexBuffer(pivotVerts.ToArray());
            s_pivotVertexCount = (uint)pivotVerts.Count;


            s_captionFont = GameEngine.CreateFont("Arial", 14, FontStyle.BOLD);

            s_inited = true;
        }

        public static void Shutdown()
        {         
            GameEngine.DeleteBuffer(s_torusVertId);
            GameEngine.DeleteBuffer(s_torusIndexId);
            GameEngine.DeleteBuffer(s_cylinderVertId );
            GameEngine.DeleteBuffer(s_cylinderIndexId);
            GameEngine.DeleteBuffer(s_circleVerts);
            GameEngine.DeleteBuffer(s_sphereVertId);
            GameEngine.DeleteBuffer(s_sphereIndexId);
            GameEngine.DeleteBuffer(s_coneVertId);
            GameEngine.DeleteBuffer(s_coneIndexId);                        
            GameEngine.DeleteBuffer(s_cubeVertId);
            GameEngine.DeleteBuffer(s_cubeIndexId);            
            GameEngine.DeleteBuffer(s_linesVertId);
            GameEngine.DeleteBuffer(s_boxVertsId);
            GameEngine.DeleteBuffer(s_boxIndicesId);
            GameEngine.DeleteBuffer(s_pivotVerts);            
            GameEngine.DeleteFont(s_captionFont);            
        }
        private static bool s_inited;

        private static ulong s_circleVerts;
        private static uint s_circleVertexCount;

        private static ulong s_pivotVerts;
        private static uint s_pivotVertexCount;


        private static ulong s_sphereVertId;
        private static ulong s_sphereIndexId;
        private static uint s_sphereIndexCount;

        private static ulong s_torusVertId;
        private static ulong s_torusIndexId;
        private static uint s_torusIndexCount;

       
        private static ulong s_coneVertId;
        private static ulong s_coneIndexId;
        private static uint s_coneIndexCount;

        private static ulong s_cylinderVertId;
        private static ulong s_cylinderIndexId;
        private static uint s_cylinderIndexCount;

        private static ulong s_boxVertsId;
        private static ulong s_boxIndicesId;
        private static uint  s_boxIndicesCount;


        private static ulong s_cubeVertId;
        private static ulong s_cubeIndexId;
        private static uint  s_cubeIndexCount;

        private static ulong s_linesVertId;
        private static uint s_axisStartVertex;
        private static uint s_axisVertexCount;

        private static uint s_rectStartVertex;
        private static uint s_rectVertexCount;

        private static uint s_unitSquareStartVertex;
        private static uint s_unitSquareVertexCount;

        private const int FastSphereSlices = 16;
        private const int FastSphereStacks = 16;

        private static ulong s_captionFont;

        // few temp matrices.
        // this no longer be needed if/when 
        // Matrix4F converted to struct.
        private static Matrix4F T1 = new Matrix4F();
       // private static Matrix4F T2 = new Matrix4F();
       // private static Matrix4F T3 = new Matrix4F();
       // private static Matrix4F T4 = new Matrix4F();
        
    }

    public static class GeometryHelper
    {

        public static void AssembleVertexPN(List<Vec3F> pos, List<Vec3F> norm,
            List<VertexPN> outVertList)
        {
            if (pos.Count != norm.Count)
                throw new ArgumentException("pos count not equal norm count");
            outVertList.Clear();
            VertexPN vertex = new VertexPN();
            for(int k = 0; k < pos.Count; k++)
            {
                vertex.Position = pos[k];
                vertex.Normal = norm[k];
                outVertList.Add(vertex);
            }
        }
        /// <summary>
        /// Create circle from linestrip.</summary>        
        public static void CreateCircle(float radius, uint segs, List<Vector3> pos)
        {
            float step = MathHelper.TwoPi/(float)segs;
            float theta = 0;
            for (int i = 0; i < segs; i++,theta+=step)
            {
                float x = radius*(float) Math.Cos(theta);
                float y = radius*(float) Math.Sin(theta);
                pos.Add(new Vector3(x,y,0));
            }
            pos.Add(pos[0]); // close the loop.
        }

        /// <summary>
        /// Create bi-polar sphere
        /// </summary>
        /// <param name="radius">radias of the sphere</param>
        /// <param name="slices">number of slices</param>
        /// <param name="stacks">number of stacks</param>
        /// <param name="pos">output positions</param>
        /// <param name="normal">output normals</param>
        /// <param name="tex">output texture coordinates</param>
        /// <param name="indices">output indices</param>
        public static void CreateSphere(float radius, uint slices, uint stacks,
          List<Vector3> pos, List<Vector3> normal, List<Vector2> tex, List<uint> indices)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException("radius");
            if (slices < 2 || stacks < 2)
                throw new ArgumentException("invalid number slices or stacks");
            
            // caches sin cos.
            float[] cosPhi = new float[stacks];
            float[] sinPhi = new float[stacks];
            float phiStep = MathHelper.Pi / stacks;

            float[] cosTheta = new float[slices];
            float[] sinTheta = new float[slices];
            float thetaStep = MathHelper.TwoPi / slices;

            float phi = 0;
            for (int s = 0; s < stacks; s++, phi += phiStep)
            {
                sinPhi[s] = (float)Math.Sin(phi);
                cosPhi[s] = (float)Math.Cos(phi);
            }

            float theta = 0;
            for (int s = 0; s < slices; s++, theta += thetaStep)
            {
                sinTheta[s] = (float)Math.Sin(theta);
                cosTheta[s] = (float)Math.Cos(theta);
            }

            uint numVerts = 2 + (stacks - 1) * slices;
            
            Vector3 northPole = new Vector3(0, radius, 0);
            pos.Add(northPole); // north pole.            
            normal.Add(Vector3.Normalize(northPole)); // north pole.
            
            for (int s = 1; s < stacks; s++)
            {
                float y = radius * cosPhi[s];
                float r = radius * sinPhi[s];
                for (int l = 0; l < slices; l++)
                {
                    Vector3 p;
                    p.Y = y;
                    p.Z = r * cosTheta[l];
                    p.X = r * sinTheta[l];
                    pos.Add(p);                    
                    if(normal != null)
                        normal.Add(Vector3.Normalize(p));                    
                }
            }
            Vector3 southPole = new Vector3(0, -radius, 0);
            pos.Add(southPole);
            normal.Add(Vector3.Normalize(southPole));


            // 2l + (s-2) * 2l
            // 2l * ( 1 + s-2)
            // 2l * ( s- 1)
            //
            uint numTris = 2 * slices * (stacks - 1);
            

            // create index of north pole cap.
            for (uint l = 1; l < slices; l++)
            {
                indices.Add(l + 1);
                indices.Add(0);
                indices.Add(l);
            }
            indices.Add(1);
            indices.Add(0);
            indices.Add(slices);

            for (uint s = 0; s < (stacks - 2); s++)
            {
                uint l = 1;
                for (; l < slices; l++)
                {
                    indices.Add( (s + 1) * slices + l + 1); // bottom right.
                    indices.Add( s * slices + l + 1); // top right.
                    indices.Add(s * slices + l); // top left.

                    indices.Add( (s + 1) * slices + l); // bottom left.
                    indices.Add( (s + 1) * slices + l + 1); // bottom right.
                    indices.Add( s * slices + l); // top left.

                }

                indices.Add( (s + 1) * slices + 1); // bottom right.
                indices.Add( s * slices + 1); // top right.
                indices.Add( s * slices + slices); // top left.


                indices.Add( (s + 1) * slices + slices); // bottom left.
                indices.Add( (s + 1) * slices + 1); // bottom right.
                indices.Add( s * slices + slices); // top left.


            }

            // create index for south pole cap.
            uint baseIndex = slices * (stacks - 2);
            uint lastIndex = (uint)pos.Count - 1;
            for (uint l = 1; l < slices; l++)
            {
                indices.Add( baseIndex + l);
                indices.Add( lastIndex);
                indices.Add( baseIndex + l + 1);
            }
            indices.Add(baseIndex + slices);
            indices.Add(lastIndex);
            indices.Add(baseIndex + 1);            
        }

        public static void CreateCone(float rad,float height, uint slices, uint stacks,
            List<Vector3> pos, List<Vector3> normal, List<uint> indices)
        {
            CreateCylinder(rad, 0, height, slices, stacks, pos, normal, indices);
        }

        public static void CreateCylinder(float rad1, float rad2,float height, uint slices, uint stacks,
            List<Vector3> pos, List<Vector3> normal, List<uint> indices)
        {

            float stackHeight = height / stacks;

            // Amount to increment radius as we move up each stack level from bottom to top.
            float radiusStep = (rad2 - rad1) / stacks;

            uint numRings = stacks+1;

            // Compute vertices for each stack ring.
            for (uint i = 0; i < numRings; ++i)
            {
                float y = i*stackHeight;
                float r = rad1 + i*radiusStep;

                // Height and radius of next ring up.
                float y_next =  (i + 1)*stackHeight;
                float r_next = rad1 + (i + 1)*radiusStep;

                // vertices of ring
                float dTheta = 2.0f*MathHelper.Pi/slices;
                for (uint j = 0; j <= slices; ++j)
                {
                    float c = (float) Math.Cos(j*dTheta);
                    float s = (float) Math.Sin(j*dTheta);

                    // tex coord if needed.
                    //float u = j/(float) slices;
                    //float v = 1.0f - (float) i/stacks;

                    // Partial derivative in theta direction to get tangent vector (this is a unit vector).
                    Vector3 T = new Vector3(-s, 0.0f, c);

                    // Compute tangent vector down the slope of the cone (if the top/bottom 
                    // radii differ then we get a cone and not a true cylinder).
                    Vector3 P = new Vector3(r*c, y, r*s);
                    Vector3 P_next = new Vector3(r_next*c, y_next, r_next*s);
                    Vector3 B = P - P_next;
                    B.Normalize();


                    Vector3 N = Vector3.Cross(T, B);
                    N.Normalize();

                    P.Z *= -1;
                    N.Z *= -1;

                    pos.Add(P);
                    if (normal != null)
                        normal.Add(N);
                }
            }

            uint numRingVertices = slices+1;

            // Compute indices for each stack.
            for(uint i = 0; i < stacks; ++i)
            {
                for (uint j = 0; j < slices; ++j)
                {
                    indices.Add(i*numRingVertices + j);                    
                    indices.Add((i + 1)*numRingVertices + j + 1);
                    indices.Add((i + 1) * numRingVertices + j);

                    indices.Add(i*numRingVertices + j);                    
                    indices.Add(i*numRingVertices + j + 1);
                    indices.Add((i + 1) * numRingVertices + j + 1);
                }
            }

            // build bottom cap.
            if(rad1 > 0)
            {
                uint baseIndex = (uint) pos.Count;


                // Duplicate cap vertices because the texture coordinates and normals differ.
                float y = 0;

                // vertices of ring
                float dTheta = 2.0f * MathHelper.Pi / slices;
                for (uint i = 0; i <= slices; ++i)
                {
                    float x = rad1 * (float)Math.Cos(i * dTheta);
                    float z = rad1 * (float)Math.Sin(i * dTheta);

                    // Map [-1,1]-->[0,1] for planar texture coordinates.
                    //float u = +0.5f * x / mBottomRadius + 0.5f;
                    //float v = -0.5f * z / mBottomRadius + 0.5f;
                    Vector3 p = new Vec3F(x,y,-z);
                    pos.Add(p);
                    if(normal != null)
                        normal.Add(new Vec3F(0.0f, -1.0f, 0.0f));
                }


                // cap center vertex
                pos.Add( new Vec3F(0.0f, y, 0.0f));
                if(normal != null)
                        normal.Add(new Vec3F(0.0f, -1.0f, 0.0f));
                // tex coord for center cap 0.5f, 0.5f


                // index of center vertex
                uint centerIndex = (uint)pos.Count - 1;

                for (uint i = 0; i < slices; ++i)
                {
                    indices.Add(centerIndex);
                    indices.Add(baseIndex + i + 1);
                    indices.Add(baseIndex + i);
                    
                }
            }

            // build top cap.
            if(rad2 > 0)
            {
                uint baseIndex = (uint) pos.Count;

                // Duplicate cap vertices because the texture coordinates and normals differ.
                float y =  height;

                // vertices of ring
                float dTheta = 2.0f * MathHelper.Pi / slices;
                for (uint i = 0; i <= slices; ++i)
                {
                    float x = rad2 * (float)Math.Cos(i * dTheta);
                    float z = rad2 * (float)Math.Sin(i * dTheta);

                    // Map [-1,1]-->[0,1] for planar texture coordinates.
                    //float u = +0.5f * x / mTopRadius + 0.5f;
                    //float v = -0.5f * z / mTopRadius + 0.5f;
                    pos.Add(new Vec3F(x, y, -z));
                    if(normal != null)
                        normal.Add( new Vec3F(0.0f, 1.0f, 0.0f) );                    
                }

                // pos, norm, tex1 for cap center vertex
                pos.Add( new Vec3F(0.0f, y, 0.0f));
                if(normal != null)
                    normal.Add(new Vec3F(0.0f, 1.0f, 0.0f));
                // tex coord 0.5f, 0.5f
                     
               
                // index of center vertex
                uint centerIndex = (uint)pos.Count - 1;
                for (uint i = 0; i < slices; ++i)
                {
                    indices.Add(centerIndex);                    
                    indices.Add(baseIndex + i);
                    indices.Add(baseIndex + i + 1);
                }

            }

        }


        public static void CreateTorus(float innerRadius,
            float outerRadius,
            uint rings,
            uint sides,
            List<Vec3F> pos,
            List<Vec3F> nor,
            List<Vec2F> tex,
            List<uint> indices)
        {

            uint ringStride = rings + 1;
            uint sideStride = sides + 1;

            // radiusC: distance to center of the ring
            float radiusC = (innerRadius + outerRadius) * 0.5f;

            //radiusR: the radius of the ring
            float radiusR = (outerRadius - radiusC);

            for (uint i = 0; i <= rings; i++)
            {
                float u = (float)i / rings;

                float outerAngle = i * MathHelper.TwoPi / rings;

                // xform from ring space to torus space.
                Matrix4F trans = new Matrix4F();
                trans.Translation = new Vec3F(radiusC, 0, 0);
                Matrix4F roty = new Matrix4F();
                roty.RotY(outerAngle);
                Matrix4F transform = trans * roty;

                // create vertices for each ring.
                for (uint j = 0; j <= sides; j++)
                {
                    float v = (float)j / sides;

                    float innerAngle = j * MathHelper.TwoPi / sides + MathHelper.Pi;
                    float dx = (float)Math.Cos(innerAngle);
                    float dy = (float)Math.Sin(innerAngle);

                    // normal, position ,and texture coordinates
                    Vec3F n = new Vec3F(dx, dy, 0);
                    Vec3F p = n * radiusR;

                    if (tex != null)
                    {
                        Vec2F t = new Vec2F(u, v);
                        tex.Add(t);
                    }

                    transform.Transform(ref p);
                    transform.TransformVector(n, out n);

                    pos.Add(p);
                    nor.Add(n);
                    

                    // And create indices for two triangles.
                    uint nextI = (i + 1) % ringStride;
                    uint nextJ = (j + 1) % sideStride;

                    indices.Add(nextI * sideStride + j);
                    indices.Add(i * sideStride + nextJ);
                    indices.Add(i * sideStride + j);

                    indices.Add(nextI * sideStride + j);
                    indices.Add(nextI * sideStride + nextJ);
                    indices.Add(i * sideStride + nextJ);
                }
            }
        }


        public static void CreateUnitBox(List<Vector3> pos, List<uint> indices)
        {
           
            // corner vertices
            // top verts

            pos.Add(new Vec3F(-0.5f, 0.5f, -0.5f)); // 0
            pos.Add(new Vec3F(0.5f, 0.5f, -0.5f));  // 1
            pos.Add(new Vec3F(0.5f, 0.5f, 0.5f));   // 2
            pos.Add(new Vec3F(-0.5f, 0.5f, 0.5f));  // 3
            // bottoms verts.
            pos.Add(new Vec3F(-0.5f, -0.5f, -0.5f)); // 4
            pos.Add(new Vec3F(0.5f, -0.5f, -0.5f));  // 5
            pos.Add(new Vec3F(0.5f, -0.5f, 0.5f));   // 6
            pos.Add(new Vec3F(-0.5f, -0.5f, 0.5f));  // 7


            // create indices for line list.
            // top
            indices.Add(0); indices.Add(1);
            indices.Add(1); indices.Add(2);
            indices.Add(2); indices.Add(3);
            indices.Add(3); indices.Add(0);
            // bottom
            indices.Add(4); indices.Add(5);
            indices.Add(5); indices.Add(6);
            indices.Add(6); indices.Add(7);
            indices.Add(7); indices.Add(4);

            // front
            indices.Add(3); indices.Add(2);
            indices.Add(2); indices.Add(6);
            indices.Add(6); indices.Add(7);
            indices.Add(7); indices.Add(3);

            // back
            indices.Add(0); indices.Add(1);
            indices.Add(1); indices.Add(5);
            indices.Add(5); indices.Add(4);
            indices.Add(4); indices.Add(0);



        }

        public static void CreateUnitCube(List<Vector3> pos, List<Vector3> normal, List<Vector2> tex, List<uint> indices)
        {
            VertexPNT[] v = new VertexPNT[24];

            // Fill in the front face vertex data.
            v[0] = new VertexPNT(-0.5f, -0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 0.0f, 1.0f);
            v[1] = new VertexPNT(-0.5f,  0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f);
            v[2] = new VertexPNT( 0.5f,  0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 1.0f, 0.0f);
            v[3] = new VertexPNT( 0.5f, -0.5f, -0.5f, 0.0f, 0.0f, -1.0f, 1.0f, 1.0f);

            // Fill in the back face vertex data.
            v[4] = new VertexPNT(-0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f);
            v[5] = new VertexPNT( 0.5f, -0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f);
            v[6] = new VertexPNT( 0.5f,  0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
            v[7] = new VertexPNT(-0.5f,  0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f);

            // Fill in the top face vertex data.
            v[8]  = new VertexPNT(-0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f);
            v[9]  = new VertexPNT(-0.5f, 0.5f,  0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f);
            v[10] = new VertexPNT( 0.5f, 0.5f,  0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f);
            v[11] = new VertexPNT( 0.5f, 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f);

            // Fill in the bottom face vertex data.
            v[12] = new VertexPNT(-0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f, 1.0f, 1.0f);
            v[13] = new VertexPNT( 0.5f, -0.5f, -0.5f, 0.0f, -1.0f, 0.0f, 0.0f, 1.0f);
            v[14] = new VertexPNT( 0.5f, -0.5f,  0.5f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f);
            v[15] = new VertexPNT(-0.5f, -0.5f,  0.5f, 0.0f, -1.0f, 0.0f, 1.0f, 0.0f);

            // Fill in the left face vertex data.
            v[16] = new VertexPNT(-0.5f, -0.5f,  0.5f, -1.0f, 0.0f, 0.0f, 0.0f, 1.0f);
            v[17] = new VertexPNT(-0.5f,  0.5f,  0.5f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f);
            v[18] = new VertexPNT(-0.5f,  0.5f, -0.5f, -1.0f, 0.0f, 0.0f, 1.0f, 0.0f);
            v[19] = new VertexPNT(-0.5f, -0.5f, -0.5f, -1.0f, 0.0f, 0.0f, 1.0f, 1.0f);

            // Fill in the right face vertex data.
            v[20] = new VertexPNT( 0.5f, -0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f);
            v[21] = new VertexPNT( 0.5f,  0.5f, -0.5f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f);
            v[22] = new VertexPNT( 0.5f,  0.5f,  0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f);
            v[23] = new VertexPNT( 0.5f, -0.5f,  0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 1.0f);

            for(int i = 0; i < v.Length; ++i)
            {
               v[i].Position.Z *= -1;
               v[i].Normal.Z *= -1;

                pos.Add(v[i].Position);
                normal.Add(v[i].Normal);
                if(tex != null)
                    tex.Add(v[i].Tex);
            }


            uint[] index = new uint[36];

	
            // Fill in the front face index data
            index[0] = 0; index[1] = 1; index[2] = 2;
            index[3] = 0; index[4] = 2; index[5] = 3;

            // Fill in the back face index data
            index[6] = 4; index[7] = 5; index[8] = 6;
            index[9] = 4; index[10] = 6; index[11] = 7;

            // Fill in the top face index data
            index[12] = 8; index[13] = 9; index[14] = 10;
            index[15] = 8; index[16] = 10; index[17] = 11;

            // Fill in the bottom face index data
            index[18] = 12; index[19] = 13; index[20] = 14;
            index[21] = 12; index[22] = 14; index[23] = 15;

            // Fill in the left face index data
            index[24] = 16; index[25] = 17; index[26] = 18;
            index[27] = 16; index[28] = 18; index[29] = 19;

            // Fill in the right face index data
            index[30] = 20; index[31] = 21; index[32] = 22;
            index[33] = 20; index[34] = 22; index[35] = 23;

            // reverse windings
            for (int i = 0; i < 34; i += 3)
            {
                indices.Add(index[i + 2]);
                indices.Add(index[i + 1]);
                indices.Add(index[i]);
            }

        }
        
    }

    public static class MathHelper
    {
        /// <summary>
        /// Represents the mathematical constant e. </summary>        
        public const float E = 2.71828f;

        /// <summary>
        /// Represents the log base ten of e.</summary>        
        public const float Log10E = 0.434294f;


        /// <summary>
        /// Represents the log base two of e.</summary>        
        public const float Log2E = 1.4427f;
        //
        // Summary:
        //     Represents the value of pi.
        public const float Pi = (float)Math.PI;
        //
        // Summary:
        //     Represents the value of pi divided by two.
        public const float PiOver2 = (float)(Math.PI / 2.0);
        //
        // Summary:
        //     Represents the value of pi divided by four.
        public const float PiOver4 = (float)(Math.PI / 4.0);
        //
        // Summary:
        //     Represents the value of pi times two.
        public const float TwoPi = (float)(2.0 * Math.PI);

        public const float PiOver180 = (float)(Math.PI / 180.0);

        public const float InvPiOver180 = (float)(180.0 / Math.PI);

        public static float ToRadians(float degree)
        {
            return (degree * PiOver180);
        }

        public static float ToDegree(float rad)
        {
            return (rad * InvPiOver180);

        }

    }
}


