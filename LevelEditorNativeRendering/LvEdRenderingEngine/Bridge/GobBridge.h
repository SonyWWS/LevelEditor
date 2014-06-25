//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include <map>
#include <set>
#include "../Core/WinHeaders.h"
#include "../Core/typedefs.h"
#include "../Core/NonCopyable.h"


namespace LvEdEngine
{
    class Object;

    typedef Object* (*ObjectCreatorFncPtr)(ObjectTypeGUID, void*, int);
    typedef void (*SetPropertyFncPtr)(ObjectGUID,void*,int);
    typedef void (*GetPropertyFncPtr)(ObjectGUID,void**,int*);
    typedef void (*AddChildFncPtr)(ObjectGUID,ObjectGUID,int);
    typedef void (*RemoveChildFncPtr)(ObjectGUID,ObjectGUID);
    typedef std::pair<AddChildFncPtr, RemoveChildFncPtr> ChildListFunc;
    typedef std::pair<SetPropertyFncPtr, GetPropertyFncPtr> PropertyFunc;

    typedef std::map<ObjectTypeGUID,ObjectCreatorFncPtr> ObjectCreationMap;
    typedef std::map<uint64_t,PropertyFunc> PropertyMap;
    typedef std::map<uint64_t,ChildListFunc> ChildListMap;

    //-------------------------------------------------------------------------------------------------
    // The GobBridge class is designed to 'bridge' the differences between a game engine and
    // the c# style schema based object system.
    // You use it by registering object creation functions for all the possible objects C# my require
    // and by registering 'set property' functions for all the attributes exposed to the C# code by the 
    // schema.
    //-------------------------------------------------------------------------------------------------
    class GobBridge : public NonCopyable
    {
    public:
        GobBridge();
        ~GobBridge();

        void RegisterObject(const char* typeName, ObjectCreatorFncPtr func);
        void RegisterProperty(const char* typeName, const char* propName, SetPropertyFncPtr set, GetPropertyFncPtr get);
        void RegisterChildList(const char* typeName, const char* listName, AddChildFncPtr add, RemoveChildFncPtr remove);

        ObjectTypeGUID GetTypeId(const char* typeName);
        ObjectPropertyUID GetPropertyId(ObjectTypeGUID tid, const char* propName);
        ObjectListUID GetChildListId(ObjectTypeGUID tid, const char* listName);

        ObjectGUID CreateObject(ObjectTypeGUID tid, void* data, int size);
        void DestroyObject(ObjectTypeGUID tid, ObjectGUID instanceId);
        void SetProperty(ObjectTypeGUID tid, ObjectPropertyUID propId, ObjectGUID instanceId, void* data, int size);
        void GetProperty(ObjectTypeGUID tid, ObjectPropertyUID propId, ObjectGUID instanceId, void** data, int* size);
        void AddChild(ObjectTypeGUID tid, ObjectListUID lid, ObjectGUID parent, ObjectGUID child, int index);
        void RemoveChild(ObjectTypeGUID tid, ObjectListUID lid, ObjectGUID parent, ObjectGUID child);

    protected:
        ObjectCreationMap m_createObjectFunctions;
        PropertyMap m_propertyFunctions;
        ChildListMap m_childListFunctions;
    };
};
