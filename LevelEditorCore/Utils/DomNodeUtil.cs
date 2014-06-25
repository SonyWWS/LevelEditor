//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

namespace LevelEditorCore
{
    /// <summary>
    /// Utility functions for operating on the value of a DomNode</summary>
    public static class DomNodeUtil
    {
        /// <summary>
        /// Gets the DomNode attribute as a Vec3F. The attribute must exist on the DomNode.</summary>
        /// <param name="domNode">DomNode holding the attribute</param>
        /// <param name="attribute">attribute of the DomNode that contains the data</param>
        /// <returns>attribute as a Vec3F</returns>
        public static Vec3F GetVector(DomNode domNode, AttributeInfo attribute)
        {
            return new Vec3F((float[])domNode.GetAttribute(attribute));
        }

      
        /// <summary>
        /// Sets the DomNode Value to the given Vec3F</summary>
        /// <param name="domNode">DomNode holding value</param>
        /// <param name="attribute">attribute of the DomNode that contains the data</param>
        /// <param name="v">Vec3F</param>
        public static void SetVector(DomNode domNode, AttributeInfo attribute, Vec3F v)
        {
            domNode.SetAttribute(attribute, v.ToArray());
        }
    }
}
