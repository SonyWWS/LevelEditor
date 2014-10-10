//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System;
using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Applications;


namespace LevelEditorCore
{
    /// <summary>
    /// DesignView settings
    /// </summary>
    [Export(typeof(IInitializable))]        
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DesignViewSettings : IInitializable
    {

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            
            var snapAngleEditor = new NumericEditor(typeof(float));
            snapAngleEditor.ScaleFactor = 180.0 / Math.PI;
            string misc = "Misc".Localize();
            var userSettings = new System.ComponentModel.PropertyDescriptor[]
                {                                       
                    new BoundPropertyDescriptor(
                        m_designView, () => m_designView.BackColor, "BackgroundColor".Localize(), misc,
                        "Background color".Localize()),
                    new BoundPropertyDescriptor(
                        m_designView, () => m_designView.CameraFarZ, "FarZ".Localize(), misc,
                        "Camera Far Z".Localize()),
                                     
                    new BoundPropertyDescriptor(
                        m_designView, () => m_designView.ControlScheme, "ControlScheme".Localize(), misc,
                        "Control scheme".Localize()),
                                       
                    new BoundPropertyDescriptor(
                        m_designView, () => m_designView.SnapVertex,
                        "SnapVertex".Localize(),
                        misc,"Snap vertex".Localize()),

                    new BoundPropertyDescriptor(
                        m_designView, () => m_designView.RotateOnSnap,
                        "RotateOnSnap".Localize(),
                        misc, "Rotate on snap".Localize()),

                    new BoundPropertyDescriptor(
                        m_designView, () => m_designView.SnapAngle,
                        "Snap Angle".Localize(),
                        misc, "Snap to angle when using rotation manipulator." +
                              "Angle is in degrees. Set it to zero to disable snapping.".Localize(), snapAngleEditor,null)
                     
                };

            m_settingsService.RegisterUserSettings( "Editors".Localize() + "/" + "DesignView".Localize(), userSettings);
            m_settingsService.RegisterSettings(this, userSettings);

            var snapfrom = new BoundPropertyDescriptor(
                         m_designView, () => m_designView.SnapFrom,
                         "SnapMode".Localize(),null, null);
            m_settingsService.RegisterSettings(this, snapfrom);

            if (m_scriptingService != null)
                m_scriptingService.SetVariable("designView", m_designView);
        }

        #endregion
               
        
        [Import(AllowDefault = false)]
        private DesignView m_designView = null;

        [Import(AllowDefault = false)]
        private ISettingsService m_settingsService = null;

        [Import(AllowDefault = false)]
        private ScriptingService m_scriptingService = null;
    }
}
