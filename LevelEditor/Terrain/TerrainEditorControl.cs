//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;


using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

using LevelEditorCore;
using LevelEditorCore.PropertyEditing;

using ATFResources = Sce.Atf.Resources;
using RenderingInterop;

namespace LevelEditor.Terrain
{
    public partial class TerrainEditorControl : UserControl
    {
        public TerrainEditorControl()
        {
            InitializeComponent();

            int itemHieght = m_header + m_thumbSize + 6;
            m_decoList.ItemHeight = itemHieght;
            m_layerList.ItemHeight = itemHieght;

            m_decoList.DrawItem2 += DrawItem;
            m_layerList.DrawItem2 += DrawItem;
            
            m_addBtn.Image = ResourceUtil.GetImage16(ATFResources.AddImage);
            m_deleteBtn.Image = ResourceUtil.GetImage16(ATFResources.RemoveImage);
            m_moveUpBtn.Image = ResourceUtil.GetImage16(ATFResources.ArrowUpImage);
            m_moveDownBtn.Image = ResourceUtil.GetImage16(ATFResources.ArrowDownImage);

            Application.Idle += (sender, e) =>
                {
                    UpdateButtonStatus();
                };

            m_thumbnailReslovers = Globals.MEFContainer.GetExportedValues<IThumbnailResolver>();

            // create no-tex bmp.
            m_noTexBmp = new Bitmap(m_thumbSize, m_thumbSize);
            Graphics g = Graphics.FromImage(m_noTexBmp);
            StringFormat strFormat = new StringFormat();
            strFormat.Alignment = StringAlignment.Center;
            strFormat.LineAlignment = StringAlignment.Center;
            g.Clear(Color.White);
            Rectangle texRect = new Rectangle(0, 0, m_noTexBmp.Width, m_noTexBmp.Height);

            g.DrawString("No\n\rTexture", Font, Brushes.Red, texRect, strFormat);
            strFormat.Dispose();
            g.Dispose();


            m_flattenRdo.Text = "Flatten".Localize();
            m_flattenRdo.Tag = new FlattenBrush(m_flattenRdo.Text);

            m_brushRiseLowRdo.Text = "Raise/Lower".Localize();
            m_brushRiseLowRdo.Tag = new RaiseLowerBrush(m_brushRiseLowRdo.Text);

            m_brushSmoothRdo.Text = "Smooth".Localize();
            m_brushSmoothRdo.Tag = new SmoothenBrush(m_brushSmoothRdo.Text);

            m_noiseRdo.Text = "Noise".Localize();
            m_noiseRdo.Tag = new NoiseBrush(m_noiseRdo.Text);
            
            m_paintEraseRdo.Text = "Paint/Erase".Localize();
            m_paintEraseRdo.Tag = new PaintEraseBrush(m_paintEraseRdo.Text);


            SizeChanged += (sender, e) =>
                {
                    m_propertyGrid.Width = Width - m_propertyGrid.Left;
                    m_brushProps.Width = Width - m_brushProps.Left;                    
                };            
        }

        /// <summary>
        /// Gets or sets splitter distance in pixels.</summary>
        public int SplitterDistance
        {
            get { return splitContainer1.SplitterDistance; }
            set { splitContainer1.SplitterDistance = value; }
        }
        public TerrainGob SelectedTerrain
        {
            get { return (TerrainGob)m_cmboxTerrain.SelectedItem; }
        }

        private TerrainBrush m_selectedBrush;
        public TerrainBrush SelectedBrush
        {
            get { return m_selectedBrush; }
        }

        public TerrainMap SelectedTerrainMap
        {
            get 
            {
                ListBox listbox = GetActiveList();
                return listbox != null ? (TerrainMap)listbox.SelectedItem : null;                
            }
        }

        public IGameContext GameContext
        {
            get;
            set;
        }

