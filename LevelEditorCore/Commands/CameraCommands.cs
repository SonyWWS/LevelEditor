//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace LevelEditorCore.Commands
{
    /// <summary>
    /// Commands for switching the DesignView's active camera</summary>
    [Export(typeof(CameraCommands))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CameraCommands : ICommandClient, IInitializable
    {
        #region IInitializable Implementation

        void IInitializable.Initialize()
        {
            // register a custom menu without any commands, so it won't appear in the main menu bar
            var cameraMenu = CommandService.RegisterMenu(this, "Camera", "Camera");
            ToolStrip strip = cameraMenu.GetToolStrip();
            foreach (var cameraController in m_cameraControllers)
            {                
                if (cameraController.CommandInfo != null)
                {
                    CommandService.RegisterCommand(cameraController.CommandInfo, this);
                    strip.Items.Add(cameraController.CommandInfo.GetButton());
                }                    
            }

            SettingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(this, () => CameraMode, "CameraMode", null, null));
        }

        #endregion

        #region ICommandClient Implementation

        /// <summary>
        /// Checks whether the client can do the command if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>true, if client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            var camController = commandTag as CameraController;
            return camController != null && camController.CanHandleCamera(m_designView.ActiveView.Camera);
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public void DoCommand(object commandTag)
        {
            var camController = commandTag as CameraController;            
            var designControl = m_designView.ActiveView;
            if(camController != null && camController.CanHandleCamera(designControl.Camera))
            {
                designControl.CameraController = (CameraController)Activator.CreateInstance(camController.GetType());
            }            
            designControl.Invalidate();
        }

        /// <summary>
        /// Updates command state for given command. Only called if CanDoCommand is true.</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, CommandState state)
        {
            var camController = commandTag as CameraController;
            if (camController == null)
                return;            
            var designControl = m_designView.ActiveView;
            state.Check = designControl.CameraController.GetType() == camController.GetType();
        }

        #endregion

        /// <summary>
        /// Gets and sets the currently selected camera controllers 
        /// for each DesignView control. This string is
        /// persisted in the user's settings file.
        /// </summary>
        public string CameraMode
        {
            get 
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
                XmlElement root = xmlDoc.CreateElement("CameraControllers");
                xmlDoc.AppendChild(root);
                foreach (var view in m_designView.AllViews)
                {
                    if (string.IsNullOrWhiteSpace(view.Name))
                        continue;
                    XmlElement elm = xmlDoc.CreateElement("ViewControl");
                    elm.SetAttribute("Name", view.Name);
                    elm.SetAttribute("CamController", view.CameraController.GetType().AssemblyQualifiedName);
                    root.AppendChild(elm);                  
                }
                return xmlDoc.InnerXml;
            }
            set
            {

                try
                {                  
                    if (string.IsNullOrEmpty(value))
                        return;
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(value);
                    XmlElement root = xmlDoc.DocumentElement;
                    if (root == null) return;
                    foreach (XmlElement elm in root.ChildNodes)
                    {
                        var view = GetViewByName(elm.GetAttribute("Name"));
                        Type type = Type.GetType(elm.GetAttribute("CamController"));
                        if (view != null && type != null)
                            view.CameraController = (CameraController)Activator.CreateInstance(type);
                    }

                }
                catch (Exception ex)
                {
                    Outputs.WriteLine(
                        OutputMessageType.Error,
                        "{0}: Exception loading CameraMode persisted settings: {1}", this, ex.Message);
                }
            }            
        }

        private DesignViewControl GetViewByName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                foreach (var view in m_designView.AllViews)
                    if (view.Name == name) return view;
            }
            return null;
        }
        /// <summary>
        /// Gets or sets the command service to use.</summary>
        [Import]
        public ICommandService CommandService { get; set; }

        /// <summary>
        /// Gets or sets the settings service to use.</summary>
        [Import]
        public ISettingsService SettingsService { get; set; }

        [Import(AllowDefault = false)]
        private IDesignView m_designView = null;

        [ImportMany]
        private IEnumerable<CameraController> m_cameraControllers;
    }
}