//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace LevelEditorCore
{
    /// <summary>
    /// Interface for entities that can have children inserted into them (e.g. through drag & drop)</summary>
    public interface IHierarchical
    {
        /// <summary>
        /// Returns true if the specified child can be added to the parent, otherwise false</summary>
        /// <param name="child">Child to be added</param>        
        /// <returns>True if the specified child can be added into the parent, otherwise false</returns>        
        bool CanAddChild(object child);

        /// <summary>
        /// Adds the specified child to the parent</summary>
        /// <param name="child">Child to be added</param>        
        /// <returns>true if the child is added otherwise false.</returns>
        bool AddChild(object child);
    }
}
