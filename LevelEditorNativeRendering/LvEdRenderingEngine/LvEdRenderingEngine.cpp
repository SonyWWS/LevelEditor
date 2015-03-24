//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#define _CRTDBG_MAP_ALLOC
#include <stdlib.h>
#include <crtdbg.h>
#include "LvEdRenderingEngine.h"
#include <stdio.h>
#include <hash_set>
#include <algorithm>
#include <D3D11.h>
#include "Core/Logger.h"
#include "Core/ErrorHandler.h"
#include "Core/PerfTimer.h"
#include "Core/Utils.h"
#include "Core/WinHeaders.h"
#include <mmsystem.h>
#include "Bridge/GobBridge.h"
#include "Bridge/RegisterSchemaObjects.h"
#include "Bridge/RegisterRuntimeObjects.h"
#include "Renderer/RenderContext.h"
#include "Renderer/RenderSurface.h"
#include "Renderer/DeviceManager.h"
#include "Renderer/SwapChain.h"
#include "Renderer/BasicRenderer.h"
#include "Renderer/TexturedShader.h"
#include "Renderer/WireframeShader.h"
#include "Renderer/RenderUtil.h"
#include "Renderer/RenderableNodeSorter.h"
#include "Renderer/ShadowMapGen.h"
#include "Renderer/LineRenderer.h"
#include "Renderer/Shader.h"
#include "Renderer/ShaderLib.h"
#include "Renderer/TextureLib.h"
#include "Renderer/GpuResourceFactory.h"
#include "ResourceManager/ResourceManager.h"
#include "Model3d/XmlModelFactory.h"
#include "Model3d/AtgiModelFactory.h"
#include "Model3d/ColladaModelFactory.h"
#include "ResourceManager/TextureFactory.h"
#include "GobSystem/GameLevel.h"
#include "GobSystem/SkyDome.h"
#include "LvEdUtils.h"
#include "Renderer/RenderBuffer.h"
#include "Renderer/Model.h"
#include "Renderer/FontRenderer.h"
#include "Renderer/Font.h"
#include "Model3d/rapidxmlhelpers.h"
#include "Core/Hasher.h"
#include "Core/FileUtils.h"
#include "DirectX\DirectXTex\DirectXTex.h"
#include "Renderer/Texture.h"
#include "DirectX/DXUtil.h"
#include "Core\ImageData.h"
#include "GobSystem\Terrain\TerrainGob.h"
#include "Renderer\TerrainShader.h"

// Use the following primitive types
//int8_t;
//int16_t;
//int32_t;
//int64_t;
//uint8_t;
//uint16_t;
//uint32_t;
//uint64_t;
//intptr_t;
//uintptr_t;


using namespace LvEdEngine;
using namespace LvEdFonts;

// hit record, if you change this structure, make sure it's
// twin in the c# code is updated in structs.cs
struct HitRecord
{
    ObjectGUID objectId;
    uint32_t index;
    float distance;
    float3 hitPt;
    float3 normal;
    float3 nearestVertex;
    char hasNormal;
    char hasNearestVertex;
    char pad1;
    char pad2;
};




// Used for sending all the required engine information 
// to managed side (C# side).
class EngineInfo : public NonCopyable
{
public:
    static void InitInstance()
    {
        if(!s_inst) s_inst = new EngineInfo();

    }
    static void DestroyInstance()
    {
        if(s_inst)
        {
            delete s_inst;
            s_inst = NULL;
        }
    }

    static      EngineInfo*  Inst() { return s_inst; }

    // Returns pointer to null terminated string of
    // a fully formatted xml document.
    const wchar_t* GetInfo() { return m_data.c_str();};

private:
    EngineInfo();
    static EngineInfo*   s_inst;
    std::wstring m_data;
};
EngineInfo*  EngineInfo::s_inst = NULL;




class MyResourceListener : public ResourceListener
{
public:
    void SetCallback(InvalidateViewsCallbackType invalidateCallback)
    {
        m_callback = invalidateCallback;
    }
    virtual void OnResourceLoaded(Resource* r);
private:
    InvalidateViewsCallbackType m_callback;
};

class EngineData : public NonCopyable
{
public:
    EngineData( ID3D11Device* device );
    ~EngineData();

    GobBridge Bridge;
    RenderSurface* pRenderSurface;
  

    // this is the root object of the scene.
    // all other game objects are descended from this object
    GameLevel* GameLevel;

    // this is our basic renderer, it is used to service the C# application's
    // rendering needs.
    BasicRenderer* basicRenderer;
      
    MyResourceListener resourceListener;
    std::vector<HitRecord> HitRecords;
    ShadowMapGen*        shadowMapShader;
    RenderableNodeSorter    renderableSorter;
    RenderableNodeSet       pickCollector; 
    Font* AxisFont;
    
};

//---------------------------------------------------------------------------
EngineData::EngineData( ID3D11Device* device )
  : pRenderSurface( NULL ),  
    GameLevel( NULL ),
    basicRenderer( NULL ),    
    shadowMapShader( NULL)    
{
    
    // Initialize the 'code generated' bridge.
    InitGobBridge(Bridge);
    RegisterRuntimeObjects(Bridge);

    basicRenderer   = new BasicRenderer(device);
    shadowMapShader = new ShadowMapGen(device);

    AxisFont = Font::CreateNewInstance( device,L"Arial",14,LvEdFonts::kFontStyleBOLD);
}

//---------------------------------------------------------------------------
EngineData::~EngineData()
{
    SAFE_DELETE(basicRenderer);    
    SAFE_DELETE(shadowMapShader); 
    SAFE_DELETE(AxisFont);    
}


static EngineData* s_engineData = NULL;


//=============================================================================================
void MyResourceListener::OnResourceLoaded(Resource* /*r*/)
{    
    RenderContext::Inst()->LightEnvDirty = true;
    if(m_callback) m_callback();   
}

//=============================================================================================
// Initialize and Shutdown
//=============================================================================================

