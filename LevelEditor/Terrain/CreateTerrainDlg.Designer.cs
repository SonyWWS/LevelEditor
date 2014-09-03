namespace LevelEditor.Terrain
{
    partial class CreateTerrainDlg
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
                if (m_importedHeightMap != null)
                {
                    m_importedHeightMap.Dispose();
                    m_importedHeightMap = null;
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.m_cancelBtn = new System.Windows.Forms.Button();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_hmapLbl = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.m_terrainSizeLbl = new System.Windows.Forms.Label();
            this.m_hmapColsCmbox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.m_hmapRowsCmbox = new System.Windows.Forms.ComboBox();
            this.m_cellsizeCmbox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.m_terrainNameTxt = new System.Windows.Forms.TextBox();
            this.m_hmtxt = new System.Windows.Forms.TextBox();
            this.m_chooseHmBtn = new System.Windows.Forms.Button();
            this.m_numVertslbl = new System.Windows.Forms.Label();
            this.m_import = new System.Windows.Forms.Button();
            this.m_scaleHmtxt = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.m_tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // m_cancelBtn
            // 
            this.m_cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_cancelBtn.AutoSize = true;
            this.m_cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_cancelBtn.Location = new System.Drawing.Point(369, 173);
            this.m_cancelBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.m_cancelBtn.Name = "m_cancelBtn";
            this.m_cancelBtn.Size = new System.Drawing.Size(56, 23);
            this.m_cancelBtn.TabIndex = 0;
            this.m_cancelBtn.Text = "Cancel";
            this.m_cancelBtn.UseVisualStyleBackColor = true;
            // 
            // m_btnOK
            // 
            this.m_btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnOK.AutoSize = true;
            this.m_btnOK.Location = new System.Drawing.Point(308, 173);
            this.m_btnOK.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.Size = new System.Drawing.Size(56, 23);
            this.m_btnOK.TabIndex = 1;
            this.m_btnOK.TabStop = false;
            this.m_btnOK.Text = "OK";
            this.m_btnOK.UseVisualStyleBackColor = true;
            this.m_btnOK.Click += new System.EventHandler(this.OKbtnClick);
            // 
            // m_hmapLbl
            // 
            this.m_hmapLbl.AutoSize = true;
            this.m_hmapLbl.Location = new System.Drawing.Point(9, 101);
            this.m_hmapLbl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.m_hmapLbl.Name = "m_hmapLbl";
            this.m_hmapLbl.Size = new System.Drawing.Size(83, 13);
            this.m_hmapLbl.TabIndex = 2;
            this.m_hmapLbl.Text = "Heightmap Res:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 125);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Cell Size:";
            // 
            // m_terrainSizeLbl
            // 
            this.m_terrainSizeLbl.AutoSize = true;
            this.m_terrainSizeLbl.Location = new System.Drawing.Point(9, 148);
            this.m_terrainSizeLbl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.m_terrainSizeLbl.Name = "m_terrainSizeLbl";
            this.m_terrainSizeLbl.Size = new System.Drawing.Size(66, 13);
            this.m_terrainSizeLbl.TabIndex = 4;
            this.m_terrainSizeLbl.Text = "Terrain Size:";
            // 
            // m_hmapColsCmbox
            // 
            this.m_hmapColsCmbox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_hmapColsCmbox.FormattingEnabled = true;
            this.m_hmapColsCmbox.Location = new System.Drawing.Point(93, 95);
            this.m_hmapColsCmbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.m_hmapColsCmbox.Name = "m_hmapColsCmbox";
            this.m_hmapColsCmbox.Size = new System.Drawing.Size(48, 21);
            this.m_hmapColsCmbox.TabIndex = 5;
            this.m_hmapColsCmbox.SelectedIndexChanged += new System.EventHandler(this.SelectedIndexChangedCmbox);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(145, 98);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "X";
            // 
            // m_hmapRowsCmbox
            // 
            this.m_hmapRowsCmbox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_hmapRowsCmbox.FormattingEnabled = true;
            this.m_hmapRowsCmbox.Location = new System.Drawing.Point(162, 95);
            this.m_hmapRowsCmbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.m_hmapRowsCmbox.Name = "m_hmapRowsCmbox";
            this.m_hmapRowsCmbox.Size = new System.Drawing.Size(48, 21);
            this.m_hmapRowsCmbox.TabIndex = 7;
            this.m_hmapRowsCmbox.SelectedIndexChanged += new System.EventHandler(this.SelectedIndexChangedCmbox);
            // 
            // m_cellsizeCmbox
            // 
            this.m_cellsizeCmbox.FormattingEnabled = true;
            this.m_cellsizeCmbox.Location = new System.Drawing.Point(93, 119);
            this.m_cellsizeCmbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.m_cellsizeCmbox.Name = "m_cellsizeCmbox";
            this.m_cellsizeCmbox.Size = new System.Drawing.Size(48, 21);
            this.m_cellsizeCmbox.TabIndex = 8;
            this.m_cellsizeCmbox.TextChanged += new System.EventHandler(this.TextChangedCombox);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(91, 79);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(25, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Col.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(160, 79);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Row";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 15);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Name";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 37);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "HeightMap";
            // 
            // m_terrainNameTxt
            // 
            this.m_terrainNameTxt.Location = new System.Drawing.Point(70, 15);
            this.m_terrainNameTxt.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.m_terrainNameTxt.Name = "m_terrainNameTxt";
            this.m_terrainNameTxt.Size = new System.Drawing.Size(116, 20);
            this.m_terrainNameTxt.TabIndex = 13;
            this.m_terrainNameTxt.Text = "Terrain";
            // 
            // m_hmtxt
            // 
            this.m_hmtxt.Location = new System.Drawing.Point(70, 37);
            this.m_hmtxt.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.m_hmtxt.Name = "m_hmtxt";
            this.m_hmtxt.ReadOnly = true;
            this.m_hmtxt.Size = new System.Drawing.Size(300, 20);
            this.m_hmtxt.TabIndex = 14;
            // 
            // m_chooseHmBtn
            // 
            this.m_chooseHmBtn.AutoSize = true;
            this.m_chooseHmBtn.Location = new System.Drawing.Point(374, 34);
            this.m_chooseHmBtn.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.m_chooseHmBtn.Name = "m_chooseHmBtn";
            this.m_chooseHmBtn.Size = new System.Drawing.Size(31, 23);
            this.m_chooseHmBtn.TabIndex = 15;
            this.m_chooseHmBtn.TabStop = false;
            this.m_chooseHmBtn.Text = "...";
            this.m_chooseHmBtn.UseVisualStyleBackColor = true;
            this.m_chooseHmBtn.Click += new System.EventHandler(this.m_chooseHmBtn_Click);
            // 
            // m_numVertslbl
            // 
            this.m_numVertslbl.AutoSize = true;
            this.m_numVertslbl.Location = new System.Drawing.Point(9, 171);
            this.m_numVertslbl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.m_numVertslbl.Name = "m_numVertslbl";
            this.m_numVertslbl.Size = new System.Drawing.Size(124, 13);
            this.m_numVertslbl.TabIndex = 16;
            this.m_numVertslbl.Text = "Total number of vertices:";
            // 
            // m_import
            // 
            this.m_import.Location = new System.Drawing.Point(261, 67);
            this.m_import.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.m_import.Name = "m_import";
            this.m_import.Size = new System.Drawing.Size(143, 25);
            this.m_import.TabIndex = 17;
            this.m_import.Text = "Import Heightmap ...";
            this.m_import.UseVisualStyleBackColor = true;
            this.m_import.Click += new System.EventHandler(this.m_import_Click);
            // 
            // m_scaleHmtxt
            // 
            this.m_scaleHmtxt.Location = new System.Drawing.Point(328, 98);
            this.m_scaleHmtxt.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.m_scaleHmtxt.Name = "m_scaleHmtxt";
            this.m_scaleHmtxt.Size = new System.Drawing.Size(42, 20);
            this.m_scaleHmtxt.TabIndex = 19;
            this.m_scaleHmtxt.Text = "1.0";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(259, 101);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(66, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Scale height";
            // 
            // CreateTerrainDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(434, 205);
            this.Controls.Add(this.m_scaleHmtxt);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.m_import);
            this.Controls.Add(this.m_numVertslbl);
            this.Controls.Add(this.m_chooseHmBtn);
            this.Controls.Add(this.m_hmtxt);
            this.Controls.Add(this.m_terrainNameTxt);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.m_cellsizeCmbox);
            this.Controls.Add(this.m_hmapRowsCmbox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_hmapColsCmbox);
            this.Controls.Add(this.m_terrainSizeLbl);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.m_hmapLbl);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_cancelBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateTerrainDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create new terrain";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button m_cancelBtn;
        private System.Windows.Forms.Button m_btnOK;
        private System.Windows.Forms.Label m_hmapLbl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label m_terrainSizeLbl;
        private System.Windows.Forms.ComboBox m_hmapColsCmbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox m_hmapRowsCmbox;
        private System.Windows.Forms.ComboBox m_cellsizeCmbox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox m_terrainNameTxt;
        private System.Windows.Forms.TextBox m_hmtxt;
        private System.Windows.Forms.Button m_chooseHmBtn;
        private System.Windows.Forms.Label m_numVertslbl;
        private System.Windows.Forms.Button m_import;
        private System.Windows.Forms.TextBox m_scaleHmtxt;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ToolTip m_tooltip;
    }
}