        public void PopulatedTerrainCmbox()
        {
            IGameDocumentRegistry gameDocumentRegistry = Globals.MEFContainer.GetExportedValue<IGameDocumentRegistry>();

            IEnumerable<TerrainGob> terrainGobs = EmptyEnumerable<TerrainGob>.Instance;
            if (gameDocumentRegistry != null)
                terrainGobs = gameDocumentRegistry.FindAll<TerrainGob>();
            
            TerrainGob curTerrain = m_cmboxTerrain.Items.Count > 0 ? (TerrainGob)m_cmboxTerrain.SelectedItem : null;
            
            m_cmboxTerrain.Enabled = false;

            m_cmboxTerrain.BeginUpdate();
            m_cmboxTerrain.Items.Clear();
            foreach (var terrain in terrainGobs)
            {
                m_cmboxTerrain.Items.Add(terrain);
            }
            m_cmboxTerrain.EndUpdate();
            if (m_cmboxTerrain.Items.Count > 0)
            {
                m_cmboxTerrain.Enabled = true;
                if (curTerrain != null && m_cmboxTerrain.Items.Contains(curTerrain))
                    m_cmboxTerrain.SelectedItem = curTerrain;
                else
                    m_cmboxTerrain.SelectedIndex = 0;
            }
            ReBind();
        }

        public void ReBind()
        {
            object layermap = m_layerList.SelectedItem;
            object decomap = m_decoList.SelectedItem;
            TerrainGob gob = (TerrainGob)m_cmboxTerrain.SelectedItem;
            Bind(gob);
            if (layermap != null && m_layerList.Items.Contains(layermap))
                m_layerList.SelectedItem = layermap;
            if (decomap != null && m_decoList.Items.Contains(decomap))
                m_decoList.SelectedItem = decomap;
        }

        #region base overrides
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool consumed = false;
            if (keyData == Keys.Delete)
            {
                consumed = true;
            }
            return consumed;
        }
        #endregion

        #region event handlers
       
