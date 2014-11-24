//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using LevelEditorCore.Commands;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.VectorMath;
using Sce.Atf.Rendering;

namespace LevelEditorCore
{
    /// <summary>
    /// Walk camera controller, which uses WASD keys to move, and mouse to rotate. This
    /// controller doesn't change the camera eye position's y-coordinate</summary>
    [Export(typeof(CameraController))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class WalkCameraController : CameraController
    {
        public WalkCameraController()
        {
            CommandInfo = new CommandInfo(
                this,
                StandardMenu.View,
                StandardCommandGroup.ViewCamera,
                "camera".Localize() + "/" + "Walk".Localize(),
                "Walk: WASD + Middle Mouse, press Alt+MiddleMouse for height, mouse wheel to adjust walk speed".Localize(),
                Sce.Atf.Input.Keys.None,
                Resources.WalkImage,
                CommandVisibility.Menu
                );
        }

        
        /// <summary>
        /// Handles key-down events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns>true, if controller handled the event</returns>
        public override bool KeyDown(object sender, KeyEventArgs e)
        {
            
            m_keyMap[e.KeyValue] = true;

            // W A S D for forward, strafe left, backward, strafe right, is the default
            Vec3F dir = new Vec3F();
            if (m_keyMap[(int)InputScheme.ActiveControlScheme.Left1] ||
                m_keyMap[(int)InputScheme.ActiveControlScheme.Left2])
                dir = dir - Camera.Right;
            if (m_keyMap[(int)InputScheme.ActiveControlScheme.Right1] ||
                m_keyMap[(int)InputScheme.ActiveControlScheme.Right2]) 
                dir = dir + Camera.Right;
            if (m_keyMap[(int)InputScheme.ActiveControlScheme.Forward1] ||
                m_keyMap[(int)InputScheme.ActiveControlScheme.Forward2])
                dir = dir + Camera.LookAt;
            if (m_keyMap[(int)InputScheme.ActiveControlScheme.Back1] ||
                m_keyMap[(int)InputScheme.ActiveControlScheme.Back2])
                dir = dir - Camera.LookAt;

            bool handled = InputScheme.ActiveControlScheme.IsControllingCamera(Control.ModifierKeys, e);

            if (handled)
            {
                dir.Normalize();
                Vec3F p = Camera.Eye;
                float y = p.Y;
                p += dir * m_scale;
                p.Y = y;
                Camera.Set(p);
            }

            return handled;
        }

        /// <summary>
        /// Clear keymap entry corresponds to the key up code .
        /// </summary>
        public override bool KeyUp(object sender, KeyEventArgs e)
        {
            m_keyMap[e.KeyValue] = false;
            return true;
        }

        /// <summary>
        /// Handles mouse wheel events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns>true, if controller handled the event</returns>
        public override bool MouseWheel(object sender, MouseEventArgs e)
        {
            m_scale += (e.Delta > 0) ? 0.1f : -0.1f;
            if (m_scale <= 0.1f)
                m_scale = 0.01f;

            return true;
        }

        /// <summary>
        /// Handles mouse-down events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns>true, if controller handled the event</returns>
        public override bool MouseDown(object sender, MouseEventArgs e)
        {
            if (InputScheme.ActiveControlScheme.IsControllingCamera(Control.ModifierKeys, e))
            {
                m_lastMousePoint = e.Location;
                m_dragging = true;
                return true;
            }

            return base.MouseDown(sender, e);
        }

        /// <summary>
        /// Handles mouse-move events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns>true, if controller handled the event</returns>
        public override bool MouseMove(object sender, MouseEventArgs e)
        {
            if (m_dragging &&
                InputScheme.ActiveControlScheme.IsControllingCamera(Control.ModifierKeys, e))
            {
                float dx = (float)(e.X - m_lastMousePoint.X) / 150.0f;
                float dy = (float)(e.Y - m_lastMousePoint.Y) / 150.0f;

                if (InputScheme.ActiveControlScheme.IsElevating(Control.ModifierKeys, e))
                {
                    // move camera up/down
                    Vec3F p = Camera.Eye;
                    p.Y += (dy < 0) ? m_scale : -m_scale;
                    Camera.Set(p);
                }
                else if (InputScheme.ActiveControlScheme.IsTurning(Control.ModifierKeys, e))
                {
                    // pitch and yaw camera
                    Matrix4F mat = Matrix4F.RotAxisRH(Camera.Right, -dy); // pitch along camera right
                    Matrix4F yaw = new Matrix4F();
                    yaw.RotY(-dx);
                    mat.Mul(yaw, mat);

                    Vec3F lookAt = Camera.LookAt;
                    Vec3F up = Camera.Up;
                    mat.Transform(ref lookAt);
                    mat.Transform(ref up);

                    Vec3F position = Camera.Eye;
                    float d = Camera.DistanceFromLookAt;
                    Camera.Set(position, position + lookAt * d, up);
                }

                m_lastMousePoint = e.Location;

                return true;
            }

            return base.MouseMove(sender, e);
        }

        /// <summary>
        /// Handles mouse-up events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns>true, if controller handled the event</returns>
        public override bool MouseUp(object sender, MouseEventArgs e)
        {
            if (m_dragging)
            {
                m_dragging = false;
                return true;
            }
            return base.MouseUp(sender, e);
        }

        /// <summary>
        /// Gets a value indicating if the controller handles the WASD keys</summary>
        public override bool HandlesWASD
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating if this camera can handle the given camera</summary>
        /// <param name="camera">Camera</param>
        /// <returns>value indicating if this camera can handle the given camera</returns>
        public override bool CanHandleCamera(Camera camera)
        {
            return camera.ProjectionType != ProjectionType.Orthographic;
        }

        /// <summary>
        /// Performs any camera initialization required by the controller</summary>
        /// <param name="camera">Camera</param>
        protected override void Setup(Camera camera)
        {
            camera.PerspectiveNearZ = 0.01f;
        }

        private float m_scale = 0.5f;
        private Point m_lastMousePoint = Point.Empty;
        private readonly bool[] m_keyMap = new bool[256];
        private bool m_dragging;
    }
}