LVEDRENDERINGENGINE_API void __stdcall LvEd_Initialize(LogCallbackType logCallback, InvalidateViewsCallbackType invalidateCallback
    , const wchar_t** outEngineInfo)
{
    // Enable run-time memory check for debug builds.
#if defined(DEBUG) || defined(_DEBUG)
  //   _crtBreakAlloc = 925; //example break on alloc number 1027, change 
	_CrtSetDbgFlag( _CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF );
#endif

    if(s_engineData) return;   
    ErrorHandler::ClearError();
    if(logCallback) 
        Logger::SetLogCallback(logCallback);

    Logger::Log(OutputMessageType::Info, L"Initializing Rendering Engine\n");    
    
    
    // note if you using game-engine
    // you don't need to use DeviceManager class.
    // the game-engine should provide
    gD3D11 = new DeviceManager();
    GpuResourceFactory::SetDevice(gD3D11->GetDevice());
    RSCache::InitInstance(gD3D11->GetDevice());
    TextureLib::InitInstance(gD3D11->GetDevice());
    ShapeLibStartup(gD3D11->GetDevice());
    ResourceManager::InitInstance();
    LineRenderer::InitInstance(gD3D11->GetDevice());
    ShadowMaps::InitInstance(gD3D11->GetDevice(),2048);
   
    RenderContext::InitInstance(gD3D11->GetDevice());
    s_engineData = new EngineData( gD3D11->GetDevice() );
    s_engineData->resourceListener.SetCallback(invalidateCallback);
    RenderContext::Inst()->SetContext(gD3D11->GetImmediateContext());

    ShaderLib::InitInstance(gD3D11->GetDevice());
    FontRenderer::InitInstance( gD3D11 );
    

    // Register resource factories...
    ResourceManager * resMan = ResourceManager::Inst();
    AtgiModelFactory * atgiFactory = new AtgiModelFactory(gD3D11->GetDevice());
    ColladaModelFactory * colladaFactory = new ColladaModelFactory(gD3D11->GetDevice());    
    TextureFactory * texFactory = new TextureFactory(gD3D11->GetDevice());
    resMan->RegisterListener(&s_engineData->resourceListener);    
    resMan->RegisterFactory(L".tga", texFactory);
    resMan->RegisterFactory(L".png", texFactory);
    resMan->RegisterFactory(L".jpg", texFactory);
    resMan->RegisterFactory(L".bmp", texFactory);
    resMan->RegisterFactory(L".dds", texFactory);
    resMan->RegisterFactory(L".tif", texFactory);
    resMan->RegisterFactory(L".atgi", atgiFactory);
    resMan->RegisterFactory(L".dae", colladaFactory);

    EngineInfo::InitInstance();
    const wchar_t* info = EngineInfo::Inst()->GetInfo();
    if(outEngineInfo)
        *outEngineInfo = info;
}


LVEDRENDERINGENGINE_API void __stdcall LvEd_Shutdown(void)
{
    ErrorHandler::ClearError();
    Logger::Log(OutputMessageType::Info, L"Shutdown Rendering Engine\n");
    if(!gD3D11) return;

    LvEd_Clear();

    ShapeLibShutdown();
    TextureLib::DestroyInstance();
    LvEdFonts::FontRenderer::DestroyInstance();
    ShaderLib::DestroyInstance();    
    LineRenderer::DestroyInstance();
    RenderContext::DestroyInstance();    
    ResourceManager::DestroyInstance();
    ShadowMaps::DestroyInstance();
    RSCache::DestroyInstance();
    EngineInfo::DestroyInstance();
    SAFE_DELETE(s_engineData);
    SAFE_DELETE(gD3D11);
}


LVEDRENDERINGENGINE_API void __stdcall LvEd_Clear()
{
    ErrorHandler::ClearError();
    Logger::Log(OutputMessageType::Info, "SceneReset\n");    
    RenderContext::Inst()->selection.clear();        
    ResourceManager * rm = ResourceManager::Inst();
    rm->GarbageCollect();
}

//===============================================================================
// Object
//===============================================================================

const char* gobSkyDome = "SkyDome";
const char* gobGroup   = "GameObjectGroup";

LVEDRENDERINGENGINE_API ObjectTypeGUID __stdcall LvEd_GetObjectTypeId(char* className)
{
    ErrorHandler::ClearError();
    return s_engineData->Bridge.GetTypeId(className);
}

LVEDRENDERINGENGINE_API ObjectPropertyUID  _stdcall LvEd_GetObjectPropertyId(ObjectTypeGUID id, char* propertyName)
{
    ErrorHandler::ClearError();
    return s_engineData->Bridge.GetPropertyId(id,propertyName);
}

LVEDRENDERINGENGINE_API ObjectPropertyUID __stdcall LvEd_GetObjectChildListId(ObjectTypeGUID id, char* listName)
{
    ErrorHandler::ClearError();
    return s_engineData->Bridge.GetChildListId(id,listName);
}

LVEDRENDERINGENGINE_API ObjectGUID  __stdcall LvEd_CreateObject(ObjectTypeGUID typeId, void* data, int size)
{
    ErrorHandler::ClearError();
    ObjectGUID instanceId = s_engineData->Bridge.CreateObject(typeId, data, size);    
    return instanceId;
}


LVEDRENDERINGENGINE_API void __stdcall LvEd_DestroyObject(ObjectTypeGUID typeId, ObjectGUID instanceId)
{
    ErrorHandler::ClearError();
    if(s_engineData->GameLevel && s_engineData->GameLevel->GetInstanceId() == instanceId)
        s_engineData->GameLevel = NULL;

    s_engineData->Bridge.DestroyObject(typeId, instanceId);

    ResourceManager::Inst()->GarbageCollect();
}



LVEDRENDERINGENGINE_API void __stdcall LvEd_InvokeMemberFn(ObjectGUID instanceId, wchar_t* fn, const void* arg, void** retVal)
{
    if(instanceId == 0) return;
    Object* obj = reinterpret_cast<Object*>(instanceId);
    obj->Invoke(fn,arg,retVal);
}

