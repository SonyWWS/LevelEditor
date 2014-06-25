//Sony Computer Entertainment Confidential

using System.Collections.Generic;
using System.Xml;

using Sce.Atf.Dom;

namespace LevelEditorCore
{
    /// <summary>
    /// Standard annotations</summary>
    public static class Annotations
    {
        /// <summary>
        /// Gets shape annotation</summary>
        /// <param name="type">Annotated type</param>
        /// <returns>shape annotation</returns>
        public static string GetShape(DomNodeType type)
        {
            string annotation = FindAnnotation(type, DisplayElement, ShapeAttribute);
            return annotation;
        }

        /// <summary>
        /// Gets width annotation</summary>
        /// <param name="type">Annotated type</param>
        /// <returns>width annotation</returns>
        public static string GetWidth(DomNodeType type)
        {
            string annotation = FindAnnotation(type, DisplayElement, WidthAttribute);
            return annotation;
        }

        /// <summary>
        /// Gets height annotation</summary>
        /// <param name="type">Annotated type</param>
        /// <returns>height annotation</returns>
        public static string GetHeight(DomNodeType type)
        {
            string annotation = FindAnnotation(type, DisplayElement, HeightAttribute);
            return annotation;
        }

        /// <summary>
        /// Gets radius annotation</summary>
        /// <param name="type">Annotated type</param>
        /// <returns>radius annotation</returns>
        public static string GetRadius(DomNodeType type)
        {
            string annotation = FindAnnotation(type, DisplayElement, RadiusAttribute);
            return annotation;
        }

        /// <summary>
        /// Gets translation annotation</summary>
        /// <param name="type">Annotated type</param>
        /// <returns>translation annotation</returns>
        public static string GetTranslation(DomNodeType type)
        {
            string annotation = FindAnnotation(type, DisplayElement, TranslationAttribute);
            return annotation;
        }

        /// <summary>
        /// Gets rotation annotation</summary>
        /// <param name="type">Annotated type</param>
        /// <returns>rotation annotation</returns>
        public static string GetRotation(DomNodeType type)
        {
            string annotation = FindAnnotation(type, DisplayElement, RotationAttribute);
            return annotation;
        }

        /// <summary>
        /// Gets wireframe color annotation</summary>
        /// <param name="type">Annotated type</param>
        /// <returns>wireframe color annotation</returns>
        public static string GetWireColor(DomNodeType type)
        {
            string annotation = FindAnnotation(type, DisplayElement, WireColorAttribute);
            return annotation;
        }

        /// <summary>
        /// Gets smooth-shading color annotation</summary>
        /// <param name="type">Annotated type</param>
        /// <returns>smooth-shading color annotation</returns>
        public static string GetSmoothColor(DomNodeType type)
        {
            string annotation = FindAnnotation(type, DisplayElement, SmoothColorAttribute);
            return annotation;
        }

        /// <summary>
        /// Gets render mode annotation</summary>
        /// <param name="type">Annotated type</param>
        /// <returns>render mode annotation</returns>
        public static string GetRenderMode(DomNodeType type)
        {
            string annotation = FindAnnotation(type, DisplayElement, RenderModeAttribute);
            return annotation;
        }

        /// <summary>
        /// Gets bounding box element name annotation</summary>
        /// <param name="type">Annotated type</param>
        /// <returns>bounding box element name annotation</returns>
        public static string GetBoundingBoxElementName(DomNodeType type)
        {
            string annotation = FindAnnotation(type, DisplayElement, BoundingBoxElementNameAttribute);
            return annotation;
        }

        /// <summary>
        /// Gets billboard texture name annotation</summary>
        /// <param name="type">Annotated type</param>
        /// <returns>billboard texture name annotation</returns>
        public static string GetBillboardTextureName(DomNodeType type)
        {
            string annotation = FindAnnotation(type, DisplayElement, BillboardTextureNameAttribute);
            return annotation;
        }

        public const string DisplayElement = "scea.games.editors.display";
        public const string ElementElement = "scea.games.editors.element";

