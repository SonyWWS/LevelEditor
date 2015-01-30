//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.IO;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;
using Sce.Atf.Controls.PropertyEditing;

using LevelEditorCore;
using LevelEditorCore.VectorMath;
using RenderingInterop;


using PropertyDescriptor = System.ComponentModel.PropertyDescriptor;

namespace LevelEditor.Terrain
{
    public unsafe class TerrainGob : DomNodeAdapter
        ,IPropertyEditingContext 
        ,IEditableResourceOwner
        ,ITerrainSurface
    {
        public static TerrainGob Create(string name, string hmPath, float cellSize)
        {            
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(name);

            if (!File.Exists(hmPath))
                throw new ArgumentException(hmPath + " does not exist");
            
            Uri ur = new Uri(hmPath);
            DomNode terrainNode = new DomNode(Schema.terrainGobType.Type);            
            terrainNode.SetAttribute(Schema.terrainGobType.cellSizeAttribute, cellSize);
            terrainNode.SetAttribute(Schema.terrainGobType.heightMapAttribute, ur);
            terrainNode.InitializeExtensions();
            TerrainGob terrain = terrainNode.As<TerrainGob>();
            terrain.Name = name;
            return terrain;
        }
        protected override void OnNodeSet()
        {            
            base.OnNodeSet();
            var xformable = this.Cast<ITransformable>();
            xformable.TransformationType = TransformationTypes.Translation;  
          
            INativeObject nobj = this.As<INativeObject>();
            if (nobj == null)
                throw new InvalidOperationException(this.GetType().FullName + " is not native object");
        }
              
        public IList<LayerMap> LayerMaps
        {
            get { return GetChildList<LayerMap>(Schema.terrainGobType.layerMapChild); }
        }

        public IList<DecorationMap> DecorationMaps
        {
            get { return GetChildList<DecorationMap>(Schema.terrainGobType.decorationMapChild); }
        }

        public string Name
        {
            get { return this.As<INameable>().Name; }
            set { this.As<INameable>().Name = value; }
        }

        public float CellSize
        {
            get { return GetAttribute<float>(Schema.terrainGobType.cellSizeAttribute); }
            set
            {   float cz = value;
                if( cz < 0.1f) cz = 0.1f;
                SetAttribute(Schema.terrainGobType.cellSizeAttribute, cz);
            }
        }

        public int HeightMapHeight
        {
            get
            {
                var hm = GetSurface();
                return hm != null ? hm.Height : 0;
            }
        }

        public int HeightMapWidth
        {
            get
            {
                var hm = GetSurface();
                return hm != null ? hm.Width : 0;
            }
        }

        public Uri HeightMapUri
        {
            get { return GetAttribute<Uri>(Schema.terrainGobType.heightMapAttribute); }
        }

        public bool RayPick(Ray3F rayw, out RayPickRetVal retval)
        {
            INativeObject nobj = this.As<INativeObject>();
            IntPtr retvalPtr = IntPtr.Zero;
            IntPtr rayptr = new IntPtr(&rayw);
            nobj.InvokeFunction("RayPick", rayptr, out retvalPtr);
            if (retvalPtr != IntPtr.Zero)
                retval = *(RayPickRetVal*)retvalPtr;
            else
                retval = new RayPickRetVal();

            return retval.picked;
        }
      
        public void DrawBrush(TerrainBrush brush, Vec2F drawscale, Vec3F posW)
        {
            INativeObject nobj = this.As<INativeObject>();
            DrawBrushArgs arg;
            arg.falloff = brush.Falloff;
            arg.radius = (float)brush.Radius;
            arg.posW = posW;
            arg.drawscale = drawscale;
            IntPtr argPtr = new IntPtr(&arg);
            IntPtr retval;
            nobj.InvokeFunction("DrawBrush", argPtr, out retval);                            
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DrawBrushArgs
        {
            public Vec3F posW;
            public float radius;
            public float falloff;
            public Vec2F drawscale;
            
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ApplyBrushArgs
        {
            public int brushOp;
            public ulong instanceId;
            public Vec3F posW;
        };


        /// <summary>
        /// Holds return value of ray pick method.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RayPickRetVal
        {
            public bool picked;
            public bool pad1;
            public bool pad2;
            public bool pad3;
            public Vec3F hitpos;
            public Vec3F normal;
            public Vec3F nearestVert;
        };

        public override object GetAdapter(Type type)
        {
            object adapter = base.GetAdapter(type);
            if (adapter == null && type == typeof(ITransactionContext))
            {
                IGameDocumentRegistry docreg = Globals.MEFContainer.GetExportedValue<IGameDocumentRegistry>();
                adapter = docreg.MasterDocument.As<ITransactionContext>();                
            }
            return adapter;
        }

        
        #region IPropertyEditingContext Members

        IEnumerable<object> IPropertyEditingContext.Items
        {
            get { yield return this; }
        }

        private PropertyDescriptor[] m_propertyDescriptor;
        IEnumerable<PropertyDescriptor> IPropertyEditingContext.PropertyDescriptors
        {
            get
            {                                            
                if(m_propertyDescriptor == null)
                {
                    FloatArrayConverter converter = new FloatArrayConverter();
                    NumericTupleEditor tupleEditor = new NumericTupleEditor(typeof(float), new string[] { "x", "y", "z" });

                    string category = "General";
                    m_propertyDescriptor = new PropertyDescriptor[]
                    {                                               
                       new UnboundPropertyDescriptor(this.GetType(),"HeightMapUri","HeightMap",category,"Height map"),                       
                       new UnboundPropertyDescriptor(this.GetType(),"HeightMapHeight","HeightMap Height",category,"HeightMap Height"),
                       new UnboundPropertyDescriptor(this.GetType(),"HeightMapWidth","HeightMap Width",category,"HeightMapWidth"),
                       new UnboundPropertyDescriptor(this.GetType(),"CellSize","CellSize",category,"Distance between two vertices"),                                           
                    };
                }
                 
                foreach (var prop in m_propertyDescriptor)
                    yield return prop;
            }
        }
        #endregion

        #region IEditableResourceOwner Members

        private bool m_mapDirty;
        public bool Dirty
        {
            get { return m_mapDirty; }
            private set
            {
                if (m_mapDirty != value)
                {
                    m_mapDirty = value;
                    var doc = DomNode.GetRoot().As<IGameDocument>();
                    doc.NotifyEditableResourceOwnerDirtyChanged(this);
                }
            }
        }

        public void Save()
        {
            if (Dirty)
            {
                ImageData hmImg = GetSurface();
                hmImg.Save(HeightMapUri);
                Dirty = false;
            }            
        }

        #endregion

        #region ITerrainSurface Members

        public ulong GetSurfaceInstanceId()
        {
            INativeObject nobj = this.As<INativeObject>();
            IntPtr retVal = IntPtr.Zero;
            nobj.InvokeFunction("GetHeightMapInstanceId", IntPtr.Zero, out retVal);
            if (retVal == IntPtr.Zero) return 0;
            return (*(ulong*)retVal.ToPointer());
        }

        public ImageData GetSurface()
        {
            ulong surfaceId = GetSurfaceInstanceId();
            return surfaceId != 0 ? new ImageData(surfaceId) : null;
        }

        public Point WorldToSurfaceSpace(Vec3F posW)
        {
            // convert posW from world space to heightmap space.
            Matrix4F xform = TransformUtils.ComputeWorldTransform(this.As<ITransformable>());
            Vec3F posH = posW - xform.Translation;
            return new Point((int)Math.Round(posH.X / CellSize), (int)Math.Round(posH.Z / CellSize));

        }

        public void ApplyDirtyRegion(Bound2di box)
        {
            INativeObject nobj = this.As<INativeObject>();
            IntPtr argPtr = new IntPtr(&box);
            IntPtr retVal = IntPtr.Zero;
            nobj.InvokeFunction("ApplyDirtyRegion", argPtr, out retVal);
            Dirty = true;            
        }

        #endregion
    }
}
