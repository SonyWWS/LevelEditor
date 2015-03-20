//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using LevelEditorCore;
using Sce.Atf;
using Sce.Atf.Applications;

namespace RenderingInterop
{
    /// <summary>
    /// Generates thumbnail for dds and tga textures.
    /// </summary>
    [Export(typeof(IThumbnailResolver))]    
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TextureThumbnailResolver : IThumbnailResolver
    {        
        #region IThumbnailResolver Members
        Image IThumbnailResolver.Resolve(Uri resourceUri)
        {
            Image img = null;
            string path = resourceUri.LocalPath;
            if (path == null || !File.Exists(path))
                return null;

            string extension = Path.GetExtension(path).ToLower();
            var res = m_gameEngine.Info.ResourceInfos.GetByType(ResourceTypes.Texture);

            if(res.IsSupported(extension))
            {
                ImageData imgdata = new ImageData();
                imgdata.LoadFromFile(resourceUri);                
                imgdata.Convert(ImageDataFORMAT.B8G8R8A8_UNORM);                
                if (imgdata.IsValid)
                {
                    Bitmap tempImage = new Bitmap((int)imgdata.Width, (int)imgdata.Height, PixelFormat.Format32bppArgb);
                    BitmapData destData = null;

                    try
                    {
                        destData = tempImage.LockBits(
                            new Rectangle(0, 0, tempImage.Width, tempImage.Height),
                            ImageLockMode.WriteOnly,
                            tempImage.PixelFormat);

                        unsafe
                        {
                            byte* srcScan0 = (byte*)imgdata.Data;
                            int srcStride = imgdata.RowPitch;

                            byte* destScan0 = (byte*)destData.Scan0;
                            int destStride = destData.Stride;

                            for (int y = 0; y < tempImage.Height; y++)
                            {
                                uint* srcRow = (uint*)(srcScan0 + y * srcStride);
                                uint* destRow = (uint*)(destScan0 + y * destStride);
                                for (int x = 0; x < tempImage.Width; x++)
                                {
                                    *destRow++ = *srcRow++;
                                    
                                }
                            }

                        }
                    }
                    finally
                    {
                        tempImage.UnlockBits(destData);
                    }

                    int width, height;
                    float aspect = (float)tempImage.Width / (float)tempImage.Height;
                    if (aspect > 1.0f)
                    {
                        width = (int)ThumbnailSize;
                        height = (int)Math.Round(ThumbnailSize / aspect);
                    }
                    else
                    {
                        height = (int)ThumbnailSize;
                        width = (int)Math.Round(ThumbnailSize * aspect);
                    }
                    img = GdiUtil.ResizeImage(tempImage, width, height);
                    tempImage.Dispose();
                    imgdata.Dispose();

                }

            }           
            return img;
        }
      
        private const float ThumbnailSize = 96;
        #endregion

        [Import(AllowDefault = false)]
        private IGameEngineProxy m_gameEngine;
    }
}
