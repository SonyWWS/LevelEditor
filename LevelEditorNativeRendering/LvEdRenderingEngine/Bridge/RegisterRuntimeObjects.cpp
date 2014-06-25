//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "RegisterRuntimeObjects.h"
#include "../Renderer/RenderState.h"
#include "../Renderer/SwapChain.h"
#include "../Renderer/DeviceManager.h"
#include "../Renderer/TextureRenderSurface.h"
#include "../Core/ImageData.h"
#include "../DirectX/DXUtil.h"
#include "../DirectX/DirectXTex/DirectXTex.h"

using namespace DirectX;

// disable warning C4100 unreferenced formal parameter for this file
#pragma warning(push)
#pragma warning(disable: 4100)

namespace LvEdEngine
{

    namespace SwapChainBridge
    {        
        static Object* CreateIntance(ObjectTypeGUID tid, void *data, int size)
        {
            HWND hwnd = (HWND)data;
            assert(size == sizeof(hwnd));
            SwapChain *swap = new SwapChain(
                hwnd,
                gD3D11->GetDevice(),
                gD3D11->GetDXGIFactory1());

            return swap;
        }

        static void SetSize(ObjectGUID instanceId, void* data, int size)
        {
            assert(size == sizeof(SIZE));
            SwapChain* instance = reinterpret_cast<SwapChain*>(instanceId);
            SIZE s= *((SIZE*)data);         
            instance->Resize(s.cx,s.cy);
        }


        static void SetBkgColor(ObjectGUID instanceId, void* data, int size)
        {
            assert(size == sizeof(float4));
            SwapChain* instance = reinterpret_cast<SwapChain*>(instanceId);
            float4 color = (float*)data;            
            instance->SetBkgColor(color);
        }
    }

    namespace RenderStateBridge
    {
        static Object* CreateIntance(ObjectTypeGUID tid, void* data, int size)
        {            
            return new RenderState();
        }

        static void SetGlobalRenderFlags(ObjectGUID instanceId, void* data, int size)
        {
            assert(sizeof(GlobalRenderFlagsEnum) == size);
            RenderState* instance = reinterpret_cast<RenderState*>(instanceId);
            GlobalRenderFlagsEnum rf = *((GlobalRenderFlagsEnum*)data);
            instance->SetGlobalRenderFlags(rf);            
        }

        static void SetWireColor(ObjectGUID instanceId, void* data, int size)
        {
            assert(sizeof(float4) == size);
            RenderState* instance = reinterpret_cast<RenderState*>(instanceId);
            float4 color = (float*)data;
            instance->SetWireframeColor(color);        
        }

        static void SetSelectionColor(ObjectGUID instanceId, void* data, int size)
        {
            assert(sizeof(float4) == size);
            RenderState* instance = reinterpret_cast<RenderState*>(instanceId);
            float4 color = (float*)data;
            instance->SetSelectionColor(color);
        }

    }
   

    namespace TextureRenderSurfaceBridge
    {
        static Object* CreateIntance(ObjectTypeGUID tid, void *data, int size)
        {
            assert(size == sizeof(SIZE));

            SIZE s= *((SIZE*)data);         

            TextureRenderSurface* surface = new TextureRenderSurface(
                gD3D11->GetDevice(),
                s.cx,
                s.cy,
                true,
                true,
                DXGI_FORMAT_R8G8B8A8_UNORM,
                4,
                4);
            return surface;            
        }
       
        static void SetBkgColor(ObjectGUID instanceId, void* data, int size)
        {
            assert(size == sizeof(float4));
            TextureRenderSurface* instance = reinterpret_cast<TextureRenderSurface*>(instanceId);
            float4 color = (float*)data;            
            instance->SetBkgColor(color);
        }
    }

    namespace ImageDataBridge
    {
       
        static Object* CreateIntance(ObjectTypeGUID tid, void *data, int size)
        {
            return new ImageData();
        }

        void Convert(ObjectGUID instanceId, void* data, int size)
        {
            assert(size == sizeof(uint32_t));
            assert(instanceId);
            if(instanceId == 0) return;

            ImageData* instance = reinterpret_cast<ImageData*>(instanceId);
            uint32_t format = *((uint32_t*)data);
            instance->Convert(format);
        }
        void LoadFromFile(ObjectGUID instanceId, void* data, int size)
        {
            wchar_t* path = reinterpret_cast<wchar_t*>(data);
            int sz = (int) (wcslen(path) * 2);
            assert(size == sz);
            if(!path || sz == 0 || instanceId == 0) return;

            ImageData* instance = reinterpret_cast<ImageData*>(instanceId);
            instance->LoadFromFile(path);
        }

        void GetBufferPointer(ObjectGUID instanceId, void** data, int* size)
        {
            ImageData* instance = reinterpret_cast<ImageData*>(instanceId);
            *data = instance->GetBufferPointer();
            *size = sizeof(intptr_t);
        }

