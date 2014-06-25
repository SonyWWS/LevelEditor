//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#ifndef UNREFERENCED_VARIABLE
#define UNREFERENCED_VARIABLE(P)           (P)
#endif

#ifndef SAFE_DELETE
#define SAFE_DELETE(p)       { if (p) { delete (p);     (p)=NULL; } }
#endif

#ifndef SAFE_DELETE_ARRAY
#define SAFE_DELETE_ARRAY(p) { if (p) { delete[] (p);   (p)=NULL; } }
#endif

#ifndef SAFE_RELEASE
#define SAFE_RELEASE(p)      { if (p) { (p)->Release(); (p)=NULL; } }
#endif

#ifndef SAFE_ADDREF
#define SAFE_ADDREF(p)      { if (p) { (p)->AddRef(); } }
#endif

#include <vector>
template<class T>
inline void FreeVectorMemory(std::vector<T>& v)
{
    // most implementations of std::vector fail to free memory on clear() or resize(0)
    // ... but if we swap the memory pointers, the memory will be freed once temp goes
    // out of scope

    std::vector<T> temp;
    temp.swap(v);
}

#ifndef ARRAY_SIZE
#define ARRAY_SIZE(_arr_) (sizeof(_arr_)/sizeof((_arr_)[0]))
#endif

//
//template<class T> 
//class ScopedObject
//{
//public:
//    explicit ScopedObject( T *p = 0 ) : _pointer(p) {}
//    ~ScopedObject()
//    {
//        if ( _pointer )
//        {
//            _pointer->Release();
//            _pointer = nullptr;
//        }
//    }
//
//    bool IsNull() const { return (!_pointer); }
//
//    T& operator*() { return *_pointer; }
//    T* operator->() { return _pointer; }
//    T** operator&() { return &_pointer; }
//
//    void Reset(T *p = 0) { if ( _pointer ) { _pointer->Release(); } _pointer = p; }
//
//    T* Get() const { return _pointer; }
//
//private:
//    ScopedObject(const ScopedObject&);
//    ScopedObject& operator=(const ScopedObject&);
//        
//    T* _pointer;
//};

