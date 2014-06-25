//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include <vector>
#include "../Core/WinHeaders.h"
#include "../Core/NonCopyable.h"

namespace LvEdEngine
{

// ------------------------------------------------------------------------------------------------
enum CustomDataType
{
    CustomDataInvalidType,
    CustomDataString,
    CustomDataBool,
    CustomDataInt,
    CustomDataFloat,
    CustomDataTypeMax,
};

// ------------------------------------------------------------------------------------------------
class CustomDataAttribute : public NonCopyable
{
public:
    CustomDataAttribute();
    ~CustomDataAttribute();

    const char *    GetName() const     { return m_name; }
    int             NumValues() const   { return (int)m_values.size(); }
    CustomDataType  GetType() const     { return m_type; }

    void SetIsArray(bool isArray)       { m_isArray = isArray; }
    void SetName(const char * name);

    // GetValueAs...  return value at specified index as specified type
    const char *    GetValueAsString(int index=0) const;    // returns NULL when invalid
    bool            GetValueAsBool(int index=0) const;      // returns false when invalid
    int             GetValueAsInt(int index=0) const;       // returns INT_MAX when invalid
    float           GetValueAsFloat(int index=0) const;     // returns FLOAT_MAX when invalid

    // PushValueAs... push values onto end of vector to ensure vector continguous
    //    (which helps managing stringValue memory)
    void PushValueAsString(const char * value);
    void PushValueAsBool(bool value);
    void PushValueAsInt(int value);
    void PushValueAsFloat(float value);

private:
    char *          m_name;
    bool            m_isArray;
    CustomDataType  m_type;

    union Value
    {
        char *  stringValue;
        bool    boolValue;
        int     intValue;
        float   floatValue;
    };
    std::vector<Value> m_values;

    bool  CheckType(CustomDataType type, bool allowInvalid) const;
    bool  CheckIndex(int index) const;
    bool  CheckArray() const;
};


}; // namespace LvEdEngine

