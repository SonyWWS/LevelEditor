//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "CollisionPrimitives.h"
#include <algorithm>
#include <float.h>

namespace LvEdEngine
{
    // Is quad ABCD Convex,
    // vertex ABCD are in clockwise order.
    // assuming all the vertices of the quad lie on the same plane then
    // the quuad is convex iff its two diagonals are intersecting.
    // this test if equivalent to
    // Vertex B and D laying on  opposite sides of a line through AC.
    // and Vertex A and C are laying on the opposite side of a line through BD
    bool IsConvexQuad(const float3& A, const float3& B, const float3& C, const float3& D)
    {
        float3 AC = C - A;
        float3 AB = B - A;        
        float3 AD = D - A;        
        if( dot( cross(AB,AC), cross(AD,AC)) >= 0) return false;

        float3 BD = D - B;
        float3 BA = A - B;
        float3 BC = C - B;
        return dot( cross(BC,BD),  cross(BD,BA)) < 0;

    }

    void CreateUnitCube(Cube &cube)
    {
        float3 *corners = cube.corners;

        //upper rect. starting from +x +Z ClockWise
        corners[0] = float3(0.5f,0.5f,0.5f);
        corners[1] = float3(0.5f,0.5f,-0.5f);
        corners[2] = float3(-0.5f,0.5f,-0.5f);
        corners[3] = float3(-0.5f,0.5f,0.5f);

        // lower rect starting from +X +Z clockwise.
        corners[4] = float3(0.5f,-0.5f,0.5f);
        corners[5] = float3(0.5f,-0.5f,-0.5f);
        corners[6] = float3(-0.5f,-0.5f,-0.5f);
        corners[7] = float3(-0.5f,-0.5f,0.5f);
    }


    // test if point P is contained in triangle ABC
    bool LvEdEngine::TestPointTriangle(const Triangle &t, const float3 &P)
    {
        //method 1
        //float3 bary;
        //Barycentric1(t,bary);
        //return (bary.y >= 0 && bary.z >= 0 && (bary.y + bary.z) <= 1.0f);

        // method 2
        float3 ab = t.B - t.A;
        float3 bc = t.C - t.B;
        float3 ca = t.A - t.C;
        float3 n = cross(ab,bc);
        if(dot(n, cross(ab,(P-t.A)))<0) return false;
        if(dot(n, cross(bc,P-t.B))<0) return false;
        if(dot(n, cross(ca,P-t.C))<0) return false;
        return true;

    }

    //================== Plane ============
    Plane::Plane(const float3 &A,const float3 &B,const float3 &C)
    {
        normal = normalize( cross((B-A),(C-A)) );
        d = -dot(normal, A);
    }


    Plane::Plane(const float3 &P,const float3 &n)
    {
        normal = n;
        d = -dot(normal, P);
    }

    float Plane::Eval(const float3 &P) const
    {
        return ( (normal.x * P.x + normal.y * P.y + normal.z * P.z) + d);
    }


    void Bound2di::Extend(const Bound2di& box)
    {
        if(box.isEmpty()) return;
        x1 = std::min(x1,box.x1);
        y1 = std::min(y1,box.y1);
        x2 = std::max(x2,box.x2);
        y2 = std::max(y2,box.y2);
    }

    // ========================= Bound2di =================
    bool Bound2di::Intersect(const Bound2di& r1, const Bound2di& r2, Bound2di& out)
    {
        // early reject.
        if (r1.x2 <= r2.x1
            || r1.x1 >= r2.x2
            || r1.y1 >= r2.y2
            || r1.y2 <= r2.y1)
        {
            out.x1 = 0;
            out.x2 = 0;
            out.y1 = 0;
            out.y2 = 0;
            return false;
        }

        
        // find intersection rect.
        out.x1 = std::max(r1.x1, r2.x1);
        out.x2 = std::min(r1.x2, r2.x2);
        out.y1 = std::max(r1.y1, r2.y1);
        out.y2 = std::min(r1.y2, r2.y2);
        return true;

    }


    //======================= AABB =========================

    AABB::AABB(const float3 &min, const float3 &max): m_min(min), m_max(max)
    {
    }

    const float3& AABB::Min() const
    {
        return m_min;
    }

    const float3& AABB::Max() const
    {		 
        return m_max;
    }

    // Grows this box to include 'otherBox'.
    void AABB::Extend( const AABB& otherBox )
    {
        m_min = minimize( m_min, otherBox.m_min );
        m_max = maximize( m_max, otherBox.m_max );
    }

