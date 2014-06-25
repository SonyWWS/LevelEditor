//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace LevelEditorCore
{
    /// <summary>
    /// Provides basic schema information.    
    /// </summary>
    public interface ISchemaLoader
    {
        /// <summary>
        /// Gets namespace of the root element</summary>
        string NameSpace { get; }

        /// <summary>
        /// Gets type collection of the root element </summary>
        XmlSchemaTypeCollection TypeCollection { get; }

        /// <summary>
        /// Gets DomNodeType of the root element's DomNode.</summary>
        DomNodeType GameType { get; }

        /// <summary>
        /// Gets DomNodeType of GameObject.</summary>
        DomNodeType GameObjectType { get; }


        /// <summary>
        /// Gets DomNodeType of ResourceReference.</summary>
        DomNodeType ResourceReferenceType { get; }

        /// <summary>
        /// Gets DomNodeType of GameReference.</summary>
        DomNodeType GameReferenceType { get; }

        /// <summary>
        /// Gets DomNodeType of GameObjectReference.</summary>
        DomNodeType GameObjectReferenceType { get; }

        /// <summary>
        /// Gets DomNodeType of GameObjectGroup.</summary>
        DomNodeType GameObjectGroupType { get; }

        /// <summary>
        /// Gets DomNodeType of GameObjectFolder.</summary>
        DomNodeType GameObjectFolderType { get; }
        
        /// <summary>
        /// Gets root element.</summary>
        ChildInfo GameRootElement { get; }
    }
}
