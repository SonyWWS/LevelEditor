//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

#pragma once
#include "Object.h"
#include <stdint.h>
#include <assert.h>
#include "../VectorMath/V3dmath.h"
#include "../VectorMath/CollisionPrimitives.h"


namespace DirectX
{
    struct Image;   
}
namespace LvEdEngine
{



class ImageData : public Object
{
public:
    
    const char* ClassName()  const  {return StaticClassName();}
    static const char* StaticClassName(){return "ImageData";}

    ImageData();    
    ~ImageData();

    void Invoke(wchar_t* fn, const void* arg, void** retVal);

    void CreateNew(int32_t width, int32_t height, uint32_t format);
    void LoadFromFile(wchar_t* file);
    void InitFrom(const DirectX::Image* image);
    void InitFrom(const ImageData* imgdata);
    void SaveToFile(wchar_t* file);
    // extend the size by x and y
    // and rows and cols will be duplicated.
    void Extend(int32_t dx, int32_t dy);

    // convert this imagedata to the give format.
    // if this image.format == format then do nothing.
    void Convert(uint32_t format);
    
    
    /*void ApplyImage(const ImageData* img, 
                    int32_t x, 
                    int32_t y,         
                    int32_t brushOp,
                    Bound2di& outRect);*/



    template<typename T>
    T BLinearSample(float x, float y) const
    {                                
        x = clamp<float>(x,0.0f,(float)(m_width-1));
        y = clamp<float>(y,0.0f,(float)(m_height-1));
        
        int32_t cols = (m_width - 1);
        int32_t rows = (m_height - 1);
        
        int32_t x1 =  (int32_t)(x);
        int32_t x2 =  x1 < cols ? (x1 + 1) : x1;

        int32_t y1 =  (int32_t)(y);
        int32_t y2 =  (y1 < rows) ?  (y1 + 1) : y1;

        T vA = *((T*)PixelAt(x1,y1));         
        T vB = *((T*)PixelAt(x2,y1));
        T vC = *((T*)PixelAt(x1,y2));
        T vD = *((T*)PixelAt(x2,y2));

        float s = x - x1;
        float t = y - y1;

        T vAB = Lerp(vA,vB,s);
        T vCD = Lerp(vC,vD,s);
        T val = Lerp(vAB,vCD,t);
        return val;
    }


    template<typename T>
    T BLinearSampleNorm(float u, float v) const
    {
        
        u = clamp<float>(u,0.0f,1.0f);
        v = clamp<float>(v,0.0f,1.0f);


        // use bilinear sampling
        //
        //   vA------------vB
        //   |  s,t        |
        //   |   *         |
        //   |             |
        //   vC-----------vD
        //

        int32_t cols = (m_width - 1);
        int32_t rows = (m_height - 1);
        float fx = u * cols;
        float fy = v * rows;

        return BLinearSample<T>(fx,fy);
      }   
    
    template<typename T>
    T  PointSample(float u, float v) const
    {
         assert(m_buffer != NULL 
            && u >= 0 && u <= 1.0f 
            && v >= 0 && v <= 1.0f);

         int32_t cols = (m_width - 1);
         int32_t rows = (m_height - 1);

         int32_t x =  round(u * cols);
         int32_t y =  round(v * rows);
         return  *((T*)PixelAt(x,y));
    }


    // get pixel value at x and y pos.
    uint8_t* GetPixelAt(int32_t x, int32_t y)  
    {

        assert(m_buffer != NULL 
            && x >= 0 && x < m_width 
            && y >=0 && y < m_height);

        return  (m_buffer + y * m_rowPitch + x * m_bytesPerPixel);
    }

    // get pixel value at x and y pos.
    inline const uint8_t* PixelAt(int32_t x, int32_t y) const 
    {
        assert(m_buffer != NULL 
            && x >= 0 && x < m_width 
            && y >=0 && y < m_height);

        return (m_buffer + y * m_rowPitch + x * m_bytesPerPixel);
    }

    void *GetBufferPointer() const { return m_buffer; }
    uint64_t GetBufferSize() const { return m_size; }
    int32_t GetWidth() const { return m_width;}
    int32_t GetHeight() const { return m_height;}
    int32_t GetRowPitch() const { return m_rowPitch;}
    int32_t GetBytesPerPixel() const { return m_bytesPerPixel;}
    uint32_t GetFormat() const {return m_format;}

private:
     uint8_t*   m_buffer;
     uint64_t   m_size;
     int32_t   m_bytesPerPixel;
     int32_t   m_width;
     int32_t   m_height;
     int32_t   m_rowPitch;
     uint32_t   m_format;

     // reset all the members to default value
     // free the pixel buffer.
     void Reset();   
     
};

}