    // Grow this box to include point
    void AABB::Extend( const float3& point )
    {
        m_min = minimize( m_min, point );
        m_max = maximize( m_max, point);
    }

    
    void AABB::Transform(const Matrix &xform)
    {                  
        float3 newMin = float3(xform.M41,xform.M42,xform.M43);
        float3 newMax = newMin;

        for(int i = 0; i < 3; i++)
        {
            for(int j = 0; j < 3; j++)
            {
                float e = m_min[j] * xform(j,i);
                float f = m_max[j] * xform(j,i);
                if(e < f)
                {
                    newMin[i] +=  e;
                    newMax[i] +=  f;

                }
                else
                {
                    newMin[i] +=  f;
                    newMax[i] +=  e;
                }

            }
        }

        m_min = newMin;         
        m_max = newMax;         
    }

    bool AABB::Contain(const float3& pt) const
    {        
        if(pt.x > m_max.x) return false;
        if(pt.x < m_min.x) return false;

        if(pt.y > m_max.y) return false;
        if(pt.y < m_min.y) return false;

        if(pt.z > m_max.z) return false;
        if(pt.z < m_min.z) return false;

        return true;
    }

    // get the 8 corners of the aabb
    void AABB::Corners( float3 corners[8] ) const
    {
        corners[0] = float3(m_min.x, m_min.y, m_min.z);
        corners[1] = float3(m_min.x, m_max.y, m_min.z);
        corners[2] = float3(m_max.x, m_min.y, m_min.z);
        corners[3] = float3(m_max.x, m_max.y, m_min.z);
        corners[4] = float3(m_min.x, m_min.y, m_max.z);
        corners[5] = float3(m_min.x, m_max.y, m_max.z);
        corners[6] = float3(m_max.x, m_min.y, m_max.z);
        corners[7] = float3(m_max.x, m_max.y, m_max.z);
    }

    // is the given point is inside this.
    bool Sphere::Contain(const float3& pt) const
    {
        float dist_from_center = length( pt - Center);
        if(dist_from_center <= Radius)
        {
            return true;
        }
        return false;
    }


    //======================= Frustum ========================
  
    // corners must be specified as follow
    // front: botton-left  bottom-right, top-right, top-left.
    // back:  botton-left  bottom-right, top-right, top-left.
    void Frustum::InitFromCorners(float3* corners)
    {
        for(int i = 0; i < 8; i++)
        {
            m_corners[i] = corners[i];
        }

        m_planes[Near] = Plane(corners[0],corners[2],corners[1]);
        m_planes[Far]  = Plane(corners[4],corners[5],corners[6]);        
        m_planes[Top] = Plane(corners[2],corners[7],corners[6]);                
        m_planes[Bottom] = Plane(corners[1],corners[5],corners[4]);
        m_planes[Right] = Plane(corners[1],corners[6],corners[5]);                
        m_planes[Left] = Plane(corners[0],corners[4],corners[7]);
    }



    // extract frustum plane in world space.
    // the normals facing inward.
    // pass in view * projection matrix.
    void Frustum::InitFromMatrix(const Matrix &VP)
    {

        // near and far plane corners
        static float3 verts[8] =
        {
            // near plane corners
            float3( -1, -1,0 ),
            float3( 1, -1,0 ),
            float3( 1, 1,0 ),
            float3( -1, 1 ,0),

            // far plane corners.
            float3( -1, -1 ,1),
            float3( 1,  -1,1 ),
            float3( 1,  1,1 ),
            float3( -1, 1,1 )
        };

        this->m_matrix = VP;

        float4 col0(VP(0,0), VP(1,0), VP(2,0), VP(3,0));
        float4 col1(VP(0,1), VP(1,1), VP(2,1), VP(3,1));
        float4 col2(VP(0,2), VP(1,2), VP(2,2), VP(3,2));
        float4 col3(VP(0,3), VP(1,3), VP(2,3), VP(3,3));

        
        // Planes face inward.
        m_planes[Near]   = Plane(col2);        // near
        m_planes[Far]    = Plane(col3 - col2); // far
        m_planes[Left]   = Plane(col3 + col0); // left
        m_planes[Right]  = Plane(col3 - col0); // right
        m_planes[Top]    = Plane(col3 - col1); // top
        m_planes[Bottom] = Plane(col3 + col1); // bottom

        // normalize all six planes
        for(int i = 0; i < 6; i++)
        {
            m_planes[i].Normalize();
        }


        // tranform eight corner from device coordiate to world coordinate.
        Matrix invVP = VP;
        invVP.Invert();
        for(int i = 0; i < 8; i++)
        {
            this->m_corners[i] = float3::Transform(verts[i],invVP);
        }
    }