static hash32_t swapchainTypeId = Hash32("SwapChain");
static hash32_t renderStateTypeId = Hash32("RenderState");
LVEDRENDERINGENGINE_API void __stdcall LvEd_SetObjectProperty(ObjectTypeGUID typeId, ObjectPropertyUID propId, ObjectGUID instanceId, void* data, int size)
{
    ErrorHandler::ClearError();
    s_engineData->Bridge.SetProperty(typeId,propId,instanceId,data,size);
    
    // for certain objects, we don't want to update lighting.    
    if(typeId == renderStateTypeId || typeId == swapchainTypeId)
        return;

    RenderContext::Inst()->LightEnvDirty = true;   
}

LVEDRENDERINGENGINE_API void __stdcall LvEd_GetObjectProperty(ObjectTypeGUID typeId, ObjectPropertyUID propId, ObjectGUID instanceId, void** data, int* size)
{
    ErrorHandler::ClearError();
    s_engineData->Bridge.GetProperty(typeId,propId,instanceId,data,size);
}


LVEDRENDERINGENGINE_API void __stdcall LvEd_ObjectAddChild(ObjectTypeGUID typeId, ObjectPropertyUID listId, ObjectGUID parentId, ObjectGUID  childId, int index)
{
    ErrorHandler::ClearError();
    assert(listId != 0);
    assert(parentId != 0);
    
    if(parentId == 0 || listId == 0 )
    {
        ErrorHandler::SetError(ErrorType::UnknownError,
                L"%s: parentId is %d and listId is %d but should both be non-zero\n",
                __WFUNCTION__, parentId, listId);
        return;
    }

    if(parentId == s_engineData->GameLevel->GetInstanceId())
    {
        listId = Hash32("Child");
        typeId = Hash32(gobGroup);
    }
    s_engineData->Bridge.AddChild(typeId, listId, parentId, childId, index);

     Object* obj = (Object*)childId;
     if(strcmp(obj->ClassName(),SkyDome::StaticClassName()) ==0)
     {
         s_engineData->GameLevel->m_activeskyeDome =(SkyDome*)obj;         
     }
     else if(strcmp(obj->ClassName(),TerrainGob::StaticClassName()) ==0)
     {
         s_engineData->GameLevel->Terrains.push_back((TerrainGob*)obj);         
     }
         
    RenderContext::Inst()->LightEnvDirty = true;
}

LVEDRENDERINGENGINE_API void __stdcall LvEd_ObjectRemoveChild(ObjectTypeGUID typeId, ObjectListUID listId, ObjectGUID parentId, ObjectGUID childId)
{
    ErrorHandler::ClearError();
    assert(listId != 0);
    assert(parentId != 0);
    
    if(parentId == 0 || listId == 0 )
    {
        ErrorHandler::SetError(ErrorType::UnknownError,
                L"%s: parentId is %d and listId is %d but should both be non-zero\n",
                __WFUNCTION__, parentId, listId);
        return;
    }

    if(parentId == s_engineData->GameLevel->GetInstanceId())
    {
        listId = Hash32("Child");
        typeId = Hash32(gobGroup);
    }
    s_engineData->Bridge.RemoveChild(typeId, listId, parentId, childId);
    Object* obj = (Object*)childId;
    if(s_engineData->GameLevel->m_activeskyeDome == obj)
    {
        FindGobsByType query(SkyDome::StaticClassName());
        s_engineData->GameLevel->Query(query);       
        size_t count = query.Gobs.size();        
        s_engineData->GameLevel->m_activeskyeDome = 
            (count > 0)? (SkyDome*)query.Gobs[count-1] : NULL;
    }
    else if(strcmp(obj->ClassName(),TerrainGob::StaticClassName()) == 0)
    {
        TerrainGob* terrain = (TerrainGob*)obj;        
        auto it = std::find(s_engineData->GameLevel->Terrains.begin(),s_engineData->GameLevel->Terrains.end(),terrain);
        if(it != s_engineData->GameLevel->Terrains.end())
        {
            s_engineData->GameLevel->Terrains.erase(it);
        }                   
    }

    RenderContext::Inst()->LightEnvDirty = true;
}



//===============================================================================
// Picking and selection
//===============================================================================

bool HitRecordSorting(const HitRecord& n1, const HitRecord& n2)
{
    return n1.distance < n2.distance;
}


