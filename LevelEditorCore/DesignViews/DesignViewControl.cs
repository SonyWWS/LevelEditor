//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;


namespace LevelEditorCore
{
    /// <summary>
    /// Control for for 3d rendering.</summary>
    public class DesignViewControl: ViewControl
    {
        /// <summary>
        /// Constructor</summary>
        public DesignViewControl(DesignView designView)        
        {
            DesignView = designView;
            this.Camera.SetPerspective((float)(Math.PI / 4), 1.0f, 0.1f, designView.CameraFarZ);            
            
        }

        /// <summary>
        /// Gets parent designview.
        /// </summary>
        public IDesignView DesignView
        {
            get;
            private set;
        }


  
     
        /// <summary>
        /// Gets a value indicating if mouse movement passed the DragThreshold in x or y</summary>
        public bool DragOverThreshold
        {
            get { return m_dragOverThreshold; }
        }

        /// <summary>
        /// Gets a value indicating if the user is picking</summary>
        public bool IsPicking
        {
            get { return m_mouseDownAction == MouseDownAction.Picking; }
        }


        /// <summary>
        /// Gets GameLoop</summary>
        public IGameLoop GameLoop
        {
            get
            {
                if(m_gameLoop == null)
                    m_gameLoop = Globals.MEFContainer.GetExportedValue<IGameLoop>();
                return m_gameLoop;
            }
        }
        private IGameLoop m_gameLoop;
        

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            // reject additional mouse clicks until current is done.
            if (m_mouseDownAction != MouseDownAction.None)
                return;

            Focus();
            
            FirstMousePoint = CurrentMousePoint = new Point(e.X, e.Y);
            m_dragOverThreshold = false;
            
            if (DesignView.Context != null)
            {                
                bool handled = CameraController.MouseDown(this, e);
                if (handled)
                {
                    m_mouseDownAction = MouseDownAction.ControllingCamera;
                }
                else if (e.Button == MouseButtons.Left)
                {// either regular pick or manipulator pick.                    
                    if (DesignView.Manipulator != null && DesignView.Manipulator.Pick(this, e.Location))
                    {
                        m_mouseDownAction = MouseDownAction.Manipulating;                        
                    }
                    else
                    {
                        m_mouseDownAction = MouseDownAction.Picking;
                    }
                }
            }
            DesignView.InvalidateViews();
            base.OnMouseDown(e);
            
        }        
        
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (DesignView.Context != null)
            {
                Size dragBoxSize = SystemInformation.DragSize;
                CurrentMousePoint = new Point(e.X, e.Y);
                int dx = CurrentMousePoint.X - FirstMousePoint.X;
                int dy = CurrentMousePoint.Y - FirstMousePoint.Y;
                if (!m_dragOverThreshold)
                {
                    if (Math.Abs(dx) > dragBoxSize.Width || Math.Abs(dy) > dragBoxSize.Height)
                    {
                        m_dragOverThreshold = true;
                        if (m_mouseDownAction == MouseDownAction.Manipulating)
                            DesignView.Manipulator.OnBeginDrag();
                    }
                }

                if (m_mouseDownAction == MouseDownAction.Picking)
                {
                    Invalidate();
                }
                else if (m_mouseDownAction == MouseDownAction.ControllingCamera)
                {
                    CameraController.MouseMove(this, e);
                }
                else if (m_mouseDownAction == MouseDownAction.Manipulating)
                {
                    if (m_dragOverThreshold)
                    {
                        DesignView.Manipulator.OnDragging(this, e.Location);
                        GameLoop.Update();
                        GameLoop.Render();

                        if (m_propEditor == null)
                            m_propEditor = Globals.MEFContainer.GetExportedValue<PropertyEditor>();
                        m_propEditor.PropertyGrid.RefreshProperties();
                    }
                }
                else if (DesignView.Manipulator != null)
                {                    
                    bool picked = DesignView.Manipulator.Pick(this, e.Location);
                    this.Cursor = picked ? Cursors.SizeAll : Cursors.Default;
                    DesignView.InvalidateViews();
                }
                else if (this.Cursor != Cursors.Default)
                {
                    this.Cursor = Cursors.Default;
                }
            }
           
