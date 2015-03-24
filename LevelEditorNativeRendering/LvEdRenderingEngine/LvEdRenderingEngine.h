//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/**
 * @file LvEdRenderingEngine.h
 * @version 3.5
 *
 * @copyright (C) 2012 Sony Computer Entertainment Worldwide Studios. All Rights Reserved. 
 *
 * @section DESCRIPTION
 *
 * This file describes a C API that provides a bridge between C++ code and 
 * C# code for native game-object renderers.
 *
 * This file defines the API for C++ code.  For C# code, this API is used by the 
 * wws_leveleditor\LevelEditorNativeRendering\NativeInterop\GameEngine.cs file.
 */

#pragma once

#ifdef LVEDRENDERINGENGINE_EXPORTS
#define LVEDRENDERINGENGINE_API __declspec(dllexport)
#else
#define LVEDRENDERINGENGINE_API __declspec(dllimport)
#endif

#include "Core/WinHeaders.h"
#include "Renderer/FontTypes.h"
#include "Core/typedefs.h"
#include "Renderer/RenderEnums.h"
#include "FrameTime.h"
#include <stdint.h>


// forward declarations
struct HitRecord;
namespace LvEdEngine
{
    class RenderSurface;
    class Ray;
}

using namespace LvEdEngine;



typedef void (__stdcall * InvalidateViewsCallbackType)(void);

//==============================================================================
// Initialization and Shutdown Functions
//==============================================================================

/**
 * Initializes the game-rendering engine.
 *
 * LevelEditor calls this function once during startup.
 * 
 * @param logCallback call back for logging.
 * @param invalidateCallback call back used for notifying LevelEditor that
 *        the views need to be redrawn.
 * @outEngineInfo: Engine information 
 *
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_Initialize(LogCallbackType logCallback,
    InvalidateViewsCallbackType invalidateCallback, 
    const wchar_t** outEngineInfo);


/**
 * Shuts down the game-rendering engine.
 *
 * LevelEditor calls this function during normal shutdown.
 *
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_Shutdown(void);

/**
 * Clears the game world.
 *
 * Performs the following tasks:
 *   1. Deletes all the game objects.
 *   2. Resets all the world properties.
 *
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_Clear();  


//===============================================================================
// Object-Management Functions
//
// General Overview:
//
//  -   Each type shared with the client application has a "type ID", which the
//      client can obtain by calling LvEd_GetObjectTypeId().
//
//  -   The type ID is passed to LvEd_CreateObject() to create an instance of
//      that type, which is returned to the client as an opaque ObjectGUID.
//
//  -   Properties of a shared class have ObjectPropertyUIDs that can be obtained
//      from LvEd_GetObjectPropertyId().
//
//  -   The client code sets and gets object properties by passing the instance
//      ObjectGUID and ObjectPropertyUID to LvEd_SetObjectProperty() and to 
//      LvEd_GetObjectProperty().
//
//===============================================================================


/**
 * Gets type ID for the given class name.
 *
 * @param className Class name
 *
 * @return Type GUID, or zero the if className is not defined
 *
 */
extern "C" LVEDRENDERINGENGINE_API ObjectTypeGUID __stdcall LvEd_GetObjectTypeId(char* className);


/**
 * Gets the property ID for the specified type and property name.
 *
 * @param id Type GUID
 * @param propertyName property name
 *
 * @return Property UID, or zero if not found
 *
 */
extern "C" LVEDRENDERINGENGINE_API ObjectPropertyUID __stdcall LvEd_GetObjectPropertyId(ObjectTypeGUID id, char* propertyName);


/**
 * Gets the list ID for the specified type and list name.
 *
 * @param id Type GUID
 * @param listName list name
 *
 * @return List UID, or zero if not found
 *
 */
extern "C" LVEDRENDERINGENGINE_API ObjectPropertyUID __stdcall LvEd_GetObjectChildListId(ObjectTypeGUID id, char* listName);


/**
 * Creates a new instance of the specified object type.
 *
 * @param typeId Type GUID of the object
 * @param data (Optional) Object's factory; you can use data to initialize the object
 * @param size Size of data, in bytes
 *
 * @return Instance GUID of the object, or zero if it failed to create the object
 *
 */
extern "C" LVEDRENDERINGENGINE_API ObjectGUID  __stdcall LvEd_CreateObject(ObjectTypeGUID typeId, void* data, int size);


