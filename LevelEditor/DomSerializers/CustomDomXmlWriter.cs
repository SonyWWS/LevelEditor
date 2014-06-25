//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Xml;
using Sce.Atf.Dom;

// custom dom xml reader and writer.
namespace LevelEditor
{
    internal class CustomDomXmlWriter : DomXmlWriter
    {
        public CustomDomXmlWriter(Uri resourceRoot, XmlSchemaTypeCollection typeCollection)
            : base(typeCollection)
        {
            m_resourceRoot = resourceRoot;
            PreserveSimpleElements = true;
            PersistDefaultAttributes = true;
        }

        /// <summary>
        /// Converts attribute to string.
        /// WriteAttributes(..) call this method to convert dom attribute to string
        /// before writing.        
        /// </summary>
        /// <param name="node">DomNode that owns the attribute to be converted</param>
        /// <param name="attributeInfo">The attribute that need to be converted</param>
        /// <returns>the string value of the attribute</returns>
        protected override string Convert(DomNode node, AttributeInfo attributeInfo)
        {
            // if attribute is uri, then convert it to relative if it is absolute.

            string valueString = string.Empty;
            object value = node.GetAttribute(attributeInfo);
            if (value == null) return valueString;

            if (attributeInfo.Type.Type == AttributeTypes.Reference)
            {
                // if reference is a valid node, convert to string
                DomNode refNode = value as DomNode;
                if (refNode != null)
                    valueString = GetNodeReferenceString(refNode, Root, Uri);
            }
            else if (attributeInfo.Type.Type == AttributeTypes.Uri)
            {
                Uri ur = (Uri)value;
                if (ur.IsAbsoluteUri)
                {
                    // todo use schema annotation to choose between resource root and this uri                    
                    if (node.Type == Schema.gameReferenceType.Type
                        || node.Type == Schema.gameObjectReferenceType.Type)
                    {// use this Uri to make it relative.
                        ur = Uri.MakeRelativeUri(ur);
                    }
                    else
                    {// use resource root to make it relative
                        ur = m_resourceRoot.MakeRelativeUri(ur);
                    }

                    ur = new Uri(Uri.UnescapeDataString(ur.ToString()), UriKind.Relative);
                    valueString = ur.ToString();
                }
            }
            else
            {
                valueString = attributeInfo.Type.Convert(value);
            }

            return valueString;
        }


        /// <summary>
        /// Writes the element corresponding to the node</summary>
        /// <param name="node">DomNode to write</param>
        /// <param name="writer">The XML writer. See <see cref="T:System.Xml.XmlWriter"/></param>
        protected override void WriteElement(DomNode node, XmlWriter writer)
        {
            if (node.Type == Schema.prefabInstanceType.Type)
            {
                WriteStartElement(node, writer);
                WriteAttributes(node, writer);
                foreach (DomNode child in node.GetChildList(Schema.prefabInstanceType.objectOverrideChild))
                    WriteElement(child, writer);
                writer.WriteEndElement();
            }
            else
            {
                base.WriteElement(node, writer);
            }
        }

        private Uri m_resourceRoot;
    }
}
