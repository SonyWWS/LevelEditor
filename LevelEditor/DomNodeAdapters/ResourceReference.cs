//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Collections.Generic;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using LevelEditorCore;

namespace LevelEditor.DomNodeAdapters
{
    /// <summary>
    /// Reference to a IResource</summary>
    public class ResourceReference : DomNodeAdapter, IReference<IResource>, IListable, IHierarchical
    {
        public static IReference<IResource> Create(IResource resource)
        {
            return Create(null, resource);
        }

        public static IReference<IResource> Create(DomNodeType domtype,IResource resource)
        {
            if (resource == null)
                throw new ArgumentNullException("resource");

            if(domtype == null)
                domtype = Schema.resourceReferenceType.Type;

            if (!Schema.resourceReferenceType.Type.IsAssignableFrom(domtype))
                return null;
            
            ResourceReference resRef = null;
            if (CanReference(domtype, resource))
            {
                resRef = new DomNode(domtype).As<ResourceReference>();
                resRef.m_target = resource;
                resRef.SetAttribute(Schema.resourceReferenceType.uriAttribute, resource.Uri);                
            }
            return resRef;
        }

        public static bool CanReference(DomNodeType domtype, IResource resource)
        {
            if (domtype == null || resource == null || !Schema.resourceReferenceType.Type.IsAssignableFrom(domtype))
                return false;
            // valid resource file extensions
            var exts = (HashSet<string>)domtype.GetTag(Annotations.ReferenceConstraint.ValidResourceFileExts);
            string reExt = Path.GetExtension(resource.Uri.LocalPath).ToLower();
            bool canReference = exts == null || exts.Contains(".*") || exts.Contains(reExt);
            return canReference;            
        }
      
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            Uri resUri = GetAttribute<Uri>(Schema.resourceReferenceType.uriAttribute);
            if (resUri != null)
                m_target = Globals.ResourceService.Load(resUri);            
        }

        #region IReference<IResource> Members

        /// <summary>
        /// Always returns true, as any IResource can be referenced and null is acceptable</summary>
        /// <param name="item">Resource to be referenced, can be null</param>
        /// <returns>Always true</returns>
        public bool CanReference(IResource item)
        {
            return false;
        }

        /// <summary>
        /// Gets or sets the referenced IResource</summary>
        public IResource Target
        {
            get { return m_target; }
            set { throw new InvalidOperationException(); }
        }

        #endregion

        #region IListable Members

        /// <summary>
        /// Provides info for the ProjectLister tree view and other controls</summary>
        /// <param name="info">Item info passed in and modified by the method</param>
        public void GetInfo(ItemInfo info)
        {
            info.Label = string.Format("[{0}] {1}",
                DomNode.ChildInfo != null ? DomNode.ChildInfo.Name : "<missing>",
                (Target != null && Target.Uri != null) ? Path.GetFileName(Target.Uri.LocalPath) : "");
            info.ImageIndex = info.GetImageList().Images.IndexOfKey(
                Target != null ? Sce.Atf.Resources.ReferenceImage : Sce.Atf.Resources.ReferenceNullImage);
            info.IsLeaf = true;
        }

        #endregion

        #region IHierarchical Members

        bool IHierarchical.CanAddChild(object child)
        {
            if (DomNode.Parent != null)
            {
                Slot slot = new Slot(this.DomNode.Parent, this.DomNode.ChildInfo);
                return slot.CanAddChild(child);
            }
            return false;            
        }

        bool IHierarchical.AddChild(object child)
        {
            if (DomNode.Parent == null)
                return false;

            ChildInfo chInfo = DomNode.ChildInfo;
            Slot slot = new Slot(this.DomNode.Parent, chInfo);
            bool added = slot.AddChild(child);
            if (added && chInfo.IsList)
            {
                DomNode.RemoveFromParent();
            }
            return added;
        }

        #endregion
        
        private IResource m_target;

       
    }
}
