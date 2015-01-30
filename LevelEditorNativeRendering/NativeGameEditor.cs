//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using LevelEditorCore;


namespace RenderingInterop
{
    /// <summary>
    /// Native Game Editor.</summary>    
    [Export(typeof(IInitializable))]
    [Export(typeof(IControlHostClient))]    
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class NativeGameEditor : IInitializable, IControlHostClient
    {        
        #region IInitializable Members
        void IInitializable.Initialize()
        {           
            m_controlInfo = new ControlInfo("DesignView", "DesignView", StandardControlGroup.CenterPermanent);
            m_controlHostService.RegisterControl(m_designView.HostControl, m_controlInfo, this);
          
            Application.ApplicationExit += delegate
            {            
                Util3D.Shutdown();
                GameEngine.Shutdown();                
            };

            GameEngine.RefreshView += (sender,e)=> m_designView.InvalidateViews();

            m_gameDocumentRegistry.DocumentAdded += m_gameDocumentRegistry_DocumentAdded;
            m_gameDocumentRegistry.DocumentRemoved += m_gameDocumentRegistry_DocumentRemoved;
                
            string ns = m_schemaLoader.NameSpace;

            // register GridRenderer on grid child.
            DomNodeType gridType = m_schemaLoader.TypeCollection.GetNodeType(ns, "gridType");            
            gridType.Define(new ExtensionInfo<GridRenderer>());

            // register NativeGameWorldAdapter on game type.
            m_schemaLoader.GameType.Define(new ExtensionInfo<NativeGameWorldAdapter>());

            // parse schema annotation.
            foreach (DomNodeType domType in m_schemaLoader.TypeCollection.GetNodeTypes())
            {
                IEnumerable<XmlNode> annotations = domType.GetTagLocal<IEnumerable<XmlNode>>();
                if (annotations == null)
                    continue;

                
                // collect all the properties that only exist in native side.
                List<NativeAttributeInfo> nativeAttribs = new List<NativeAttributeInfo>();

                foreach (XmlNode annot in annotations)
                {
                    XmlElement elm = annot as XmlElement;
                    if (elm.LocalName == NativeAnnotations.NativeType)
                    {
                        string typeName = elm.GetAttribute(NativeAnnotations.NativeName);
                        domType.SetTag(NativeAnnotations.NativeType, GameEngine.GetObjectTypeId(typeName));                        
                        if (domType.IsAbstract == false)
                            domType.Define(new ExtensionInfo<NativeObjectAdapter>());                        
                    }
                    else if (elm.LocalName == NativeAnnotations.NativeProperty)
                    {
                        // find a prop name and added to the attribute.
                        
                        
                        string nativePropName = elm.GetAttribute(NativeAnnotations.NativeName);
                        string attribName = elm.GetAttribute(NativeAnnotations.Name);
                        uint typeId = (uint)domType.GetTag(NativeAnnotations.NativeType);
                        uint propId = GameEngine.GetObjectPropertyId(typeId, nativePropName);
                        if(!string.IsNullOrEmpty(attribName))
                        {
                            AttributeInfo attribInfo = domType.GetAttributeInfo(elm.GetAttribute(NativeAnnotations.Name));
                            attribInfo.SetTag(NativeAnnotations.NativeProperty, propId);                            
                        }
                        else
                        {
                            NativeAttributeInfo attribInfo = new NativeAttributeInfo(domType,nativePropName,typeId,propId);
                            nativeAttribs.Add(attribInfo);
                        }
                        
                    }
                    else if (elm.LocalName == NativeAnnotations.NativeElement)
                    {                        
                        ChildInfo info = domType.GetChildInfo(elm.GetAttribute(NativeAnnotations.Name));
                        uint typeId = (uint)domType.GetTag(NativeAnnotations.NativeType);
                        string name = elm.GetAttribute(NativeAnnotations.NativeName);
                        info.SetTag(NativeAnnotations.NativeElement, GameEngine.GetObjectChildListId(typeId, name));
                    }
                }

                if(nativeAttribs.Count > 0)
                {
                    domType.SetTag(nativeAttribs.ToArray());
                }
            }

            
            // register BoundableObject
            m_schemaLoader.GameObjectType.Define(new ExtensionInfo<BoundableObject>());
            m_schemaLoader.GameObjectFolderType.Define(new ExtensionInfo<BoundableObject>());
            
            #region code to handle gameObjectFolder

            {
                // This code is fragile and need to be updated whenever 
                // any relevant part of the schema changes.
                // purpose:
                // gameObjectFolderType does not exist in C++
                // this code will map gameObjectFolderType to gameObjectGroupType.                                    
                DomNodeType gobFolderType = m_schemaLoader.GameObjectFolderType;
                DomNodeType groupType = m_schemaLoader.GameObjectGroupType;

                // map native bound attrib from gameObject to GobFolder
                NativeAttributeInfo[] nativeAttribs = m_schemaLoader.GameObjectType.GetTag<NativeAttributeInfo[]>();
                foreach (var attrib in nativeAttribs)
                {
                    if (attrib.Name == "Bounds")
                    {
                        gobFolderType.SetTag(new NativeAttributeInfo[] {attrib});
                        break;
                    }
                }

                // map type.
                gobFolderType.Define(new ExtensionInfo<NativeObjectAdapter>());
                gobFolderType.SetTag(NativeAnnotations.NativeType, groupType.GetTag(NativeAnnotations.NativeType));

                // map all native attributes of gameObjectGroup to gameFolder
                foreach (AttributeInfo srcAttrib in groupType.Attributes)
                {
                    object nativeIdObject = srcAttrib.GetTag(NativeAnnotations.NativeProperty);
                    if (nativeIdObject == null) continue;
                    AttributeInfo destAttrib = gobFolderType.GetAttributeInfo(srcAttrib.Name);
                    if (destAttrib == null) continue;
                    destAttrib.SetTag(NativeAnnotations.NativeProperty, nativeIdObject);
                    destAttrib.SetTag(NativeAnnotations.MappedAttribute, srcAttrib);
                }

                // map native element from gameObjectGroupType to gameObjectFolderType.
                object gobsId = groupType.GetChildInfo("gameObject").GetTag(NativeAnnotations.NativeElement);
                foreach (ChildInfo srcChildInfo in gobFolderType.Children)
                {
                    if (srcChildInfo.IsList)
                    {
                        srcChildInfo.SetTag(NativeAnnotations.NativeElement, gobsId);
                    }
                }

                m_schemaLoader.GameType.GetChildInfo("gameObjectFolder").SetTag(NativeAnnotations.NativeElement, gobsId);                
            }

            #endregion

        }

        #endregion

        #region IControlHostClient Members

        void IControlHostClient.Activate(Control control)
        {
            if (m_designView.Context != null)
                m_contextRegistry.ActiveContext = m_designView.Context;
        }

        void IControlHostClient.Deactivate(Control control)
        {

        }

        bool IControlHostClient.Close(Control control)
        {            
            if (m_documentRegistry.ActiveDocument != null)
            {
                return m_documentService.Close(m_documentRegistry.ActiveDocument);
            }

            return true;
        }

        #endregion
    
        private void m_gameDocumentRegistry_DocumentAdded(object sender, ItemInsertedEventArgs<IGameDocument> e)
        {
            IGameDocument document = e.Item;
            IGame game = document.Cast<IGame>();
            if (document == m_gameDocumentRegistry.MasterDocument)
            {
                NativeObjectAdapter gameLevel = document.Cast<NativeObjectAdapter>();
                GameEngine.CreateObject(gameLevel);
                GameEngine.SetGameLevel(gameLevel);
                gameLevel.UpdateNativeOjbect();

                //create vertex buffer for grid.
                IGrid grid = document.As<IGame>().Grid;
                GridRenderer gridRender = grid.Cast<GridRenderer>();
                gridRender.CreateVertices();

                m_designView.Context = document.Cast<IGameContext>();                
            }
            DomNode masterNode = m_gameDocumentRegistry.MasterDocument.As<DomNode>();
            DomNode rooFolderNode = game.RootGameObjectFolder.Cast<DomNode>();

            NativeGameWorldAdapter gworld = masterNode.Cast<NativeGameWorldAdapter>();
            gworld.Insert(masterNode, rooFolderNode, masterNode.Type.GetChildInfo("gameObjectFolder"), -1);            
        }

        private void m_gameDocumentRegistry_DocumentRemoved(object sender, ItemRemovedEventArgs<IGameDocument> e)
        {
            IGameDocument document = e.Item;            
            IGame game = document.Cast<IGame>();
            if (document == m_designView.Context.Cast<IGameDocument>())
            {// master document.
                IGrid grid = document.As<IGame>().Grid;
                GridRenderer gridRender = grid.Cast<GridRenderer>();
                gridRender.DeleteVertexBuffer();
                m_designView.Context = null;                
                GameEngine.DestroyObject(game.Cast<NativeObjectAdapter>());
                GameEngine.Clear();
            }
            else
            {// sub document.
                DomNode masterNode = m_gameDocumentRegistry.MasterDocument.As<DomNode>();
                DomNode rooFolderNode = game.RootGameObjectFolder.Cast<DomNode>();
                NativeGameWorldAdapter gworld = masterNode.Cast<NativeGameWorldAdapter>();
                gworld.Remove(masterNode, rooFolderNode, masterNode.Type.GetChildInfo("gameObjectFolder"));
            }
        }
       

        [Import(AllowDefault = false)]
        private IDocumentRegistry m_documentRegistry;

        [Import(AllowDefault = false)]
        private IDocumentService m_documentService;

        [Import(AllowDefault = false)] 
        private IContextRegistry m_contextRegistry;

        [Import(AllowDefault = false)]
        private IControlHostService m_controlHostService;

        [Import(AllowDefault = false)]
        private DesignView m_designView;

        [Import(AllowDefault = false)]
        private ISchemaLoader m_schemaLoader = null;

        [Import(AllowDefault = false)]
        private IGameDocumentRegistry m_gameDocumentRegistry = null;

        private ControlInfo m_controlInfo;
               
    }
}
