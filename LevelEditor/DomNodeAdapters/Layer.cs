//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

using LevelEditorCore;

namespace LevelEditor.DomNodeAdapters
{
    /// <summary>
    /// Adapter for layer folders
    /// </summary>
    public class Layer : DomNodeAdapter, ILayer, IHierarchical, INameable
    {
        #region INameable Members

        public string Name
        {
            get { return (string)DomNode.GetAttribute(Schema.layerType.nameAttribute); }
            set { DomNode.SetAttribute(Schema.layerType.nameAttribute, value); }
        }

        #endregion

        #region ILayer Members

        public IList<ILayer> Layers
        {
            get { return GetChildList<ILayer>(Schema.layerType.layerChild); }
        }

        public IList<IReference<IGameObject>> GameObjectReferences
        {
            get { return GetChildList<IReference<IGameObject>>(Schema.layerType.gameObjectReferenceChild); }
        }

        #endregion

        #region IHierarchical Members

        public bool CanAddChild(object child)
        {
            DomNode domNode = child.As<DomNode>();
            if (domNode.GetRoot() != DomNode.GetRoot())
                return false;
            
            return child.Is<ILayer>() || child.Is<IReference<IGameObject>>() || child.Is<IGameObject>();
        }

        public bool AddChild(object child)
        {
            bool added = false;
            ILayer layer = child.As<ILayer>();
            if (layer != null)
            {
                if (!Layers.Contains(layer))
                {
                    Layers.Add(layer);
                    added = true;
                }
            }
            else
            {
                IReference<IGameObject> reference = child.As<IReference<IGameObject>>();
                if (reference != null)
                {
                    if (reference.Target != null
                        && !this.Contains(reference.Target)
                        && !GameObjectReferences.Contains(reference))
                    {
                        GameObjectReferences.Add(reference);
                        added = true;
                    }
                }
                else
                {
                    IGameObject gameObject = child.As<IGameObject>();
                    if (gameObject != null && !this.Contains(gameObject))
                    {
                        DomNode referenceNode = new DomNode(Schema.gameObjectReferenceType.Type);
                        reference = referenceNode.Cast<IReference<IGameObject>>();
                        reference.Target = gameObject;
                        GameObjectReferences.Add(reference);
                        added = true;
                    }
                }
            }
            return added;
        }

        #endregion
    }
}