/**
 * Deletes the specified object.
 *
 * @param typeId Type GUID of the object
 * @param instanceId Instance GUID of the object
 *
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_DestroyObject(ObjectTypeGUID typeId, ObjectGUID instanceId);




/**
 * Invoke member function of the specified object.
 *
 
 * @param instanceId Instance GUID of the object
 * @param fn  function name.
 * @param arg function arguments.
 * @param retVal return value.
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_InvokeMemberFn(ObjectGUID instanceId, wchar_t* fn, const void* arg, void** retVal);



/**
 * Sets a property for the specified object and property ID.
 *
 * @param typeId Type GUID of the parent object
 * @param propId The property UID to be set
 * @param instanceId Instance GUID of the object
 * @param data Data to be set
 * @param size Size of data, in bytes
 *
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_SetObjectProperty(ObjectTypeGUID typeId, ObjectPropertyUID propId, ObjectGUID instanceId, void* data, int size);


/**
 * Gets a property for the specified object and property ID.
 *
 * @param typeId Type GUID of the object
 * @param propId The property UID to get
 * @param instanceId Instance GUID of the object
 * @param data[out] Pointer to data to be set
 * @param size[out] Pointer to size of data, in bytes
 *
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_GetObjectProperty(ObjectTypeGUID typeId, ObjectPropertyUID propId, ObjectGUID instanceId, void** data, int* size);


/**
 * Adds the specified child object to its parent under the specified list at the specified index.
 *
 * @param typeId Type GUID of the parent object
 * @param listId List UID for the list to which the child will be added
 * @param parentId Instance GUID of the parent object
 * @param childId Instance GUID of the child object
 * @param index: Insertion index
 *
 * @remark If index == -1, this function adds the child at the end of the specified list.
 *         If index ==  0, this function adds the child at the front of the specified list.
 *         If index  >  0, this function inserts the child at the specified index.
 *
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_ObjectAddChild(ObjectTypeGUID typeId, ObjectListUID listId, ObjectGUID parentId, ObjectGUID childId, int index);


/**
 * Removes the specified object from its parent within the specified list ID.
 *
 * @param typeId Type GUID of the parent object
 * @param listId List UID for the list to which the child will be removed
 * @param parentId Instance GUID of the parent object
 * @param childId Instance GUID of the child object
 *
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_ObjectRemoveChild(ObjectTypeGUID typeId, ObjectListUID listId, ObjectGUID parentId, ObjectGUID childId);


//===============================================================================
// Picking and Selection Functions
//===============================================================================


/**
 * Selects (picks) the specified ray.
 *
 * @param renderSurface Instance GUID of the render surface
 * @param viewxform View transform
 * @param projxform Projection of the transform
 * @param rayW Picking ray in world space
 * @param hits An array of HitRecord 
 * @param count Number of picked objects
 *
 * @remark The HitRecords are sorted along the ray.
 *
 * @return TRUE if one or more objects picked, FALSE otherwise
 *
 */
extern "C" LVEDRENDERINGENGINE_API bool __stdcall LvEd_RayPick(float viewxform[], float projxform[],Ray* rayW, bool skipSelected, HitRecord** hits, int* count);


/**
 * Selects (picks) the specified frustum.
 *
 * @param renderSurface ObjectGUID of the render surface (an instance of type "SwapChain")
 * @param viewxform View transform
 * @param projxform Projection of the transform
 * @param rect Picking rectangle in screen space
 * @param hits An array of HitRecord 
 * @param count Number of picked objects
 *
 * @return TRUE if one or more objects picked, FALSE otherwise
 *
 */
extern "C" LVEDRENDERINGENGINE_API bool __stdcall LvEd_FrustumPick(ObjectGUID renderSurface, float viewxform[], float projxform[],float* rect, HitRecord** hits, int* count);


/**
 * Sets the selection.
 *
 * The level editor calls this function to set selected game objects.
 *
 * @param instanceIds Instance GUIDs of the objects to be selected
 * @param count Number of objects selected
 *
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_SetSelection(ObjectGUID*  instanceIds, int count);


//===============================================================================
// Update and Rendering Functions
//===============================================================================


/**
 * Sets the global render state.
 *
 * The level editor uses this function for setting render states, such as 
 * texture on/off or lighting on/off.
 *
 * @param instId ObjectGUID of the render state (an instance of type "RenderState")
 *
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_SetRenderState(ObjectGUID instId);


/**
 * Sets root level (master level)
 * @param instId the Id of an instance of type class GameLevel
 */ 
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_SetGameLevel(ObjectGUID instId);

/**
 * Gets the instance id of last set game level see LvEd_SetGameLevel
 * @return Instance GUID of game level or zero if 
 */ 
extern "C" LVEDRENDERINGENGINE_API ObjectGUID __stdcall LvEd_GetGameLevel();

/**
* Wait until all the pending resources are loaded.
*/
extern "C" LVEDRENDERINGENGINE_API void  __stdcall LvEd_WaitForPendingResources();


/**
 * Updates the game world.
 *
 * LevelEditor calls this function for each frame.
 *
 * @param t Total time since LevelEditor start, in seconds
 * @param dt Delta time since last update 
 *
 */
 extern "C" LVEDRENDERINGENGINE_API void  __stdcall LvEd_Update(FrameTime* ft, UpdateTypeEnum updateType);



