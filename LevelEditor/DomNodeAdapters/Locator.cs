//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.IO;

using LevelEditorCore;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace LevelEditor.DomNodeAdapters
{
    /// <summary>
    /// DomNodeAdapter for locators, to place resource in a game</summary>
    public class Locator : GameObject, IHierarchical
    {        

        /// <summary>
        /// Gets or sets the resource reference</summary>
        public IReference<IResource> Reference
        {
            get { return GetChild<IReference<IResource>>(Schema.locatorType.resourceChild); }
            set { SetChild(Schema.locatorType.resourceChild, value.As<DomNode>()); }
        }
      
       
        #region IHierarchical Members

        /// <summary>
        /// Returns true iff the specified child can be inserted</summary>
        /// <param name="child">Child to be inserted</param>
        /// <returns>True iff the specified child can be inserted</returns>
        /// <remarks>Call forwarded to Resource child</remarks>
        public bool CanAddChild(object child)
        {
            Slot slot = new Slot(this.DomNode, Schema.locatorType.resourceChild);
            return slot.CanAddChild(child);            
        }

        /// <summary>
        /// Inserts the specified child</summary>
        /// <param name="child">Child to be inserted</param>
        /// <remarks>Call forwarded to Resource child</remarks>
        public bool AddChild(object child)
        {
            Slot slot = new Slot(this.DomNode, Schema.locatorType.resourceChild);
            return slot.AddChild(child);
        }

        #endregion

        #region IListable Members

        /// <summary>
        /// Provides info for the ProjectLister tree view and other controls</summary>
        /// <param name="info">Item info passed in and modified by the method</param>
        public override void GetInfo(ItemInfo info)
        {            
            info.ImageIndex = Util.GetTypeImageIndex(DomNode.Type, info.GetImageList());
            if (Reference != null && Reference.Target != null && Reference.Target.Uri != null)
                info.Label = string.Format("{0}: {1}", Name, Path.GetFileName(Reference.Target.Uri.LocalPath));
            else
                info.Label = Name;
            if (IsLocked)
                info.StateImageIndex = info.GetImageList().Images.IndexOfKey(Sce.Atf.Resources.LockImage);

            info.IsLeaf = false;
        }

        #endregion
       
        public static Locator Create()
        {
            Locator locator = new DomNode(Schema.locatorType.Type).As<Locator>();
            locator.Name = "Locator".Localize();
            return locator;
        }
    }
}
