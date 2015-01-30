//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "Resource.h"
#include "../Core/Utils.h"
#include "../ResourceManager/ResourceManager.h"

namespace LvEdEngine
{
// -----------------------------------------------------------------------------------------------
Resource::Resource()
{
    m_refCount = 0;
    m_ready = false;
}

// -----------------------------------------------------------------------------------------------
//virtual
Resource::~Resource()
{
}

// -----------------------------------------------------------------------------------------------
void Resource::AddRef()
{
    ++m_refCount;
}

// -----------------------------------------------------------------------------------------------
void Resource::Release()
{
    --m_refCount;
}

// -----------------------------------------------------------------------------------------------
int Resource::GetRef()
{
    return m_refCount;
}

// -----------------------------------------------------------------------------------------------
bool Resource::IsReady()
{
    return m_ready;
}

// -----------------------------------------------------------------------------------------------
void Resource::SetReady()
{
    m_ready = true;
}

// -----------------------------------------------------------------------------------------------
ResourceReference::ResourceReference()
{
    m_target = NULL;
}

// -----------------------------------------------------------------------------------------------
ResourceReference::~ResourceReference()
{
    SAFE_RELEASE(m_target);
}

// -----------------------------------------------------------------------------------------------
Resource* ResourceReference::GetTarget()
{
    return m_target;
}

// -----------------------------------------------------------------------------------------------
void ResourceReference::SetTarget(const wchar_t* fileName, Resource* def)
{
    SAFE_RELEASE(m_target);
    if(fileName && wcslen(fileName)>0)
    {
        m_target = ResourceManager::Inst()->LoadAsync(fileName, def);
    }
}

};