        private void CreateTerrainBtnClick(object sender, EventArgs e)
        {
            using (var dlg = new CreateTerrainDlg(Globals.ResourceRoot.LocalPath))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {                   
                    TerrainGob terrain = TerrainGob.Create(dlg.TerrainName, dlg.HeightMapPath, dlg.CellSize);
                    var xformable = terrain.As<ITransformable>();
                    xformable.Translation = new Vec3F(-dlg.CellSize * (dlg.HmapCols-1) / 2.0f, 0.0f, -dlg.CellSize * (dlg.HmapRows-1) / 2.0f);
                    xformable.UpdateTransform();

                    ApplicationUtil.Insert(GameContext,
                        GameContext.RootGameObjectFolder,
                        terrain,
                        "Add Terrain",
                        null);
                }
            }
        
        }

        private void CmboxTerrain_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cmbbox = (ComboBox)sender;
            TerrainGob gob = (TerrainGob)cmbbox.SelectedItem;
            Bind(gob);
        }
       
        private void ListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox listbox = GetActiveList();
            if (sender != listbox) return;
            var propcontext = new CustomPropertyEditingContext(listbox.SelectedItem);
            m_propertyGrid.Bind(propcontext);
        }

        private void MoveBtnClick(object sender, EventArgs e)
        {
            TerrainGob gob = (TerrainGob)m_cmboxTerrain.SelectedItem;
            ListBox listbox = GetActiveList();
            if (listbox == null || listbox.SelectedItem == null || gob == null) return;


            int lastIndex = listbox.Items.Count - 1;
            int selectedIndex = listbox.SelectedIndex;
            int insertionIndex = -1;
            if (sender == m_moveUpBtn && selectedIndex > 0)
            {
                insertionIndex = selectedIndex - 1;
            }
            else if (sender == m_moveDownBtn && selectedIndex < lastIndex)
            {
                insertionIndex = selectedIndex + 1;
            }

            if (insertionIndex >= 0)
            {
                // todo: move class NativeGameWorldAdapter to LevelEditorCore
                //       and create an interface for accessing class GameEngine
                RenderingInterop.NativeGameWorldAdapter
                    gw = GameContext.As<RenderingInterop.NativeGameWorldAdapter>();
                try
                {                    
                    gw.ManageNativeObjectLifeTime = false;
                    object item = listbox.Items[selectedIndex];
                    GameContext.As<ITransactionContext>().DoTransaction(() =>
                        {                            
                            // the item must be terrain map.
                            DomNode node = item.As<DomNode>();
                            DomNode parentNode = node.Parent;
                            IList<DomNode> items = parentNode.GetChildList(node.ChildInfo);
                            int itemIndex = items.IndexOf(node);
                            System.Diagnostics.Debug.Assert(itemIndex == selectedIndex);
                            items.RemoveAt(itemIndex);
                            items.Insert(insertionIndex, node);                            
                        }, "Move Map");
                    listbox.SelectedItem = item;
                }
                finally
                {
                    gw.ManageNativeObjectLifeTime = true;
                }
            }
        }
        private void AddBtnClick(object sender, EventArgs e)
        {
            TerrainGob gob = (TerrainGob)m_cmboxTerrain.SelectedItem;
            ListBox listbox = GetActiveList();
            if (gob == null) return;
            ImageData hmImg = gob.GetSurface();

            using (var dlg = new CreateTerrainMapDlg(Globals.ResourceRoot.LocalPath, hmImg.Width, hmImg.Height))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    if (listbox == m_layerList)
                    {
                        LayerMap map = LayerMap.Create(new Uri(dlg.Mask));
                        map.Name = dlg.MapName;
                        GameContext.As<ITransactionContext>().DoTransaction(() =>
                        {
                            gob.LayerMaps.Add(map);
                        }, "Add LayerMap");
                    }
                    else if (listbox == m_decoList)
                    {
                        DecorationMap map = DecorationMap.Create(new Uri(dlg.Mask));
                        map.Name = dlg.MapName;
                        GameContext.As<ITransactionContext>().DoTransaction(() =>
                        {
                            gob.DecorationMaps.Add(map);
                        }, "Add DecorationMap");

                    }
                }
            }
        }
        private void DeleteBtnClick(object sender, EventArgs e)
        {
            ListBox listbox = GetActiveList();
            DomNode item = listbox != null ? Adapters.As<DomNode>(listbox.SelectedItem) : null;
            if (item != null)
            {
                GameContext.As<ITransactionContext>().DoTransaction(() =>
                {
                    item.RemoveFromParent();
                },"Delete terrain map");
                m_propertyGrid.Bind(null);
            }
        }

        private void TabPageSelected(object sender, TabControlEventArgs e)
        {
            ListBox listbox = GetActiveList();
            if (listbox == null) return;
            var propcontext = new CustomPropertyEditingContext(listbox.SelectedItem);
            m_propertyGrid.Bind(propcontext);            
        }
      
        #endregion

        #region Draw list item

                
        private int m_thumbSize = 96;
        private int m_header = 15;
        private void DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            ListBox list = (ListBox)sender;
            TerrainMap map = (TerrainMap)list.Items[e.Index];

            // bounds
            Rectangle bounds = e.Bounds;
            bounds.Width -= 1;
            bounds.Height -= 1;
            
            Brush bkgBrush = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                ? SystemBrushes.Highlight : Brushes.DarkGray;
            // draw bkg
            e.Graphics.FillRectangle(bkgBrush, bounds);                
          
            var thumbnailRect = new Rectangle(bounds.X+5, bounds.Y + m_header, m_thumbSize, m_thumbSize);
                       
            foreach (var texInfo in map.TextureInfos)
            {       
                Image img = GetThumNail(texInfo.Uri);
                var srcRect = new Rectangle(0, 0, img.Width, img.Height);
                e.Graphics.DrawImage(img, thumbnailRect, srcRect, GraphicsUnit.Pixel);
                e.Graphics.DrawRectangle(Pens.Yellow, thumbnailRect);
                SizeF texsize = e.Graphics.MeasureString(texInfo.Name, e.Font);
                var texRect = new RectangleF(thumbnailRect.X + 1, thumbnailRect.Y + 1, texsize.Width, texsize.Height);
                e.Graphics.FillRectangle(Brushes.Gray, texRect);
                e.Graphics.DrawString(texInfo.Name, e.Font, Brushes.White, texRect.Location);
                thumbnailRect.X += (thumbnailRect.Width + 6);
            }
            e.Graphics.DrawString(map.Name, e.Font, SystemBrushes.WindowText, bounds.Location);

        }

        #endregion
        #region helper methods

        private void Bind(TerrainGob gob)
        {            
            m_terrainPropGrid.Bind(gob);
            m_layerList.Items.Clear();
            m_decoList.Items.Clear();
            if (gob != null)
            {
                foreach (var layer in gob.LayerMaps)
                    m_layerList.Items.Add(layer);
                foreach (var deco in gob.DecorationMaps)
                    m_decoList.Items.Add(deco);
            }
            m_propertyGrid.Bind(null);
        }

        private ListBox GetActiveList()
        {
            TerrainGob terrain = (TerrainGob)m_cmboxTerrain.SelectedItem;
            if(terrain != null && m_layersTabControl.SelectedTab != null)
                return (ListBox)m_layersTabControl.SelectedTab.Controls[0];
            return null;
        }        
        private void UpdateButtonStatus()
        {
            //Console.WriteLine("update buttons: " + Environment.TickCount);
            TerrainGob gob = (TerrainGob)m_cmboxTerrain.SelectedItem;
            ListBox listbox = GetActiveList();

            m_addBtn.Enabled = gob != null && listbox != null;
            m_deleteBtn.Enabled = listbox != null && listbox.Items.Count > 0;
            m_moveUpBtn.Enabled = listbox != null && listbox.SelectedIndex > 0;
            m_moveDownBtn.Enabled = listbox != null
                && listbox.SelectedIndex >= 0
                && listbox.SelectedIndex < (listbox.Items.Count - 1);
           
        }

        private Image GetThumNail(Uri ur)
        {
            Image img = null;
            if (ur != null && ur.IsAbsoluteUri && !m_thumbnails.TryGetValue(ur, out img))
            {
                foreach (var resolver in m_thumbnailReslovers)
                {
                    img = resolver.Resolve(ur);
                    if (img != null)                    
                        break;                    
                }
                m_thumbnails.Add(ur, img);
            }
            if (img == null)
            {
                img = m_noTexBmp;
            }

            return img;
        }
        #endregion

        private Dictionary<Uri, Image> m_thumbnails = new Dictionary<Uri, Image>();
        private IEnumerable<IThumbnailResolver> m_thumbnailReslovers;
        private Bitmap m_noTexBmp;

        private void BrushRdoCheckedChanged(object sender, EventArgs e)
        {
            RadioButton button = (RadioButton)sender;
            m_selectedBrush = button.Checked ? (TerrainBrush)button.Tag : null;
            if (m_selectedBrush == null)
                m_brushProps.Bind(new object());                            
            else
                m_brushProps.Bind(m_selectedBrush);            
            
        }
    }

    
    public class DoubleBufferedListBox : ListBox
    {
        [Browsable(true)]
        public EventHandler<DrawItemEventArgs> DrawItem2 = delegate { };

        public DoubleBufferedListBox()
        {            
            this.DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint
               | ControlStyles.AllPaintingInWmPaint
               | ControlStyles.OptimizedDoubleBuffer
               | ControlStyles.ResizeRedraw
               | ControlStyles.Opaque
               , true);

            
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                var itemRect = GetItemRectangle(i);
                itemRect.Height = ItemHeight;
                if (e.ClipRectangle.IntersectsWith(itemRect))
                {
                    if ((this.SelectionMode == SelectionMode.One && this.SelectedIndex == i)
                        || (this.SelectionMode == SelectionMode.MultiSimple && this.SelectedIndices.Contains(i))
                        || (this.SelectionMode == SelectionMode.MultiExtended && this.SelectedIndices.Contains(i)))
                    {
                        DrawItem2(this,new DrawItemEventArgs(e.Graphics, this.Font,
                            itemRect, i,
                            DrawItemState.Selected, this.ForeColor,
                            this.BackColor));
                    }
                    else
                    {
                        DrawItem2(this,new DrawItemEventArgs(e.Graphics, this.Font,
                            itemRect, i,
                            DrawItemState.Default, this.ForeColor,
                            this.BackColor));
                    }
                }
            }// end of loop
        }
    }
}


