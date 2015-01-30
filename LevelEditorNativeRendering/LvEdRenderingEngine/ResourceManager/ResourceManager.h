//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <string>
#include <map>
#include <vector>
#include "../Core/WinHeaders.h"
#include "../Core/NonCopyable.h"


namespace LvEdEngine
{
    class Resource;
    class ResourceManager;
    class ResourceFactory;


    //--------------------------------------------------
    class ResourceFactory : public NonCopyable
    {
    public:
        virtual Resource* CreateResource(Resource* def)=0;
        virtual bool LoadResource(Resource* resource, const WCHAR* name)=0;
    };

    // ----------------------------------------------------------------------------
    class ResourceListener : public NonCopyable
    {
    public:
        virtual void OnResourceLoaded(Resource* r)=0;
    };


    // ----------------------------------------------------------------------------
    class ResourceManager : public NonCopyable
    {
    public:
        static void             InitInstance();
        static void             DestroyInstance(void);
        static ResourceManager* Inst() { return s_Inst; }

        // factory registration which takes ownership of the factory
        void RegisterFactory(const WCHAR* ext, ResourceFactory* factory);
        void RegisterListener(ResourceListener* listener);

        // loading : Resource* will never be null.
        // Note: dont ever delete resources, only release them.
        Resource* LoadAsync(const WCHAR* filename, Resource* def);
        Resource* LoadImmediate(const WCHAR* filename, Resource* def);

        // wait until all the pending resources loaded.
        void WaitOnPending();
        
        int GarbageCollect();

    private:
        ResourceManager();
        ~ResourceManager();

        typedef std::map<std::wstring, Resource*> ResourceInfoMap;
        bool LoadResource(Resource* r, const WCHAR* filename);
        ResourceFactory * GetFactory(const WCHAR* filename);
        
        ResourceInfoMap m_loaded;
        ResourceInfoMap m_pending;
        std::map<std::wstring,ResourceFactory*> m_factories;
        std::vector<ResourceListener*> m_listeners;

        static ResourceManager * s_Inst;
        static DWORD WINAPI ThreadProc (void* user);       // this is our async load thread

        CRITICAL_SECTION m_criticalSection;  // used for thread synchronization.
        HANDLE m_ThreadHandle;
        HANDLE m_EventHandle;
        volatile bool m_exitRequested;


    };

}; // namespace LvEdEngine