        void GetBufferSize(ObjectGUID instanceId, void** data, int* size)
        {
            ImageData* instance = reinterpret_cast<ImageData*>(instanceId);
            static size_t localData;
            localData = instance->GetBufferSize();
            *data = (void*)&localData;
            *size = sizeof(localData);    
        }

        void GetWidth(ObjectGUID instanceId, void** data, int* size)
        {
            ImageData* instance = reinterpret_cast<ImageData*>(instanceId);
            static size_t localData;
            localData = instance->GetWidth();
            *data = (void*)&localData;
            *size = sizeof(localData);

        }

        void GetHeight(ObjectGUID instanceId, void** data, int* size)
        {
            ImageData* instance = reinterpret_cast<ImageData*>(instanceId);
            static size_t localData;
            localData = instance->GetHeight();
            *data = (void*)&localData;
            *size = sizeof(localData);

        }

        void GetRowPitch(ObjectGUID instanceId, void** data, int* size)
        {
            ImageData* instance = reinterpret_cast<ImageData*>(instanceId);
            static size_t localData;
            localData = instance->GetRowPitch();
            *data = (void*)&localData;
            *size = sizeof(localData);

        }

        void GetBytesPerPixel(ObjectGUID instanceId, void** data, int* size)
        {
            ImageData* instance = reinterpret_cast<ImageData*>(instanceId);
            static size_t localData;
            localData = instance->GetBytesPerPixel();
            *data = (void*)&localData;
            *size = sizeof(localData);
        }

        void GetFormat(ObjectGUID instanceId, void** data, int* size)
        {
            ImageData* instance = reinterpret_cast<ImageData*>(instanceId);
            static uint32_t localData;
            localData = instance->GetFormat();
            *data = (void*)&localData;
            *size = sizeof(localData);
        }
    }
    // register object that exist in runtime and C# side
    // and not defined in schema.    
    void RegisterRuntimeObjects( GobBridge& bridge)
    {
        // swapchain ------------------------------
        bridge.RegisterObject(SwapChain::StaticClassName(), &SwapChainBridge::CreateIntance);
        bridge.RegisterProperty(SwapChain::StaticClassName(), "Size", &SwapChainBridge::SetSize, NULL);
        bridge.RegisterProperty(SwapChain::StaticClassName(), "BkgColor", &SwapChainBridge::SetBkgColor, NULL);

        // TextureRenderSurface
        bridge.RegisterObject(TextureRenderSurface::StaticClassName(), &TextureRenderSurfaceBridge::CreateIntance);        
        bridge.RegisterProperty(TextureRenderSurface::StaticClassName(), "BkgColor", &TextureRenderSurfaceBridge::SetBkgColor, NULL);


        // RenderState.
        bridge.RegisterObject(RenderState::StaticClassName(),RenderStateBridge::CreateIntance);
        bridge.RegisterProperty(RenderState::StaticClassName(),"GlobalRenderFlags",RenderStateBridge::SetGlobalRenderFlags, NULL);
        bridge.RegisterProperty(RenderState::StaticClassName(),"WireframeColor",RenderStateBridge::SetWireColor, NULL);
        bridge.RegisterProperty(RenderState::StaticClassName(),"SelectionColor",RenderStateBridge::SetSelectionColor, NULL);

        // ImageDataBridg
        // 
        bridge.RegisterObject(ImageData::StaticClassName(),ImageDataBridge::CreateIntance);
        bridge.RegisterProperty(ImageData::StaticClassName(),"LoadFromFile",ImageDataBridge::LoadFromFile,NULL);
        bridge.RegisterProperty(ImageData::StaticClassName(),"Convert",ImageDataBridge::Convert,NULL);
        bridge.RegisterProperty(ImageData::StaticClassName(),"BufferPointer",NULL,ImageDataBridge::GetBufferPointer);        
        bridge.RegisterProperty(ImageData::StaticClassName(),"BufferSize",NULL,ImageDataBridge::GetBufferSize);
        bridge.RegisterProperty(ImageData::StaticClassName(),"Width",NULL,ImageDataBridge::GetWidth);
        bridge.RegisterProperty(ImageData::StaticClassName(),"Height",NULL,ImageDataBridge::GetHeight);
        bridge.RegisterProperty(ImageData::StaticClassName(),"RowPitch",NULL,ImageDataBridge::GetRowPitch);
        bridge.RegisterProperty(ImageData::StaticClassName(),"BytesPerPixel",NULL,ImageDataBridge::GetBytesPerPixel);                
        bridge.RegisterProperty(ImageData::StaticClassName(),"Format",NULL,ImageDataBridge::GetFormat);                
    }
};

#pragma warning(pop)