    const void Frustum::GetCorners( float3* out_points) const
    {
        for( int i = 0; i < 8; ++i)
        {
            out_points[i] = Corner(i);
        }
    }


    // ================== Triangle =========================
    // compute 3d point for the given weights.
    // P = uA + vB + wC where u + v + w = 1; u = 1-v-w;
    // p = (1-v-w)A + vB + wC
    // p = A + v(B-A) + w(C-A)
    float3 Triangle::Barycentric(float v, float w)
    {
        float3 p;

        p.x = A.x + v*(B.x - A.x) + w*(C.x - A.x);
        p.y = A.y + v*(B.y - A.y) + w*(C.y - A.y);
        p.z = A.z + v*(B.z - A.z) + w*(C.z - A.z);

        return p;         
    }

    // compute barycentric coord for point p
    // p = uA + vB + wC
    // output bar.x = u, bary.y = v,  bary.z = w;
    // P = uA + vB + wC where u + v + w = 1; u = 1-v-w;
    // p = (1-v-w)A + vB + wC
    // p = A + v(B-A) + w(C-A) ==> v(B-A) + w(C-A) = P - A     
    //  K0 = B-A,  K1 = C - A,  K2 = P - A
    //  vK0 + wK1 = K2
    // Dot is by K0 and K1 to get two equations
    // v(K0 dot K0) + w(K1 dot K0) = K2 dot K0
    // v(K0 dot K1) + w(K1 dot K1) = K2 dot K1
    // user cramar's rule to solve.
    void Triangle::Barycentric1(const float3 &P,float3 &bary) const
    {
    float3 K0 = B - A;
    float3 K1 = C - A;
    float3 K2 = P - A;

    float d00 = dot(K0,K0);
    float d01 = dot(K0,K1);
    float d11 = dot(K1,K1);

    float d20 = dot(K2,K0);
    float d21 = dot(K2,K1);

    // CoEff A = | d00   d01 | 
    //           | d01   d11 | 
    // Constant B = | d20 |
    //              | d21 |
    // Solution Vector X = | v |
    //                     | w |
    // AX = B  ==>  X = inv(A) * B
    // cramer's rule
    // det of coefficient
    // 
    //      | d20  d01 |
    //      | d21  d11 |
    // u =  -------------
    //      det(A)   
    //
    //     | d00  d20 |
    //     | d01  d21 |
    // v = --------------
    //      det(A)
    //
    //


    float detA = (d00 * d11) - (d01 * d01);
        
    bary.y = (d20 * d11 - d01 * d21) / detA;
    bary.z = (d00 * d21 - d20 * d01) / detA;
    bary.x = 1 - bary.y - bary.z; // u = 1 - v - w
    }



    void Triangle::Barycentric2(const float3 &P, float3 &bary) const
    {
        float abc = GetArea();
        bary.x = TriangleArea(P,B,C)/abc;
        bary.y = TriangleArea(A,P,C)/abc;
        bary.z = 1.0f -bary.x - bary.y;
    }

    void Triangle::Barycentric3(const float3 &P,float3 &bary) const
    {
        float3 Q = cross(B,C);
        float3 M = cross(A,P);
        float det = dot(A,Q);
        bary.x = dot(P,Q)/det;
        bary.y = dot(C,M)/det;
        //bary.z = dot(B,-M)/det;
        bary.z = 1-bary.x - bary.y;
    }
     

    //================= closest point =====================================


    // find close point on line l that is closest to point p.
    // see arg l and p.
    float3 ClosestPointOnLineToPoint(const LineSeg &l, const float3 &p)
    {
        float3 ab = l.B - l.A;
        float t = dot((p-l.A), ab) / lengthsquared(ab);
        t = clamp(t,0.0f,1.0f);
        return l.A + t * ab;
    }

