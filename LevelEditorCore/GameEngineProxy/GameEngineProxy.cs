//Copyright © 2015 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace LevelEditorCore
{
    /// <summary>
    /// Main interface for communicating 
    /// with game engine.
    /// It exposes a minimum Egnine API surface required for LevelEditing.
    /// This interface is meant to be expanded to support more features.
    /// It is implemented as MEF component 
    /// in a separate project</summary>
    public interface IGameEngineProxy
    {
        /// <summary>
        /// Gets Game Egine information.
        /// The information is retrieved during 
        /// engine initilization.</summary>
        EngineInfo Info { get; }

        /// <summary>
        /// Sets active game world.</summary>
        /// <param name="game">Game world to set</param>
        void SetGameWorld(IGame game);

        /// <summary>
        /// Updates game world</summary>
        /// <param name="time">Frame time</param>
        /// <param name="updateType">Update type</param>        
        void Update(FrameTime time, UpdateType updateType);

        /// <summary>
        /// Wait until all the resources are loaded</summary>
        void WaitForPendingResources();
    }


    /// <summary>
    /// Update types</summary>
    public enum UpdateType
    {
        /// <summary>
        /// Editing, in this mode physics and AI 
        /// should not be updated.
        /// While particle system and other editing related effects
        /// should be updated</summary>
        Editing,

        /// <summary>
        /// GamePlay, update all the subsystems.
        /// Some editing related effects shoult not updated</summary>
        GamePlay,

        /// <summary>
        /// Paused, none of the time based effects are simulated.
        /// Delta time should be zero.
        /// </summary>
        Paused,
    }
}