LVEDRENDERINGENGINE_API bool __stdcall LvEd_RayPick(float viewxform[], float projxform[],Ray* rayW, bool skipSelected, HitRecord** hits, int* count)
{
    ErrorHandler::ClearError();
    if(s_engineData->GameLevel == NULL)
    {
        ErrorHandler::SetError(ErrorType::UnknownError, L"%s: no GameLevel set", __WFUNCTION__);
        *hits = 0;
        *count = 0;
        return false;
    }

    Ray ray = *rayW;
    Matrix view = viewxform;
    Matrix proj = projxform;

    GlobalRenderFlagsEnum flags = RenderContext::Inst()->State()->GetGlobalRenderFlags();
    bool backfaceCull = !((flags & GlobalRenderFlags::RenderBackFace) == GlobalRenderFlags::RenderBackFace);

    RenderContext::Inst()->Cam().SetViewProj(view,proj);

    s_engineData->pickCollector.ClearLists();
    s_engineData->pickCollector.SetFlags( RenderContext::Inst()->State()->GetGlobalRenderFlags() );
    s_engineData->pickCollector.SetSkipSelected(skipSelected);

    s_engineData->GameLevel->GetRenderables(&s_engineData->pickCollector, RenderContext::Inst());
    

    s_engineData->HitRecords.clear();
    for(auto it = s_engineData->pickCollector.GetList().begin(); it != s_engineData->pickCollector.GetList().end(); it++)
    {
        RenderableNode& r = (*it);
        AABB boundingBox = r.bounds;

        float t;    // hit distance
        float3 p;  // hit position
        float3 n;  // hit normal
        float3 nearestVertex;

        // perform ray aabb intersection
        // if passed then perform more complex intersection tests
        if(IntersectRayAABB(ray, boundingBox, &t, &p, &n))
        {
            if(it->GetFlag( RenderableNode::kTestAgainstBBoxOnly ))
            {
                HitRecord hit;
                hit.objectId = r.objectId;
                hit.index = 0;
                hit.hitPt = p;
                hit.normal = n;
                hit.nearestVertex = float3(0,0,0);
                hit.distance = t;
                hit.hasNearestVertex = false;
                hit.hasNormal = true;

                s_engineData->HitRecords.push_back(hit);
            }
            else
            {
                Mesh* mesh = it->mesh;
                if(mesh != NULL)
                {
                    Matrix invWorld;
                    Matrix::Invert(it->WorldXform,invWorld);

                    // ray in object space.
                    Ray lray;
                    lray.pos = float3::Transform(ray.pos,invWorld);                    
                    lray.direction = normalize( float3::TransformNormal(ray.direction,invWorld) );

                    if(mesh->primitiveType == PrimitiveType::TriangleList)
                    {                        
                        // perform ray tri intersection and return
                        // the closest intersection distance a long lray.direction.
                        bool picked = MeshIntersects(lray,&mesh->pos[0],
                           (uint32_t)mesh->pos.size(),
                           &mesh->indices[0],
                           (uint32_t)mesh->indices.size(),
                           backfaceCull,
                           &t,
                           &p,
                           &n,
                           &nearestVertex);

                        if(picked)
                        {
                            // intersection point,nor in World space.
                            float3 posW = float3::Transform(p,it->WorldXform);
                            float3 nearestVertexW = float3::Transform(nearestVertex,it->WorldXform);
                            float3 norW = float3::TransformNormal(n, it->WorldXform);
                            // compute dist to intersection point.
                            float dist = length(posW - ray.pos);

                            HitRecord hit;
                            hit.objectId = r.objectId;
                            hit.index = 0;
                            hit.hitPt = posW;
                            hit.normal = norW;
                            hit.nearestVertex = nearestVertexW;
                            hit.distance = dist;
                            hit.hasNormal = true;
                            hit.hasNearestVertex = true;
                            s_engineData->HitRecords.push_back(hit);
                        }
                    }
                    else if(mesh->primitiveType == PrimitiveType::LineStrip)
                    {
                        const float pixelWidth = 5.f;

                        uint32_t hitIndex = 0;
                        float distTo, distBetween;
                        bool infront = DistanceRayToLineStrip(ray, &mesh->pos[0], (uint32_t)mesh->pos.size(),it->WorldXform,                                         
                                        &distTo, &distBetween, &p, &n, &hitIndex);

                        // we need to adjust distance for screen space calculations
                        // because the ray doesn't start at the camera position
                        float distCamCenter = distTo + length(RenderContext::Inst()->Cam().CamPos() - ray.pos);

                        float viewportHeight = RenderContext::Inst()->ViewPort().y;
                        float nearRatio = RenderContext::Inst()->Cam().Proj().M22;
                        float distToScreenRatio = (nearRatio / distCamCenter) * viewportHeight / 2.f;
                        float screenBetween = distBetween * distToScreenRatio;

                        if (infront && screenBetween < pixelWidth)
                        {
                            HitRecord hit;
                            hit.objectId = r.objectId;
                            hit.index = hitIndex;
                            hit.hitPt = p;
                            hit.normal = n;
                            hit.distance = distTo;
                            hit.hasNearestVertex = false;
                            hit.hasNormal = true;
                            s_engineData->HitRecords.push_back(hit);
                        }
                    }
                    else
                    {
                        assert(0); // unhandled primitive type.
                        ErrorHandler::SetError(ErrorType::UnknownError, L"%s: Unhandled primitive type", __WFUNCTION__);
                    }
                }
            }
        }
    }

    
    for(auto it = s_engineData->GameLevel->Terrains.begin(); it != s_engineData->GameLevel->Terrains.end(); it++)
    {
        HitRecord hitrec;
        if((*it)->RayPick(ray,hitrec.hitPt,hitrec.normal,hitrec.nearestVertex))
        {
            hitrec.index = 0;
            hitrec.objectId = (*it)->GetInstanceId();
            hitrec.distance = length(ray.pos - hitrec.hitPt);
            hitrec.hasNormal = true;
            hitrec.hasNearestVertex = true;            
            s_engineData->HitRecords.push_back(hitrec);
        }
    }


    if(s_engineData->HitRecords.size() > 0)
    {
        std::sort(s_engineData->HitRecords.begin(), s_engineData->HitRecords.end(), HitRecordSorting);
        *hits = &s_engineData->HitRecords[0];
        *count = (int)s_engineData->HitRecords.size();
    }
    else
    {
        *hits = 0;
        *count = 0;
    }
  
    return *count > 0;

}


