//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using Sce.Atf;

namespace LevelEditor
{
    /// <summary>
    /// Splash screen used by any application 
    /// the user Application need to provide splash image.</summary>
    public class SplashForm : Form
    {       
        /// <summary>
        /// Create and Show Splash Form</summary>        
        public static void ShowForm(Type type, string resourcePath)
        {
            if (theInstance != null) return;            
            theInstance = new SplashForm(type, resourcePath.ToString());
            theInstance.Show();
            Application.DoEvents();
        }

        /// <summary>
        /// Close splash form and exit message loop
        /// </summary>
        public static void CloseForm()
        {
            if (theInstance == null)
                return;
            theInstance.Close();
            theInstance = null;
        }

        /// <summary>
        /// construct Splash Form and assing background image if provided.
        /// </summary>        
        private SplashForm(Type type, string resourcePath)
        {
            Bitmap bkgImage = null;
            Assembly assem = Assembly.GetAssembly(type);
            var attr = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assem, typeof(AssemblyCopyrightAttribute));
            string copyright = attr.Copyright;

            if (!string.IsNullOrEmpty(resourcePath))
            {
                Stream strm = assem.GetManifestResourceStream(resourcePath);
                if (strm != null)
                    bkgImage = new Bitmap(strm);
            }

            this.SuspendLayout();

            // 
            // lblAppVer
            //             
            this.lblAppVer = new Label();
            this.lblAppVer.BackColor = Color.Transparent;
            this.lblAppVer.ForeColor = Color.LightGray;
            this.lblAppVer.Font = new Font("Trebuchet MS", 10.0f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.lblAppVer.Name = "lblAppVer";
            this.lblAppVer.Text = "Version " + Application.ProductVersion;
            this.lblAppVer.Width = 200;
            lblAppVer.AutoSize = true;
            

            // lblATFVer
            this.lblATFVer = new System.Windows.Forms.Label();
            this.lblATFVer.BackColor = System.Drawing.Color.Transparent;
            this.lblATFVer.ForeColor = Color.LightGray;
            this.lblATFVer.Font = new System.Drawing.Font("Trebuchet MS", 10.0f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblATFVer.Name = "lblATFVer";
            this.lblATFVer.Text = "Uses ATF Version " + AtfVersion.GetVersion();
            this.lblATFVer.Width = 200;
            lblATFVer.AutoSize = true;
            

            //
            // lblCopyright
            //
            this.lblCopyright = new System.Windows.Forms.Label();
            this.lblCopyright.BackColor = System.Drawing.Color.Transparent;
            this.lblCopyright.ForeColor = Color.LightGray;
            this.lblCopyright.Font = new System.Drawing.Font("Trebuchet MS", 10.0f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCopyright.Name = "lblCopyright";
            this.lblCopyright.Text = copyright;
            this.lblCopyright.Width = 500;
            this.AutoSize = true;
            

            // 
            // SplashForm
            //             
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ControlBox = false;
            this.Controls.Add(this.lblATFVer);
            this.Controls.Add(this.lblAppVer);
            this.Controls.Add(this.lblCopyright);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplashForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.TopMost = true;

            if (bkgImage != null)
            {
                this.Size = bkgImage.Size;
                this.BackgroundImage = bkgImage;
            }

            this.ResumeLayout(false);
            this.PerformLayout();

            SizeChanged += (sender, e) =>
                {
                    // Reposition labels.                  
                    const int x = 10;
                    int y = Height - lblCopyright.Height - 6;

                    lblCopyright.Location = new Point(x, y);

                    y -= lblATFVer.Height;
                    lblATFVer.Location = new Point(x, y);

                    y -= (lblAppVer.Height);
                    lblAppVer.Location
                        = new Point(x, y);
                };
        }

        #region  Allow user to drag the splash form with the left mouse button.
        private Point curLoc;
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                curLoc = new Point(e.X, e.Y);
            }
        }

        private Point mouseLoc;
        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left)
            {
                mouseLoc = Control.MousePosition;
                Location = new Point(mouseLoc.X - curLoc.X, mouseLoc.Y - curLoc.Y);
            }
        }

        #endregion

        private Label lblAppVer;
        private Label lblATFVer;
        private Label lblCopyright;

        /// <summary>
        /// The only splashForm instance.
        /// </summary>
        private static SplashForm theInstance = null;
    }
}