    // find a point on the plane that is closest to p.
    // assume plane is normalized.
    float3 ClosestPointFromPlaneToPoint(Plane &plane, float3 &p)
    {
        // Let R be a point on plane closest to p.
        // project point p on to plane
        // R(t) = p - t·N //
        // N·X + D = 0;  // plance equation
        // N · (p-tN)+D = 0; 
        // N·P - tN·N + D = 0; ==> t = N·P - D;
        // R(t) = p - (N·P - D)·N;

        float t = dot(plane.normal, p) - plane.d;
        float3 R = p - t * plane.normal;
        return p;
    }


    //======================= collision test functions ======================

    // a and b are not overlapping if they are seperated on any major axis
    bool TestAABBAABB(const AABB& a, const AABB& b)
    {
        if(a.Max().x < b.Min().x || a.Min().x > b.Max().x) return false;
        if(a.Max().z < b.Min().z || a.Min().z > b.Max().z) return false;
        if(a.Max().y < b.Min().y || a.Min().y > b.Max().y) return false;
        return true;
    }

    bool IntersectRayAABB(const Ray& r, const AABB& a, float* out_tmin, float3* out_pos, float3* out_nor)
    {
        const float3& p = r.pos;
        const float3& d = r.direction;
        float tmin = -FLT_MAX;
        float tmax = FLT_MAX;
        float3 minNorm, maxNorm;
        
        // check vs. all three 'slabs' of the aabb
        for(int i = 0; i < 3; ++i)
        {
            if(abs(d[i]) < Epsilon) 
            {   // ray is parallel to slab, no hit if origin not within slab
                if(p[i] < a.Min()[i] || p[i] > a.Max()[i] )
                {
                    return false;
                }
            }
            else
            {
                // compute intersection t values of ray with near and far plane of slab
                float ood = 1.0f / d[i];
                float t1 = (a.Min()[i] - p[i]) * ood;
                float t2 = (a.Max()[i] - p[i]) * ood;
                tmin = maximize(tmin, minimize(t1, t2));
                tmax = minimize(tmax, maximize(t1, t2));

                // exit with no collision as soon as slab intersection becomes empty
                if(tmin > tmax) 
                {
                    return false;
                }
            }
        }
        
        if(tmax < 0.f)
        {
            // entire bounding box is behind us
            return false;
        }
        else if(tmin < 0.f)
        {
            // we are inside the bounding box
            *out_tmin = 0.f;
            *out_pos = p;
            *out_nor = normalize(a.GetCenter() - (*out_pos)); // use 'sphere' type normal calculation to approximate.
            return true;
        }
        else
        {
            // ray intersects all 3 slabs. return point and normal of intersection
            *out_tmin = tmin;
            *out_pos = p + d * tmin;
            *out_nor = normalize(a.GetCenter() - (*out_pos)); // use 'sphere' type normal calculation to approximate.
            return true;
        }
    }



    // ray triangle intersection 
    // approaches:
    // 1- compute ray plane intersection
    //    test if the point is inside all edges
    //    or compute barycentric weights for the point.     
    float IntersectionRayTriangle1(const Ray &r, const Triangle &t)
    {
        Plane p(t.A,t.B,t.C);
        
        float dist = IntersectionRayPlane(r, p);
        if(dist < 0) return dist;
        float3 Q = r.pos + dist * r.direction;
        
        
        float3 ab = t.B - t.A;
        float3 bc = t.C - t.B;
        float3 ca = t.A - t.C;
        float3 n = cross(ab,bc);
        if(dot(n, cross(ab,(Q-t.A)))<0) return -1;
        if(dot(n, cross(bc,Q-t.B))<0) return -1;
        if(dot(n, cross(ca,Q-t.C))<0) return -1;
        return dist;

        //float3 bary;
        //t.Barycentric1(Q,bary);
        //if(bary.x >= 0 && bary.y >= 0 && (bary.x + bary.y) <= 1)
        //    return dist;
        //else 
        //    return -1;
    }

