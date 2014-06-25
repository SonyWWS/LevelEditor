//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Forms;

namespace LevelEditorCore
{
    /// <summary>
    /// Interface for filtering picked object.
    /// It is used by PickFilterService.
    ///  </summary>
    public interface IPickFilter
    {

        /// <summary>
        /// Gets name of this pick filter.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// filters the given object.
        /// Tests the lineage starting from the given object and returns first object passes the test
        ///  Or null. </summary>    
        /// <param name="obj">object to filter</param>    
        /// <param name="e">mouse event arg</param>
        object Filter(object obj, MouseEventArgs e);
    }
}