LVEDRENDERINGENGINE_API bool __stdcall LvEd_FrustumPick(ObjectGUID renderSurface, float viewxform[], float projxform[],float* rect, HitRecord** hits, int* count)
{
    ErrorHandler::ClearError();
    *hits = 0;
    *count = 0;
    float w = rect[2];
    float h = rect[3];

    if(w == 0 || h == 0)
    {
        return false;
    }


    // init camera.
    Matrix view = viewxform;
    Matrix proj = projxform;
    RenderContext::Inst()->Cam().SetViewProj(view,proj);  
    
    // same code used for rendering.
    s_engineData->pickCollector.ClearLists();
    s_engineData->pickCollector.SetFlags( RenderContext::Inst()->State()->GetGlobalRenderFlags() );

    s_engineData->GameLevel->GetRenderables(&s_engineData->pickCollector, RenderContext::Inst());
   
    RenderSurface* pRenderSurface = reinterpret_cast<RenderSurface*>(renderSurface);

    float3 corners[8];
    float x0 = rect[0];
    float y0 = rect[1];
    float x1 = x0 + w;
    float y1 = y0 + h;       
           
    Matrix viewProj = view * proj;
    Matrix invWVP; // inverse of world view projection matrix.
    s_engineData->HitRecords.clear();
    float3 zeroVector(0,0,0);
    Frustum fr; // frustum in local space.
    for(auto it = s_engineData->pickCollector.GetList().begin(); it != s_engineData->pickCollector.GetList().end(); it++)
    {
        RenderableNode& r = (*it);
        if(r.mesh == NULL) continue;

        invWVP = r.WorldXform * viewProj;
        invWVP.Invert();

        corners[0] = pRenderSurface->Unproject(float3(x0,y1,0),invWVP);
        corners[4] = pRenderSurface->Unproject(float3(x0,y1,1),invWVP);

        corners[1] = pRenderSurface->Unproject(float3(x1,y1,0),invWVP);
        corners[5] = pRenderSurface->Unproject(float3(x1,y1,1),invWVP);

        corners[2] = pRenderSurface->Unproject(float3(x1,y0,0),invWVP);
        corners[6] = pRenderSurface->Unproject(float3(x1,y0,1),invWVP);

        corners[3] = pRenderSurface->Unproject(float3(x0,y0,0),invWVP);
        corners[7] = pRenderSurface->Unproject(float3(x0,y0,1),invWVP);
        fr.InitFromCorners(corners);

        int test = FrustumAABBIntersect(fr,r.mesh->bounds);                
        if(test)
        {
            if(test == 1 
                && r.GetFlag(RenderableNode::kTestAgainstBBoxOnly) == false
                && r.mesh != NULL 
                && r.mesh->primitiveType == PrimitiveType::TriangleList)
            {
                Mesh* mesh = r.mesh;
               
                Triangle tr;                
                bool triHit = FrustumMeshIntersect(fr, 
                     &mesh->pos[0],
                   (uint32_t)mesh->pos.size(),
                   &mesh->indices[0],
                   (uint32_t)mesh->indices.size());
                
                if( triHit == false) continue;               
            }
            
            HitRecord hit;
            hit.objectId = r.objectId;
            hit.index = 0;
            hit.hitPt = zeroVector;
            hit.normal = zeroVector;
            hit.nearestVertex = zeroVector;
            hit.distance = 0;
            hit.hasNormal = false;
            hit.hasNearestVertex = false;
            s_engineData->HitRecords.push_back(hit);
        }
    }

    // return the results to the C#. 
    if(s_engineData->HitRecords.size() > 0)
    {
        *hits = &s_engineData->HitRecords[0];
        *count = (int)s_engineData->HitRecords.size();
    }
    return *count > 0;
}

LVEDRENDERINGENGINE_API void __stdcall LvEd_SetSelection(ObjectGUID*  instanceIds, int count)
{
    ErrorHandler::ClearError();
    RenderContext::Inst()->selection.clear();

    for(int i = 0; i < count; i++)
    {
        RenderContext::Inst()->selection.insert(instanceIds[i]);
    }
}

//===============================================================================
// Update and Rendering
//===============================================================================

LVEDRENDERINGENGINE_API void __stdcall LvEd_SetRenderState(ObjectGUID instId)
{
    ErrorHandler::ClearError();
    RenderState* renderState = reinterpret_cast<RenderState*>(instId);    
    RenderContext::Inst()->SetState(renderState);
}

LVEDRENDERINGENGINE_API void __stdcall LvEd_SetGameLevel(ObjectGUID instId)
{
    ErrorHandler::ClearError();
    if(instId != 0)
    {
        GameLevel* gameLevel = reinterpret_cast<GameLevel*>(instId);
        s_engineData->GameLevel = gameLevel;
    }
    else
    {
        s_engineData->GameLevel = NULL;
    }
}

LVEDRENDERINGENGINE_API ObjectGUID __stdcall LvEd_GetGameLevel()
{
    ErrorHandler::ClearError();
    return s_engineData->GameLevel ? s_engineData->GameLevel->GetInstanceId() : 0;
}

LVEDRENDERINGENGINE_API void __stdcall LvEd_WaitForPendingResources()
{
	ResourceManager::Inst()->WaitOnPending();
}

LVEDRENDERINGENGINE_API void __stdcall LvEd_Update(FrameTime* ft, UpdateTypeEnum updateType)
{    
    ErrorHandler::ClearError();    
    s_engineData->GameLevel->Update(*ft, updateType);  
	ShaderLib::Inst()->Update(*ft, updateType);
}

LVEDRENDERINGENGINE_API void __stdcall LvEd_Begin(ObjectGUID renderSurface, float viewxform[], float projxform[])
{
    ErrorHandler::ClearError();
    
    s_engineData->pRenderSurface = reinterpret_cast<RenderSurface*>(renderSurface);

    RenderContext* rc = RenderContext::Inst();

    ExpFog fog;
    fog.enabled = 0;
    if(s_engineData->GameLevel)
    {
        fog = s_engineData->GameLevel->GetFog();
    }            
    rc->SetFog(fog);
    
    // set render target and depth buffer.
    rc->SetContext(gD3D11->GetImmediateContext());
    ID3D11DeviceContext* d3dcontext = rc->Context();

    ID3D11RenderTargetView* rt[1] = {s_engineData->pRenderSurface->GetRenderTargetView()};

    // set render target and depth buffer.
    d3dcontext->OMSetRenderTargets(1,rt,s_engineData->pRenderSurface->GetDepthStencilView());

    // Setup the viewport
    D3D11_VIEWPORT vp = s_engineData->pRenderSurface->GetViewPort();    
    d3dcontext->RSSetViewports( 1, &vp );
    float4 vf(vp.Width,vp.Height,vp.TopLeftX,vp.TopLeftY);
    rc->SetViewPort(vf);

    Matrix view(viewxform);    
    rc->Cam().SetViewProj(view, Matrix(projxform));
    
    d3dcontext->RSSetState(NULL);
    d3dcontext->OMSetDepthStencilState(NULL,0);
    d3dcontext->OMSetBlendState( NULL, NULL, 0xFFFFFFFF );

    // setup default lighting.
    DirLight& light = *LightingState::Inst()->DefaultDirLight();
    light.ambient = float3(0.05f,0.06f,0.07f);    
    light.diffuse = float3(250.0f/255.0f, 245.0f/255.0f, 240.0f/255.0f);       
    light.specular = light.diffuse; // float3(0.4f,0.4f,0.4f);
    light.dir = float3(0.258819073f, -0.965925932f, 0.0f);
    
    d3dcontext->ClearDepthStencilView( s_engineData->pRenderSurface->GetDepthStencilView(), D3D11_CLEAR_DEPTH, 1.0f, 0 );
    if(s_engineData->GameLevel->m_activeskyeDome && s_engineData->GameLevel->m_activeskyeDome->GetVisible())
    {
        s_engineData->GameLevel->m_activeskyeDome->Render(RenderContext::Inst());
    }
    else
    {
        float4 color = s_engineData->pRenderSurface->GetBkgColor();
        d3dcontext->ClearRenderTargetView( s_engineData->pRenderSurface->GetRenderTargetView(), &color.x );
    }

    //Level editor may perform some rendering before LvEd_RenderGame get called.
    s_engineData->basicRenderer->Begin(RenderContext::Inst()->Context(), 
        s_engineData->pRenderSurface,
        RenderContext::Inst()->Cam().View(),
        RenderContext::Inst()->Cam().Proj());
}

