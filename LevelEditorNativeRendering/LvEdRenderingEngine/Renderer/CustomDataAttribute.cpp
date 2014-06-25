//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "CustomDataAttribute.h"
#include "../Core/Utils.h"
#include "../Core/Logger.h"

// REMOVE
#include "../Model3d/rapidxmlhelpers.h"


namespace LvEdEngine
{

// ------------------------------------------------------------------------------------------------
const char * c_customDataTypeNames[CustomDataTypeMax] =
{
    "CustomDataInvalidType",
    "CustomDataString",
    "CustomDataBool",
    "CustomDataInt",
    "CustomDataFloat",
};


CustomDataAttribute::CustomDataAttribute() : m_name(NULL), m_type(CustomDataInvalidType), m_isArray(false)
{
}

CustomDataAttribute::~CustomDataAttribute()
{
    SAFE_DELETE(m_name);
    if (m_type == CustomDataString)
    {
        for (auto it=m_values.begin(); it!=m_values.end(); it++)
        {
            SAFE_DELETE(it->stringValue);
        }
    }
}

void CustomDataAttribute::SetName(const char * name)
{
    if (m_name)
    {
        SAFE_DELETE(m_name);
    }
    int len = (int)strlen(name);
    m_name = new char[len+1];
    memcpy(m_name, name, len+1);
}

const char * CustomDataAttribute::GetValueAsString(int index) const
{
    if (!CheckIndex(index) || !CheckType(CustomDataString, false))
    {
        return NULL;
    }
    return m_values[index].stringValue;
}

bool CustomDataAttribute::GetValueAsBool(int index) const
{
    if (!CheckIndex(index) || !CheckType(CustomDataBool, false))
    {
        return false;
    }
    return m_values[index].boolValue;
}

int CustomDataAttribute::GetValueAsInt(int index) const
{
    if (!CheckIndex(index) || !CheckType(CustomDataInt, false))
    {
        return INT_MAX;
    }
    return m_values[index].intValue;
}

float CustomDataAttribute::GetValueAsFloat(int index) const
{
    if (!CheckIndex(index) || !CheckType(CustomDataFloat, false))
    {
        return FLT_MAX;
    }
    return m_values[index].floatValue;
}

void CustomDataAttribute::PushValueAsString(const char * value)
{
    if (CheckArray() && CheckType(CustomDataString, true))
    {
        m_type = CustomDataString;
        Value v;
        int len = (int)strlen(value);
        v.stringValue = new char[len+1];
        memcpy(v.stringValue, value, len+1);
        m_values.push_back(v);
    }
}

void CustomDataAttribute::PushValueAsBool(bool value)
{
    if (CheckArray() && CheckType(CustomDataBool, true))
    {
        m_type = CustomDataBool;
        Value v;
        v.boolValue = value;
        m_values.push_back(v);
    }
}

void CustomDataAttribute::PushValueAsInt(int value)
{
    if (CheckArray() && CheckType(CustomDataInt, true))
    {
        m_type = CustomDataInt;
        Value v;
        v.intValue = value;
        m_values.push_back(v);
    }
}

void CustomDataAttribute::PushValueAsFloat(float value)
{
    if (CheckArray() && CheckType(CustomDataFloat, true))
    {
        m_type = CustomDataFloat;
        Value v;
        v.floatValue = value;
        m_values.push_back(v);
    }
}

bool CustomDataAttribute::CheckType(CustomDataType type, bool allowInvalid) const
{
    if (type != m_type)
    {
        if (!(allowInvalid && m_type == CustomDataInvalidType))
        {
            Logger::Log(OutputMessageType::Error, "Expecting custom data type '%s' for '%s' but got '%s'\n",
                    c_customDataTypeNames[type], m_name, c_customDataTypeNames[m_type]);
            return false;
        }
    }
    return true;
}

bool CustomDataAttribute::CheckIndex(int index) const
{
    if (index > 0 && !m_isArray)
    {
        Logger::Log(OutputMessageType::Error, "Custom data index, %d, on non-array, for '%s'\n", index, m_name);
        return false;
    }

    if (index < 0 || index >= (int)m_values.size())
    {
        Logger::Log(OutputMessageType::Error, "Custom data index, %d, outside bounds, (0,%d), for '%s'\n",
                index, m_values.size(), m_name);
        return false;
    }
    return true;
}

bool CustomDataAttribute::CheckArray() const
{
    if (!m_isArray && m_values.size() == 1)
    {
        Logger::Log(OutputMessageType::Error, "Trying to add additional value to non-array custom data attribute, '%s'\n", m_name);
        return false;
    }
    return true;
}

}; // namespace LvEdEngine

