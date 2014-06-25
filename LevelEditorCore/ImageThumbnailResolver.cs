//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;

using Sce.Atf;
using Sce.Atf.Applications;


namespace LevelEditorCore
{
    /// <summary>
    /// Thumbnail resolver for image resources
    /// The ImageThumbnailResolver in ATF does not resize the image which result 
    /// large memory usage.</summary>
    [Export(typeof(ImageThumbnailResolver))]
    [Export(typeof(IThumbnailResolver))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ImageThumbnailResolver : IThumbnailResolver, IInitializable
    {
        #region IInitializable Members

        public void Initialize()
        {
        }

        #endregion

        /// <summary>
        /// Resolves Resource to a thumbnail image</summary>
        /// <param name="resourceUri">Resource URI to resolve</param>
        /// <returns>Thumbnail image</returns>
        public Image Resolve(Uri resourceUri)
        {
            string path = resourceUri.LocalPath;
            if (path == null || !File.Exists(path))
                return null;

            Image image = null;
            string extension = Path.GetExtension(path);

            if (extension.EndsWith("jpg", StringComparison.InvariantCultureIgnoreCase) ||
                extension.EndsWith("jpeg", StringComparison.InvariantCultureIgnoreCase) ||
                extension.EndsWith("bmp", StringComparison.InvariantCultureIgnoreCase) ||
                extension.EndsWith("png", StringComparison.InvariantCultureIgnoreCase) ||
                extension.EndsWith("tif", StringComparison.InvariantCultureIgnoreCase) ||
                extension.EndsWith("tiff", StringComparison.InvariantCultureIgnoreCase) ||
                extension.EndsWith("gif", StringComparison.InvariantCultureIgnoreCase))
            {
                Image imgTemp = null;
                using (FileStream strm = File.OpenRead(path))
                {
                    imgTemp = new Bitmap(strm);
                    strm.Close();
                }
                int width, height;
                float aspect = (float)imgTemp.Width / (float)imgTemp.Height;
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

                image = GdiUtil.ResizeImage(imgTemp, width, height);
                imgTemp.Dispose();
            }

            return image;
        }


        private const float ThumbnailSize = 96;
    }
}
