//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Runtime.InteropServices;

using Sce.Atf.Dom;
using Sce.Atf.Adaptation;

using LevelEditorCore;

namespace RenderingInterop
{
    /// <summary>
    /// Adapter for DomNode that have runtime counterpart.
    /// It hold native object id and pushes dom changes to native object</summary>
    public class NativeObjectAdapter : DomNodeAdapter, INativeObject
    {
        protected override void OnNodeSet()
        {
            base.OnNodeSet();            
            DomNode node = DomNode;           
            node.AttributeChanged += node_AttributeChanged;            
            TypeId = (uint)DomNode.Type.GetTag(NativeAnnotations.NativeType);
            
        }
       
        void node_AttributeChanged(object sender, AttributeEventArgs e)
        {
            // process events only for the DomNode attached to this adapter.
            if (this.DomNode != e.DomNode)
                return;
            UpdateNativeProperty(e.AttributeInfo);
        }

        
        /// <summary>
        /// Updates all the shared properties  
        /// between this and native object 
        /// only call onece.
        /// </summary>
        public void UpdateNativeOjbect()
        {
            foreach (AttributeInfo attribInfo in this.DomNode.Type.Attributes)
            {
                UpdateNativeProperty(attribInfo);
            }
        }

      
        unsafe private void UpdateNativeProperty(AttributeInfo attribInfo)
        {
            object idObj = attribInfo.GetTag(NativeAnnotations.NativeProperty);
            if (idObj == null) return;
            uint id = (uint)idObj;
            if (this.InstanceId == 0)
                return;

            AttributeInfo mappedAttribute = attribInfo.GetTag(NativeAnnotations.MappedAttribute) as AttributeInfo;
            DomNodeType definingType = (mappedAttribute != null) ? mappedAttribute.DefiningType : attribInfo.DefiningType;
            uint typeId = (uint)definingType.GetTag(NativeAnnotations.NativeType);
            
            Type clrType = attribInfo.Type.ClrType;
            Type elmentType = clrType.GetElementType();

            object data = this.DomNode.GetAttribute(attribInfo);
            
            if (clrType.IsArray && elmentType.IsPrimitive)
            {
                GCHandle pinHandle = new GCHandle();
                try
                {
                    pinHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                    IntPtr ptr = pinHandle.AddrOfPinnedObject();
                    int sz = Marshal.SizeOf(elmentType);
                    GameEngine.SetObjectProperty(typeId, InstanceId, id, ptr, sz * attribInfo.Type.Length);

                }
                finally
                {
                    if (pinHandle.IsAllocated)
                        pinHandle.Free();
                }                
            }
            else
            {
                IntPtr ptr = IntPtr.Zero;
                int sz = 0;
                if (clrType == typeof(string))
                {
                    string str = (string)data;
                    if (!string.IsNullOrEmpty(str))
                    {
                        fixed (char* chptr = str)
                        {
                            ptr = new IntPtr((void*)chptr);
                            sz = str.Length * 2;
                            GameEngine.SetObjectProperty(typeId, InstanceId, id, ptr, sz);
                        }
                        return;
                    }
                }
                else if (clrType == typeof(DateTime))
                {
                    DateTime dt = (DateTime)data;
                    float seconds = (float)(dt.Hour * 60 * 60 + dt.Minute * 60 + dt.Second);
                    float secondsInADay = 60.0f * 60.0f * 24.0f; // sec per minute * min per hour * hour per day
                    float normalizedTime =  seconds / secondsInADay; // normalize 0.0 to 1.0
                    ptr = new IntPtr(&normalizedTime);
                    sz = sizeof(float);
                }
                else if (clrType == typeof(bool))
                {                  
                    bool val = (bool)data;
                    ptr = new IntPtr(&val);
                    sz = sizeof(bool);
                }
                else if (clrType == typeof(byte))
                {
                    byte val = (byte)data;
                    ptr = new IntPtr(&val);
                    sz = sizeof(byte);
                }
                else if (clrType == typeof(sbyte))
                {
                    sbyte val = (sbyte)data;
                    ptr = new IntPtr(&val);
                    sz = sizeof(sbyte);
                }
                else if (clrType == typeof(short))
                {
                    short val = (short)data;
                    ptr = new IntPtr(&val);
                    sz = sizeof(short);
                }
                else if (clrType == typeof(ushort))
                {
                    ushort val = (ushort)data;
                    ptr = new IntPtr(&val);
                    sz = sizeof(ushort);
                }
                else if (clrType == typeof(int))
                {
                    int val = (int)data;
                    ptr = new IntPtr(&val);
                    sz = sizeof(int);
                }
                else if (clrType == typeof(uint))
                {
                    uint val = (uint)data;
                    ptr = new IntPtr(&val);
                    sz = sizeof(uint);
                }
                else if (clrType == typeof(long))
                {
                    long val = (long)data;
                    ptr = new IntPtr(&val);
                    sz = sizeof(long);
                }
                else if (clrType == typeof(ulong))
                {
                    ulong val = (ulong)data;
                    ptr = new IntPtr(&val);
                    sz = sizeof(ulong);
                }
                else if (clrType == typeof(float))
                {
                    float val = (float)data;
                    ptr = new IntPtr(&val);
                    sz = sizeof(float);
                }
                else if (clrType == typeof(double))
                {
                    double val = (double)data;
                    ptr = new IntPtr(&val);
                    sz = sizeof(double);
                }
                else if (clrType == typeof(System.Uri))
                {
                    if(data != null && !string.IsNullOrWhiteSpace(data.ToString()))
                    {
                        Uri uri = (Uri)data;                        
                        string str = uri.LocalPath;
                        fixed (char* chptr = str)
                        {
                            ptr = new IntPtr((void*)chptr);
                            sz = str.Length * 2;
                            GameEngine.SetObjectProperty(typeId, InstanceId, id, ptr, sz);
                        }
                        return;
                    }
                }
                else if (clrType == typeof(DomNode))
                {
                    // this is a 'reference' to an object
                    DomNode node = (DomNode)data;
                    NativeObjectAdapter nativeGob = node.As<NativeObjectAdapter>();
                    if(nativeGob != null)
                    {
                        ptr = new IntPtr((void*)nativeGob.InstanceId);
                        sz = sizeof(ulong);                        
                    }                    
                }

                GameEngine.SetObjectProperty(typeId, InstanceId, id, ptr, sz);
            }
        }

        public uint TypeId
        {
            get;
            private set;
        }        

        /// <summary>
        /// this method is exclusively used by GameEngine class.                
        public void SetNativeHandle(ulong instanceId)
        {            
            m_instanceId = instanceId;           
        }

        private ulong m_instanceId;
        
        #region INativeObject Members
        public void InvokeFunction(string fn, IntPtr arg, out IntPtr retval)
        {
            GameEngine.InvokeMemberFn(InstanceId,fn, arg, out retval);
        }

        /// <summary>
        /// Gets native id for this instance of the C++ counter part of this object.</summary>
        public ulong InstanceId
        {
            get { return m_instanceId; }

        }
        #endregion
    }
}
