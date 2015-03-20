//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using Sce.Atf;

using LevelEditorCore;

using LevelEditor.DomNodeAdapters;

namespace LevelEditor
{
    [Export(typeof(IResourceConverter))]    
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ResourceConverter : IResourceConverter
    {
        #region IResourceConverter Members

        IGameObject IResourceConverter.Convert(IResource resource)
        {
            if (resource == null) return null;

            IGameObject gob = null;
            if (resource.Type == ResourceTypes.Model)
            {
                Locator locator = Locator.Create();
                IReference<IResource> resRef = ResourceReference.Create(resource);                
                locator.Reference = resRef;
                locator.DomNode.InitializeExtensions();
                gob = locator;                
            }           
            return gob;
        }

        #endregion
    }
}
