//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace LevelEditorCore
{
    /// <summary>
    /// Used by Render loop service to pass total and elasped time 
    /// to RenderLoop client.    
    /// </summary>
    public struct FrameTime
    {        
        /// <summary>
        /// construct FrameTime</summary>        
        public FrameTime(double totalTime, float elapsedTime)
        {
            TotalTime = totalTime;
            ElapsedTime = elapsedTime;
        }

        /// <summary>
        /// Gets total simulation time in seconds</summary>
        public readonly double TotalTime;
        
        /// <summary>
        /// Gets elapsed time in seconds.
        /// since last update</summary>
        public readonly float ElapsedTime;                
    }

    /// <summary>
    /// GameLoop, updates Engine and renders  view ports.
    /// </summary>
    public interface IGameLoop
    {
        /// <summary>
        /// Gets and sets update type.
        /// UpdateType controls how the game engine systems are
        /// updated</summary>
        UpdateType UpdateType { get; set; }
        void Update();
        void Render();
    }
}
