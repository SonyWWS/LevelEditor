//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;

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
                        misc, "Rotate on snap".Localize())                     
                };

            m_settingsService.RegisterUserSettings( "Editors".Localize() + "/" + "DesignView".Localize(), userSettings);
            m_settingsService.RegisterSettings(this, userSettings);

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
