//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;


namespace RenderingInterop
{
    public class TextureRenderSurface : DisposableObject
    {
        public TextureRenderSurface(int width, int height)
        {
            Size sz = new Size(width, height);
            IntPtr ptr = IntPtr.Zero;
            unsafe
            {
                ptr = new IntPtr(&sz);
            };

            int sizeInBytes = System.Runtime.InteropServices.Marshal.SizeOf(sz);

            string typeName = this.GetType().Name;
            m_typeId = GameEngine.GetObjectTypeId(typeName);
            BkgColorPropId = GameEngine.GetObjectPropertyId(m_typeId, "BkgColor");

            m_intanceId = GameEngine.CreateObject(m_typeId, ptr, sizeInBytes);

        }

        public ulong InstanceId
        {
            get { return m_intanceId; }
        }

        public Color BackColor
        {
            set
            {
                GameEngine.SetObjectProperty(m_typeId, m_intanceId, BkgColorPropId, value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (m_intanceId != 0)
            {
                GameEngine.DestroyObject(m_typeId, m_intanceId);
                m_intanceId = 0;
            }
            base.Dispose(disposing);
        }


        // instance id
        private readonly uint BkgColorPropId;
        private readonly uint m_typeId;
        private ulong m_intanceId;
    }
}
