//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "LineRenderer.h"
#include "RenderUtil.h"
#include "RenderContext.h"
#include "RenderState.h"
#include "GpuResourceFactory.h"

namespace LvEdEngine
{

LineRenderer*   LineRenderer::s_inst = NULL;


void LineRenderer::DrawFrustum(const Frustum& frustum, const float4& color)
{
    float3 corners[8];
    frustum.GetCorners(corners);

    m_vertsPC.push_back(VertexPC(corners[0],color));      
    m_vertsPC.push_back(VertexPC(corners[1],color));      

    m_vertsPC.push_back(VertexPC(corners[1],color));      
    m_vertsPC.push_back(VertexPC(corners[2],color));      

    m_vertsPC.push_back(VertexPC(corners[2],color));      
    m_vertsPC.push_back(VertexPC(corners[3],color));      

    m_vertsPC.push_back(VertexPC(corners[3],color));      
    m_vertsPC.push_back(VertexPC(corners[0],color));      
    
    m_vertsPC.push_back(VertexPC(corners[4],color));      
    m_vertsPC.push_back(VertexPC(corners[5],color));      

    m_vertsPC.push_back(VertexPC(corners[5],color));      
    m_vertsPC.push_back(VertexPC(corners[6],color));      

    m_vertsPC.push_back(VertexPC(corners[6],color));      
    m_vertsPC.push_back(VertexPC(corners[7],color));      

    m_vertsPC.push_back(VertexPC(corners[7],color));      
    m_vertsPC.push_back(VertexPC(corners[4],color));      
    
    m_vertsPC.push_back(VertexPC(corners[0],color));      
    m_vertsPC.push_back(VertexPC(corners[4],color));      

    m_vertsPC.push_back(VertexPC(corners[1],color));      
    m_vertsPC.push_back(VertexPC(corners[5],color));      

    m_vertsPC.push_back(VertexPC(corners[2],color));      
    m_vertsPC.push_back(VertexPC(corners[6],color));      

    m_vertsPC.push_back(VertexPC(corners[3],color));      
    m_vertsPC.push_back(VertexPC(corners[7],color));      



}
void LineRenderer::DrawAABB(const AABB& aabb, const float4& color)
{
    float3 min = aabb.Min();
    float3 max = aabb.Max(); 
    

    // render top quad    
    m_vertsPC.push_back(VertexPC(max,color));      
    m_vertsPC.push_back(VertexPC(float3(min.x,max.y,max.z),color));

    m_vertsPC.push_back(VertexPC(float3(min.x,max.y,max.z),color));
    m_vertsPC.push_back(VertexPC(float3(min.x,max.y,min.z),color));

    m_vertsPC.push_back(VertexPC(float3(min.x,max.y,min.z),color));
    m_vertsPC.push_back(VertexPC(float3(max.x,max.y,min.z),color));

    m_vertsPC.push_back(VertexPC(float3(max.x,max.y,min.z),color));
    m_vertsPC.push_back(VertexPC(max,color));

    // render bottom quad
    m_vertsPC.push_back(VertexPC(float3(max.x,min.y,max.z),color));      
    m_vertsPC.push_back(VertexPC(float3(min.x,min.y,max.z),color));

    m_vertsPC.push_back(VertexPC(float3(min.x,min.y,max.z),color));
    m_vertsPC.push_back(VertexPC(float3(min.x,min.y,min.z),color));

    m_vertsPC.push_back(VertexPC(float3(min.x,min.y,min.z),color));
    m_vertsPC.push_back(VertexPC(float3(max.x,min.y,min.z),color));

    m_vertsPC.push_back(VertexPC(float3(max.x,min.y,min.z),color));
    m_vertsPC.push_back(VertexPC(float3(max.x,min.y,max.z),color));

    // four legs
    m_vertsPC.push_back(VertexPC(max,color));  
    m_vertsPC.push_back(VertexPC(float3(max.x,min.y,max.z),color));      

    m_vertsPC.push_back(VertexPC(float3(min.x,max.y,max.z),color));
    m_vertsPC.push_back(VertexPC(float3(min.x,min.y,max.z),color));

    m_vertsPC.push_back(VertexPC(float3(min.x,max.y,min.z),color));
    m_vertsPC.push_back(VertexPC(float3(min.x,min.y,min.z),color));

    m_vertsPC.push_back(VertexPC(float3(max.x,max.y,min.z),color));
    m_vertsPC.push_back(VertexPC(float3(max.x,min.y,min.z),color));

}

void LineRenderer::RenderAll(RenderContext* rc)
{
    ID3D11DeviceContext* d3dContext = rc->Context();

    // update constant buffer

    Matrix viewProj = rc->Cam().View() * rc->Cam().Proj();    
    Matrix::Transpose(viewProj,m_perframeCB.Data);
    m_perframeCB.Update(d3dContext);
            	
    // set constant buffer to vertex shader    
    ID3D11Buffer* cbuffers[1] = {m_perframeCB.GetBuffer()};
    d3dContext->VSSetConstantBuffers(0,1,cbuffers);

    // set shaders
	d3dContext->VSSetShader(m_vsShader,NULL,0);
    d3dContext->GSSetShader( NULL, NULL, 0 );
	d3dContext->PSSetShader(m_psShader,NULL,0);

    // set vertex layout and primitive  topology    
    d3dContext->IASetInputLayout(m_vertexLayoutPC);
    d3dContext->IASetPrimitiveTopology( D3D_PRIMITIVE_TOPOLOGY_LINELIST );

    RSCache* rscache = RSCache::Inst();
    // set state-blocks ( raster, depth, and blend states)     
    d3dContext->RSSetState(rscache->GetRasterState(FillMode::Wireframe,CullMode::BACK));    
    d3dContext->OMSetDepthStencilState(NULL,0);    
    d3dContext->OMSetBlendState( NULL, NULL, 0xFFFFFFFF );
    
    ID3D11Buffer* vbuffers[1] = {m_vbPC->GetBuffer()};
    uint32_t strides[1] = {m_vbPC->GetStride()};
    uint32_t offsets[1] = {0};
    d3dContext->IASetVertexBuffers( 0, 1, vbuffers, strides, offsets );
    

    uint32_t bufSize = m_vbPC->GetCount();
    uint32_t totalVertexCount = (uint32_t) m_vertsPC.size();
    uint32_t start = 0;
    uint32_t count =  (totalVertexCount < bufSize) ? totalVertexCount : bufSize;
    
    while(start < totalVertexCount)
    {        
        m_vbPC->Update(d3dContext,&m_vertsPC[start],count);        
        d3dContext->Draw(count,0);
        start += count;
        if( (start + count) > totalVertexCount)
            count = totalVertexCount - start;
    }
    m_vertsPC.clear();
}

void LineRenderer::InitInstance(ID3D11Device* device)
{
    if(s_inst == NULL)
        s_inst = new LineRenderer(device);
}

void  LineRenderer::DestroyInstance()
{       
    SAFE_DELETE(s_inst);
}

LineRenderer::LineRenderer(ID3D11Device* device)
{
    // create vertex buffer.
    uint32_t vertCount = 1000;

  
    m_vbPC = GpuResourceFactory::CreateVertexBuffer(NULL,VertexFormat::VF_PC,vertCount,BufferUsage::DYNAMIC);    
    assert(m_vbPC);

    // compile shaders
    ID3DBlob* vsBlob = CompileShaderFromResource(L"LineShader.hlsl", "VS","vs_4_0", NULL);    
    ID3DBlob* psBlob = CompileShaderFromResource(L"LineShader.hlsl", "PS","ps_4_0", NULL);
    assert(vsBlob);
    assert(psBlob);

    
    m_vsShader = GpuResourceFactory::CreateVertexShader(vsBlob);
    m_psShader = GpuResourceFactory::CreatePixelShader(psBlob);
    assert(m_vsShader);
    assert(m_psShader);

    m_vertexLayoutPC = GpuResourceFactory::CreateInputLayout(vsBlob, VertexFormat::VF_PC);
        
    m_perframeCB.Construct(device);

    vsBlob->Release();
    psBlob->Release();

}

LineRenderer::~LineRenderer()
{
    SAFE_DELETE(m_vbPC);
    SAFE_RELEASE(m_vsShader);
    SAFE_RELEASE(m_psShader);
    SAFE_RELEASE(m_vertexLayoutPC);    
}

}//namespace LvEdEngine