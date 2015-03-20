//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
#pragma once
#include "GobBridge.h"
#include "../Core/Object.h"

#include "../GobSystem/BillboardGob.h"
#include "../GobSystem/BoxLightGob.h"
#include "../GobSystem/ConeGob.h"
#include "../GobSystem/ControlPointGob.h"
#include "../GobSystem/CubeGob.h"
#include "../GobSystem/CurveGob.h"
#include "../GobSystem/CylinderGob.h"
#include "../GobSystem/DirLightGob.h"
#include "../GobSystem/GameLevel.h"
#include "../GobSystem/GameObject.h"
#include "../GobSystem/GameObjectGroup.h"
#include "../GobSystem/Locator.h"
#include "../GobSystem/OrcGob.h"
#include "../GobSystem/PlaneGob.h"
#include "../GobSystem/PointLightGob.h"
#include "../GobSystem/PolyLineGob.h"
#include "../GobSystem/SkyDome.h"
#include "../GobSystem/SphereGob.h"
#include "../GobSystem/TorusGob.h"
#include "../GobSystem/GameObjectComponent.h"
#include "../GobSystem/MeshComponent.h"
#include "../GobSystem/SpinnerComponent.h"
#include "../GobSystem/Terrain/TerrainGob.h"
#include "../GobSystem/Terrain/TerrainMap.h"
#include "../GobSystem/Terrain/LayerMap.h"
#include "../GobSystem/Terrain/DecorationMap.h"

#include "../Renderer/Resource.h"


namespace LvEdEngine
{
    void InitGobBridge( GobBridge& bridge);
};

