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

namespace LevelEditor
{
    [Export(typeof(IInitializable))]
    [Export(typeof(IResourceResolver))]
    [Export(typeof(IResourceConverter))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PrototypingService : IResourceConverter, IResourceResolver, ICommandClient, IInitializable
    {

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(
                Commands.CreatePrototype,
                StandardMenu.File,
                StandardCommandGroup.FileSave,
                "Create Prototype ...".Localize(),
                "Create prototype from selected GameObjects".Localize(),
                Sce.Atf.Input.Keys.None,
                null,
                CommandVisibility.Menu, this);

            if (m_scriptingService != null)
                m_scriptingService.SetVariable("protoService", this);
        }

        #endregion

        #region ICommandClient Members

        bool ICommandClient.CanDoCommand(object commandTag)
        {

            bool cando = false;
            if (commandTag.Equals(Commands.CreatePrototype))
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
            if (!commandTag.Equals(Commands.CreatePrototype))
                return;

            string filePath = Util.GetFilePath(m_fileFilter,Globals.ResourceRoot.LocalPath,true);
            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    // save selected gameobject to a new prototype file.
                    Uri ur = new Uri(filePath);
                    DomNode prototype = CreatePrototype(SelectedGobs);

                    string filePathLocal = ur.LocalPath;
                    FileMode fileMode = File.Exists(filePathLocal) ? FileMode.Truncate : FileMode.OpenOrCreate;
                    using (FileStream stream = new FileStream(filePathLocal, fileMode))
                    {
                        var writer = new CustomDomXmlWriter(Globals.ResourceRoot, m_schemaLoader.TypeCollection);
                        writer.Write(prototype, stream, ur);
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
                        resource = new Prototype(node, uri);
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

        #region IResourceConverter Members

        IGameObject IResourceConverter.Convert(IResource resource)
        {
            IPrototype prototype = resource as IPrototype;
            return (prototype != null) ? prototype.CreateInstance() : null;
        }

        #endregion

        private DomNode CreatePrototype(IEnumerable<IGameObject> gobs)
        {
            DomNode[] originals = new DomNode[1];

            List<IGameObject> copyList = new List<IGameObject>();
            AABB bound = new AABB();
            foreach (IGameObject gameObject in SelectedGobs)
            {
                IBoundable boundable = gameObject.As<IBoundable>();
                bound.Extend(boundable.BoundingBox);
                Matrix4F world = TransformUtils.ComputeWorldTransform(gameObject);
                originals[0] = gameObject.As<DomNode>();
                DomNode[] copies = DomNode.Copy(originals);
                IGameObject copy = copies[0].As<IGameObject>();
                TransformUtils.SetTransform(copy, world);
                copyList.Add(copy);
            }

            DomNode gobchild = null;
            if (copyList.Count > 1)
            {// create group
                IGame game = m_contextRegistry.GetActiveContext<IGame>();
                IGameObjectGroup gobgroup = game.CreateGameObjectGroup();
                gobgroup.Translation = bound.Center;
                gobgroup.UpdateTransform();
                Matrix4F worldInv = new Matrix4F();
                worldInv.Invert(gobgroup.Transform);
                foreach (IGameObject gob in copyList)
                {
                    Vec3F translate = gob.Translation;
                    worldInv.Transform(ref translate);
                    gob.Translation = translate;
                    gob.UpdateTransform();
                    gobgroup.GameObjects.Add(gob);
                }
                gobchild = gobgroup.As<DomNode>();                
            }
            else
            {
                gobchild = copyList[0].As<DomNode>();                
            }

            gobchild.InitializeExtensions();
            gobchild.As<IGameObject>().Translation = new Vec3F(0, 0, 0);

            DomNode prototype = null;
            if (gobchild != null)
            {
                prototype = new DomNode(Schema.prototypeType.Type, Schema.prototypeRootElement);
                prototype.SetChild(Schema.prototypeType.gameObjectChild, gobchild);
            }
            return prototype;
        }

        private IEnumerable<IGameObject> SelectedGobs
        {
            get
            {
                object context = m_contextRegistry.GetActiveContext<IGameContext>();
                ISelectionContext selectionContext = (ISelectionContext)context;
                IEnumerable<DomNode> rootDomNodes = selectionContext != null ?
                         DomNode.GetRoots(selectionContext.GetSelection<DomNode>()) : EmptyArray<DomNode>.Instance;
                return rootDomNodes.AsIEnumerable<IGameObject>();
            }
        }

        private enum Commands
        {
            CreatePrototype,
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
        private const string m_ext = ".ptp";
        private string m_fileFilter = string.Format("Prototype (*{0})|*{0}", m_ext);

        private class Prototype : IPrototype
        {
            public Prototype(DomNode node, Uri ur)
            {
                m_node = node;
                m_uri = ur;
                IGameObject gob = m_node.GetChild(Schema.prototypeType.gameObjectChild).As<IGameObject>();
                gob.Name = Type + "_" + Path.GetFileNameWithoutExtension(ur.LocalPath);
            }

            #region IPrototype Members

            public IGameObject CreateInstance()
            {
                DomNode[] org = { m_node.GetChild(Schema.prototypeType.gameObjectChild) };
                DomNode copy = DomNode.Copy(org)[0];
                copy.InitializeExtensions();
                return copy.As<IGameObject>();
            }

            #endregion

            #region IResource Members

            public string Type
            {
                get { return "Prototype"; }
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
            private DomNode m_node;
        }
    }
}
