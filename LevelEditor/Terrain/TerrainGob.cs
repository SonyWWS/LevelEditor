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
    public class TerrainGob : DomNodeAdapter, IPropertyEditingContext, IEditableResourceOwner
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
                var hm = GetHeightMap();
                return hm != null ? hm.Height : 0;
            }
        }

        public int HeightMapWidth
        {
            get 
            {
                var hm = GetHeightMap();
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
            unsafe
            {
                IntPtr retvalPtr = IntPtr.Zero;
                IntPtr rayptr = new IntPtr(&rayw);
                nobj.InvokeFunction("RayPick", rayptr, out retvalPtr);
                if (retvalPtr != IntPtr.Zero)
                    retval = *(RayPickRetVal*)retvalPtr;
                else
                    retval = new RayPickRetVal();
            }
            return retval.picked;
        }
        public ImageData GetHeightMap()
        {

            unsafe
            {
                INativeObject nobj = this.As<INativeObject>();
                IntPtr retVal = IntPtr.Zero;
                nobj.InvokeFunction("GetHeightMapInstanceId", IntPtr.Zero, out retVal);
                if (retVal == IntPtr.Zero) return null;
                ulong hmInstId = *(ulong*)retVal.ToPointer();
                return hmInstId != 0 ? new ImageData(hmInstId) : null;
            }
        }
        /// <summary>
        ///  Transform the give position in world space 
        ///  to position in heighmap space.</summary>        
        public Point WorldToHmapSpace(Vec3F posW)
        {
            // convert posW from world space to heightmap space.                
            Matrix4F xform = TransformUtils.ComputeWorldTransform(this.As<ITransformable>());
            Vec3F posH = posW - xform.Translation;
            return new Point((int)Math.Round(posH.X / CellSize),(int)Math.Round(posH.Z / CellSize));            
        }


        public void ApplyDirtyRegion(Bound2di box)
        {
            unsafe
            {
                INativeObject nobj = this.As<INativeObject>();
                IntPtr argPtr = new IntPtr(&box);
                IntPtr retVal = IntPtr.Zero;
                nobj.InvokeFunction("ApplyDirtyRegion", argPtr, out retVal);
                Dirty = true;
            }


            // 
            // undo/redo
            // - have a global list of brush-strokes.
            //   each brush stroke defines brush properties and position and brush ops.
            // - in OnEndDrag of TerrainManipulator DoTransaction() and push one instance 
            //   off class ApplyBrushOperation : Operation.  
            //   The ApplyBrushOperation hold the index of start and end index of the 
            //   BrushStroke list. 
            // - when creating terrain object and map object save a copy of heightmap and mask.
            // - when undoing re-apply all the operation from the zero upto start 
            // - when doing re-apply all the operation from zero to end index 
            //IGameDocumentRegistry docreg = Globals.MEFContainer.GetExportedValue<IGameDocumentRegistry>();
            //EditingContext edit = docreg.MasterDocument.As<EditingContext>();
            //edit.DoTransaction(
            //    delegate
            //    {
            //        edit.AddOperation(new BrushAppliedOperation());
            //    }, "Apply terrain brush");

        }

       

        public void DrawBrush(TerrainBrush brush, Vec2F drawscale, Vec3F posW)
        {
            INativeObject nobj = this.As<INativeObject>();
            unsafe
            {
                DrawBrushArgs arg;
                arg.falloff = brush.Falloff;
                arg.radius = (float)brush.Radius;
                arg.posW = posW;
                arg.drawscale = drawscale;
                IntPtr argPtr = new IntPtr(&arg);
                IntPtr retval;
                nobj.InvokeFunction("DrawBrush", argPtr, out retval);                
            }
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

        public bool Dirty
        {
            get;
            private set;
        }

        public void Save()
        {
            if (Dirty)
            {
                ImageData hmImg = GetHeightMap();
                hmImg.Save(HeightMapUri);
                Dirty = false;
            }            
        }

        #endregion

    }

}
