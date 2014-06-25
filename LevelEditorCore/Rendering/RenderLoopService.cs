//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Sce.Atf;

namespace LevelEditorCore
{

    /// <summary>
    /// Render loop driver.
    /// It continously call update/render as long as 
    /// there is no message in the windows message queue.    
    /// </summary>
    [Export(typeof(RenderLoopService))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class RenderLoopService : IInitializable
    {        
        #region IInitializable Members

        void IInitializable.Initialize()
        {
            Application.Idle += Application_Idle;
        }

        #endregion




        //[Import(AllowDefault = true)]
        //private IDesignView m_designView = null;


        private void Application_Idle(object sender, EventArgs e)
        {
            //if (m_designView != null)
            //{
            //    while (!PeekMessage(out m_msg, IntPtr.Zero, 0, 0, 0))
            //    {
            //        m_designView.Tick();
            //        System.Threading.Thread.Sleep(1);
            //    }
            //}
        }
        

        /// <summary>Windows Message</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Message
        {
            public IntPtr hWnd;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;
        }

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);        
        //private Message m_msg;        
    }
}
