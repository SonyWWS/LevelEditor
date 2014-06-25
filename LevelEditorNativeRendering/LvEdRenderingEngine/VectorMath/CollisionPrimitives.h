//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "V3dMath.h"
#include "float.h"
#include "../Core/NonCopyable.h"

namespace LvEdEngine
{
    class Plane
    {
    public:
        Plane(){}
             // create plane from ABC, ordered clockwise.
        // let n be plane normal
        // let A be a point on the plain
        // n . (X - A) = 0
        // n . X = n . A 
        // n . X = d;
        Plane(const float3 &A,const float3 &B,const float3 &C);

        Plane(const float4 &v)
        {
            normal.x = v.x;
            normal.y = v.y;
            normal.z = v.z;
            d = v.w;

        }
        // plane from point and normal.
        Plane(const float3 &P, const float3 &n);

        void Normalize()
        {
            float inv_mag = 1.0f / length(normal);
            normal.x *= inv_mag;
            normal.y *= inv_mag;
            normal.z *= inv_mag;
            d *= inv_mag;
        }

        // evaluate P
        float Eval(const float3 &P) const;
		const float& operator[] (int index) const
		{
			if(index < 3)
				return normal[index];
			return d;	
		}
        
        float3 normal;
        float d;
    };


    class Ray
    {
    public:
        Ray() { }
        Ray(const float3 &p, const float3 d)
        {
            pos = p;
            direction = normalize(d);
        }

        float3 pos;
        float3 direction;
        

    };


    class LineSeg
    {
    public:
        LineSeg() { }
        
        LineSeg(const float3 &a, const float3 &b): A(a), B(b)
        { }

        float3 A;
        float3 B;
    };


    class Bound2di
    {
    public:
        int32_t x1;
        int32_t y1;
        int32_t x2;
        int32_t y2;

        static bool Intersect(const Bound2di& r1, const Bound2di& r2, Bound2di& out);
        void Extend(const  Bound2di& box);
        bool isEmpty() const
        {           
           return  (x2-x1 <= 0 ) || (y2 - y1) <= 0;
        }
        
    };

    class AABB
    {
    public:
        AABB():m_min(FLT_MAX,FLT_MAX,FLT_MAX),m_max(-FLT_MAX,-FLT_MAX,-FLT_MAX){}
        AABB(const float3 &min, const float3 &max);
        const float3& Min() const;
        const float3& Max() const;
        float3 GetCenter() const
        {
          return (m_min + m_max)/2.0f;
        }
        void Transform(const Matrix &xform);

        // Grows this box to include 'otherBox'.
        void Extend( const AABB& otherBox );

        // Grow this box to include point
        void Extend( const float3& point );

        // is the given point is inside this AABB.
        bool Contain(const float3& pt) const;

        // get the 8 corners of the aabb
        void Corners( float3 corners[8] ) const;

    private:
        float3 m_min;
        float3 m_max;
    };

    class OBB
    {
    public:
        float3 Center; // center point.
        float3 extend; // half extend along axis
        float3 Axis[3]; // x y z
        int collision;
    };

    class Sphere
    {
    public:
        Sphere(const float3& c, float r)
        {
            Center = c;
            Radius = r;
        }

        // is the given point is inside this.
        bool Contain(const float3& pt) const;

        float3 Center;
        float Radius;
        int collision;        
    };

    class Cube
    {
    public:           
        float3 corners[8];
    };

    class Triangle
    {
    public:
        float3 A;
        float3 B;
        float3 C;

        int collision; 
        float3 Barycentric(float v, float w);

        // compute barycentric coord for point p
        // p = uA + vB + wC
        // output bar.x = u, bary.y = v,  bary.z = w;
        void Barycentric1(const float3 &P,float3 &bary) const;

        void Barycentric3(const float3 &P,float3 &bary) const;

        // // compute barycentric coord for point p
        // p = uA + vB + wC
        // output bar.x = u, bary.y = v,  bary.z = w;
        // using relative area.
        void Barycentric2(const float3 &p, float3 &bary) const;

        float GetArea() const
        {
            return length(cross((B-A),(C-A)))/2.0f;
        }

    };

    class Frustum : public NonCopyable
    {
    public: 
        Frustum(){}
        ~Frustum(){}
        enum Side { Near=0, Far, Left, Right,Top, Bottom,NumPlanes };
        enum corner {NearBottonLeft = 0, NearBottomRight, NearTopRight, NearTopLeft,
                     FarBottonLeft,FarBottomRight,FarTopRight, FarTopLeft};

        const Plane& NearPlane() const {return m_planes[Near];}
        const Plane& FarPlane() const {return m_planes[Far];}        
        const Plane& LeftPlane() const {return m_planes[Left];}
        const Plane& RightPlane() const {return m_planes[Right];}
        const Plane& TopPlane() const {return m_planes[Top];}        
        const Plane& BottomPlane() const {return m_planes[Bottom];}		

		const Plane& operator[](int i) const  { return m_planes[i];}
        const float3& Corner(int i ) const { return m_corners[i];}        
        void InitFromMatrix(const Matrix &viewproj);
        void InitFromCorners(float3* corners);
        const void GetCorners( float3* out_points) const;		
        
    private:
        Plane m_planes[6];   

        // corners must be specified as follow
        // front: botton-left  bottom-right, top-right, top-left.
        // back:  botton-left  bottom-right, top-right, top-left.
        float3 m_corners[8];

        Matrix m_matrix;
    };

     void CreateUnitCube(Cube &cube);

     
     inline float TriangleArea(const float3 &A, const float3 &B, const float3 &C)
     {
         return length(cross((B-A),(C-A)))/2.0f;
     }
     
     //================= closest point =============================
     float3 ClosestPointOnLineToPoint(const LineSeg &l, const float3 &p);
     float3 ClosestPointFromPlaneToPoint(Plane &plane, float3 &p);


     // test if point P is contained in triangle ABC
     bool TestPointTriangle(const Triangle &t, const float3 &P);
     
    
     //================= Intersect and collision test functions =================     
     bool TestAABBAABB(const AABB& a, const AABB& b);
     bool FrustumMeshIntersect(const Frustum& fr,
                               float3* pos, 
                               uint32_t posCount,
                               uint32_t* indices, 
                               uint32_t indicesCount);
                               
	 bool TestFrustumAABB(const Frustum& frustum, const AABB& box);
     int FrustumAABBIntersect(const Frustum& frustum, const AABB& box);

     bool IntersectRayAABB(const Ray& r, const AABB& box, float* out_tmin, float3* out_pos, float3* out_nor);

     bool IntersectionRayTriangle(const Ray &r, const Triangle &t, bool backfaceCull,
                                float* out_tmin, float3* out_pos, float3* out_nor);

     float IntersectionRayTriangle1(const Ray &r, const Triangle &t);

     float IntersectionRayPlane(const Ray &r, const Plane &p);

     bool MeshIntersects(const Ray& ray, float3* pos, uint32_t posCount, uint32_t* indices, uint32_t indicesCount,
                bool backfaceCull, float* out_tmin, float3* out_pos, float3* out_nor, float3* nearestVertex);

    bool DistanceRayToLineStrip(const Ray& ray, float3* pos,uint32_t posCount, const Matrix& worldXform,                 
                float* out_distTo, float* out_distBetween, float3* out_pos, float3* out_nor, uint32_t* out_hitIndex);


}
