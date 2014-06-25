//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

/****************************************************************************
    StringBlob.h

****************************************************************************/
#pragma once

#include "NonCopyable.h"

namespace LvEdEngine
{


    //=======================================================================
    //
    //  StringBlob
    //
    //  A "blob" style string storage class.
    //
    //  GOOD for temporary storage of strings (e.g., for a single frame).
    //  Calling clear() does not cause memory to be freed and subsequently
    //  reallocated. So the cost of clear() is essentially nothing.
    //
    //  Access to the string data is by index. This is because the buffer
    //  is subject to reallocation as strings are added. But the index
    //  (offset into the buffer) remains valid.
    //
    //  DO NOT USE if you must carry around string pointers. It is only
    //  safe to access the buffer by index!!
    //
    //  Because memory is not free'd when you call clear() the buffer will
    //  grow as large as needed and then remain that size until the
    //  destructor is called, or you explicitly call ClearAndRelease().
    //
    //  Example usgage:
    //      StringBlob<WCHAR,8192> m_stringBlob;
    //          // During the frame
    //      someStruct.stringIndex = m_stringBlob.Store( L"Hello!" );
    //      ...
    //      WCHAR* t = m_stringBlob[someStruct.stringIndex];
    //          // End of the frame when the data is no longer needed.
    //      m_stringBlob.clear();
    //
    //=======================================================================
    template <typename _CharType_, size_t _GrowSize_>
    class StringBlob : public NonCopyable
    {
    public:
        //-------------------------------------------------------------------
        //  StringBlob
        //-------------------------------------------------------------------
        StringBlob() : m_arrayStart( NULL ), m_dataTail( NULL ), m_allocated(0), m_elementsUsed( 0 ) 
        {
        }

        //-------------------------------------------------------------------
        //  ~StringBlob
        //-------------------------------------------------------------------
        ~StringBlob()
        {
            ClearAndRelease();
        }

        //-------------------------------------------------------------------
        //  ClearAndRelease
        //-------------------------------------------------------------------
        void ClearAndRelease()
        {
            if ( m_arrayStart )
            {
                free( m_arrayStart );
                m_arrayStart = NULL;
            }
            clear();
        }

        //-------------------------------------------------------------------
        //  size
        //-------------------------------------------------------------------
        size_t size()
        {
            return m_elementsUsed; 
        }
    
        //-------------------------------------------------------------------
        //  Store
        //
        //  Returns an index which you can use to get the stored copy
        //  via the array access operator.
        //-------------------------------------------------------------------
        size_t Store( const _CharType_* p, size_t numCharacters )
        {
            size_t nIndex = m_elementsUsed;
            size_t buffUsage = ( numCharacters + 1 );

            reserve( m_elementsUsed + buffUsage );

            memcpy( m_dataTail, p, ( buffUsage * sizeof(_CharType_)) );
            m_elementsUsed += buffUsage;
            SetDataTail();

            return nIndex;
        }

        //-------------------------------------------------------------------
        //  operator[]
        //
        //  "index" is a value previously returned by Store().
        //
        //  NOTE! The pointer returned here is only valid until the next
        //  time Store() is called. Use it immedately -- DO NOT STORE IT.
        //-------------------------------------------------------------------
        const _CharType_* operator[]( size_t index ) const
        {
            assert( index < m_elementsUsed );
            return ( m_arrayStart + index );
        }
    
        //-------------------------------------------------------------------
        //  clear
        //-------------------------------------------------------------------
        void clear()
        {
            m_elementsUsed = 0;
            SetDataTail();
        }

        //-------------------------------------------------------------------
        //  reserve
        //-------------------------------------------------------------------
        void reserve( size_t numElements )
        {
            if ( numElements > m_allocated )
            {
                m_allocated = ( numElements + _GrowSize_ );

                m_arrayStart = (_CharType_*)realloc( m_arrayStart, ( sizeof( _CharType_ ) * m_allocated ) );
                SetDataTail();
            }
        }

    private:
        size_t m_allocated;
        size_t m_elementsUsed;

        //-------------------------------------------------------------------
        void SetDataTail()
        {
            m_dataTail = ( m_arrayStart + m_elementsUsed ); 
        }

        _CharType_*        m_arrayStart;
        _CharType_*        m_dataTail;
    };

} // namespace LvEdEngine
