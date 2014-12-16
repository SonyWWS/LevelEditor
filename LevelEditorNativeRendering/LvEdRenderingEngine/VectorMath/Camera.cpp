//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "Camera.h"

namespace LvEdEngine
{

void Camera::SetViewProj(const Matrix& view, const Matrix& proj)
{
    m_view = view;
    m_proj = proj;

    UpdateInternals();
   
    Matrix invProj;
    Matrix::Invert(m_proj,invProj);
    float3 p0(0,0,0);
    float3 p1(0,0,1);
    p0.Transform(invProj);
    p1.Transform(invProj);


    // re-compute cached values;
    m_right     = &m_invView.M11;
    m_up        = &m_invView.M21;
    m_direction = -float3(&m_invView.M31);
    m_position  = &m_invView.M41;
    m_nearZ = abs(p0.z);
    m_farZ = abs(p1.z);
}

void Camera::UpdateInternals()
{
    Matrix::Invert(m_view,m_invView);
    m_frustum.InitFromMatrix(m_view * m_proj);
}

float Camera::ComputeUnitPerPixel(const float3& posW, float vh) const
{
    float worldHeight, worldWidth;
    ComputeWorldDimensions(posW,&worldHeight,&worldWidth);
    return (worldHeight / vh);
}

void Camera::ComputeWorldDimensions(const float3& posW, float* h, float* w) const
{
    
    if (IsOrtho())
    {
        *w = 2.0f / m_proj.M11;
        *h = 2.0f / m_proj.M22;
    }
    else
    {
        // Transform worldPos to view space.
        float3 posV = float3::Transform(posW, m_view);
        float  dist = abs(posV.z);
        *w = (2.0f * dist) / m_proj.M11;
        *h = (2.0f * dist) / m_proj.M22;        
    }
}

};
