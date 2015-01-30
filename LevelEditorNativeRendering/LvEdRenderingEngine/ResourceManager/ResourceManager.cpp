//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
#include "ResourceManager.h"
#include <vector>
#include <string>
#include <algorithm>
#include <cassert>
#include <functional>
#include <process.h>
#include <d3d11.h>
#include <D3Dcompiler.h>
#include "../Core/NonCopyable.h"
#include "../Core/typedefs.h"
#include "../Core/WinHeaders.h"
#include <WinBase.h>
#include "../Core/PerfTimer.h"
#include "../Core/Utils.h"
#include "../Core/FileUtils.h"
#include "../Core/Logger.h"
#include "../Renderer/RenderEnums.h"
#include "../Renderer/RenderUtil.h"
#include "../Renderer/Resource.h"


namespace LvEdEngine
{



ResourceManager * ResourceManager::s_Inst = NULL;

// ----------------------------------------------------------------------------------------------
class AutoSync : public NonCopyable
{
public:
    AutoSync(const PRTL_CRITICAL_SECTION pcriticalSection)
    {
        m_criticalSection  = pcriticalSection;
        EnterCriticalSection(m_criticalSection);
    }
    ~AutoSync()
    {
        LeaveCriticalSection(m_criticalSection);
    }

private:
    PRTL_CRITICAL_SECTION m_criticalSection;
};

// ----------------------------------------------------------------------------------------------
// This is the async thread. It will continue to check for pending resources to load until s_exitRequested
// is set to true, at which point it will exit.
DWORD WINAPI ResourceManager::ThreadProc (void* arg)
{
    ResourceManager* mgr = (ResourceManager*)arg;

    while(!mgr->m_exitRequested)
    {
        WaitForSingleObject(mgr->m_EventHandle, INFINITE);
        if(mgr->m_exitRequested)
        {
            break;
        }
        std::wstring filename;
        Resource * res = NULL;
        bool pending = true;
        while(pending)
        {

            { // CRITICAL SECTION - BEGIN
                AutoSync sync(&mgr->m_criticalSection);
                // just grab the 1st one from the map, not worrying about queue order or anything like that.
                ResourceInfoMap::iterator it = mgr->m_pending.begin();
                if(it == mgr->m_pending.end() )
                {
                    pending = false;
                    continue;                    
                }

                assert(it != mgr->m_pending.end() );

                // make a copy on purpose since we are going to leave the critical section very soon.
                filename = it->first;

                // we can use the info pointer here because the info must not be deleted
                // clients have access to the pointer. So, all we are going to do is move the
                // pointer from the pending map to the loaded map once we load the underlying resource.
                res = it->second;
            } // CRITICAL SECTION - END

            // do the actual 'load' here.....this could take some time....better not be in a critical section
            mgr->LoadResource(res, filename.c_str());


            { // CRITICAL SECTION - BEGIN
                AutoSync sync(&mgr->m_criticalSection);
                mgr->m_loaded[filename] = res;
                mgr->m_pending.erase(filename);   //imporntant to clear pending only after added to loading.
            } // CRITICAL SECTION - END

            for(auto it = mgr->m_listeners.begin(); it != mgr->m_listeners.end(); ++it)
            {
                ResourceListener * listener = (*it);
                listener->OnResourceLoaded(res);
            }

        }
    }
    return 0;
}


// ----------------------------------------------------------------------------------------------
//static 
void ResourceManager::InitInstance()
{
    assert(s_Inst == NULL);
    if(s_Inst) return;
    s_Inst = new ResourceManager();
   
}

// ----------------------------------------------------------------------------------------------
//static 
void ResourceManager::DestroyInstance()
{    
    assert(s_Inst);     
    SAFE_DELETE(s_Inst);
}

// ----------------------------------------------------------------------------------------------
ResourceManager::ResourceManager()
{
    
    m_exitRequested = false;

    InitializeCriticalSection(&m_criticalSection);
    m_EventHandle = CreateEvent(NULL, false, false, NULL);
    m_ThreadHandle = CreateThread(NULL, 0, &ResourceManager::ThreadProc, this, 0, NULL);
    SetThreadPriority(m_ThreadHandle, THREAD_PRIORITY_NORMAL);   
}

// ----------------------------------------------------------------------------------------------
ResourceManager::~ResourceManager()
{
    AutoSync sync(&m_criticalSection);
    m_exitRequested = true;
    SetEvent(m_EventHandle);
    WaitForSingleObject(m_ThreadHandle, INFINITE);
    DeleteCriticalSection(&m_criticalSection);
    CloseHandle(m_EventHandle);

    // delete loaded resources.
    for(auto it = m_loaded.begin(); it != m_loaded.end(); ++it)
    {
        delete it->second;
    }

    
    // delete factories only once (since they could be registered multiple times)
    std::vector<ResourceFactory*> deletedFactories;
    for(auto it = m_factories.begin(); it != m_factories.end(); ++it)
    {
        ResourceFactory* resFact = it->second;
        if (std::find(deletedFactories.begin(), deletedFactories.end(), resFact) == deletedFactories.end()) 
        {
            deletedFactories.push_back(resFact);
            delete resFact;
        }
    }
}

// factory registration
// factories create resources based on file extensions.
// ----------------------------------------------------------------------------------------------
void ResourceManager::RegisterFactory(const WCHAR* ext, ResourceFactory * factory)
{
    m_factories[ext] = factory;
}

// ----------------------------------------------------------------------------------------------
void ResourceManager::RegisterListener(ResourceListener* listener)
{
    m_listeners.push_back(listener);
}

// ----------------------------------------------------------------------------------------------
ResourceFactory * ResourceManager::GetFactory(const WCHAR* filename)
{
    std::wstring ext = FileUtils::GetExtensionLower(filename);

    ResourceFactory * factory = NULL;
    auto it  = m_factories.find(ext);
    if(it != m_factories.end())
    {
    factory = it->second;
    }
    if (!factory)
    {
        Logger::Log(OutputMessageType::Error, L"No resource factory found for file, '%s'\n", filename);
    }
    return factory;
}

// loading/unloading
// ----------------------------------------------------------------------------------------------
// Loads a file and turns it into a runtime resource
bool ResourceManager::LoadResource(Resource * res,  const WCHAR* filename)
{
    bool ok = false;
    // try to create resource
    if (!FileUtils::Exists(filename))
    {
        Logger::Log(OutputMessageType::Error, L"Failed to load file, '%ls' -- does not exist\n", filename);
        return false;
    }

    PerfTimer timer;
    timer.Start();

    // get factory associated with 'filename'
    ResourceFactory * factory = GetFactory(filename);
    if (!factory)
    {
        return false;
    }


    ok = factory->LoadResource(res, filename);
    if (ok)
    {
        res->SetReady();
        timer.Stop();
        Logger::Log(OutputMessageType::Debug, L"%d ms Loaded %ls\n", timer.ElapsedMilliseconds(), FileUtils::Name(filename));
    }
    else
    {
        timer.Stop();
        Logger::Log(OutputMessageType::Error, L"%d ms failed to load %ls\n", timer.ElapsedMilliseconds(), filename);
    }

    return ok;
}

// ----------------------------------------------------------------------------------------------
// this function must *always* return a valid Resource, even for 'missing' resources.
Resource* ResourceManager::LoadAsync(const WCHAR* filename, Resource* def)
{
    AutoSync sync(&m_criticalSection); // CRITICAL SECTION  - ENTIRE FUNCTION
    Resource * res = NULL;
    // check cache, use if already there.
    if(NULL == res)
    {
        auto it = m_loaded.find(filename);
        if(it != m_loaded.end() ) res = it->second;
    }

    // check pending, use if already there.
    if(NULL == res)
    {
        auto it = m_pending.find(filename);
        if(it != m_pending.end() ) res = it->second;
    }

    // create new info and add it to pending list to be loaded.
    if(NULL == res)
    {
        ResourceFactory * factory = GetFactory(filename);
        if (!factory)
        {
            return NULL;
        }

        res = factory->CreateResource(def);
        m_pending[filename] = res;
        BOOL success = SetEvent(m_EventHandle);
        #ifdef  NDEBUG
        UNREFERENCED_VARIABLE(success);
        #endif
        assert(success);
    }
    assert(res);
    res->AddRef();
    return res;
}

// ----------------------------------------------------------------------------------------------
Resource* ResourceManager::LoadImmediate(const WCHAR* filename, Resource* def)
{
    AutoSync sync(&m_criticalSection); // CRITICAL SECTION - ENTIRE FUNCTION
    Resource * res = NULL;
    // check cache, use if already there.
    if(NULL == res)
    {
        auto it = m_loaded.find(filename);
        if(it != m_loaded.end() ) res = it->second;
    }

    // check pending, use if already there.
    if(NULL == res)
    {
        auto it = m_pending.find(filename);
        if(it != m_pending.end() ) res = it->second;
    }

    // create new info and add load it immediatelly.
    if(NULL == res)
    {
        ResourceFactory * factory = GetFactory(filename);
        res = factory->CreateResource(def);
        bool loaded = LoadResource(res, filename);
        if(!loaded)
        {
            Logger::Log(OutputMessageType::Error, L"failed to load %s\n",filename);
            SAFE_DELETE(res);
            return NULL;
        }
        else
        {
            m_loaded[filename] = res;
        }
    }
    res->AddRef();
    return res;
}

void ResourceManager::WaitOnPending()
{
    
    int numPending = 0;
    do
    {
        {
            AutoSync sync(&m_criticalSection);
            numPending = (int)m_pending.size();           
        } 

        //if there are pending assets, sleep for a little while, then check again.
        if(numPending)
        {
            Sleep(16);
        }        
    } while(numPending);
}
// ----------------------------------------------------------------------------------------------
int ResourceManager::GarbageCollect()
{
    int numCollected = 0;   
    WaitOnPending();
    assert(m_pending.size() == 0);

    // CRITICAL SECTION for the remainder of the function (deadlock could occur if this is done before WaitOnPending)
    AutoSync sync(&m_criticalSection);

    bool foundOne = true;
    while(foundOne)
    {
        foundOne = false;
        auto it = m_loaded.begin();
        while(it != m_loaded.end())
        {
            Resource * r = it->second;
            assert(r);
            if(r->GetRef()==0 )
            {
                foundOne = true;
                Logger::Log(OutputMessageType::Debug, L"Unloading %ls\n", FileUtils::Name(it->first.c_str()));
                SAFE_DELETE(r);
                ++numCollected;
                auto temp = it;
                ++it;
                m_loaded.erase(temp);
            }
            else
            {
                ++it;
            }
        }
    }
    Logger::Log(OutputMessageType::Debug, L"GarbageCollect completed\n");
    Logger::Log(OutputMessageType::Debug, L"Active Resources# %u\n", m_loaded.size());
    // complain about any leaked resources.
    //for(auto it = m_loaded.begin(); it != m_loaded.end(); ++it)
    //{        
    //    Logger::Log(OutputMessageType::Debug, L"Active Resource %ls\n", FileUtils::Name(it->first.c_str()));
   // }
    
    return numCollected;
}

}; // namespace
