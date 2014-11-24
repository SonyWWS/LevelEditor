//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Diagnostics;
using System.IO;
using LevelEditorCore.VectorMath;


namespace RenderingInterop
{

    /// <summary>
    /// Class mirros ImageData in C++</summary>
    public unsafe class ImageData : DisposableObject
    {
        /// <summary>
        /// Construct Image data</summary>        
        public ImageData() : this(0)
        {
            
        }

        public ImageData(ulong instanceId)
        {
            TypeId = GameEngine.GetObjectTypeId("ImageData");
            LoadFromFileId = GameEngine.GetObjectPropertyId(TypeId, "LoadFromFile");
            ConvertId = GameEngine.GetObjectPropertyId(TypeId, "Convert");
            BufferPointerId = GameEngine.GetObjectPropertyId(TypeId, "BufferPointer");
            WidthId = GameEngine.GetObjectPropertyId(TypeId, "Width");
            HeightId = GameEngine.GetObjectPropertyId(TypeId, "Height");
            BytesPerPixelId = GameEngine.GetObjectPropertyId(TypeId, "BytesPerPixel");
            RowPitchId = GameEngine.GetObjectPropertyId(TypeId, "RowPitch");
            FormatId = GameEngine.GetObjectPropertyId(TypeId, "Format");
            m_instanceId = instanceId;
            if (m_instanceId == 0)
            {
                m_manageLifetime = true;
                m_instanceId = GameEngine.CreateObject(TypeId, IntPtr.Zero, 0);
            }
            RefreshCachedProperties();
        }


        public byte GetPixelByte(int x, int y)
        {
            bool valid = x >= 0 && x < Width && y >= 0 && y < Height;               
            if (!valid) return (byte)0;
            byte* pixelPtr = (byte*)m_data + y * m_rowPitch + x * m_bytesPerPixel;
            return *pixelPtr;
        }

        /// <summary>
        /// Get pixel at (x,y) and apply clamp 
        /// for out of range coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public float GetPixelFloat(int x, int y)
        {
            if (m_format != ImageDataFORMAT.R32_FLOAT)
                throw new InvalidOperationException("Invalid Image format");
            x = Sce.Atf.MathUtil.Clamp(x, 0, Width - 1);
            y = Sce.Atf.MathUtil.Clamp(y, 0, Height - 1);                        
            byte* pixelPtr = (byte*)m_data + y * m_rowPitch + x * m_bytesPerPixel;
            return  *(float*)pixelPtr;
        }
       
        public byte* GetPixel(int x, int y)
        {
            bool valid = x >= 0 && x < Width && y >= 0 && y < Height;            
            if (!valid) return null;

            byte* pixelPtr = ((byte*)m_data + y * m_rowPitch + x * m_bytesPerPixel);
            return pixelPtr;
        }

        public void SetPixel(int x, int y, byte val)
        {
            byte* pixel = GetPixel(x, y);
            *pixel = val;
        }

        public void SetPixel(int x, int y, float val)         
        {            
            int w = Width;
            int h = Height;
            bool valid = x >= 0 && x < w && y >= 0 && y < h && Format == ImageDataFORMAT.R32_FLOAT;
            System.Diagnostics.Debug.Assert(valid);
            if (!valid) return;

            int row = RowPitch;
            int bpp = BytesPerPixel;
            float* ptr = (float*)((byte*)Data + y * row + x * bpp);
            *ptr = val;            
        }
        public void InitNew(int width, int height, ImageDataFORMAT format)
        {
            int[] arg = { width, height, (int)format };
            fixed (int* ptr = arg)
            {
                IntPtr retval = IntPtr.Zero;
                GameEngine.InvokeMemberFn(m_instanceId, "CreateNew", (IntPtr)ptr, out retval);
            }
            RefreshCachedProperties();            
        }

        public void Save(Uri ur)
        {
            if (!ur.IsAbsoluteUri)
                throw new ArgumentException("uri must be absolute");
            string localPath = ur.LocalPath;            
            fixed (char* ptr = localPath)
            {
                IntPtr retval = IntPtr.Zero;
                GameEngine.InvokeMemberFn(m_instanceId, "SaveToFile", (IntPtr)ptr, out retval);
            }            
        }
        public void LoadFromFile(Uri ur)
        {
            if (!ur.IsAbsoluteUri)
                throw new ArgumentException("uri must be absolute");
            string localPath = ur.LocalPath;            
            if (!File.Exists(localPath))
                throw new FileNotFoundException(localPath);
            fixed (char* chptr = localPath)
            {
                IntPtr ptr = new IntPtr((void*)chptr);
                int sz = localPath.Length * 2;
                GameEngine.SetObjectProperty(TypeId, m_instanceId, LoadFromFileId, ptr, sz);
            }            
            RefreshCachedProperties();
        }

