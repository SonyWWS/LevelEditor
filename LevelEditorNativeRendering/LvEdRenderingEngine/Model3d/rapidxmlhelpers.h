//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include <vector>
#include <map>


//NOTE: Don't inculde "rapidxml.hpp" directly, include it via this file 
// as we declare some important stuff that you need to do before including the .h file
//#define RAPIDXML_NO_EXCEPTIONS
#define RAPIDXML_STATIC_POOL_SIZE (64 * 1024)
#define RAPIDXML_DYNAMIC_POOL_SIZE (64 * 1024)
#define RAPIDXML_ALIGNMENT sizeof(void *)

#include "../rapidxml-1.13/rapidxml.hpp"
#include "../rapidxml-1.13/rapidxml_print.hpp"


namespace LvEdEngine
{

class float4;
class float3;
class float2;

typedef rapidxml::xml_node<> xml_node;
typedef rapidxml::xml_attribute<> xml_attribute;
typedef rapidxml::xml_document<> xml_document;


// node traversal
xml_node* GetChildEle(xml_node* node);
xml_node* GetNextEle(xml_node* node);

void FindAllByName(xml_node* node, const char* name, bool recurse, std::vector<xml_node*> * out);
xml_node* FindFirstByName(xml_node* node, const char * name);
xml_node* FindChildByName(xml_node* node, const char * name);
xml_node* FindNextByName(xml_node* node, const char * name);


// node parsing
const char * GetAttributeText(xml_node* node, const char * name, bool required);
unsigned int GetAttributeUINT(xml_node* node, const char * name);
int ParseUINTArray(xml_node* node, std::vector<unsigned int> * out );
int ParseFloatArray(xml_node* node, std::vector<float> * out );
int ParseVector3Array(xml_node* node, std::vector<float3> * out);
int ParseVector2Array(xml_node* node, std::vector<float2> * out);
bool ParseVector4(xml_node* node, float4 * out);
bool ParseVector3(xml_node* node, float3 * out);
bool ParseFloat(xml_node* node, float * out );
bool ConvertToBool(const char * name);

};
