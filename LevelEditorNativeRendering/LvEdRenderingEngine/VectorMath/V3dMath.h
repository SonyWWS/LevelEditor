//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once

#include <cassert>
#include <cstdio>
#include <cstring>
#include <cmath>
#include <cstdlib>
#include <stdint.h>
#include <vector>
#include <math.h>

namespace LvEdEngine
{

    // constants
    extern const float E;
    extern const float Log10E;
    extern const float Log2E;
    extern const float Pi;
    extern const float OneDivPi;
    extern const float PiOver2;
    extern const float PiOver4;
    extern const float TwoPi;
    extern const float PiOver180;
    extern const float InvPiOver180;
    extern const float Infinity;
    extern const float Epsilon;
    
    template <typename T>
    T Lerp(T a, T b, float t)
    {
        return (a + t*(b - a));
    }

    
    class Matrix;

    // --------------------------------------------------------------------------------
    class float2
    {
    public:
        float x, y;
        float2(){x=y=0.0f;}
        float2(float vx, float vy){x=vx; y=vy;}
        float2(const float2& v){x=v.x; y=v.y;}
        float2(const float *ar){x=ar[0]; y=ar[1];}

        // assignment
        float2 &operator =(const float2 &v)
        {         
            x = v.x; y = v.y;
            return *this;
        }

        // binary operators
        float2 operator +(const float2 &v) const 
        {
            return float2(x+v.x, y+v.y);
        }


        float2 operator -(const float2 &v) const 
        {
            return float2(x - v.x, y - v.y);
        }

        float2 operator -(float v) const 
        {
            return float2(x - v, y - v);
        }

        float2 operator *(const float2 &v) const 
        {
            return float2(x*v.x, y*v.y);
        }

        float2 operator /(const float2 &v) const 
        {
            return float2(x/v.x, y/v.y);
        }

        operator float* () { return (float *) &x; }
        operator const float* () const { return (const float *) &x; }
    };
    typedef float2 Vector2;

    // --------------------------------------------------------------------------------
    class float3
    {
    public:
        float x;
        float y;
        float z;

        // ctors
        float3(){x=y=z=0.0f;}
        float3(float vx, float vy, float vz){x=vx; y=vy; z=vz;}
        float3(const float3 &v){x=v.x;y=v.y;z=v.z;}
        float3(const float *ar){x=ar[0]; y=ar[1]; z=ar[2];}

        // casting
        operator float* ();
        operator const float* () const;

        // operators
        float& operator[] (int index)
        {
            assert(index < 3);
            float* fptr = (float*)&x;
            return fptr[index];            
        }

        float3 &operator =(const float3 &v)
        {         
           x = v.x; y = v.y; z = v.z;
           return *this;
        }

        float3 &operator +=(const float3 &a) 
        {
           x += a.x; y += a.y; z += a.z;
           return *this;
        }

        float3 &operator -=(const float3 &a) 
        {
           x -= a.x; y -= a.y; z -= a.z;
           return *this;
        }

        // multiply vector by a float
        float3 &operator *=(float a) 
        {
            x *= a; y *= a; z *= a;
            return *this;
        }

        float3 &operator /=(float a) 
        {
           float	oneOverA = 1.0f / a;
           x *= oneOverA;
           y *= oneOverA; 
           z *= oneOverA;
           return *this;
        }

        // Check for equality
        bool operator ==(const float3 &v) const 
        {
           return x==v.x && y==v.y && z==v.z;
        }

        // Check for equality
        bool operator !=(const float3 &v) const 
        {
           return x!=v.x || y!=v.y || z!=v.z;
        }

        // Unary minus returns the negative of the vector
        float3 operator -() const { return float3(-x,-y,-z); }


        // binary operators
        float3 operator +(const float3 &v) const 
        {
           return float3(x+v.x, y+v.y, z+v.z);
        }

        float3 operator -(const float3 &v) const 
        {
           return float3(x - v.x, y - v.y, z - v.z);
        }

        float3 operator *(const float3 &v) const 
        {
            return float3(x*v.x, y*v.y, z*v.z);
        }

        float3 operator *(float f) const 
        {
            return float3(x*f, y*f, z*f);
        }

        float3 operator *(const Matrix &m) const ;
        static float3 Transform(const float3 &v, const Matrix &m);
        static float3 TransformNormal(const float3 &v,const Matrix &m);
   