        public void Convert(ImageDataFORMAT format)
        {
            GameEngine.SetObjectProperty(TypeId, m_instanceId, ConvertId, (uint)format);
            RefreshCachedProperties();
        }
        
        public bool IsValid
        {
            get { return m_instanceId != 0; }
        }
        public int Width
        {
            get { return m_width; }            
        }

        public int Height
        {
            get { return m_height; }            
        }

        public int BytesPerPixel
        {
            get { return m_bytesPerPixel; }
        }

        public int RowPitch
        {
            get { return m_rowPitch; }
        }

        public ImageDataFORMAT Format
        {
            get { return m_format; }
        }

        public IntPtr Data
        {
            get { return m_data; }            
        }
        public ulong InstanceId
        {
            get { return m_instanceId; }
        }
        protected override void Dispose(bool disposing)
        {
            if (m_instanceId != 0)
            {
                if(m_manageLifetime)
                    GameEngine.DestroyObject(TypeId, m_instanceId);

                m_instanceId = 0;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Apply the data to the given bound</summary>
        /// <param name="bound"></param>
        /// <param name="data">data to be applied</param>
        public void ApplyRegion(Bound2di bound, byte[] data)
        {
            int dataSize = bound.Width * bound.Height * BytesPerPixel;
            bool valid = bound.isValid
                && bound.x1 >= 0
                && bound.x2 <= Width
                && bound.y1 >= 0
                && bound.y2 <= Height
                && data != null
                && data.Length == dataSize;
            Debug.Assert(valid);
            if (!valid) return;

            // restore 
            int rowPitch = bound.Width * BytesPerPixel;
            fixed (byte* srcptr = data)
            {
                byte* src = srcptr;
                for (int cy = bound.y1; cy < bound.y2; cy++)
                {
                    byte* destPtr = GetPixel(bound.x1, cy);
                    for (int i = 0; i < rowPitch; i++)
                        *destPtr++ = *src++;
                }
            }

        }


        /// <summary>
        /// Copy region to outData</summary>
        /// <param name="bound">The region to be copied</param>
        /// <param name="outData">copy to outData</param>
        public void CopyRegion(Bound2di bound, byte[] outData)
        {
            int dataSize = bound.Width * bound.Height * BytesPerPixel;
            bool valid = bound.isValid
                && bound.x1 >= 0
                && bound.x2 <= Width
                && bound.y1 >= 0
                && bound.y2 <= Height
                && outData != null
                && outData.Length == dataSize;
            Debug.Assert(valid);
            if (!valid) return;

            int rowPitch = bound.Width * BytesPerPixel;
            fixed (byte* dest = outData)
            {
                byte* ds = dest;
                for (int cy = bound.y1; cy < bound.y2; cy++)
                {
                    byte* scrPtr = GetPixel(bound.x1, cy);
                    for (int i = 0; i < rowPitch; i++)
                        *ds++ = *scrPtr++;
                }
            }
        }

        private void RefreshCachedProperties()
        {            
            // data pointer.
            int datasize;
            GameEngine.GetObjectProperty(TypeId, BufferPointerId, m_instanceId, out m_data, out datasize);
            

            uint format;
            GameEngine.GetObjectProperty(TypeId, FormatId, m_instanceId, out format);
            m_format = (ImageDataFORMAT)format;

            GameEngine.GetObjectProperty(TypeId, RowPitchId, m_instanceId, out m_rowPitch);
            GameEngine.GetObjectProperty(TypeId, HeightId, m_instanceId, out m_height);
            GameEngine.GetObjectProperty(TypeId, WidthId, m_instanceId, out m_width);
            GameEngine.GetObjectProperty(TypeId, BytesPerPixelId, m_instanceId, out m_bytesPerPixel);
        }

        // cached props
        private int m_width;
        private int m_height;
        private int m_bytesPerPixel;
        private int m_rowPitch;
        private ImageDataFORMAT m_format;
        private IntPtr m_data;


        // method ids
        private readonly uint LoadFromFileId;
        private readonly uint BufferPointerId;
        private readonly uint WidthId;
        private readonly uint HeightId;
        private readonly uint BytesPerPixelId;
        private readonly uint RowPitchId;
        private readonly uint FormatId;
        public readonly uint ConvertId;
        private readonly uint TypeId;
        private bool m_manageLifetime;
        private ulong m_instanceId;
    }
     
    /// <summary>
    /// subset of DXGI format.</summary>
    public enum ImageDataFORMAT : uint
    {                                                
        R8G8B8A8_UNORM = 28,                        
        R32_FLOAT = 41,                        
        R16_FLOAT = 54,                
        R16_UINT = 57,        
        R16_SINT = 59,        
        R8_UNORM = 61,
        R8_UINT = 62,        
        R8_SINT = 64,                
        B8G8R8A8_UNORM = 87,
        B8G8R8X8_UNORM = 88
    }
    
   
}
