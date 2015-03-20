//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;

using Sce.Atf;
using Sce.Atf.Dom;

using LevelEditor.DomNodeAdapters;

using PropertyDescriptor = Sce.Atf.Dom.PropertyDescriptor;

using LevelEditor.Terrain;

namespace LevelEditor
{
    /// <summary>
    /// Registers DomNodeAdapters for all DomNodeTypes</summary>
    public static class GameAdapters
    {
        /// <summary>
        /// Register DomNodeAdapters </summary>        
        public static void Initialize(SchemaLoader schemaLoader)
        {
            // resource meta data             
            Schema.resourceMetadataType.Type.Define(new ExtensionInfo<ResourceMetadataDocument>());
            Schema.textureMetadataType.Type.Define(new ExtensionInfo<ResourceMetadataDocument>());
               
            // game
            Schema.gameType.Type.Define(new ExtensionInfo<Game>());                        
            Schema.gameType.Type.Define(new ExtensionInfo<GameDocument>());
            Schema.gameType.Type.Define(new ExtensionInfo<IdToDomNode>());
            Schema.gameType.Type.Define(new ExtensionInfo<GameContext>());            
            Schema.gameType.Type.Define(new ExtensionInfo<LayeringContext>());            
            Schema.gameType.Type.Define(new ExtensionInfo<BookmarkingContext>());
            Schema.gameType.Type.Define(new ExtensionInfo<CustomLockingValidator>());
            Schema.gameType.Type.Define(new ExtensionInfo<UniqueIdValidator>());

            // grid            
            Schema.gridType.Type.Define(new ExtensionInfo<Grid>());

            // gob            
            Schema.gameObjectType.Type.Define(new ExtensionInfo<GameObject>());
            Schema.gameObjectType.Type.Define(new ExtensionInfo<GameObjectProperties>());
            Schema.gameObjectType.Type.Define(new ExtensionInfo<TransformUpdater>());

            // gob folder
            Schema.gameObjectFolderType.Type.Define(new ExtensionInfo<GameObjectFolder>());

            // gob group
            Schema.gameObjectGroupType.Type.Define(new ExtensionInfo<GameObjectGroup>());

            // prefab
            Schema.objectOverrideType.Type.Define(new ExtensionInfo<ObjectOverride>());
            Schema.attributeOverrideType.Type.Define(new ExtensionInfo<AttributeOverride>());                
            Schema.prefabInstanceType.Type.Define(new ExtensionInfo<PrefabInstance>());
            Schema.prefabType.Type.Define(new ExtensionInfo<Prefab>());
            
            // locator
            Schema.locatorType.Type.Define(new ExtensionInfo<Locator>());

            // curves
            Schema.curveType.Type.Define(new ExtensionInfo<Curve>());            
            Schema.controlPointType.Type.Define(new ExtensionInfo<ControlPoint>());
            Schema.catmullRomType.Type.Define(new ExtensionInfo<CatmullRom>());
            Schema.bezierType.Type.Define(new ExtensionInfo<BezierCurves>());

            // lights            
            Schema.PointLight.Type.Define(new ExtensionInfo<PointLight>());            
            Schema.DirLight.Type.Define(new ExtensionInfo<DirLight>());
            Schema.BoxLight.Type.Define(new ExtensionInfo<BoxLight>());

            // Billboard
            Schema.billboardTestType.Type.Define(new ExtensionInfo<Billboard>());
                                   
            // terrain
            Schema.terrainGobType.Type.Define(new ExtensionInfo<TerrainGob>());
            Schema.layerMapType.Type.Define(new ExtensionInfo<LayerMap>());
            Schema.decorationMapType.Type.Define(new ExtensionInfo<DecorationMap>());
            

            // references
            Schema.gameReferenceType.Type.Define(new ExtensionInfo<GameReference>());            
            Schema.resourceReferenceType.Type.Define(new ExtensionInfo<ResourceReference>());
            Schema.gameObjectReferenceType.Type.Define(new ExtensionInfo<GameObjectReference>());
            

            Schema.layerType.Type.Define(new ExtensionInfo<Layer>());                                    
            Schema.bookmarkType.Type.Define(new ExtensionInfo<Bookmark>());


            string category = "Grid".Localize();
            Schema.gameType.Type.SetTag(
                  new PropertyDescriptorCollection(
                      new PropertyDescriptor[]
                    {
                        new ChildAttributePropertyDescriptor(
                            "Height".Localize(),
                            Schema.gridType.heightAttribute,
                            Schema.gameType.gridChild,
                            category,
                            "Grid's height (along the world's up vector)".Localize(),
                            false),
                        new ChildAttributePropertyDescriptor(
                            "Size".Localize(),
                            Schema.gridType.sizeAttribute,
                            Schema.gameType.gridChild,
                            category,
                            "the size of grid step".Localize(),
                            false),
                        new ChildAttributePropertyDescriptor(
                            "Subdivisions".Localize(),
                            Schema.gridType.subdivisionsAttribute,
                            Schema.gameType.gridChild,
                            category,
                            "Number of sub-divisions".Localize(),
                            false),
                        
                        new ChildAttributePropertyDescriptor(
                            "snap".Localize(),
                            Schema.gridType.snapAttribute,
                            Schema.gameType.gridChild,
                            category,
                            "Snap to grid vertex".Localize(),
                            false),

                        new ChildAttributePropertyDescriptor(
                            "Visible".Localize(),
                            Schema.gridType.visibleAttribute,
                            Schema.gameType.gridChild,
                            category,
                            "Grid visibility".Localize(),
                            false)
                    }));
        }
    }
}