        void Transform(const Matrix &m);
        void TransformNormal(const Matrix &m);
        friend float3 operator * (float f, const float3 &v);


        float3 operator /(float f) const 
        {
            float	oneOverf = 1.0f / f; // NOTE: no check for divide by zero here
            return float3(x*oneOverf, y*oneOverf, z*oneOverf);
        }

        float3 operator /(const float3 &v) const 
        {
            return float3(x/v.x, y/v.y, z/v.z);
        }
        
    };
    typedef float3 Vector3;

    // --------------------------------------------------------------------------------
    class float4
    {    
    public:

        float x, y, z, w;
        float4()
        {
            x = 0.0f;
            y = 0.0f;
            z = 0.0f;
            w = 0.0f;
        }
                   
        float4(float vx, float vy, float vz, float vw)
        {
            x = vx;
            y = vy;
            z = vz;
            w = vw;

        }    
        float4(const float3 &v,float vw)
        {        
            x = v.x;
            y = v.y;
            z = v.z;
            w = vw;
        
        }
        float4(const float3 &v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
            w = 0.0f;
        }

         float4(const float *ar)
         {
             x = ar[0];
             y = ar[1];
             z = ar[2];
             w = ar[3];
         }
         float4(float v)
         {
            x = v;
            y = v;
            z = v;
            w = v;
         }

         float3 xyz() const
         {
             return float3(x,y,z);
         }

        // binary operators
         float4 operator +(const float4 &v) const 
         {
             return float4(x+v.x, y+v.y, z+v.z, w+v.w);
         }


         float4 operator -(const float4 &v) const 
         {
             return float4(x - v.x, y - v.y, z - v.z, w-v.w);
         }

         float4 operator *(const float4 &v) const 
         {
             return float4(x*v.x, y*v.y, z*v.z, w*v.w);
         }

         float4 operator *(float f) const 
         {
             return float4(x*f, y*f, z*f, w*f);
         }

         float4 operator /(float f) const 
         {
             float	oneOverf = 1.0f / f; // NOTE: no check for divide by zero here
             return float4(x*oneOverf, y*oneOverf, z*oneOverf,w*oneOverf);
         }

         float4 operator /(const float4 &v) const 
         {
             return float4(x/v.x, y/v.y, z/v.z, w/v.w);
         }

         static float4 Transform(const float4 &v, const Matrix &m);         
    };
    typedef float4 Vector4;



    class BezierCurve
    {
    public:
        BezierCurve(float3 v0, float3 v1, float3 v2, float3 v3)
            : p0(v0),p1(v1),p2(v2),p3(v3)
        {
            ComputeCoefficients();
        }

        BezierCurve(float3* v) : p0(v[0]),p1(v[1]),p2(v[2]),p3(v[3])
        {
            ComputeCoefficients();
        }

        float3 Eval(float t) const
        {
            float tsq = t * t;
            float tcb = tsq * t;
            return (tcb * c0 + tsq * c1 + t * c2 + p0);
        }

        float3 GetControlPoint(int i) const           
        {
            float3* pts = (float3*)&p0;
            return pts[i];
        }
    private:
        void ComputeCoefficients()
        {
            c0 = 3.0f * (p1 - p2) + p3 - p0;
            c1 = 3.0f * (p0-2*p1+p2);
            c2 = 3 * (p1-p0);
        }
        float3 p0,p1,p2,p3;  // control points
        float3 c0,c1,c2; // co-efficeints

    };

    class BezierSpline
    {
    public:
        BezierSpline(float3* points, int count, bool isClosed);
        
        int CurveCount() const
        { 
            return (int) m_curves.size();
        }

        const BezierCurve& GetCurveAt(int i) const
        {
            return m_curves[i];
        }
    private:                
        std::vector<BezierCurve> m_curves;

    };
    

    float3 Vec3CatmullRom(const float3 &p0
        ,const float3 &p1
        ,const float3 &p2
        ,const float3 &p3,
        float t);
    class Matrix
    {
    public:
        float   M11, M12, M13, M14;
        float   M21, M22, M23, M24;
        float   M31, M32, M33, M34;
        float   M41, M42, M43, M44;
        
