//Copyright © 2015 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace LevelEditorCore.GameEngineProxy
{
    /// <summary>
    /// Main interface for communicating 
    /// with game engine.
    /// Implemented as MEF component 
    /// in a separate project</summary>
    public interface IGameEngineProxy
    {
        /// <summary>
        /// Gets Game Egine information.
        /// The information is retrieved during 
        /// engine initilization.</summary>
        EngineInfo Info { get; }

    }
}
