//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using System.IO;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

using LevelEditorCore;
using LevelEditorCore.VectorMath;

using LevelEditor.DomNodeAdapters;

namespace LevelEditor
{
    /// <summary>
    /// Prefab service</summary>
    /// <remarks>
    /// There are some common code between this class and 
    /// PrototypingService class need to be factored out</remarks>
    [Export(typeof(IInitializable))]
    [Export(typeof(IResourceResolver))]
    [Export(typeof(IResourceConverter))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PrefabService : IResourceConverter, IResourceResolver, ICommandClient, IInitializable
    {
        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(
               Commands.CreatePrefab,
               StandardMenu.File,
               StandardCommandGroup.FileSave,
               "Create Prefab ...".Localize(),
               "Create prefab from selected GameObjects".Localize(),
               Sce.Atf.Input.Keys.None,
               null,
               CommandVisibility.Menu, this);

            if (m_scriptingService != null)
                m_scriptingService.SetVariable(this.GetType().Name, this);            

        }

        #endregion

        #region IResourceConverter Members

        IGameObject IResourceConverter.Convert(IResource resource)
        {
            Prefab prefab = resource as Prefab;
            if (prefab != null)
            {
                PrefabInstance instance = PrefabInstance.Create(prefab);
                return instance.As<IGameObject>();              
            }
            return null;
        }

        #endregion

        #region IResourceResolver Members

        IResource IResourceResolver.Resolve(Uri uri)
        {
            IResource resource = null;
            try
            {

                string fileName = uri.LocalPath;
                string ext = Path.GetExtension(fileName).ToLower();
                if (ext == m_ext)
                {
                    using (Stream stream = File.OpenRead(fileName))
                    {
                        var reader = new CustomDomXmlReader(Globals.ResourceRoot, m_schemaLoader);
                        DomNode node = reader.Read(stream, uri);                        
                        resource = Prefab.Create(node, uri);
                    }
                }
            }
            catch (System.IO.IOException e)
            {
                Outputs.WriteLine(OutputMessageType.Warning, "Could not load resource: " + e.Message);
            }

            return resource;
        }

        #endregion

        #region ICommandClient Members

        bool ICommandClient.CanDoCommand(object commandTag)
        {
            bool cando = false;
            if (commandTag.Equals(Commands.CreatePrefab))
            {
                foreach (IGameObject gob in SelectedGobs)
                {
                    cando = true;
                    break;
                }
            }
            return cando;
        }

        void ICommandClient.DoCommand(object commandTag)
        {
            if (!commandTag.Equals(Commands.CreatePrefab))
                return;

            string filePath = Util.GetFilePath(m_fileFilter, Globals.ResourceRoot.LocalPath, true);
            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    // save selected gameobject to a new prototype file.
                    Uri ur = new Uri(filePath);
                    DomNode prefab = CreatePrefab(SelectedGobs);

                    string filePathLocal = ur.LocalPath;
                    FileMode fileMode = File.Exists(filePathLocal) ? FileMode.Truncate : FileMode.OpenOrCreate;
                    using (FileStream stream = new FileStream(filePathLocal, fileMode))
                    {
                        var writer = new CustomDomXmlWriter(Globals.ResourceRoot, m_schemaLoader.TypeCollection);
                        writer.Write(prefab, stream, ur);
                    }
                    m_resourceService.Unload(ur);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(m_mainWindow.DialogOwner, ex.Message);
                }
            }
        }

        void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
        {
            
        }

        #endregion

        private IEnumerable<IGameObject> SelectedGobs
        {
            get
            {
                object context = m_contextRegistry.GetActiveContext<IGameContext>();
                ISelectionContext selectionContext = (ISelectionContext)context;
                IEnumerable<DomNode> rootDomNodes = selectionContext != null ?
                         DomNode.GetRoots(selectionContext.GetSelection<DomNode>()) : EmptyArray<DomNode>.Instance;
                foreach (DomNode node in rootDomNodes)
                {
                    PrefabInstance prefabInst = node.As<PrefabInstance>();
                    if (prefabInst != null)
                    {
                        foreach (IGameObject gob in prefabInst.GameObjects)
                            yield return gob;
                    }
                    else
                    {
                        IGameObject gob = node.As<IGameObject>();
                        if(gob != null)
                            yield return gob;

                    }
                }                
            }
        }

        private DomNode CreatePrefab(IEnumerable<IGameObject> gobs)
        {
            UniqueNamer uniqueNamer = new UniqueNamer();
            DomNode[] temp = new DomNode[1];
            List<IGameObject> copyList = new List<IGameObject>();
            AABB bound = new AABB();
            foreach (IGameObject gameObject in SelectedGobs)
            {
                IBoundable boundable = gameObject.As<IBoundable>();
                bound.Extend(boundable.BoundingBox);
                Matrix4F world = TransformUtils.ComputeWorldTransform(gameObject);
                temp[0] = gameObject.As<DomNode>();
                DomNode[] copies = DomNode.Copy(temp);
                copies[0].InitializeExtensions();
                IGameObject copy = copies[0].As<IGameObject>();
                copy.Name = uniqueNamer.Name(copy.Name);
                TransformUtils.SetTransform(copy, world);
                copyList.Add(copy);
            }

            DomNode prefab = new DomNode(Schema.prefabType.Type, Schema.prefabRootElement);
            var list = prefab.GetChildList(Schema.prefabType.gameObjectChild);
            Vec3F center = bound.Center;
            foreach (IGameObject gob in copyList)
            {                
                gob.Translation = gob.Translation - center;
                gob.UpdateTransform(); 
                list.Add(gob.As<DomNode>());
            }            
            return prefab;
        }
        
        [Import(AllowDefault = false)]
        private IMainWindow m_mainWindow = null;

        [Import(AllowDefault = false)]
        private ICommandService m_commandService = null;

        [Import(AllowDefault = false)]
        private IContextRegistry m_contextRegistry = null;

        [Import(AllowDefault = false)]
        private SchemaLoader m_schemaLoader = null;

        [Import(AllowDefault = false)]
        private IResourceService m_resourceService = null;

        [Import(AllowDefault = true)]
        private ScriptingService m_scriptingService = null;

        private enum Commands
        {
            CreatePrefab,
        }

        private const string m_ext = ".prefab";
        private string m_fileFilter = string.Format("Prefab (*{0})|*{0}", m_ext);
        
    }

    public class Prefab : DomNodeAdapter, IPrefab
    {
        public static Prefab Create(DomNode node, Uri ur)
        {
            if (node.Type != Schema.prefabType.Type)
                throw new InvalidOperationException("Invalid node type");
            Prefab prefab = node.As<Prefab>();
            prefab.m_uri = ur;
            prefab.Name = Path.GetFileNameWithoutExtension(ur.LocalPath);
            return prefab;
        }
        public string Name
        {
            get;
            private set;
        }
        protected override void OnNodeSet()
        {
            m_gobs = GetChildList<IGameObject>(Schema.prefabType.gameObjectChild);
        }
        public IEnumerable<IGameObject> GameObjects
        {
            get { return m_gobs; }
        }

        #region IResource Members

        public string Type
        {
            get { return "Prefab"; }
        }

        public Uri Uri
        {
            get { return m_uri; }
            set { throw new InvalidOperationException(); }
        }

        public event EventHandler<UriChangedEventArgs> UriChanged
            = delegate { };

        #endregion

        private Uri m_uri;        
        private IEnumerable<IGameObject> m_gobs;
    }
}
