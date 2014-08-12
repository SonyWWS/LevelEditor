//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;
using System.IO;

using RenderingInterop;

namespace LevelEditor.Terrain
{
    /// <summary>
    /// UI for Creating terrain map</summary>
    public partial class CreateTerrainMapDlg : Form
    {
        public CreateTerrainMapDlg(string resourceRoot,int hmWidth, int hmHeight)
        {
            if (!(hmWidth > 0 && hmHeight > 0))
                throw new ArgumentOutOfRangeException();
            
            InitializeComponent();
            m_heightTxt.Text = hmWidth.ToString();
            m_widthTxt.Text = hmHeight.ToString();
            ResourceRoot = resourceRoot;
        }

        private void m_btnOK_Click(object sender, EventArgs e)
        {
            string error = ValidateData();
            if (string.IsNullOrWhiteSpace(error))
            {
                // create mask map.
                ImageData maskImg = new ImageData();
                maskImg.InitNew(MaskWidth, MaskHeight, ImageDataFORMAT.R8_UNORM);
                if (m_importedMask != null)
                {
                    for (int y = 0; y < maskImg.Height; y++)
                    {
                        for (int x = 0; x < maskImg.Width; x++)
                        {
                            byte src = m_importedMask.GetPixelByte(x, y);
                            maskImg.SetPixel(x, y, src);
                        }
                    }

                    m_importedMask.Dispose();
                    m_importedMask = null;
                }
                maskImg.Save(new Uri(m_maskTxt.Text));
                maskImg.Dispose();
            }
            else
            {
                error = "Error: " + Environment.NewLine + error;
                MessageBox.Show(this, error, "Create Terrain", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            DialogResult = System.Windows.Forms.DialogResult.OK;

        }

        public string MapName
        {
            get { return m_terrainMapNameTxt.Text; }
        }
        public int MaskWidth
        {
            get
            {
                int width;
                int.TryParse(m_widthTxt.Text, out width);
                return width;
            }
        }

        public int MaskHeight
        {
            get
            {
                int height;
                int.TryParse(m_heightTxt.Text, out height);
                return height;
            }
        }

        public string Mask
        {
            get { return m_maskTxt.Text; }            
        }

        
        private string ValidateData()
        {
            string error = string.Empty;

            if (string.IsNullOrWhiteSpace(m_terrainMapNameTxt.Text))
                error += "Please specify terrain map name" + Environment.NewLine;

            try
            {
                if (string.IsNullOrWhiteSpace(m_maskTxt.Text))
                    throw new Exception("Please choose mask file");
                string fullPath = Path.GetFullPath(m_maskTxt.Text);
            }
            catch (Exception ex)
            {
                error += ex.Message + Environment.NewLine;
            }

            if (MaskWidth <= 0)
                error += "Mask map: Width  must be greater zero" + Environment.NewLine;
            if (MaskHeight <= 0)
                error += "Mask map: Height must be greater than zero" + Environment.NewLine;                        
            return error;           
        }

        private void m_import_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Mask (*.png; *.dds; *.bmp; *.tga; *.tif)|*.png;*.dds;*.bmp;*.tga;*.tif";
                dlg.CheckFileExists = true;
                dlg.InitialDirectory = ResourceRoot;
                if (dlg.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    string filename = dlg.FileName;
                    string error = string.Empty;
                    m_importedMask = new ImageData();
                    m_importedMask.LoadFromFile(new Uri(filename));
                    m_importedMask.Convert(ImageDataFORMAT.R8_UNORM);
                    if (m_importedMask.IsValid)
                    {
                        m_widthTxt.Text = m_importedMask.Width.ToString();
                        m_heightTxt.Text = m_importedMask.Height.ToString();                        
                    }
                    else
                    {
                        m_importedMask.Dispose();
                        m_importedMask = null;
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

        private void m_chooseMaskBtn_Click(object sender, EventArgs e)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "Mask *.dds|*.dds";
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
                    m_maskTxt.Text = dlg.FileName;
                }
            }
        }

        private ImageData m_importedMask;
        private readonly string ResourceRoot;
    }
}
