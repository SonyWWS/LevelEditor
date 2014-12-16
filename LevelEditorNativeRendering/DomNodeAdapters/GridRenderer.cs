//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.VectorMath;


using LevelEditorCore;


using Camera = Sce.Atf.Rendering.Camera;
using ViewTypes = Sce.Atf.Rendering.ViewTypes;

namespace RenderingInterop
{
    // DomNodeAdapter for rendering grid.
    class GridRenderer : DomNodeAdapter
    {
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            DomNode.AttributeChanged += DomNode_AttributeChanged;            
        }

        public void Render(Camera cam)
        {
            GameEngine.SetRendererFlag(BasicRendererFlags.WireFrame);
            IGrid grid = this.As<IGrid>();

            if (grid.Visible == false)
                return;

            float s = grid.Size;

            Matrix4F scale = new Matrix4F();            
            scale.Scale(new Vec3F(s, s, s));

            Matrix4F gridXform = new Matrix4F();
            if (cam.Frustum.IsOrtho)
            {                
                float dist = cam.ViewMatrix.Translation.Z;
                ViewTypes vt = cam.ViewType;
                if (vt == ViewTypes.Top)
                {
                    gridXform.Translation
                        = new Vec3F(0, dist, 0);
                }
                else if (vt == ViewTypes.Bottom)
                {
                    gridXform.Translation
                        = new Vec3F(0, -dist, 0);
                }
                else if (vt == ViewTypes.Right)
                {
                    gridXform.RotZ(MathHelper.PiOver2);
                    gridXform.Translation
                        = new Vec3F(dist, 0, 0);
                }
                else if (vt == ViewTypes.Left)
                {
                    gridXform.RotZ(MathHelper.PiOver2);
                    gridXform.Translation
                        = new Vec3F(-dist, 0, 0);

                }
                else if (vt == ViewTypes.Front)
                {
                    gridXform.RotX(MathHelper.PiOver2);
                    gridXform.Translation
                        = new Vec3F(0, 0, dist);

                }
                else if (vt == ViewTypes.Back)
                {
                    gridXform.RotX(MathHelper.PiOver2);
                    gridXform.Translation
                        = new Vec3F(0, 0, -dist);

                }
                gridXform.Mul(scale, gridXform);
            }
            else
            {
                Matrix4F trans = new Matrix4F();
                trans.Translation = new Vec3F(0, grid.Height, 0);
                gridXform = Matrix4F.Multiply(scale, trans);
            }

            GameEngine.DrawPrimitive(PrimitiveType.LineList, m_gridVBId, 0, m_gridVertexCount, Color.LightGray,
                                     gridXform);
        }

        // creates grid unit grid.       
        public void CreateVertices()
        {
            DeleteVertexBuffer();

            IGrid grid = this.As<IGrid>();
            m_subDiv = grid.Subdivisions;
            float corner = 0.5f;                  // grid.Size / 2.0f;
            float step = 1.0f / (float)m_subDiv;   // grid.Size / (float)subDiv;

            int numLines = (m_subDiv + 1) * 2;
            int numVerts = numLines * 2;

            m_vertices = new Vec3F[numVerts];

            int index = 0;
            float s = -corner;

            // Create vertical lines
            for (int i = 0; i <= m_subDiv; i++)
            {
                m_vertices[index] = new Vec3F(s, 0, corner);
                m_vertices[index + 1] = new Vec3F(s, 0, -corner);

                index += 2;
                s += step;
            }

            // Create horizontal lines
            s = -corner;
            for (int i = 0; i <= m_subDiv; i++)
            {
                m_vertices[index] = new Vec3F(corner, 0, s);
                m_vertices[index + 1] = new Vec3F(-corner, 0, s);

                index += 2;
                s += step;
            }


            m_gridVBId = GameEngine.CreateVertexBuffer(m_vertices);
            m_gridVertexCount = (uint)m_vertices.Length;
        }

        public void DeleteVertexBuffer()
        {
            if (m_gridVBId  > 0)
            {
                GameEngine.DeleteBuffer(m_gridVBId);
                m_gridVBId = 0;
                m_gridVertexCount = 0;                
            }            
        }

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            // if subdev changed then re-create vertices.
            IGrid grid = this.As<IGrid>();
            if (m_subDiv != grid.Subdivisions)
                CreateVertices();
        }
       
        private int m_subDiv;
        private Vec3F[] m_vertices;

        private ulong m_gridVBId; // id for grid vertex buffer.
        private uint m_gridVertexCount;

    }
}
