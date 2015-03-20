//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Controls.PropertyEditing;

using LevelEditorCore;

using PropertyDescriptor = System.ComponentModel.PropertyDescriptor;
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
        [ImportingConstructor]
        public SchemaLoader(IGameEngineProxy gameEngine)
        {
            m_gameEngine = gameEngine;
            // set resolver to locate embedded .xsd file
            SchemaResolver = new ResourceStreamResolver(Assembly.GetExecutingAssembly(), "LevelEditor.schemas");
            Load("level_editor.xsd");           
        }

        protected override void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {
            m_typeCollection = null;
            foreach (XmlSchemaTypeCollection typeCollection in GetTypeCollections())
            {
                m_namespace = typeCollection.TargetNamespace;
                m_typeCollection = typeCollection;
                Schema.Initialize(typeCollection);
                GameAdapters.Initialize(this);            
                // the level editor schema defines only one type collection
                break;
            }
            if (m_typeCollection == null) return;

            
            Schema.gameObjectComponentType.Type.SetTag(
                    new PropertyDescriptorCollection(
                        new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Name".Localize(),
                                Schema.gameObjectComponentType.nameAttribute,
                                null,
                                "Component name".Localize(),
                                false),                            
                            new AttributePropertyDescriptor(
                                "Active".Localize(),
                                Schema.gameObjectComponentType.activeAttribute,
                                null,
                                "Is this component active".Localize(),
                                false,
                                new BoolEditor())
                }));

            Schema.renderComponentType.Type.SetTag(
                   new PropertyDescriptorCollection(
                       new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "Visible".Localize(),
                                Schema.renderComponentType.visibleAttribute,
                                null,
                                "Component visiblity".Localize(),
                                false,
                                new BoolEditor()),                            
                            new AttributePropertyDescriptor(
                                "cast Shadow".Localize(),
                                Schema.renderComponentType.castShadowAttribute,
                                null,
                                "Is component casts shadaw".Localize(),
                                false,
                                new BoolEditor()),
                           new AttributePropertyDescriptor(
                                "Receive Shadow".Localize(),
                                Schema.renderComponentType.receiveShadowAttribute,
                                null,
                                "Is component receive  shadow".Localize(),
                                false,
                                new BoolEditor()),

                           new AttributePropertyDescriptor(
                                "Draw Distance".Localize(),
                                Schema.renderComponentType.drawDistanceAttribute,
                                null,
                                "Minimum distance to draw the component".Localize(),
                                false)
                }));
                     
            Schema.spinnerComponentType.Type.SetTag(
                   new PropertyDescriptorCollection(
                       new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "RPS".Localize(),
                                Schema.spinnerComponentType.rpsAttribute,
                                null,
                                "Revolutions per second".Localize(),
                                false,
                                new NumericTupleEditor(typeof(float),new string[] { "x", "y", "z" })
                                )
                       }));



            ResourceInfo resInfo = m_gameEngine.Info.ResourceInfos.GetByType(ResourceTypes.Model);
            string filter = resInfo != null ? resInfo.Filter : null;
            var refEdit = new FileUriEditor(filter);
            Schema.meshComponentType.Type.SetTag(
                   new PropertyDescriptorCollection(
                       new PropertyDescriptor[] {
                            new AttributePropertyDescriptor(
                                "3d Model".Localize(),
                                Schema.meshComponentType.refAttribute,
                                null,
                                "path to 3d model".Localize(),
                                false,
                                refEdit
                                )}));



            var collectionEditor = new EmbeddedCollectionEditor();

            // the following  lambda's handles (add, remove, move ) items.
            collectionEditor.GetItemInsertersFunc = (context) =>
            {
                var list = context.GetValue() as IList<DomNode>;
                if (list == null) return EmptyArray<EmbeddedCollectionEditor.ItemInserter>.Instance;

                // create ItemInserter for each component type.
                var insertors = new EmbeddedCollectionEditor.ItemInserter[2];

                insertors[0] = new EmbeddedCollectionEditor.ItemInserter("StaticMeshComponent",
                    delegate
                    {
                        DomNode node = new DomNode(Schema.meshComponentType.Type);
                        node.SetAttribute(Schema.gameObjectComponentType.nameAttribute, node.Type.Name);
                        list.Add(node);
                        return node;
                    });


                insertors[1] = new EmbeddedCollectionEditor.ItemInserter("SpinnerComponent",
                    delegate
                    {
                        DomNode node = new DomNode(Schema.spinnerComponentType.Type);
                        node.SetAttribute(Schema.gameObjectComponentType.nameAttribute, node.Type.Name);
                        list.Add(node);
                        return node;
                    });

                return insertors;                
            };


            collectionEditor.RemoveItemFunc = (context, item) =>
            {
                var list = context.GetValue() as IList<DomNode>;
                if (list != null)
                    list.Remove(item.Cast<DomNode>());
            };


            collectionEditor.MoveItemFunc = (context, item, delta) =>
            {
                var list = context.GetValue() as IList<DomNode>;
                if (list != null)
                {
                    DomNode node = item.Cast<DomNode>();
                    int index = list.IndexOf(node);
                    int insertIndex = index + delta;
                    if (insertIndex < 0 || insertIndex >= list.Count)
                        return;
                    list.RemoveAt(index);
                    list.Insert(insertIndex, node);
                }

            };

            // add child property descriptors gameObjectType
            Schema.gameObjectType.Type.SetTag(
                   new PropertyDescriptorCollection(
                       new PropertyDescriptor[] {
                            new ChildPropertyDescriptor(
                                "Components".Localize(),
                                Schema.gameObjectType.componentChild,
                                null,
                                "List of GameObject Components".Localize(),
                                false,
                                collectionEditor)
                                }));                         
        }

        protected override void ParseAnnotations(
            XmlSchemaSet schemaSet,
            IDictionary<NamedMetadata, IList<XmlNode>> annotations)
        {
            base.ParseAnnotations(schemaSet, annotations);

            foreach (var kv in annotations)
            {                               
                DomNodeType nodeType = kv.Key as DomNodeType;
                if (kv.Value.Count == 0) continue;

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
                    if (!string.IsNullOrEmpty(category))
                    {
                        NodeTypePaletteItem item = new NodeTypePaletteItem(nodeType, name, description, image, category, menuText);
                        nodeType.SetTag<NodeTypePaletteItem>(item);
                    }
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
        
        private IGameEngineProxy m_gameEngine;
    }
}
