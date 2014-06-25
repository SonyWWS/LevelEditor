//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Dom;

namespace LevelEditor
{

    /// <summary>
    /// custom dom xml reader.
    /// for any attribute of type anyuri.
    /// this reader will convert relative uri to absolute 
    /// using either resource-root or resource uri.
    /// </summary>
    internal class CustomDomXmlReader : DomXmlReader
    {
        public CustomDomXmlReader(Uri resourceRoot, XmlSchemaTypeLoader typeLoader)
            : base(typeLoader)
        {
            m_resourceRoot = resourceRoot;
        }

        /// <summary>
        /// Converts the give string to attribute value and set it to given node
        /// using attributeInfo.         
        /// </summary>
        /// <param name="node">DomNode </param>
        /// <param name="attributeInfo">attributeInfo to set</param>
        /// <param name="valueString">The string representation of the attribute value</param>
        protected override void ReadAttribute(DomNode node, AttributeInfo attributeInfo, string valueString)
        {
            if (IsReferenceAttribute(attributeInfo))
            {
                // save reference so it can be resolved after all nodes have been read
                NodeReferences.Add(new XmlNodeReference(node, attributeInfo, valueString));
            }
            else if (!string.IsNullOrEmpty(valueString))
            {
                object value = attributeInfo.Type.Convert(valueString);

                if (value is Uri)
                {
                    //todo reference to objects in other documents must be made absolute using
                    //this Uri instead of resourceRoot.

                    // then convert it to absolute.
                    Uri ur = (Uri)value;
                    if (!ur.IsAbsoluteUri)
                    {
                        // todo use schema annotation to decide what to use 
                        // for converting relative uri to absolute.
                        if (node.Type == Schema.gameReferenceType.Type
                            || node.Type == Schema.gameObjectReferenceType.Type)
                        {
                            string urStr = ur.ToString();
                            int fragIndex = urStr.LastIndexOf('#');
                            if (fragIndex == -1)
                            {
                                value = new Uri(Uri, ur);
                            }
                            else
                            {
                                string frag = urStr.Substring(fragIndex);
                                string path = urStr.Substring(0, fragIndex);
                                Uri absUri = new Uri(Uri, path);
                                value = new Uri(absUri + frag);
                            }

                        }
                        else
                        {
                            value = new Uri(m_resourceRoot, ur);
                        }

                    }
                }
                node.SetAttribute(attributeInfo, value);
            }
        }

        private Uri m_resourceRoot;
    }
}
