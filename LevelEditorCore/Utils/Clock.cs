//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Diagnostics;

namespace LevelEditorCore
{
    public class Clock
    {
        public Clock()
        {
            m_freql = Stopwatch.Frequency;
            m_frq = m_freql;

        }

        public void Start()
        {
            m_start = Stopwatch.GetTimestamp();
        }

        public float Milliseconds
        {
            get{ return Seconds * 1000.0f;}
        }
        public float Seconds
        {
            get
            {
                m_stop = Stopwatch.GetTimestamp();
                double dt = (double)(m_stop - m_start);
                return (float)(dt / m_frq);
            }
        }

        /// <summary>
        /// Gets elapsed since Start() called in milliseconds;
        /// </summary>
        public int ElapsedMS
        {
            get
            {
                m_stop = Stopwatch.GetTimestamp();
                long dt = (m_stop - m_start) * 1000;
                return (int)(dt / m_freql);
            }
        }

        double m_frq;
        long m_freql;
        long m_start;
        long m_stop;
    }

}
