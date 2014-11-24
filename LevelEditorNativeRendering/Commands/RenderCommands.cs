//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Collections.Generic;

using Sce.Atf;
using Sce.Atf.Applications;

using LevelEditorCore;
using Sce.Atf.Controls.PropertyEditing;
using Resources = LevelEditorCore.Resources;
using PropertyDescriptor = System.ComponentModel.PropertyDescriptor;

namespace RenderingInterop
{   
    /// <summary>
    /// Commands for switching the active DesignControl's rendering mode</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(RenderCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RenderCommands : ICommandClient, IInitializable
    {       
        #region IInitializable Members

        public virtual void Initialize()
        {
            // register a custom menu without any commands, so it won't appear in the main menu bar
            MenuInfo menuInfo = m_commandService.RegisterMenu(this, "RenderModes", "Rendering modes");
            ToolStrip strip = menuInfo.GetToolStrip();
          
             CommandInfo cmdInfo = m_commandService.RegisterCommand(                
                Command.RenderSmooth,
                StandardMenu.View,
                m_commandGroup,
                "Solid".Localize(),
                "Solid rendering".Localize(),
                Keys.None,
                Resources.SmoothImage,
                CommandVisibility.Menu,
                this);
             strip.Items.Add(cmdInfo.GetButton());

             cmdInfo = m_commandService.RegisterCommand(                
                Command.RenderWireFrame,
                StandardMenu.View,
                m_commandGroup,
                "Wireframe".Localize(),
                "Wireframe rendering".Localize(),
                Keys.None,
                Resources.WireframeImage,
                CommandVisibility.Menu,
                this);
             strip.Items.Add(cmdInfo.GetButton());

            cmdInfo = m_commandService.RegisterCommand(                
                Command.RenderOutlined,
                StandardMenu.View,
                m_commandGroup,
                "SolidOverWire",
                "Solid over wireframe rendering".Localize(),
                Keys.None,
                Resources.OutlinedImage,
                CommandVisibility.Menu,
                this);
            strip.Items.Add(cmdInfo.GetButton());

            cmdInfo = m_commandService.RegisterCommand(                
                Command.RenderTextured,
                StandardMenu.View,
                m_commandGroup,
                "Textured".Localize(),
                "Textured rendering".Localize(),
                Keys.T,
                Resources.TexturedImage,
                CommandVisibility.Menu,
                this);
            strip.Items.Add(cmdInfo.GetButton());


            cmdInfo = m_commandService.RegisterCommand(                
                Command.RenderLight,
                StandardMenu.View,
                m_commandGroup,
                "Lighting".Localize(),
                "Lighting".Localize(),
                Keys.L,
                Resources.LightImage,
                CommandVisibility.Menu,
                this);
            strip.Items.Add(cmdInfo.GetButton());

            cmdInfo = m_commandService.RegisterCommand(                
                Command.RenderBackFace,
                StandardMenu.View,
                m_commandGroup,
                "BackFace".Localize(),
                "Render back faces".Localize(),
                Keys.B,
                Resources.BackfaceImage,
                CommandVisibility.Menu,
                this);
            strip.Items.Add(cmdInfo.GetButton());

            cmdInfo = m_commandService.RegisterCommand(
               Command.RenderShadow,
               StandardMenu.View,
               m_commandGroup,
               "Shadow".Localize(),
               "Render shadow".Localize(),
               Keys.None,
               Resources.ShadowImage,
               CommandVisibility.Menu,
               this);
            strip.Items.Add(cmdInfo.GetButton());


            cmdInfo = m_commandService.RegisterCommand(
             Command.RenderNormals,
             StandardMenu.View,
             m_commandGroup,
             "Normals".Localize(),
             "Render Normals".Localize(),
             Keys.None,
             Resources.NormalImage,
             CommandVisibility.Menu,
             this);
            strip.Items.Add(cmdInfo.GetButton());

            m_commandService.RegisterCommand(                
                Command.RenderCycle,
                StandardMenu.View,
                m_commandGroup,
                "CycleRenderModes".Localize(),
                "Cycle render modes".Localize(),
                Keys.Space,
                null,
                CommandVisibility.Menu,
                this);
          
            //cmdInfo = m_commandService.RegisterCommand(
            //  Command.RealTime,
            //  StandardMenu.View,
            //  m_commandGroup,
            //  "RealTime".Localize(),
            //  "Toggle real time rendering".Localize(),
            //  Keys.None,
            //  Resources.RealTimeImage,
            //  CommandVisibility.Menu,
            //  this);
            //strip.Items.Add(cmdInfo.GetButton());

            ControlInfo controlInfo = new ControlInfo("Render settings", "per view port render settings", StandardControlGroup.Hidden);
            
            m_propertyGrid = new Sce.Atf.Controls.PropertyEditing.PropertyGrid();
            m_controlHostService.RegisterControl(m_propertyGrid, controlInfo, null);                       

            if (m_scriptingService != null)
                m_scriptingService.SetVariable("renderCommands", this);
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Can the client do the command?</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>true, iff client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return false;

            NativeDesignControl activeControl = (NativeDesignControl)m_designView.ActiveView;
            RenderState rs = activeControl.RenderState;

            var context = m_propertyGrid.PropertyGridView.EditingContext as RenderStateEditingContext;
            if (context == null || context.Item != rs)
            {
                context = new RenderStateEditingContext(rs);                
                m_propertyGrid.Bind(context);
            }
            
                      
            switch ((Command)commandTag)
            {
                case Command.RenderSmooth:
                case Command.RenderWireFrame:
                case Command.RenderOutlined:
                case Command.RenderLight:
                case Command.RenderBackFace:
                case Command.RenderShadow:
                case Command.RenderNormals:
                case Command.RenderCycle:
              //  case Command.RealTime:
                    return true;
                case Command.RenderTextured:
                    return (rs.RenderFlag & GlobalRenderFlags.Solid) != 0;
                
            }

            return false;
        }

        /// <summary>
        /// Do a command</summary>
        /// <param name="commandTag">Command</param>
        public void DoCommand(object commandTag)
        {
            if (commandTag is Command)
            {
                NativeDesignControl control = (NativeDesignControl)m_designView.ActiveView;
                RenderState rs = control.RenderState;
                
                switch ((Command)commandTag)
                {
                    case Command.RenderSmooth:
                        rs.RenderFlag &= ~(GlobalRenderFlags.WireFrame | GlobalRenderFlags.RenderBackFace);
                        rs.RenderFlag |= (GlobalRenderFlags.Solid | GlobalRenderFlags.Lit | GlobalRenderFlags.Textured);
                        
                        break;

                    case Command.RenderWireFrame:
                        rs.RenderFlag |= (GlobalRenderFlags.WireFrame ); //| RenderFlags.RenderBackFace
                        rs.RenderFlag &= ~(GlobalRenderFlags.Solid | GlobalRenderFlags.Lit | GlobalRenderFlags.Textured);
                        
                        break;

                    case Command.RenderOutlined:
                        rs.RenderFlag |= (GlobalRenderFlags.WireFrame | GlobalRenderFlags.Solid |
                            GlobalRenderFlags.Lit | GlobalRenderFlags.Textured);
                        rs.RenderFlag &= ~GlobalRenderFlags.RenderBackFace;
                        
                        break;

                    case Command.RenderTextured:
                        rs.RenderFlag ^= GlobalRenderFlags.Textured;
                        
                        break;

                    case Command.RenderLight:
                        rs.RenderFlag ^= GlobalRenderFlags.Lit;
                        
                        break;

                    case Command.RenderBackFace:
                        rs.RenderFlag ^= GlobalRenderFlags.RenderBackFace;
                        break;

                    case Command.RenderShadow:
                        rs.RenderFlag ^= GlobalRenderFlags.Shadows;
                        break;

                    case Command.RenderNormals:
                        rs.RenderFlag ^= GlobalRenderFlags.RenderNormals;
                        break;

                    case Command.RenderCycle:
                        GlobalRenderFlags flags = rs.RenderFlag;

                        if ((flags & GlobalRenderFlags.Solid) != 0 &&
                            (flags & GlobalRenderFlags.WireFrame) != 0)
                        {
                            // outlined -> smooth
                            goto case Command.RenderSmooth;
                        }
                        if ((flags & GlobalRenderFlags.Solid) != 0)
                        {
                            // smooth -> wireframe
                            goto case Command.RenderWireFrame;
                        }
                        // wireframe -> outlined
                        goto case Command.RenderOutlined;
                        
                  //  case Command.RealTime:
                  //      m_designView.RealTime = !m_designView.RealTime;
                  //      break;
                }
                control.Invalidate();                
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, Sce.Atf.Applications.CommandState state)
        {
            if (commandTag is Command)
            {
                NativeDesignControl control = (NativeDesignControl)m_designView.ActiveView;
                GlobalRenderFlags flags = control.RenderState.RenderFlag;
                switch ((Command)commandTag)
                {
                    case Command.RenderSmooth:
                        state.Check = (flags & GlobalRenderFlags.Solid) != 0;
                        break;

                    case Command.RenderWireFrame:
                        state.Check = (flags & GlobalRenderFlags.WireFrame) != 0;
                        break;

                    case Command.RenderOutlined:
                        state.Check = (flags & GlobalRenderFlags.Solid) != 0 &&
                                      (flags & GlobalRenderFlags.WireFrame) != 0;
                        break;

                    case Command.RenderTextured:
                        state.Check = (flags & GlobalRenderFlags.Textured) != 0;
                        break;

                    case Command.RenderLight:
                        state.Check = ((flags & GlobalRenderFlags.Lit) != 0);
                        break;

                    case Command.RenderBackFace:
                        state.Check = (flags & GlobalRenderFlags.RenderBackFace) != 0;
                        break;
                    case Command.RenderShadow:
                        state.Check = (flags & GlobalRenderFlags.Shadows) == GlobalRenderFlags.Shadows;
                        break;

                    case Command.RenderNormals:
                        state.Check = (flags & GlobalRenderFlags.RenderNormals) == GlobalRenderFlags.RenderNormals;
                        break;
                   // case Command.RealTime:
                  //      state.Check = m_designView.RealTime;                  

                }
            }
        }

        #endregion
        private enum Command
        {
            RenderSmooth,
            RenderWireFrame,
            RenderOutlined,
            RenderTextured,
            RenderLight,
            RenderBackFace,
            RenderShadow,
            RenderNormals,
            RenderCycle,
          //  RealTime
        }

        private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyGrid;
              
        [Import(AllowDefault = false)]
        private ICommandService m_commandService;

        [Import(AllowDefault = false)]
        private IControlHostService m_controlHostService;

        [Import(AllowDefault = false)]
        private IDesignView m_designView = null;
        
        [Import(AllowDefault = true)]
        private ScriptingService m_scriptingService;

        private string m_commandGroup = "RenderingModes";

        private class RenderStateEditingContext : IPropertyEditingContext
        {
            public RenderStateEditingContext(RenderState rs)
            {
                m_items[0] = rs;
            }
            public object Item
            {
                get { return m_items[0];}
            }
            #region IPropertyEditingContext Members

            public IEnumerable<object> Items
            {
                get { return m_items; }
            }

            public IEnumerable<PropertyDescriptor> PropertyDescriptors
            {
                get 
                {
                    if (s_propertyDescriptor == null)
                    {                        
                        var colorEd = new Sce.Atf.Controls.ColorPickerEditor();
                        string category = "Render Settings".Localize();                        
                        
                        s_propertyDescriptor = new PropertyDescriptor[]
                        {
                            new UnboundPropertyDescriptor(typeof(RenderState),"WireFrameColor","Wire FrameColor".Localize(),category,"color used for wireframe mode".Localize(),colorEd),
                            new UnboundPropertyDescriptor(typeof(RenderState),"SelectionColor","Selection Color".Localize(),category,"Wireframe color for selected objects".Localize(),colorEd),
                            new UnboundPropertyDescriptor(typeof(RenderState),"DisplayCaption","Display Caption".Localize(),category,"Display object name".Localize()),
                            new UnboundPropertyDescriptor(typeof(RenderState),"DisplayBound","Display Bound".Localize(),category,"Display objects' bounding volume".Localize()),
                            new UnboundPropertyDescriptor(typeof(RenderState),"DisplayPivot","Display Pivot".Localize(),category,"Display object pivot".Localize())
                        };
                    }
                    return s_propertyDescriptor;
                }
            }

            #endregion
            private readonly object[] m_items = new object[1];
            private static PropertyDescriptor[] s_propertyDescriptor;
        }
    }
}
