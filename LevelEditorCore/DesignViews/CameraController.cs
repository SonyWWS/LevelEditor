//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Applications;

namespace LevelEditorCore
{
    /// <summary>
    /// Base class for CameraControllers</summary>
    public abstract class CameraController : Sce.Atf.Rendering.CameraController
    {       
        /// <summary>
        /// Gets or sets CommandInfo for this controller</summary>
        public CommandInfo CommandInfo { get; protected set; } 
    }
}