        Matrix() {this->MakeIdentity();};
        Matrix( const float *pm );
        Matrix( const Matrix& );    
        Matrix( float f11, float f12, float f13, float f14,
                float f21, float f22, float f23, float f24,
                float f31, float f32, float f33, float f34,
                float f41, float f42, float f43, float f44 );


        // access grants
        float& operator () ( unsigned int Row, unsigned int Col );
        float  operator () ( unsigned int Row, unsigned int Col ) const;
    

        // casting operators
        operator float* ();
        operator const float* () const;

        // unary operators    
        Matrix operator - () const;

        // binary operators
        Matrix operator * ( const Matrix& mat) const;
        Matrix operator + ( const Matrix& mat) const;
        Matrix operator - ( const Matrix& mat) const;
        Matrix operator * ( float ) const;
        Matrix operator / ( float ) const;
        friend Matrix operator * ( float, const Matrix& );

        // comparsion 
        bool operator == ( const Matrix& ) const;
        bool operator != ( const Matrix& ) const;

        void MakeIdentity();
    
        static Matrix CreateFromAxisAngle(const float3& axis, float angle);
        static Matrix CreateOrthographicOffCenter(float minx, float maxx, float miny, float maxy, float zn, float zf);
        static Matrix CreateOrthographic(float width, float height, float nearZ, float farZ);
        static Matrix CreatePerspective(float width, float height, float nearZ, float farZ);
        static Matrix CreatePerspectiveFieldOfView(float fovy, float aspect, float nearZ, float farZ);
        static Matrix CreateLookAtRH(const float3 &eye,const float3& at, const float3& up);
        static Matrix CreateRotationX(float angle);
        static Matrix CreateRotationY(float angle);
        static Matrix CreateRotationZ(float angle);                
        static Matrix CreateTranslation();
        static Matrix CreateTranslation(float x, float y, float z);
        static Matrix CreateTranslation(const float3 &v);
        static void CreateTranslation(float x, float y, float z, Matrix &mat);
        static void CreateTranslation(const float3& v, Matrix &mat);
        static Matrix CreateBillboard(const float3& objectPos, const float3& camPos, const float3& camUp, const float3& camLook);

        void Invert();
        static void Invert(const Matrix &matrix, Matrix &result);
        void Transpose();
        static void Transpose(const Matrix &matrix, Matrix &result);
        static Matrix CreateScale(float3 s);
        static Matrix CreateScale(float x, float y, float z);
        static Matrix CreateScale(float s);

    };
    typedef Matrix float4x4;
    
    class Transform
    {
    public:
        const Vector3& Scale() const { return m_scale; }
        void SetScale(const Vector3& scale) { m_scale = scale; }

        const Vector3& Translation() { return m_translate; }
        void SetTranslation(const Vector3& trans) { m_translate = trans; }

        const Vector3& Rotation() { return m_rotate; }
        void SetRotation(const Vector3& rotation) { m_rotate = rotation; }
		const Matrix& GetMatrix();

        Transform() : m_translate(0,0,0),
            m_rotate(0,0,0),
            m_scale(0,0,0),
            m_needUpdate(false)	{m_matrix.MakeIdentity();}

        Transform(const Vector3& scale, const Vector3& rotate, const Vector3& translate)
        {
			m_scale = scale;
			m_rotate = rotate;
			m_translate = translate;            
            m_needUpdate = true;
         }
        private:
            bool m_needUpdate;
            Vector3 m_translate;
            Vector3 m_rotate;
            Vector3 m_scale;
            Matrix m_matrix;
    };
    //******************************* inline  float3 *************************************
    inline
    float3::operator float* ()
    {
        return (float *) &x;
    }

    inline
    float3::operator const float* () const
    {
        return (const float *) &x;
    }

    inline float3 float3::operator *(const Matrix &m) const 
    {

        float3 t;
        t.x = x * m.M11 +  y * m.M21 + z * m.M31 + m.M41;
        t.y = x * m.M12 +  y * m.M22 + z * m.M32 + m.M42;
        t.z = x * m.M13 +  y * m.M23 + z * m.M33 + m.M43;
        return t;    
    }


