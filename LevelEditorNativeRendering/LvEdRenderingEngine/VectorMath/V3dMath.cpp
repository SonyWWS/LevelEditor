//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#include "V3dMath.h"
#include <math.h>

namespace LvEdEngine
{

    const float E = 2.71828f;
    const float Log10E = 0.434294f;
    const float Log2E = 1.4427f;
    const float Pi = 3.141592654f; 
    const float OneDivPi = 0.318309886f; 
    const float PiOver2 = (1.570796327f);
    const float PiOver4 = (0.785398163f);
    const float TwoPi = 6.283185307f;
    const float PiOver180 = 0.017453293f;
    const float InvPiOver180 = 57.2957795f;
    const float Infinity = FLT_MAX;
    const float Epsilon  = 0.000000001f;
    
     static float3 CalcTangent(float3 p1, float3 p2, float3 p3)
     {
         float3 v1 = normalize(p1 - p2);
         float3 v2 = normalize(p3 - p2);
         float3 bisector = v1 + v2;
         float3 normal =cross(v1, v2);
         if (length(normal) < Epsilon) // 3 points  colinear
         {
             return v2;
         }
         float3 tangent = cross(normal, bisector);
         tangent = normalize(tangent);
         return tangent;
     }

      static float3 CalcEndTangents(float3 p1, float3 p2, float3 p2Tangent)
      {

          float3 v1 = p1 - p2;
          float v1Len = length(v1);
          float3 u1 = normalize(v1);

          // Make sure that v1 and p2Tangent are facing the same way
          if (dot(v1, p2Tangent) < 0)
              p2Tangent = -p2Tangent;

          // calc t for which the tangent vector t*p2Tangent is projected on 1/2 of v1
          float t = (0.5f * v1Len) / dot(p2Tangent, u1);
          float3 p3 = p2 + (p2Tangent * t);
          float3 tangent = normalize(p3 - p1);
          return tangent;
      }

     static void CalcPointTangents(float3* p, float3* t, int count, bool isClosed)
     {
         assert(count > 2);
         if( count < 2 ) return;

         
         for (int i = 1; i < count - 1; i++)
         {
             t[i] = CalcTangent(p[i - 1], p[i], p[i + 1]);
         }

         if (isClosed)
         {
             t[0] = CalcTangent(p[count - 1], p[0], p[1]);
             t[count - 1] = CalcTangent(p[count - 2], p[count - 1], p[0]);
         }
         else
         {
             t[0] = CalcEndTangents(p[0], p[1], t[1]);
             t[count - 1] = CalcEndTangents(p[count - 2], p[count - 1], t[count - 2]);
         }         
     }

    static void BuildCurves(float3* points, int count, bool isClosed,
        std::vector<BezierCurve>* out)
    {
        assert(count > 1);

        float3 cp[4];
        if(count == 2)
        {
            cp[0] = points[0];
            cp[3] = points[1];
            cp[1] = cp[0] + 0.3333f * (cp[3] - cp[0]);
            cp[2] = cp[0] + 0.6666f * (cp[3] - cp[0]);
            out->push_back(BezierCurve(cp));
        }
        else
        {
            float3 zerov(0.0f,0.0f,0.0f);
            std::vector<float3> tangents(count,zerov);            
            CalcPointTangents(points,&tangents[0],count,isClosed);
            for (int i = 1; i < count; i++)
            {
                float3 chord = points[i] - points[i - 1];
                float segLen = length(chord) * 0.3333f;                
                cp[0] = points[i - 1];
                cp[3] = points[i];

                float3 tangent1 = tangents[i - 1];
                if (dot(chord, tangent1) < 0)
                    tangent1 = -tangent1;
                cp[1] = cp[0] + (tangent1 * segLen);


                float3 tangent2 = tangents[i];
                if (dot(-chord, tangent2) < 0)
                    tangent2 = -tangent2;
                cp[2] = cp[3] + (tangent2 * segLen);
               
                BezierCurve curve(cp);
                out->push_back(curve);
            }

            // Calculate last curve if is closed
            if (isClosed)
            {           
                float3 lastcp1 = cp[1];
                float3 lastcp2 = cp[2];
                
                cp[0] = points[count - 1];
                cp[3] = points[0];
                
                BezierCurve lastCurve = out->back();
                float tanLen = length(cp[3] - cp[0]) * 0.3333f;
               
                float3 v = normalize(lastCurve.GetControlPoint(2) - cp[0]);                
                cp[1] = cp[0] - (v * tanLen);

                BezierCurve firstCurve = out->front();

                v = normalize(firstCurve.GetControlPoint(1) - cp[3]);
                cp[2] = cp[3] - (v * tanLen);
              
                BezierCurve curve(cp);
                out->push_back(curve);
            }
        }
    }
   
