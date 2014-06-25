//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using LevelEditorCore;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace LevelEditor.DomNodeAdapters
{
    /// <summary>
    /// Reference to a GameObject</summary>
    /// <remarks>This can be used to establish relationships between GameObjects:
    /// e.g. a Warrior can reference its Enemies or a Teleporter can reference a Switch that activates it</remarks>    
    public class GameObjectReference : DomNodeAdapter, IReference<IGameObject>, IListable
    {
        public static GameObjectReference Create(DomNode node)
        {
            if(node == null)
                throw new ArgumentNullException("node");
            if (!node.Is<IGameObject>())
                throw new ArgumentException(node.Type.Name + " is not derived from " +
                                            Schema.gameObjectType.Type.Name);           
            DomNode rootNode = node.GetRoot();
            GameDocument gameDoc = rootNode.As<GameDocument>();
            if(gameDoc == null)
                throw new ArgumentException("nod must belong to a document.");


            // create game object reference.
            DomNode refNode = new DomNode(Schema.gameObjectReferenceType.Type);
            GameObjectReference gobRef = refNode.As<GameObjectReference>();
            
            // Create Uri
            Uri ur = new Uri(gameDoc.Uri +"#"+node.GetId());
            refNode.SetAttribute(Schema.gameObjectReferenceType.refAttribute,ur);
            gobRef.m_target = node;
            return gobRef;

        }
       
        #region IReference<IGameObject> Members

        /// <summary>
        /// Returns true, iff the reference can reference the specified GameObject</summary>
        /// <param name="item"></param>
        /// <returns>True, iff the reference can reference the specified GameObject</returns>
        public bool CanReference(IGameObject item)
        {           
            return false;
        }

        /// <summary>
        /// Gets or sets the Target of the reference</summary>
        public IGameObject Target
        {
            get
            {
                if (m_target == null)
                    Resolve();
                return Adapters.As<IGameObject>(m_target);
            }
            set { throw new InvalidOperationException(); }
        }

        #endregion
       
        #region IListable Members

        /// <summary>
        /// Gets item info for the ProjectLister tree view and other controls</summary>
        /// <param name="info">Item info: passed in and modified by this method</param>
        /// <remarks>Label e.g.: [enemies] Oger_42</remarks>
        public void GetInfo(ItemInfo info)
        {
            info.Label = string.Format("[{0}] {1}",
                DomNode.ChildInfo != null ? DomNode.ChildInfo.Name : "",
                Target != null ? Target.Name : "<missing>");
            //info.ImageIndex = info.GetImageList().Images.IndexOfKey(
            //    Target != null ? Sce.Atf.Resources.ReferenceImage : Sce.Atf.Resources.ReferenceNullImage);
            
            info.IsLeaf = true;
        }

        #endregion

        private void Resolve()
        {
            if (m_target != null) return;
            Uri ur = GetAttribute<Uri>(Schema.gameObjectReferenceType.refAttribute);

            // find game document 
            Uri docUri = new Uri(ur.GetLeftPart(UriPartial.Path));
            var gameDocRegistry = Globals.MEFContainer.GetExportedValue<GameDocumentRegistry>();
            IGameDocument gamedoc = gameDocRegistry.FindDocument(docUri);
            if (gamedoc == null) return;
            string nodeId = ur.Fragment.TrimStart('#');
            DomNode target = gamedoc.Cast<IdToDomNode>().FindById(nodeId);
            m_target = target;            
        }
              
        private DomNode m_target;        
    }
}
