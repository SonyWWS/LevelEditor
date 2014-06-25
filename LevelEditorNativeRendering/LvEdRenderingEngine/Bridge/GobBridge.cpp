//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "GobBridge.h"
#include <cassert>
#include "../Core/Object.h"
#include "../Core/Hasher.h"
#include "../Core/Logger.h"


namespace LvEdEngine
{

    
// ------------------------------------------------------------------------------------------------
static uint64_t MakePair( uint32_t p1, uint32_t p2)
{
    return (uint64_t)p1 << 32 | p2;
}

// ------------------------------------------------------------------------------------------------
GobBridge::GobBridge()
{
}

GobBridge::~GobBridge()
{
}

// ------------------------------------------------------------------------------------------------
void GobBridge::RegisterObject(const char * typeName, ObjectCreatorFncPtr func)
{
    ObjectTypeGUID tid = Hash32(typeName);
    assert(m_createObjectFunctions.find(tid) == m_createObjectFunctions.end()); // double registration.
    m_createObjectFunctions[tid] = func;
}

// ------------------------------------------------------------------------------------------------
void GobBridge::RegisterProperty(const char* typeName, const char* propName, SetPropertyFncPtr set, GetPropertyFncPtr get)
{
    ObjectTypeGUID tid = Hash32(typeName);
    ObjectPropertyUID pid = Hash32(propName);
    uint64_t tidpid = MakePair(tid, pid);
    assert(m_propertyFunctions.find(tidpid) == m_propertyFunctions.end()); // double registration.
    m_propertyFunctions[tidpid] = PropertyFunc(set, get);
}

// ------------------------------------------------------------------------------------------------
void GobBridge::RegisterChildList(const char* typeName, const char* listName, AddChildFncPtr add, RemoveChildFncPtr remove)
{
    ObjectTypeGUID tid = Hash32(typeName);
    ObjectListUID lid = Hash32(listName);
    uint64_t tidlid = MakePair(tid, lid);
    assert(m_childListFunctions.find(tid) == m_childListFunctions.end()); // double registration.
    m_childListFunctions[tidlid] = ChildListFunc(add, remove);
}


// ------------------------------------------------------------------------------------------------
ObjectTypeGUID GobBridge::GetTypeId(const char* typeName)
{
    ObjectTypeGUID tid = Hash32(typeName);
    auto it = m_createObjectFunctions.find(tid);
    if(it != m_createObjectFunctions.end())
    {
        return tid;
    }
    return 0;
}

// ------------------------------------------------------------------------------------------------
ObjectPropertyUID GobBridge::GetPropertyId(ObjectTypeGUID tid, const char* propName)
{
    ObjectPropertyUID pid = Hash32(propName);
    uint64_t tidpid = MakePair(tid, pid);
    auto it = m_propertyFunctions.find(tidpid);
    if(it != m_propertyFunctions.end())
    {
        return pid;
    }
    return 0;
}

// ------------------------------------------------------------------------------------------------
ObjectListUID GobBridge::GetChildListId(ObjectTypeGUID tid, const char* listName)
{
    ObjectListUID id = Hash32(listName);
    uint64_t pair = MakePair(tid, id);
    auto it = m_childListFunctions.find(pair);
    if(it != m_childListFunctions.end())
    {
        return id;
    }
    return 0;
}


// ------------------------------------------------------------------------------------------------
ObjectGUID GobBridge::CreateObject(ObjectTypeGUID tid, void* data, int size)
{
    ObjectGUID instanceId = 0;
    auto it = m_createObjectFunctions.find(tid);
    if(it != m_createObjectFunctions.end())
    {
        ObjectCreatorFncPtr func = it->second;
        Object * obj = func(tid, data, size);
        if(obj)
        {
            instanceId = obj->GetInstanceId();
             Logger::Log(OutputMessageType::Debug, "Created %s\n", obj->ClassName());
        }
    }

    if(instanceId == 0)
    {
        Logger::Log(OutputMessageType::Error, "Failed to create object tid(0x%08x)\n", tid);
    }
      
    return instanceId;
}

// ------------------------------------------------------------------------------------------------
void GobBridge::DestroyObject(ObjectTypeGUID tid, ObjectGUID instanceId)
{

    if(instanceId != 0)
    {
        Object* obj = reinterpret_cast<Object*>(instanceId);
        const char * cname  = obj->ClassName();
        Logger::Log(OutputMessageType::Debug, "Destroying %s\n", cname);
    }
    else
    {
        Logger::Log(OutputMessageType::Error, "failed to destroy object tid(0x%08x)\n", tid);
    }

    if(instanceId != 0)
    {
        Object* obj = reinterpret_cast<Object*>(instanceId);
        delete obj;
    }
}


// ------------------------------------------------------------------------------------------------
void GobBridge::SetProperty(ObjectTypeGUID tid, ObjectPropertyUID pid, ObjectGUID instanceId, void* data, int size)
{
    SetPropertyFncPtr func = NULL;
    uint64_t tidpid = MakePair(tid, pid);
    auto it = m_propertyFunctions.find(tidpid);
    if(it != m_propertyFunctions.end())
    {
        PropertyFunc& pair = it->second;
        func = pair.first;
        if(func)
        {
            func(instanceId, data, size);
        }
    }

    if(!func)
    {
        printf("Failed to set property tid(0x%08x), pid(0x%08x)\n", tid, pid);
    }

}

// ------------------------------------------------------------------------------------------------
void GobBridge::GetProperty(ObjectTypeGUID tid, ObjectPropertyUID pid, ObjectGUID instanceId, void** data, int* size)
{
    // default to null,zero
    *data = NULL;
    *size = 0;

    GetPropertyFncPtr func = NULL;
    uint64_t tidpid = MakePair(tid, pid);
    auto it = m_propertyFunctions.find(tidpid);
    if(it != m_propertyFunctions.end())
    {
        PropertyFunc& pair = it->second;
        func = pair.second;
        if(func)
        {
            func(instanceId, data, size);
        }
    }
    if(!func)
    {
        printf("Failed to get property tid(0x%08x), pid(0x%08x)\n", tid, pid);
    }
}

// ------------------------------------------------------------------------------------------------
void GobBridge::AddChild(ObjectTypeGUID tid, ObjectListUID lid, ObjectGUID parent, ObjectGUID child, int index)
{
    AddChildFncPtr func = NULL;
    uint64_t tidpid = MakePair(tid, lid);
    auto it = m_childListFunctions.find(tidpid);
    if(it != m_childListFunctions.end())
    {
        ChildListFunc& pair = it->second;
        func = pair.first;
        func(parent, child, index);
    }

    if(!func)
    {
        printf("Failed to add child tid(0x%08x), lid(0x%08x)\n", tid, lid);
    }

}

// ------------------------------------------------------------------------------------------------
void GobBridge::RemoveChild(ObjectTypeGUID tid, ObjectListUID lid, ObjectGUID parent, ObjectGUID child)
{
    RemoveChildFncPtr func = NULL;
    uint64_t tidpid = MakePair(tid, lid);
    auto it = m_childListFunctions.find(tidpid);
    if(it != m_childListFunctions.end())
    {
        std::pair<AddChildFncPtr, RemoveChildFncPtr>& pair = it->second;
        func = pair.second;
        func(parent, child);
    }

    if(!func)
    {
        printf("Failed to remove child tid(0x%08x), lid(0x%08x)\n", tid, lid);
    }

}


}; // namespace