            if(m_dragOverThreshold)
                m_hitIndex = -1;

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (m_mouseDownAction == MouseDownAction.Picking)
            {
                HandlePick(e);
            }
            else if (m_mouseDownAction == MouseDownAction.ControllingCamera)
            {
                CameraController.MouseUp(this, e);
            }
            else if (m_mouseDownAction == MouseDownAction.Manipulating)
            {
                DesignView.Manipulator.OnEndDrag(this, e.Location);
            }
            else if (e.Button == MouseButtons.Right)
            {
                IEnumerable<IContextMenuCommandProvider>
                    contextMenuCommandProviders = Globals.MEFContainer.GetExportedValues<IContextMenuCommandProvider>();

                Point clientPoint = new Point(e.X, e.Y);
                Point screenPoint = PointToScreen(clientPoint);

                object target = this;
                IList<object> pickList = Pick(e);
                if (pickList.Count > 0)
                {
                    Path<object> pickedObj = (Path<object>)pickList[0];
                    target = pickedObj.Last;
                }


                IEnumerable<object> commands =
                    contextMenuCommandProviders.GetCommands(DesignView.Context, target);

                ICommandService commandService = Globals.MEFContainer.GetExportedValue<ICommandService>();
                commandService.RunContextMenu(commands, screenPoint);                
            }

            m_mouseDownAction = MouseDownAction.None;

            DesignView.InvalidateViews();
            base.OnMouseUp(e);
            
        }

        
        protected virtual void HandlePick(MouseEventArgs e)
        {
            SelectMode selectMode = InputScheme.GetSelectMode(Control.ModifierKeys);
            ISelectionContext selection = DesignView.Context.As<ISelectionContext>();
            IList<object> hits = Pick(e);
            bool multiSelect = DragOverThreshold;            
            if (multiSelect == false && hits.Count > 0)
            {
                List<object> singleHit = new List<object>();
                if(selectMode == SelectMode.Normal)
                {
                    m_hitIndex++;
                    if (m_hitIndex >= hits.Count)
                        m_hitIndex = 0;            
                    singleHit.Add(hits[m_hitIndex]);
                }
                else
                {
                    singleHit.Add(hits[0]);
                }
                hits = singleHit;
            }
                            
            switch (selectMode)
            {
                case SelectMode.Normal:
                    selection.SetRange(hits);
                    break;
                case SelectMode.Extend:
                    selection.AddRange(hits);
                    break;
                case SelectMode.Toggle:
                    selection.ToggleRange(hits);
                    break;
                case SelectMode.Remove:
                    selection.RemoveRange(hits);
                    break;
                default:
                    break;
            }
        }

        protected virtual IList<object> Pick(MouseEventArgs e)
        {
            return new List<object>();
        }
        /// <summary>
        /// Gets the starting mouse coordinates</summary>
        public Point FirstMousePoint
        {
            protected set;
            get;
        }

        /// <summary>
        /// Gets the current mouse coordinates</summary>
        public Point CurrentMousePoint
        {
            protected set;
            get;
        }
      
        private bool m_dragOverThreshold;               
        private MouseDownAction m_mouseDownAction;
        private int m_hitIndex = -1;
        private PropertyEditor m_propEditor;
        private enum MouseDownAction
        {
            None,
            ControllingCamera,
            Picking,
            Manipulating,            
        }

    }

    /// <summary>    
    /// ControlScheme was stored in canvascontrol3d
    /// which created dependecy between CameraControllers and canvascontrol3d
    /// 
    /// this is temp location to store global control scheme.
    /// </summary>
    public static class InputScheme
    {
        /// <summary>
        /// Gets and sets active control scheme        
        /// </summary>
        public static Sce.Atf.Rendering.ControlScheme ActiveControlScheme
        {
            get;
            set;
        }

        public static SelectMode GetSelectMode(Keys modifiers)
        {
            Sce.Atf.Input.Keys modkeys = KeysInterop.ToAtf(modifiers);
            if (ActiveControlScheme.ToggleSelection != Sce.Atf.Input.Keys.None 
                && modkeys == ActiveControlScheme.ToggleSelection)
                return SelectMode.Toggle;
            if (ActiveControlScheme.AddSelection != Sce.Atf.Input.Keys.None 
                && modkeys == ActiveControlScheme.AddSelection)
                return SelectMode.Extend;
            if (ActiveControlScheme.RemoveSelection != Sce.Atf.Input.Keys.None 
                && modkeys == ActiveControlScheme.RemoveSelection)
                return SelectMode.Remove;
            return SelectMode.Normal;
        }

    }

    /// <summary>
    /// Enumerates the selection modes, to specify how the user is intending to modify the
    /// selection set by holding down different modifier keys -- Ctrl and Shift, typically</summary>
    public enum SelectMode
    {
        /// <summary>
        /// Clear the selection, add object to selection, and make it the last selected</summary>
        Normal,

        /// <summary>
        /// Add object to selection and make it the last selected</summary>
        Extend,

        /// <summary>
        /// Remove object from selection</summary>
        Remove,

        /// <summary>
        /// If object is in selection, remove it; otherwise, add it to selection
        /// and make it the last selected</summary>
        Toggle,
    }
}
