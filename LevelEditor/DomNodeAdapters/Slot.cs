//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using LevelEditorCore;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using LevelEditor.DomNodeAdapters;


namespace LevelEditor
{
    /// <summary>
    /// Placeholder representing a potential reference child</summary>
    /// <remarks>Slots are used by the ProjectLister to show an empty slots for
    /// reference lists to which mebers can be added.</remarks>
    public class Slot : IListable, IHierarchical
    {
        public Slot(DomNode owner, ChildInfo childInfo)
        {
            if (owner == null || childInfo == null)
                throw new ArgumentNullException();

            m_owner = owner;
            m_childInfo = childInfo;
        }

        /// <summary>
        /// Gets the child info, representing the child list
        /// to which objects can be inserted</summary>
        public ChildInfo ChildInfo
        {
            get { return m_childInfo; }
        }

        /// <summary>
        /// Gets the owner that will become the parent of objects
        /// inserted into this slot</summary>
        public DomNode Owner
        {
            get { return m_owner; }
        }

        #region IListable Members

        public void GetInfo(ItemInfo info)
        {
            info.Label = string.Format("[{0}]", ChildInfo != null ? ChildInfo.Name : "<missing>");
            info.ImageIndex = info.GetImageList().Images.IndexOfKey(Sce.Atf.Resources.ReferenceNullImage);
            info.IsLeaf = true;
        }

        #endregion

        #region IHierarchical Members

        /// <summary>
        /// Returns true iff a reference to specified child can be inserted into this slot</summary>
        /// <param name="child">Child to be inserted</param>
        /// <returns>True iff the specified child can be inserted into this slot</returns>
        public bool CanAddChild(object child)
        {            
            DomNode childNode = child.As<DomNode>();
            bool isGobRef = child.Is<IGameObject>() && childNode.GetRoot().Is<IGame>() && m_childInfo.Type == Schema.gameObjectReferenceType.Type;
            bool isResRef = ResourceReference.CanReference(m_childInfo.Type, child.As<IResource>());

            return isGobRef || isResRef;            
        }

        /// <summary>
        /// Inserts a reference to the specified child into the Owner
        /// in the List specified by the ChildInfo property</summary>
        /// <param name="child">Child to be inserted</param>
        public bool AddChild(object child)
        {
            DomNode childNode = child.As<DomNode>();
            bool isGobRef = child.Is<IGameObject>() && childNode.GetRoot().Is<IGame>() && m_childInfo.Type == Schema.gameObjectReferenceType.Type;
            bool isResRef = ResourceReference.CanReference(m_childInfo.Type, child.As<IResource>());
            
            DomNode refNode = null;
            if (isGobRef)
            {
                GameObjectReference gobRef = GameObjectReference.Create(childNode);
                refNode = gobRef.Cast<DomNode>();
            }
            else if(isResRef)
            {
                IReference<IResource> resReference =
                    ResourceReference.Create(m_childInfo.Type, child.As<IResource>());
                refNode = resReference.As<DomNode>();
            }
          
            if (m_childInfo.IsList)
                m_owner.GetChildList(m_childInfo).Add(refNode);
            else
                m_owner.SetChild(m_childInfo, refNode);
            return true;                   
        }

        #endregion

        private readonly ChildInfo m_childInfo;
        private readonly DomNode m_owner;
    }
}
