// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "level_editor.xsd" "Schema.cs" "gap" "LevelEditor"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace LevelEditor
{
    public static class Schema
    {
        public const string NS = "gap";

        public static void Initialize(XmlSchemaTypeCollection typeCollection)
        {
            Initialize((ns,name)=>typeCollection.GetNodeType(ns,name),
                (ns,name)=>typeCollection.GetRootElement(ns,name));
        }

        public static void Initialize(IDictionary<string, XmlSchemaTypeCollection> typeCollections)
        {
            Initialize((ns,name)=>typeCollections[ns].GetNodeType(name),
                (ns,name)=>typeCollections[ns].GetRootElement(name));
        }

        private static void Initialize(Func<string, string, DomNodeType> getNodeType, Func<string, string, ChildInfo> getRootElement)
        {
            gameType.Type = getNodeType("gap", "gameType");
            gameType.nameAttribute = gameType.Type.GetAttributeInfo("name");
            gameType.fogEnabledAttribute = gameType.Type.GetAttributeInfo("fogEnabled");
            gameType.fogColorAttribute = gameType.Type.GetAttributeInfo("fogColor");
            gameType.fogRangeAttribute = gameType.Type.GetAttributeInfo("fogRange");
            gameType.fogDensityAttribute = gameType.Type.GetAttributeInfo("fogDensity");
            gameType.gameObjectFolderChild = gameType.Type.GetChildInfo("gameObjectFolder");
            gameType.layersChild = gameType.Type.GetChildInfo("layers");
            gameType.bookmarksChild = gameType.Type.GetChildInfo("bookmarks");
            gameType.gameReferenceChild = gameType.Type.GetChildInfo("gameReference");
            gameType.gridChild = gameType.Type.GetChildInfo("grid");

            gameObjectFolderType.Type = getNodeType("gap", "gameObjectFolderType");
            gameObjectFolderType.nameAttribute = gameObjectFolderType.Type.GetAttributeInfo("name");
            gameObjectFolderType.visibleAttribute = gameObjectFolderType.Type.GetAttributeInfo("visible");
            gameObjectFolderType.lockedAttribute = gameObjectFolderType.Type.GetAttributeInfo("locked");
            gameObjectFolderType.gameObjectChild = gameObjectFolderType.Type.GetChildInfo("gameObject");
            gameObjectFolderType.folderChild = gameObjectFolderType.Type.GetChildInfo("folder");

            gameObjectType.Type = getNodeType("gap", "gameObjectType");
            gameObjectType.transformAttribute = gameObjectType.Type.GetAttributeInfo("transform");
            gameObjectType.translateAttribute = gameObjectType.Type.GetAttributeInfo("translate");
            gameObjectType.rotateAttribute = gameObjectType.Type.GetAttributeInfo("rotate");
            gameObjectType.scaleAttribute = gameObjectType.Type.GetAttributeInfo("scale");
            gameObjectType.pivotAttribute = gameObjectType.Type.GetAttributeInfo("pivot");
            gameObjectType.nameAttribute = gameObjectType.Type.GetAttributeInfo("name");
            gameObjectType.visibleAttribute = gameObjectType.Type.GetAttributeInfo("visible");
            gameObjectType.lockedAttribute = gameObjectType.Type.GetAttributeInfo("locked");
            gameObjectType.transformationTypeAttribute = gameObjectType.Type.GetAttributeInfo("transformationType");
            gameObjectType.componentChild = gameObjectType.Type.GetChildInfo("component");

            gameObjectComponentType.Type = getNodeType("gap", "gameObjectComponentType");
            gameObjectComponentType.nameAttribute = gameObjectComponentType.Type.GetAttributeInfo("name");
            gameObjectComponentType.activeAttribute = gameObjectComponentType.Type.GetAttributeInfo("active");

            layersType.Type = getNodeType("gap", "layersType");
            layersType.layerChild = layersType.Type.GetChildInfo("layer");

            layerType.Type = getNodeType("gap", "layerType");
            layerType.nameAttribute = layerType.Type.GetAttributeInfo("name");
            layerType.gameObjectReferenceChild = layerType.Type.GetChildInfo("gameObjectReference");
            layerType.layerChild = layerType.Type.GetChildInfo("layer");

            gameObjectReferenceType.Type = getNodeType("gap", "gameObjectReferenceType");
            gameObjectReferenceType.refAttribute = gameObjectReferenceType.Type.GetAttributeInfo("ref");

            bookmarksType.Type = getNodeType("gap", "bookmarksType");
            bookmarksType.bookmarkChild = bookmarksType.Type.GetChildInfo("bookmark");

            bookmarkType.Type = getNodeType("gap", "bookmarkType");
            bookmarkType.nameAttribute = bookmarkType.Type.GetAttributeInfo("name");
            bookmarkType.cameraChild = bookmarkType.Type.GetChildInfo("camera");
            bookmarkType.bookmarkChild = bookmarkType.Type.GetChildInfo("bookmark");

            cameraType.Type = getNodeType("gap", "cameraType");
            cameraType.eyeAttribute = cameraType.Type.GetAttributeInfo("eye");
            cameraType.lookAtPointAttribute = cameraType.Type.GetAttributeInfo("lookAtPoint");
            cameraType.upVectorAttribute = cameraType.Type.GetAttributeInfo("upVector");
            cameraType.viewTypeAttribute = cameraType.Type.GetAttributeInfo("viewType");
            cameraType.yFovAttribute = cameraType.Type.GetAttributeInfo("yFov");
            cameraType.nearZAttribute = cameraType.Type.GetAttributeInfo("nearZ");
            cameraType.farZAttribute = cameraType.Type.GetAttributeInfo("farZ");
            cameraType.focusRadiusAttribute = cameraType.Type.GetAttributeInfo("focusRadius");

            gameReferenceType.Type = getNodeType("gap", "gameReferenceType");
            gameReferenceType.nameAttribute = gameReferenceType.Type.GetAttributeInfo("name");
            gameReferenceType.refAttribute = gameReferenceType.Type.GetAttributeInfo("ref");
            gameReferenceType.tagsAttribute = gameReferenceType.Type.GetAttributeInfo("tags");

            gridType.Type = getNodeType("gap", "gridType");
            gridType.sizeAttribute = gridType.Type.GetAttributeInfo("size");
            gridType.subdivisionsAttribute = gridType.Type.GetAttributeInfo("subdivisions");
            gridType.heightAttribute = gridType.Type.GetAttributeInfo("height");
            gridType.snapAttribute = gridType.Type.GetAttributeInfo("snap");
            gridType.visibleAttribute = gridType.Type.GetAttributeInfo("visible");

            prototypeType.Type = getNodeType("gap", "prototypeType");
            prototypeType.gameObjectChild = prototypeType.Type.GetChildInfo("gameObject");

            prefabType.Type = getNodeType("gap", "prefabType");
            prefabType.gameObjectChild = prefabType.Type.GetChildInfo("gameObject");

            textureMetadataType.Type = getNodeType("gap", "textureMetadataType");
            textureMetadataType.uriAttribute = textureMetadataType.Type.GetAttributeInfo("uri");
            textureMetadataType.keywordsAttribute = textureMetadataType.Type.GetAttributeInfo("keywords");
            textureMetadataType.compressionSettingAttribute = textureMetadataType.Type.GetAttributeInfo("compressionSetting");
            textureMetadataType.memoryLayoutAttribute = textureMetadataType.Type.GetAttributeInfo("memoryLayout");
            textureMetadataType.mipMapAttribute = textureMetadataType.Type.GetAttributeInfo("mipMap");

            resourceMetadataType.Type = getNodeType("gap", "resourceMetadataType");
            resourceMetadataType.uriAttribute = resourceMetadataType.Type.GetAttributeInfo("uri");
            resourceMetadataType.keywordsAttribute = resourceMetadataType.Type.GetAttributeInfo("keywords");

            resourceReferenceType.Type = getNodeType("gap", "resourceReferenceType");
            resourceReferenceType.uriAttribute = resourceReferenceType.Type.GetAttributeInfo("uri");

            transformComponentType.Type = getNodeType("gap", "transformComponentType");
            transformComponentType.nameAttribute = transformComponentType.Type.GetAttributeInfo("name");
            transformComponentType.activeAttribute = transformComponentType.Type.GetAttributeInfo("active");
            transformComponentType.translationAttribute = transformComponentType.Type.GetAttributeInfo("translation");
            transformComponentType.rotationAttribute = transformComponentType.Type.GetAttributeInfo("rotation");
            transformComponentType.scaleAttribute = transformComponentType.Type.GetAttributeInfo("scale");

            gameObjectGroupType.Type = getNodeType("gap", "gameObjectGroupType");
            gameObjectGroupType.transformAttribute = gameObjectGroupType.Type.GetAttributeInfo("transform");
            gameObjectGroupType.translateAttribute = gameObjectGroupType.Type.GetAttributeInfo("translate");
            gameObjectGroupType.rotateAttribute = gameObjectGroupType.Type.GetAttributeInfo("rotate");
            gameObjectGroupType.scaleAttribute = gameObjectGroupType.Type.GetAttributeInfo("scale");
            gameObjectGroupType.pivotAttribute = gameObjectGroupType.Type.GetAttributeInfo("pivot");
            gameObjectGroupType.nameAttribute = gameObjectGroupType.Type.GetAttributeInfo("name");
            gameObjectGroupType.visibleAttribute = gameObjectGroupType.Type.GetAttributeInfo("visible");
            gameObjectGroupType.lockedAttribute = gameObjectGroupType.Type.GetAttributeInfo("locked");
            gameObjectGroupType.transformationTypeAttribute = gameObjectGroupType.Type.GetAttributeInfo("transformationType");
            gameObjectGroupType.componentChild = gameObjectGroupType.Type.GetChildInfo("component");
            gameObjectGroupType.gameObjectChild = gameObjectGroupType.Type.GetChildInfo("gameObject");

            objectOverrideType.Type = getNodeType("gap", "objectOverrideType");
            objectOverrideType.objectNameAttribute = objectOverrideType.Type.GetAttributeInfo("objectName");
            objectOverrideType.attributeOverrideChild = objectOverrideType.Type.GetChildInfo("attributeOverride");

            attributeOverrideType.Type = getNodeType("gap", "attributeOverrideType");
            attributeOverrideType.nameAttribute = attributeOverrideType.Type.GetAttributeInfo("name");
            attributeOverrideType.valueAttribute = attributeOverrideType.Type.GetAttributeInfo("value");

            prefabInstanceType.Type = getNodeType("gap", "prefabInstanceType");
            prefabInstanceType.transformAttribute = prefabInstanceType.Type.GetAttributeInfo("transform");
            prefabInstanceType.translateAttribute = prefabInstanceType.Type.GetAttributeInfo("translate");
            prefabInstanceType.rotateAttribute = prefabInstanceType.Type.GetAttributeInfo("rotate");
            prefabInstanceType.scaleAttribute = prefabInstanceType.Type.GetAttributeInfo("scale");
            prefabInstanceType.pivotAttribute = prefabInstanceType.Type.GetAttributeInfo("pivot");
            prefabInstanceType.nameAttribute = prefabInstanceType.Type.GetAttributeInfo("name");
            prefabInstanceType.visibleAttribute = prefabInstanceType.Type.GetAttributeInfo("visible");
            prefabInstanceType.lockedAttribute = prefabInstanceType.Type.GetAttributeInfo("locked");
            prefabInstanceType.transformationTypeAttribute = prefabInstanceType.Type.GetAttributeInfo("transformationType");
            prefabInstanceType.prefabRefAttribute = prefabInstanceType.Type.GetAttributeInfo("prefabRef");
            prefabInstanceType.componentChild = prefabInstanceType.Type.GetChildInfo("component");
            prefabInstanceType.gameObjectChild = prefabInstanceType.Type.GetChildInfo("gameObject");
            prefabInstanceType.objectOverrideChild = prefabInstanceType.Type.GetChildInfo("objectOverride");

            renderComponentType.Type = getNodeType("gap", "renderComponentType");
            renderComponentType.nameAttribute = renderComponentType.Type.GetAttributeInfo("name");
            renderComponentType.activeAttribute = renderComponentType.Type.GetAttributeInfo("active");
            renderComponentType.translationAttribute = renderComponentType.Type.GetAttributeInfo("translation");
            renderComponentType.rotationAttribute = renderComponentType.Type.GetAttributeInfo("rotation");
            renderComponentType.scaleAttribute = renderComponentType.Type.GetAttributeInfo("scale");
            renderComponentType.visibleAttribute = renderComponentType.Type.GetAttributeInfo("visible");
            renderComponentType.castShadowAttribute = renderComponentType.Type.GetAttributeInfo("castShadow");
            renderComponentType.receiveShadowAttribute = renderComponentType.Type.GetAttributeInfo("receiveShadow");
            renderComponentType.drawDistanceAttribute = renderComponentType.Type.GetAttributeInfo("drawDistance");

            meshComponentType.Type = getNodeType("gap", "meshComponentType");
            meshComponentType.nameAttribute = meshComponentType.Type.GetAttributeInfo("name");
            meshComponentType.activeAttribute = meshComponentType.Type.GetAttributeInfo("active");
            meshComponentType.translationAttribute = meshComponentType.Type.GetAttributeInfo("translation");
            meshComponentType.rotationAttribute = meshComponentType.Type.GetAttributeInfo("rotation");
            meshComponentType.scaleAttribute = meshComponentType.Type.GetAttributeInfo("scale");
            meshComponentType.visibleAttribute = meshComponentType.Type.GetAttributeInfo("visible");
            meshComponentType.castShadowAttribute = meshComponentType.Type.GetAttributeInfo("castShadow");
            meshComponentType.receiveShadowAttribute = meshComponentType.Type.GetAttributeInfo("receiveShadow");
            meshComponentType.drawDistanceAttribute = meshComponentType.Type.GetAttributeInfo("drawDistance");
            meshComponentType.refAttribute = meshComponentType.Type.GetAttributeInfo("ref");

            spinnerComponentType.Type = getNodeType("gap", "spinnerComponentType");
            spinnerComponentType.nameAttribute = spinnerComponentType.Type.GetAttributeInfo("name");
            spinnerComponentType.activeAttribute = spinnerComponentType.Type.GetAttributeInfo("active");
            spinnerComponentType.rpsAttribute = spinnerComponentType.Type.GetAttributeInfo("rps");

            modelReferenceType.Type = getNodeType("gap", "modelReferenceType");
            modelReferenceType.uriAttribute = modelReferenceType.Type.GetAttributeInfo("uri");
            modelReferenceType.tagAttribute = modelReferenceType.Type.GetAttributeInfo("tag");

            locatorType.Type = getNodeType("gap", "locatorType");
            locatorType.transformAttribute = locatorType.Type.GetAttributeInfo("transform");
            locatorType.translateAttribute = locatorType.Type.GetAttributeInfo("translate");
            locatorType.rotateAttribute = locatorType.Type.GetAttributeInfo("rotate");
            locatorType.scaleAttribute = locatorType.Type.GetAttributeInfo("scale");
            locatorType.pivotAttribute = locatorType.Type.GetAttributeInfo("pivot");
            locatorType.nameAttribute = locatorType.Type.GetAttributeInfo("name");
            locatorType.visibleAttribute = locatorType.Type.GetAttributeInfo("visible");
            locatorType.lockedAttribute = locatorType.Type.GetAttributeInfo("locked");
            locatorType.transformationTypeAttribute = locatorType.Type.GetAttributeInfo("transformationType");
            locatorType.componentChild = locatorType.Type.GetChildInfo("component");
            locatorType.resourceChild = locatorType.Type.GetChildInfo("resource");
            locatorType.stmRefChild = locatorType.Type.GetChildInfo("stmRef");

            stateMachineRefType.Type = getNodeType("gap", "stateMachineRefType");
            stateMachineRefType.uriAttribute = stateMachineRefType.Type.GetAttributeInfo("uri");
            stateMachineRefType.flatPropertyTableChild = stateMachineRefType.Type.GetChildInfo("flatPropertyTable");

            flatPropertyTableType.Type = getNodeType("gap", "flatPropertyTableType");
            flatPropertyTableType.propertyChild = flatPropertyTableType.Type.GetChildInfo("property");

            propertyType.Type = getNodeType("gap", "propertyType");
            propertyType.scopeAttribute = propertyType.Type.GetAttributeInfo("scope");
            propertyType.typeAttribute = propertyType.Type.GetAttributeInfo("type");
            propertyType.absolutePathAttribute = propertyType.Type.GetAttributeInfo("absolutePath");
            propertyType.propertyNameAttribute = propertyType.Type.GetAttributeInfo("propertyName");
            propertyType.defaultValueAttribute = propertyType.Type.GetAttributeInfo("defaultValue");
            propertyType.valueAttribute = propertyType.Type.GetAttributeInfo("value");
            propertyType.minValueAttribute = propertyType.Type.GetAttributeInfo("minValue");
            propertyType.maxValueAttribute = propertyType.Type.GetAttributeInfo("maxValue");
            propertyType.descriptionAttribute = propertyType.Type.GetAttributeInfo("description");
            propertyType.categoryAttribute = propertyType.Type.GetAttributeInfo("category");
            propertyType.warningAttribute = propertyType.Type.GetAttributeInfo("warning");

            DirLight.Type = getNodeType("gap", "DirLight");
            DirLight.transformAttribute = DirLight.Type.GetAttributeInfo("transform");
            DirLight.translateAttribute = DirLight.Type.GetAttributeInfo("translate");
            DirLight.rotateAttribute = DirLight.Type.GetAttributeInfo("rotate");
            DirLight.scaleAttribute = DirLight.Type.GetAttributeInfo("scale");
            DirLight.pivotAttribute = DirLight.Type.GetAttributeInfo("pivot");
            DirLight.nameAttribute = DirLight.Type.GetAttributeInfo("name");
            DirLight.visibleAttribute = DirLight.Type.GetAttributeInfo("visible");
            DirLight.lockedAttribute = DirLight.Type.GetAttributeInfo("locked");
            DirLight.transformationTypeAttribute = DirLight.Type.GetAttributeInfo("transformationType");
            DirLight.ambientAttribute = DirLight.Type.GetAttributeInfo("ambient");
            DirLight.diffuseAttribute = DirLight.Type.GetAttributeInfo("diffuse");
            DirLight.specularAttribute = DirLight.Type.GetAttributeInfo("specular");
            DirLight.directionAttribute = DirLight.Type.GetAttributeInfo("direction");
            DirLight.componentChild = DirLight.Type.GetChildInfo("component");

            BoxLight.Type = getNodeType("gap", "BoxLight");
            BoxLight.transformAttribute = BoxLight.Type.GetAttributeInfo("transform");
            BoxLight.translateAttribute = BoxLight.Type.GetAttributeInfo("translate");
            BoxLight.rotateAttribute = BoxLight.Type.GetAttributeInfo("rotate");
            BoxLight.scaleAttribute = BoxLight.Type.GetAttributeInfo("scale");
            BoxLight.pivotAttribute = BoxLight.Type.GetAttributeInfo("pivot");
            BoxLight.nameAttribute = BoxLight.Type.GetAttributeInfo("name");
            BoxLight.visibleAttribute = BoxLight.Type.GetAttributeInfo("visible");
            BoxLight.lockedAttribute = BoxLight.Type.GetAttributeInfo("locked");
            BoxLight.transformationTypeAttribute = BoxLight.Type.GetAttributeInfo("transformationType");
            BoxLight.ambientAttribute = BoxLight.Type.GetAttributeInfo("ambient");
            BoxLight.diffuseAttribute = BoxLight.Type.GetAttributeInfo("diffuse");
            BoxLight.specularAttribute = BoxLight.Type.GetAttributeInfo("specular");
            BoxLight.directionAttribute = BoxLight.Type.GetAttributeInfo("direction");
            BoxLight.attenuationAttribute = BoxLight.Type.GetAttributeInfo("attenuation");
            BoxLight.componentChild = BoxLight.Type.GetChildInfo("component");

            PointLight.Type = getNodeType("gap", "PointLight");
            PointLight.transformAttribute = PointLight.Type.GetAttributeInfo("transform");
            PointLight.translateAttribute = PointLight.Type.GetAttributeInfo("translate");
            PointLight.rotateAttribute = PointLight.Type.GetAttributeInfo("rotate");
            PointLight.scaleAttribute = PointLight.Type.GetAttributeInfo("scale");
            PointLight.pivotAttribute = PointLight.Type.GetAttributeInfo("pivot");
            PointLight.nameAttribute = PointLight.Type.GetAttributeInfo("name");
            PointLight.visibleAttribute = PointLight.Type.GetAttributeInfo("visible");
            PointLight.lockedAttribute = PointLight.Type.GetAttributeInfo("locked");
            PointLight.transformationTypeAttribute = PointLight.Type.GetAttributeInfo("transformationType");
            PointLight.ambientAttribute = PointLight.Type.GetAttributeInfo("ambient");
            PointLight.diffuseAttribute = PointLight.Type.GetAttributeInfo("diffuse");
            PointLight.specularAttribute = PointLight.Type.GetAttributeInfo("specular");
            PointLight.attenuationAttribute = PointLight.Type.GetAttributeInfo("attenuation");
            PointLight.rangeAttribute = PointLight.Type.GetAttributeInfo("range");
            PointLight.componentChild = PointLight.Type.GetChildInfo("component");

            controlPointType.Type = getNodeType("gap", "controlPointType");
            controlPointType.transformAttribute = controlPointType.Type.GetAttributeInfo("transform");
            controlPointType.translateAttribute = controlPointType.Type.GetAttributeInfo("translate");
            controlPointType.rotateAttribute = controlPointType.Type.GetAttributeInfo("rotate");
            controlPointType.scaleAttribute = controlPointType.Type.GetAttributeInfo("scale");
            controlPointType.pivotAttribute = controlPointType.Type.GetAttributeInfo("pivot");
            controlPointType.nameAttribute = controlPointType.Type.GetAttributeInfo("name");
            controlPointType.visibleAttribute = controlPointType.Type.GetAttributeInfo("visible");
            controlPointType.lockedAttribute = controlPointType.Type.GetAttributeInfo("locked");
            controlPointType.transformationTypeAttribute = controlPointType.Type.GetAttributeInfo("transformationType");
            controlPointType.componentChild = controlPointType.Type.GetChildInfo("component");

            curveType.Type = getNodeType("gap", "curveType");
            curveType.transformAttribute = curveType.Type.GetAttributeInfo("transform");
            curveType.translateAttribute = curveType.Type.GetAttributeInfo("translate");
            curveType.rotateAttribute = curveType.Type.GetAttributeInfo("rotate");
            curveType.scaleAttribute = curveType.Type.GetAttributeInfo("scale");
            curveType.pivotAttribute = curveType.Type.GetAttributeInfo("pivot");
            curveType.nameAttribute = curveType.Type.GetAttributeInfo("name");
            curveType.visibleAttribute = curveType.Type.GetAttributeInfo("visible");
            curveType.lockedAttribute = curveType.Type.GetAttributeInfo("locked");
            curveType.transformationTypeAttribute = curveType.Type.GetAttributeInfo("transformationType");
            curveType.colorAttribute = curveType.Type.GetAttributeInfo("color");
            curveType.isClosedAttribute = curveType.Type.GetAttributeInfo("isClosed");
            curveType.stepsAttribute = curveType.Type.GetAttributeInfo("steps");
            curveType.interpolationTypeAttribute = curveType.Type.GetAttributeInfo("interpolationType");
            curveType.componentChild = curveType.Type.GetChildInfo("component");
            curveType.pointChild = curveType.Type.GetChildInfo("point");

            catmullRomType.Type = getNodeType("gap", "catmullRomType");
            catmullRomType.transformAttribute = catmullRomType.Type.GetAttributeInfo("transform");
            catmullRomType.translateAttribute = catmullRomType.Type.GetAttributeInfo("translate");
            catmullRomType.rotateAttribute = catmullRomType.Type.GetAttributeInfo("rotate");
            catmullRomType.scaleAttribute = catmullRomType.Type.GetAttributeInfo("scale");
            catmullRomType.pivotAttribute = catmullRomType.Type.GetAttributeInfo("pivot");
            catmullRomType.nameAttribute = catmullRomType.Type.GetAttributeInfo("name");
            catmullRomType.visibleAttribute = catmullRomType.Type.GetAttributeInfo("visible");
            catmullRomType.lockedAttribute = catmullRomType.Type.GetAttributeInfo("locked");
            catmullRomType.transformationTypeAttribute = catmullRomType.Type.GetAttributeInfo("transformationType");
            catmullRomType.colorAttribute = catmullRomType.Type.GetAttributeInfo("color");
            catmullRomType.isClosedAttribute = catmullRomType.Type.GetAttributeInfo("isClosed");
            catmullRomType.stepsAttribute = catmullRomType.Type.GetAttributeInfo("steps");
            catmullRomType.interpolationTypeAttribute = catmullRomType.Type.GetAttributeInfo("interpolationType");
            catmullRomType.componentChild = catmullRomType.Type.GetChildInfo("component");
            catmullRomType.pointChild = catmullRomType.Type.GetChildInfo("point");

            bezierType.Type = getNodeType("gap", "bezierType");
            bezierType.transformAttribute = bezierType.Type.GetAttributeInfo("transform");
            bezierType.translateAttribute = bezierType.Type.GetAttributeInfo("translate");
            bezierType.rotateAttribute = bezierType.Type.GetAttributeInfo("rotate");
            bezierType.scaleAttribute = bezierType.Type.GetAttributeInfo("scale");
            bezierType.pivotAttribute = bezierType.Type.GetAttributeInfo("pivot");
            bezierType.nameAttribute = bezierType.Type.GetAttributeInfo("name");
            bezierType.visibleAttribute = bezierType.Type.GetAttributeInfo("visible");
            bezierType.lockedAttribute = bezierType.Type.GetAttributeInfo("locked");
            bezierType.transformationTypeAttribute = bezierType.Type.GetAttributeInfo("transformationType");
            bezierType.colorAttribute = bezierType.Type.GetAttributeInfo("color");
            bezierType.isClosedAttribute = bezierType.Type.GetAttributeInfo("isClosed");
            bezierType.stepsAttribute = bezierType.Type.GetAttributeInfo("steps");
            bezierType.interpolationTypeAttribute = bezierType.Type.GetAttributeInfo("interpolationType");
            bezierType.componentChild = bezierType.Type.GetChildInfo("component");
            bezierType.pointChild = bezierType.Type.GetChildInfo("point");

            skyDomeType.Type = getNodeType("gap", "skyDomeType");
            skyDomeType.transformAttribute = skyDomeType.Type.GetAttributeInfo("transform");
            skyDomeType.translateAttribute = skyDomeType.Type.GetAttributeInfo("translate");
            skyDomeType.rotateAttribute = skyDomeType.Type.GetAttributeInfo("rotate");
            skyDomeType.scaleAttribute = skyDomeType.Type.GetAttributeInfo("scale");
            skyDomeType.pivotAttribute = skyDomeType.Type.GetAttributeInfo("pivot");
            skyDomeType.nameAttribute = skyDomeType.Type.GetAttributeInfo("name");
            skyDomeType.visibleAttribute = skyDomeType.Type.GetAttributeInfo("visible");
            skyDomeType.lockedAttribute = skyDomeType.Type.GetAttributeInfo("locked");
            skyDomeType.transformationTypeAttribute = skyDomeType.Type.GetAttributeInfo("transformationType");
            skyDomeType.cubeMapAttribute = skyDomeType.Type.GetAttributeInfo("cubeMap");
            skyDomeType.componentChild = skyDomeType.Type.GetChildInfo("component");

            shapeTestType.Type = getNodeType("gap", "shapeTestType");
            shapeTestType.transformAttribute = shapeTestType.Type.GetAttributeInfo("transform");
            shapeTestType.translateAttribute = shapeTestType.Type.GetAttributeInfo("translate");
            shapeTestType.rotateAttribute = shapeTestType.Type.GetAttributeInfo("rotate");
            shapeTestType.scaleAttribute = shapeTestType.Type.GetAttributeInfo("scale");
            shapeTestType.pivotAttribute = shapeTestType.Type.GetAttributeInfo("pivot");
            shapeTestType.nameAttribute = shapeTestType.Type.GetAttributeInfo("name");
            shapeTestType.visibleAttribute = shapeTestType.Type.GetAttributeInfo("visible");
            shapeTestType.lockedAttribute = shapeTestType.Type.GetAttributeInfo("locked");
            shapeTestType.transformationTypeAttribute = shapeTestType.Type.GetAttributeInfo("transformationType");
            shapeTestType.colorAttribute = shapeTestType.Type.GetAttributeInfo("color");
            shapeTestType.emissiveAttribute = shapeTestType.Type.GetAttributeInfo("emissive");
            shapeTestType.specularAttribute = shapeTestType.Type.GetAttributeInfo("specular");
            shapeTestType.specularPowerAttribute = shapeTestType.Type.GetAttributeInfo("specularPower");
            shapeTestType.diffuseAttribute = shapeTestType.Type.GetAttributeInfo("diffuse");
            shapeTestType.normalAttribute = shapeTestType.Type.GetAttributeInfo("normal");
            shapeTestType.textureTransformAttribute = shapeTestType.Type.GetAttributeInfo("textureTransform");
            shapeTestType.componentChild = shapeTestType.Type.GetChildInfo("component");

            cubeTestType.Type = getNodeType("gap", "cubeTestType");
            cubeTestType.transformAttribute = cubeTestType.Type.GetAttributeInfo("transform");
            cubeTestType.translateAttribute = cubeTestType.Type.GetAttributeInfo("translate");
            cubeTestType.rotateAttribute = cubeTestType.Type.GetAttributeInfo("rotate");
            cubeTestType.scaleAttribute = cubeTestType.Type.GetAttributeInfo("scale");
            cubeTestType.pivotAttribute = cubeTestType.Type.GetAttributeInfo("pivot");
            cubeTestType.nameAttribute = cubeTestType.Type.GetAttributeInfo("name");
            cubeTestType.visibleAttribute = cubeTestType.Type.GetAttributeInfo("visible");
            cubeTestType.lockedAttribute = cubeTestType.Type.GetAttributeInfo("locked");
            cubeTestType.transformationTypeAttribute = cubeTestType.Type.GetAttributeInfo("transformationType");
            cubeTestType.colorAttribute = cubeTestType.Type.GetAttributeInfo("color");
            cubeTestType.emissiveAttribute = cubeTestType.Type.GetAttributeInfo("emissive");
            cubeTestType.specularAttribute = cubeTestType.Type.GetAttributeInfo("specular");
            cubeTestType.specularPowerAttribute = cubeTestType.Type.GetAttributeInfo("specularPower");
            cubeTestType.diffuseAttribute = cubeTestType.Type.GetAttributeInfo("diffuse");
            cubeTestType.normalAttribute = cubeTestType.Type.GetAttributeInfo("normal");
            cubeTestType.textureTransformAttribute = cubeTestType.Type.GetAttributeInfo("textureTransform");
            cubeTestType.componentChild = cubeTestType.Type.GetChildInfo("component");

            TorusTestType.Type = getNodeType("gap", "TorusTestType");
            TorusTestType.transformAttribute = TorusTestType.Type.GetAttributeInfo("transform");
            TorusTestType.translateAttribute = TorusTestType.Type.GetAttributeInfo("translate");
            TorusTestType.rotateAttribute = TorusTestType.Type.GetAttributeInfo("rotate");
            TorusTestType.scaleAttribute = TorusTestType.Type.GetAttributeInfo("scale");
            TorusTestType.pivotAttribute = TorusTestType.Type.GetAttributeInfo("pivot");
            TorusTestType.nameAttribute = TorusTestType.Type.GetAttributeInfo("name");
            TorusTestType.visibleAttribute = TorusTestType.Type.GetAttributeInfo("visible");
            TorusTestType.lockedAttribute = TorusTestType.Type.GetAttributeInfo("locked");
            TorusTestType.transformationTypeAttribute = TorusTestType.Type.GetAttributeInfo("transformationType");
            TorusTestType.colorAttribute = TorusTestType.Type.GetAttributeInfo("color");
            TorusTestType.emissiveAttribute = TorusTestType.Type.GetAttributeInfo("emissive");
            TorusTestType.specularAttribute = TorusTestType.Type.GetAttributeInfo("specular");
            TorusTestType.specularPowerAttribute = TorusTestType.Type.GetAttributeInfo("specularPower");
            TorusTestType.diffuseAttribute = TorusTestType.Type.GetAttributeInfo("diffuse");
            TorusTestType.normalAttribute = TorusTestType.Type.GetAttributeInfo("normal");
            TorusTestType.textureTransformAttribute = TorusTestType.Type.GetAttributeInfo("textureTransform");
            TorusTestType.componentChild = TorusTestType.Type.GetChildInfo("component");

            sphereTestType.Type = getNodeType("gap", "sphereTestType");
            sphereTestType.transformAttribute = sphereTestType.Type.GetAttributeInfo("transform");
            sphereTestType.translateAttribute = sphereTestType.Type.GetAttributeInfo("translate");
            sphereTestType.rotateAttribute = sphereTestType.Type.GetAttributeInfo("rotate");
            sphereTestType.scaleAttribute = sphereTestType.Type.GetAttributeInfo("scale");
            sphereTestType.pivotAttribute = sphereTestType.Type.GetAttributeInfo("pivot");
            sphereTestType.nameAttribute = sphereTestType.Type.GetAttributeInfo("name");
            sphereTestType.visibleAttribute = sphereTestType.Type.GetAttributeInfo("visible");
            sphereTestType.lockedAttribute = sphereTestType.Type.GetAttributeInfo("locked");
            sphereTestType.transformationTypeAttribute = sphereTestType.Type.GetAttributeInfo("transformationType");
            sphereTestType.colorAttribute = sphereTestType.Type.GetAttributeInfo("color");
            sphereTestType.emissiveAttribute = sphereTestType.Type.GetAttributeInfo("emissive");
            sphereTestType.specularAttribute = sphereTestType.Type.GetAttributeInfo("specular");
            sphereTestType.specularPowerAttribute = sphereTestType.Type.GetAttributeInfo("specularPower");
            sphereTestType.diffuseAttribute = sphereTestType.Type.GetAttributeInfo("diffuse");
            sphereTestType.normalAttribute = sphereTestType.Type.GetAttributeInfo("normal");
            sphereTestType.textureTransformAttribute = sphereTestType.Type.GetAttributeInfo("textureTransform");
            sphereTestType.componentChild = sphereTestType.Type.GetChildInfo("component");

            coneTestType.Type = getNodeType("gap", "coneTestType");
            coneTestType.transformAttribute = coneTestType.Type.GetAttributeInfo("transform");
            coneTestType.translateAttribute = coneTestType.Type.GetAttributeInfo("translate");
            coneTestType.rotateAttribute = coneTestType.Type.GetAttributeInfo("rotate");
            coneTestType.scaleAttribute = coneTestType.Type.GetAttributeInfo("scale");
            coneTestType.pivotAttribute = coneTestType.Type.GetAttributeInfo("pivot");
            coneTestType.nameAttribute = coneTestType.Type.GetAttributeInfo("name");
            coneTestType.visibleAttribute = coneTestType.Type.GetAttributeInfo("visible");
            coneTestType.lockedAttribute = coneTestType.Type.GetAttributeInfo("locked");
            coneTestType.transformationTypeAttribute = coneTestType.Type.GetAttributeInfo("transformationType");
            coneTestType.colorAttribute = coneTestType.Type.GetAttributeInfo("color");
            coneTestType.emissiveAttribute = coneTestType.Type.GetAttributeInfo("emissive");
            coneTestType.specularAttribute = coneTestType.Type.GetAttributeInfo("specular");
            coneTestType.specularPowerAttribute = coneTestType.Type.GetAttributeInfo("specularPower");
            coneTestType.diffuseAttribute = coneTestType.Type.GetAttributeInfo("diffuse");
            coneTestType.normalAttribute = coneTestType.Type.GetAttributeInfo("normal");
            coneTestType.textureTransformAttribute = coneTestType.Type.GetAttributeInfo("textureTransform");
            coneTestType.componentChild = coneTestType.Type.GetChildInfo("component");

            cylinderTestType.Type = getNodeType("gap", "cylinderTestType");
            cylinderTestType.transformAttribute = cylinderTestType.Type.GetAttributeInfo("transform");
            cylinderTestType.translateAttribute = cylinderTestType.Type.GetAttributeInfo("translate");
            cylinderTestType.rotateAttribute = cylinderTestType.Type.GetAttributeInfo("rotate");
            cylinderTestType.scaleAttribute = cylinderTestType.Type.GetAttributeInfo("scale");
            cylinderTestType.pivotAttribute = cylinderTestType.Type.GetAttributeInfo("pivot");
            cylinderTestType.nameAttribute = cylinderTestType.Type.GetAttributeInfo("name");
            cylinderTestType.visibleAttribute = cylinderTestType.Type.GetAttributeInfo("visible");
            cylinderTestType.lockedAttribute = cylinderTestType.Type.GetAttributeInfo("locked");
            cylinderTestType.transformationTypeAttribute = cylinderTestType.Type.GetAttributeInfo("transformationType");
            cylinderTestType.colorAttribute = cylinderTestType.Type.GetAttributeInfo("color");
            cylinderTestType.emissiveAttribute = cylinderTestType.Type.GetAttributeInfo("emissive");
            cylinderTestType.specularAttribute = cylinderTestType.Type.GetAttributeInfo("specular");
            cylinderTestType.specularPowerAttribute = cylinderTestType.Type.GetAttributeInfo("specularPower");
            cylinderTestType.diffuseAttribute = cylinderTestType.Type.GetAttributeInfo("diffuse");
            cylinderTestType.normalAttribute = cylinderTestType.Type.GetAttributeInfo("normal");
            cylinderTestType.textureTransformAttribute = cylinderTestType.Type.GetAttributeInfo("textureTransform");
            cylinderTestType.componentChild = cylinderTestType.Type.GetChildInfo("component");

            planeTestType.Type = getNodeType("gap", "planeTestType");
            planeTestType.transformAttribute = planeTestType.Type.GetAttributeInfo("transform");
            planeTestType.translateAttribute = planeTestType.Type.GetAttributeInfo("translate");
            planeTestType.rotateAttribute = planeTestType.Type.GetAttributeInfo("rotate");
            planeTestType.scaleAttribute = planeTestType.Type.GetAttributeInfo("scale");
            planeTestType.pivotAttribute = planeTestType.Type.GetAttributeInfo("pivot");
            planeTestType.nameAttribute = planeTestType.Type.GetAttributeInfo("name");
            planeTestType.visibleAttribute = planeTestType.Type.GetAttributeInfo("visible");
            planeTestType.lockedAttribute = planeTestType.Type.GetAttributeInfo("locked");
            planeTestType.transformationTypeAttribute = planeTestType.Type.GetAttributeInfo("transformationType");
            planeTestType.colorAttribute = planeTestType.Type.GetAttributeInfo("color");
            planeTestType.emissiveAttribute = planeTestType.Type.GetAttributeInfo("emissive");
            planeTestType.specularAttribute = planeTestType.Type.GetAttributeInfo("specular");
            planeTestType.specularPowerAttribute = planeTestType.Type.GetAttributeInfo("specularPower");
            planeTestType.diffuseAttribute = planeTestType.Type.GetAttributeInfo("diffuse");
            planeTestType.normalAttribute = planeTestType.Type.GetAttributeInfo("normal");
            planeTestType.textureTransformAttribute = planeTestType.Type.GetAttributeInfo("textureTransform");
            planeTestType.componentChild = planeTestType.Type.GetChildInfo("component");

            billboardTestType.Type = getNodeType("gap", "billboardTestType");
            billboardTestType.transformAttribute = billboardTestType.Type.GetAttributeInfo("transform");
            billboardTestType.translateAttribute = billboardTestType.Type.GetAttributeInfo("translate");
            billboardTestType.rotateAttribute = billboardTestType.Type.GetAttributeInfo("rotate");
            billboardTestType.scaleAttribute = billboardTestType.Type.GetAttributeInfo("scale");
            billboardTestType.pivotAttribute = billboardTestType.Type.GetAttributeInfo("pivot");
            billboardTestType.nameAttribute = billboardTestType.Type.GetAttributeInfo("name");
            billboardTestType.visibleAttribute = billboardTestType.Type.GetAttributeInfo("visible");
            billboardTestType.lockedAttribute = billboardTestType.Type.GetAttributeInfo("locked");
            billboardTestType.transformationTypeAttribute = billboardTestType.Type.GetAttributeInfo("transformationType");
            billboardTestType.intensityAttribute = billboardTestType.Type.GetAttributeInfo("intensity");
            billboardTestType.diffuseAttribute = billboardTestType.Type.GetAttributeInfo("diffuse");
            billboardTestType.textureTransformAttribute = billboardTestType.Type.GetAttributeInfo("textureTransform");
            billboardTestType.componentChild = billboardTestType.Type.GetChildInfo("component");

            orcType.Type = getNodeType("gap", "orcType");
            orcType.transformAttribute = orcType.Type.GetAttributeInfo("transform");
            orcType.translateAttribute = orcType.Type.GetAttributeInfo("translate");
            orcType.rotateAttribute = orcType.Type.GetAttributeInfo("rotate");
            orcType.scaleAttribute = orcType.Type.GetAttributeInfo("scale");
            orcType.pivotAttribute = orcType.Type.GetAttributeInfo("pivot");
            orcType.nameAttribute = orcType.Type.GetAttributeInfo("name");
            orcType.visibleAttribute = orcType.Type.GetAttributeInfo("visible");
            orcType.lockedAttribute = orcType.Type.GetAttributeInfo("locked");
            orcType.transformationTypeAttribute = orcType.Type.GetAttributeInfo("transformationType");
            orcType.weightAttribute = orcType.Type.GetAttributeInfo("weight");
            orcType.emotionAttribute = orcType.Type.GetAttributeInfo("emotion");
            orcType.goalsAttribute = orcType.Type.GetAttributeInfo("goals");
            orcType.colorAttribute = orcType.Type.GetAttributeInfo("color");
            orcType.toeColorAttribute = orcType.Type.GetAttributeInfo("toeColor");
            orcType.componentChild = orcType.Type.GetChildInfo("component");
            orcType.geometryChild = orcType.Type.GetChildInfo("geometry");
            orcType.animationChild = orcType.Type.GetChildInfo("animation");
            orcType.targetChild = orcType.Type.GetChildInfo("target");
            orcType.friendsChild = orcType.Type.GetChildInfo("friends");
            orcType.childrenChild = orcType.Type.GetChildInfo("children");

            terrainMapType.Type = getNodeType("gap", "terrainMapType");
            terrainMapType.nameAttribute = terrainMapType.Type.GetAttributeInfo("name");
            terrainMapType.visibleAttribute = terrainMapType.Type.GetAttributeInfo("visible");
            terrainMapType.minHeightAttribute = terrainMapType.Type.GetAttributeInfo("minHeight");
            terrainMapType.maxHeightAttribute = terrainMapType.Type.GetAttributeInfo("maxHeight");
            terrainMapType.minSlopeAttribute = terrainMapType.Type.GetAttributeInfo("minSlope");
            terrainMapType.maxSlopeAttribute = terrainMapType.Type.GetAttributeInfo("maxSlope");
            terrainMapType.diffuseAttribute = terrainMapType.Type.GetAttributeInfo("diffuse");
            terrainMapType.normalAttribute = terrainMapType.Type.GetAttributeInfo("normal");
            terrainMapType.specularAttribute = terrainMapType.Type.GetAttributeInfo("specular");
            terrainMapType.maskAttribute = terrainMapType.Type.GetAttributeInfo("mask");

            decorationMapType.Type = getNodeType("gap", "decorationMapType");
            decorationMapType.nameAttribute = decorationMapType.Type.GetAttributeInfo("name");
            decorationMapType.visibleAttribute = decorationMapType.Type.GetAttributeInfo("visible");
            decorationMapType.minHeightAttribute = decorationMapType.Type.GetAttributeInfo("minHeight");
            decorationMapType.maxHeightAttribute = decorationMapType.Type.GetAttributeInfo("maxHeight");
            decorationMapType.minSlopeAttribute = decorationMapType.Type.GetAttributeInfo("minSlope");
            decorationMapType.maxSlopeAttribute = decorationMapType.Type.GetAttributeInfo("maxSlope");
            decorationMapType.diffuseAttribute = decorationMapType.Type.GetAttributeInfo("diffuse");
            decorationMapType.normalAttribute = decorationMapType.Type.GetAttributeInfo("normal");
            decorationMapType.specularAttribute = decorationMapType.Type.GetAttributeInfo("specular");
            decorationMapType.maskAttribute = decorationMapType.Type.GetAttributeInfo("mask");
            decorationMapType.scaleAttribute = decorationMapType.Type.GetAttributeInfo("scale");
            decorationMapType.numOfDecoratorsAttribute = decorationMapType.Type.GetAttributeInfo("numOfDecorators");
            decorationMapType.lodDistanceAttribute = decorationMapType.Type.GetAttributeInfo("lodDistance");
            decorationMapType.useBillboardAttribute = decorationMapType.Type.GetAttributeInfo("useBillboard");

            layerMapType.Type = getNodeType("gap", "layerMapType");
            layerMapType.nameAttribute = layerMapType.Type.GetAttributeInfo("name");
            layerMapType.visibleAttribute = layerMapType.Type.GetAttributeInfo("visible");
            layerMapType.minHeightAttribute = layerMapType.Type.GetAttributeInfo("minHeight");
            layerMapType.maxHeightAttribute = layerMapType.Type.GetAttributeInfo("maxHeight");
            layerMapType.minSlopeAttribute = layerMapType.Type.GetAttributeInfo("minSlope");
            layerMapType.maxSlopeAttribute = layerMapType.Type.GetAttributeInfo("maxSlope");
            layerMapType.diffuseAttribute = layerMapType.Type.GetAttributeInfo("diffuse");
            layerMapType.normalAttribute = layerMapType.Type.GetAttributeInfo("normal");
            layerMapType.specularAttribute = layerMapType.Type.GetAttributeInfo("specular");
            layerMapType.maskAttribute = layerMapType.Type.GetAttributeInfo("mask");
            layerMapType.lodTextureAttribute = layerMapType.Type.GetAttributeInfo("lodTexture");
            layerMapType.textureScaleAttribute = layerMapType.Type.GetAttributeInfo("textureScale");

            terrainGobType.Type = getNodeType("gap", "terrainGobType");
            terrainGobType.transformAttribute = terrainGobType.Type.GetAttributeInfo("transform");
            terrainGobType.translateAttribute = terrainGobType.Type.GetAttributeInfo("translate");
            terrainGobType.rotateAttribute = terrainGobType.Type.GetAttributeInfo("rotate");
            terrainGobType.scaleAttribute = terrainGobType.Type.GetAttributeInfo("scale");
            terrainGobType.pivotAttribute = terrainGobType.Type.GetAttributeInfo("pivot");
            terrainGobType.nameAttribute = terrainGobType.Type.GetAttributeInfo("name");
            terrainGobType.visibleAttribute = terrainGobType.Type.GetAttributeInfo("visible");
            terrainGobType.lockedAttribute = terrainGobType.Type.GetAttributeInfo("locked");
            terrainGobType.transformationTypeAttribute = terrainGobType.Type.GetAttributeInfo("transformationType");
            terrainGobType.cellSizeAttribute = terrainGobType.Type.GetAttributeInfo("cellSize");
            terrainGobType.heightMapAttribute = terrainGobType.Type.GetAttributeInfo("heightMap");
            terrainGobType.componentChild = terrainGobType.Type.GetChildInfo("component");
            terrainGobType.layerMapChild = terrainGobType.Type.GetChildInfo("layerMap");
            terrainGobType.decorationMapChild = terrainGobType.Type.GetChildInfo("decorationMap");

            gameRootElement = getRootElement(NS, "game");
            prototypeRootElement = getRootElement(NS, "prototype");
            prefabRootElement = getRootElement(NS, "prefab");
            textureMetadataRootElement = getRootElement(NS, "textureMetadata");
            resourceMetadataRootElement = getRootElement(NS, "resourceMetadata");
        }

        public static class gameType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo fogEnabledAttribute;
            public static AttributeInfo fogColorAttribute;
            public static AttributeInfo fogRangeAttribute;
            public static AttributeInfo fogDensityAttribute;
            public static ChildInfo gameObjectFolderChild;
            public static ChildInfo layersChild;
            public static ChildInfo bookmarksChild;
            public static ChildInfo gameReferenceChild;
            public static ChildInfo gridChild;
        }

        public static class gameObjectFolderType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static ChildInfo gameObjectChild;
            public static ChildInfo folderChild;
        }

        public static class gameObjectType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static ChildInfo componentChild;
        }

        public static class gameObjectComponentType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo activeAttribute;
        }

        public static class layersType
        {
            public static DomNodeType Type;
            public static ChildInfo layerChild;
        }

        public static class layerType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo gameObjectReferenceChild;
            public static ChildInfo layerChild;
        }

        public static class gameObjectReferenceType
        {
            public static DomNodeType Type;
            public static AttributeInfo refAttribute;
        }

        public static class bookmarksType
        {
            public static DomNodeType Type;
            public static ChildInfo bookmarkChild;
        }

        public static class bookmarkType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo cameraChild;
            public static ChildInfo bookmarkChild;
        }

        public static class cameraType
        {
            public static DomNodeType Type;
            public static AttributeInfo eyeAttribute;
            public static AttributeInfo lookAtPointAttribute;
            public static AttributeInfo upVectorAttribute;
            public static AttributeInfo viewTypeAttribute;
            public static AttributeInfo yFovAttribute;
            public static AttributeInfo nearZAttribute;
            public static AttributeInfo farZAttribute;
            public static AttributeInfo focusRadiusAttribute;
        }

        public static class gameReferenceType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo refAttribute;
            public static AttributeInfo tagsAttribute;
        }

        public static class gridType
        {
            public static DomNodeType Type;
            public static AttributeInfo sizeAttribute;
            public static AttributeInfo subdivisionsAttribute;
            public static AttributeInfo heightAttribute;
            public static AttributeInfo snapAttribute;
            public static AttributeInfo visibleAttribute;
        }

        public static class prototypeType
        {
            public static DomNodeType Type;
            public static ChildInfo gameObjectChild;
        }

        public static class prefabType
        {
            public static DomNodeType Type;
            public static ChildInfo gameObjectChild;
        }

        public static class textureMetadataType
        {
            public static DomNodeType Type;
            public static AttributeInfo uriAttribute;
            public static AttributeInfo keywordsAttribute;
            public static AttributeInfo compressionSettingAttribute;
            public static AttributeInfo memoryLayoutAttribute;
            public static AttributeInfo mipMapAttribute;
        }

        public static class resourceMetadataType
        {
            public static DomNodeType Type;
            public static AttributeInfo uriAttribute;
            public static AttributeInfo keywordsAttribute;
        }

        public static class resourceReferenceType
        {
            public static DomNodeType Type;
            public static AttributeInfo uriAttribute;
        }

        public static class transformComponentType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo activeAttribute;
            public static AttributeInfo translationAttribute;
            public static AttributeInfo rotationAttribute;
            public static AttributeInfo scaleAttribute;
        }

        public static class gameObjectGroupType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static ChildInfo componentChild;
            public static ChildInfo gameObjectChild;
        }

        public static class objectOverrideType
        {
            public static DomNodeType Type;
            public static AttributeInfo objectNameAttribute;
            public static ChildInfo attributeOverrideChild;
        }

        public static class attributeOverrideType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo valueAttribute;
        }

        public static class prefabInstanceType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo prefabRefAttribute;
            public static ChildInfo componentChild;
            public static ChildInfo gameObjectChild;
            public static ChildInfo objectOverrideChild;
        }

        public static class renderComponentType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo activeAttribute;
            public static AttributeInfo translationAttribute;
            public static AttributeInfo rotationAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo castShadowAttribute;
            public static AttributeInfo receiveShadowAttribute;
            public static AttributeInfo drawDistanceAttribute;
        }

        public static class meshComponentType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo activeAttribute;
            public static AttributeInfo translationAttribute;
            public static AttributeInfo rotationAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo castShadowAttribute;
            public static AttributeInfo receiveShadowAttribute;
            public static AttributeInfo drawDistanceAttribute;
            public static AttributeInfo refAttribute;
        }

        public static class spinnerComponentType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo activeAttribute;
            public static AttributeInfo rpsAttribute;
        }

        public static class modelReferenceType
        {
            public static DomNodeType Type;
            public static AttributeInfo uriAttribute;
            public static AttributeInfo tagAttribute;
        }

        public static class locatorType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static ChildInfo componentChild;
            public static ChildInfo resourceChild;
            public static ChildInfo stmRefChild;
        }

        public static class stateMachineRefType
        {
            public static DomNodeType Type;
            public static AttributeInfo uriAttribute;
            public static ChildInfo flatPropertyTableChild;
        }

        public static class flatPropertyTableType
        {
            public static DomNodeType Type;
            public static ChildInfo propertyChild;
        }

        public static class propertyType
        {
            public static DomNodeType Type;
            public static AttributeInfo scopeAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo absolutePathAttribute;
            public static AttributeInfo propertyNameAttribute;
            public static AttributeInfo defaultValueAttribute;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo minValueAttribute;
            public static AttributeInfo maxValueAttribute;
            public static AttributeInfo descriptionAttribute;
            public static AttributeInfo categoryAttribute;
            public static AttributeInfo warningAttribute;
        }

        public static class DirLight
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo ambientAttribute;
            public static AttributeInfo diffuseAttribute;
            public static AttributeInfo specularAttribute;
            public static AttributeInfo directionAttribute;
            public static ChildInfo componentChild;
        }

        public static class BoxLight
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo ambientAttribute;
            public static AttributeInfo diffuseAttribute;
            public static AttributeInfo specularAttribute;
            public static AttributeInfo directionAttribute;
            public static AttributeInfo attenuationAttribute;
            public static ChildInfo componentChild;
        }

        public static class PointLight
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo ambientAttribute;
            public static AttributeInfo diffuseAttribute;
            public static AttributeInfo specularAttribute;
            public static AttributeInfo attenuationAttribute;
            public static AttributeInfo rangeAttribute;
            public static ChildInfo componentChild;
        }

        public static class controlPointType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static ChildInfo componentChild;
        }

        public static class curveType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo isClosedAttribute;
            public static AttributeInfo stepsAttribute;
            public static AttributeInfo interpolationTypeAttribute;
            public static ChildInfo componentChild;
            public static ChildInfo pointChild;
        }

        public static class catmullRomType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo isClosedAttribute;
            public static AttributeInfo stepsAttribute;
            public static AttributeInfo interpolationTypeAttribute;
            public static ChildInfo componentChild;
            public static ChildInfo pointChild;
        }

        public static class bezierType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo isClosedAttribute;
            public static AttributeInfo stepsAttribute;
            public static AttributeInfo interpolationTypeAttribute;
            public static ChildInfo componentChild;
            public static ChildInfo pointChild;
        }

        public static class skyDomeType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo cubeMapAttribute;
            public static ChildInfo componentChild;
        }

        public static class shapeTestType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo emissiveAttribute;
            public static AttributeInfo specularAttribute;
            public static AttributeInfo specularPowerAttribute;
            public static AttributeInfo diffuseAttribute;
            public static AttributeInfo normalAttribute;
            public static AttributeInfo textureTransformAttribute;
            public static ChildInfo componentChild;
        }

        public static class cubeTestType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo emissiveAttribute;
            public static AttributeInfo specularAttribute;
            public static AttributeInfo specularPowerAttribute;
            public static AttributeInfo diffuseAttribute;
            public static AttributeInfo normalAttribute;
            public static AttributeInfo textureTransformAttribute;
            public static ChildInfo componentChild;
        }

        public static class TorusTestType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo emissiveAttribute;
            public static AttributeInfo specularAttribute;
            public static AttributeInfo specularPowerAttribute;
            public static AttributeInfo diffuseAttribute;
            public static AttributeInfo normalAttribute;
            public static AttributeInfo textureTransformAttribute;
            public static ChildInfo componentChild;
        }

        public static class sphereTestType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo emissiveAttribute;
            public static AttributeInfo specularAttribute;
            public static AttributeInfo specularPowerAttribute;
            public static AttributeInfo diffuseAttribute;
            public static AttributeInfo normalAttribute;
            public static AttributeInfo textureTransformAttribute;
            public static ChildInfo componentChild;
        }

        public static class coneTestType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo emissiveAttribute;
            public static AttributeInfo specularAttribute;
            public static AttributeInfo specularPowerAttribute;
            public static AttributeInfo diffuseAttribute;
            public static AttributeInfo normalAttribute;
            public static AttributeInfo textureTransformAttribute;
            public static ChildInfo componentChild;
        }

        public static class cylinderTestType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo emissiveAttribute;
            public static AttributeInfo specularAttribute;
            public static AttributeInfo specularPowerAttribute;
            public static AttributeInfo diffuseAttribute;
            public static AttributeInfo normalAttribute;
            public static AttributeInfo textureTransformAttribute;
            public static ChildInfo componentChild;
        }

        public static class planeTestType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo emissiveAttribute;
            public static AttributeInfo specularAttribute;
            public static AttributeInfo specularPowerAttribute;
            public static AttributeInfo diffuseAttribute;
            public static AttributeInfo normalAttribute;
            public static AttributeInfo textureTransformAttribute;
            public static ChildInfo componentChild;
        }

        public static class billboardTestType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo intensityAttribute;
            public static AttributeInfo diffuseAttribute;
            public static AttributeInfo textureTransformAttribute;
            public static ChildInfo componentChild;
        }

        public static class orcType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo weightAttribute;
            public static AttributeInfo emotionAttribute;
            public static AttributeInfo goalsAttribute;
            public static AttributeInfo colorAttribute;
            public static AttributeInfo toeColorAttribute;
            public static ChildInfo componentChild;
            public static ChildInfo geometryChild;
            public static ChildInfo animationChild;
            public static ChildInfo targetChild;
            public static ChildInfo friendsChild;
            public static ChildInfo childrenChild;
        }

        public static class terrainMapType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo minHeightAttribute;
            public static AttributeInfo maxHeightAttribute;
            public static AttributeInfo minSlopeAttribute;
            public static AttributeInfo maxSlopeAttribute;
            public static AttributeInfo diffuseAttribute;
            public static AttributeInfo normalAttribute;
            public static AttributeInfo specularAttribute;
            public static AttributeInfo maskAttribute;
        }

        public static class decorationMapType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo minHeightAttribute;
            public static AttributeInfo maxHeightAttribute;
            public static AttributeInfo minSlopeAttribute;
            public static AttributeInfo maxSlopeAttribute;
            public static AttributeInfo diffuseAttribute;
            public static AttributeInfo normalAttribute;
            public static AttributeInfo specularAttribute;
            public static AttributeInfo maskAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo numOfDecoratorsAttribute;
            public static AttributeInfo lodDistanceAttribute;
            public static AttributeInfo useBillboardAttribute;
        }

        public static class layerMapType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo minHeightAttribute;
            public static AttributeInfo maxHeightAttribute;
            public static AttributeInfo minSlopeAttribute;
            public static AttributeInfo maxSlopeAttribute;
            public static AttributeInfo diffuseAttribute;
            public static AttributeInfo normalAttribute;
            public static AttributeInfo specularAttribute;
            public static AttributeInfo maskAttribute;
            public static AttributeInfo lodTextureAttribute;
            public static AttributeInfo textureScaleAttribute;
        }

        public static class terrainGobType
        {
            public static DomNodeType Type;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo rotateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo pivotAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo visibleAttribute;
            public static AttributeInfo lockedAttribute;
            public static AttributeInfo transformationTypeAttribute;
            public static AttributeInfo cellSizeAttribute;
            public static AttributeInfo heightMapAttribute;
            public static ChildInfo componentChild;
            public static ChildInfo layerMapChild;
            public static ChildInfo decorationMapChild;
        }

        public static ChildInfo gameRootElement;

        public static ChildInfo prototypeRootElement;

        public static ChildInfo prefabRootElement;

        public static ChildInfo textureMetadataRootElement;

        public static ChildInfo resourceMetadataRootElement;
    }
}