    inline float3 float3::Transform(const float3 &v, const Matrix &m)
     {
         float3 t;
         t.x = v.x * m.M11 +  v.y * m.M21 + v.z * m.M31 + m.M41;
         t.y = v.x * m.M12 +  v.y * m.M22 + v.z * m.M32 + m.M42;
         t.z = v.x * m.M13 +  v.y * m.M23 + v.z * m.M33 + m.M43;
         float w = v.x * m.M14 +  v.y * m.M24 + v.z * m.M34 + m.M44;
         t = t / w;
         return t;
     }

    inline float3 float3::TransformNormal(const float3 &v,const Matrix &m)
     {
         float3 t;
         t.x = v.x * m.M11 +  v.y * m.M21 + v.z * m.M31;
         t.y = v.x * m.M12 +  v.y * m.M22 + v.z * m.M32;
         t.z = v.x * m.M13 +  v.y * m.M23 + v.z * m.M33;
         return t;
     }

     inline void float3::Transform(const Matrix &m)
     {         
         float n1 = x * m.M11 +  y * m.M21 + z * m.M31 + m.M41;
         float n2 = x * m.M12 +  y * m.M22 + z * m.M32 + m.M42;
         float n3 = x * m.M13 +  y * m.M23 + z * m.M33 + m.M43;
         float w  = x * m.M14 +  y * m.M24 + z * m.M34 + m.M44;
         x = n1 / w;
         y = n2 / w;
         z = n3 / w;         
     }


     inline void float3::TransformNormal(const Matrix &m)
     {
         float n1 = x * m.M11 +  y * m.M21 + z * m.M31;
         float n2 = x * m.M12 +  y * m.M22 + z * m.M32;
         float n3 = x * m.M13 +  y * m.M23 + z * m.M33;
         x = n1;
         y = n2;
         z = n3;
     }

    inline float3 operator * (float f, const float3 &v)
    {
        return float3(f*v.x, f*v.y, f*v.z);
    }

    // ******************************* inline Matrix ************************************
    inline float4 float4::Transform(const float4 &v, const Matrix &m)
    {
        float4 t;
        t.x = v.x * m.M11 +  v.y * m.M21 + v.z * m.M31 + v.w * m.M41;
        t.y = v.x * m.M12 +  v.y * m.M22 + v.z * m.M32 + v.w * m.M42;
        t.z = v.x * m.M13 +  v.y * m.M23 + v.z * m.M33 + v.w * m.M43;
        t.w = v.x * m.M14 +  v.y * m.M24 + v.z * m.M34 + v.w * m.M44;             
        return t;
    }

    inline Matrix::Matrix( const float *pf )
    {
        assert(pf);
        memcpy(&M11, pf, sizeof(Matrix));
    }


    inline Matrix::Matrix( const Matrix& mat )
    {
        memcpy(&M11, &mat, sizeof(Matrix));
    }


    inline Matrix::Matrix(  float f11, float f12, float f13, float f14,
                            float f21, float f22, float f23, float f24,
                            float f31, float f32, float f33, float f34,
                            float f41, float f42, float f43, float f44 )

    {
        M11 = f11; M12 = f12; M13 = f13; M14 = f14;
        M21 = f21; M22 = f22; M23 = f23; M24 = f24;
        M31 = f31; M32 = f32; M33 = f33; M34 = f34;
        M41 = f41; M42 = f42; M43 = f43; M44 = f44;
    }

    // access grants
    inline float& Matrix::operator () ( unsigned int Row, unsigned int Col )
    {
        float* fptr = (float*)&M11;      
        unsigned int index = Row * 4 + Col;
        return *(fptr+index);
         
    }
    inline float  Matrix::operator () ( unsigned int Row, unsigned int Col ) const
    {
        float* fptr = (float*)&M11;      
        unsigned int index = Row * 4 + Col;
        return *(fptr+index);
    }

    // casting operators
    inline Matrix::operator float* ()
    {
        return (float *) &M11;
    }
    inline Matrix::operator const float* () const
    {
        return (const float *) &M11;
    }



    // unary operators
    inline Matrix Matrix::operator-() const
    {
        return Matrix(    -M11, -M12, -M13, -M14,
                          -M21, -M22, -M23, -M24,
                          -M31, -M32, -M33, -M34,
                          -M41, -M42, -M43, -M44);    
    
    }


