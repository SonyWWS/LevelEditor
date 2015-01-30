//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

using LevelEditorCore;


namespace LevelEditor.DomNodeAdapters
{
    /// <summary>
    /// Additional properties for GameObjects</summary>
    /// <remarks>The property grid lists these properties in addition to the ones 
    /// defined in the schema annotation</remarks>
    public class GameObjectProperties : CustomTypeDescriptorNodeAdapter
    {
        /// <summary>
        /// Creates an array of property descriptors that are associated with the adapted DomNode's
        /// DomNodeType. No duplicates will be in the array (based on the property descriptor's Name
        /// property).</summary>
        /// <returns>Array of property descriptors</returns>
        protected override System.ComponentModel.PropertyDescriptor[] GetPropertyDescriptors()
        {
            // Initialize property desciptors with the ones from the base class
            // If this is not done, the new property descriptors would be used instead of
            // rather than in addition to the ones defined in the schema
            List<System.ComponentModel.PropertyDescriptor> descriptors =
                new List<System.ComponentModel.PropertyDescriptor>(base.GetPropertyDescriptors());

            // Add ITransformable properties:
            // Translation, Rotation, Scale, RotatePivot, ScalePivot (if supported by this object)
            
            ITransformable node = this.Cast<ITransformable>();
            TransformationTypes transformType = node.TransformationType;

            NumericTupleEditor tupleEditor =
                new NumericTupleEditor(typeof(float), new string[] { "x", "y", "z" });
            NumericTupleEditor rotationTupleEditor =
                new NumericTupleEditor(typeof(float), new string[] { "x", "y", "z" });
            rotationTupleEditor.ScaleFactor = 360 / (2 * Math.PI); // Radians to Degrees
          
            string category = "Transform".Localize();
            // Check for transform types
            if ((transformType & TransformationTypes.Translation) != 0)
                descriptors.Add(
                    new AttributePropertyDescriptor(
                        "Translation", Schema.gameObjectType.translateAttribute, category, "Translation of Game Object along X, Y, and Z axes".Localize(),
                        false, tupleEditor));

            if ((transformType & TransformationTypes.Rotation) != 0)
                descriptors.Add(new AttributePropertyDescriptor(
                        "Rotation".Localize(), Schema.gameObjectType.rotateAttribute, category, 
                        "Origin of Rotation transform relative to Game Object Translation".Localize(),
                        false, rotationTupleEditor));

            if ((transformType & TransformationTypes.Scale) != 0)
            {
                if ((transformType & TransformationTypes.UniformScale) == 0)
                    descriptors.Add(
                        new AttributePropertyDescriptor(
                            "Scale".Localize(), 
                            Schema.gameObjectType.scaleAttribute, category, 
                            "Scale of Game Object along X, Y, and Z axes".Localize(),
                            false, tupleEditor));
                else
                    descriptors.Add(
                        new AttributePropertyDescriptor(
                            "Uniform Scale".Localize(), Schema.gameObjectType.scaleAttribute, 
                            category,
                            "Scale of Game Object uniformly along X, Y, and Z axes".Localize(),
                            false, new UniformArrayEditor<Single>()));
            }

            if ((transformType & TransformationTypes.Pivot) != 0)
                descriptors.Add(
                    new AttributePropertyDescriptor(
                        "Pivot".Localize(), Schema.gameObjectType.pivotAttribute, category,
                        "Origin of Rotation and scale transform relative to Game Object Translation".Localize(),
                        false, tupleEditor));  
           

            // remove hidden properties
            HashSet<string> hiddenProps = (HashSet<string>)this.DomNode.Type.GetTag(SchemaLoader.HiddenProperties);
            if (hiddenProps != null)
            {
                List<PropertyDescriptor> removeList = new List<PropertyDescriptor>();
                foreach (AttributePropertyDescriptor propdescr in descriptors.AsIEnumerable<AttributePropertyDescriptor>())
                {
                    if (hiddenProps.Contains(propdescr.AttributeInfo.Name))
                    {
                        removeList.Add(propdescr);
                    }
                }

                foreach (PropertyDescriptor propDescr in removeList)
                    descriptors.Remove(propDescr);
            }


            return descriptors.ToArray();
        }
    }
}
