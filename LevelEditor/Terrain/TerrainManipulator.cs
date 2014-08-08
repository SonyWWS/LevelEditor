//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Drawing;
using Sce.Atf;

using Sce.Atf.VectorMath;
using Sce.Atf.Adaptation;

using LevelEditorCore;
using RenderingInterop;

namespace LevelEditor.Terrain
{
    [Export(typeof(IManipulator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TerrainManipulator : IManipulator
    {

        public TerrainManipulator()
        {
            m_manipulatorInfo = new ManipulatorInfo("Terrain".Localize(),
                                         "Activate Terrain editing".Localize(),
                                         LevelEditorCore.Resources.TerrainManipImage,
                                         Keys.None);            
        }
        #region IManipulator Members

        bool IManipulator.Pick(ViewControl vc, System.Drawing.Point scrPt)
        {            
            TerrainGob terrain = m_terrainEditor.TerrainEditorControl.SelectedTerrain;
            TerrainBrush brush = m_terrainEditor.TerrainEditorControl.SelectedBrush;
            if (terrain != null && brush != null)
            {
                FlattenBrush fbrush = brush as FlattenBrush;                
                if (fbrush != null)
                {
                    Ray3F rayw = vc.GetWorldRay(scrPt);
                    TerrainGob.RayPickRetVal retval;
                    if (terrain.RayPick(rayw, out retval))
                    {
                        Point pt = terrain.WorldToSurfaceSpace(retval.hitpos);
                        ImageData hm = terrain.GetSurface();
                        fbrush.Height = hm.GetPixelFloat(pt.X, pt.Y);                        
                    }
                }
                return true;
            }            
            return false;
        }

        void IManipulator.Render(ViewControl vc)
        {
            TerrainGob terrain = m_terrainEditor.TerrainEditorControl.SelectedTerrain;
            TerrainBrush brush = m_terrainEditor.TerrainEditorControl.SelectedBrush;
            TerrainMap terrainMap = m_terrainEditor.TerrainEditorControl.SelectedTerrainMap;
            if (brush == null || (!brush.CanApplyTo(terrain) && !brush.CanApplyTo(terrainMap))) return;
            
            Vec2F drawscale = new Vec2F(1.0f,1.0f);
            if (brush.CanApplyTo(terrainMap))
            {
                ImageData mapImg = terrainMap.GetSurface();
                ImageData hmImg = terrain.GetSurface();
                drawscale.X = (float)hmImg.Width / (float)mapImg.Width;
                drawscale.Y = (float)hmImg.Height / (float)mapImg.Height;
            }

            Point scrPt = vc.PointToClient(Control.MousePosition);
            if (!vc.ClientRectangle.Contains(scrPt)) return;                       
            Ray3F rayw = vc.GetWorldRay(scrPt);            
            TerrainGob.RayPickRetVal retval;
            if (terrain.RayPick(rayw, out retval))
            {
                terrain.DrawBrush(brush, drawscale, retval.hitpos);
            }           
        }
        
        void IManipulator.OnBeginDrag()
        {
            m_tmpOps.Clear();            
        }
                        
        void IManipulator.OnDragging(ViewControl vc, Point scrPt)
        {
            dragged = true;
            ApplyBrush(vc, scrPt);            
        }

        void IManipulator.OnEndDrag(ViewControl vc, Point scrPt)
        {
            if(!dragged)
                ApplyBrush(vc, scrPt);            
            dragged = false;

            if (m_tmpOps.Count > 0)
            {
                TerrainBrush brush = m_terrainEditor.TerrainEditorControl.SelectedBrush;
                string transName = string.Format("Apply {0} brush",brush.Name);

                GameContext context = m_designView.Context.As<GameContext>();
                context.DoTransaction(
                    delegate
                    {
                        foreach (var op in m_tmpOps)
                            context.TransactionOperations.Add(op);
                    }, transName);
                m_tmpOps.Clear();
            }

            if (m_memUsage > MemoryQuota)
            {                
                m_memUsage = 0;
                // removed all the TerrainOps that been GC-ed
                // and re-compute current memory usage.
                List<WeakReference> tmplist = new List<WeakReference>();
                foreach (WeakReference wkref in m_terrainOpList)
                {
                    if (!wkref.IsAlive)
                    {
                        tmplist.Add(wkref);
                    }
                    else
                    {
                        m_memUsage += ((TerrainOp)wkref.Target).SizeInBytes;
                    }
                }

                if (m_memUsage == 0)
                {
                    m_terrainOpList.Clear();
                }
                else
                {
                    foreach (WeakReference wkref in tmplist)
                        m_terrainOpList.Remove(wkref);
                }

                // disable oldest terrainOP to free memory.
                int limit = MemoryQuota - MemoryQuota / 5;                
                foreach (WeakReference wkref in m_terrainOpList)
                {
                    if (m_memUsage < limit) break;
                    TerrainOp op = (TerrainOp)wkref.Target;
                    m_memUsage -= op.SizeInBytes;
                    op.FreeData();
                }                
            }
        }

        private ManipulatorInfo m_manipulatorInfo;
        ManipulatorInfo IManipulator.ManipulatorInfo
        {
            get { return m_manipulatorInfo; }            
        }

        #endregion

        private void ApplyBrush(ViewControl vc, System.Drawing.Point scrPt)
        {
            TerrainGob terrain = m_terrainEditor.TerrainEditorControl.SelectedTerrain;
            TerrainBrush brush = m_terrainEditor.TerrainEditorControl.SelectedBrush;
            TerrainMap terrainMap = m_terrainEditor.TerrainEditorControl.SelectedTerrainMap;
            if (brush == null || (!brush.CanApplyTo(terrain) && !brush.CanApplyTo(terrainMap))) return;

            Ray3F rayw = vc.GetWorldRay(scrPt);
            TerrainGob.RayPickRetVal retval;
            if (terrain.RayPick(rayw, out retval))
            {
                TerrainOp op = null;
                if (brush.CanApplyTo(terrain))
                {
                    Point pt = terrain.WorldToSurfaceSpace(retval.hitpos);
                    brush.Apply(terrain, pt.X, pt.Y, out op);
                }
                else if (brush.CanApplyTo(terrainMap))
                {
                    Point pt = terrainMap.WorldToSurfaceSpace(retval.hitpos);
                    brush.Apply(terrainMap, pt.X, pt.Y, out op);
                }
                m_tmpOps.Add(op);
                m_terrainOpList.Add(new WeakReference(op));
                m_memUsage += op.SizeInBytes;

            }
        }

        [Import(AllowDefault = false)]
        private TerrainEditor m_terrainEditor;

        [Import(AllowDefault = false)]
        private IDesignView m_designView = null;

        private bool dragged;

        // temp list of TerrainOp, 
        // the list will be cleared after each  DoTransaction
        private List<TerrainOp> m_tmpOps = new List<TerrainOp>();

        // keep track of all the terrain operation.
        // when MemoryQuota is exceeded this list is used.
        // to disable the operation and free memory.
        private List<WeakReference> m_terrainOpList = new List<WeakReference>();

        // current memory usage by undo/redo stack measured in bytes.
        private int m_memUsage;

        // Amout of memory allowed to be used by undo/redo
        private const int MemoryQuota = 256 * 1024 * 1024;
    }
}
