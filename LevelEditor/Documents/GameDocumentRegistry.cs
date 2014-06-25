//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf;

using LevelEditorCore;

namespace LevelEditor
{
    /// <summary>
    /// Component that tracks master and sub documents</summary>
    [Export(typeof(GameDocumentRegistry))]
    [Export(typeof(IGameDocumentRegistry))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GameDocumentRegistry : IGameDocumentRegistry
    {
        public IGameDocument MasterDocument
        {
            get { return m_documents.Count > 0 ? m_documents[0] : null; }
        }

        public IEnumerable<IGameDocument> SubDocuments
        {
            get
            {
                for (int i = 1; i < m_documents.Count; i++)
                    yield return m_documents[i];                
            }
        }

        public IEnumerable<IGameDocument> Documents
        {
            get { return m_documents; }
        }

        public IGameDocument FindDocument(Uri ur)
        {           
            foreach (var doc in m_documents)
                if(doc.Uri == ur)
                    return doc;            
            return null;            
        }

        /// <summary>
        /// Event that is raised after a document is added</summary>
        public event EventHandler<ItemInsertedEventArgs<IGameDocument>> DocumentAdded = delegate { };

        /// <summary>
        /// Event that is raised after a document is removed</summary>
        public event EventHandler<ItemRemovedEventArgs<IGameDocument>> DocumentRemoved = delegate { }; 
      
        public void Add(IGameDocument doc)
        {
            if (doc == null)
                throw new ArgumentNullException("doc");

            if(!m_documents.Contains(doc))
            {
                m_documents.Add(doc);
                DocumentAdded(this, new ItemInsertedEventArgs<IGameDocument>(m_documents.Count - 1, doc));
            }
            
        }
      
        public void Remove(IGameDocument doc)
        {
            if (doc == null)
                return;                

            if (!m_documents.Contains(doc))
                return;

            foreach (IReference<IGameDocument> gameDocRef in doc.GameDocumentReferences)
            {
                Remove(gameDocRef.Target);
            }
            m_documents.Remove(doc);
            DocumentRemoved(this, new ItemRemovedEventArgs<IGameDocument>(0, doc));
        }

        public void Clear()
        {
            for (int i = m_documents.Count - 1; i >= 0; i--)
            {
                IGameDocument doc = m_documents[i];
                m_documents.RemoveAt(i);
                DocumentRemoved(this,new ItemRemovedEventArgs<IGameDocument>(i, doc));
            }
        }

        private List<IGameDocument> m_documents = new List<IGameDocument>();        
    }
}
