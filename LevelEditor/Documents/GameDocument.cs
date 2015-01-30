//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using Sce.Atf;

using Sce.Atf.Dom;
using Sce.Atf.Adaptation;

using LevelEditorCore;
using LevelEditor.DomNodeAdapters;


namespace LevelEditor
{
    /// <summary>
    /// The Level Editor document is a DOM hierarchy that contains game objects and other data
    /// about a level file, *.lvl. Game objects have user-editable properties and often reference
    /// assets such as 3D models and textures.</summary>
    public class GameDocument : DomDocument, IGameDocument
    {  
        protected override void OnNodeSet()
        {
            base.OnNodeSet();                        
            DomNode.AttributeChanged += DomNode_AttributeChanged;
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += DomNode_ChildRemoved;

            UriChanged += delegate{m_uriChanged = true;};

            m_gameDocumentRegistry = Globals.MEFContainer.GetExportedValue<GameDocumentRegistry>();
        }

        /// <summary>
        /// Gets a string identifying the type of the resource to the end-user</summary>
        public override string Type
        {
            get { return GameEditor.s_info.FileType; }
        }
        
        public void Save(Uri uri, SchemaLoader schemaLoader)
        {
            if (Dirty || m_uriChanged)
            {                
                string filePath = uri.LocalPath;
                FileMode fileMode = File.Exists(filePath) ? FileMode.Truncate : FileMode.OpenOrCreate;
                using (FileStream stream = new FileStream(filePath, fileMode))
                {
                    var writer = new CustomDomXmlWriter(Globals.ResourceRoot, schemaLoader.TypeCollection);
                    writer.Write(DomNode, stream, uri);
                }
                m_uriChanged = false;
            }
            
            // save all the game-references
            foreach (var gameRef in GetChildList<GameReference>(Schema.gameType.gameReferenceChild))
            {
                GameDocument subDoc = Adapters.As<GameDocument>(gameRef.Target);
                if(subDoc == null) continue;
                subDoc.Save(subDoc.Uri, schemaLoader);                
            }

            Dirty = false;
        }

        #region IGameDocument Members

        public IGameObjectFolder RootGameObjectFolder
        {
            get
            {
                return GetChild<IGameObjectFolder>(Schema.gameType.gameObjectFolderChild);
            }
        }

        public bool IsMasterGameDocument
        {
            get { return DomNode == m_gameDocumentRegistry.MasterDocument.As<GameDocument>().DomNode; }
        }

        public IEnumerable<IReference<IGameDocument>> GameDocumentReferences
        {
            get { return GetChildList<IReference<IGameDocument>>(Schema.gameType.gameReferenceChild); }
        }
                        
        public event EventHandler<ItemChangedEventArgs<IEditableResourceOwner>> EditableResourceOwnerDirtyChanged = delegate{};

        public void NotifyEditableResourceOwnerDirtyChanged(IEditableResourceOwner resOwner)
        {
            EditableResourceOwnerDirtyChanged(this, new ItemChangedEventArgs<IEditableResourceOwner>(resOwner));
        }

        #endregion

        
        /// <summary>
        /// Should the sub-documents be resolved on load</summary>                
        public static bool ResolveOnLoad
        {
            get;
            set;
        }

