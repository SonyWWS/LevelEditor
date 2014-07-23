//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Drawing;

using Sce.Atf;
using Sce.Atf.VectorMath;
using Sce.Atf.Adaptation;
using Camera = Sce.Atf.Rendering.Camera;

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
                        Point pt = terrain.WorldToHmapSpace(retval.hitpos);
                        ImageData hm = terrain.GetHeightMap();
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
                ImageData mapImg = terrainMap.GetMaskMap();               
                ImageData hmImg = terrain.GetHeightMap();
                drawscale.X = (float)hmImg.Width / (float)mapImg.Width;
                drawscale.Y = (float)hmImg.Height / (float)mapImg.Height;
            }

            System.Drawing.Point scrPt = vc.PointToClient(Control.MousePosition);
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
        }
        
                
        void IManipulator.OnDragging(ViewControl vc, System.Drawing.Point scrPt)
        {
            dragged = true;
            TerrainGob terrain = m_terrainEditor.TerrainEditorControl.SelectedTerrain;
            TerrainBrush brush = m_terrainEditor.TerrainEditorControl.SelectedBrush;
            TerrainMap terrainMap = m_terrainEditor.TerrainEditorControl.SelectedTerrainMap;
            if( brush == null || (!brush.CanApplyTo(terrain) && !brush.CanApplyTo(terrainMap))) return;
                                       
            Ray3F rayw = vc.GetWorldRay(scrPt);
            TerrainGob.RayPickRetVal retval;
            if (terrain.RayPick(rayw, out retval))
            {                
                if (brush.CanApplyTo(terrain))
                {
                    Point pt = terrain.WorldToHmapSpace(retval.hitpos);
                    brush.Apply(terrain, pt.X, pt.Y);                    
                }
                else if (brush.CanApplyTo(terrainMap))
                {
                    Point pt = terrainMap.WorldToMapSpace(retval.hitpos);
                    brush.Apply(terrainMap, pt.X, pt.Y);                    
                }

            }
        }

        void IManipulator.OnEndDrag(ViewControl vc, System.Drawing.Point scrPt)
        {
            TerrainGob terrain = m_terrainEditor.TerrainEditorControl.SelectedTerrain;
            TerrainBrush brush = m_terrainEditor.TerrainEditorControl.SelectedBrush;
            TerrainMap terrainMap = m_terrainEditor.TerrainEditorControl.SelectedTerrainMap;
            if (brush == null || (!brush.CanApplyTo(terrain) && !brush.CanApplyTo(terrainMap))) return;

            if (!dragged && terrain != null && brush != null)
            {
                Camera camera = vc.Camera;
                Ray3F rayw = vc.GetWorldRay(scrPt);
                TerrainGob.RayPickRetVal retval;
                if (terrain.RayPick(rayw, out retval))
                {
                    if (brush.CanApplyTo(terrain))
                    {
                        Point pt = terrain.WorldToHmapSpace(retval.hitpos);
                        brush.Apply(terrain, pt.X, pt.Y);
                    }
                    else if (brush.CanApplyTo(terrainMap))
                    {
                        Point pt = terrainMap.WorldToMapSpace(retval.hitpos);
                        brush.Apply(terrainMap, pt.X, pt.Y);
                    }
                }                
            }            
            dragged = false;
        }

        private ManipulatorInfo m_manipulatorInfo;
        ManipulatorInfo IManipulator.ManipulatorInfo
        {
            get { return m_manipulatorInfo; }            
        }

        #endregion

        [Import(AllowDefault = false)]
        private TerrainEditor m_terrainEditor;
        
        private bool dragged;

      
    }
}
