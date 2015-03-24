//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Linq;
using System.Threading;

using Sce.Atf;
using Sce.Atf.VectorMath;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;

using LevelEditorCore;

namespace RenderingInterop
{
    /// <summary>
    /// Exposes a minimum set of game-engine functionalities 
    /// for LevelEditor purpose.</summary>    
    [Export(typeof(IGameEngineProxy))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [SuppressUnmanagedCodeSecurity()]
    public unsafe class GameEngine : DisposableObject, IGameEngineProxy, IInitializable
    {
        public GameEngine()
        {
            s_inist = this;
            GameEngine.Init();
        }
        #region IGameEngineProxy Members

        public EngineInfo Info
        {
            get { return m_engineInfo; }
        }

        /// <summary>
        /// Sets active game world.</summary>
        /// <param name="game">Game world to set</param>
        public void SetGameWorld(IGame game)
        {
            NativeObjectAdapter nobject = game.Cast<NativeObjectAdapter>();            
            NativeSetGameLevel(nobject.InstanceId);
        }

        /// <summary>
        /// Updates game world</summary>
        /// <param name="ft">Frame time</param>
        /// <param name="updateType">Update type</param>        
        public void Update(FrameTime ft, UpdateType updateType)
        {
            NativeUpdate(&ft, updateType);
        }

        public void WaitForPendingResources()
        {
            NativeWaitForPendingResources();
        }
        #endregion

        #region IInitializable Members

        void IInitializable.Initialize()
        {
           
            
        }

        #endregion

        private EngineInfo m_engineInfo;        
        private void PopulateEngineInfo(string engineInfoStr)
        {
            if (m_engineInfo != null) return;
            if (!string.IsNullOrWhiteSpace(engineInfoStr))
                m_engineInfo = new EngineInfo(engineInfoStr);
        }
        #region initialize and shutdown


        private static GameEngine s_inist;
        /// <summary>
        /// init game engine 
        /// call it one time during startup on the UI thread.</summary>        
        public static void Init()
        {
            // the full dll name can be loaded in a config when needed.
            if (s_libHandle != IntPtr.Zero)
                return;

            CriticalError = s_notInitialized;
            try
            {
                Uri uri = new Uri(System.Windows.Forms.Application.StartupPath);
                string dllDir = uri.LocalPath + "\\NativePlugin";

                string dllName = "LvEdRenderingEngine.dll";

                if (IntPtr.Size == 4)
                {// 32 bit
                    s_fullDllName = dllDir + "\\x86\\" + dllName;
                }
                else if (IntPtr.Size == 8)
                {// 64 bit.
                    s_fullDllName = dllDir + "\\x64\\" + dllName;
                }
                else
                {
                    throw new Exception("unsupported address space");
                }

                if (!File.Exists(s_fullDllName))
                    throw new FileNotFoundException(s_fullDllName);

                s_libHandle = NativeMethods.LoadLibrary(s_fullDllName);
                if (s_libHandle == IntPtr.Zero)
                {
                    int hr = Marshal.GetHRForLastWin32Error();
                    Marshal.ThrowExceptionForHR(hr);
                }

                // verify entry points
                /*
                Type type = typeof(GameEngine);

                foreach (MethodInfo minfo in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
                {                    
                    foreach( object obj in minfo.GetCustomAttributes(false))
                    {                        
                        DllImportAttribute dllimport = obj as DllImportAttribute;
                        if (dllimport != null)
                        {
                            string entryname = dllimport.EntryPoint;                                                        
                            // verify entry point name.
                            IntPtr fntPtr = NativeMethods.GetProcAddress(s_libHandle, entryname);
                            if (fntPtr == IntPtr.Zero)
                                throw new ArgumentException(string.Format("Can't find native function: {0}(...) in {1}", dllimport.EntryPoint, dllName));                            
                            break;
                        }
                    }                        
                }
                 */


                CriticalError = string.Empty;

                IntPtr data;
                s_invalidateCallback = new InvalidateViewsDlg(InvalidateViews);
                s_logInstance = new LogCallbackType(LogCallback);
                NativeInitialize(s_logInstance, s_invalidateCallback, out data);                
                if (data != IntPtr.Zero)
                {
                    string engineInfo = Marshal.PtrToStringUni(data);
                    s_inist.PopulateEngineInfo(engineInfo);
                }
                
                // get SynchronizationContext for current thread.
                // Note: 
                s_syncContext = SynchronizationContext.Current;
                //Application.AddMessageFilter(new MessageFilter());
                
                Util3D.Init();
            }
            catch (Exception e)
            {
                Console.WriteLine("loading dll: " + s_fullDllName);
                Console.WriteLine("Engine init failed: " + e.Message);
                CriticalError = e.Message;
            }

        }

        /// <summary>
        /// Test if GameEngine is in error.
        /// </summary>
        public static bool IsInError
        {
            get { return CriticalError.Length > 0; }
        }

        /// <summary>
        /// Gets CriticalError or empty string.        
        /// </summary>
        public static string CriticalError { get; private set; }

        public static string FullDllName
        {
            get { return s_fullDllName; }
        }

        /// <summary>
        /// delete all the game object in native side.
        /// and reset the game to default.
        /// </summary>
        public static void Clear()
        {
            s_idToDomNode.Clear();
            NativeClear();
        }

        /// <summary>
        /// shutdown game engine.
        /// call it one time on application exit.
        /// </summary>
        public static void Shutdown()
        {
            if (s_libHandle != IntPtr.Zero)
            {
                while (s_idToDomNode.Count > 0)
                {
                    DestroyObject(s_idToDomNode.Values.First());
                }
             
                NativeShutdown();
                NativeMethods.FreeLibrary(s_libHandle);
                s_libHandle = IntPtr.Zero;
                CriticalError = s_notInitialized;
            }
        }

        public static event EventHandler RefreshView = delegate { };

        #endregion

        #region call back for invalidating views.
        private delegate void InvalidateViewsDlg();
        private static InvalidateViewsDlg s_invalidateCallback;
        private static void InvalidateViews()
        {
            if (s_syncContext != null)
            {
                s_syncContext.Post(obj=>RefreshView(null, EventArgs.Empty),null);
            }
        }
        private static SynchronizationContext s_syncContext;
        #endregion

        #region log callbacks

        [UnmanagedFunctionPointer(CallingConvention.StdCall,CharSet = CharSet.Unicode)]
        private delegate void LogCallbackType(int messageType, string text);
        private static LogCallbackType s_logInstance;
        private static void LogCallback(int messageType, string text)
        {
            Console.Write(text);
            if (messageType == (int)OutputMessageType.Warning
                || messageType == (int)OutputMessageType.Error)
                Outputs.Write((OutputMessageType)messageType, text);                            
        }

        #endregion

        #region update and rendering
        
        public static void SetRenderState(RenderState renderState)
        {
            GameEngine.NativeSetRenderState(renderState.InstanceId);
        }

        public static void SetGameLevel(NativeObjectAdapter game)
        {
            ulong insId = game != null ? game.InstanceId : 0;
            NativeSetGameLevel(insId);
        }

        public static NativeObjectAdapter GetGameLevel()
        {
            ulong insId = NativeGetGameLevel();
            if (insId != 0)
                return GetAdapterFromId(insId);
            else
                return null;
        }

        public static void Begin(ulong renderSurface, Matrix4F viewxform, Matrix4F projxfrom)
        {
            fixed (float* ptr1 = &viewxform.M11, ptr2 = &projxfrom.M11)
            {
                NativeBegin(renderSurface, ptr1, ptr2);
            }
        }

        public static void RenderGame()
        {
            NativeRenderGame();
        }

        public static bool SaveRenderSurfaceToFile(ulong renderSurface, string fileName)
        {
            return NativeSaveRenderSurfaceToFile(renderSurface, fileName);
        }

        public static void End()
        {
            NativeEnd();
        }

        #endregion

        #region Object management.


        public static uint GetObjectTypeId(string className)
        {
            if (IsInError) return 0;
            uint id = NativeGetObjectTypeId(className);
            if (id == 0)
            {
                CriticalError = className + " is not defined in runtime";
            }
            return id;
        }

        public static uint GetObjectPropertyId(uint typeId, string propertyName)
        {
            if (IsInError) return 0;
            uint propId = NativeGetObjectPropertyId(typeId, propertyName);
            if (propId == 0)
            {
                CriticalError = propertyName + " is not defined for typeid " + typeId;
            }
            return propId;
        }

        public static uint GetObjectChildListId(uint typeId, string listName)
        {
            if (IsInError) return 0;
            uint propId = NativeGetObjectChildListId(typeId, listName);
            if (propId == 0)
            {
                CriticalError = listName + " is not defined for typeid " + typeId;
            }
            return propId;
        }

        public static ulong CreateObject(uint typeId, IntPtr data, int size)
        {
            ulong instanceId = NativeCreateObject(typeId, data, size);
            return instanceId;
        }

        public static ulong CreateObject(NativeObjectAdapter gob)
        {
            ulong instanceId = CreateObject(gob.TypeId, IntPtr.Zero, 0);
            if (instanceId != 0)
            {
                s_idToDomNode.Add(instanceId, gob);
                gob.SetNativeHandle(instanceId);
            }
            return instanceId;
        }

        public static void DestroyObject(uint typeId, ulong instanceId)
        {
            if (s_libHandle == IntPtr.Zero) return;
            NativeDestroyObject(typeId, instanceId);
        }

        public static void DestroyObject(NativeObjectAdapter gob)
        {
            if (gob.InstanceId == 0)
                return;
            NativeDestroyObject(gob.TypeId, gob.InstanceId);
            ResetIds(gob);

        }

        private static void ResetIds(NativeObjectAdapter gob)
        {
            s_idToDomNode.Remove(gob.InstanceId);
            gob.SetNativeHandle(0);
            foreach (DomNode child in gob.DomNode.Children)
            {
                NativeObjectAdapter childObject = child.As<NativeObjectAdapter>();
                if (childObject != null)
                    ResetIds(childObject);
            }
        }

        public static void ObjectAddChild(uint typeId, uint listId, ulong parentId, ulong childId)
        {
            // a negative index means to 'append' the child to the parent list.
            NativeObjectAddChild(typeId, listId, parentId, childId, -1);
        }

        public static void ObjectInsertChild(uint typeId, uint listId, ulong parentId, ulong childId, int index)
        {
            // a negative index means to 'append' the child to the parent list.
            NativeObjectAddChild(typeId, listId, parentId, childId, index);
        }

        public static void ObjectInsertChild(NativeObjectAdapter parent, NativeObjectAdapter child, uint listId,int index)
        {
            uint typeId = parent != null ? parent.TypeId : 0;
            ulong parentId = parent != null ? parent.InstanceId : 0;
            ulong childId = child != null ? child.InstanceId : 0;
            ObjectInsertChild(typeId, listId, parentId, childId, index);
        }

        public static void ObjectAddChild(NativeObjectAdapter parent, NativeObjectAdapter child, uint listId)
        {
            uint typeId = parent != null ? parent.TypeId : 0;
            ulong parentId = parent != null ? parent.InstanceId : 0;
            ulong childId = child != null ? child.InstanceId : 0;
            ObjectAddChild(typeId, listId, parentId, childId);
        }

        public static void ObjectRemoveChild(uint typeId, uint listId, ulong parentId, ulong childId)
        {
            NativeObjectRemoveChild(typeId, listId, parentId, childId);
        }

        public static void ObjectRemoveChild(NativeObjectAdapter parent, NativeObjectAdapter child, uint listId)
        {
            uint typeId = parent != null ? parent.TypeId : 0;
            ulong parentId = parent != null ? parent.InstanceId : 0;
            ulong childId = child != null ? child.InstanceId : 0;
            ObjectRemoveChild(typeId, listId, parentId, childId);
        }

        public static void InvokeMemberFn(ulong instanceId, string fn, IntPtr arg, out IntPtr retVal)
        {
           NativeInvokeMemberFn(instanceId, fn, arg, out retVal);
        }

        public static void SetObjectProperty(uint typeid, ulong instanceId, uint propId, Color color)
        {
            Vec4F val = new Vec4F(color.R/255.0f, color.G/255.0f, color.B/255.0f, color.A/255.0f);
            IntPtr ptr = new IntPtr(&val);
            int sz = Marshal.SizeOf(val);
            NativeSetObjectProperty(typeid, propId, instanceId, ptr, sz);
        }

        public static void SetObjectProperty(uint typeid, ulong instanceId, uint propId, uint val)
        {
            IntPtr ptr = new IntPtr(&val);
            NativeSetObjectProperty(typeid, propId, instanceId, ptr, sizeof (uint));
        }

        public static void SetObjectProperty(uint typeid, ulong instanceId, uint propId, Vec4F val)
        {
            IntPtr ptr = new IntPtr(&val);
            int sizeInBytes = Marshal.SizeOf(val);
            NativeSetObjectProperty(typeid, propId, instanceId, ptr, sizeInBytes);
        }

        public static void SetObjectProperty(uint typeid, ulong instanceId, uint propId, Size sz)
        {
            IntPtr ptr = new IntPtr(&sz);
            int sizeInBytes = Marshal.SizeOf(sz);
            NativeSetObjectProperty(typeid, propId, instanceId, ptr, sizeInBytes);
        }

        public static void SetObjectProperty(uint typeid, ulong instanceId, uint propId, IntPtr data, int size)
        {
            NativeSetObjectProperty(typeid, propId, instanceId, data, size);
        }

        public static void GetObjectProperty(uint typeId, uint propId, ulong instanceId, out int data)
        {
            int datasize = 0;
            IntPtr ptrData;
            GetObjectProperty(typeId, propId, instanceId, out ptrData, out datasize);
            if (datasize > 0)
            {
                data = *(int*)ptrData.ToPointer();
            }
            else
            {
                data = 0;
            }

        }

        public static void GetObjectProperty(uint typeId, uint propId, ulong instanceId, out uint data)
        {
            int datasize = 0;
            IntPtr ptrData;
            GetObjectProperty(typeId, propId, instanceId, out ptrData, out datasize);
            if (datasize > 0)
            {
                data = *(uint*)ptrData.ToPointer();
            }
            else
            {
                data = 0;
            }

        }
        public static void GetObjectProperty(uint typeId, uint propId, ulong instanceId, out IntPtr data, out int size)
        {
            NativeGetObjectProperty(typeId,propId,instanceId, out data,out size);
        }
        public static NativeObjectAdapter GetAdapterFromId(ulong instanceId)
        {
            return s_idToDomNode[instanceId];
        }

        #endregion

        #region picking and selection
        public static bool RayPick(Matrix4F viewxform, Matrix4F projxfrom, Ray3F rayW, bool skipSelected, out HitRecord hit)
        {
            HitRecord* nativeHits = null;
            int count;

            //bool skipSelected,
            fixed (float* ptr1 = &viewxform.M11, ptr2 = &projxfrom.M11)
            {
                NativeRayPick(
                ptr1,
                ptr2,
                &rayW,
                skipSelected,
                &nativeHits,
                out count);
            }

            if(count > 0)
            {
                hit = *nativeHits;
            }
            else
            {
                hit = new HitRecord();
            }
           
            return count > 0;

        }
        public static HitRecord[] RayPick(Matrix4F viewxform, Matrix4F projxfrom, Ray3F rayW, bool skipSelected)
        {
            HitRecord* nativeHits = null;
            int count;

            fixed (float* ptr1 = &viewxform.M11, ptr2 = &projxfrom.M11)
            {
                NativeRayPick(
                ptr1,
                ptr2,
                &rayW,
                skipSelected,
                &nativeHits,
                out count);                
            }
            
            var objects = new List<HitRecord>();

            for (int k = 0; k < count; k++)
            {                
                objects.Add(*nativeHits);                
                nativeHits++;
            }
            return objects.ToArray();

        }

        private static float[] s_rect = new float[4];
        public static HitRecord[] FrustumPick(ulong renderSurface, Matrix4F viewxform,
                                               Matrix4F projxfrom,
                                               RectangleF rect)
        {            
            s_rect[0] = rect.X;
            s_rect[1] = rect.Y;
            s_rect[2] = rect.Width;
            s_rect[3] = rect.Height;

            HitRecord* nativeHits = null;
            int count;

            fixed (float* ptr1 = &viewxform.M11, ptr2 = &projxfrom.M11)
            {
                NativeFrustumPick(
                    renderSurface,
                    ptr1,
                    ptr2,
                    s_rect,
                    &nativeHits,
                    out count);
            }

            var objects = new List<HitRecord>();
            
            for (int k = 0; k < count; k++)
            {
                HitRecord nativehit = *nativeHits;
                objects.Add(nativehit);                
                nativeHits++;
            }
            return objects.ToArray();
        }

        public static void SetSelection(IEnumerable<NativeObjectAdapter> selection)
        {
            List<ulong> ids = new List<ulong>();
            foreach (NativeObjectAdapter nativeObj in selection)
            {
                ids.Add(nativeObj.InstanceId);
            }

            NativeSetSelection(ids.ToArray(), ids.Count);
        }

        #endregion

        #region basic rendering 
        // create vertex buffer with given vertex format from user data.
        public static ulong CreateVertexBuffer(VertexPN[] buffer)
        {
            if (buffer == null || buffer.Length < 2)
                return 0;

            ulong vbId = 0;
            fixed (float* ptr = &buffer[0].Position.X)
            {
                vbId = NativeCreateVertexBuffer(VertexFormat.VF_PN, ptr, (uint)buffer.Length);
            }
            return vbId;
        }


        // create vertex buffer with given vertex format from user data.
        public static ulong CreateVertexBuffer(Vec3F[] buffer)
        {
            if (buffer == null || buffer.Length < 2)
                return 0;

            ulong vbId = 0;
            fixed (float* ptr = &buffer[0].X)
            {
                vbId = NativeCreateVertexBuffer(VertexFormat.VF_P, ptr, (uint)buffer.Length);
            }
            return vbId;
        }

        // Create index buffer from user data.
        public static ulong CreateIndexBuffer(uint[] buffer)
        {            
            fixed (uint* ptr = buffer)
            {
                return NativeCreateIndexBuffer(ptr, (uint)buffer.Length);
            }            
        }
    
        // deletes index/vertex buffer.
        public static void DeleteBuffer(ulong bufferId)
        {
            if (bufferId == 0) return;
            NativeDeleteBuffer(bufferId);
        }

        /// <summary>
        /// Sets render flags used for basic drawing.</summary>        
        public static void SetRendererFlag(BasicRendererFlags renderFlags)
        {
            NativeSetRendererFlag(renderFlags);
        }

        //Draw primitive with the given parameters.
        public static void DrawPrimitive(PrimitiveType pt,
                                            ulong vb,
                                            uint StartVertex,
                                            uint vertexCount,
                                            Color color,
                                            Matrix4F xform)
                                            
        {
            Vec4F vc;
            vc.X = color.R / 255.0f;
            vc.Y = color.G / 255.0f;
            vc.Z = color.B / 255.0f;
            vc.W = color.A / 255.0f;
            fixed (float* mtrx = &xform.M11)
            {
                NativeDrawPrimitive(pt, vb, StartVertex, vertexCount, &vc.X, mtrx);
            }

        }

        public static void DrawIndexedPrimitive(PrimitiveType pt,
                                                ulong vb,
                                                ulong ib,
                                                uint startIndex,
                                                uint indexCount,
                                                uint startVertex,
                                                Color color,
                                                Matrix4F xform)
                                                
        {
            Vec4F vc;
            vc.X = color.R / 255.0f;
            vc.Y = color.G / 255.0f;
            vc.Z = color.B / 255.0f;
            vc.W = color.A / 255.0f;
            fixed (float* mtrx = &xform.M11)
            {
                NativeDrawIndexedPrimitive(pt, vb, ib, startIndex, indexCount, startVertex, &vc.X, mtrx);
            }

        }
        #endregion

        #region font and text rendering

        public static ulong CreateFont(string fontName,float pixelHeight,FontStyle fontStyles)
        {
            return NativeCreateFont(fontName, pixelHeight, fontStyles);
        }

        public static void DeleteFont(ulong fontId)
        {
            NativeDeleteFont(fontId);
        }

        public static void DrawText2D(string text, ulong fontId, int x, int y, Color color)
        {
            //ulong fontId, string text, int x, int y, int color);
            NativeDrawText2D(fontId, text, x, y, color.ToArgb());
        }

        #endregion

        #region private members

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_Initialize", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeInitialize(LogCallbackType logCallback, InvalidateViewsDlg invalidateCallback,
            out IntPtr engineInfo);

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_Shutdown", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeShutdown();
        
        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_Clear")]
        private static extern void NativeClear();
        
        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_GetObjectTypeId", CallingConvention = CallingConvention.StdCall)]
        private static extern uint NativeGetObjectTypeId(string className);

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_GetObjectPropertyId", CallingConvention = CallingConvention.StdCall)]
        private static extern uint NativeGetObjectPropertyId(uint id, string propertyName);

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_GetObjectChildListId", CallingConvention = CallingConvention.StdCall)]
        private static extern uint NativeGetObjectChildListId(uint id, string listName);

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_CreateObject", CallingConvention = CallingConvention.StdCall)]
        private static extern ulong NativeCreateObject(uint typeId, IntPtr data, int size);

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_DestroyObject", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeDestroyObject(uint typeId, ulong instanceId);

        
        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_InvokeMemberFn", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeInvokeMemberFn(ulong instanceId, string fn, IntPtr arg, out IntPtr retVal);
        
        
        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_SetObjectProperty", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeSetObjectProperty(uint typeId, uint propId, ulong instanceId, IntPtr data, int size);

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_GetObjectProperty", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeGetObjectProperty(uint typeId, uint propId, ulong instanceId, out IntPtr data, out int size);

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_ObjectAddChild", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeObjectAddChild(uint typeid, uint listId, ulong parentId, ulong childId, int index);

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_ObjectRemoveChild", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeObjectRemoveChild(uint typeid, uint listId, ulong parentId, ulong childId);


        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_RayPick", CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeRayPick(
            [In] float* viewxform,
            [In] float* projxfrom,
            [In] Ray3F* rayW,
            [In] bool skipSelected,
            [Out] HitRecord** instanceIds,
            [Out] out int count);


        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_FrustumPick", CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeFrustumPick(
            [In]ulong renderSurface,
            [In]float* viewxform, 
            [In]float* projxfrom, 
            [In]float[] rect,
            [Out]HitRecord** instanceIds, 
            [Out]out int count);

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_SetSelection", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeSetSelection(ulong[] instanceIds, int count);
        
        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_SetRenderState", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeSetRenderState(ulong instanceId);

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_SetGameLevel", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeSetGameLevel(ulong instanceId);

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_GetGameLevel", CallingConvention = CallingConvention.StdCall)]
        private static extern ulong NativeGetGameLevel();

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_WaitForPendingResources", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeWaitForPendingResources();

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_Update", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeUpdate(FrameTime* time, UpdateType updateType);

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_Begin", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeBegin(ulong renderSurface, float* viewxform, float* projxfrom);
        
        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_End")]
        private static extern void NativeEnd();

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_RenderGame")]
        private static extern void NativeRenderGame();
       
        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_SaveRenderSurfaceToFile", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private static extern bool NativeSaveRenderSurfaceToFile(ulong renderSurface, string fileName);

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_CreateVertexBuffer", CallingConvention = CallingConvention.StdCall)]
        private static extern ulong NativeCreateVertexBuffer(VertexFormat vf, void* buffer, uint vertexCount);
        
        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_CreateIndexBuffer", CallingConvention = CallingConvention.StdCall)]
        private static extern ulong NativeCreateIndexBuffer(uint* buffer, uint indexCount);

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_DeleteBuffer", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeDeleteBuffer(ulong buffer);



        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_SetRendererFlag", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeSetRendererFlag(BasicRendererFlags renderFlag);
        

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_DrawPrimitive", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeDrawPrimitive(PrimitiveType pt,
                                                        ulong vb,
                                                        uint StartVertex,
                                                        uint vertexCount,
                                                        float* color,
                                                        float* xform);
                                                    

        [DllImportAttribute("LvEdRenderingEngine", EntryPoint = "LvEd_DrawIndexedPrimitive", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeDrawIndexedPrimitive(PrimitiveType pt,
                                                        ulong vb,
                                                        ulong ib,
                                                        uint startIndex,
                                                        uint indexCount,
                                                        uint startVertex,
                                                        float* color,
                                                        float* xform);
                                                        
        [DllImport("LvEdRenderingEngine", EntryPoint = "LvEd_CreateFont", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private static extern ulong NativeCreateFont(string fontName, float pixelHeight, FontStyle fontStyles);

        [DllImport("LvEdRenderingEngine", EntryPoint = "LvEd_DeleteFont", CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeDeleteFont(ulong fontId);

        [DllImport("LvEdRenderingEngine", EntryPoint = "LvEd_DrawText2D", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        private static extern void NativeDrawText2D(ulong fontId, string text, int x, int y, int color);
        
        private static string s_notInitialized = "Not initialized, please call Initialize()";
        
        private static void ThrowExceptionForLastError()
        {
            int hr = Marshal.GetHRForLastWin32Error();
            Marshal.ThrowExceptionForHR(hr);
        }

        //private static FntDlg GetFunction<FntDlg>(string fntName) where FntDlg : class
        //{
        //    IntPtr fntPtr = NativeMethods.GetProcAddress(s_libHandle, fntName);
        //    if (fntPtr == IntPtr.Zero)
        //        throw new ArgumentException("can't find functions: " + fntName);

        //    object dlg = Marshal.GetDelegateForFunctionPointer(fntPtr, typeof(FntDlg));
        //    return (FntDlg)dlg;
        //}

        private static string s_fullDllName;
        private static IntPtr s_libHandle;

        private static Dictionary<ulong, NativeObjectAdapter> s_idToDomNode = new Dictionary<ulong, NativeObjectAdapter>();
        private static class NativeMethods
        {
            [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr LoadLibrary(string fileName);

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            [DllImport("kernel32", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool FreeLibrary(IntPtr hModule);


            [DllImport("kernel32")]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procname);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr GetModuleHandle(string moduleName);

        }



        //private class MessageFilter : IMessageFilter
        //{
        //    #region IMessageFilter Members

        //    bool IMessageFilter.PreFilterMessage(ref Message m)
        //    {
        //        const int WM_USER = 0x0400;
        //        const int InvalidateViews = WM_USER + 0x1;

        //        if (m.Msg == InvalidateViews)
        //        {
        //            RefreshView(this, EventArgs.Empty);
        //            return true;
        //        }
        //        return false;
        //    }

        //    #endregion

        //}
        #endregion
    }
}

