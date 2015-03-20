//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml;
using System.Reflection;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

using LevelEditorCore;

namespace LevelEditor
{
    /// <summary>
    /// Editor for GameDocuments</summary>
    [Export(typeof(GameEditor))]
    [Export(typeof(IInitializable))]
    [Export(typeof(IDocumentClient))]
    [Export(typeof(IPaletteClient))]        
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class GameEditor :
        IDocumentClient,
        IInitializable,
        IPaletteClient,        
        IWindowLayoutClient
    {
        [ImportingConstructor]
        public GameEditor(
            IContextRegistry contextRegistry,
            IDocumentRegistry documentRegistry,            
            IControlHostService controlHostService,
            ICommandService commandService,
            IDocumentService documentService,
            IPaletteService paletteService,
            ISettingsService settingsService,            
            IResourceService resourceService,
            LevelEditorCore.ResourceLister resourceLister,            
            BookmarkLister bookmarkLister
            )
        {
            m_contextRegistry = contextRegistry;
            m_documentRegistry = documentRegistry;
            m_paletteService = paletteService;
            m_settingsService = settingsService;            
            m_documentService = documentService;            
            m_resourceService = resourceService;
            m_resourceLister = resourceLister;            
            m_bookmarkLister = bookmarkLister;
            
            //to-do wire it to to command service
            InputScheme.ActiveControlScheme = new MayaControlScheme();
            ResolveOnLoad = true;
        }

        

        #region IInitializable Members

        public void Initialize()
        {
            RegisterSettings();

            // Register palette items based on the schema file's annotations.
            foreach (DomNodeType nodeType in m_schemaLoader.TypeCollection.GetNodeTypes())
            {
                NodeTypePaletteItem item = nodeType.GetTagLocal<NodeTypePaletteItem>();
                if (item != null)
                {
                    Console.WriteLine("reg: " + nodeType.Name);
                    m_paletteService.AddItem(nodeType, item.Category, this);
                }
            }
            
            m_mainWindow.Loaded += delegate
            {
                if (ResourceRoot == null)
                    ResourceRoot = Application.StartupPath + "\\";
            };


            if (m_scriptingService != null)
            {                
                
                // load this assembly into script domain.
                m_scriptingService.LoadAssembly(GetType().Assembly);
                m_scriptingService.LoadAssembly(Assembly.GetAssembly(typeof(LevelEditorCore.IDesignView)));
                m_scriptingService.LoadAssembly(Assembly.GetAssembly(typeof(RenderingInterop.RenderCommands)));
                m_scriptingService.ImportAllTypes("LevelEditor");
                m_scriptingService.ImportAllTypes("LevelEditor.DomNodeAdapters");
                m_scriptingService.ImportAllTypes("LevelEditorCore");
                m_scriptingService.ImportAllTypes("Sce.Atf.Controls.Adaptable.Graphs");
                m_scriptingService.ImportAllTypes("RenderingInterop");

                m_scriptingService.SetVariable("editor", this);
                m_scriptingService.SetVariable("bookmarkLister", m_bookmarkLister);

                m_contextRegistry.ActiveContextChanged += delegate
                {
                    EditingContext editingContext = m_contextRegistry.GetActiveContext<EditingContext>();
                    IHistoryContext hist = m_contextRegistry.GetActiveContext<IHistoryContext>();
                    m_scriptingService.SetVariable("editingContext", editingContext);
                    m_scriptingService.SetVariable("hist", hist);
                };
            }
        }

        #endregion

        #region IDocumentClient Members

        /// <summary>
        /// Gets editor's information</summary>
        public DocumentClientInfo Info
        {
            get { return s_info; }
        }

        public bool CanOpen(Uri uri)
        {
            return s_info.IsCompatibleUri(uri);
        }

        public IDocument Open(Uri uri)
        {            
            IGameDocument activeDoc = m_documentRegistry.GetActiveDocument<IGameDocument>();

            if (activeDoc != null)
            {
                if (!m_documentService.Close(activeDoc))
                    return null;              
            }

            List<IResource> resources = new List<IResource>(m_resourceService.Resources);
            foreach (var res in resources)
            {
                m_resourceService.Unload(res.Uri);
            }
            
            GameDocument document = GameDocument.OpenOrCreate(uri, m_schemaLoader);            
            m_contextRegistry.ActiveContext = document.As<GameContext>();
            return document;                          
        }

        public void Show(IDocument document)
        {           
        }

        public void Save(IDocument document, Uri uri)
        {            
            GameDocument gameDocument = document as GameDocument;
            gameDocument.Save(uri, m_schemaLoader);

            // save external resources.
            foreach (var obj in m_gameDocumentRegistry.FindAll<IEditableResourceOwner>())
            {
                if (obj.Dirty)
                    obj.Save();                
            }

            m_projLister.Refresh();
        }

        public void Close(IDocument document)
        {
            GameDocument gameDocument = document as GameDocument;                        
            if (gameDocument != null)
            {                
                m_gameDocumentRegistry.Remove((IGameDocument)document);                
                List<object> removeList = new List<object>(m_contextRegistry.Contexts);
                foreach (object cnt in removeList)
                    m_contextRegistry.RemoveContext(cnt);
                m_documentRegistry.Remove(document);
            }
        }

        #endregion
     
        #region IPaletteClient Members

        void IPaletteClient.GetInfo(object item, ItemInfo info)
        {
            DomNodeType nodeType = item as DomNodeType;
            if (nodeType != null)
            {
                NodeTypePaletteItem paletteItem = nodeType.GetTag<NodeTypePaletteItem>();
                if (paletteItem != null)
                {
                    info.Label = paletteItem.Name;
                    info.Description = paletteItem.Description;
                    info.ImageIndex = info.GetImageList().Images.IndexOfKey(paletteItem.ImageName);
                }

                if (info.ImageIndex == -1)
                    info.ImageIndex = Util.GetTypeImageIndex(nodeType, info.GetImageList());
            }
        }

        object IPaletteClient.Convert(object item)
        {
            DomNodeType nodeType = (DomNodeType)item;
            DomNode node = new DomNode(nodeType);

            NodeTypePaletteItem paletteItem = nodeType.GetTag<NodeTypePaletteItem>();
            AttributeInfo idAttribute = nodeType.IdAttribute;
            if (paletteItem != null &&
                idAttribute != null)
            {
                node.SetAttribute(idAttribute, paletteItem.Name);
            }
            return node;            
        }

        #endregion
            
        #region IWindowLayoutClient Members

        public object LayoutData
        {
            get
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "utf-8", "yes"));
                XmlElement root = xmlDoc.CreateElement(SettingsWlcDocumentElementName);
                xmlDoc.AppendChild(root);

