namespace LevelEditor.Terrain
{
    partial class TerrainEditorControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.m_cmboxTerrain = new System.Windows.Forms.ComboBox();
            this.m_layersTabControl = new System.Windows.Forms.TabControl();
            this.m_layersPage = new System.Windows.Forms.TabPage();
            this.m_layerList = new LevelEditor.Terrain.DoubleBufferedListBox();
            this.m_decoPage = new System.Windows.Forms.TabPage();
            this.m_decoList = new LevelEditor.Terrain.DoubleBufferedListBox();
            this.m_propertyGrid = new Sce.Atf.Controls.PropertyEditing.PropertyGrid();
            this.m_toolStrip = new System.Windows.Forms.ToolStrip();
            this.m_addBtn = new System.Windows.Forms.ToolStripButton();
            this.m_deleteBtn = new System.Windows.Forms.ToolStripButton();
            this.m_moveUpBtn = new System.Windows.Forms.ToolStripButton();
            this.m_moveDownBtn = new System.Windows.Forms.ToolStripButton();
            this.m_createTerrainBtn = new System.Windows.Forms.Button();
            this.m_toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.m_noiseRdo = new System.Windows.Forms.RadioButton();
            this.m_brushProps = new Sce.Atf.Controls.PropertyEditing.PropertyGrid();
            this.m_terrainPropGrid = new Sce.Atf.Controls.PropertyEditing.PropertyGrid();
            this.m_paintEraseRdo = new System.Windows.Forms.RadioButton();
            this.m_flattenRdo = new System.Windows.Forms.RadioButton();
            this.m_brushRiseLowRdo = new System.Windows.Forms.RadioButton();
            this.m_brushSmoothRdo = new System.Windows.Forms.RadioButton();
            this.m_layersTabControl.SuspendLayout();
            this.m_layersPage.SuspendLayout();
            this.m_decoPage.SuspendLayout();
            this.m_toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_cmboxTerrain
            // 
            this.m_cmboxTerrain.DisplayMember = "Name";
            this.m_cmboxTerrain.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cmboxTerrain.Enabled = false;
            this.m_cmboxTerrain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_cmboxTerrain.FormattingEnabled = true;
            this.m_cmboxTerrain.Location = new System.Drawing.Point(5, 3);
            this.m_cmboxTerrain.Name = "m_cmboxTerrain";
            this.m_cmboxTerrain.Size = new System.Drawing.Size(177, 24);
            this.m_cmboxTerrain.TabIndex = 0;
            this.m_toolTip.SetToolTip(this.m_cmboxTerrain, "Selected terrain");
            this.m_cmboxTerrain.SelectedIndexChanged += new System.EventHandler(this.CmboxTerrain_SelectedIndexChanged);
            // 
            // m_layersTabControl
            // 
            this.m_layersTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_layersTabControl.Controls.Add(this.m_layersPage);
            this.m_layersTabControl.Controls.Add(this.m_decoPage);
            this.m_layersTabControl.ItemSize = new System.Drawing.Size(53, 20);
            this.m_layersTabControl.Location = new System.Drawing.Point(1, 1);
            this.m_layersTabControl.Name = "m_layersTabControl";
            this.m_layersTabControl.Padding = new System.Drawing.Point(0, 0);
            this.m_layersTabControl.SelectedIndex = 0;
            this.m_layersTabControl.Size = new System.Drawing.Size(346, 282);
            this.m_layersTabControl.TabIndex = 1;
            this.m_layersTabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.TabPageSelected);
            // 
            // m_layersPage
            // 
            this.m_layersPage.Controls.Add(this.m_layerList);
            this.m_layersPage.Location = new System.Drawing.Point(4, 24);
            this.m_layersPage.Name = "m_layersPage";
            this.m_layersPage.Padding = new System.Windows.Forms.Padding(3);
            this.m_layersPage.Size = new System.Drawing.Size(338, 254);
            this.m_layersPage.TabIndex = 0;
            this.m_layersPage.Text = "Layers";
            this.m_layersPage.UseVisualStyleBackColor = true;
            // 
            // m_layerList
            // 
            this.m_layerList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_layerList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_layerList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.m_layerList.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_layerList.FormattingEnabled = true;
            this.m_layerList.HorizontalScrollbar = true;
            this.m_layerList.IntegralHeight = false;
            this.m_layerList.ItemHeight = 20;
            this.m_layerList.Location = new System.Drawing.Point(3, 3);
            this.m_layerList.Name = "m_layerList";
            this.m_layerList.Size = new System.Drawing.Size(332, 248);
            this.m_layerList.TabIndex = 0;
            this.m_layerList.SelectedIndexChanged += new System.EventHandler(this.ListBoxSelectedIndexChanged);
            // 
            // m_decoPage
            // 
            this.m_decoPage.Controls.Add(this.m_decoList);
            this.m_decoPage.Location = new System.Drawing.Point(4, 24);
            this.m_decoPage.Name = "m_decoPage";
            this.m_decoPage.Padding = new System.Windows.Forms.Padding(3);
            this.m_decoPage.Size = new System.Drawing.Size(338, 254);
            this.m_decoPage.TabIndex = 1;
            this.m_decoPage.Text = "Decorations";
            this.m_decoPage.UseVisualStyleBackColor = true;
            // 
            // m_decoList
            // 
            this.m_decoList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_decoList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_decoList.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.m_decoList.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.m_decoList.FormattingEnabled = true;
            this.m_decoList.HorizontalScrollbar = true;
            this.m_decoList.IntegralHeight = false;
            this.m_decoList.ItemHeight = 20;
            this.m_decoList.Location = new System.Drawing.Point(3, 3);
            this.m_decoList.Name = "m_decoList";
            this.m_decoList.Size = new System.Drawing.Size(332, 248);
            this.m_decoList.TabIndex = 0;
            this.m_decoList.SelectedIndexChanged += new System.EventHandler(this.ListBoxSelectedIndexChanged);
            // 
            // m_propertyGrid
            // 
            this.m_propertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_propertyGrid.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.m_propertyGrid.Location = new System.Drawing.Point(350, 25);
            this.m_propertyGrid.Margin = new System.Windows.Forms.Padding(0);
            this.m_propertyGrid.Name = "m_propertyGrid";
            this.m_propertyGrid.PropertySorting = Sce.Atf.Controls.PropertyEditing.PropertySorting.Categorized;
            this.m_propertyGrid.SelectedPropertyDescriptor = null;
            this.m_propertyGrid.Settings = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><PropertyView PropertySort" +
    "ing=\"Categorized\" />";
            this.m_propertyGrid.Size = new System.Drawing.Size(302, 256);
            this.m_propertyGrid.TabIndex = 2;
            this.m_propertyGrid.Text = "propertyGrid1";
            // 
            // m_toolStrip
            // 
            this.m_toolStrip.AllowMerge = false;
            this.m_toolStrip.AutoSize = false;
            this.m_toolStrip.CanOverflow = false;
            this.m_toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.m_toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.m_toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_addBtn,
            this.m_deleteBtn,
            this.m_moveUpBtn,
            this.m_moveDownBtn});
            this.m_toolStrip.Location = new System.Drawing.Point(171, 1);
            this.m_toolStrip.Name = "m_toolStrip";
            this.m_toolStrip.Padding = new System.Windows.Forms.Padding(0);
            this.m_toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.m_toolStrip.Size = new System.Drawing.Size(149, 24);
            this.m_toolStrip.TabIndex = 3;
            // 
            // m_addBtn
            // 
            this.m_addBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_addBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_addBtn.Name = "m_addBtn";
            this.m_addBtn.Size = new System.Drawing.Size(23, 21);
            this.m_addBtn.ToolTipText = "Add";
            this.m_addBtn.Click += new System.EventHandler(this.AddBtnClick);
            // 
            // m_deleteBtn
            // 
            this.m_deleteBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_deleteBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_deleteBtn.Name = "m_deleteBtn";
            this.m_deleteBtn.Size = new System.Drawing.Size(23, 21);
            this.m_deleteBtn.ToolTipText = "delete selected item";
            this.m_deleteBtn.Click += new System.EventHandler(this.DeleteBtnClick);
            // 
            // m_moveUpBtn
            // 
            this.m_moveUpBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_moveUpBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_moveUpBtn.Name = "m_moveUpBtn";
            this.m_moveUpBtn.Size = new System.Drawing.Size(23, 21);
            this.m_moveUpBtn.ToolTipText = "Move up";
            this.m_moveUpBtn.Click += new System.EventHandler(this.MoveBtnClick);
            // 
            // m_moveDownBtn
            // 
            this.m_moveDownBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_moveDownBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_moveDownBtn.Name = "m_moveDownBtn";
            this.m_moveDownBtn.Size = new System.Drawing.Size(23, 21);
            this.m_moveDownBtn.ToolTipText = "Move down";
            this.m_moveDownBtn.Click += new System.EventHandler(this.MoveBtnClick);
            // 
            // m_createTerrainBtn
            // 
            this.m_createTerrainBtn.AutoSize = true;
            this.m_createTerrainBtn.Location = new System.Drawing.Point(188, 3);
            this.m_createTerrainBtn.Name = "m_createTerrainBtn";
            this.m_createTerrainBtn.Size = new System.Drawing.Size(30, 27);
            this.m_createTerrainBtn.TabIndex = 4;
            this.m_createTerrainBtn.TabStop = false;
            this.m_createTerrainBtn.Text = "...";
            this.m_toolTip.SetToolTip(this.m_createTerrainBtn, "...");
            this.m_createTerrainBtn.UseVisualStyleBackColor = true;
            this.m_createTerrainBtn.Click += new System.EventHandler(this.CreateTerrainBtnClick);
            // 
            // m_toolTip
            // 
            this.m_toolTip.ShowAlways = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel1.Controls.Add(this.m_noiseRdo);
            this.splitContainer1.Panel1.Controls.Add(this.m_brushProps);
            this.splitContainer1.Panel1.Controls.Add(this.m_terrainPropGrid);
            this.splitContainer1.Panel1.Controls.Add(this.m_cmboxTerrain);
            this.splitContainer1.Panel1.Controls.Add(this.m_paintEraseRdo);
            this.splitContainer1.Panel1.Controls.Add(this.m_flattenRdo);
            this.splitContainer1.Panel1.Controls.Add(this.m_brushRiseLowRdo);
            this.splitContainer1.Panel1.Controls.Add(this.m_brushSmoothRdo);
            this.splitContainer1.Panel1.Controls.Add(this.m_createTerrainBtn);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel2.Controls.Add(this.m_toolStrip);
            this.splitContainer1.Panel2.Controls.Add(this.m_layersTabControl);
            this.splitContainer1.Panel2.Controls.Add(this.m_propertyGrid);
            this.splitContainer1.Size = new System.Drawing.Size(653, 531);
            this.splitContainer1.SplitterDistance = 245;
            this.splitContainer1.TabIndex = 5;
            this.splitContainer1.TabStop = false;
            // 
            // m_noiseRdo
            // 
            this.m_noiseRdo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.m_noiseRdo.Appearance = System.Windows.Forms.Appearance.Button;
            this.m_noiseRdo.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.m_noiseRdo.FlatAppearance.BorderSize = 2;
            this.m_noiseRdo.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.MenuHighlight;
            this.m_noiseRdo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_noiseRdo.Location = new System.Drawing.Point(338, 150);
            this.m_noiseRdo.Name = "m_noiseRdo";
            this.m_noiseRdo.Size = new System.Drawing.Size(104, 30);
            this.m_noiseRdo.TabIndex = 13;
            this.m_noiseRdo.TabStop = true;
            
            this.m_noiseRdo.UseVisualStyleBackColor = true;
            this.m_noiseRdo.CheckedChanged += new System.EventHandler(this.BrushRdoCheckedChanged);
            // 
            // m_brushProps
            // 
            this.m_brushProps.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_brushProps.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.m_brushProps.Location = new System.Drawing.Point(444, 3);
            this.m_brushProps.Name = "m_brushProps";
            this.m_brushProps.PropertySorting = ((Sce.Atf.Controls.PropertyEditing.PropertySorting)(((Sce.Atf.Controls.PropertyEditing.PropertySorting.Categorized | Sce.Atf.Controls.PropertyEditing.PropertySorting.Alphabetical) 
            | Sce.Atf.Controls.PropertyEditing.PropertySorting.CategoryAlphabetical)));
            this.m_brushProps.SelectedPropertyDescriptor = null;
            this.m_brushProps.Settings = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><PropertyView PropertySort" +
    "ing=\"ByCategory\" />";
            this.m_brushProps.Size = new System.Drawing.Size(206, 242);
            this.m_brushProps.TabIndex = 12;
            this.m_brushProps.Text = "propertyGrid1";
            // 
            // m_terrainPropGrid
            // 
            this.m_terrainPropGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_terrainPropGrid.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.m_terrainPropGrid.Location = new System.Drawing.Point(8, 33);
            this.m_terrainPropGrid.Margin = new System.Windows.Forms.Padding(0);
            this.m_terrainPropGrid.Name = "m_terrainPropGrid";
            this.m_terrainPropGrid.PropertySorting = Sce.Atf.Controls.PropertyEditing.PropertySorting.Categorized;
            this.m_terrainPropGrid.SelectedPropertyDescriptor = null;
            this.m_terrainPropGrid.Settings = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?><PropertyView PropertySort" +
    "ing=\"Categorized\" />";
            this.m_terrainPropGrid.Size = new System.Drawing.Size(312, 212);
            this.m_terrainPropGrid.TabIndex = 11;
            this.m_terrainPropGrid.Text = "propertyGrid2";
            // 
            // m_paintEraseRdo
            // 
            this.m_paintEraseRdo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.m_paintEraseRdo.Appearance = System.Windows.Forms.Appearance.Button;
            this.m_paintEraseRdo.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.m_paintEraseRdo.FlatAppearance.BorderSize = 2;
            this.m_paintEraseRdo.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.MenuHighlight;
            this.m_paintEraseRdo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_paintEraseRdo.Location = new System.Drawing.Point(338, 210);
            this.m_paintEraseRdo.Name = "m_paintEraseRdo";
            this.m_paintEraseRdo.Size = new System.Drawing.Size(104, 30);
            this.m_paintEraseRdo.TabIndex = 10;
            this.m_paintEraseRdo.TabStop = true;
            
            this.m_paintEraseRdo.UseVisualStyleBackColor = true;
            this.m_paintEraseRdo.CheckedChanged += new System.EventHandler(this.BrushRdoCheckedChanged);
            // 
            // m_flattenRdo
            // 
            this.m_flattenRdo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.m_flattenRdo.Appearance = System.Windows.Forms.Appearance.Button;
            this.m_flattenRdo.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.m_flattenRdo.FlatAppearance.BorderSize = 2;
            this.m_flattenRdo.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.MenuHighlight;
            this.m_flattenRdo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_flattenRdo.Location = new System.Drawing.Point(338, 78);
            this.m_flattenRdo.Name = "m_flattenRdo";
            this.m_flattenRdo.Size = new System.Drawing.Size(104, 30);
            this.m_flattenRdo.TabIndex = 9;
            this.m_flattenRdo.TabStop = true;            
            this.m_flattenRdo.UseVisualStyleBackColor = true;
            this.m_flattenRdo.CheckedChanged += new System.EventHandler(this.BrushRdoCheckedChanged);
            // 
            // m_brushRiseLowRdo
            // 
            this.m_brushRiseLowRdo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.m_brushRiseLowRdo.Appearance = System.Windows.Forms.Appearance.Button;
            this.m_brushRiseLowRdo.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.m_brushRiseLowRdo.FlatAppearance.BorderSize = 2;
            this.m_brushRiseLowRdo.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.MenuHighlight;
            this.m_brushRiseLowRdo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_brushRiseLowRdo.Location = new System.Drawing.Point(338, 42);
            this.m_brushRiseLowRdo.Name = "m_brushRiseLowRdo";
            this.m_brushRiseLowRdo.Size = new System.Drawing.Size(104, 30);
            this.m_brushRiseLowRdo.TabIndex = 8;
            this.m_brushRiseLowRdo.TabStop = true;
            
            this.m_brushRiseLowRdo.UseVisualStyleBackColor = true;
            this.m_brushRiseLowRdo.CheckedChanged += new System.EventHandler(this.BrushRdoCheckedChanged);
            // 
            // m_brushSmoothRdo
            // 
            this.m_brushSmoothRdo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.m_brushSmoothRdo.Appearance = System.Windows.Forms.Appearance.Button;
            this.m_brushSmoothRdo.FlatAppearance.BorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.m_brushSmoothRdo.FlatAppearance.BorderSize = 2;
            this.m_brushSmoothRdo.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.MenuHighlight;
            this.m_brushSmoothRdo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_brushSmoothRdo.Location = new System.Drawing.Point(338, 114);
            this.m_brushSmoothRdo.Name = "m_brushSmoothRdo";
            this.m_brushSmoothRdo.Size = new System.Drawing.Size(104, 30);
            this.m_brushSmoothRdo.TabIndex = 7;
            this.m_brushSmoothRdo.TabStop = true;
            
            this.m_brushSmoothRdo.UseVisualStyleBackColor = true;
            this.m_brushSmoothRdo.Click += new System.EventHandler(this.BrushRdoCheckedChanged);
            // 
            // TerrainEditorControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "TerrainEditorControl";
            this.Size = new System.Drawing.Size(653, 531);
            this.m_layersTabControl.ResumeLayout(false);
            this.m_layersPage.ResumeLayout(false);
            this.m_decoPage.ResumeLayout(false);
            this.m_toolStrip.ResumeLayout(false);
            this.m_toolStrip.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox m_cmboxTerrain;
        private System.Windows.Forms.TabControl m_layersTabControl;
        private System.Windows.Forms.TabPage m_layersPage;
        private System.Windows.Forms.TabPage m_decoPage;
        private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyGrid;
        private System.Windows.Forms.ToolStrip m_toolStrip;
        private System.Windows.Forms.ToolStripButton m_addBtn;
        private System.Windows.Forms.ToolStripButton m_deleteBtn;
        private System.Windows.Forms.ToolStripButton m_moveUpBtn;
        private System.Windows.Forms.ToolStripButton m_moveDownBtn;
        private System.Windows.Forms.Button m_createTerrainBtn;
        private System.Windows.Forms.ToolTip m_toolTip;
        private DoubleBufferedListBox m_layerList;
        private DoubleBufferedListBox m_decoList;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RadioButton m_brushSmoothRdo;
        private System.Windows.Forms.RadioButton m_brushRiseLowRdo;
        private System.Windows.Forms.RadioButton m_paintEraseRdo;
        private System.Windows.Forms.RadioButton m_flattenRdo;
        private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_terrainPropGrid;
        private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_brushProps;
        private System.Windows.Forms.RadioButton m_noiseRdo;
    }
}
