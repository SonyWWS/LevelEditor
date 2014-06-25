//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using LevelEditorCore;

namespace LevelEditor.DomNodeAdapters
{
    /// <summary>
    /// DomNodeAdapter for game object groups</summary>
    public class GameObjectGroup : GameObject, IGameObjectGroup
    {
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            
            // Register child events
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += DomNode_ChildRemoved;                      
            CheckScaleFlags();
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            if (e.Parent == DomNode)
            {
                CheckScaleFlags();
            }
           
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            if (e.Parent == DomNode)
            {
                CheckScaleFlags();
            }           
        }

        private void CheckScaleFlags()
        {
            ITransformable thisNode = this.As<ITransformable>();
            if (thisNode == null)
                return;

            // Assume that Scale is set and UniformScale is not set until we know otherwise.
            TransformationTypes newFlags = thisNode.TransformationType;
            newFlags |= TransformationTypes.Scale;
            newFlags &= ~TransformationTypes.UniformScale;

            IEnumerable<ITransformable> transformables = GetChildList<ITransformable>(Schema.gameObjectGroupType.gameObjectChild);
            foreach (ITransformable childNode in transformables)
            {
                if ((childNode.TransformationType & TransformationTypes.Scale) == 0)
                        newFlags &= ~(TransformationTypes.Scale);
                if ((childNode.TransformationType & TransformationTypes.UniformScale) != 0)
                        newFlags |= TransformationTypes.UniformScale;                                
            }

            thisNode.TransformationType = newFlags;
        }

        /// <summary>
        /// Gets game object children</summary>
        public IList<IGameObject> GameObjects
        {
            get { return GetChildList<IGameObject>(Schema.gameObjectGroupType.gameObjectChild); }
        }

        #region IHierarchical Members

        public bool CanAddChild(object child)
        {
            return (child.Is<IGameObject>());
        }

        public bool AddChild(object child)
        {
            IGameObject gameObject = child.As<IGameObject>();
            if (gameObject == null)
                return false;
            GameObjects.Add(gameObject);
            return true;            
        }

        #endregion

        #region IListable Members

        /// <summary>
        /// Gets display info (label, icon, ...) for the ProjectLister and other controls</summary>
        /// <param name="info">Item info: passed in and modified by this method</param>
        public override void GetInfo(ItemInfo info)
        {
            info.ImageIndex = Util.GetTypeImageIndex(DomNode.Type, info.GetImageList());
            info.Label = Name;
            info.IsLeaf = GameObjects.Count == 0;

            if (IsLocked)
                info.StateImageIndex = info.GetImageList().Images.IndexOfKey(Sce.Atf.Resources.LockImage);
        }

        #endregion
       
    }
}
