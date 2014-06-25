//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf;

namespace LevelEditorCore
{

    /// <summary>
    /// Converts resource to GameObject using imported ResourceConverters</summary>
    [Export(typeof(ResourceConverterService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ResourceConverterService
    {

        /// <summary>
        /// Converts resource to gameObject.
        /// </summary>
        /// <param name="resource">resource to be converted</param>
        /// <returns>GameObject or null if convertion failed</returns>
        public IGameObject Convert(IResource resource)
        {
            IGameObject gob = null;
            if (resource != null)
            {
                foreach (var converter in m_resConverters)
                {
                    gob = converter.Convert(resource);
                    if (gob != null)
                        break;
                    
                }
            }
            return gob;
        }

        [ImportMany]
        private IEnumerable<IResourceConverter> m_resConverters = null;
    }
}
