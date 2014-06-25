//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf;

namespace LevelEditorCore
{
    public interface ILayer : INameable
    {
        IList<ILayer> Layers { get; }
        IList<IReference<IGameObject>> GameObjectReferences { get; }
    }

    public static class Layers
    {
        public static IEnumerable<IGameObject> GetGameObjects(this ILayer layer)
        {
            foreach (IReference<IGameObject> reference in layer.GameObjectReferences)
                yield return reference.Target;
        }

        public static bool Contains(this ILayer layer, IGameObject gameObject)
        {
            foreach (IReference<IGameObject> reference in layer.GameObjectReferences)
                if (reference.Target == gameObject)
                    return true;
            return false;
        }
    }
}