                try
                {
                    // Create an XmlElement for each open document that
                    // exists on disk and store specific information
                    // relating to that open document's QuadPanelControl.

                    //foreach (var document in m_documentRegistry.Documents)
                    //{
                    //    var context = document.As<GameContext>();
                    //    if (context == null)
                    //        continue;

                    //    if (!File.Exists(document.Uri.LocalPath))
                    //        continue;

                    //    if (context.DesignView == null)
                    //        continue;

                    //    if (context.DesignView.ActiveViewControl == null)
                    //        continue;

                    //    var quadPanel = context.DesignView.ActiveViewControl.As<QuadPanelControl>();
                    //    if (quadPanel == null)
                    //        continue;

                    //    XmlElement elem = xmlDoc.CreateElement(SettingsWlcDocumentSettingsElementName);
                    //    elem.SetAttribute(SettingsWlcPathAttributeName, document.Uri.ToString());
                    //    elem.InnerXml = FixupXmlData(quadPanel.PersistedSettings);

                    //    root.AppendChild(elem);
                    //}

                    //if (xmlDoc.DocumentElement == null)
                    //    xmlDoc.RemoveAll();
                }
                catch (Exception ex)
                {
                    Outputs.WriteLine(
                        OutputMessageType.Error,
                        "{0}: Exception saving layout persisted settings: {1}", this, ex.Message);

                    xmlDoc.RemoveAll();
                }

