//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;
using System.IO;

using RenderingInterop;
namespace LevelEditor.Terrain
{
    /// <summary>
    /// Dialog for creating new terrain</summary>
    public partial class CreateTerrainDlg : Form
    {
        public CreateTerrainDlg(string resourceRoot)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(resourceRoot);
            if (!dirInfo.Exists)
                throw new ArgumentException(resourceRoot + " is not a valid directory");

            ResourceRoot = resourceRoot;
            InitializeComponent();
            
            // populate valid height map sizes
            object[] hmapSizes = { 129, 257, 513, 1025, 2049, 4097, 8193 };

            m_hmapColsCmbox.Items.AddRange(hmapSizes);
            m_hmapColsCmbox.SelectedIndex = 2;

            m_hmapRowsCmbox.Items.AddRange(hmapSizes);
            m_hmapRowsCmbox.SelectedIndex = 2;
             
            object[] cellsizes = {  0.125f,0.25f,0.5f,1.0f,2.0f,4.0f,8.0f,16.0f,32.0f};

            m_cellsizeCmbox.Items.AddRange(cellsizes);
            m_cellsizeCmbox.SelectedIndex = 4;

            ComputeTerrainSize();

        }

        public string TerrainName
        {
            get { return m_terrainNameTxt.Text; }
        }
        public string HeightMapPath
        {
            get { return m_hmtxt.Text; }
        }
        public float CellSize
        {
            get
            {
                float cz;
                float.TryParse(m_cellsizeCmbox.Text, out cz);
                if (cz == 0) cz = 1.0f;
                return cz;
            }
        }

        public int HmapCols
        {
            get
            {
                return m_hmapColsCmbox.SelectedIndex >= 0 ?
                    (int)m_hmapColsCmbox.SelectedItem : 0;
            }
        }

        public int HmapRows
        {
            get
            {
                return m_hmapRowsCmbox.SelectedIndex >= 0 ?
                    (int)m_hmapRowsCmbox.SelectedItem : 0;

            }
        }

        private float ScaleImportedHeightmap
        {
            get
            {
                float sc;
                float.TryParse(m_scaleHmtxt.Text, out sc);
                if (sc == 0) sc = 1.0f;
                return sc;
            }
        }
               
        private void ComputeTerrainSize()
        {
            int u = HmapCols;
            int v = HmapRows;
            float cz = CellSize;
            m_numVertslbl.Text = "Total number of vertices: ";
            if (u > 0 && v > 0 && cz > 0)
            {
                m_terrainSizeLbl.Text
                    = string.Format("Terrain Size: {0} x {1} units", (u * cz), (v * cz));
                m_numVertslbl.Text = "Total number of vertices: " + (u * v);
                    
            }
            else
            {
                m_terrainSizeLbl.Text = "Terrain Size:";                
            }                
        }

        private void SelectedIndexChangedCmbox(object sender, EventArgs e)
        {
            ComputeTerrainSize();
        }        
        private void TextChangedCombox(object sender, EventArgs e)
        {
            ComputeTerrainSize();
        }

        private string ValidateData()
        {
            string error = string.Empty;
            int u = HmapCols;
            int v = HmapRows;
            float cz = CellSize;
            if (u <= 0)
                error += "Height map: Number of columns  must be greater zero" + Environment.NewLine;
            if(v <= 0)
                error += "Height map: number of rows must be greater than zero" + Environment.NewLine;
            if (cz <= 0)
                error += "CellSize must be greater than zero" + Environment.NewLine;
            if(string.IsNullOrWhiteSpace(TerrainName))
                error += "Please specify terrain name" + Environment.NewLine;

            try
            {
                if (string.IsNullOrWhiteSpace(m_hmtxt.Text))
                    throw new Exception("Please choose heightmap");
                string fullPath = Path.GetFullPath(m_hmtxt.Text);                
            }
            catch (Exception ex)
            {
                error += ex.Message + Environment.NewLine;
            }

            try
            {
                float scale = float.Parse(m_scaleHmtxt.Text);
                if (scale <= 0.0f)
                {
                    error += "Import height map scale must be greater than zero";
                }
            }
            catch (Exception ex)
            {
                error += "Error parsing import heightmap scale: " + ex.Message;
            }

            return error;
        }

        private void OKbtnClick(object sender, EventArgs e)
        {
            string error = ValidateData();
            if (string.IsNullOrEmpty(error))
            {
                ImageData hmImg = new ImageData();
                hmImg.InitNew(HmapCols, HmapRows, ImageDataFORMAT.R32_FLOAT);
                if (m_importedHeightMap != null)
                {
                    float scalehm = ScaleImportedHeightmap;
                    if (m_importedHeightMap != null)
                    {
                        // apply imported heightmap.
                        for (int y = 0; y < hmImg.Height; y++)
                        {
                            for (int x = 0; x < hmImg.Width; x++)
                            {
                                float val = m_importedHeightMap.GetPixelFloat(x, y) * scalehm;
                                hmImg.SetPixel(x, y, val);
                            }
                        }
                    }
                }
                hmImg.Save(new Uri(HeightMapPath));
                hmImg.Dispose();

            }
            else
            {
                error = "Error: " + Environment.NewLine + error;
                MessageBox.Show(this, error, "Create Terrain", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult = System.Windows.Forms.DialogResult.OK;
        }


        private readonly string ResourceRoot;

        private void m_chooseHmBtn_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "HeightMap *.dds|*.dds";
                dlg.OverwritePrompt = true;
                dlg.InitialDirectory = ResourceRoot;
                if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    string filename = dlg.FileName;                    
                    string error = string.Empty;
                    if (!filename.StartsWith(ResourceRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        error += "file location must be under resource root" + Environment.NewLine;
                    }

                    if (Path.GetExtension(dlg.FileName).ToLower() != ".dds")
                    {
                        error += "Height map must be DDS file";
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        MessageBox.Show(this, error, "Create Terrain", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    m_hmtxt.Text = dlg.FileName;
                }
            }
        }

        private void m_import_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "HeightMap (*.png; *.dds; *.bmp; *.tga; *.tif)|*.png;*.dds;*.bmp;*.tga;*.tif";
                dlg.CheckFileExists = true;
                dlg.InitialDirectory = ResourceRoot;
                if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    string filename = dlg.FileName;
                    string error = string.Empty;

                    if(m_importedHeightMap != null) 
                        m_importedHeightMap.Dispose();

                    m_importedHeightMap = new ImageData();
                    m_importedHeightMap.LoadFromFile(new Uri(filename));
                    m_importedHeightMap.Convert(ImageDataFORMAT.R32_FLOAT);
                    if (m_importedHeightMap.IsValid)
                    {
                       // set rows and cols.
                        foreach(int c in m_hmapColsCmbox.Items)
                        {
                            if (c >= m_importedHeightMap.Width)
                            {
                                m_hmapColsCmbox.SelectedItem = c;
                                break;
                            }
                        }

                        foreach(int r in m_hmapRowsCmbox.Items)
                        {
                            if (r >= m_importedHeightMap.Height)
                            {
                                m_hmapRowsCmbox.SelectedItem = r;
                                break;
                            }
                        }                        
                    }
                    else
                    {
                        m_importedHeightMap.Dispose();
                        m_importedHeightMap = null;
                        error = "unsupported file type ";
                    }
                        
                    if (!string.IsNullOrEmpty(error))
                    {
                        MessageBox.Show(this, error, "Error importing heightmap", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }                    
                }
            }
        }

        private ImageData m_importedHeightMap;
    }
}
