//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.VectorMath;

using Camera = Sce.Atf.Rendering.Camera;
using CameraController = Sce.Atf.Rendering.CameraController;
using ViewTypes = Sce.Atf.Rendering.ViewTypes;

namespace LevelEditorCore
{
    /// <summary>
    /// Control used as render surface for 3d scene. </summary>
    public class ViewControl : Control
    {
        public ViewControl()
        {           
            // this control will be painted using some 3d API
            // no need for double buffering
            // also no need for the OS to paint it.
            this.ResizeRedraw = false;
            this.DoubleBuffered = false;
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.Opaque, true);
            AllowDrop = true;
            m_camera = new Camera();


            m_camera.SetPerspective((float)(Math.PI / 4), 1.0f, 0.1f, 2048);
            m_cameraController = new MayaStyleCameraController();
            m_cameraController.Camera = m_camera;

            Sphere3F sphere = new Sphere3F(new Vec3F(0, 0, 0), 25.0f);
            float nearZ = m_camera.PerspectiveNearZ;
            m_camera.ZoomOnSphere(sphere);
            m_camera.PerspectiveNearZ = nearZ;
            m_camera.CameraChanged += new EventHandler(CameraChanged);            
        }
        
        
        /// <summary>
        /// Get the camera object.
        /// there is only one camera object.
        /// </summary>
        public Camera Camera
        {
            get { return m_camera; }
        }

        /// <summary>
        /// Get/Set current camera controller.
        /// </summary>
        public CameraController CameraController
        {
            get { return m_cameraController; }
            set
            {
                if (value == null)
                    throw new ArgumentException();
                if (!value.CanHandleCamera(m_camera))
                    return;

                if (m_cameraController != null)
                    m_cameraController.Camera = null;

                m_cameraController = value;
                m_cameraController.Camera = m_camera;
            }
        }

        /// <summary>
        /// Gets/sets view type
        /// </summary>
        public ViewTypes ViewType
        {
            get { return m_camera.ViewType; }
            set { m_camera.ViewType = value; }
        }


        /// <summary>
        /// compute ray in  given space starting from 
        /// screen space x,y.        
        /// The space of the computed ray depends on the value of mtrx:
        ///               world * view * projection // ray in local space (object space).
        ///               view * projection  // ray in world space.
        ///               projection   // ray in view space.        
        /// </summary>        
        public Ray3F GetRay(Point scrPt, Matrix4F mtrx)
        {
            Vec3F min = Unproject(new Vec3F(scrPt.X, scrPt.Y, 0), mtrx);
            Vec3F max = Unproject(new Vec3F(scrPt.X, scrPt.Y, 1), mtrx);
            Vec3F dir = Vec3F.Normalize(max - min);
            Ray3F ray = new Ray3F(min, dir);
            return ray;
        }

        /// <summary>
        /// compute ray in world space starting from 
        /// screen space x,y.
        /// </summary>        
        public Ray3F GetWorldRay(Point scrPt)
        {
            Matrix4F vp = Camera.ViewMatrix * Camera.ProjectionMatrix;
            Vec3F min = Unproject(new Vec3F(scrPt.X, scrPt.Y, 0), vp);
            Vec3F max = Unproject(new Vec3F(scrPt.X, scrPt.Y, 1), vp);
            Vec3F dir = Vec3F.Normalize(max - min);
            Ray3F ray = new Ray3F(min, dir);
            return ray;
        }


        /// <summary>
        /// Render the scene</summary>
        public virtual void Render()
        {
        }

        /// <summary>
        /// project the v from 3d space to viewport space
        /// using the given wvp matrix.
        /// </summary>        
        public Point Project(Matrix4F wvp, Vec3F v)
        {

            float w = v.X * wvp.M14 + v.Y * wvp.M24 + v.Z * wvp.M34 + wvp.M44;
            wvp.Transform(ref v);
            v = v / w;
            Point pt = new Point();
            pt.X = (int)((v.X + 1) * 0.5f * Width);
            pt.Y = (int)((1.0f - v.Y) * 0.5f * Height);
            return pt;
        }

        /// <summary>
        /// unproject vector from screen space to object space.
        /// </summary>        
        public Vec3F Unproject(Vec3F scrPt, Matrix4F wvp)
        {
            float width = ClientSize.Width;
            float height = ClientSize.Height;
            Matrix4F invWVP = new Matrix4F();
            invWVP.Invert(wvp);
            Vec3F worldPt = new Vec3F();
            worldPt.X = scrPt.X / width * 2.0f - 1f;
            worldPt.Y = -(scrPt.Y / height * 2.0f - 1f);
            worldPt.Z = scrPt.Z;

            float w = worldPt.X * invWVP.M14 + worldPt.Y * invWVP.M24 + worldPt.Z * invWVP.M34 + invWVP.M44;
            invWVP.Transform(ref worldPt);
            worldPt = worldPt / w;
            return worldPt;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"></see> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            bool handled = CameraController.KeyDown(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyUp"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"></see> that contains the event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = CameraController.KeyUp(this, e);
        }

        /// <summary>
        /// Catching the arrow keys is important here. If we didn't return true then OnKeyDown and OnKeyUp will not get
        /// called for the arrow keys.</summary>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool IsInputKey(Keys keyData)
        {
            if (InputScheme.ActiveControlScheme.IsInputKey(KeysInterop.ToAtf(keyData)))
            {
                return true;
            }
            return base.IsInputKey(keyData);
        }

        /// <summary>
        /// Calling the base ProcessCmdKey allows this keypress to be consumed by owning
        /// Controls like PropertyView and PropertyGridView and be seen by ControlHostService.
        /// Returning false allows the keypress to escape to IsInputKey, OnKeyDown, OnKeyUp, etc.
        /// Returning true means that this keypress has been consumed by this method and this
        /// event is not passed on to any other methods or Controls.</summary>
        /// <param name="msg"></param>
        /// <param name="keyDataWf"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyDataWf)
        {
            if (CameraController.HandlesWASD
                && InputScheme.ActiveControlScheme.IsInputKey(KeysInterop.ToAtf(keyDataWf)))
            {
                return false;
            }
            return base.ProcessCmdKey(ref msg, keyDataWf);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseEnter"></see> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {            
            CameraController.MouseWheel(this, e);
            base.OnMouseWheel(e);
        }

        private void CameraChanged(object sender, EventArgs e)
        {
            Invalidate();
        }
        
        private readonly Camera m_camera;  // only camera object.
        private CameraController m_cameraController;        
    }

}