    //************** BezierSpline  class **********************************
    BezierSpline::BezierSpline(float3* points, int count, bool isClosed)
    {
      assert(count > 1);
      BuildCurves(points,count,isClosed,&m_curves);
    }


    float3 Vec3CatmullRom(const float3 &p0
        ,const float3 &p1
        ,const float3 &p2
        ,const float3 &p3
        ,float t)
    {
        float t2 = t * t;
        float t3 = t * t2;
        float3 n = (2.0f * p1) + 
                   (p2-p0) * t + 
                   (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t2 +
                   (3.0f * p1 - p0 + p3 - 3.0f * p2)  * t3;
        return (n * 0.5f);
    }
  

    // ************************* color
    //-------------------------------------------------------------------------------------------------
void ConvertColor(int color, float4* out)
{
    *out = float4((float)((color>>16) & 0xFF)/255.0f, (float)((color>>8) & 0xFF)/255.0f, (float)((color>>0) & 0xFF)/255.0f, (float)((color>>24) & 0xFF)/255.0f );
}

//-------------------------------------------------------------------------------------------------
void ConvertColor( int color , float3* out)
{
    float4 lcolor;
    ConvertColor(color, &lcolor);
    *out = float3(lcolor.x, lcolor.y, lcolor.z);
}




    //********************************* Matrix class **********************************

    // binary operators
    Matrix Matrix::operator * ( const Matrix& mat) const
    {
        Matrix m;
        m.M11 = (((M11 * mat.M11) + (M12 * mat.M21)) + (M13 * mat.M31)) + (M14 * mat.M41);
        m.M12 = (((M11 * mat.M12) + (M12 * mat.M22)) + (M13 * mat.M32)) + (M14 * mat.M42);
        m.M13 = (((M11 * mat.M13) + (M12 * mat.M23)) + (M13 * mat.M33)) + (M14 * mat.M43);
        m.M14 = (((M11 * mat.M14) + (M12 * mat.M24)) + (M13 * mat.M34)) + (M14 * mat.M44);
        m.M21 = (((M21 * mat.M11) + (M22 * mat.M21)) + (M23 * mat.M31)) + (M24 * mat.M41);
        m.M22 = (((M21 * mat.M12) + (M22 * mat.M22)) + (M23 * mat.M32)) + (M24 * mat.M42);
        m.M23 = (((M21 * mat.M13) + (M22 * mat.M23)) + (M23 * mat.M33)) + (M24 * mat.M43);
        m.M24 = (((M21 * mat.M14) + (M22 * mat.M24)) + (M23 * mat.M34)) + (M24 * mat.M44);
        m.M31 = (((M31 * mat.M11) + (M32 * mat.M21)) + (M33 * mat.M31)) + (M34 * mat.M41);
        m.M32 = (((M31 * mat.M12) + (M32 * mat.M22)) + (M33 * mat.M32)) + (M34 * mat.M42);
        m.M33 = (((M31 * mat.M13) + (M32 * mat.M23)) + (M33 * mat.M33)) + (M34 * mat.M43);
        m.M34 = (((M31 * mat.M14) + (M32 * mat.M24)) + (M33 * mat.M34)) + (M34 * mat.M44);
        m.M41 = (((M41 * mat.M11) + (M42 * mat.M21)) + (M43 * mat.M31)) + (M44 * mat.M41);
        m.M42 = (((M41 * mat.M12) + (M42 * mat.M22)) + (M43 * mat.M32)) + (M44 * mat.M42);
        m.M43 = (((M41 * mat.M13) + (M42 * mat.M23)) + (M43 * mat.M33)) + (M44 * mat.M43);
        m.M44 = (((M41 * mat.M14) + (M42 * mat.M24)) + (M43 * mat.M34)) + (M44 * mat.M44);
        return m;
    }

    Matrix Matrix::CreateFromAxisAngle(const float3 &axis, float angle)
    {        
    
       float3 vcAxis = axis;
       float fCos = cosf(angle);
       float fSin = sinf(angle);
       float fSum = 1.0f - fCos;   
       if (length(vcAxis) != 1.0f)
          vcAxis = normalize(vcAxis);

       Matrix mat;
       mat.M11 = (vcAxis.x * vcAxis.x) * fSum + fCos;
       mat.M12 = (vcAxis.x * vcAxis.y) * fSum - (vcAxis.z * fSin);
       mat.M13 = (vcAxis.x * vcAxis.z) * fSum + (vcAxis.y * fSin);

       mat.M21 = (vcAxis.y * vcAxis.x) * fSum + (vcAxis.z * fSin);
       mat.M22 = (vcAxis.y * vcAxis.y) * fSum + fCos ;
       mat.M23 = (vcAxis.y * vcAxis.z) * fSum - (vcAxis.x * fSin);

       mat.M31 = (vcAxis.z * vcAxis.x) * fSum - (vcAxis.y * fSin);
       mat.M32 = (vcAxis.z * vcAxis.y) * fSum + (vcAxis.x * fSin);
       mat.M33 = (vcAxis.z * vcAxis.z) * fSum + fCos;
       mat.M14 = mat.M24 = mat.M34 = mat.M41 = mat.M42 = mat.M43 = 0.0f;
       mat.M44 = 1.0f;
       return mat;
    }


    Matrix Matrix::CreateRotationX(float angle)
    {
        Matrix mat;
        float fCos = cosf(angle);
        float fSin = sinf(angle);

        mat.M22 =  fCos;
        mat.M23 =  fSin;
        mat.M32 = -fSin;
        mat.M33 =  fCos;

        mat.M11 = mat.M44 = 1.0f;
        mat.M12 = mat.M13 = mat.M14 = mat.M21 = mat.M24 = mat.M31 = mat.M34 = mat.M41 = mat.M42 = mat.M43 = 0.0f;
        return mat;
    }
    Matrix Matrix::CreateRotationY(float angle)
    {
        Matrix mat;
        float fCos = cosf(angle);
        float fSin = sinf(angle);

        mat.M11 =  fCos;
        mat.M13 = -fSin;
        mat.M31 =  fSin;
        mat.M33 =  fCos;

        mat.M22 = mat.M44 = 1.0f;
        mat.M12 = mat.M23 = mat.M14 = mat.M21 = mat.M24 = mat.M32 = mat.M34 = mat.M41 = mat.M42 = mat.M43 = 0.0f;
        return mat;

    }
    Matrix Matrix::CreateRotationZ(float angle)
    {
       Matrix mat;
       float fCos = cosf(angle);
       float fSin = sinf(angle);

       mat.M11  =  fCos;
       mat.M12  =  fSin;
       mat.M21  = -fSin;
       mat.M22  =  fCos;

       mat.M33 = mat.M44 = 1.0f;
       mat.M13 = mat.M14 = mat.M23 = mat.M24 = mat.M31 = mat.M32 = mat.M34 = mat.M41 = mat.M42 = mat.M43 = 0.0f;
       return mat;
    }
        
    Matrix Matrix::CreateLookAtRH(const float3 &eye,const float3& at, const float3& up)
    {
       
        /*
          zaxis = normal(Eye - At)
          xaxis = normal(cross(Up, zaxis))
          yaxis = cross(zaxis, xaxis)

          xaxis.x           yaxis.x           zaxis.x          0
          xaxis.y           yaxis.y           zaxis.y          0
          xaxis.z           yaxis.z           zaxis.z          0
         -dot(xaxis, eye)  -dot(yaxis, eye)  -dot(zaxis, eye)  1

          */

        Matrix mat;
        memset(&mat.M11, 0, sizeof(Matrix));

        float3 zaxis = normalize(eye - at);
        float3 xaxis = normalize( cross(up, zaxis));
        float3 yaxis = cross(zaxis, xaxis);

        mat.M11 = xaxis.x;
        mat.M12 = yaxis.x;
        mat.M13 = zaxis.x;
        mat.M21 = xaxis.y;
        mat.M22 = yaxis.y;
        mat.M23 = zaxis.y;
        mat.M31 = xaxis.z;
        mat.M32 = yaxis.z;
        mat.M33 = zaxis.z;
        mat.M41 = -dot(xaxis, eye);
        mat.M42 = -dot(yaxis, eye);
        mat.M43 = -dot(zaxis, eye);
        mat.M44 = 1.0f;

        return mat;
    }
    
    Matrix Matrix::CreateOrthographicOffCenter(float minx, float maxx, float miny, float maxy, float zn, float zf)
    {        
        //  FLOAT l,   // minx
        //  FLOAT r,   //maxx
        //  FLOAT b,   // miny
        //  FLOAT t,  // maxy                

        /*2/(r-l)      0            0           0
        0            2/(t-b)      0           0
        0            0            1/(zn-zf)   0
        (l+r)/(l-r)  (t+b)/(b-t)  zn/(zn-zf)  1*/
        Matrix mat;
        memset(&mat.M11, 0, sizeof(Matrix));
        mat.M11 = 2.0f /(maxx-minx);
        mat.M22 = 2.0f/(maxy-miny);
        mat.M33 = 1.0f/(zn-zf);
        mat.M41 = (minx+maxx)/(minx-maxx);
        mat.M42 = (maxy+miny)/(miny-maxy);
        mat.M43 = zn/(zn-zf);
        mat.M44 = 1.0f;
        return mat;
    }
    Matrix Matrix::CreateOrthographic(float width, float height, float zn, float zf)
    {
        // CreateOrthographic RH
        //  
        //  FLOAT w  // width
        //  FLOAT h  // height  

        // 2/w  0    0           0
        // 0    2/h  0           0
        // 0    0    1/(zn-zf)   0
        // 0    0    zn/(zn-zf)  1

        Matrix mat;
        memset(&mat.M11, 0, sizeof(Matrix));
        mat.M11 = 2.0f / width;
        mat.M22 = 2.0f / height;
        mat.M33 = 1.0f/(zn-zf);
        mat.M43 = zn/(zn-zf);
        mat.M44 = 1.0f;
        return mat;
    }

    Matrix Matrix::CreatePerspectiveFieldOfView(float fovy, float aspect, float zn, float zf)
    {
       //CreatePerspectiveFieldOfViewRH

       // yScale = cot(fovY/2)    
       // xScale = yScale / aspect ratio

       //   xScale     0          0              0
       //   0        yScale       0              0
       //   0        0        zf/(zn-zf)        -1
       //   0        0        zn*zf/(zn-zf)      0 

        float yScale = 1.0f / tanf(fovy * 0.5f);
        float xScale = yScale / aspect;        

        Matrix mat;
        memset(&mat.M11, 0, sizeof(Matrix));        
        mat.M11 = xScale;
        mat.M22 = yScale;
        mat.M33 = zf/(zn-zf);
        mat.M34 = -1.0f;
        mat.M43 = zn*zf/(zn-zf);
        return mat;
    }

    Matrix Matrix::CreatePerspective(float width, float height, float zn, float zf)
    {

        // CreatePerspectiveRH
        // 2*zn/w  0       0              0
        // 0       2*zn/h  0              0
        // 0       0       zf/(zn-zf)    -1
        // 0       0       zn*zf/(zn-zf)  0*/

        Matrix mat;
        memset(&mat.M11, 0, sizeof(Matrix));
        mat.M11 = (2.0f * zn) / width;
        mat.M22 = (2.0f * zn) / height;
        mat.M33 = zf/(zn-zf);
        mat.M34 = -1.0f;
        mat.M43 = (zn*zf)/(zn-zf);        
        return mat;
    }

    void Matrix::MakeIdentity()
    {   
       memset(&M11, 0, sizeof(Matrix));
       M11 = M22 = M33 = M44 = 1.0f;
    }

    Matrix Matrix::CreateTranslation(float x, float y, float z)
    {
        Matrix mat;
        memset(&mat.M11, 0, sizeof(Matrix));
        mat.M11 = mat.M22 = mat.M33 = mat.M44 = 1.0f;
        mat.M41 = x;
        mat.M42 = y;
        mat.M43 = z;
        return mat;
    }

    Matrix Matrix::CreateTranslation(const float3 &v)
    {
        Matrix mat;
        memset(&mat.M11, 0, sizeof(Matrix));
        mat.M11 = mat.M22 = mat.M33 = mat.M44 = 1.0f;
        mat.M41 = v.x;
        mat.M42 = v.y;
        mat.M43 = v.z;
        return mat;
    }

    void Matrix::CreateTranslation(float x, float y, float z, Matrix &mat)
    {
        memset(&mat.M11, 0, sizeof(Matrix));
        mat.M11 = mat.M22 = mat.M33 = mat.M44 = 1.0f;
        mat.M41 = x;
        mat.M42 = y;
        mat.M43 = z;

    }

    void Matrix::CreateTranslation(const float3 &v, Matrix &mat)
    {

        memset(&mat.M11, 0, sizeof(Matrix));
        mat.M11 = mat.M22 = mat.M33 = mat.M44 = 1.0f;
        mat.M41 = v.x;
        mat.M42 = v.y;
        mat.M43 = v.z;
    }
    
    Matrix Matrix::CreateScale(float3 s)
    {
        Matrix mat;
        memset(&mat.M11, 0, sizeof(Matrix));
        mat.M11 = s.x;
        mat.M22 = s.y;
        mat.M33 = s.z;
        mat.M44 = 1.0f;
        return mat;
    }

    Matrix Matrix::CreateScale(float x, float y, float z)
    {
        Matrix mat;
        memset(&mat.M11, 0, sizeof(Matrix));
        mat.M11 = x;
        mat.M22 = y;
        mat.M33 = z;
        mat.M44 = 1.0f;
        return mat;
    }

    Matrix Matrix::CreateScale(float s)
    {
        Matrix mat;
        memset(&mat.M11, 0, sizeof(Matrix));
        mat.M11 = s;
        mat.M22 = s;
        mat.M33 = s;
        mat.M44 = 1.0f;
        return mat;
    }

    Matrix Matrix::CreateBillboard(const float3& objectPos, const float3& camPos, const float3& camUp, const float3& camLook)
    {
         Matrix matrix;
         float3 look = camPos - objectPos;            
         float len = lengthsquared(look);

         if (len < 0.0001f)
         {
             look = -camLook;
         }
         else
         {
             look = normalize(look);
         }

         float3 right = normalize( cross(camUp, look) );
         float3 up = cross(look,right);

         matrix.M11 = right.x;
         matrix.M12 = right.y;
         matrix.M13 = right.z;
         matrix.M14 = 0.0f;
         matrix.M21 = up.x;
         matrix.M22 = up.y;
         matrix.M23 = up.z;
         matrix.M24 = 0.0f;
         matrix.M31 = look.x;
         matrix.M32 = look.y;
         matrix.M33 = look.z;
         matrix.M34 = 0.0f;
         matrix.M41 = objectPos.x;
         matrix.M42 = objectPos.y;
         matrix.M43 = objectPos.z;
         matrix.M44 = 1.0f;
         return matrix;
    }


    void Matrix::Invert()
    {
        float num5 = M11;
        float num4 = M12;
        float num3 = M13;
        float num2 = M14;
        float num9 = M21;
        float num8 = M22;
        float num7 = M23;
        float num6 = M24;
        float num17 = M31;
        float num16 = M32;
        float num15 = M33;
        float num14 = M34;
        float num13 = M41;
        float num12 = M42;
        float num11 = M43;
        float num10 = M44;

        float num23 = (num15 * num10) - (num14 * num11);
        float num22 = (num16 * num10) - (num14 * num12);
        float num21 = (num16 * num11) - (num15 * num12);
        float num20 = (num17 * num10) - (num14 * num13);
        float num19 = (num17 * num11) - (num15 * num13);
        float num18 = (num17 * num12) - (num16 * num13);
        float num39 = ((num8 * num23) - (num7 * num22)) + (num6 * num21);
        float num38 = -(((num9 * num23) - (num7 * num20)) + (num6 * num19));
        float num37 = ((num9 * num22) - (num8 * num20)) + (num6 * num18);
        float num36 = -(((num9 * num21) - (num8 * num19)) + (num7 * num18));
        float num = 1.0f / ((((num5 * num39) + (num4 * num38)) + (num3 * num37)) + (num2 * num36));
        M11 = num39 * num;
        M21 = num38 * num;
        M31 = num37 * num;
        M41 = num36 * num;
        M12 = -(((num4 * num23) - (num3 * num22)) + (num2 * num21)) * num;
        M22 = (((num5 * num23) - (num3 * num20)) + (num2 * num19)) * num;
        M32 = -(((num5 * num22) - (num4 * num20)) + (num2 * num18)) * num;
        M42 = (((num5 * num21) - (num4 * num19)) + (num3 * num18)) * num;
        float num35 = (num7 * num10) - (num6 * num11);
        float num34 = (num8 * num10) - (num6 * num12);
        float num33 = (num8 * num11) - (num7 * num12);
        float num32 = (num9 * num10) - (num6 * num13);
        float num31 = (num9 * num11) - (num7 * num13);
        float num30 = (num9 * num12) - (num8 * num13);
        M13 = (((num4 * num35) - (num3 * num34)) + (num2 * num33)) * num;
        M23 = -(((num5 * num35) - (num3 * num32)) + (num2 * num31)) * num;
        M33 = (((num5 * num34) - (num4 * num32)) + (num2 * num30)) * num;
        M43 = -(((num5 * num33) - (num4 * num31)) + (num3 * num30)) * num;
        float num29 = (num7 * num14) - (num6 * num15);
        float num28 = (num8 * num14) - (num6 * num16);
        float num27 = (num8 * num15) - (num7 * num16);
        float num26 = (num9 * num14) - (num6 * num17);
        float num25 = (num9 * num15) - (num7 * num17);
        float num24 = (num9 * num16) - (num8 * num17);
        M14 = -(((num4 * num29) - (num3 * num28)) + (num2 * num27)) * num;
        M24 = (((num5 * num29) - (num3 * num26)) + (num2 * num25)) * num;
        M34 = -(((num5 * num28) - (num4 * num26)) + (num2 * num24)) * num;
        M44 = (((num5 * num27) - (num4 * num25)) + (num3 * num24)) * num;

    }

    void Matrix::Invert(const Matrix &matrix, Matrix &result)
    {
        float num5 = matrix.M11;
        float num4 = matrix.M12;
        float num3 = matrix.M13;
        float num2 = matrix.M14;
        float num9 = matrix.M21;
        float num8 = matrix.M22;
        float num7 = matrix.M23;
        float num6 = matrix.M24;
        float num17 = matrix.M31;
        float num16 = matrix.M32;
        float num15 = matrix.M33;
        float num14 = matrix.M34;
        float num13 = matrix.M41;
        float num12 = matrix.M42;
        float num11 = matrix.M43;
        float num10 = matrix.M44;

        float num23 = (num15 * num10) - (num14 * num11);
        float num22 = (num16 * num10) - (num14 * num12);
        float num21 = (num16 * num11) - (num15 * num12);
        float num20 = (num17 * num10) - (num14 * num13);
        float num19 = (num17 * num11) - (num15 * num13);
        float num18 = (num17 * num12) - (num16 * num13);
        float num39 = ((num8 * num23) - (num7 * num22)) + (num6 * num21);
        float num38 = -(((num9 * num23) - (num7 * num20)) + (num6 * num19));
        float num37 = ((num9 * num22) - (num8 * num20)) + (num6 * num18);
        float num36 = -(((num9 * num21) - (num8 * num19)) + (num7 * num18));
        float num = 1.0f / ((((num5 * num39) + (num4 * num38)) + (num3 * num37)) + (num2 * num36));
        result.M11 = num39 * num;
        result.M21 = num38 * num;
        result.M31 = num37 * num;
        result.M41 = num36 * num;
        result.M12 = -(((num4 * num23) - (num3 * num22)) + (num2 * num21)) * num;
        result.M22 = (((num5 * num23) - (num3 * num20)) + (num2 * num19)) * num;
        result.M32 = -(((num5 * num22) - (num4 * num20)) + (num2 * num18)) * num;
        result.M42 = (((num5 * num21) - (num4 * num19)) + (num3 * num18)) * num;
        float num35 = (num7 * num10) - (num6 * num11);
        float num34 = (num8 * num10) - (num6 * num12);
        float num33 = (num8 * num11) - (num7 * num12);
        float num32 = (num9 * num10) - (num6 * num13);
        float num31 = (num9 * num11) - (num7 * num13);
        float num30 = (num9 * num12) - (num8 * num13);
        result.M13 = (((num4 * num35) - (num3 * num34)) + (num2 * num33)) * num;
        result.M23 = -(((num5 * num35) - (num3 * num32)) + (num2 * num31)) * num;
        result.M33 = (((num5 * num34) - (num4 * num32)) + (num2 * num30)) * num;
        result.M43 = -(((num5 * num33) - (num4 * num31)) + (num3 * num30)) * num;
        float num29 = (num7 * num14) - (num6 * num15);
        float num28 = (num8 * num14) - (num6 * num16);
        float num27 = (num8 * num15) - (num7 * num16);
        float num26 = (num9 * num14) - (num6 * num17);
        float num25 = (num9 * num15) - (num7 * num17);
        float num24 = (num9 * num16) - (num8 * num17);
        result.M14 = -(((num4 * num29) - (num3 * num28)) + (num2 * num27)) * num;
        result.M24 = (((num5 * num29) - (num3 * num26)) + (num2 * num25)) * num;
        result.M34 = -(((num5 * num28) - (num4 * num26)) + (num2 * num24)) * num;
        result.M44 = (((num5 * num27) - (num4 * num25)) + (num3 * num24)) * num;
    }


    void Matrix::Transpose()
    {
        float num16 = M11;
        float num15 = M12;
        float num14 = M13;
        float num13 = M14;
        float num12 = M21;
        float num11 = M22;
        float num10 = M23;
        float num9 = M24;
        float num8 = M31;
        float num7 = M32;
        float num6 = M33;
        float num5 = M34;
        float num4 = M41;
        float num3 = M42;
        float num2 = M43;
        float num = M44;
        M11 = num16;
        M12 = num12;
        M13 = num8;
        M14 = num4;
        M21 = num15;
        M22 = num11;
        M23 = num7;
        M24 = num3;
        M31 = num14;
        M32 = num10;
        M33 = num6;
        M34 = num2;
        M41 = num13;
        M42 = num9;
        M43 = num5;
        M44 = num;
    }
    void Matrix::Transpose(const Matrix &matrix, Matrix &result)
    {
        float num16 = matrix.M11;
        float num15 = matrix.M12;
        float num14 = matrix.M13;
        float num13 = matrix.M14;
        float num12 = matrix.M21;
        float num11 = matrix.M22;
        float num10 = matrix.M23;
        float num9 = matrix.M24;
        float num8 = matrix.M31;
        float num7 = matrix.M32;
        float num6 = matrix.M33;
        float num5 = matrix.M34;
        float num4 = matrix.M41;
        float num3 = matrix.M42;
        float num2 = matrix.M43;
        float num = matrix.M44;
        result.M11 = num16;
        result.M12 = num12;
        result.M13 = num8;
        result.M14 = num4;
        result.M21 = num15;
        result.M22 = num11;
        result.M23 = num7;
        result.M24 = num3;
        result.M31 = num14;
        result.M32 = num10;
        result.M33 = num6;
        result.M34 = num2;
        result.M41 = num13;
        result.M42 = num9;
        result.M43 = num5;
        result.M44 = num;
    }

	//================= Transform imple ==============================
	const Matrix& Transform::GetMatrix()
	{
		if (m_needUpdate)
		{
			m_needUpdate = false;
			m_matrix
				= Matrix::CreateScale(m_scale) *
				Matrix::CreateRotationX(m_rotate.x) *
				Matrix::CreateRotationY(m_rotate.y) *
				Matrix::CreateRotationZ(m_rotate.z) *
				Matrix::CreateTranslation(m_translate);
		}
		return m_matrix;
	}
	//void Transform::SetMatrix(const Matrix& mtrx)
	//{
	//	// decompose mtrx to scale, translation and rotation.
	//	m_scale.x = length(Vector3(&mtrx.M11));
	//	m_scale.y = length(Vector3(&mtrx.M21));
	//	m_scale.z = length(Vector3(&mtrx.M31));
	//	m_translate.x = mtrx.M41;
	//	m_translate.y = mtrx.M42;
	//	m_translate.z = mtrx.M43;
	//}
}

    
