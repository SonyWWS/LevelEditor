//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

using LevelEditorCore;

namespace LevelEditor.DomNodeAdapters
{
    /// <summary>
    /// Prefab instance.</summary>
    public class PrefabInstance : DomNodeAdapter
    {
        public static PrefabInstance Create(IPrefab prefab)
        {
            DomNode instNode = new DomNode(Schema.prefabInstanceType.Type);
            PrefabInstance inst = instNode.As<PrefabInstance>();
            inst.m_prefab = prefab;
            instNode.InitializeExtensions();
            inst.Resolve(null);
            return inst;
        }

        public IEnumerable<IGameObject> GameObjects
        {
            get { return GetChildList<IGameObject>(Schema.prefabInstanceType.gameObjectChild);}
        }
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            DomNode.ChildRemoving += new EventHandler<ChildEventArgs>(DomNodeStructureChanged);
            DomNode.ChildInserting += new EventHandler<ChildEventArgs>(DomNodeStructureChanged);
            DomNode.AttributeChanged += new EventHandler<AttributeEventArgs>(DomNode_AttributeChanged);
       
            m_overrideList = GetChildList<ObjectOverride>(Schema.prefabInstanceType.objectOverrideChild);
            foreach (var objectOverride in m_overrideList)
            {
                m_overridesMap.Add(objectOverride.ObjectName, objectOverride);
            }

            if(DomNode.Parent == null)
                Resolve(null);

        }

        private IList<ObjectOverride> m_overrideList;
        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            if (e.DomNode == this.DomNode || !e.DomNode.Is<IGameObject>())
                return;

