//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Adaptation;

using LevelEditorCore;
using System.Diagnostics;

namespace LevelEditor
{
    /// <summary>
    /// Game loop driver.
    /// It continously call update/render as long as 
    /// there is no message in the windows message queue</summary>    
    [Export(typeof(IInitializable))]
    [Export(typeof(IGameLoop))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GameLoopService : IGameLoop, IInitializable
    {

        public GameLoopService()
        {
            // Initilize variables used by GameLoop.
            m_lastUpdateTime = Timing.GetHiResCurrentTime() - UpdateStep;
            m_lastRenderTime = m_lastUpdateTime;
            UpdateType = UpdateType.Editing;
        }

        #region IInitializable Members
        void IInitializable.Initialize()
        {
            RegisterToolStripComboBox();
            Application.Idle += Application_Idle;
        }

        #endregion

        private void Application_Idle(object sender, EventArgs e)
        {            
            while (IsIdle())
            {                
                Update();
                Render();                
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

        #region IGameLoop members and related code
        public UpdateType UpdateType
        {
            get;
            set;
        }

        public void Update()
        {
            var context = m_designView.Context;
            if (context == null) return;

            m_gameEngine.SetGameWorld(context.Cast<IGame>());
            double lag = (Timing.GetHiResCurrentTime() - m_lastUpdateTime)
                + m_updateLagRemainder;

            // early return
            if (lag < UpdateStep) return;

            if (UpdateType == UpdateType.Paused)
            {
                m_lastUpdateTime = Timing.GetHiResCurrentTime();
                FrameTime fr = new FrameTime(m_simulationTime, 0.0f);
                m_gameEngine.Update(fr, UpdateType);
                m_updateLagRemainder = 0.0;
            }
            else
            {
                // set upper limit of update calls 
                const int MaxUpdates = 3;
                int updateCount = 0;

                while (lag >= UpdateStep
                    && updateCount < MaxUpdates)
                {
                    m_lastUpdateTime = Timing.GetHiResCurrentTime();
                    FrameTime fr = new FrameTime(m_simulationTime, (float)UpdateStep);
                    m_gameEngine.Update(fr, UpdateType);
                    m_simulationTime += UpdateStep;
                    lag -= UpdateStep;
                    updateCount++;
                }

                m_updateLagRemainder = MathUtil.Clamp(lag, 0, UpdateStep);
                Debug.Assert(updateCount != 0);
            }
        }

        public void Render()
        {
            // set upper limit of rendering frequency to 1/UpdateStep
            var startTime = Timing.GetHiResCurrentTime();
            var rdt = startTime - m_lastRenderTime;

            // if not enought time passed, skip rendering
            if (rdt < UpdateStep) return;

            m_lastRenderTime = startTime;
            foreach (var view in m_designView.Views)
                view.Render();
        }


        private void RegisterToolStripComboBox()
        {
            m_updateTypeComboBox = new ToolStripComboBox();
            m_updateTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            m_updateTypeComboBox.Name = "UpdateType".Localize();
            m_updateTypeComboBox.ComboBox.DataSource = Enum.GetValues(typeof(UpdateType));
            m_updateTypeComboBox.SelectedItem = this.UpdateType;
            m_updateTypeComboBox.SelectedIndexChanged += (sender, e) => this.UpdateType = (UpdateType)m_updateTypeComboBox.SelectedItem;
            m_updateTypeComboBox.ToolTipText = "Update type".Localize();
            MenuInfo editMenuInfo = MenuInfo.Edit;
            editMenuInfo.GetToolStrip().Items.Add(m_updateTypeComboBox);            
        }


        [Import(AllowDefault = false)]
        private IGameEngineProxy m_gameEngine;

        [Import(AllowDefault = false)]
        private IDesignView m_designView;

        private double m_simulationTime;
        private double m_lastRenderTime;
        private double m_lastUpdateTime;
        private double m_updateLagRemainder;
        private const double UpdateStep = 1.0 / 60.0;
        private ToolStripComboBox m_updateTypeComboBox;
        #endregion
    }
}


