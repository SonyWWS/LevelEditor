using System;
using System.Drawing;
using LevelEditorCore.VectorMath;
using RenderingInterop;
using Sce.Atf.VectorMath;

namespace LevelEditor.Terrain
{
    /// <summary>
    /// Interface for accessing terrain surface.
    /// Terrain Surface can be heighmap or terrain map.   
    /// </summary>
    public interface ITerrainSurface
    {
        /// <summary>
        /// Gets the native id of the surface.</summary>
        /// <returns>Surface id</returns>
        ulong GetSurfaceInstanceId();

        /// <summary>
        /// Gets surface</summary>
        /// <returns>Surface</returns>
        ImageData GetSurface();

        /// <summary>
        /// Transform the give position in world space 
        /// to surface space.</summary>        
        /// </summary>
        /// <param name="posW">Position in world space</param>
        /// <returns>2d position in surface space</returns>
        Point WorldToSurfaceSpace(Vec3F posW);

        /// <summary>
        /// Apply dirty region from surface in system memory 
        /// to the corresponding surface in GPU memory.</summary>
        /// <param name="box">region to update</param>
        void ApplyDirtyRegion(Bound2di box);
    }
}
