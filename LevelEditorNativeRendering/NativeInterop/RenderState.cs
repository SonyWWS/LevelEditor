//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing;
using Sce.Atf;


namespace RenderingInterop
{
    /// <summary>
    /// class mirros RenderState in C++
    /// the memory layout and size in bytes do not have to match.
    /// class name and common property names have to match.
    /// </summary>
    public class RenderState : DisposableObject
    {
        public RenderState()
        {
            string typeName = typeof (RenderState).Name;
            m_typeId = GameEngine.GetObjectTypeId(typeName);
            m_intanceId = GameEngine.CreateObject(m_typeId, IntPtr.Zero, 0);
            m_renderFlagId = GameEngine.GetObjectPropertyId(m_typeId, "GlobalRenderFlags");
            m_wirecolorId = GameEngine.GetObjectPropertyId(m_typeId, "WireframeColor");
            m_selColorId = GameEngine.GetObjectPropertyId(m_typeId, "SelectionColor");
        }

        /// <summary>
        /// Event that is raised when any properties of this RenderState change</summary>
        public event EventHandler Changed;
          
        private GlobalRenderFlags m_renderflags;
        
        [Browsable(false)]
        public GlobalRenderFlags RenderFlag
        {
            get { return m_renderflags; }
            set 
            { 
                GameEngine.SetObjectProperty(m_typeId, m_intanceId, m_renderFlagId, (uint)value);
                if (value != m_renderflags)
                {
                    m_renderflags = value;
                    Changed.Raise(this, EventArgs.Empty);
                }
            }
        }


        private Color m_wireColor;

        [CategoryAttribute("Render Settings"),
        DescriptionAttribute("color used for wireframe mode")]       
        public Color WireFrameColor
        {
            get { return m_wireColor; }
            set 
            { 
                GameEngine.SetObjectProperty(m_typeId, m_intanceId, m_wirecolorId, value);
                if (value != m_wireColor)
                {
                    m_wireColor = value;
                    Changed.Raise(this, EventArgs.Empty);
                }
            }
        }

        private Color m_selectionColor;

        [CategoryAttribute("Render Settings"),
        DescriptionAttribute("Selection color")]
        public Color SelectionColor
        {
            get { return m_selectionColor; }
            set
            {
                GameEngine.SetObjectProperty(m_typeId, m_intanceId, m_selColorId, value);
                if (value != m_selectionColor)
                {
                    m_selectionColor = value;
                    Changed.Raise(this, EventArgs.Empty);
                }
            }
        }

        [Browsable(false)]
        public ulong InstanceId
        {
            get { return m_intanceId; }
        }

        private DisplayFlagModes m_displayCaption;

        /// <summary>
        /// Gets/Sets value to control display caption on objects.</summary>
        [CategoryAttribute("Render Settings"),
         DescriptionAttribute("Display object name")]
        public DisplayFlagModes DisplayCaption
        {
            get { return m_displayCaption; }
            set
            {
                if (m_displayCaption != value)
                {
                    m_displayCaption = value;
                    Changed.Raise(this, EventArgs.Empty);
                }
            }
        }

        private DisplayFlagModes m_displayBound;

        /// <summary>
        /// Gets/Sets value to control display bounding box of objects.</summary>
        [CategoryAttribute("Render Settings"),
         DescriptionAttribute("Display object bound")]
        public DisplayFlagModes DisplayBound
        {
            get { return m_displayBound; }
            set
            {
                if (m_displayBound != value)
                {
                    m_displayBound = value;
                    Changed.Raise(this, EventArgs.Empty);
                }
            }
        }

        private DisplayFlagModes m_displayPivot;

        /// <summary>
        /// Gets/Sets value to control dispaly pivot of objects.</summary>
        [CategoryAttribute("Render Settings"),
         DescriptionAttribute("Display object pivot")]
        public DisplayFlagModes DisplayPivot
        {
            get { return m_displayPivot; }
            set
            {
                if (m_displayPivot != value)
                {
                    m_displayPivot = value;
                    Changed.Raise(this, EventArgs.Empty);
                }
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            if(m_intanceId != 0)
            {
                GameEngine.DestroyObject(m_typeId, m_intanceId);
                m_typeId = 0;
                m_renderFlagId = 0;
                m_wirecolorId = 0;
                m_selColorId = 0;
                m_intanceId = 0;
            }            
            base.Dispose(disposing);
        }

        // native property ids
        private uint m_renderFlagId;  
        private uint m_wirecolorId;
        private uint m_selColorId;

        // instance id
        private uint m_typeId;
        private ulong m_intanceId;
    }

    public enum DisplayFlagModes
    {
        None,     // don't display the property
        Always,   // always display
        Selection, // display only for selected objects.
    }

   
}
