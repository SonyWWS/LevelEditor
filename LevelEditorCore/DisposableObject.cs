//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;

namespace RenderingInterop
{
    /// <summary>
    /// base class for all the non-DomNode native objects.</summary>
    public class DisposableObject : IDisposable
    {
         /// <summary>        
        /// Gets a value indicating whether the object has been disposed of. </summary>
        [Browsable(false)]
        public bool IsDisposed
        {
            get { return m_disposed; }
        }
        
        public void Dispose()
        {
            if (m_disposed) return;
            Dispose(true);
            m_disposed = true;
            GC.SuppressFinalize(this);            
        }

        protected virtual void Dispose(bool disposing)
        {            
            
        }

        ~DisposableObject()
        {
            Dispose(false);
            m_disposed = true;
        }

        private bool m_disposed;
    }
}
