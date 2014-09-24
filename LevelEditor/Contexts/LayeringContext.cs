//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using LevelEditorCore;
using LevelEditor.DomNodeAdapters;

using LayerLister = LevelEditorCore.LayerLister;

namespace LevelEditor
{
    /// <summary>
    /// Context for the LayerLister. </summary>
    /// <remarks>This context has its own independent Selection, 
    /// but uses the main GameContext's HistoryContext for undo/redo operations.
    /// IInstancingContext and IHierarchicalInsertionContext implementations control drag/drop and 
    /// copy/past operations within the LayerLister (internal), pastes/drops to the 
    /// LayerLister and drag/copies from the LayerLister (external).
    /// The IObservableContext implementation notifies the LayerLister's TreeControl
    /// when a change occurs that requires an update of one or more tree nodes.
    /// The ITreeView implementation controls the hierarchy in the LayerLister's TreeControl.
    /// The IItemView implementation controls icons and labels in the LayerLister's TreeControl.</remarks>
    public class LayeringContext : SelectionContext,
        IInstancingContext,
        IHierarchicalInsertionContext,
        ILayeringContext,
        IObservableContext        
    {
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            var gameDocRegistry = Globals.MEFContainer.GetExportedValue<GameDocumentRegistry>();

            if (gameDocRegistry.MasterDocument == null || gameDocRegistry.MasterDocument == this.As<IGameDocument>())
            {
                m_layerRoot = DomNode.GetChild(Schema.gameType.layersChild);
                m_layers = new DomNodeListAdapter<ILayer>(m_layerRoot, Schema.layersType.layerChild);
                
                m_layerRoot.ChildInserted += DomNode_ChildInserted;
                m_layerRoot.ChildRemoved += DomNode_ChildRemoved;
                m_layerRoot.AttributeChanged += DomNode_AttributeChanged;
                GameContext gameContext = DomNode.Cast<GameContext>();
                IValidationContext validationContext = (IValidationContext)gameContext;
                validationContext.Ended += validationContext_Ended;
                m_activeItem = m_layerRoot;                
            }
        }

        public void RefreshRoot()
        {
            // refresh root.
            LayerLister lister = Globals.MEFContainer.GetExportedValue<LayerLister>();
            if(lister != null)
                lister.TreeControlAdapter.Refresh(m_layerRoot);                       
        }
        private void validationContext_Ended(object sender, EventArgs e)
        {
            RefreshRoot();
        }

        private DomNode m_layerRoot;
        private IList<ILayer> m_layers;
        public IList<ILayer> Layers
        {
            get { return m_layers; }
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
            return CanInsert(m_activeItem, insertingObject);
        }

        public void Insert(object insertingObject)
        {
            Insert(m_activeItem, insertingObject);
        }

        public bool CanDelete()
        {
            return Selection.Count > 0;            
        }

        public void Delete()
        {
            IEnumerable<DomNode> rootDomNodes = DomNode.GetRoots(Selection.AsIEnumerable<DomNode>());
            foreach (DomNode domNode in rootDomNodes)
                domNode.RemoveFromParent();
        }

        #endregion

        #region IHierarchicalInsertionContext Members

        public bool CanInsert(object parent, object insertingObject)
        {
            if (parent == null)
                parent = m_activeItem ?? this;

            IEnumerable<DomNode> childNodes = GetNodes(insertingObject);
            if (childNodes == null) return false;

            DomNode insertionPoint = Adapters.As<DomNode>(parent);
            if (insertionPoint == null)
                return false;


            foreach (DomNode node in childNodes)
            {                
                if (!node.Is<IGameObject>() || !node.GetRoot().Is<IGame>())
                    return false;

                // Don't allow parenting cycles
                foreach (DomNode ancestor in insertionPoint.Lineage)
                    if (node.Equals(ancestor))
                        return false;
                // Don't reparent to same parent
                if (insertionPoint != DomNode && node.Parent == insertionPoint)
                    return false;
                
            }         
            return true;
        }

        public void Insert(object parent, object insertingObject)
        {
            if (parent == null)
                parent = m_activeItem ?? this;

            IEnumerable<DomNode> childNodes = GetNodes(insertingObject);
            if (childNodes == null) return;

            // If the parent is the LayeringContext itself:
            // create a new layer for the objects about to be inserted
            ILayer layer = Adapters.As<ILayer>(parent);
            if (layer == null)
            {
                DomNode node = new DomNode(Schema.layerType.Type);
                ILayer subLayer = node.As<ILayer>();
                subLayer.Name = "New Layer".Localize();
                Layers.Add(subLayer);
                layer = subLayer;
            }


            foreach (DomNode node in childNodes)
            {
                // Insert GameObjects
                IGameObject gameObject = node.As<IGameObject>();
                if (gameObject != null && !layer.Contains(gameObject))
                {
                    GameObjectReference newRef = GameObjectReference.Create(node);                    
                    layer.GameObjectReferences.Add(newRef);
                }

                // Insert References
                IReference<IGameObject> reference = node.As<IReference<IGameObject>>();
                if (reference != null)
                {
                    if (reference.Target != null && !layer.Contains(reference.Target))
                        layer.GameObjectReferences.Add(reference);
                }

                // Insert Sub-Layers
                ILayer otherLayer = node.As<ILayer>();
                if (otherLayer != null)
                    layer.Layers.Add(otherLayer);
            }
        }
       
