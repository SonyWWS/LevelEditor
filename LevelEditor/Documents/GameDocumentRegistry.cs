//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf;

using LevelEditorCore;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

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

        public bool Contains(IGameDocument doc)
        {
            return m_documents.Contains(doc);
        }

        public bool AnyDocumentDirty
        {
            get;
            private set;
        }

        public bool AnyEditableResourceOwnerDirty
        {
            get;
            private set;
        }

        public event EventHandler<ItemInsertedEventArgs<IGameDocument>> DocumentAdded = delegate { };   
        public event EventHandler<ItemRemovedEventArgs<IGameDocument>> DocumentRemoved = delegate { };
        public event EventHandler<ItemChangedEventArgs<IGameDocument>> DocumentDirtyChanged = delegate { };
        public event EventHandler<ItemChangedEventArgs<IGameDocument>> DocumentUriChanged = delegate { };        
        public event EventHandler<ItemChangedEventArgs<IEditableResourceOwner>> EditableResourceOwnerDirtyChanged = delegate { };
        
        public void Add(IGameDocument doc)
        {
            if (doc == null)
                throw new ArgumentNullException("doc");
            
            if(!m_documents.Contains(doc))
            {
                m_documents.Add(doc);
                doc.DirtyChanged += OnDocumentDirtyChanged;
                doc.UriChanged += OnDocumentUriChanged;                
                doc.EditableResourceOwnerDirtyChanged += OnResourceDirtyChanged;
                UpdateAnyDocDirty();
                UpdateAnyResourceDirty();
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
            doc.DirtyChanged -= OnDocumentDirtyChanged;
            doc.UriChanged -= OnDocumentUriChanged;
            doc.EditableResourceOwnerDirtyChanged -= OnResourceDirtyChanged;

            UpdateAnyDocDirty();
            UpdateAnyResourceDirty();
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

        private void OnResourceDirtyChanged(object sender, ItemChangedEventArgs<IEditableResourceOwner> e)
        {
            UpdateAnyResourceDirty();
            EditableResourceOwnerDirtyChanged(this, e);
        }

        private void OnDocumentDirtyChanged(object sender, EventArgs e)
        {
            UpdateAnyDocDirty();
            IGameDocument doc = (IGameDocument)sender;
            DocumentDirtyChanged(this, new ItemChangedEventArgs<IGameDocument>(doc));
        }

        private void OnDocumentUriChanged(object sender, UriChangedEventArgs e)
        {
            UpdateAnyResourceDirty();
            IGameDocument doc = (IGameDocument)sender;            
            DocumentUriChanged(this, new ItemChangedEventArgs<IGameDocument>(doc));
        }
        private void UpdateAnyDocDirty()
        {
            AnyDocumentDirty = false;
            foreach (var doc in Documents)
            {
                AnyDocumentDirty = doc.Dirty;
                if (AnyDocumentDirty) break;
            }
        }

        /// <summary>
        /// Find all objects of type T in all the open game documents</summary>        
        public IEnumerable<T> FindAll<T>()
            where T : class
        {            
            foreach (IGameDocument doc in Documents)
            {
                DomNode folderNode = doc.RootGameObjectFolder.Cast<DomNode>();
                foreach (DomNode childNode in folderNode.Subtree)
                {
                    T t = childNode.As<T>();
                    if (t != null)
                        yield return t;
                }
            }
        }

        private void UpdateAnyResourceDirty()
        {
            AnyEditableResourceOwnerDirty = false;
            foreach (var resOwner in FindAll<IEditableResourceOwner>())
            {
                AnyEditableResourceOwnerDirty = resOwner.Dirty;
                if (AnyEditableResourceOwnerDirty) break;
            }
        }

        private readonly List<IGameDocument> m_documents = new List<IGameDocument>();        
    }
}
