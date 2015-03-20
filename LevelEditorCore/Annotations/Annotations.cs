//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Xml;

using Sce.Atf.Dom;

namespace LevelEditorCore
{
    /// <summary>
    /// Schema annotations</summary>
    public static class Annotations
    {
       
        /// <summary>
        /// This annotation can be applied to any schema type derived from resourceReferenceType.
        /// To constrain type of resource that this can reference.</summary>
        public static class ReferenceConstraint
        {
            // name of the annotation.
            public const string Name = "sce.atf.referenceConstraint";

            // attributes
            public const string ValidResourceFileExts = "validResourceFileExts";
            public const string ResourceType = "resourceType";
        }


        /// <summary>
        /// Gets all the annotation with given name for 
        /// the given type all the way to the root.
        /// <remarks> If name is null or empty then get all the annotations.</remarks>
        /// </summary>                
        public static IEnumerable<XmlElement> GetAllAnnotation(DomNodeType type, string name)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            var list = new List<XmlElement>();
            DomNodeType nodetype = type;
            while(nodetype != null)
            {
                IEnumerable<XmlNode> annotations = nodetype.GetTagLocal<IEnumerable<XmlNode>>();
                if (annotations != null)
                {
                    foreach (var annot in annotations)
                    {
                        if (string.IsNullOrWhiteSpace(name) || annot.LocalName == name)
                            list.Add((XmlElement)annot);
                    }
                }
                nodetype = nodetype.BaseType;
            }
            return list;
        }
        /// <summary>
        /// Gets the Xml node with the given local name, searching the type
        /// and all of its base types</summary>
        /// <param name="type">DomNodeType whose Tag contains an IEnumerable of XmlNodes</param>
        /// <param name="name">Node's local name</param>
        /// <returns>Xml node with the given local name, or null</returns>
        public static XmlNode FindLocalAnnotation(DomNodeType type, string name)
        {
            IEnumerable<XmlNode> annotations = type.GetTagLocal<IEnumerable<XmlNode>>();
            if (annotations != null)
            {
                foreach (XmlNode xmlNode in annotations)
                {
                    if (xmlNode.LocalName == name)
                        return xmlNode;
                }
            }
            return null;
        }            
    }
}
