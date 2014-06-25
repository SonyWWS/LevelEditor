namespace LevelEditor.Terrain
{
    partial class CreateTerrainMapDlg
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_cancelBtn = new System.Windows.Forms.Button();
            this.m_terrainMapNameTxt = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.m_chooseMaskBtn = new System.Windows.Forms.Button();
            this.m_maskTxt = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.m_hmapLbl = new System.Windows.Forms.Label();
            this.m_heightTxt = new System.Windows.Forms.TextBox();
            this.m_widthTxt = new System.Windows.Forms.TextBox();
            this.m_import = new System.Windows.Forms.Button();
            this.m_tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // m_btnOK
            // 
            this.m_btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnOK.Location = new System.Drawing.Point(392, 126);
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.Size = new System.Drawing.Size(75, 23);
            this.m_btnOK.TabIndex = 3;
            this.m_btnOK.TabStop = false;
            this.m_btnOK.Text = "OK";
            this.m_btnOK.UseVisualStyleBackColor = true;
            this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
            // 
            // m_cancelBtn
            // 
            this.m_cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_cancelBtn.Location = new System.Drawing.Point(473, 126);
            this.m_cancelBtn.Name = "m_cancelBtn";
            this.m_cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.m_cancelBtn.TabIndex = 2;
            this.m_cancelBtn.Text = "Cancel";
            this.m_cancelBtn.UseVisualStyleBackColor = true;
            // 
            // m_terrainMapNameTxt
            // 
            this.m_terrainMapNameTxt.Location = new System.Drawing.Point(63, 9);
            this.m_terrainMapNameTxt.Name = "m_terrainMapNameTxt";
            this.m_terrainMapNameTxt.Size = new System.Drawing.Size(168, 22);
            this.m_terrainMapNameTxt.TabIndex = 15;
            this.m_terrainMapNameTxt.Text = "TerrainMap";
            this.m_tooltip.SetToolTip(this.m_terrainMapNameTxt, "Terrain map name");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(45, 17);
            this.label5.TabIndex = 14;
            this.label5.Text = "Name";
            this.m_tooltip.SetToolTip(this.label5, "Terrain map name");
            // 
            // m_chooseMaskBtn
            // 
            this.m_chooseMaskBtn.Location = new System.Drawing.Point(498, 46);
            this.m_chooseMaskBtn.Name = "m_chooseMaskBtn";
            this.m_chooseMaskBtn.Size = new System.Drawing.Size(41, 23);
            this.m_chooseMaskBtn.TabIndex = 18;
            this.m_chooseMaskBtn.TabStop = false;
            this.m_chooseMaskBtn.Text = "...";
            this.m_tooltip.SetToolTip(this.m_chooseMaskBtn, "Choose mask file");
            this.m_chooseMaskBtn.UseVisualStyleBackColor = true;
            this.m_chooseMaskBtn.Click += new System.EventHandler(this.m_chooseMaskBtn_Click);
            // 
            // m_maskTxt
            // 
            this.m_maskTxt.Location = new System.Drawing.Point(90, 47);
            this.m_maskTxt.Name = "m_maskTxt";
            this.m_maskTxt.ReadOnly = true;
            this.m_maskTxt.Size = new System.Drawing.Size(402, 22);
            this.m_maskTxt.TabIndex = 17;
            this.m_tooltip.SetToolTip(this.m_maskTxt, "Mask file");
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 52);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(72, 17);
            this.label6.TabIndex = 16;
            this.label6.Text = "Map Mask";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(225, 81);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 17);
            this.label4.TabIndex = 22;
            this.label4.Text = "Height";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(133, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 17);
            this.label3.TabIndex = 21;
            this.label3.Text = "Width";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(193, 105);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 17);
            this.label1.TabIndex = 20;
            this.label1.Text = "X";
            // 
            // m_hmapLbl
            // 
            this.m_hmapLbl.AutoSize = true;
            this.m_hmapLbl.Location = new System.Drawing.Point(12, 108);
            this.m_hmapLbl.Name = "m_hmapLbl";
            this.m_hmapLbl.Size = new System.Drawing.Size(74, 17);
            this.m_hmapLbl.TabIndex = 19;
            this.m_hmapLbl.Text = "Mask Res:";
            // 
            // m_heightTxt
            // 
            this.m_heightTxt.Location = new System.Drawing.Point(216, 105);
            this.m_heightTxt.Name = "m_heightTxt";
            this.m_heightTxt.Size = new System.Drawing.Size(63, 22);
            this.m_heightTxt.TabIndex = 23;
            // 
            // m_widthTxt
            // 
            this.m_widthTxt.Location = new System.Drawing.Point(124, 105);
            this.m_widthTxt.Name = "m_widthTxt";
            this.m_widthTxt.Size = new System.Drawing.Size(63, 22);
            this.m_widthTxt.TabIndex = 24;
            // 
            // m_import
            // 
            this.m_import.Location = new System.Drawing.Point(392, 81);
            this.m_import.Name = "m_import";
            this.m_import.Size = new System.Drawing.Size(147, 31);
            this.m_import.TabIndex = 25;
            this.m_import.Text = "Import Mask ...";
            this.m_tooltip.SetToolTip(this.m_import, "Import mask");
            this.m_import.UseVisualStyleBackColor = true;
            this.m_import.Click += new System.EventHandler(this.m_import_Click);
            // 
            // CreateTerrainMapDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(560, 161);
            this.Controls.Add(this.m_import);
            this.Controls.Add(this.m_widthTxt);
            this.Controls.Add(this.m_heightTxt);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_hmapLbl);
            this.Controls.Add(this.m_chooseMaskBtn);
            this.Controls.Add(this.m_maskTxt);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.m_terrainMapNameTxt);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_cancelBtn);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateTerrainMapDlg";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create new terrain map.";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button m_btnOK;
        private System.Windows.Forms.Button m_cancelBtn;
        private System.Windows.Forms.TextBox m_terrainMapNameTxt;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button m_chooseMaskBtn;
        private System.Windows.Forms.TextBox m_maskTxt;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label m_hmapLbl;
        private System.Windows.Forms.TextBox m_heightTxt;
        private System.Windows.Forms.TextBox m_widthTxt;
        private System.Windows.Forms.Button m_import;
        private System.Windows.Forms.ToolTip m_tooltip;
    }
}