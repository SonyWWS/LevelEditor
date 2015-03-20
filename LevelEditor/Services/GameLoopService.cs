//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Sce.Atf;

using LevelEditorCore;

namespace LevelEditor
{
    /// <summary>
    /// Game loop driver.
    /// It continously call update/render as long as 
    /// there is no message in the windows message queue</summary>    
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GameLoopService : IInitializable
    {
        #region IInitializable Members

        void IInitializable.Initialize()
        {            
            Application.Idle += Application_Idle;
        }

        #endregion

        [Import(AllowDefault = false)]
        private IGameLoop m_gameLoop = null;
       
        private void Application_Idle(object sender, EventArgs e)
        {
            if (m_gameLoop == null) return;
            while (IsIdle())
            {
                //note: 
                // - timing is computed in game loop.
                //   if custom timing is needed, import IGameEngineProxy
                //   and call IGameEngineProxy.Update(..) with custom time.
                // - The update and render frequency is controlled by gameLoop.
                m_gameLoop.Update();
                m_gameLoop.Render();                
            }
        }

        private bool IsIdle()
        {
            return PeekMessage(out m_msg, IntPtr.Zero, 0, 0, 0) == 0;
        }

        ///<summary>Windows Message</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct Message
        {
            public IntPtr hWnd;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public Point p;
        }

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        private static extern int PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);
        private Message m_msg;
        
    }
}


