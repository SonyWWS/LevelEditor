//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "../Core/winheaders.h"
#include "../Core/NonCopyable.h"
#include <d3d11.h>

namespace LvEdEngine
{

// A simple class for creating and initializing d3d11 device.
class DeviceManager : public NonCopyable
{

public:
	DeviceManager(void);
	~DeviceManager(void);

		
	ID3D11Device* GetDevice()
	{
		return m_pd3dDevice;
	}

	IDXGIFactory1* GetDXGIFactory1()
	{
		return m_pDXGIFactory1;
	}

	ID3D11DeviceContext* GetImmediateContext()
	{
		return m_pImmediateContext;
	}


private:	
	ID3D11Device*           m_pd3dDevice;
	ID3D11DeviceContext*    m_pImmediateContext;		
	IDXGIFactory1*          m_pDXGIFactory1;
    

};
extern DeviceManager* gD3D11;

}

//
//#ifdef _DEBUG
// deviceCreationFlags |= D3D11_CREATE_DEVICE_DEBUG;
// #endif
// ...
// ID3D11Debug *d3dDebug = nullptr;
// if( SUCCEEDED( d3dDevice->QueryInterface( __uuidof(ID3D11Debug), (void**)&d3dDebug ) ) )
// {
// ID3D11InfoQueue *d3dInfoQueue = nullptr;
// if( SUCCEEDED( d3dDebug->QueryInterface( __uuidof(ID3D11InfoQueue), (void**)&d3dInfoQueue ) ) )
// {
// #ifdef _DEBUG
// d3dInfoQueue->SetBreakOnSeverity( D3D11_MESSAGE_SEVERITY_CORRUPTION, true );
// d3dInfoQueue->SetBreakOnSeverity( D3D11_MESSAGE_SEVERITY_ERROR, true );
// #endif
// 
// D3D11_MESSAGE_ID hide [] =
// {
// D3D11_MESSAGE_ID_SETPRIVATEDATA_CHANGINGPARAMS,
// // Add more message IDs here as needed
// };
// 
// D3D11_INFO_QUEUE_FILTER filter;
// memset( &filter, 0, sizeof(filter) );
// filter.DenyList.NumIDs = _countof(hide);
// filter.DenyList.pIDList = hide;
// d3dInfoQueue->AddStorageFilterEntries( &filter );
// d3dInfoQueue->Release();
// }
// d3dDebug->Release();
// }
//
//