    // see Fast RayTriangle Intersection.pdf in C:\Users\Alan\Projects\EduRes
    bool IntersectionRayTriangle(const Ray &r, const Triangle &t, bool backfaceCull,
                                float* out_tmin, float3* out_pos, float3* out_nor)
    {         
        float3 E1 = t.B - t.A;
        float3 E2 = t.C - t.A;        
        float3 P =   cross(r.direction ,E2);
        float det = dot(P, E1);
        
        // det is the same as the dot(r.direction, cross(E1, E2))
        // when det is < 0, r.direction is opposite direction then winding order
        // So if backfaceCull'ing and det < 0, then return false
        // If not backfaceCull'ing, then just check if det is close to zero
        if (det < Epsilon && (backfaceCull || det > -Epsilon)) return false;

        float3 K = r.pos - t.A;
        float3 Q = cross(K,E1);
        float u = dot(P, K) / det;
        if (u < 0.f || u > 1.f) return false;
        float v = dot(Q, r.direction) / det;
        if (v < 0.f || (u+v) > 1.f) return false;
         
        // if need to compute v and u.
        //u /= det;
        //v /= det;
        float tmin = dot(Q, E2)/det;
        if(tmin > 0)
        {
            *out_tmin = tmin;
            *out_pos = r.pos + tmin * r.direction;
            *out_nor = normalize(cross(E1, E2));
            return true;
        }
        else
        {
            return false;
        }                 
    }

    float IntersectionRayPlane(const Ray &r, const Plane &p)
    {
        float nd = dot(p.normal, r.direction); 
        if( abs(nd) < Epsilon) return -1; // no intersection.
        float t = (-p.d - dot(p.normal, r.pos))/nd;
        return t;
    }


    bool MeshIntersects(const Ray& ray, float3* pos, uint32_t posCount, uint32_t* indices, uint32_t indicesCount,
                        bool backfaceCull, float* out_tmin, float3* out_pos, float3* out_nor, float3* nearestVertex)
    {        
        if(posCount == 0) 
            return false;

        uint32_t LastTri = indicesCount -3;
        bool hit = false;
        bool tri_hit = false;
        float dist = FLT_MAX;
        float hit_dist = FLT_MAX;
        float3 hit_pos;
        float3 hit_nor;
        Triangle tri;

        for( uint32_t i = 0; i <= LastTri; i+=3 )
        {
            assert(indices[i] < posCount);
            assert(indices[i+1] < posCount);
            assert(indices[i+2] < posCount);

            tri.A = pos[ indices[i]];
            tri.B = pos[ indices[i+1]];
            tri.C = pos[ indices[i+2]];
            
            tri_hit = IntersectionRayTriangle(ray, tri, backfaceCull, &hit_dist, &hit_pos, &hit_nor);            
            if(tri_hit)
            {                
                hit = true;
                if(hit_dist < dist)
                {
                    dist = hit_dist;
                    *out_tmin = hit_dist;
                    *out_pos = hit_pos;
                    *out_nor = hit_nor;

                    float distA = lengthsquared(hit_pos - tri.A);
                    float distB = lengthsquared(hit_pos - tri.B);
                    float distC = lengthsquared(hit_pos - tri.C);
                    
                    if(distA <= distB && distA <= distC)
                        *nearestVertex = tri.A;
                    else if( distB < distC)
                        *nearestVertex = tri.B;
                    else
                        *nearestVertex = tri.C;

                }
            }
           
        }  
        return hit;
    }

    void DistancePointToPoint(const float3& p1, const float3& p2, float* out_distTo, float* out_distBetween, float3* out_pos, float3* out_nor)
    {
        float3 p1p2 = p2 - p1;
        *out_distTo = length(p1p2);
        *out_distBetween = *out_distTo;
        *out_pos = p2;
        *out_nor = normalize(p1p2);
    }

    void DistancePointToSegment(const LineSeg & segment, const float3& point, float* out_distTo, float* out_distBetween, float3* out_pos, float3* out_nor)
    {
        float3 AtoB = segment.B - segment.A;
        float3 direction = normalize(AtoB);
        float3 AtoPoint = point - segment.A;
        float t = dot(AtoPoint, direction);
        if (t < 0.f)
        {
            DistancePointToPoint(segment.A, point, out_distTo, out_distBetween, out_pos, out_nor);
        }
        else if (t > length(AtoB))
        {
            DistancePointToPoint(segment.B, point, out_distTo, out_distBetween, out_pos, out_nor);
        }
        else
        {
            *out_distTo = t;
            *out_pos = segment.A + t * direction;
            float3 pointToRay = *out_pos - point;
            *out_distBetween = length(pointToRay);
            *out_nor = normalize(pointToRay);
        }
    }

