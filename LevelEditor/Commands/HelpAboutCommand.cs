//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;

using Sce.Atf;
using Sce.Atf.Controls;

namespace LevelEditor.Commands
{
    /// <summary>
    /// Overrides and customized the Help -> About menu command</summary>
    [Export(typeof(HelpAboutCommand))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class HelpAboutCommand : Sce.Atf.Applications.HelpAboutCommand
    {
        /// <summary>
        /// Shows the About dialog box</summary>
        protected override void ShowHelpAbout()
        {
            Assembly assem = Assembly.GetAssembly(typeof(LevelEditorApplication));
            string v = "V" + assem.GetName().Version.ToString(3);
            object[] attribs = assem.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            string copyright = ((AssemblyCopyrightAttribute)attribs[0]).Copyright;
            attribs = assem.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            string productName = ((AssemblyProductAttribute)attribs[0]).Product;

            string appName = productName + " " + v;

            var credits = new List<string>();
            credits.Add(appName + " by Alan Beckus, Ron Little, Matt Mahony, and Max Elliott. " + copyright);

            RichTextBox richTextBox = new RichTextBox();
            richTextBox.BorderStyle = BorderStyle.None;
            richTextBox.ReadOnly = true;
            richTextBox.Text = appName;

            string appURL = "http://wiki.ship.scea.com/confluence/display/WWSSDKLEVELEDITOR/LevelEditor";
            AboutDialog dialog = new AboutDialog(productName, appURL, richTextBox, null, credits, true);
            dialog.ShowDialog();
        }
    }
}
