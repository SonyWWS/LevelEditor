//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "V3dMath.h"
#include "CollisionPrimitives.h"

namespace LvEdEngine
{
    class Camera 
    {
    public:
        Camera(){}
        ~Camera(){}
        void SetViewProj(const Matrix& view, const Matrix& proj);
        
        bool IsOrtho() const { return m_proj.M34 == 0;}
        float Aspect() const { return m_proj.M22 / m_proj.M11; }
        const Matrix& View() const {return m_view;}
        const Matrix& Proj() const {return m_proj;}
        const Matrix& InvView(){return m_invView;}
        const float3& CamRight() const {return m_right;}
        const float3& CamUp()const {return m_up;};
        const float3& CamLook() const {return m_direction;}
        const float3& CamPos() const {return m_position;}
        const Frustum& GetFrustum() const { return m_frustum;}
        float NearZ() { return m_nearZ; }
        float FarZ()  { return m_farZ; }

        // compute world dimensions at the given world position.
        void ComputeWorldDimensions(const float3& posW, float* h, float* w) const;

        // compute unit per pixel at the given world position.
        // posW: world positoin.
        // vh:  view port height in pixels.
        float ComputeUnitPerPixel(const float3& posW, float vh) const;
        
    private:
        void UpdateInternals();

        Camera( const Camera& );
        Camera& operator=( const Camera& );
        Matrix m_proj;
        Matrix m_view;
        Matrix m_invView;
        float m_nearZ;
        float m_farZ;
        float3 m_position;
        float3 m_direction;
        float3 m_up;
        float3 m_right;        
        Frustum m_frustum;
    };

}