bool NodeSortGreater(const RenderableNode& n1, const RenderableNode& n2)
{    
    return n1.Distance > n2.Distance;
}


// ---------------------------------------------------------------------------------------------------------
LVEDRENDERINGENGINE_API void __stdcall LvEd_RenderGame()
{
   
    s_engineData->basicRenderer->End(); 

    ErrorHandler::ClearError();
    if(s_engineData->pRenderSurface == NULL || s_engineData->GameLevel == NULL)
    {
        ErrorHandler::SetError(ErrorType::UnknownError,
                L"%s: RenderSurface is 0x%x and GameLevel is 0x%x but they both must be non-NULL",
                __WFUNCTION__, s_engineData->pRenderSurface, s_engineData->GameLevel);
        return;
    }
   
    RenderState* renderState = RenderContext::Inst()->State();    

    GlobalRenderFlagsEnum flags = renderState->GetGlobalRenderFlags();

    s_engineData->renderableSorter.SetFlags( flags );
    s_engineData->GameLevel->GetRenderables(&s_engineData->renderableSorter, RenderContext::Inst());
   
    // sort semi-transparent objects back to front
     for(unsigned int i = 0; i < s_engineData->renderableSorter.GetBucketCount(); ++i)
     {
         RenderableNodeSorter::Bucket& bucket = *s_engineData->renderableSorter.GetBucket(i);         
         if (bucket.renderables.size() > 0  && (bucket.renderFlags & RenderFlags::AlphaBlend))
         {
             // compute dist along camera's view vector.         
             float3 camLook  = RenderContext::Inst()->Cam().CamLook();
             float3 camPos   = RenderContext::Inst()->Cam().CamPos();
             for(auto it = bucket.renderables.begin(); it != bucket.renderables.end(); it++)
             {
                  RenderableNode& r = (*it);
                  float3 viewVec = r.bounds.GetCenter() - camPos;
                  r.Distance =  dot(camLook, viewVec);                  
             }             
             std::sort(bucket.renderables.begin(),bucket.renderables.end(),NodeSortGreater);             
         }
     }          
     bool renderShadows = (flags & GlobalRenderFlags::Shadows) != 0;
     ShadowMaps::Inst()->SetEnabled(renderShadows);
    //  Pre-Pass For Shadow Maps    
    if (renderShadows)
    {           
        s_engineData->shadowMapShader->Begin( RenderContext::Inst(), s_engineData->pRenderSurface, s_engineData->GameLevel->GetBounds() );        
        for(unsigned int i = 0; i < s_engineData->renderableSorter.GetBucketCount(); ++i)
        {
            RenderableNodeSorter::Bucket& bucket = *s_engineData->renderableSorter.GetBucket(i);
            if (( bucket.shaderId == Shaders::TexturedShader ) &&
                ( bucket.renderables.size() > 0 ))
            {                     
                s_engineData->shadowMapShader->DrawNodes( bucket.renderables);
            }
        }            
        s_engineData->shadowMapShader->End();
    }
   
    // render  opaque objects
    for(unsigned int i = 0; i < s_engineData->renderableSorter.GetBucketCount(); ++i)
    {
        RenderableNodeSorter::Bucket& bucket = *s_engineData->renderableSorter.GetBucket(i);
        if ( bucket.renderables.size() > 0 && (bucket.renderFlags & RenderFlags::AlphaBlend) == 0)
        {            
            Shader* pShader = ShaderLib::Inst()->GetShader((ShadersEnum)bucket.shaderId);
            pShader->Begin( RenderContext::Inst());
            pShader->SetRenderFlag( bucket.renderFlags );   // call this *after* Begin()
            pShader->DrawNodes( bucket.renderables );
            pShader->End();
        }
    }     

    auto terrainlist = &(s_engineData->GameLevel->Terrains);
    if(terrainlist->size() > 0)
    {
        TerrainShader* tshader = (TerrainShader*)ShaderLib::Inst()->GetShader(Shaders::TerrainShader);
        tshader->Begin(RenderContext::Inst());
        for(auto it = terrainlist->begin(); it != terrainlist->end(); it++)
        {
            tshader->RenderTerrain(*it);
        }        
        tshader->End();
    }
           
    // render semi-transparent objects
    for(unsigned int i = 0; i < s_engineData->renderableSorter.GetBucketCount(); ++i)
    {
        RenderableNodeSorter::Bucket& bucket = *s_engineData->renderableSorter.GetBucket(i);
        if ( bucket.renderables.size() > 0 && (bucket.renderFlags & RenderFlags::AlphaBlend))
        {            
            Shader* pShader = ShaderLib::Inst()->GetShader((ShadersEnum)bucket.shaderId);
            pShader->Begin( RenderContext::Inst());
            pShader->SetRenderFlag( bucket.renderFlags );   // call this *after* Begin()
            pShader->DrawNodes( bucket.renderables );
            pShader->End();
        }
    }       


    if ( flags & GlobalRenderFlags::RenderNormals )
    {
        Shader* normShader = ShaderLib::Inst()->GetShader(Shaders::NormalsShader);
        normShader->Begin( RenderContext::Inst());
        for(unsigned int i = 0; i < s_engineData->renderableSorter.GetBucketCount(); ++i)
        {
            RenderableNodeSorter::Bucket& bucket = *s_engineData->renderableSorter.GetBucket(i);
            if ( bucket.renderables.size() > 0 )
            {   
                normShader->DrawNodes( bucket.renderables);
            }
        }            
        normShader->End();
    }

    //Level editor may perform additional rendering before LvEd_End() is called 
    s_engineData->basicRenderer->Begin(RenderContext::Inst()->Context(),
        s_engineData->pRenderSurface,
        RenderContext::Inst()->Cam().View(),
        RenderContext::Inst()->Cam().Proj());
}