    void DistanceRayToPoint(const Ray& ray, const float3& point, float* out_distTo, float* out_distBetween, float3* out_pos, float3* out_nor)
    {
        // Find closest distance between ray and point:
        //      Construct perpendicular to ray passing through point
        //      Project ray.pos-to-point onto ray.direction to find where on ray this perpendicular occurs.
        //      If this point is behind ray, then just use point-to-point
        float3 rayPosToPoint = point - ray.pos;
        float tRay = dot(rayPosToPoint, ray.direction);

        // if point is behind ray then just use point-to-point
        if (tRay < 0.f)
        {
            DistancePointToPoint(point, ray.pos, out_distTo, out_distBetween, out_pos, out_nor);
        }
        else
        {
            *out_distTo = tRay;
            *out_pos = ray.pos + tRay * ray.direction;
            float3 pointToRay = *out_pos - point;
            *out_distBetween = length(pointToRay);
            *out_nor = normalize(pointToRay);
        }
    }

    void DistanceRayToSegment(const Ray& ray, const LineSeg& segment,
                                float* out_distTo, float* out_distBetween, float3* out_pos, float3* out_nor)
    {
        // Find closest distance between ray and segment:
        //      Construct seg ray:
        //          seg.direction = normal(segment.B - segment.A)
        //          seg.pos = segment.A
        //
        //      Construct plane through ray.pos that contains both ray.direction and segment.direction
        //      Construct second plane through segment.pos that contains both ray.direction and segment.direction
        //      The normal of both these planes will be:
        //          planeNormal = normal(cross(ray.direction, seg.direction))
        //          planeR . planeNormal = ray.pos . planeNormal
        //          planeS . planeNormal = seg.pos . planeNormal
        //
        //      The distance between these planes will be ray-to-seg projected on the planeNormal:
        //          distRayPosToSegment = (seg.pos - ray.pos) . planeNormal
        //
        // Now we need to find where this closest point occurs to return distanceToClosest and check if
        // it actually occurs between segment.A and segment.B:
        //          rayLine = ray.pos + tRay * ray.direction
        //          segLine = seg.pos + tSeg * seg.direction
        //
        //      Since we already have the normal and distance between ray and segment:
        //          rayLine + distBetweenRaySegment * planeNormal = segLine
        //
        //      By setting components equal we can solve for tRay and tSeg:
        //                                 (B * seg.direction.x + A * seg.direction.y)
        //          tRay =  ---------------------------------------------------------------------------
        //                  (ray.direction.y * seg.direction.x - ray.direction.x * seg.direction.y)
        //
        //          where
        //          A = ray.pos.x - seg.pos.x + distBetweenRaySegment * planeNormal.x
        //          B = seg.pos.y - ray.pos.y - distBetweenRaySegment * planeNormal.y
        //
        //          tSeg = (A + tRay * ray.direction.x) / seg.direction.x
        //
        //      if      tSeg < 0                                =>  use point-to-ray-distance(segment.A, ray)
        //      else if tSeg > length(segment.B - segment.A)    =>  use point-to-ray-distance(segment.B, ray)
        //      else                                            =>  distToClosest = tRay
        //                                                          posClosest = tRay * ray.direction + ray.position
        //

        Ray seg(segment.A, segment.B - segment.A);

        // check for parallel seg and ray

        float3 planeNormal = normalize(cross(ray.direction, seg.direction));
        float distBetweenRaySegment = dot(seg.pos - ray.pos, planeNormal);
        float A = ray.pos.x - seg.pos.x + distBetweenRaySegment * planeNormal.x;
        float B = seg.pos.y - ray.pos.y - distBetweenRaySegment * planeNormal.y;
        float tRay = (B * seg.direction.x + A * seg.direction.y) /
                        (ray.direction.y * seg.direction.x - ray.direction.x * seg.direction.y);
        float tSeg = (A + tRay * ray.direction.x) / seg.direction.x;

        float segmentLength = length(segment.B - segment.A);

        if (tRay < 0) // the segment is behind us
        {
            DistancePointToSegment(segment, ray.pos, out_distTo, out_distBetween, out_pos, out_nor);
        }
        else if (tSeg < 0) // closest is before segment.A
        {
            DistanceRayToPoint(ray, segment.A, out_distTo, out_distBetween, out_pos, out_nor);
        }
        else if (tSeg > segmentLength) // closest is after segment.B
        {
            DistanceRayToPoint(ray, segment.B, out_distTo, out_distBetween, out_pos, out_nor);
        }
        else
        {
            *out_distTo = tRay;
            *out_distBetween = fabsf(distBetweenRaySegment);
            *out_pos = ray.pos + tRay * ray.direction;
            *out_nor = planeNormal;
        }
    }