        #endregion

        #region ILayeringContext Members

        public void SetActiveItem(object item)
        {
            m_activeItem = item;
        }

        #endregion

        #region ITreeView Members

        object ITreeView.Root
        {
            get { return m_layerRoot; }
        }

        IEnumerable<object> ITreeView.GetChildren(object parent)
        {           
            ILayer layer = Adapters.As<ILayer>(parent);
            if (layer != null)
            {
                foreach (Layer subLayer in layer.Layers)
                    yield return subLayer;
                foreach (GameObjectReference reference in layer.GameObjectReferences)
                    yield return reference;
            }
            else if (parent == m_layerRoot)
            {
                foreach (ILayer childLayer in Layers)
                    yield return childLayer;
            }            
        }

        #endregion

        #region IItemView Members

        public void GetInfo(object item, ItemInfo info)
        {
            ILayer layer = Adapters.As<ILayer>(item);
            if (layer != null)
            {
                info.Label = layer.Name;
                info.HasCheck = true;
                info.SetCheckState(GetCheckState(layer));
            }
            else
            {               
                IReference<IGameObject> reference = Adapters.As<IReference<IGameObject>>(item);
                if (reference != null)
                {
                    IListable listable = reference.As<IListable>();
                    listable.GetInfo(info);
                    //INameable nameable = Adapters.As<INameable>(reference.Target);
                    //if (nameable != null && !string.IsNullOrEmpty(nameable.Name))
                    //    info.Label = nameable.Name;

                    info.IsLeaf = true;
                    IVisible iVisible = GetIVisible(item);
                    if (iVisible != null)
                    {
                        info.HasCheck = true;
                        info.Checked = iVisible.Visible;
                    }
                }
            }
        }

        private CheckState GetCheckState(ILayer layer)
        {
            bool hasUncheckedChild = false;
            bool hasCheckedChild = false;

            foreach (ILayer subLayer in layer.Layers)
            {
                CheckState subCheckState = GetCheckState(subLayer);
                switch (subCheckState)
                {
                    case CheckState.Checked:
                        hasCheckedChild = true;
                        break;
                    case CheckState.Unchecked:
                        hasUncheckedChild = true;
                        break;
                    case CheckState.Indeterminate:
                        hasCheckedChild = true;
                        hasUncheckedChild = true;
                        break;
                }
            }

            if (!hasCheckedChild || !hasUncheckedChild)
            {
                foreach (IGameObject gameObject in layer.GetGameObjects())
                {
                    IVisible iVisible = gameObject.As<IVisible>();
                    if (iVisible != null)
                    {
                        if (iVisible.Visible)
                            hasCheckedChild = true;
                        else
                            hasUncheckedChild = true;
                    }
                }
            }

            if (hasCheckedChild && !hasUncheckedChild)
                return CheckState.Checked;
            if (hasUncheckedChild && !hasCheckedChild)
                return CheckState.Unchecked;

            return CheckState.Indeterminate;
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

       

        void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            ItemInserted.Raise(this, new ItemInsertedEventArgs<object>(e.Index, e.Child, e.Parent));
        }

        void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            ItemRemoved.Raise(this, new ItemRemovedEventArgs<object>(e.Index, e.Child, e.Parent));
        }

        void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {            
            ItemChanged.Raise(this, new ItemChangedEventArgs<object>(e.DomNode));            
        }
       

        #endregion

        #region IVisibilityContext Members

        public bool IsVisible(object item)
        {
            ILayer layer = item.As<ILayer>();
            return (layer == null || GetCheckState(layer) == CheckState.Checked);
        }

        public bool CanSetVisible(object item)
        {
            return Adapters.Is<ILayer>(item) || Adapters.Is<IVisible>(item);
        }

        public void SetVisible(object item, bool value)
        {
            ILayer layer = item.As<ILayer>();
            if (layer != null)
                PropagateVisible(layer, value);
            else
            {
                IVisible iVisible = GetIVisible(item);
                if (iVisible != null)
                    iVisible.Visible = value;
            }
        }

        private void PropagateVisible(ILayer layer, bool visible)
        {
            // Recursive call to update all sub-layers
            foreach (ILayer subLayer in layer.Layers)
                PropagateVisible(subLayer, visible);

            // Set visibility for all GameObjects
            foreach (IGameObject gameObject in layer.GetGameObjects())
            {
                IVisible iVisible = gameObject.As<IVisible>();
                if (iVisible != null)
                    iVisible.Visible = visible;
            }
        }

        private IVisible GetIVisible(object item)
        {
            IVisible iVisible;

            IReference<IGameObject> reference = item.As<IReference<IGameObject>>();
            if (reference != null)
                iVisible = Adapters.As<IVisible>(reference.Target);
            else
                iVisible = item.As<IVisible>();

            return iVisible;
        }

        #endregion

        private IEnumerable<DomNode> GetNodes(object insertingObject)
        {
            IDataObject dataObject = (IDataObject)insertingObject;
            object[] items = dataObject.GetData(typeof(object[])) as object[];
            if (items == null || items.Length == 0)
                return null;
            return Adapters.AsIEnumerable<DomNode>(items);
        }        
        private object m_activeItem;
    }
}
