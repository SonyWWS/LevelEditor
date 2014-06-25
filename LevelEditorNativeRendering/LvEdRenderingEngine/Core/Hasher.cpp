//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include <cassert>
#include <string>
#include <map>
#include <cstdint>
#include "Hasher.h"

// -------------------------------------------------------------------------
namespace LvEdEngine
{

    // -------------------------------------------------------------------------
    // generate a hash for a string
    hash32_t Hash32(const char * string)
    {
        hash32_t hval = Hash32InitialValue;
        unsigned char * current = (unsigned char*)string;
        while(*current != 0 )
        {
            hval = hval + (hval<<1) + (hval<<4) + (hval<<7) + (hval<<8) + (hval<<24);
            hval = hval ^ (unsigned int)(*current);
            ++current;
        }
        return hval;
    }

    // -------------------------------------------------------------------------
    // generate a case insensitive hash for a string
    hash32_t HashLowercase32(const char * string)
    {
        hash32_t hval = Hash32InitialValue;
        unsigned char * current = (unsigned char*)string;
        while(*current != 0 )
        {
            unsigned char val = *current;
            if(val >='A' && val <='Z') // convert to lower case
            {
                val += 'a'-'A';
            }
            hval = hval + (hval<<1) + (hval<<4) + (hval<<7) + (hval<<8) + (hval<<24);
            hval = hval ^ (unsigned int)(val);
            ++current;
        }
        return hval;
    }

}; // namespace LvEdEngine
