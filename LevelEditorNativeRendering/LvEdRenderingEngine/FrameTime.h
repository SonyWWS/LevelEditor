//Copyright © 2015 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

namespace LvEdEngine
{

// FrameTime contains used by Update() 
// This is also defiened in C# side.
struct FrameTime
{   
    // Gets total simulation time in seconds.
    double TotalTime;
        
    // Gets elapsed time in seconds.
    // since last update.
    float ElapsedTime;             
};

// Update types.
// This is also defiened in C# side.
namespace UpdateType
{
enum Enum
{    
    // Editing, in this mode physics and AI 
    // should not be updated.
    // While particle system and other editing related effects
    // should be updated
    Editing,
    
    // GamePlay, update all the subsystems.
    // Some editing related effects shoult not updated.
    GamePlay,
    
    // Paused, none of the time based effects are simulated.
    // Delta time should be zero.
    Paused,
};
}
typedef UpdateType::Enum UpdateTypeEnum;

}