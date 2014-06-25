//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading;

using Sce.Atf;
using Sce.Atf.Applications;

namespace LevelEditorCore
{
    /// <summary>
    /// Service that manages the transformation of Resources into thumbnail images and file paths</summary>
    [Export(typeof(ThumbnailService))]    
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ThumbnailService
    {
        public ThumbnailService()
        {
            m_syncContext = SynchronizationContext.Current;
            if (m_syncContext == null)
            {
                throw new Exception("The instance of this class can only be created on a thread"
                    + "that has WindowsFormsSynchronizationContext, ie GUI thread");
            }

            StartThread();
        }
        /// <summary>
        /// Event that is raised when a thumbnail is ready</summary>
        public event EventHandler<ThumbnailReadyEventArgs> ThumbnailReady = delegate { };

        /// <summary>
        /// Resolves the Resource into a path to a thumbnail image file</summary>
        /// <param name="resourceUri">URI of the resource to resolve</param>
        public void ResolveThumbnail(Uri resourceUri)
        {
            // Push the resource onto the resolve queue
            lock (m_resourcesToResolve)
            {
                m_resourcesToResolve.Enqueue(resourceUri);
                m_autoResetEvent.Set();
            }            
        }

        /// <summary>
        /// Raises the ThumbnailReady event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnThumbnailReady(ThumbnailReadyEventArgs e)
        {
            EventHandler<ThumbnailReadyEventArgs> handler = ThumbnailReady;
            if (handler != null)
                handler(this, e);
        }

        private void StartThread()
        {
            m_workThread = new Thread(ResolverThread)
            {
                Name = "thumbnail service",
                IsBackground = true,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture,
                Priority = ThreadPriority.Normal
            };
            m_workThread.Start();
        }

            
        private bool m_exit = false;
        private Uri GetNext()
        {
            Uri resourceUri = null;
            lock (m_resourcesToResolve)
            {
                if (m_resourcesToResolve.Count > 0)
                    resourceUri = m_resourcesToResolve.Dequeue();
            }

            return resourceUri;
        }
        
        private void ResolverThread()
        {
            while (m_exit == false)
            {                
                m_autoResetEvent.WaitOne();                
                Uri resourceUri = GetNext();
                while (resourceUri != null)
                {
                    foreach (IThumbnailResolver resolver in m_resolvers)
                    {
                        try
                        {
                            Image thumbnailImage = resolver.Resolve(resourceUri);
                            if (thumbnailImage == null) continue;
                            m_syncContext.Send(delegate
                                {
                                    OnThumbnailReady(new ThumbnailReadyEventArgs(resourceUri, thumbnailImage));
                                },
                                null);
                            
                        }
                        catch (Exception ex)
                        {
                            Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
                        }
                    }

                    resourceUri = GetNext();
                }//while (resourceUri != null)                                
            }
        }
       
        /// <summary>
        /// MEF import of available thumbnail resolvers</summary>
        [ImportMany]
        private IEnumerable<IThumbnailResolver> m_resolvers = null;

        private Thread m_workThread;
        private readonly SynchronizationContext m_syncContext;
        private readonly Queue<Uri> m_resourcesToResolve = new Queue<Uri>();        
        private readonly AutoResetEvent m_autoResetEvent = new AutoResetEvent(false);
    }
}
