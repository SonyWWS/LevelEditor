//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Xml;


using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;

using LevelEditorCore;
using LevelEditor.DomNodeAdapters;

namespace LevelEditor
{
    /// <summary>
    /// Component to provide  resource meta-data.
    /// </summary>
    [Export(typeof(IResourceMetadataService))]
    [Export(typeof(IInitializable))]    
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ResourceMetadataService : IResourceMetadataService, IInitializable
    {

        #region IInitializable Members

        void IInitializable.Initialize()
        {

            // parse resource metadata annotation.

            HashSet<string> metafileExts = new HashSet<string>();            
            char[] sep = { ';' };
            foreach (ChildInfo chInfo in m_schemaLoader.GetRootElements())
            {
                DomNodeType domtype = chInfo.Type;
                IEnumerable<XmlNode> annotations = domtype.GetTag<IEnumerable<XmlNode>>();
                if (annotations == null)
                    continue;
                
                foreach (XmlElement annot in annotations)
                {
                    if (annot.LocalName != "ResourceMetadata")
                        continue;

                    string metaExt = annot.GetAttribute("metadataFileExt").ToLower();
                    metafileExts.Add(metaExt);
                    string[] resExts = annot.GetAttribute("resourceFileExts").ToLower().Split(sep, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string ext in resExts)
                    {
                        ResourceMetadataInfo metadataInfo
                            = new ResourceMetadataInfo(chInfo, metaExt);
                        m_extMap.Add(ext, metadataInfo);
                    }                    
                }                
            }
            m_metadataFileExts = new string[metafileExts.Count];
            metafileExts.CopyTo(m_metadataFileExts);
            
        }

        #endregion

        #region IResourceMetadataService Members

        IEnumerable<string> IResourceMetadataService.MetadataFileExtensions
        {
            get { return m_metadataFileExts; }
        }

        IEnumerable<object> IResourceMetadataService.GetMetadata(IEnumerable<Uri> resourceUris)
        {           
            List<DomNode> rootNodes = new List<DomNode>();
            foreach (Uri resourceUri in resourceUris)
            {
                string reExt = System.IO.Path.GetExtension(resourceUri.LocalPath).ToLower();

                ResourceMetadataInfo resInfo;
                m_extMap.TryGetValue(reExt, out resInfo);
                if(resInfo == null)
                    m_extMap.TryGetValue(".*", out resInfo);

                string metadataFilePath = resourceUri.LocalPath + resInfo.MetadataFileExt;
                Uri metadataUri = new Uri(metadataFilePath);
                DomNode rootNode = null;

                if (File.Exists(metadataFilePath))
                {
                    // read existing metadata
                    using (FileStream stream = File.OpenRead(metadataFilePath))
                    {
                        var reader = new DomXmlReader(m_schemaLoader);
                        rootNode = reader.Read(stream, metadataUri);
                    }
                }
                else
                {
                    rootNode = resInfo.CreateNode();                   
                    rootNode.SetAttribute(Schema.resourceMetadataType.uriAttribute, resourceUri);
                }

                rootNode.InitializeExtensions();
                ResourceMetadataDocument document = rootNode.Cast<ResourceMetadataDocument>();
                document.Uri = metadataUri;
                rootNodes.Add(rootNode);
            }            
            return rootNodes;   
        }
        
        #endregion
       
        private string[] m_metadataFileExts;

        [Import(AllowDefault = false)]
        private SchemaLoader m_schemaLoader = null;

        private Dictionary<string, ResourceMetadataInfo>
            m_extMap = new Dictionary<string, ResourceMetadataInfo>();

        /// <summary>
        /// Resource metadata info
        /// </summary>
        private class ResourceMetadataInfo
        {
            public ResourceMetadataInfo(ChildInfo chInfo, string fileExt)
            {
                if (string.IsNullOrWhiteSpace(fileExt))
                    throw new ArgumentNullException("fileExt");
                if (!fileExt.StartsWith(".", StringComparison.InvariantCultureIgnoreCase))
                    throw new FormatException("File extension must start with '.' example .png");
                   
                NodeType = chInfo.Type;
                Elemnet = chInfo;
                MetadataFileExt = fileExt;
            }

            public DomNode CreateNode()
            {
                return new DomNode(NodeType, Elemnet);
            }
        
            public readonly DomNodeType NodeType;
            public readonly ChildInfo Elemnet;
            public readonly string MetadataFileExt;            
        }

    }
}