            if (!e.AttributeInfo.Equivalent(e.DomNode.Type.IdAttribute))
            {
                IGameObject gob = e.DomNode.Cast<IGameObject>();                
                string originalName = m_intsToOriginal[e.DomNode];
                ObjectOverride objectOverride;
                m_overridesMap.TryGetValue(originalName, out objectOverride);
                if (objectOverride == null)
                {
                    objectOverride = ObjectOverride.Create(originalName);                    
                    m_overrideList.Add(objectOverride);
                    m_overridesMap.Add(originalName, objectOverride);
                }

                AttributeOverride attrOverride = objectOverride.GetOrCreateByName(e.AttributeInfo.Name);
                attrOverride.AttribValue = e.AttributeInfo.Type.Convert(e.NewValue);

            }                      
        }

        private void DomNodeStructureChanged(object sender, ChildEventArgs e)
        {
            if (!m_updating && e.ChildInfo.Equivalent(Schema.prefabInstanceType.gameObjectChild))
            {
                throw new InvalidTransactionException("Structure of PrefabInstance cannot be changesd");
            }
        }

        private bool m_updating;
        public void Resolve(UniqueNamer namer)
        {
            try
            {
                m_updating = true;
                Uri resUri = GetAttribute<Uri>(Schema.prefabInstanceType.prefabRefAttribute);
                if (resUri != null)
                    m_prefab = Globals.ResourceService.Load(resUri) as IPrefab;
                if (m_prefab == null) return;

                // update name and uri                
                if(resUri == null)
                    SetAttribute(Schema.prefabInstanceType.prefabRefAttribute, m_prefab.Uri);

                IGameObjectGroup gobgroup = DomNode.As<IGameObjectGroup>();
                if (string.IsNullOrWhiteSpace(gobgroup.Name))
                    gobgroup.Name = "PrefabInst_" + m_prefab.Name;

                DomNode[] gobs = DomNode.Copy(m_prefab.GameObjects.AsIEnumerable<DomNode>());
                HashSet<string> gobIds = new HashSet<string>();
                
                gobgroup.GameObjects.Clear();                
                foreach (var gobNode in gobs)
                {
                    gobNode.InitializeExtensions();
                    IGameObject gob = gobNode.As<IGameObject>();
                    m_intsToOriginal.Add(gobNode, gob.Name);
                    gobIds.Add(gob.Name);
                    ObjectOverride objectOverride;                    
                    m_overridesMap.TryGetValue(gob.Name, out objectOverride);
                    updateNode(gobNode, objectOverride);
                    string name = gob.Name;
                    if (namer != null)
                        gob.Name = namer.Name(gob.Name);
                    
                    gobgroup.GameObjects.Add(gob);
                }

                // cleanup m_overridesmap
                List<string> overrideIds = new List<string>(m_overridesMap.Keys);
                foreach (string id in overrideIds)
                {
                    if (!gobIds.Contains(id))
                    {
                        ObjectOverride objectOverride = m_overridesMap[id];
                        m_overridesMap.Remove(id);
                        m_overrideList.Remove(objectOverride);
                    }
                }
            }
            finally
            {
                m_updating = false;
            }
        }

        private void updateNode(DomNode node, ObjectOverride objectOverride)
        {
            if (node == null || objectOverride == null)
                return;
            string nodeId = node.GetId();
            System.Diagnostics.Debug.Assert(nodeId == objectOverride.ObjectName);

            foreach (AttributeOverride attrOverride in objectOverride.AttributeOverrides)
            {
                AttributeInfo attrInfo = node.Type.GetAttributeInfo(attrOverride.Name);
                node.SetAttribute(attrInfo, attrInfo.Type.Convert(attrOverride.AttribValue));
            }

        }

        // maps the id of the instance to object override
        private Dictionary<string, ObjectOverride>
            m_overridesMap = new Dictionary<string, ObjectOverride>();
            
        // maps instances node to the Id of the original node
        private Dictionary<DomNode, string> m_intsToOriginal
            = new Dictionary<DomNode, string>();
        private IPrefab m_prefab;
    }
    
    public class AttributeOverride : DomNodeAdapter
    {
        public static AttributeOverride Create(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("name cannot be null or empty");

            DomNode node = new DomNode(Schema.attributeOverrideType.Type);
            AttributeOverride attrOverride = node.As<AttributeOverride>();
            attrOverride.Name = name;
            attrOverride.AttribValue = value;
            return attrOverride;
        }
        public string Name
        {
            get { return GetAttribute<string>(Schema.attributeOverrideType.nameAttribute); }
            private set 
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidTransactionException("name cannot be null");
                SetAttribute(Schema.attributeOverrideType.nameAttribute, value);
            }
        }

        public string AttribValue
        {
            get { return GetAttribute<string>(Schema.attributeOverrideType.valueAttribute); }
            set { SetAttribute(Schema.attributeOverrideType.valueAttribute, value); }
        }
    }
    public class ObjectOverride : DomNodeAdapter
    {
        public static ObjectOverride Create(string name)
        {
            DomNode node = new DomNode(Schema.objectOverrideType.Type);
            ObjectOverride objectOverride = node.As<ObjectOverride>();
            objectOverride.ObjectName = name;
            return objectOverride;
        }

        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            m_overrides = GetChildList<AttributeOverride>(Schema.objectOverrideType.attributeOverrideChild); 
        }
        public string ObjectName
        {
            get{return GetAttribute<string>(Schema.objectOverrideType.objectNameAttribute);}
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new InvalidTransactionException("type name cannot be empty or null");
                SetAttribute(Schema.objectOverrideType.objectNameAttribute, value);
            }
        }

        /// <summary>
        /// Gets AttributeOverride by name
        /// if not found then it will create new instacne</summary>        
        public AttributeOverride GetOrCreateByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("name cannot be null or empty");

            //note:linear search is OK there are not many items.
            // find AttributeOverride by name.
            foreach (var attrib in m_overrides)
            {
                if (attrib.Name == name)
                    return attrib;
            }

            // if not found  create new instance and add it to the list.
            AttributeOverride attrOverride = AttributeOverride.Create(name, "");
            m_overrides.Add(attrOverride);
            return attrOverride;
        }
        public IEnumerable<AttributeOverride> AttributeOverrides
        {
            get { return m_overrides; }
        }

        private IList<AttributeOverride> m_overrides;        
    }
}
