//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "rapidxmlhelpers.h"
#include "../VectorMath/V3dMath.h"
#include "../Core/Logger.h"
using namespace LvEdEngine;

namespace rapidxml
{
    //! When exceptions are disabled by defining RAPIDXML_NO_EXCEPTIONS, 
    //! this function is called to notify user about the error.
    //! It must be defined by the user.
    //! <br><br>
    //! This function cannot return. If it does, the results are undefined.
    //! <br><br>
    //! A very simple definition might look like that:
    //! <pre>
    //! void %rapidxml::%parse_error_handler(const char *what, void *where)
    //! {
    //!     std::cout << "Parse error: " << what << "\n";
    //!     std::abort();
    //! }
    //! </pre>
    //! \param what Human readable description of the error.
    //! \param where Pointer to character data where error was detected.
    /*void parse_error_handler(const char *what, void *where)
    {
        Logger::Log(OutputMessageType::Error, "%s\n", what);
    }*/
}


namespace LvEdEngine
{

// ------------------------------------------------------------------------------------------------
xml_node* GetChildEle(xml_node* node)
{
    return node->first_node();
}

// ------------------------------------------------------------------------------------------------
xml_node* GetNextEle(xml_node* node)
{
    if(node) return node->next_sibling();
    return NULL;
}



// ------------------------------------------------------------------------------------------------
void FindAllByName(xml_node* node, const char* name, bool recurse, std::vector<xml_node*> * out)
{
  if(node)
  {
    for(xml_node* child = node->first_node(); child != NULL; child=child->next_sibling())
    {
        if(strcmp(child->name(), name) == 0)
        {
            out->push_back(child);
        }
        if(recurse)
        {
            FindAllByName(child, name, recurse, out);
        }
    }
  }
}

static xml_node* FindFirstByNameInernal(xml_node* node, const char * name)
{
    if(node == NULL) return NULL;
    if(strcmp(node->name(), name)==0) return node;            
    xml_node* first = FindFirstByNameInernal(node->first_node(),name);
    if(first) return first;
    xml_node* next = FindFirstByNameInernal(node->next_sibling(),name);
    return next;
}

// ------------------------------------------------------------------------------------------------
xml_node* FindFirstByName(xml_node* node, const char * name)
{
   if(node == NULL) return NULL;
   if(strcmp(node->name(), name)==0)
       return node;
   else
       return FindFirstByNameInernal(node->first_node(),name);  
}


// ------------------------------------------------------------------------------------------------
xml_node* FindChildByName(xml_node* node, const char * name)
{
  if(node) return node->first_node(name);
  return NULL;
}

// ------------------------------------------------------------------------------------------------
xml_node* FindNextByName(xml_node* node, const char * name)
{
  if(node) return node->next_sibling(name);
  return NULL;
}

// ------------------------------------------------------------------------------------------------
const char * GetAttributeText(xml_node* node, const char* name, bool required)
{
    if (node)
    {
        xml_attribute* att = node->first_attribute(name);
        if (att) return att->value();
    }

    if (required)
    {
        const char* nodeName = node ? node->name() : "unknown";        
        name = name ? name : "unknown";
        Logger::Log(OutputMessageType::Error, "<%s> is missing required attribute, '%s'\n", nodeName, name);
    }
    return NULL;
}

// ------------------------------------------------------------------------------------------------
unsigned int  GetAttributeUINT(xml_node* node, const char * name)
{
  if(node)
  {
    const char * text = GetAttributeText(node, name, true);
    if(text)
    {
      return (unsigned int)atoi(text);
    }
  }
  return 0;
}



// ------------------------------------------------------------------------------------------------
int ParseFloatArray(xml_node* node, std::vector<float> * out )
{
  if(node)
  {
    // if there is a 'count' attribute, then resize to that
    const char * countVal = GetAttributeText(node, "count", false);
    char * values = node->value();
    size_t size = strlen(values);
   
    if(values)
    {
        if(countVal)
        {
          int count = atoi(countVal);
          out->reserve(count);
        }
        else if(size)
        {
            out->reserve(size/2); // make a guess at hom many values there are.
        }
        char * end = values + size;
        while(values < end)
        {
            float f = (float)strtod(values, &values);
            ++values; // skip the space
            out->push_back(f);
        }
    }
  }
  return (int)out->size();
}



// ------------------------------------------------------------------------------------------------
int ParseUINTArray(xml_node* node, std::vector<unsigned int> * out )
{
  if(node)
  {
    // if there is a 'count' attribute, then resize to that
    const char * countVal = GetAttributeText(node, "count", false);
    char * values = node->value();
    size_t size = strlen(values);
   
    if(values)
    {
        if(countVal)
        {
          int count = atoi(countVal);
          out->reserve(count);
        }
        else if(size)
        {
            out->reserve(size/2); // make a guess at how many values there are.
        }
        char * end = values + size;
        while(values < end)
        {
            int f = (int)strtol(values, &values, 0);
            ++values; // skip the space
            out->push_back(f);
        }
    }
  }
  return (int)out->size();
}

// ------------------------------------------------------------------------------------------------
int ParseVector3Array(xml_node* node, std::vector<float3> * out)
{
  if(node)
  {
    out->resize(0);
    std::vector<float> temp;
    if(ParseFloatArray(node, &temp))
    {
      out->resize(0);
      out->reserve(temp.size()/3);
      float3 vec;
      for( unsigned int i = 0; i+2 < temp.size(); i+=3)
      {
        vec.x = temp[i];
        vec.y = temp[i+1];
        vec.z = temp[i+2];
        out->push_back(vec);
      }
    }
  }
  return (int)out->size();
}

// ------------------------------------------------------------------------------------------------
int ParseVector2Array(xml_node* node, std::vector<float2> * out)
{
  if(node)
  {
    out->resize(0);
    std::vector<float> temp;
    if(ParseFloatArray(node, &temp))
    {
      out->resize(0);
      out->reserve(temp.size()/2);
      float2 vec;
      for( unsigned int i = 0; i+1 < temp.size(); i+=2)
      {
        vec.x = temp[i];
        vec.y = temp[i+1];
        out->push_back(vec);
      }
    }
  }
  return (int)out->size();
}

// ------------------------------------------------------------------------------------------------
bool ParseVector4(xml_node* node, float4 * out)
{
  if (node)
  {
    std::vector<float> temp;
    if (ParseFloatArray(node, &temp) >=4)
    {
      out->x = temp[0];
      out->y = temp[1];
      out->z = temp[2];
      out->w = temp[3];
      return true;
    }
  }
  return false;
}

// ------------------------------------------------------------------------------------------------
bool ParseVector3(xml_node* node, float3 * out)
{
  if (node)
  {
    std::vector<float> temp;
    if (ParseFloatArray(node, &temp) >=3)
    {
      out->x = temp[0];
      out->y = temp[1];
      out->z = temp[2];
      return true;
    }
  }
  return false;
}

// ------------------------------------------------------------------------------------------------
bool ParseFloat(xml_node* node, float * out )
{
  if (node)
  {
    std::vector<float> temp;
    if (ParseFloatArray(node, &temp) >=1)
    {
      *out = temp[0];
      return true;
    }
  }
  return false;
}

// ------------------------------------------------------------------------------------------------
bool ConvertToBool(const char * name)
{
    if (!name)
    {
        return false;
    }

    if (strcmp(name, "true")==0 || strcmp(name, "1")==0)
    {
        return true;
    }

    if (strcmp(name, "false")==0 || strcmp(name, "0")==0)
    {
        return false;
    }

    Logger::Log(OutputMessageType::Error, "Unexpected xs::boolean value, '%s'\n", name);
    return false;
}

};
