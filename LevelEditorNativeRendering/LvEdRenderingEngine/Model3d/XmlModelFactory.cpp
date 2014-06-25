//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "../Core/Utils.h"
#include "../Core/Logger.h"
#include "../Core/FileUtils.h"
#include "../ResourceManager/ResourceManager.h"
#include "Model3dBuilder.h"
#include "rapidxmlhelpers.h"
#include "XmlModelFactory.h"

namespace LvEdEngine
{


// ----------------------------------------------------------------------------------------------
XmlModelFactory::XmlModelFactory(ID3D11Device* device) : m_device(device)
{
}
 
// ----------------------------------------------------------------------------------------------
bool XmlModelFactory::LoadResource(Resource* resource, const WCHAR * filename)
{
    UINT dataSize;
    BYTE* data = FileUtils::LoadFile(filename, &dataSize);
    if (!data)
    {
        return false;
    }

    Model * model = (Model*)resource;
    model->SetSourceFileName(filename);

    // char name for logging 'char*' exceptions
    char charName[MAX_PATH];
    WideCharToMultiByte(0, 0, filename, -1, charName, MAX_PATH, NULL, NULL);



    Model3dBuilder builder;
    builder.m_model = model;
    xml_document doc;
    bool succeeded = false;

    try
    {
        doc.parse<0>((char*)data);

        m_parseErrors = 0;

        builder.Begin();

        ProcessXml(doc.first_node(), &builder);

        builder.End();

        if (m_parseErrors > 0)
        {
            Logger::Log(OutputMessageType::Error, L"%d errors occured while parsing, '%s'\n",
                                                            m_parseErrors, filename);
        }
        else
        {
            // this will create the D3D vertex/index buffers as well
            // as trigger the loading of the textures.
            model->Construct(m_device, ResourceManager::Inst());
        }

        succeeded = true;
    }
    catch(rapidxml::parse_error& error)
    {
        Logger::Log(OutputMessageType::Error, "Parse exception: '%s' while processing '%s'\n", error.what(), charName);
    }
    catch(std::runtime_error& error)
    {
        Logger::Log(OutputMessageType::Error, "Processing exception: '%s' while processing '%s'\n", error.what(), charName);
    }
    catch(...)
    {
        Logger::Log(OutputMessageType::Error, L"Generic exception while processing '%s'\n", filename);
    }

    if (!succeeded)
    {
        // clean up model (remove all geometry), but don't free memory because there
        // are other references to this model from 
        model->Destroy();
    }

    SAFE_DELETE_ARRAY(data);

    return succeeded;
}

void XmlModelFactory::ParseError(const char * fmt, ...)
{
    va_list args;
    va_start(args, fmt);
    Logger::LogVA(OutputMessageType::Error, fmt, args);
    va_end(args);
    m_parseErrors++;
}

}; // namespace LvEdEngine