    inline Matrix Matrix::operator + ( const Matrix& mat) const
    {

        return Matrix(    M11 + mat.M11, M12 + mat.M12, M13 + mat.M13, M14 + mat.M14,
                          M21 + mat.M21, M22 + mat.M22, M23 + mat.M23, M24 + mat.M24,
                          M31 + mat.M31, M32 + mat.M32, M33 + mat.M33, M34 + mat.M34,
                          M41 + mat.M41, M42 + mat.M42, M43 + mat.M43, M44 + mat.M44);    
    }
    inline Matrix Matrix::operator - ( const Matrix& mat) const
    {
        return Matrix(    M11 - mat.M11, M12 - mat.M12, M13 - mat.M13, M14 - mat.M14,
                          M21 - mat.M21, M22 - mat.M22, M23 - mat.M23, M24 - mat.M24,
                          M31 - mat.M31, M32 - mat.M32, M33 - mat.M33, M34 - mat.M34,
                          M41 - mat.M41, M42 - mat.M42, M43 - mat.M43, M44 - mat.M44);
    }
    inline Matrix Matrix::operator * ( float f) const
    {
        return Matrix(    M11 * f, M12 * f, M13 * f, M14 * f,
                          M21 * f, M22 * f, M23 * f, M24 * f,
                          M31 * f, M32 * f, M33 * f, M34 * f,
                          M41 * f, M42 * f, M43 * f, M44 * f);
    }

    inline Matrix Matrix::operator / ( float f) const
    {
         float fInv = 1.0f / f;
         return Matrix(   M11 * fInv, M12 * fInv, M13 * fInv, M14 * fInv,
                          M21 * fInv, M22 * fInv, M23 * fInv, M24 * fInv,
                          M31 * fInv, M32 * fInv, M33 * fInv, M34 * fInv,
                          M41 * fInv, M42 * fInv, M43 * fInv, M44 * fInv);
    }

    inline Matrix operator * ( float f, const Matrix& mat)
    {
        return Matrix(    mat.M11 * f, mat.M12 * f, mat.M13 * f, mat.M14 * f,
                          mat.M21 * f, mat.M22 * f, mat.M23 * f, mat.M24 * f,
                          mat.M31 * f, mat.M32 * f, mat.M33 * f, mat.M34 * f,
                          mat.M41 * f, mat.M42 * f, mat.M43 * f, mat.M44 * f);
    }

    // comparsion 
    inline bool Matrix::operator == ( const Matrix& mat) const
    {
        return 0 == memcmp(this, &mat, sizeof(Matrix));
    }
    inline bool Matrix::operator != ( const Matrix& mat) const
    {
        return 0 != memcmp(this, &mat, sizeof(Matrix));
    }



    //********************** util functions *******************************\\
    //*********************************************************************\\

    inline bool IsPowerOf2(uint32_t a)
    {
        return !((a-1) & a);
    }


    inline float GetRandomFloat( float fMax )
    {
        float fRandNum = (float)rand() / (float) RAND_MAX;
        return  fMax * fRandNum;
    }

    inline float GetRandomFloat( float fMin, float fMax )
    {
        float fRandNum = (float)rand() / (float) RAND_MAX;
        return fMin + fRandNum * (fMax - fMin);
    }

    inline float AbsFloat(float val)
    {
        unsigned int i = *(reinterpret_cast<unsigned int*>(&val));
        i &= 0x7fffffff;
        return *(reinterpret_cast<float*>(&i));
    }

    template <class T> 
    inline T clamp(T value, T low, T high)
    {
        if(value < low)
        {
            return low;
        }
    
        if(value > high)
        {
            return high;
        } 
        
        return value;
    }

    // get random unit vector evenly distributed on a unit sphere.
    inline void GetRandomVector(float3 &out)
    {
        out.x = GetRandomFloat(-1.0f, 1.0f);
        float Ryz = sqrt(1- out.x * out.x);
        float Thetayz = GetRandomFloat(-Pi, Pi);
        out.y = Ryz * cosf(Thetayz);
        out.z = Ryz * sinf(Thetayz);
    }

    inline int FtoDW( float f ) { return *((int*)&f); }