// todo: move this code to C# side.
// Renders world axis at the bottom-left corner.
static void RenderWorldAxis()
{
    RenderContext* rc = RenderContext::Inst();
    RenderSurface* surface = s_engineData->pRenderSurface;
    LineRenderer *lr = LineRenderer::Inst();
    Camera& cam = rc->Cam();    
    float margin = 36; // margin in pixels
    float xl = 28; // axis length in pixels.
    float vw = (float)surface->GetWidth();
    float vh = (float)surface->GetHeight();
    Matrix view = cam.View();    
    view.M41 = -vw/2 + margin;
    view.M42 = -vh/2 + margin;
    view.M43 = -xl;
    
    float3 look = cam.CamLook();    
    bool perspective = !cam.IsOrtho();

    // for orthographic hide one of the axis depending on view-type
    const float epsilon  = 0.001f; // use relatively large number for this test.
    bool renderX = perspective || abs(look.x) < epsilon;
    bool renderY = perspective || abs(look.y) < epsilon;
    bool renderZ = perspective || abs(look.z) < epsilon;
    
    Matrix proj = Matrix::CreateOrthographic(vw,vh,1,10000);
    cam.SetViewProj(view,proj);

    float3 centerV(0,0,0);
    // draw x,y,z
    if(renderX)
        lr->DrawLine(centerV,float3(xl,0,0),float4(1,0,0,1));    
    if(renderY)
        lr->DrawLine(centerV,float3(0,xl,0),float4(0,1,0,1));    
    if(renderZ)
        lr->DrawLine(centerV,float3(0,0,xl),float4(0,0,1,1));       

    lr->RenderAll(rc);

    Font* font = s_engineData->AxisFont;
    if(font)
    {   
        float fh = font->GetFontSize();
        float fhh = fh /2.0f;
        FontRenderer* fr = LvEdFonts::FontRenderer::Inst();
        Matrix vp = view * proj;
                
        // draw x,y,z
        if(renderX)
        {
            float3 xpos = surface->Project(float3(xl,fhh,0),vp);
            fr->DrawText(font,L"X",(int)xpos.x,(int)xpos.y,float4(1,0,0,1));
        }

        if(renderY)
        {
            float3 ypos = surface->Project(float3(0,xl+fhh,0),vp);
            fr->DrawText(font,L"Y",(int)ypos.x,(int)ypos.y,float4(0,1,0,1));
        }

        if(renderZ)
        {
            float3 zpos = surface->Project(float3(0,fhh,xl),vp);
            int ycoord =(int) ((zpos.y + fh) > vh ? vh - fh : zpos.y);
            fr->DrawText(font,L"Z",(int)zpos.x,ycoord,float4(0,0,1,1));
        }
        fr->FlushPrintRequests( rc );
    }    
    
}
// ---------------------------------------------------------------------------------------------------------
LVEDRENDERINGENGINE_API void __stdcall LvEd_End()
{
    RenderContext* rc = RenderContext::Inst();
    RenderSurface* surface = s_engineData->pRenderSurface;

    s_engineData->basicRenderer->End();    
    LineRenderer::Inst()->RenderAll(rc);
    ErrorHandler::ClearError();    
    LvEdFonts::FontRenderer::Inst()->FlushPrintRequests( rc );
    
    if(surface->GetType() == RenderSurface::kSwapChain)
    {        
        RenderWorldAxis();
        SwapChain* swapchain = static_cast<SwapChain*>(surface);
        HRESULT hr = swapchain->GetDXGISwapChain()->Present(0,0);
        Logger::IsFailureLog(hr, L"presenting swapchain");
    }

    s_engineData->renderableSorter.ClearLists();    
    s_engineData->pRenderSurface = NULL;    
    RenderContext::Inst()->LightEnvDirty = false; 
}

LVEDRENDERINGENGINE_API bool __stdcall LvEd_SaveRenderSurfaceToFile(ObjectGUID renderSurfaceId, wchar_t *fileName)
{
    ErrorHandler::ClearError();
       
    if(fileName == NULL || wcslen(fileName) == 0 )
    {
        ErrorHandler::SetError(ErrorType::UnknownError, L"%s: filename is empty", __WFUNCTION__);
        return false;
    }

    DirectX::ScratchImage scratchImg; 
    RenderSurface* renderSurface = reinterpret_cast<RenderSurface*>(renderSurfaceId);
    
    HRESULT hr = CaptureTexture(gD3D11->GetDevice(),::gD3D11->GetImmediateContext(),
        (ID3D11Resource*)renderSurface->GetColorBuffer()->GetTex(),scratchImg);
    if(FAILED(hr)) return false;
        
    const DirectX::Image* img = scratchImg.GetImage(0,0,0);

    ImageData imgdata;
    imgdata.InitFrom(img);
    imgdata.SaveToFile(fileName);   
    return SUCCEEDED(hr);
}


LVEDRENDERINGENGINE_API ObjectGUID __stdcall LvEd_CreateVertexBuffer(VertexFormatEnum vf, void* buffer, uint32_t vertexCount)
{
    ErrorHandler::ClearError();
    return s_engineData->basicRenderer->CreateVertexBuffer(vf,buffer,vertexCount);
}

