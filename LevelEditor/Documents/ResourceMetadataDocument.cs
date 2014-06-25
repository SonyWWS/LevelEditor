//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;

using Sce.Atf.Dom;

using LevelEditorCore;

namespace LevelEditor.DomNodeAdapters
{
    public class ResourceMetadataDocument : DomDocument
    {
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += DomNode_AttributeChanged;
        }


        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {            
            SchemaLoader schemaTypeLoader = Globals.MEFContainer.GetExportedValue<SchemaLoader>();
            string filePath = Uri.LocalPath;
            FileMode fileMode = File.Exists(filePath) ? FileMode.Truncate : FileMode.OpenOrCreate;
            using (FileStream stream = new FileStream(filePath, fileMode))
            {
                var writer = new DomXmlWriter(schemaTypeLoader.TypeCollection);
                writer.Write(DomNode, stream, Uri);
            }            
        }
    }
}