    bool DistanceRayToLineStrip(const Ray& ray, float3* pos,uint32_t posCount, const Matrix& worldXform,                 
                float* out_distTo, float* out_distBetween, float3* out_pos, float3* out_nor, uint32_t* out_hitIndex)
    {
        uint32_t LastSeg = posCount - 1;
        OBB box;
        float closestScreenRatio = FLT_MAX;
        float hit_dist,hit_distBetween;
        float3 hit_pos,hit_nor;

        for( uint32_t i = 0; i < LastSeg; i+=1)
        {
            float3 A = float3::Transform(pos[i], worldXform);
            float3 B = float3::Transform(pos[i+1], worldXform);
            LineSeg segment(A, B);
            DistanceRayToSegment(ray, segment, &hit_dist, &hit_distBetween, &hit_pos, &hit_nor);
            float screenRatio = hit_distBetween / hit_dist;
            if (screenRatio >= 0.f && screenRatio < closestScreenRatio)
            {
                closestScreenRatio = screenRatio;
                *out_distTo = hit_dist;
                *out_distBetween = hit_distBetween;
                *out_pos = hit_pos;
                *out_nor = hit_nor;
                *out_hitIndex = i;
            }
        }

        return (closestScreenRatio < FLT_MAX);
    }

    //-----------------------------------------------------------------------------
    // Plane vs AABB test.
    //
    // Return values: 
    //              0 = (Front) There is no intersection, and the box is in 
    //                   the positive half-space of the Plane. 
    //                  
    //              1 = (Back) There is no intersection, and the box is in 
    //                  the negative half-space of the Plane.
    //              2 = (Intersecting) the box intersects the plane.
    //-----------------------------------------------------------------------------
    int PlaneAABBIntersection(const Plane& plane, const AABB& box)
    {
        float3 c  = box.GetCenter();
        float3 r  = box.Max() - c;
        float e = r.x * abs(plane.normal.x) 
                + r.y * abs(plane.normal.y) 
                + r.z * abs(plane.normal.z);
        float s = plane.Eval(c);
        if((s - e) > 0)  return 0; // front side
        if((s + e) < 0)  return 1; // back side
        return 2; // Intersecting
    }


