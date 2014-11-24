//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.VectorMath;

using Camera = Sce.Atf.Rendering.Camera;
using ViewTypes = Sce.Atf.Rendering.ViewTypes;

namespace LevelEditorCore
{

    [Export(typeof(CameraController))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MayaStyleCameraController : CameraController
    {
        public MayaStyleCameraController()
        {
            CommandInfo = new CommandInfo(
                   this,
                   StandardMenu.View,
                   StandardCommandGroup.ViewCamera,
                   "camera".Localize() + "/" + "Maya".Localize(),
                   "Maya style camera".Localize(),
                   Sce.Atf.Input.Keys.None,
                   Resources.MayaImage,
                   CommandVisibility.Menu);            
        }
        
        /// <summary>
        /// Handles mouse-down events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns>true, if controller handled the event</returns>
        public override bool MouseDown(object sender, MouseEventArgs e)
        {

            if (InputScheme.ActiveControlScheme.IsControllingCamera(KeysInterop.ToAtf(Control.ModifierKeys), MouseEventArgsInterop.ToAtf(e)))
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
                InputScheme.ActiveControlScheme.IsControllingCamera(KeysInterop.ToAtf(Control.ModifierKeys), MouseEventArgsInterop.ToAtf(e)))
            {
                Control c = sender as Control;
                float dx = (e.X - m_lastMousePoint.X) * (4.0f / c.Width);
                float dy = (e.Y - m_lastMousePoint.Y) * (4.0f / c.Height);

                if (InputScheme.ActiveControlScheme.IsRotating(KeysInterop.ToAtf(Control.ModifierKeys), MouseEventArgsInterop.ToAtf(e)) &&
                    (Camera.ViewType == ViewTypes.Perspective || LockOrthographic == false))
                {
                    // elevation
                    m_elevation += dy;
                    if (m_elevation > PI)
                        m_elevation -= TwoPI;
                    else if (m_elevation < -PI)
                        m_elevation += TwoPI;

                    // azimuth
                    m_azimuth -= dx;
                    if (m_azimuth > PI)
                        m_azimuth -= TwoPI;
                    else if (m_azimuth < -PI)
                        m_azimuth += TwoPI;

                    if (Camera.ViewType != ViewTypes.Perspective)
                        Camera.ViewType = ViewTypes.Perspective;

                    ControllerToCamera();
                }
                else if (InputScheme.ActiveControlScheme.IsZooming(KeysInterop.ToAtf(Control.ModifierKeys), MouseEventArgsInterop.ToAtf(e)))
                {
                    float zoom = (-dy - dx);
                    const float ZoomScale = 0.7f; // fudge factor to get the right amount of zoom
                    m_distanceFromLookAt += zoom * CalculateZoomScale() * ZoomScale;
                    ControllerToCamera();
                }
                else if (InputScheme.ActiveControlScheme.IsPanning(KeysInterop.ToAtf(Control.ModifierKeys),MouseEventArgsInterop.ToAtf(e)))
                {
                    // The fudge factor was calculated so that an object that is fitted to view will have
                    // its pivot point stay under the cursor during panning. This was tested using the proxy
                    // cube. 0.25f was a bit too high.
                    float s = CalculateZoomScale() * 0.21f;
                    if (s > 0)
                    {
                        m_lookAtPoint += Camera.Up * dy * s;
                        m_lookAtPoint += Camera.Right * -dx * s * Camera.Aspect;
                    }
                    else
                    {
                        m_lookAtPoint -= Camera.Up * dy * s;
                        m_lookAtPoint -= Camera.Right * -dx * s * Camera.Aspect;
                    }

                    ControllerToCamera();
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
        /// Handles mouse wheel events for zooming in and out. By the standard of Microsoft
        /// and AutoCAD and others, scrolling the mouse wheel away from the user zooms in
        /// and scrolling the mouse wheel towards the user zooms out. </summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns>true, if controller handled the event</returns>
        public override bool MouseWheel(object sender, MouseEventArgs e)
        {
            if (!InputScheme.ActiveControlScheme.IsZooming(KeysInterop.ToAtf(Control.ModifierKeys), MouseEventArgsInterop.ToAtf(e)))
                return true;


            
            // disable accelerated vertical scrolling.
            int eDelta = e.Delta > 0 ? 120 : -120;

            // on a Logitech mouse, scrolling back by one "notch" made e.Delta be -120.
            float delta = -eDelta * CalculateZoomScale() * 0.002f;
            
            // account for the fact that zooming in starts with a larger look-at distance
            if (eDelta > 0)
            {
                float origLookAtDist = m_distanceFromLookAt;
                m_distanceFromLookAt += delta;
                delta = -eDelta * CalculateZoomScale() * 0.002f;
                m_distanceFromLookAt = origLookAtDist;
            }

            //minimum distance to travel in world space with one wheel "notch". If this is too
            // small, zooming can feel too slow as we get close to the look-at-point.
            const float min_wheel_delta = 1.5f;
            if (delta > -min_wheel_delta && delta < min_wheel_delta)
            {
                if (delta < 0.0f)
                    delta = -min_wheel_delta;
                else
                    delta = min_wheel_delta;
            }

            m_distanceFromLookAt += delta;
            ControllerToCamera();
            return true;
        }

        /// <summary>
        /// Synchronizes the controller to the camera's current state</summary>
        /// <param name="camera">Camera</param>
        protected override void CameraToController(Camera camera)
        {
            m_lookAtPoint = Camera.LookAtPoint;
            m_distanceFromLookAt = Camera.DistanceFromLookAt;
            m_dollyThreshold = Camera.FocusRadius * 0.1f;

            Vec3F lookAtDir = Camera.LookAt;
            Vec3F up = Camera.Up;

            m_elevation = (float)Math.Asin(-lookAtDir.Y);
            if (up.Y < 0)
            {
                if (lookAtDir.Y > 0)
                    m_elevation = -PI - m_elevation;
                else
                    m_elevation = PI - m_elevation;
                m_azimuth = (float)Math.Atan2(lookAtDir.X, lookAtDir.Z);
            }
            else
            {
                m_azimuth = (float)Math.Atan2(-lookAtDir.X, -lookAtDir.Z);
            }

            base.CameraToController(camera);
        }

        /// <summary>
        /// Synchronizes the camera to the controller's current state</summary>
        /// <param name="camera">Camera</param>
        protected override void ControllerToCamera(Camera camera)
        {
            Vec3F lookAt = camera.LookAt;
            Vec3F right = camera.Right;
            Vec3F up = camera.Up;

            if (camera.ViewType == ViewTypes.Perspective)
            {
                
                // override the camera's frame of reference
                float sinPhi = (float)Math.Sin(m_elevation);
                float cosPhi = (float)Math.Cos(m_elevation);
                float sinTheta = (float)Math.Sin(m_azimuth);
                float cosTheta = (float)Math.Cos(m_azimuth);

                lookAt = new Vec3F(-cosPhi * sinTheta, -sinPhi, -cosPhi * cosTheta);
                right = new Vec3F(cosTheta, 0, -sinTheta);
                up = Vec3F.Cross(right, lookAt); // TODO compute from sin/cos values
            }

            float lookAtOffset = 0;
            if (m_distanceFromLookAt < m_dollyThreshold) // do we need to start dollying?
                lookAtOffset = m_distanceFromLookAt - m_dollyThreshold;

            float eyeOffset = m_distanceFromLookAt;

            Camera.Set(m_lookAtPoint - (eyeOffset * lookAt),       // eye point
                m_lookAtPoint - (lookAtOffset * lookAt),    // look-at point
                up);                                        // up vector

            base.ControllerToCamera(camera);
        }
       

        /// <summary>
        /// Calculates an appropriate zoom scale factor based on distance from the look-at point.
        /// It's important that it allows the camera to zoom through objects, so m_distanceFromLookAt
        /// may be negative. Calling this for every event, rather than calculating it only on mouse-down
        /// events, allows for the zooming effort to be consistent regardless of the original zoom level.
        /// </summary>
        private float CalculateZoomScale()
        {
            return Math.Max( Math.Abs(m_distanceFromLookAt), m_dollyThreshold);
        }

        private float m_distanceFromLookAt = 16.0f; // distance from camera to lookAt point
        private float m_azimuth;                    // angle of rotation in XZ plane starting from +Z axis (radians)
        private float m_elevation;                  // angle of elevation from XZ plane (radians)
        private Vec3F m_lookAtPoint = new Vec3F();

        private float m_dollyThreshold;
        private Point m_lastMousePoint;
        private bool m_dragging;
    }
    
}