// ---------------------------------------------------------------------------------------------------------
// Create index buffer from user data.
LVEDRENDERINGENGINE_API ObjectGUID __stdcall LvEd_CreateIndexBuffer(uint32_t* buffer, uint32_t indexCount)
{
    ErrorHandler::ClearError();
    return s_engineData->basicRenderer->CreateIndexBuffer(buffer,indexCount);
}


// ---------------------------------------------------------------------------------------------------------
LVEDRENDERINGENGINE_API void __stdcall LvEd_DeleteBuffer(ObjectGUID buffer)
{
    ErrorHandler::ClearError();
    s_engineData->basicRenderer->DeleteBuffer(buffer);
}


LVEDRENDERINGENGINE_API void __stdcall LvEd_SetRendererFlag(BasicRendererFlagsEnum renderFlags)
{
    s_engineData->basicRenderer->SetRendererFlag(renderFlags);
}


// ---------------------------------------------------------------------------------------------------------
LVEDRENDERINGENGINE_API void __stdcall LvEd_DrawPrimitive(PrimitiveTypeEnum pt,
                                                    ObjectGUID vb,
                                                    uint32_t StartVertex,
                                                    uint32_t vertexCount,
                                                    float* color,
                                                    float* xform)                                                    
{
    ErrorHandler::ClearError();
    s_engineData->basicRenderer->DrawPrimitive(pt,vb,StartVertex, vertexCount,color,xform);
}

// ---------------------------------------------------------------------------------------------------------
LVEDRENDERINGENGINE_API void __stdcall LvEd_DrawIndexedPrimitive(PrimitiveTypeEnum pt,
                                                                ObjectGUID vb,
                                                                ObjectGUID ib,
                                                                uint32_t startIndex,
                                                                uint32_t indexCount,
                                                                uint32_t startVertex,
                                                                float* color,
                                                                float* xform)
{
    ErrorHandler::ClearError();
    s_engineData->basicRenderer->DrawIndexedPrimitive(pt,vb,ib,startIndex,indexCount,startVertex,color,xform);
}

// ---------------------------------------------------------------------------------------------------------
LVEDRENDERINGENGINE_API ObjectGUID LvEd_CreateFont(WCHAR* fontName, float pixelHeight, LvEdFonts::FontStyleFlags fontStyles )
{
    ErrorHandler::ClearError();
    return (ObjectGUID)LvEdFonts::Font::CreateNewInstance( gD3D11->GetDevice(), fontName, pixelHeight, fontStyles );
}

LVEDRENDERINGENGINE_API void __stdcall LvEd_DeleteFont(ObjectGUID font)
{
    ErrorHandler::ClearError();
    using namespace LvEdFonts;
    Font* pFont = reinterpret_cast<Font*>(font);
    delete pFont;
}

// Draw text in screen space.
LVEDRENDERINGENGINE_API void LvEd_DrawText2D(ObjectGUID font, WCHAR* text, int x, int y, int color)
{
    ErrorHandler::ClearError();
    using namespace LvEdFonts;    
    Font* pFont = reinterpret_cast<Font*>(font);

    float4 colorRGBA;
    ConvertColor( color, &colorRGBA );
    FontRenderer::Inst()->DrawText( pFont, text, x, y, colorRGBA );
}

//===============================================================================
// Error Handling
//===============================================================================

LVEDRENDERINGENGINE_API int __stdcall LvEd_GetLastError(const wchar_t ** errorText)
{
    ErrorDescription * errorDescription = ErrorHandler::GetError();
    *errorText = errorDescription->errorText;
    return (int)errorDescription->errorType;
}



//==========================================
// Create xml document to hold 
// Engine information.
// The data is passed to C# side.
//=========================================


#include "rapidxml-1.13\rapidxml.hpp"
#include "rapidxml-1.13\rapidxml_print.hpp"
typedef rapidxml::xml_document<wchar_t> XmlDocument;
typedef rapidxml::xml_node<wchar_t> XmlNode;
typedef rapidxml::xml_attribute<wchar_t> XmlAttribute;


// create node for resource type
void AddResNode(XmlDocument& doc,
    XmlNode* parent,
    const wchar_t* type,
    const wchar_t* name,
    const wchar_t* description,
    const wchar_t* fileExt)
{   
    XmlNode* resnode = doc.allocate_node(rapidxml::node_element,L"ResourceDescriptor");
    resnode->append_attribute(doc.allocate_attribute(L"Type", type));
    resnode->append_attribute(doc.allocate_attribute(L"Name", name));
    resnode->append_attribute(doc.allocate_attribute(L"Description", description));
    resnode->append_attribute(doc.allocate_attribute(L"Ext", fileExt));
    parent->append_node(resnode);
}


//using namespace rapidxml;
EngineInfo::EngineInfo()
{       
    XmlDocument doc;
    XmlNode* decl = doc.allocate_node(rapidxml::node_declaration);
    decl->append_attribute(doc.allocate_attribute(L"version", L"1.0"));
    decl->append_attribute(doc.allocate_attribute(L"encoding", L"utf-8"));
    decl->append_attribute(doc.allocate_attribute(L"standalone", L"yes"));
    doc.append_node(decl);
    XmlNode* root = doc.allocate_node(rapidxml::node_element, L"EngineInfo");
    root->append_attribute(doc.allocate_attribute(L"version", L"1.0"));
    doc.append_node(root);

    XmlNode* resNodes = doc.allocate_node(rapidxml::node_element,L"SupportedResources");
    root->append_node(resNodes);
        
    // add supported 3d models.
    const wchar_t* modeltype = ResourceType::ToWString(ResourceType::Model);
    AddResNode(doc,resNodes,modeltype,L"Model",L"3d model",L".atgi,.dae");
        
    // add supported textures
    const wchar_t* textureType = ResourceType::ToWString(ResourceType::Texture);
    AddResNode(doc,resNodes,textureType,L"Texture",L"Texture file",L".dds,.bmp,.jpg,.png,.tga,.tif");

    // Add any other engine information 

    // print to string.
    rapidxml::print(back_inserter(m_data), doc, 0);   
}
