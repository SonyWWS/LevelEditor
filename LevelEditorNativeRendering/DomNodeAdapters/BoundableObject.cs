//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

using LevelEditorCore;
using LevelEditorCore.VectorMath;
using Sce.Atf.VectorMath;

namespace RenderingInterop
{
    class BoundableObject : DomNodeAdapter, IBoundable
    {

        protected override void OnNodeSet()
        {
            // find Bounds native properties.
            foreach (var node  in DomNode.Lineage)
            {
                NativeAttributeInfo[] nativeAttribs = node.Type.GetTag<NativeAttributeInfo[]>();
                if (nativeAttribs == null) continue;
                foreach (var attrib in nativeAttribs)
                {
                    if (attrib.Name == "Bounds")
                    {
                        m_boundsAttrib = attrib;
                    }
                    else if (attrib.Name == "LocalBounds")
                    {
                        m_localBoundsAttrib = attrib;
                    }

                }
            }            
        }


        #region IBoundable Members

        public AABB LocalBoundingBox
        {
            get { return GetAABB(m_localBoundsAttrib); }
        }
        public AABB BoundingBox
        {
            get { return GetAABB(m_boundsAttrib); }
        }

        #endregion

        private AABB GetAABB(NativeAttributeInfo attrInfo)
        {
            unsafe
            {
                NativeObjectAdapter nativeobj = this.As<NativeObjectAdapter>();
                ulong instanceId = nativeobj != null ? nativeobj.InstanceId : 0;
                if (instanceId != 0)
                {
                    int datasize = 0;
                    IntPtr data;
                    GameEngine.GetObjectProperty(attrInfo.TypeId, attrInfo.PropertyId, this.Cast<NativeObjectAdapter>().InstanceId
                        , out data, out datasize);
                    Vec3F* vecptr = (Vec3F*)data.ToPointer();                    
                    AABB bound = new AABB(vecptr[0], vecptr[1]);
                    return bound;
                }
                return new AABB();
            }
        }

        private NativeAttributeInfo m_boundsAttrib;
        private NativeAttributeInfo m_localBoundsAttrib;        
    }
}