        // General attributes
        public const string NameAttribute = "name";

        // Rendering attributes
        public const string RenderAttribute = "render";

        // Proxy object attributes
        public const string BoundingBoxElementNameAttribute = "boundingBoxElementName";
        public const string ShapeAttribute = "shape";
        public const string WidthAttribute = "width";
        public const string HeightAttribute = "height";
        public const string RadiusAttribute = "radius";
        public const string RotationAttribute = "rotation";
        public const string TranslationAttribute = "translation";
        public const string WireColorAttribute = "wireColor";
        public const string SmoothColorAttribute = "solidColor";
        public const string RenderModeAttribute = "renderMode";
        public const string BillboardTextureNameAttribute = "billboardTexture";

        /// <summary>
        /// Gets the Xml node with the given local name, searching the type
        /// and all of its base types</summary>
        /// <param name="type">DomNodeType whose Tag contains an IEnumerable of XmlNodes</param>
        /// <param name="name">Node's local name</param>
        /// <returns>Xml node with the given local name, or null</returns>
        public static XmlNode FindAnnotation(this DomNodeType type, string name)
        {
            foreach (XmlNode xmlNode in FindAnnotations(type, name))
            {
                return xmlNode;
            }
            return null;
        }

        /// <summary>
        /// Finds annotation with the given element and attribute, searching the type
        /// and all of its base types</summary>
        /// <param name="type">DomNodeType whose Tag contains an IEnumerable of XmlNodes</param>
        /// <param name="elementName">Annotation element local name</param>
        /// <param name="attributeName">Annotation attribute local name</param>
        /// <returns>Annotation with the given element and attribute, or null</returns>
        public static string FindAnnotation(this DomNodeType type, string elementName, string attributeName)
        {
            foreach (XmlNode xmlNode in FindAnnotations(type, elementName))
            {
                XmlAttribute attribute = xmlNode.Attributes[attributeName];
                if (attribute != null)
                    return attribute.Value;
            }
            return null;
        }

        /// <summary>
        /// Gets the Xml nodes with the given local name, searching the type and all of its base types</summary>
        /// <param name="type">DomNodeType whose Tag contains an IEnumerable of XmlNodes</param>
        /// <param name="elementName">Nodes' local name</param>
        /// <returns>Xml nodes with the given local name, or null</returns>
        public static IEnumerable<XmlNode> FindAnnotations(this DomNodeType type, string elementName)
        {
            Dictionary<string, List<XmlNode>> namesToNodes;
            if (!s_cachedAnnotations.TryGetValue(type, out namesToNodes))
            {
                namesToNodes = new Dictionary<string, List<XmlNode>>();
                s_cachedAnnotations.Add(type, namesToNodes);
            }

            List<XmlNode> cache;
            if (!namesToNodes.TryGetValue(elementName, out cache))
            {
                cache = CreateCachedAnnotations(type, elementName);
                namesToNodes.Add(elementName, cache);
            }
            return cache;
        }

        private static List<XmlNode> CreateCachedAnnotations(DomNodeType derivedType, string elementName)
        {
            var cache = new List<XmlNode>();
            foreach (DomNodeType type in derivedType.Lineage)
            {
                IEnumerable<XmlNode> xmlNodes = type.GetTag<IEnumerable<XmlNode>>();
                if (xmlNodes != null)
                {
                    foreach (XmlNode xmlNode in xmlNodes)
                    {
                        if (xmlNode.LocalName == elementName)
                        {
                            cache.Add(xmlNode);
                        }
                    }
                }
            }
            return cache;
        }

        // For each DomNodeType, we need to keep a mapping of element names to the list of XmlNodes
        //  on that DomNodeType whose LocalName matches the element name.
        private static readonly Dictionary<DomNodeType, Dictionary<string, List<XmlNode>>> s_cachedAnnotations =
            new Dictionary<DomNodeType, Dictionary<string, List<XmlNode>>>();
    }
}
