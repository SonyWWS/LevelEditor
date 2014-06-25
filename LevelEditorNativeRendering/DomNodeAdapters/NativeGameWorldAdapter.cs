//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;
using Sce.Atf.Adaptation;

namespace RenderingInterop
{
    public class NativeGameWorldAdapter : DomNodeAdapter
    {
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            DomNode node = this.DomNode;
            
            node.ChildInserted += node_ChildInserted;
            node.ChildRemoved += node_ChildRemoved;
            ManageNativeObjectLifeTime = true;
        }

        void node_ChildInserted(object sender, ChildEventArgs e)
        {                        
            Insert(e.Parent, e.Child, e.ChildInfo, e.Index);
        }

        void node_ChildRemoved(object sender, ChildEventArgs e)
        {                        
            Remove(e.Parent, e.Child, e.ChildInfo);
        }

        /// <summary>
        /// Gets/Sets whether this adapter  creates native object on child inserted 
        /// and deletes it on child removed.
        /// The defautl is true.
        /// </summary>
        public bool ManageNativeObjectLifeTime
        {
            get;
            set;
        }
        public void Remove(DomNode parent, DomNode child, ChildInfo chInfo)
        {
            NativeObjectAdapter childObject = child.As<NativeObjectAdapter>();
            NativeObjectAdapter parentObject = parent.As<NativeObjectAdapter>();

            object listIdObj = chInfo.GetTag(NativeAnnotations.NativeElement);

            if (childObject == null || parentObject == null || listIdObj == null)
                return;

            uint listId = (uint)listIdObj;
            uint typeId = (uint)chInfo.DefiningType.GetTag(NativeAnnotations.NativeType);
            ulong parentId = parentObject.InstanceId;
            ulong childId = childObject.InstanceId;
            GameEngine.ObjectRemoveChild(typeId, listId, parentId, childId);
            if (ManageNativeObjectLifeTime)
            {                
                GameEngine.DestroyObject(childObject);
            }
        }

        public void Insert(DomNode parent, DomNode child, ChildInfo chInfo, int index)
        {
            NativeObjectAdapter childObject = child.As<NativeObjectAdapter>();
            NativeObjectAdapter parentObject = parent.As<NativeObjectAdapter>();

            object listIdObj = chInfo.GetTag(NativeAnnotations.NativeElement);

            if (childObject == null || parentObject == null || listIdObj == null)
                return;

            if (chInfo.IsList && index >= (parent.GetChildList(chInfo).Count - 1))
                index = -1;

            if (ManageNativeObjectLifeTime)
            {
                GameEngine.CreateObject(childObject);
                childObject.UpdateNativeOjbect();
            }
            System.Diagnostics.Debug.Assert(childObject.InstanceId != 0);

            uint listId = (uint)listIdObj;
            uint typeId = (uint)chInfo.DefiningType.GetTag(NativeAnnotations.NativeType);
            ulong parentId = parentObject.InstanceId;
            ulong childId = childObject.InstanceId;

            if (index >= 0)
            {
                GameEngine.ObjectInsertChild(typeId, listId, parentId, childId, index);
            }
            else
            {
                GameEngine.ObjectAddChild(typeId, listId, parentId, childId);
            }

            foreach (var node in child.Children)
            {
                Insert(child, node, node.ChildInfo, -1); // use -1 for index to indicate an append operation.
            }
        }    
    }
}
