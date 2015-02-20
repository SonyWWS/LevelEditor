//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

using LevelEditorCore;
using LevelEditorCore.VectorMath;
using LevelEditor.DomNodeAdapters;

namespace LevelEditor
{   
    /// <summary>
    /// Mangages views, selection and editing operations on a loaded game document</summary>    
    public class GameContext : 
        EditingContext,
        IGameContext,
        ITreeView, 
        IItemView,
        IInstancingContext,
        IHierarchicalInsertionContext,
        IObservableContext,
        IEnumerableContext,
        INamingContext,
        IVisibilityContext,
        IViewingContext,
        ILockingContext                 
    {            
        #region ITreeView Members

        public object Root
        {
            get { return DomNode; }
        }

        public IEnumerable<object> GetChildren(object parent)
        {            
            DomNode domNode = parent.As<DomNode>();
            if (domNode != null)
            {
                GameReference gameRef = domNode.As<GameReference>();
                if (gameRef != null && gameRef.Target != null)
                {
                    domNode = gameRef.Target.As<DomNode>();
                }

                foreach (ChildInfo childInfo in domNode.Type.Children)
                {                    
                   
                    // todo use schema annotatoin to mark types that need to have ref slot.
                    bool isReference = Schema.gameObjectReferenceType.Type.IsAssignableFrom(childInfo.Type) ||
                                        Schema.resourceReferenceType.Type.IsAssignableFrom(childInfo.Type);

                    bool hasChild = false;
                    foreach (DomNode child in domNode.GetChildren(childInfo))
                    {                        
                        hasChild = true;
                        if (child.Is<IListable>())
                            yield return child;
                    }                    
                    if ( (hasChild == false || childInfo.IsList) && isReference)
                    {
                        yield return new Slot(domNode, childInfo);
                    }
                }                
            }            
        }

            
        #endregion

        #region IItemView Members

        public void GetInfo(object item, ItemInfo info)
        {
            IListable listable = item.As<IListable>();
            if (listable != null)
            {
                listable.GetInfo(info);
                return;
            }

            IResource resource = item.As<IResource>();
            if (resource != null && !item.Is<Game>())
            {
                info.Label = Path.GetFileName(resource.Uri.LocalPath);
                info.ImageIndex = info.GetImageList().Images.IndexOfKey(Sce.Atf.Resources.ResourceImage);
                return;
            }
                
            // If the object has a name use it as the label (overriding whatever may have been set previously)
            INameable nameable = Adapters.As<INameable>(item);
            if (nameable != null)
                info.Label = nameable.Name;
        }

        #endregion

        #region IInstancingContext Members

        public bool CanCopy()
        {
            IEnumerable<DomNode> rootDomNodes = DomNode.GetRoots(Selection.AsIEnumerable<DomNode>());
            foreach (DomNode domNode in rootDomNodes)
            {
                if (domNode.Is<IGame>() || domNode.Is<GameReference>()
                    || domNode.Is<IResource>())
                    return false;                
            }
            return Selection.Count > 0;
        }

        public object Copy()
        {
            return Copy(Selection);
        }
        
        public object Copy(Selection<object> selection)
        {
            IEnumerable<DomNode> rootDomNodes = DomNode.GetRoots(Selection.AsIEnumerable<DomNode>());
            object[] copies = DomNode.Copy(rootDomNodes).ToArray<object>();
            return new DataObject(copies);
        }

      

        /// <summary>
        /// Returns true iff the specified DataObject can be inserted into
        /// either the selected object or the root GameObjectFolder</summary>
        /// <param name="insertingObject">IDataObject to be inserted</param>
        /// <returns>True iff the object can be inserted</returns>
        public bool CanInsert(object insertingObject)
        {
            return CanInsert(InsertParent, insertingObject);
        }

        /// <summary>
        /// Inserts the specified object into the currently selected object
        /// if it is a potential parent. If not the object will be inserted
        /// into the root GameObjectFolder</summary>
        /// <param name="insertingObject">IDataObject to be inserted</param>
        public void Insert(object insertingObject)
        {
            IDataObject dataObject = insertingObject as IDataObject;
            if (dataObject != null)
            {                
                Insert(InsertParent, dataObject);                                    
            }
        }

        private object InsertParent
        {
            get
            {
                return m_activeItem ?? RootGameObjectFolder;                
            }
        }

        public bool CanDelete()
        {
            IEnumerable<DomNode> rootDomNodes = DomNode.GetRoots(Selection.AsIEnumerable<DomNode>());
            foreach (DomNode domNode in rootDomNodes)
            {
                if (domNode.Is<IGame>() 
                    || domNode.Is<GameReference>()
                    || domNode.Parent == null
                    || (domNode.Parent.Is<IGame>() && domNode.Is<IGameObjectFolder>())
                    || IsLocked(domNode))
                    return false;
            }
            return Selection.Count > 0;            
        }

        public void Delete()
        {
            if (!CanDelete()) return;
            IEnumerable<DomNode> rootDomNodes = DomNode.GetRoots(Selection.AsIEnumerable<DomNode>());

            foreach (DomNode domNode in rootDomNodes)
            {
                if (domNode.Is<IGame>()
                   || domNode.Is<GameReference>()
                   || domNode.Parent == null
                   || (domNode.Parent.Is<IGame>() && domNode.Is<IGameObjectFolder>()))
                    continue;
                domNode.RemoveFromParent();
            }                
            UpdateGameObjectReferences();
        }


        public void UpdateGameObjectReferences()
        {          
            // Iterate through the entire scene graph and check each GameObjectReference
            // If the Target is null or its root is not the Game node, remove the Reference to it
            IList<DomNode> removeList = new List<DomNode>();

            MasterContext.CollectInvalidGameObjectReference(removeList);
            foreach (IGameDocument subDoc in m_gameDocumentRegistry.SubDocuments)
            {
                GameContext subContext = subDoc.Cast<GameContext>();
                subContext.CollectInvalidGameObjectReference(removeList);
            }
            
            foreach (DomNode domNode in removeList)
                domNode.RemoveFromParent();
        }

        private void CollectInvalidGameObjectReference( IList<DomNode> removeList)
        {
            DomNode rootNode = DomNode;
            foreach (DomNode domNode in rootNode.Subtree)
            {
                GameObjectReference gameObjectReference = domNode.As<GameObjectReference>();
                if (gameObjectReference != null)
                {
                    DomNode targetNode = Adapters.As<DomNode>(gameObjectReference.Target);
                    if (targetNode == null || !targetNode.GetRoot().Is<IGame>())
                        removeList.Add(gameObjectReference.DomNode);
                }
            }            
        }
        
        #endregion

        #region IHierarchicalInsertionContext Members

        public bool CanInsert(object parent, object child)
        {
            IHierarchical hierarchical = parent.As<IHierarchical>();
            if (hierarchical == null) return false;
            ILockable lockable = parent.As<ILockable>();
            if (lockable != null && lockable.IsLocked) return false;
            
            DomNode parentNode = parent.As<DomNode>();

            IEnumerable<object> items = Util.ConvertData(child, false);
            bool canInsert = false;
            foreach (object item in items)
            {
                DomNode childNode = item as DomNode;
                if (parentNode != null && childNode != null)
                {
                    if((parentNode.IsDescendantOf(childNode)
                    || parentNode == childNode
                    || parentNode == childNode.Parent))
                        return false;
                }
                IResource res = item as IResource;
                IGameObject gob = m_resourceConverterService.Convert(res);
                if (!hierarchical.CanAddChild(item)
                    && !hierarchical.CanAddChild(gob))
                    return false;

                canInsert = true;
            }
            return canInsert;
        }

        /// <summary>
        /// Inserts the specified child into the specified parent</summary>
        /// <param name="parent">Parent to insert into</param>
        /// <param name="child">Child to be inserted</param>
        /// <remarks>This method is used by copy-paste and drag-drop from various sources.
        /// When making any changes to this method, please test the following:
        /// - Copy/Paste, Cut/Paste
        /// - Drag-drop from {Palette|ResourceLister} to {ProjectLister|DesignView}
        /// Pay special attention to:
        /// - (GameObjects with) GameObjectReferences
        /// - (GameObjects with) ResourceReferences incl. Locators
        /// - GameObjectGroups (and hierarchies thereof)
        /// - GameObjectFolders (and hierarchies thereof)
        /// - Pasting the same objects more than once</remarks>
        public void Insert(object parent, object child)
        {
            if (!CanInsert(parent, child)) return;

            IHierarchical hierarchical = parent.As<IHierarchical>();
                                   
            // Extract node list from IDataObject
            IEnumerable<object> items = Util.ConvertData(child, true);

            DomNode parentRoot = null;
            DomNode parentNode = parent.As<DomNode>();
            if (parentNode != null)
            {
                parentRoot = parentNode.GetRoot();
            }

            List<DomNode> copyList = new List<DomNode>();
            List<object> objectlist = new List<object>();            
            foreach (object item in items)
            {
                if (item.Is<IGameObject>() || item.Is<IGameObjectFolder>())
                {
                    DomNode childNode = item.As<DomNode>();
                    DomNode childRoot = childNode.GetRoot();
                    
                    if ((parentRoot != null && childRoot.Is<IGame>() && parentRoot != childRoot))
                    {
                        childNode.RemoveFromParent();
                        copyList.Add(childNode);
                        continue;                        
                    }                    
                }
                objectlist.Add(item); 
            }

            if (copyList.Count > 0)
            {
                IEnumerable<DomNode> copies = DomNode.Copy(copyList);
                // remvoe lock
                foreach (DomNode copy in copies)
                {
                    this.SetLocked(copy, false);
                }
                objectlist.AddRange(copies);
            }

            foreach (object obj in objectlist)
            {
                DomNode node = obj.As<DomNode>();
                if(node != null)
                    node.InitializeExtensions();                
            }

            List<DomNode> insertedNodes = new List<DomNode>();            
            foreach (object obj in objectlist)
            {
                object insertedObj = null;
                bool inserted = hierarchical.AddChild(obj);
                if(inserted)
                {
                    insertedObj = obj;
                }
                else                
                {
                    IResource res = obj as IResource;
                    IGameObject gob = m_resourceConverterService.Convert(res);                    
                    inserted = hierarchical.AddChild(gob);
                    if (inserted) insertedObj = gob;

                }
                DomNode insertNode = Adapters.As<DomNode>(insertedObj);
                if (insertNode != null)
                    insertedNodes.Add(insertNode);

            }

            IEnumerable<DomNode> rootDomNodes = DomNode.GetRoots(insertedNodes);
            List<object> newSelection = new List<object>();
            foreach (DomNode rootNode in rootDomNodes)
            {
                AdaptablePath<object> path = Util.AdaptDomPath(rootNode);

                if (path.First.Is<IGame>() && (rootNode.Is<IGameObject>() || rootNode.Is<IGameObjectFolder>()))
                {
                    newSelection.Add(path);
                }
            }
            if (newSelection.Count > 0)
            {
                if(InTransaction)
                    m_savedSelection = new List<object>(MasterContext.Selection);

                MasterContext.SetRange(newSelection);
            }            
        }
            
        #endregion

        #region IObservableContext Members

        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

        public event EventHandler Reloaded;

        #endregion

        #region IEnumerableContext Members
        
        public IEnumerable<object> Items
        {
            get 
            {            
                
                DomNode folderNode = MasterContext.RootGameObjectFolder.Cast<DomNode>();
                IEnumerable<DomNode> descendants = folderNode.Subtree.Skip(1);
                foreach (DomNode childNode in descendants)
                {
                    if (childNode.Is<IReference<IResource>>()) continue;
                    yield return Util.AdaptDomPath(childNode);
                }

                foreach (GameContext subContext in m_gameDocumentRegistry.SubDocuments.AsIEnumerable<GameContext>())
                {
                    folderNode = subContext.RootGameObjectFolder.Cast<DomNode>();
                    descendants = folderNode.Subtree.Skip(1);
                    foreach (DomNode childNode in descendants)
                    {
                        if (childNode.Is<IReference<IResource>>()) continue;
                        yield return Util.AdaptDomPath(childNode);
                    }                    
                }                
            }
        }

        #endregion

        #region INamingContext Members

        public string GetName(object item)
        {
            INameable nameable = item.As<INameable>();
            if (nameable != null)
                return nameable.Name;
            return null;
        }

        public bool CanSetName(object item)
        {
            return item.Is<INameable>();
        }

        public void SetName(object item, string name)
        {
            INameable nameable = item.As<INameable>();
            if (nameable != null && !string.IsNullOrEmpty(name))
                nameable.Name = name;
        }

        #endregion

        #region ILockingContext Members

        public bool IsLocked(object item)
        {
            var lockable = item.As<ILockable>();
            return lockable == null ? false : lockable.IsLocked;
        }

        public bool CanSetLocked(object item)
        {
            var lockable = item.As<ILockable>();
            if (lockable == null) return false;

            var node = item.As<DomNode>();
            if (node == null)
                return false;
            
            lockable = Adapters.As<ILockable>(node.Parent);
            return lockable == null ? true : !lockable.IsLocked;
        }

        public void SetLocked(object item, bool value)
        {
            var lockable = item.As<ILockable>();
            if (lockable != null)
                lockable.IsLocked = value;
        }

        #endregion

        #region ITransactionContext Members

        private static bool s_subContextBegan = false;
        public override void Begin(string transactionName)
        {
            if(UndoingOrRedoing)
            {
                base.Begin(transactionName);
                return;
            }

            if (!IsMasterContext && !MasterContext.InTransaction)
            {
                s_subContextBegan = true;
                MasterContext.Begin(transactionName);
                return;
            }

            base.Begin(transactionName);
            if (IsMasterContext)
            {
                foreach (GameContext subContext in m_gameDocumentRegistry.SubDocuments.AsIEnumerable<GameContext>())
                {
                    subContext.Begin(transactionName);
                }                
            }            
        }

        public override void Cancel()
        {
            
            if (s_subContextBegan)
            {
                s_subContextBegan = false;
                MasterContext.Cancel();
                return;
            }

            if (IsMasterContext)
            {
                foreach (GameContext subContext in m_gameDocumentRegistry.SubDocuments.AsIEnumerable<GameContext>())
                {
                    subContext.Cancel();
                }
            }
            if(!InTransaction)
            {
                base.Undo();                
            }
            base.Cancel();
            if (m_savedSelection != null)
            {
                MasterContext.SetRange(m_savedSelection);
                m_savedSelection = null;
            }
        }

        public override void End()
        {            

            if (UndoingOrRedoing)
            {                
                base.End();
                m_savedSelection = null;
                return;
            }

            if (s_subContextBegan)
            {               
                s_subContextBegan = false;
                MasterContext.End();
                return;
            }

            if (IsMasterContext)
            {
                foreach (GameContext subContext in m_gameDocumentRegistry.SubDocuments.AsIEnumerable<GameContext>())
                {
                    subContext.End();
                }
            }

            if(TransactionOperations.Count == 0)
            {
                TransactionOperations.Add(new Nop());                
            }
            base.End();
            m_savedSelection = null;           
        }

        #endregion

        #region IHistoryContext Members

        public override bool CanUndo
        {
            get
            {
                bool canudo = base.CanUndo;
                if (!canudo && IsMasterContext)
                {
                    foreach (GameContext subContext in m_gameDocumentRegistry.SubDocuments.AsIEnumerable<GameContext>())
                    {
                        canudo |= subContext.CanUndo;
                    }
                }
                return canudo;
            }
        }

        public override bool CanRedo
        {
            get
            {
                bool canredo = base.CanRedo;
                if (!canredo && IsMasterContext)
                {
                    foreach (GameContext subContext in m_gameDocumentRegistry.SubDocuments.AsIEnumerable<GameContext>())
                    {
                        canredo |= subContext.CanRedo;
                    }

                }
                return canredo;
            }
        }

        public override string UndoDescription
        {
            get
            {
                string strundo = base.UndoDescription;
                if (string.IsNullOrEmpty(strundo) && IsMasterContext)
                {
                    foreach (GameContext subContext in m_gameDocumentRegistry.SubDocuments.AsIEnumerable<GameContext>())
                    {
                        strundo = subContext.UndoDescription;
                        if (!string.IsNullOrEmpty(strundo))
                            break;
                    }

                }
                return strundo;
            }
        }

        public override string RedoDescription
        {
            get
            {
                string strRedo = base.RedoDescription;
                if (string.IsNullOrEmpty(strRedo) && IsMasterContext)
                {
                    foreach (GameContext subContext in m_gameDocumentRegistry.SubDocuments.AsIEnumerable<GameContext>())
                    {
                        strRedo = subContext.RedoDescription;
                        if (!string.IsNullOrEmpty(strRedo))
                            break;
                    }

                }
                return strRedo;
            }
        }

        public override void Undo()
        {
            if (IsMasterContext)
            {
                foreach (GameContext subContext in m_gameDocumentRegistry.SubDocuments.AsIEnumerable<GameContext>())
                {
                    if (subContext.CanUndo)
                        subContext.Undo();
                }
            }

            if (base.CanUndo)
            {
                base.Undo();
            }
           
        }

        public override void Redo()
        {
            if (IsMasterContext)
            {
                foreach (GameContext subContext in m_gameDocumentRegistry.SubDocuments.AsIEnumerable<GameContext>())
                {
                    if (subContext.CanRedo)
                        subContext.Redo();
                }
            }

            if (base.CanRedo)
            {
                base.Redo();
            }
            
        }
       
        #endregion

        #region IVisibilityContext Members

        public bool IsVisible(object item)
        {
            IVisible visible = item.As<IVisible>();
            if (visible != null)
                return visible.Visible;

            return false;
        }

        public bool CanSetVisible(object item)
        {
            return (item != null && item.Is<IVisible>());
        }

        public void SetVisible(object item, bool value)
        {            
            IVisible visible = item.As<IVisible>();
            if (visible != null)
                visible.Visible = value;
        }

        #endregion

        #region IViewingContext Members

        public bool CanFrame(IEnumerable<object> items)
        {
            // return true iff items enumeration not empty
            foreach (object item in items)
            {
                // ignore items that aren't visible
                var visible = item.As<IVisible>();
                if ( visible != null && !visible.Visible)
                    continue;

                return true;
            }

            return false;
        }

        public void Frame(IEnumerable<object> items)
        {
            IEnumerable<DomNode> rootDomNodes = DomNode.GetRoots(items.AsIEnumerable<DomNode>());
            AABB bound = new AABB();

            foreach (var item in items)
            {
                IBoundable boundable = null;
                DomNode domItem = item.As<DomNode>();
                if (domItem != null)
                {
                    foreach (var node in domItem.Lineage)
                    {
                        boundable = node.As<IBoundable>();
                        if (boundable != null)
                            break;
                    }
                }
                else
                {
                    Slot slot = item.As<Slot>();
                    boundable = slot.Owner.As<IBoundable>();
                }

                IVisible vn = boundable.As<IVisible>();                
                if (boundable != null && (vn == null || vn.Visible))
                    bound.Extend(boundable.BoundingBox);
            }

            if (!bound.IsEmpty)
            {
                Sphere3F sphere = bound.ToSphere();
                sphere.Radius *= 3.0f;
                IDesignView designView = Globals.MEFContainer.GetExportedValue<IDesignView>();
                Util.ZoomOnSphere(designView.ActiveView.Camera, sphere);
            }

        }

        public bool CanEnsureVisible(IEnumerable<object> items)
        {
            // return true iff items enumeration not empty
            foreach (object item in items)
                return true;
            return false;
        }

        public void EnsureVisible(IEnumerable<object> items)
        {
            foreach (IVisible vnode in items.AsIEnumerable<IVisible>())
            {
                vnode.Visible = true;
            }
            Frame(items);
        }

        #endregion

       

        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            
            DomNode.AttributeChanged += DomNode_AttributeChanged;
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += DomNode_ChildRemoved;
            Reloaded.Raise(this, EventArgs.Empty);
            m_gameDocumentRegistry = Globals.MEFContainer.GetExportedValue<GameDocumentRegistry>();
            m_resourceConverterService = Globals.MEFContainer.GetExportedValue<ResourceConverterService>();            

        }
        
        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            if (!IsHandledType(e.DomNode))
                return;
            if (!InTransaction) return;
            ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));
            if (!IsMasterContext)
            {
                MasterContext.DomNode_AttributeChanged(sender, e);
            }
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            if (!IsHandledType(e.Child))
                return;
            ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(-1, e.Child, e.Parent));
            if (!IsMasterContext)
            {
                MasterContext.DomNode_ChildInserted(sender,e);                
            }
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            if (!IsHandledType(e.Child))
                return;
            ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(-1, e.Child, e.Parent));
            if (!IsMasterContext)
            {
                MasterContext.DomNode_ChildRemoved(sender,e);                
            }
        }
        
        private static bool IsHandledType(DomNode domNode)
        {
            if (Adapters.Is<ILayer>(domNode)            
            || Adapters.Is<Bookmark>(domNode))                
                return false;

            return true;
        }


        #region IGameContext members
        
        public bool IsMasterContext
        {
            get { return this.Cast<IGameDocument>() == m_gameDocumentRegistry.MasterDocument; }
        }
        public IGameObjectFolder RootGameObjectFolder
        {
            get
            {
                return this.As<IGame>().RootGameObjectFolder;
            }
        }
        private object m_activeItem;
        /// <summary>
        /// Sets the active item; used by UI components.
        /// The active is used as insersion point where paste and drag and drop operations 
        /// insert new objects into the UI data.</summary>
        /// <param name="item">Active item</param>
        public void SetActiveItem(object item)
        {
            m_activeItem = null;
            DomNode node = Adapters.As<DomNode>(item);            
            while (node != null)
            {
                if (node.Is<GameObjectFolder>() || node.Is<GameObjectGroup>())
                {
                    m_activeItem = node;
                    break;
                }
                node = node.Parent;
            }
        }

        #endregion
        private GameContext MasterContext
        {
            get { return m_gameDocumentRegistry.MasterDocument.Cast<GameContext>(); }

        }

        private ResourceConverterService m_resourceConverterService;
        private IGameDocumentRegistry m_gameDocumentRegistry = null;

        // save selection in case of transaction canceled.
        private List<object> m_savedSelection;

        
        private class Nop : Operation
        {
            /// <summary>
            /// Does the transaction operation</summary>
            public override void Do() { }

            /// <summary>
            /// Rolls back the transaction operation</summary>
            public override void Undo() { }
        }
    }
}
