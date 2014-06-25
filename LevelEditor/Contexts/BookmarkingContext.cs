//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using LevelEditor.DomNodeAdapters;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using LevelEditorCore;

namespace LevelEditor
{
    public class BookmarkingContext : SelectionContext, 
        ITreeView, 
        IItemView, 
        IObservableContext,
        IInstancingContext
    {
        protected override void OnNodeSet()
        {  
            var gameDocRegistry = Globals.MEFContainer.GetExportedValue<GameDocumentRegistry>();
            if (gameDocRegistry.MasterDocument == null || gameDocRegistry.MasterDocument == this.As<IGameDocument>())
            {
                // get the root of bookmarks.
                // bookmarks is a child element of gametype.
                m_bookmarkRoot = this.DomNode.GetChild(Schema.gameType.bookmarksChild);
                m_bookmarkList = new DomNodeListAdapter<Bookmark>(m_bookmarkRoot, Schema.bookmarksType.bookmarkChild);

                m_bookmarkRoot.AttributeChanged += DomNode_AttributeChanged;
                m_bookmarkRoot.ChildInserted += DomNode_ChildInserted;
                m_bookmarkRoot.ChildRemoved += DomNode_ChildRemoved;
            }
        }

        private DomNode m_bookmarkRoot;
        private DomNodeListAdapter<Bookmark> m_bookmarkList;
        public IList<Bookmark> Bookmarks
        {
            get { return m_bookmarkList; }
        }
        
        #region IInstancingContext Members

        public bool CanCopy()
        {
            return false;
        }

        public object Copy()
        {
            return null;
        }

        public bool CanInsert(object insertingObject)
        {
            return false;
        }

        public void Insert(object insertingObject)
        {            
        }

        public bool CanDelete()
        {
            return Selection.Count > 0;         
        }

        public void Delete()
        {
            IEnumerable<DomNode> selectedDomNodes = GetSelection<DomNode>();
            foreach (DomNode domNode in DomNode.GetRoots(selectedDomNodes))
            {                
                domNode.RemoveFromParent();
            }
                       
        }
        #endregion

        #region ITreeView Members

        object ITreeView.Root
        {
            get { return m_bookmarkRoot; }
        }

        IEnumerable<object> ITreeView.GetChildren(object parent)
        {            
            Bookmark bookmark = Adapters.As<Bookmark>(parent);
            if (bookmark != null)
            {
                foreach (Bookmark subBookmark in bookmark.Bookmarks)
                    yield return subBookmark;                
            }
            else if (parent == m_bookmarkRoot)
            {
                foreach (Bookmark subBookmark in Bookmarks)
                    yield return subBookmark;                                
            }            
        }

        #endregion

        #region IItemView Members

        public void GetInfo(object item, ItemInfo info)
        {
            Bookmark bookmark = Adapters.As<Bookmark>(item);
            if (bookmark != null)
            {
                info.Label = bookmark.Name;
                info.HasCheck = false;
                info.IsLeaf = bookmark.Bookmarks.Count == 0;
                info.ImageIndex = info.GetImageList().Images.IndexOfKey(LevelEditorCore.Resources.BookmarkImage);
            }           
        }
      
        #endregion

        #region IObservableContext Members

        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

        public event EventHandler Reloaded // Never raised
        {
            add { }
            remove { }
        }

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {            
            ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));                                        
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));                
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));                
        }

        #endregion
        
    }
}
