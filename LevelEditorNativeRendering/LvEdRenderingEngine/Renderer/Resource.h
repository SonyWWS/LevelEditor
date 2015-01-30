//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "../Core/WinHeaders.h"
#include "../Core/Object.h"
#include "RenderEnums.h"

namespace LvEdEngine
{
    //--------------------------------------------------
    // this is our base resource class which all resources
    // must derive from.
    class Resource : public Object
    {
    public:
        Resource();
        virtual ~Resource();
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "Resource";}
        virtual ResourceTypeEnum GetType()=0;

        void AddRef();
        void Release();
        int GetRef();
        bool IsReady();
        void SetReady();
    protected:
        int m_refCount;
        bool m_ready;
    };

    //--------------------------------------------------
    class ResourceReference  : public Object
    {
    public:
        ResourceReference();       
        ~ResourceReference();
        virtual const char* ClassName() const {return StaticClassName();}
        static const char* StaticClassName(){return "ResourceReference";}
        Resource * GetTarget();       
        void SetTarget(const wchar_t* fileName, Resource* def = NULL);
    protected:
        Resource* m_target;
    };

};