    //-----------------------------------------------------------------------------
    // AABB vs Frustum test.     
    //
    // Return values: 0 = no intersection, 
    //                1 = intersection, 
    //                2 = box is completely inside frustum    
    //
    //-----------------------------------------------------------------------------
    int FrustumAABBIntersect(const Frustum& frustum, const AABB& box)
    {
        float3 c  = box.GetCenter();
        float3 r  = box.Max() - c;
        bool intersects = false;        
        for(int i = 0; i < 6; ++i)
        {
            const Plane& plane = frustum[i];
            // test plane and AABB intersection.
            float e =  r.x * abs(plane.normal.x) 
                + r.y * abs(plane.normal.y) 
                + r.z * abs(plane.normal.z);
            float s = plane.Eval(c);

           // assert(e > 0);
            // the box is in positive half-space.
            if((s - e) > 0) continue;
            
            // if box is completely in negative side of the plain 
            // then the box is outside frustum.
            if( (s + e) < 0) return 0; // no intersection, early exit.            
            intersects = true;             
        }    
        if(intersects) return 1;
        return 2; // either intersects or completely inside frustum      
    }
    //-----------------------------------------------------------------------------
    // AABB vs frustum test.
    //
    // Return values: false = no intersection, 
    //                true  = either the box intersections or it is fully containt.
    // note: this test is fast but not 100% accurate (it is good for viewfrustum culling)          
    //-----------------------------------------------------------------------------
    bool TestFrustumAABB(const Frustum& frustum, const AABB& box)
	{              
        float3 c  = box.GetCenter();
        float3 r  = box.Max() - c;        
        for(int i = 0; i < 6; ++i)
        {
            const Plane& plane = frustum[i];
            // test plane and AABB intersection.
            float e =  r.x * abs(plane.normal.x) 
                + r.y * abs(plane.normal.y) 
                + r.z * abs(plane.normal.z);
            float s = plane.Eval(c);
            
            // if box is completely in negative side of the plain 
            // then the box is outside frustum.
            if( (s + e) < 0)  return false; // no intersection, early exit.               
        }            
        return true; // either intersects or completely inside frustum         
    }

 
    //-----------------------------------------------------------------------------
    // Frustum Triangle intersection  using separating axis test.
    // note: the frustum and the triangle must be in the same space.
    //       optimization needed
    bool FrustumTriangleIntersect(const Frustum& fr, const Triangle& tri)
    {            
        bool allInside = true;        
        for(int i = 0; i < 6; ++i)
        {
            const Plane& plane = fr[i];
            float d1 = plane.Eval(tri.A);
            float d2 = plane.Eval(tri.B);
            float d3 = plane.Eval(tri.C); 

            // if all outside a single plane, then there is
            // no intersection.
            if( d1 < 0 && d2 < 0 && d3 < 0) 
                return false;
            allInside = allInside && ( d1 > 0 && d2 > 0 && d3 > 0);            
        }

        // the tri is completely inside the frustum.
        if( allInside ) 
            return true; 

        // separating axis test.
        // Triangle:  3 edges  1 face normal.
        // Frustum:   6 edges, 5 face normal.
        // for total of 24 separating axis.
        // note this test can be optimized.
                        
        // tri edges
        float3 triEdges[3];
        triEdges[0] = tri.B - tri.A;
        triEdges[1] = tri.C - tri.A;
        triEdges[2] = tri.C - tri.B;

        // frustum edges
        float3 FrEdges[6];
        FrEdges[0] = fr.Corner(Frustum::NearTopLeft) - fr.Corner(Frustum::NearTopRight);
        FrEdges[1] = fr.Corner(Frustum::NearBottomRight) - fr.Corner(Frustum::NearTopRight);
        FrEdges[2] = (fr.Corner(Frustum::FarBottonLeft) - fr.Corner(Frustum::NearBottonLeft));
        FrEdges[3] = (fr.Corner(Frustum::FarBottomRight) - fr.Corner(Frustum::NearBottomRight));
        FrEdges[4] = (fr.Corner(Frustum::FarTopRight) - fr.Corner(Frustum::NearTopRight));
        FrEdges[5] = (fr.Corner(Frustum::FarTopLeft) - fr.Corner(Frustum::NearTopLeft));

        int k = 0;        
        float3 Axis[24];                
        Axis[k++] = fr.TopPlane().normal;
        Axis[k++] = fr.BottomPlane().normal;
        Axis[k++] = fr.LeftPlane().normal;
        Axis[k++] = fr.RightPlane().normal;
        Axis[k++] = fr.NearPlane().normal;
        Axis[k++] = normalize(cross(triEdges[0], triEdges[1]));

        for(int te = 0;  te < 3; te++)
        {
            for(int fe = 0; fe < 6; fe++)
            {
                float3 axis = cross( triEdges[te], FrEdges[fe]); 
                Axis[k++] = normalize(axis);                
            }
        }
                
        for(int n = 0; n < 24; n++)
        {
            float3 axis = Axis[n];
            if( lengthsquared(axis) < Epsilon) 
                continue;
            float trid1 = dot(tri.A,axis);
            float trid2 = dot(tri.B,axis);
            float trid3 = dot(tri.C,axis);

            float trMin = std::min(trid1,trid2);
            trMin = std::min(trMin, trid3);
            float trMax = std::max(trid1,trid2);
            trMax = std::max(trMax,trid3);

            float frMin = dot(fr.Corner(0),axis);
            float frMax = frMin;
            for(int c = 1; c < 8; c++)
            {
                float fdist = dot(fr.Corner(c), axis);
                frMin = std::min(frMin,fdist);
                frMax = std::max(frMax,fdist);
            }
           
            if( (trMax < frMin) || (frMax < trMin))
            {               
                return false;                
            }
        }

        // must be intersecting.
        return true;
               
    }


    
    // loop over all the triagle in the mesh and use frustum triangle intersection.
    // note: the frustum and the mesh must be in the same space.    
    bool FrustumMeshIntersect(const Frustum& fr,
                              float3* pos, 
                              uint32_t posCount,
                              uint32_t* indices, 
                              uint32_t indicesCount)
    {      

        if(posCount == 0) return false;        
        uint32_t LastTri = indicesCount -3;        
        Triangle tri;

        uint32_t i = 0;
        for( ; i <= LastTri; i+=3 )
        {
            assert(indices[i] < posCount);
            assert(indices[i+1] < posCount);
            assert(indices[i+2] < posCount);
           
            tri.A = pos[indices[i]];
            tri.B = pos[indices[i+1]];
            tri.C = pos[indices[i+2]];
            if(FrustumTriangleIntersect(fr, tri))
            {                
                return true;               
            }
        }          
        return false;
    }

}