                return xmlDoc.InnerXml.Trim();
            }

            set
            {
                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(value.ToString());

                    if (xmlDoc.DocumentElement == null)
                        return;

                    //foreach (XmlElement elem in xmlDoc.DocumentElement.ChildNodes)
                    //{
                    //    if (string.Compare(elem.Name, SettingsWlcDocumentSettingsElementName) != 0)
                    //        continue;

                    //    string path = elem.GetAttribute(SettingsWlcPathAttributeName);
                    //    if (string.IsNullOrEmpty(path))
                    //        continue;

                    //    var uri = new Uri(path);
                    //    string xml = elem.InnerXml;

                    //    foreach (var document in m_documentRegistry.Documents)
                    //    {
                    //        var context = document.As<GameContext>();
                    //        if (context == null)
                    //            continue;

                    //        if (string.Compare(uri.LocalPath, document.Uri.LocalPath) != 0)
                    //            continue;

                    //        if (context.DesignView == null)
                    //            continue;

                    //        if (context.DesignView.ActiveViewControl == null)
                    //            continue;

                    //        var quadPanel = context.DesignView.ActiveViewControl.As<QuadPanelControl>();
                    //        if (quadPanel == null)
                    //            continue;

                    //        quadPanel.PersistedSettings = xml;
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    Outputs.WriteLine(
                        OutputMessageType.Error,
                        "{0}: Exception loading layout persisted settings: {1}", this, ex.Message);
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets the root resource directory for finding assets</summary>        
        public string ResourceRoot
        {
            get
            {
                if(Globals.ResourceRoot != null)
                    return Globals.ResourceRoot.LocalPath;
                return null;
            }
            set
            {
                if (!value.EndsWith("\\"))
                    value += "\\";

                Uri uri = new Uri(value, UriKind.RelativeOrAbsolute);
                if (!uri.IsAbsoluteUri)
                {
                    Uri startPath = new Uri(Application.StartupPath + "\\");
                    uri = new Uri(startPath, value);
                }

                Globals.ResourceRoot = uri;

                IResourceFolder rootResourceFolder = new CustomFileSystemResourceFolder(Globals.ResourceRoot.LocalPath);                
                if (m_resourceLister != null)
                    m_resourceLister.SetRootFolder(rootResourceFolder);
            }
        }
      
        private void RegisterSettings()
        {
            string descr = "Root path for all resources".Localize();
            var resourceRoot =
                new BoundPropertyDescriptor(this, () => ResourceRoot,
                    "ResourceRoot".Localize("A user preference and the name of the preference in the settings file"),
                    null,
                    descr,
                    new FolderBrowserDialogUITypeEditor(descr), null);
            

            m_settingsService.RegisterSettings(this, resourceRoot);
            m_settingsService.RegisterUserSettings("Resources".Localize(), resourceRoot);

            var resolveOnLoad =
                new BoundPropertyDescriptor(this, () => ResolveOnLoad,
                    "Resolve on load".Localize("A user preference and the name of the preference in the settings file"),
                    null,
                    "Resolve sub-documents on load".Localize());
                    
            string docs = "Documents".Localize();
            m_settingsService.RegisterSettings(docs, resolveOnLoad);
            m_settingsService.RegisterUserSettings(docs, resolveOnLoad);

        }

        
        public bool ResolveOnLoad
        {
            get { return GameDocument.ResolveOnLoad; }
            set { GameDocument.ResolveOnLoad = value; }
        }

        

        // MEF Plugins/Services
        private readonly IContextRegistry m_contextRegistry;
        private readonly IDocumentRegistry m_documentRegistry;
        private readonly IPaletteService m_paletteService;
        private readonly ISettingsService m_settingsService;        
        private readonly IDocumentService m_documentService;        
        private readonly IResourceService m_resourceService = null;
        private readonly LevelEditorCore.ResourceLister m_resourceLister = null;        
        private readonly BookmarkLister m_bookmarkLister = null;
        private const string SettingsWlcDocumentElementName = "GameEditorSettings";
        

        // scripting related members
        [Import(AllowDefault = true)]
        private ScriptingService m_scriptingService = null;

        [Import(AllowDefault = false)]
        private GameProjectLister m_projLister = null;

        [Import(AllowDefault = false)]
        private GameDocumentRegistry m_gameDocumentRegistry = null;

        [Import(AllowDefault = false)]
        private SchemaLoader m_schemaLoader = null;

        [Import(AllowDefault = false)]
        private IMainWindow m_mainWindow = null;

        public static readonly DocumentClientInfo s_info = new DocumentClientInfo(
            "Game",
            new string[] { ".lvl" },
            Sce.Atf.Resources.DocumentImage,
            Sce.Atf.Resources.FolderImage,
            false);
    }

}