        /// <summary>
        /// Open or create new document.
        /// It opens if the file exist otherwise it will creates new document        
        /// </summary>        
        public static GameDocument OpenOrCreate(Uri uri, SchemaLoader schemaLoader)
        {
            if (!uri.IsAbsoluteUri)
                return null;

            var docRegistry = Globals.MEFContainer.GetExportedValue<GameDocumentRegistry>();

            GameDocument document = docRegistry.FindDocument(uri) as GameDocument;
            if (document != null)
            {
                return document;
            }

            string filePath = uri.LocalPath;

            DomNode rootNode = null;
            if (File.Exists(filePath))
            {               
                // read existing document using custom dom XML reader
                using (FileStream stream = File.OpenRead(filePath))
                {
                    var reader = new CustomDomXmlReader(Globals.ResourceRoot, schemaLoader);
                    rootNode = reader.Read(stream, uri);
                }
            }
            else
            {
                // create new document by creating a Dom node of the root type defined by the schema                 
                rootNode = new DomNode(Schema.gameType.Type, Schema.gameRootElement);
                rootNode.SetAttribute(Schema.gameType.nameAttribute, "Game".Localize());
            }


            GameObjectFolder rootFolder = Adapters.As<GameObjectFolder>(rootNode.GetChild(Schema.gameType.gameObjectFolderChild));
            if (rootFolder == null)
            {
                // create the game object folder
                rootFolder = (GameObjectFolder)GameObjectFolder.Create();
                rootFolder.Name = "GameObjects".Localize("this is the name of a folder in the project lister");
                rootNode.SetChild(Schema.gameType.gameObjectFolderChild, rootFolder.DomNode);
            }
            

            // create bookmarks 
            DomNode bookmarks = rootNode.GetChild(Schema.gameType.bookmarksChild);
            if (bookmarks == null)
            {
                bookmarks = new DomNode(Schema.bookmarksType.Type);
                rootNode.SetChild(Schema.gameType.bookmarksChild, bookmarks);
            }

            DomNode layersNode = rootNode.GetChild(Schema.gameType.layersChild);
            if (layersNode == null)
            {
                layersNode = new DomNode(Schema.layersType.Type);
                rootNode.SetChild(Schema.gameType.layersChild, layersNode);
            }

            // Create the grid
            DomNode gridNode = rootNode.GetChild(Schema.gameType.gridChild);
            if (gridNode == null)
            {
                gridNode = new DomNode(Schema.gridType.Type);
                rootNode.SetChild(Schema.gameType.gridChild, gridNode);
            }

            document = rootNode.As<GameDocument>();
            document.Uri = uri;

            // Initialize Dom extensions now that the data is complete
            rootNode.InitializeExtensions();
           
            docRegistry.Add(document);


            UniqueNamer uniqueNamer = new UniqueNamer('_');
            foreach (DomNode node in rootNode.Subtree)
            {
                if (node.Type.IdAttribute != null)
                {
                    uniqueNamer.Name(node.GetId());
                }
            }


            // sync all the prefab instances
            DomNode folderNode = document.RootGameObjectFolder.As<DomNode>();
            foreach (DomNode node in folderNode.Subtree)
            {
                PrefabInstance prefab = node.As<PrefabInstance>();
                if (prefab == null) continue;
                prefab.Resolve(uniqueNamer);
            }

            if (ResolveOnLoad)
            {
                // resovle all the game references.
                foreach (var subGame in document.GetChildList<GameReference>(Schema.gameType.gameReferenceChild))
                {
                    subGame.Resolve();
                }
            }

            document.Dirty = false;
            return document;
        }

        

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            Dirty = true;
        }
        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            Dirty = true;
        }
        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            Dirty = true;
            IReference<IGameDocument> gameRef = e.Child.As<IReference<IGameDocument>>();
            if (gameRef != null)
            {
                m_gameDocumentRegistry.Remove(gameRef.Target);
            }
        }

        private GameDocumentRegistry m_gameDocumentRegistry; 
        private bool m_uriChanged;
        
    }
    
    /// <summary>
    /// Id to DomNode map.</summary>    
    public class IdToDomNode : DomNodeAdapter
    {
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            DomNode.AttributeChanging += (sender, e) =>
                {
                    if (e.AttributeInfo.Equivalent(e.DomNode.Type.IdAttribute))
                    {
                        string newId = (string)e.NewValue;
                        if (string.IsNullOrWhiteSpace(newId))
                            throw new InvalidTransactionException("Id cannot be null, empty, or whitespace");                        
                    }

                };
        }

        
        /// <summary>
        /// Find DomNode by id.
        /// return DomNode or null if not found.
        /// </summary>        
        public DomNode FindById(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException("id");
            
            foreach (DomNode node in DomNode.Subtree)
            {
                if (node.Type.IdAttribute != null)
                {
                    if (id == node.GetId())
                    {
                        return node;
                    }                        
                }
            }
            return null;
        }        
    }   
}