/**
 * Begin rendering.
 *
 * LevelEditor calls this function for each view in the quad view panel.
 *
 * @param renderSurface ObjectGUID of the render surface
 * @param viewxform View transform
 * @param projxform Projection of the transform 
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_Begin(ObjectGUID renderSurface, float viewxform[], float projxform[]);


/**
 * End rendering.
 *
 * Presents active render surface if it is a swapchain.
 *
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_End();


/**
 * Renders the game world.
 *
 * This function renders the entire game world, after which the LevelEditor 
 * can do additional rendering before calling LvEd_End.
 *
 * This function is called per frame per view in the quad view panel.
 *
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_RenderGame();

/**
 * Saves render surface to the given file path.
 * if the file exit it will be overwritten.
 * @param renderSurface ObjectGUID of the render surface
 * @param fileName  absolute file path.
 * @return true if the operation successful otherwise false. 
 */
extern "C" LVEDRENDERINGENGINE_API bool __stdcall LvEd_SaveRenderSurfaceToFile(ObjectGUID renderSurfaceId, wchar_t *fileName);



//=============================================================================
// LevelEditor Low-Level Rendering Functions
//==============================================================================


/**
 * Creates a vertex buffer from user data with the specified vertex format.
 *
 * @param vf Vertex format
 * @param buffer User data
 * @param vertexCount: Vertex count
 *
 * @return Instance GUID of vertex buffer
 *
 */
extern "C" LVEDRENDERINGENGINE_API ObjectGUID __stdcall LvEd_CreateVertexBuffer(VertexFormatEnum vf, void* buffer, uint32_t vertexCount);


/**
 * Creates an index buffer from user data.
 *
 * @param buffer User data
 * @param indexCount Index count
 *
 * @return Instance GUID of index buffer
 *
 */
extern "C" LVEDRENDERINGENGINE_API ObjectGUID __stdcall LvEd_CreateIndexBuffer(uint32_t* buffer, uint32_t indexCount);


/**
 * Deletes the specified index buffer or vertex buffer.
 *
 * @param buffer Instance GUID of index/vertex buffer
 *
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_DeleteBuffer(ObjectGUID buffer);



/**
* Set render flag
* @param renderFlags basic rendering flags
*/
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_SetRendererFlag(BasicRendererFlagsEnum renderFlags);

/**
 * Draws the specified primitive with the specified parameters.
 *
 * @param pt Primitive type
 * @param vb Instance GUID of a vertex buffer
 * @param StartVertex Starting vertex
 * @param vertexCount Vertex count
 * @param color Color component array (Red, Green, Blue, Alpha)
 * @param xform View transform 
 *
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_DrawPrimitive(PrimitiveTypeEnum pt,                                                                 
                                                                ObjectGUID vb, 
                                                                uint32_t StartVertex,
                                                                uint32_t vertexCount,
                                                                float* color,
                                                                float* xform);


/**
 * Draws the specified indexed primitive with the specified parameters.
 *
 * @param pt Primitive type
 * @param vb Instance GUID of a vertex buffer
 * @param ib Instance GUID of an index buffer
 * @param startIndex Starting index
 * @param indexCount Index count
 * @param startVertex Starting vertex
 * @param color Color component array (Red, Green, Blue, Alpha)
 * @param xform View transform 
 *
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_DrawIndexedPrimitive(PrimitiveTypeEnum pt,                                                             
                                                                    ObjectGUID vb, 
                                                                    ObjectGUID ib,
                                                                    uint32_t startIndex,
                                                                    uint32_t indexCount,
                                                                    uint32_t startVertex,                        
                                                                    float* color,
                                                                    float* xform);
                                                                    


//=============================================================================
// LevelEditor Font Support Functions
//==============================================================================


/**
 * Creates a font object.
 *
 * @param fontName Font name
 * @param pixelHeight Font height, in pixels
 * @param fontStyles Font style flags for styles desired
 *
 * @return Instance GUID of font, or NULL if failed to create font
 *
 */
extern "C" LVEDRENDERINGENGINE_API ObjectGUID LvEd_CreateFont(WCHAR* fontName, float pixelHeight, LvEdEngine::LvEdFonts::FontStyleFlags fontStyles );


/**
 * Deletes a font object.
 *
 * @param font Instance GUID of font
 *
 */
extern "C" LVEDRENDERINGENGINE_API void __stdcall LvEd_DeleteFont(ObjectGUID font);


/**
 * Draws specified text in screen space.
 *
 * @param font Instance GUID of font
 * @param text Text to draw
 * @param x Top x-coordinate of starting text position
 * @param y Top y-coordinate of starting text position
 * @param color Color of the text
 *
 */
extern "C" LVEDRENDERINGENGINE_API void LvEd_DrawText2D(ObjectGUID font, WCHAR* text, int x, int y, int color);


/**
 * Gets the last error type.  Error results on each API call, but is thread specific
 *
 * @param errorText pointer to error (should used or copied before next API call)
 * @return error type of last error that occured
 */
extern "C" LVEDRENDERINGENGINE_API int __stdcall LvEd_GetLastError(const wchar_t ** errorText);

