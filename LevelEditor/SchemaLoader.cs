//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using LevelEditorCore.GameEngineProxy;
using Sce.Atf;
using Sce.Atf.Dom;

using LevelEditorCore;

namespace LevelEditor
{
    /// <summary>
    /// Xml schema loader for the LevelEditor's schemas</summary>
    [Export(typeof(XmlSchemaTypeLoader))]
    [Export(typeof(SchemaLoader))]
    [Export(typeof(ISchemaLoader))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SchemaLoader : XmlSchemaTypeLoader, ISchemaLoader
    {
        public SchemaLoader()
        {
            // set resolver to locate embedded .xsd file
            SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "LevelEditor.schemas");
            Load("level_editor.xsd");           
        }

        protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {           
            foreach (XmlSchemaTypeCollection typeCollection in GetTypeCollections())
            {
                m_namespace = typeCollection.TargetNamespace;
                m_typeCollection = typeCollection;
                Schema.Initialize(typeCollection);
                GameAdapters.Initialize(this);            
                // the level editor schema defines only one type collection
                //break;
            }            
        }

        protected override void ParseAnnotations(
            XmlSchemaSet schemaSet,
            IDictionary<NamedMetadata, IList<XmlNode>> annotations)
        {
            base.ParseAnnotations(schemaSet, annotations);

            foreach (var kv in annotations)
            {                               
                DomNodeType nodeType = kv.Key as DomNodeType;
                if (nodeType == null || kv.Value.Count == 0) continue;

                // create a hash of hidden attributes                
                HashSet<string> hiddenprops = new HashSet<string>();
                foreach (XmlNode xmlnode in kv.Value)
                {
                    
                    if (xmlnode.LocalName == "scea.dom.editors.attribute")
                    {
                        XmlAttribute hiddenAttrib = xmlnode.Attributes["hide"];
                        if (hiddenAttrib != null && hiddenAttrib.Value == "true")
                        {
                            XmlAttribute nameAttrib = xmlnode.Attributes["name"];
                            string name = (nameAttrib != null) ? nameAttrib.Value : null;
                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                hiddenprops.Add(name);
                            }                            
                        }
                    }
                }
                if (hiddenprops.Count > 0)
                {
                    nodeType.SetTag(HiddenProperties, hiddenprops);
                }
                
                PropertyDescriptorCollection localDescriptor = nodeType.GetTagLocal<PropertyDescriptorCollection>();
                PropertyDescriptorCollection annotationDescriptor = Sce.Atf.Dom.PropertyDescriptor.ParseXml(nodeType, kv.Value);
                
                // if the type already have local property descriptors 
                // then add annotation driven property descriptors to it.
                if (localDescriptor != null)
                {
                    foreach (System.ComponentModel.PropertyDescriptor propDecr in annotationDescriptor)
                    {
                        localDescriptor.Add(propDecr);
                    }
                }
                else
                {
                    localDescriptor = annotationDescriptor;
                }
               
                if (localDescriptor.Count > 0)
                    nodeType.SetTag<PropertyDescriptorCollection>(localDescriptor);


                // process annotations resourceReferenceTypes.
                XmlNode rfNode = FindElement(kv.Value, Annotations.ReferenceConstraint.Name);
                if (rfNode != null)
                {
                    HashSet<string> extSet = null;
                    string exts = FindAttribute(rfNode, Annotations.ReferenceConstraint.ValidResourceFileExts);
                    if (!string.IsNullOrWhiteSpace(exts))
                    {
                        exts = exts.ToLower();
                        char[] sep = { ',' };
                        extSet = new HashSet<string>(exts.Split(sep, StringSplitOptions.RemoveEmptyEntries));
                        
                    }
                    else if(m_gameEngine != null)
                    {
                        string restype = FindAttribute(rfNode, Annotations.ReferenceConstraint.ResourceType);
                        ResourceInfo resInfo = m_gameEngine.Info.ResourceInfos.GetByType(restype);
                        if (resInfo != null)
                            extSet = new HashSet<string>(resInfo.FileExts);
                    }

                    if(extSet != null)
                        nodeType.SetTag(Annotations.ReferenceConstraint.ValidResourceFileExts, extSet);
                }
                             
                // todo use schema annotation to mark  Palette types.                    
                XmlNode xmlNode = FindElement(kv.Value, "scea.dom.editors");
                if (xmlNode != null)
                {
                    string name = FindAttribute(xmlNode, "name");
                    string description = FindAttribute(xmlNode, "description");
                    string image = FindAttribute(xmlNode, "image");
                    string category = FindAttribute(xmlNode, "category");
                    string menuText = FindAttribute(xmlNode, "menuText");
                    NodeTypePaletteItem item = new NodeTypePaletteItem(nodeType, name, description, image, category,
                                                                       menuText);
                    nodeType.SetTag<NodeTypePaletteItem>(item);
                }
            }           
        }

        /// <summary>
        /// Name of a DomNodeType tag that contains hashset of properties to be hidden.</summary>
        public const string HiddenProperties = "HiddenProperties";


        #region ISchemaLoader Members

        public string NameSpace
        {
            get { return m_namespace; }
        }
        private string m_namespace;

        public XmlSchemaTypeCollection TypeCollection
        {
            get { return m_typeCollection; }
        }
        private XmlSchemaTypeCollection m_typeCollection;
        
        public DomNodeType GameType
        {
            get { return Schema.gameType.Type; }
        }

        public DomNodeType GameObjectType
        {
            get { return Schema.gameObjectType.Type; }
        }

        /// <summary>
        /// Gets DomNodeType of ResourceReference.</summary>
        public DomNodeType ResourceReferenceType
        {
            get { return Schema.resourceReferenceType.Type; }
        }

        /// <summary>
        /// Gets DomNodeType of GameReference.</summary>
        public DomNodeType GameReferenceType
        {
            get { return Schema.gameReferenceType.Type; }
        }

        /// <summary>
        /// Gets DomNodeType of GameObjectReference.</summary>
        public DomNodeType GameObjectReferenceType
        {
            get { return Schema.gameObjectReferenceType.Type; }
        }

        /// <summary>
        /// Gets DomNodeType of GameObjectGroup.</summary>
        public DomNodeType GameObjectGroupType
        {
            get { return Schema.gameObjectGroupType.Type; }
        }

        /// <summary>
        /// Gets DomNodeType of GameObjectFolder.</summary>
        public DomNodeType GameObjectFolderType
        {
            get { return Schema.gameObjectFolderType.Type; }
        }

        public ChildInfo GameRootElement
        {
            get { return Schema.gameRootElement; }
        }      
 
        #endregion        


        [Import(AllowDefault = true)]
        private IGameEngineProxy m_gameEngine;
    }
}
