//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;


using LevelEditorCore;


namespace LevelEditor.PickFilters
{    
    /// <summary>
    /// PickFilter that Only allow 
    /// locator-type to be picked.</summary>
    [Export(typeof (IPickFilter))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class LocatorPickFilter :  IPickFilter
    {             
        #region IPickFilter Members

        public string Name
        {
            get { return "Locators"; }
        }

        public object Filter(object obj,MouseEventArgs e)
        {            
            Path<object> path = obj as Path<object>;
            DomNode node = path != null ? path.Last.As<DomNode>() : Adapters.As<DomNode>(obj);
            return node == null || Schema.locatorType.Type.IsAssignableFrom(node.Type) ? obj : null;            
        }
        #endregion              
    }


    /// <summary>
    /// PickFilter that only allow basic shapes 
    /// to be picked.</summary>
    [Export(typeof(IPickFilter))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class BasicShapePickFilter : IPickFilter
    {
        #region IPickFilter Members

        public string Name
        {
            get { return "Basic Shapes"; }
        }

        public object Filter(object obj, MouseEventArgs e)
        {
            Path<object> path = obj as Path<object>;
            DomNode node = path != null ? path.Last.As<DomNode>() : Adapters.As<DomNode>(obj);
            return node == null || Schema.shapeTestType.Type.IsAssignableFrom(node.Type) ? obj : null;
        }       
        #endregion       
    }


    /// <summary>
    /// PickFilter that only allow basic shapes 
    /// to be picked.</summary>
    [Export(typeof(IPickFilter))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class NoCubePickFilter : IPickFilter
    {
        #region IPickFilter Members

        public string Name
        {
            get { return "No Cubes"; }
        }

        public object Filter(object obj, MouseEventArgs e)
        {
            Path<object> path = obj as Path<object>;
            DomNode node = path != null ? path.Last.As<DomNode>() : Adapters.As<DomNode>(obj);
            return (node == null || !Schema.cubeTestType.Type.IsAssignableFrom(node.Type)) ? obj : null;
        }
        #endregion
    }
}
