//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include <stdio.h>
#include "../Core/Logger.h"
#include "../Core/Utils.h"
#include "DeviceManager.h"

namespace LvEdEngine
{

DeviceManager* gD3D11;


DeviceManager::DeviceManager(void)
	: m_pd3dDevice(NULL), 
	m_pImmediateContext(NULL),    
	m_pDXGIFactory1(NULL)
{	
    
	// create DXGIFactory, d3d device

	HRESULT hr = S_OK;

	// create dxgi factory and keep it for future use.	
	hr = CreateDXGIFactory1(__uuidof(IDXGIFactory1), (void**)(&m_pDXGIFactory1) );
	if (Logger::IsFailureLog(hr))
	{
         return;
	}
    
    
	IDXGIAdapter1* pAdapter;    
    // hardware vendor ID
    uint32_t nvidia = 0x10DE;  // NVIDIA Corporation
    uint32_t ati  = 0x1002;    // ATI Technologies Inc. / Advanced Micro Devices, Inc.

    int i = 0;
    int adapterIndex = 0;
    IDXGIOutput* output;

    printf("Available DXGIAdapters:\n");
    while(m_pDXGIFactory1->EnumAdapters1(i, &pAdapter) != DXGI_ERROR_NOT_FOUND) 
    {        
        DXGI_ADAPTER_DESC1 descr;
        pAdapter->GetDesc1(&descr);
        wprintf(L"\t%s\n",descr.Description);


        uint32_t j = 0;
       while(pAdapter->EnumOutputs(j, &output) != DXGI_ERROR_NOT_FOUND)
       {

           DXGI_OUTPUT_DESC outdescr;
           output->GetDesc(&outdescr);
           wprintf(L"\t\toutput Name: %s\n",outdescr.DeviceName);
           wprintf(L"\t\toutput IsAttached: %d\n",outdescr.AttachedToDesktop);
           
           j++;
           output->Release();
           output = NULL;
       }


        // choose discrete graphics over integrated.
        if(adapterIndex == 0 && (descr.VendorId == nvidia || descr.VendorId == ati))
        {
            adapterIndex = i;
        }
        i++;
        pAdapter->Release();
        pAdapter = NULL;
    }
    
	hr = m_pDXGIFactory1->EnumAdapters1(adapterIndex, &pAdapter);
	if (Logger::IsFailureLog(hr))
	{
         return;
	}

	
    UINT createDeviceFlags = 0;
#ifdef _DEBUG
    createDeviceFlags |= D3D11_CREATE_DEVICE_DEBUG;
#endif

    /*D3D_DRIVER_TYPE driverTypes[] =
    {
        D3D_DRIVER_TYPE_HARDWARE,
        D3D_DRIVER_TYPE_WARP,
        D3D_DRIVER_TYPE_REFERENCE,
		
    };
    UINT numDriverTypes = ARRAYSIZE( driverTypes );*/

    D3D_FEATURE_LEVEL featureLevels[] =
    {
        D3D_FEATURE_LEVEL_11_0,
        D3D_FEATURE_LEVEL_10_1,
        D3D_FEATURE_LEVEL_10_0,
    };
	UINT numFeatureLevels = ARRAYSIZE( featureLevels );

	DXGI_ADAPTER_DESC1 pdescr;
	pAdapter->GetDesc1(&pdescr);
    Logger::Log(OutputMessageType::Info, L"%s\n",pdescr.Description);


    // Note: If you set the pAdapter parameter to a non-NULL value,
    //      you must also set the DriverType parameter 
    //      to the D3D_DRIVER_TYPE_UNKNOWN
    
	// create d3d11 device on the first graphics adapter.
	hr =  D3D11CreateDevice(
			pAdapter,
			D3D_DRIVER_TYPE_UNKNOWN,
			NULL,
			createDeviceFlags,
			featureLevels,
			numFeatureLevels,
			D3D11_SDK_VERSION,
			&m_pd3dDevice,
			NULL,
			&m_pImmediateContext);	
  		
	pAdapter->Release(); // no longer needed.

	if (Logger::IsFailureLog(hr))
	{
		return;
	}
    D3D_FEATURE_LEVEL featureLevel = m_pd3dDevice->GetFeatureLevel();
    wchar_t* strFeatureLevel = NULL;

    if( featureLevel == D3D_FEATURE_LEVEL_9_1)
    {
        strFeatureLevel = L"D3D_FEATURE_LEVEL_9_1";
    }
    else if( featureLevel == D3D_FEATURE_LEVEL_9_2)
    {
        strFeatureLevel = L"D3D_FEATURE_LEVEL_9_2";        
    }
    else if( featureLevel == D3D_FEATURE_LEVEL_9_3)
    {
        strFeatureLevel = L"D3D_FEATURE_LEVEL_9_3";        
    }
    else if( featureLevel ==D3D_FEATURE_LEVEL_10_0)
    {
        strFeatureLevel = L"D3D_FEATURE_LEVEL_10_0";                
    }
    else if( featureLevel ==D3D_FEATURE_LEVEL_10_1)
    {
        strFeatureLevel = L"D3D_FEATURE_LEVEL_10_1";                
    }
    else if( featureLevel ==D3D_FEATURE_LEVEL_11_0)
    {
        strFeatureLevel = L"D3D_FEATURE_LEVEL_11_0";                
    }
    else
    {
        strFeatureLevel = L"Newer than D3D_FEATURE_LEVEL_11_0";                        
    }        
    Logger::Log(OutputMessageType::Info,L"Feature Level: %s\n", strFeatureLevel);

}

DeviceManager::~DeviceManager(void)
{
    Logger::Log(OutputMessageType::Info, "start shutdown DX11\n");
	if( m_pImmediateContext ) 
    {
        m_pImmediateContext->ClearState();	        
        m_pImmediateContext->Flush();        
    }
    
    SAFE_RELEASE(m_pImmediateContext);    
    SAFE_RELEASE(m_pd3dDevice);
    SAFE_RELEASE(m_pDXGIFactory1);

    
   /* #ifdef _DEBUG
    ID3D11Debug* dbg;
     m_pd3dDevice->QueryInterface(__uuidof(ID3D11Debug), reinterpret_cast<void**>(&dbg));
     if(m_dbg)
     {
         m_dbg->ReportLiveDeviceObjects(D3D11_RLDO_DETAIL);            
     }
#endif */


    Logger::Log(OutputMessageType::Info, "shutdown DX11\n");
}

}
