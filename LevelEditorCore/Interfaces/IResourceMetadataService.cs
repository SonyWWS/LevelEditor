//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace LevelEditorCore
{
    /// <summary>
    /// Interface to get resource meta data.
    /// It is used by ResourceMetadataEditor.
    /// </summary>
    public interface IResourceMetadataService
    {        
        /// <summary>
        /// Gets extensions for files of type resource meta data.</summary>
        IEnumerable<string> MetadataFileExtensions
        {
            get;
        }

        /// <summary>
        /// Gets resource metadata 
        /// </summary>
        /// <param name="resourceUri">resource uri</param>
        /// <returns>Resource metadata or null</returns>
        IEnumerable<object> GetMetadata(IEnumerable<Uri> resourceUris);

    }
}
