//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Rendering;
using Sce.Atf.Rendering.Dom;

using LevelEditorCore;

using Resources = LevelEditorCore.Resources;

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


            cmdInfo = m_commandService.RegisterCommand(                
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
            m_propGrid = new PropertyGrid();
            m_propGrid.PropertyValueChanged += delegate
            {
                m_designView.InvalidateViews();
            };
            
            m_controlHostService.RegisterControl(m_propGrid, controlInfo, null);
            
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

            if (activeControl.RenderState != m_propGrid.SelectedObject)
            {
                m_propGrid.SelectedObject = activeControl.RenderState;
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
                m_propGrid.Refresh();
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

        private PropertyGrid m_propGrid;
        
        [Import(AllowDefault = false)]
        private ICommandService m_commandService;

        [Import(AllowDefault = false)]
        private IControlHostService m_controlHostService;

        [Import(AllowDefault = false)]
        private IDesignView m_designView = null;
        
        [Import(AllowDefault = false)]
        private ScriptingService m_scriptingService;

        private string m_commandGroup = "RenderingModes";        
    }
}
