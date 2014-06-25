//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;

using Sce.Atf.Adaptation;

using LevelEditorCore;

namespace LevelEditor
{
    /// <summary>
    /// Default snap filter.
    /// A snap filter used to filter objects that 
    /// cannot be used a snap anchor.</summary>
    [Export(typeof(ISnapFilter))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SnapFilter : ISnapFilter
    {

        #region ISnapFilter Members

        bool ISnapFilter.CanSnapTo(object dragObj, object snapObj)  
        {
            if (snapObj == null 
                || snapObj.Is<ILinear>()
                || snapObj.Is<IControlPoint>())
                return false;

            return true;
        }

        #endregion
    }
}
