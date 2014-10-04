//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LevelEditorCore
{
    /// <summary>
    /// Maintain a set of IDesigViewControls</summary>
    public interface IDesignView
    {
        /// <summary>
        /// Gets the host control that is parent for 
        /// DesigViewControls </summary>
        Control HostControl
        {
            get;
        }

        /// <summary>
        /// Gets the active view.</summary>        
        DesignViewControl ActiveView
        {
            get;
        }

        /// <summary>
        /// Gets the IDesigViewControls</summary>
        /// <returns></returns>
        IEnumerable<DesignViewControl> Views
        {
            get;
        }
        
        /// <summary>
        /// Gets and sets view mode.</summary>
        ViewModes ViewMode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the context that IDesignView work with</summary>
        object Context
        {
            get;
            set;
        }
        /// <summary>
        /// Event that is raised before Context changes</summary>
        event EventHandler ContextChanging;

        /// <summary>
        /// Event that is raised after Context changes</summary>
        event EventHandler ContextChanged;        

        /// <summary>
        /// Gets/Sets active manipulator.
        /// </summary>
        IManipulator Manipulator
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets pick filter.
        /// </summary>
        IPickFilter PickFilter
        {
            get; set;
        }
       
        /// <summary>
        /// Invalates all the visible views.</summary>        
        /// <remarks>if realtime is true then this method do nothing.</remarks>
        void InvalidateViews();

        /// <summary>
        /// Gets or sets the background color of the design controls</summary>
        Color BackColor
        {
            get;
            set;
        }
        /// <summary>
        /// Advance update and render by step</summary>
        void Tick();

        /// <summary>
        /// Tick using the specified frame time.
        /// </summary>        
        void Tick(FrameTime ft);

        /// <summary>
        /// Gets next frame time.</summary>        
        FrameTime GetFrameTime();

    }

    

    // supported enums and classes.
    public enum ViewModes
    {
        Single,
        DualVertical,
        DualHorizontal,
        Quad,
    }

}
