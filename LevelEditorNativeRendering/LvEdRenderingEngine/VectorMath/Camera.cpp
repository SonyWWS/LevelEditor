//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "Camera.h"

namespace LvEdEngine
{

void Camera::SetViewProj(const Matrix& view, const Matrix& proj)
{
    m_view = view;
    m_proj = proj;

    UpdateInternals();

    // re-compute cached values;
    m_right     = &m_invView.M11;
    m_up        = &m_invView.M21;
    m_direction = -float3(&m_invView.M31);
    m_position  = &m_invView.M41; 
}

void Camera::UpdateInternals()
{
    Matrix::Invert(m_view,m_invView);
    m_frustum.InitFromMatrix(m_view * m_proj);
}


void Camera::ComputeWorldDimensions(const float3& worldPos, float * h, float * w) const
{
    // transform worldPos to view space.
    float3 posV = float3::Transform(worldPos, m_view);

    float  dist = abs(posV.z);
    
    if (IsOrtho())
    {
        *w = 2.0f * m_proj.M11;
        *h = 2.0f * m_proj.M22;
    }
    else
    {
        *w = (2.0f * dist) / m_proj.M11;
        *h = (2.0f * dist) / m_proj.M22;
    }
}

void Camera::ComputeViewDimensions(const float3& viewPos, float * h, float * w) const
{
    float  dist = abs(viewPos.z);
    
    if (IsOrtho())
    {
        *w = 2.0f * m_proj.M11;
        *h = 2.0f * m_proj.M22;
    }
    else
    {
        *w = (2.0f * dist) / m_proj.M11;
        *h = (2.0f * dist) / m_proj.M22;
    }
}

};
