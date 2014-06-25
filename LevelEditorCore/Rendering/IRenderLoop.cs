//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace LevelEditorCore
{
    /// <summary>
    /// Used for realtime rendering.</summary>
    public interface IRenderLoop
    {        
        /// <summary>
        /// update simultion logic.</summary>        
        void Update(FrameTime t);

        /// <summary>
        /// Render the scene.</summary>
        void Render();

        /// <summary>
        /// Gets and sets realtime mode.</summary>
        bool RealTime
        {
            get;
            set;
        }

    }
}
