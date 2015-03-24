//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls;


using MayaControlScheme = Sce.Atf.Rendering.MayaControlScheme;
using MayaLaptopControlScheme = Sce.Atf.Rendering.MayaLaptopControlScheme;
using MaxControlScheme = Sce.Atf.Rendering.MaxControlScheme;
using ControlSchemes = Sce.Atf.Rendering.ControlSchemes;


namespace LevelEditorCore
{
    /// <summary>
    /// Base designview class
    /// </summary>
    public abstract class DesignView : IDesignView, ISnapSettings
    {
        public DesignView()
        {            
            QuadView = new QuadPanelControl();
            CameraController.LockOrthographic = true;
        }

        #region IDesignView Members

        public Control HostControl
        {
            get { return QuadView; }
        }

        public DesignViewControl ActiveView
        {
            get { return (DesignViewControl)QuadView.ActiveControl; }
        }

        /// <summary>
        /// Gets all the DesignViewControls</summary>
        public IEnumerable<DesignViewControl> AllViews
        {
            get
            {
                foreach (Control ctrl in QuadView.Controls)
                {
                    DesignViewControl view = ctrl as DesignViewControl;
                    if (view != null) yield return view;                    
                }                                    
            }
        }

        /// <summary>
        /// Gets only DesigViewControls
        /// for the current ViewMode</summary>
        public IEnumerable<DesignViewControl> Views
        {
            get {return AllViews.Where(view => view.Width > 1 && view.Height > 1); }
        }

        private ViewModes m_viewMode = ViewModes.Quad;
        public ViewModes ViewMode
        {
            get { return m_viewMode; }
            set
            {
                m_viewMode = value;
                switch (m_viewMode)
                {
                    case ViewModes.Single:                        
                        QuadView.EnableX = false;
                        QuadView.EnableY = false;
                        QuadView.SplitterThickness = 0;
                        QuadView.SplitX = 1.0f;
                        QuadView.SplitY = 1.0f;                        
                        break;
                    case ViewModes.DualHorizontal:
                        if (QuadView.ActiveControl == QuadView.TopLeft
                            || QuadView.ActiveControl == QuadView.TopRight)
                            QuadView.SplitX = 1.0f;
                        else
                            QuadView.SplitX = 0.0f;
                        QuadView.SplitY = 0.5f;
                        QuadView.EnableX = false;
                        QuadView.EnableY = true;
                        QuadView.SplitterThickness = DefaultSplitterThickness;
                        
                        break;
                    case ViewModes.DualVertical:
                        
                        if (QuadView.ActiveControl == QuadView.TopLeft
                            || QuadView.ActiveControl == QuadView.BottomLeft)
                            QuadView.SplitY = 1.0f;
                            else
                            QuadView.SplitY = 0.0f;

                        QuadView.SplitterThickness = DefaultSplitterThickness;
                        QuadView.EnableX = true;
                        QuadView.EnableY = false;
                        QuadView.SplitX = 0.5f;
                        
                        break;
                    case ViewModes.Quad:
                        QuadView.EnableX = true;
                        QuadView.EnableY = true;
                        QuadView.SplitterThickness = DefaultSplitterThickness;
                        QuadView.SplitX = 0.5f;
                        QuadView.SplitY = 0.5f;
                        break;
                }

                QuadView.Refresh();
                
            }
        }
        public object Context
        {
            get { return m_context; }
            set
            {
                ContextChanging(this, EventArgs.Empty);

                if (m_validationContext != null)
                {
                    m_validationContext.Cancelled -= validationContext_Refresh;
                    m_validationContext.Ended -= validationContext_Refresh;
                }

                m_context = value; 
                m_validationContext = m_context.As<IValidationContext>();

                if (m_validationContext != null)
                {
                    m_validationContext.Cancelled += validationContext_Refresh;
                    m_validationContext.Ended += validationContext_Refresh;
                }
                
                ContextChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler ContextChanging = delegate { };
        public event EventHandler ContextChanged = delegate { };

        private void validationContext_Refresh(object sender, EventArgs e)
        {
            InvalidateViews();
        }

      
        private object m_context = null;
        public IManipulator Manipulator
        {
            get { return m_manipulator; }
            set
            {                
                m_manipulator = value;
                                
                if (m_manipulator != null)
                {                    
                    Point pt = ActiveView.PointToClient(Control.MousePosition);                    
                    bool picked = m_manipulator.Pick(ActiveView, pt);
                    ActiveView.Cursor = picked ? Cursors.SizeAll : Cursors.Default;                    
                }
                else
                {
                    ActiveView.Cursor = Cursors.Default;
                }
                InvalidateViews();
            }
            

        }

        public IPickFilter PickFilter
        {
            get;
            set;
        }

        [DefaultValue(typeof(Color), "0xFF606060")]
        public Color BackColor
        {
            get { return m_backColor; }
            set
            {
                m_backColor = value;
                InvalidateViews();
            }
        }
        private Color m_backColor = Color.FromArgb(96, 96, 96);

        public void InvalidateViews()
        {
            if (m_gameLoop != null)
            {
                m_gameLoop.Update();
                m_gameLoop.Render();
            }
        }
        #endregion

        #region ISnapSettings Members

        [DefaultValue(false)]
        public bool SnapVertex { get; set; }

        [DefaultValue(false)]
        public bool RotateOnSnap { get; set; }

        [DefaultValue(SnapFromMode.Pivot)]
        public SnapFromMode SnapFrom { get; set; }

        [DefaultValue(false)]
        public bool ManipulateLocalAxis { get; set; }

        [DefaultValue((float)(5.0f * (Math.PI / 180.0f)))]
        public float SnapAngle
        {
            get{return m_SnapAngle;}
            set
            {
                m_SnapAngle = MathUtil.Clamp(value,0, (float) (2.0 * Math.PI));
            }
        }

        #endregion

        /// <summary>
        /// Distance to the camera's far clipping plane.</summary>        
        [DefaultValue(2048.0f)]
        public float CameraFarZ
        {
            get { return m_cameraFarZ; }
            set
            {
                m_cameraFarZ = value;
                foreach (DesignViewControl view in QuadView.Controls)
                {
                    view.Camera.FarZ = m_cameraFarZ;
                }
            }
        }


        /// <summary>
        /// Gets/sets input scheme.</summary>
        [DefaultValue(ControlSchemes.Maya)]
        public ControlSchemes ControlScheme
        {
            get { return m_controlScheme; }
            set
            {
                switch (value)
                {
                    case ControlSchemes.Maya:
                        InputScheme.ActiveControlScheme = new MayaControlScheme();
                        break;
                    case ControlSchemes.MayaLaptop:
                        InputScheme.ActiveControlScheme = new MayaLaptopControlScheme();
                        break;
                    case ControlSchemes.Max:
                        InputScheme.ActiveControlScheme = new MaxControlScheme();
                        break;
                }
                m_controlScheme = value;
            }
        }
                   
        protected const int DefaultSplitterThickness = 8;
        protected readonly QuadPanelControl QuadView;

        #region private members

        [Import(AllowDefault = false)]
        private IGameLoop m_gameLoop;

        private float m_cameraFarZ = 2048;
        private ControlSchemes m_controlScheme = ControlSchemes.Maya;
        private float m_SnapAngle = (float)(5.0 * (Math.PI / 180.0f));
        private IManipulator m_manipulator;
        private IValidationContext m_validationContext;
                
        #endregion                 
    }
}