    inline float ToRadian( float degree ) 
    {
        return (degree * PiOver180);
    }        
    inline float ToDegree( float radian ) 
    {
        return ( radian * InvPiOver180);
    }        


// -----------------------------------------------------------------------------
// OPERATORS
inline float4 operator*(const float4& v, const float4x4& m)
{
    float fx = (m.M11 * v.x) + (m.M21 * v.y) + (m.M31 * v.z) + (m.M41 * v.w);
    float fy = (m.M12 * v.x) + (m.M22 * v.y) + (m.M32 * v.z) + (m.M42  * v.w);
    float fz = (m.M13 * v.x) + (m.M23 * v.y) + (m.M33 * v.z) + (m.M43  * v.w);
    float fw = (m.M14 * v.x) + (m.M24 * v.y) + (m.M34 * v.z) + (m.M44  * v.w);
    return float4(fx,fy,fz,fw);
}    
// -----------------------------------------------------------------------------
// functions that mirror the HLSL instrinsics. 
// this allows the C++ code to look like the HLSL code.
// -----------------------------------------------------------------------------

inline float2 absolute(const float2& a)
{
    return float2(fabs(a.x), fabs(a.y));
}
inline float3 absolute(const float3& a)
{
    return float3(fabs(a.x), fabs(a.y), fabs(a.z));
}
inline float4 absolute(const float4& a)
{
    return float4(fabs(a.x), fabs(a.y), fabs(a.z), fabs(a.w));
}


inline float3 cross(const float3& a, const float3& b)
{
    float3 t;
    t.x = a.y * b.z - a.z * b.y;
    t.y = a.z * b.x - a.x * b.z;
    t.z = a.x * b.y - a.y * b.x;
    return t;  
}

inline float dot(const float2& a, const float2& b)
{
    return (a.x * b.x  +  a.y * b.y);
}

inline float dot(const float3& a, const float3& b)
{
    return (a.x * b.x  +  a.y * b.y  +  a.z * b.z);
}
inline float dot(const float4& a, const float4& b)
{
    return (a.x * b.x  +  a.y * b.y  +  a.z * b.z + a.w * b.w);
}


inline float length(const float2& v)
{
    return sqrt(v.x*v.x + v.y*v.y);
}
inline float length(const float3& v)
{
    return sqrt(v.x*v.x + v.y*v.y + v.z*v.z);
}
inline float length(const float4& v)
{
    return sqrt(v.x*v.x + v.y*v.y + v.z*v.z + v.w * v.w);
}

inline float lengthsquared(const float2& v)
{
    return v.x*v.x + v.y*v.y;
}
inline float lengthsquared(const float3& v)
{
    return v.x*v.x + v.y*v.y + v.z*v.z;
}
inline float lengthsquared(const float4& v)
{
    return v.x*v.x + v.y*v.y + v.z*v.z + v.w * v.w;
}

inline float2 lerp(const float2& a, const float2& b, const float2& c)
{
    float2 t;
    t.x = a.x + (b.x - a.x) * c.x;
    t.y = a.y + (b.y - a.y) * c.y;
    return t;
}
inline float3 lerp(const float3& a, const float3& b, const float3& c)
{
    float3 t;
    t.x = a.x + (b.x - a.x) * c.x;
    t.y = a.y + (b.y - a.y) * c.y;
    t.z = a.z + (b.z - a.z) * c.z;
    return t;
}
inline float4 lerp(const float4& a, const float4& b, const float4& c)
{
    float4 t;
    t.x = a.x + (b.x - a.x) * c.x;
    t.y = a.y + (b.y - a.y) * c.y;
    t.z = a.z + (b.z - a.z) * c.z;
    t.w = a.w + (b.w - a.w) * c.w;
    return t;
}

// 'max' is a #define in WinDef.h, so we use 'maximize' instead.
inline float maximize(const float a, const float b)
{
    return a > b ? a : b;
}
inline float2 maximize(const float2& a, const float2& b)
{
    float2 r;
    r.x = a.x > b.x ? a.x : b.x;
    r.y = a.y > b.y ? a.y : b.y;
    return r;
}
inline float3 maximize(const float3& a, const float3& b)
{
    float3 r;
    r.x = a.x > b.x ? a.x : b.x;
    r.y = a.y > b.y ? a.y : b.y;
    r.z = a.z > b.z ? a.z : b.z;
    return r;
}
inline float4 maximize(const float4& a, const float4& b)
{
    float4 r;
    r.x = a.x > b.x ? a.x : b.x;
    r.y = a.y > b.y ? a.y : b.y;
    r.z = a.z > b.z ? a.z : b.z;
    r.w = a.w > b.w ? a.w : b.w;
    return r;
}

// 'min' is a #define in WinDef.h, so we use 'minimize' instead.
inline float minimize(const float a, const float b)
{
    return a < b ? a : b;
}

inline float2 minimize(const float2& a, const float2& b)
{
    float2 r;
    r.x = a.x < b.x ? a.x : b.x;
    r.y = a.y < b.y ? a.y : b.y;
    return r;
}
inline float3 minimize(const float3& a, const float3& b)
{
    float3 r;
    r.x = a.x < b.x ? a.x : b.x;
    r.y = a.y < b.y ? a.y : b.y;
    r.z = a.z < b.z ? a.z : b.z;
    return r;
}
inline float4 minimize(const float4& a, const float4& b)
{
    float4 r;
    r.x = a.x < b.x ? a.x : b.x;
    r.y = a.y < b.y ? a.y : b.y;
    r.z = a.z < b.z ? a.z : b.z;
    r.w = a.w < b.w ? a.w : b.w;
    return r;
}


inline float2 normalize(const float2& v)
{
    float2 temp(0,0);
    float l = length(v);
    if(l != 0)
    {
        float invLength = 1.0f / l;
        temp.x = v.x * invLength;
        temp.y = v.y * invLength;
    }
    return temp;
}

inline float3 normalize(const float3& v)
{
    float3 temp(0,0,0);
    float l = length(v);
    if(l != 0)
    {
        float invLength = 1.0f / l;
        temp.x = v.x * invLength;
        temp.y = v.y * invLength;
        temp.z = v.z * invLength;
    }
    return temp;
}

inline float4 normalize(const float4& v)
{
    float4 temp(0,0,0,0);
    float l = length(v);
    if(l != 0)
    {
        float invLength = 1.0f / l;
        temp.x = v.x * invLength;
        temp.y = v.y * invLength;
        temp.z = v.z * invLength;
        temp.z = v.w * invLength;
    }
    return temp;
}
inline float2 rcp(const float2& a)
{
    float2 t;
    t.x = 1/a.x;
    t.y = 1/a.y;
    return t;
}
inline float3 rcp(const float3& a)
{
    float3 t;
    t.x = 1/a.x;
    t.y = 1/a.y;
    t.z = 1/a.z;
    return t;
}
inline float4 rcp(const float4& a)
{
    float4 t;
    t.x = 1/a.x;
    t.y = 1/a.y;
    t.z = 1/a.z;
    t.w = 1/a.w;
    return t;
}

inline float round(float v)
{
    return floor(v + 0.5f);
}


inline float2 saturate(const float2& a)
{
    return maximize(float2(0.f, 0.f), minimize(float2(1.f, 1.f), a));
}

inline float3 saturate(const float3& a)
{
    return maximize(float3(0.f, 0.f, 0.f), minimize(float3(1.f, 1.f, 1.f), a));
}

inline float4 saturate(const float4& a)
{
    return maximize(float4(0.f, 0.f, 0.f, 0.f), minimize(float4(1.f, 1.f, 1.f, 1.f), a));
}
// float4x4 saturate(float4x4);

inline float2 select( const float2& a, const float2& b, const float2& c)
{
    float2 t;
    t.x = (c.x == 0) ? a.x : b.x;
    t.y = (c.y == 0) ? a.y : b.y;
    return t;
}
inline float3 select( const float3& a, const float3& b, const float3& c)
{
    float3 t;
    t.x = (c.x == 0) ? a.x : b.x;
    t.y = (c.y == 0) ? a.y : b.y;
    t.z = (c.z == 0) ? a.z : b.z;
    return t;
}
inline float4 select( const float4& a, const float4& b, const float4& c)
{
    float4 t;
    t.x = (c.x == 0) ? a.x : b.x;
    t.y = (c.y == 0) ? a.y : b.y;
    t.z = (c.z == 0) ? a.z : b.z;
    t.w = (c.w == 0) ? a.w : b.w;
    return t;
}

inline void OrthoNormalize( float3* a, float3* b )
{
    *a = normalize(*a);
    float3 tmp;
    tmp = (*a) * dot(*a, *b);
    *b = (*b) - tmp;
    *b = normalize(*b);
}



// COLOR
void ConvertColor( int color, float4* out );
void ConvertColor( int color, float3* out );


}
