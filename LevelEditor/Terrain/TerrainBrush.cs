//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using System.ComponentModel;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

using LevelEditorCore.VectorMath;
using RenderingInterop;

namespace LevelEditor.Terrain
{
   
    /// <summary>
    /// Base class for all Terrain brushes.</summary>
    public abstract class TerrainBrush : IPropertyEditingContext 
    {
        /// <summary>
        /// Consruct brush with the given name.</summary>
        public TerrainBrush(string name)
        {            
            m_radius = 3;
            m_falloff = 0.5f;
            CreateKernel();
            Name = name;
        }

        #region IPropertyEditingContext Members

        IEnumerable<object> IPropertyEditingContext.Items
        {
            get { yield return this; }
        }

        IEnumerable<PropertyDescriptor> IPropertyEditingContext.PropertyDescriptors
        {
            get
            {
                foreach (var prop in PropertyDescriptors)
                    yield return prop;
            }
        }

        #endregion

        /// <summary>
        /// Gets brush name.</summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether this brush can be applied to target. </summary>        
        public abstract bool CanApplyTo(ITerrainSurface target);

        /// <summary>
        /// Apply this brush to target at the given coordinate
        /// output TerrainOp for undo/redo
        /// </summary>        
        public abstract void Apply(ITerrainSurface target, int x, int y, out TerrainOp op);       


        /// <summary>
        /// Computes brush bound in imgdata space.</summary>        
        protected void ComputeBound(ImageData imgData,
                    int x,
                    int y,                    
                    out Bound2di outRect)
        {
            outRect.x1 = 0;
            outRect.x2 = 0;
            outRect.y1 = 0;
            outRect.y2 = 0;

            bool valid = imgData != null && imgData.IsValid;
            System.Diagnostics.Debug.Assert(valid);
            if (!valid) return;


            Bound2di kernelRect;

            kernelRect.x1 = x - Radius;
            kernelRect.y1 = y - Radius;
            kernelRect.x2 = x + Radius + 1;
            kernelRect.y2 = y + Radius + 1;

            Bound2di destRect;
            destRect.x1 = 0;
            destRect.y1 = 0;
            destRect.x2 = imgData.Width;
            destRect.y2 = imgData.Height;
            Bound2di.Intersect(kernelRect, destRect, out outRect);
        }         

                
        /// <summary>
        /// Gets/Sets brush size.
        /// </summary>
        public int Radius
        {
            get  { return m_radius; }
            set  
            {
                m_radius = value;
                if (m_radius < 1) m_radius = 1;
                CreateKernel();                
            }
        }
        
        /// <summary>
        /// Gets/Sets brush falloff.</summary>
        public float Falloff
        {
            get { return m_falloff; }
            set
            {
               m_falloff = Sce.Atf.MathUtil.Clamp<float>(value,0.0f,1.0f);
               CreateKernel();               
            }
        }

      
        /// <summary>
        /// Event that is raised when brush kernel is changed</summary>
        protected event EventHandler KernelMaskChanged = delegate { };

        /// <summary>
        /// Creates brush kernel</summary>
        private void CreateKernel()
        {            
            float falloff = Falloff;
            float rad2 = (float)Radius;
            float rad1 = rad2 * (1 - falloff);
            float xc = Radius;
            float yc = Radius;

            // use linear falloff
            float range = rad2 - rad1;
            int size = 2 * Radius + 1;

            Kernel = new float[size * size];
            int k = 0;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float vx = x - xc;
                    float vy = y - yc;
                    float dist = (float)Math.Sqrt(vx * vx + vy * vy);
                    float  val;
                    if (dist >= rad2)
                    {
                        val = 0;
                    }
                    else if (dist < rad1)
                    {
                        val = 1.0f;
                    }
                    else
                    {
                        val = (rad2 - dist) / range; 
                        System.Diagnostics.Debug.Assert( val >= 0.0f && val <= 1.0f);
                        val = Sce.Atf.MathUtil.Clamp<float>(val,0.0f,1.0f);                                                    
                    }
                    Kernel[k++] = val;                    
                }
            }
            KernelMaskChanged(this, EventArgs.Empty);


        }
        protected virtual PropertyDescriptor[] PropertyDescriptors
        {
            get
            {                
                if (m_propertyDescriptor == null)
                {
                    string category = "General";
                    BoundedFloatEditor editor = new BoundedFloatEditor();
                    //BoundedFloatConverter
                    editor.Min = 0.0f;
                    editor.Max = 1.0f;
                    m_propertyDescriptor = new PropertyDescriptor[]
                    {                        
                       new UnboundPropertyDescriptor(this.GetType(),"Radius","Brush radius",category,"Brush radius"),
                       new UnboundPropertyDescriptor(this.GetType(),"Falloff","Brush Softness",category,"Brush Softness",editor, null)
                    };
                }
                return m_propertyDescriptor;
            }
        }
        
        private PropertyDescriptor[] m_propertyDescriptor;
        private float m_falloff;
        private int m_radius;
        protected float[] Kernel;
    }

    /// <summary>
    /// Paint/Erase layer and decoration map</summary>
    unsafe public class PaintEraseBrush : TerrainBrush
    {
        public PaintEraseBrush(string name)
            : base(name)
        {
            Falloff = 0.75f;
            m_strength = 1.0f;
        }

        private float m_strength;
        /// <summary>
        /// Gets and sets brush strength.</summary>
        public float Strength
        {
            get { return m_strength; }
            set { m_strength = MathUtil.Clamp(value, 0.0f, 1.0f); }
            
        }
        public override bool CanApplyTo(ITerrainSurface target)
        {
            TerrainMap map = target.As<TerrainMap>();
            return map != null && map.GetSurfaceInstanceId() != 0;            
        }

        public BrushOps GetBrushOp()
        {
            return (Control.ModifierKeys == Keys.Control) ? BrushOps.Erase : BrushOps.Paint;
        }


        public override void Apply(ITerrainSurface target, int x, int y, out TerrainOp op)
        {
            op = null;
            if (!CanApplyTo(target)) return;

            Bound2di outRect;
            TerrainMap tmap = target.As<TerrainMap>();
            ImageData mask = tmap.GetSurface();
            ComputeBound(mask, x, y, out outRect);
            bool validArgs = outRect.isValid && mask.Format == ImageDataFORMAT.R8_UNORM;
            System.Diagnostics.Debug.Assert(validArgs);
            if (validArgs == false) return;

            op = new TerrainOp(target, outRect);
            var brushOp = GetBrushOp();

            float minheight = tmap.MinHeight;
            float maxheight = tmap.MaxHeight;
            float minslope = tmap.MinSlope;
            float maxslope = tmap.MaxSlope;
            ImageData hmImg = tmap.Parent.GetSurface();

            float dx = (float)hmImg.Width / (float)mask.Width;
            float dy = (float)hmImg.Height / (float)mask.Height;

            int pixelstrength = (int)Math.Round(255.0f * Strength);
            Func<int, int, byte> ops = null;
            if (brushOp == BrushOps.Paint)
            {
                ops = (px, py) =>
                    {                       
                        if (px >= pixelstrength)
                            return (byte)px;
                        else
                            return (byte)Math.Min(pixelstrength, px + py);
                    };
            }
            else
            {
                ops = (px, py) => (byte)MathUtil.Clamp<int>(px - py, 0, 255);
            }


            // start point in kernel space.
            int bx0 = x - Radius;
            int by0 = y - Radius;
            
            float run = 2.0f * tmap.Parent.CellSize;                
            int size = 2 * Radius + 1;
            for (int cy = outRect.y1; cy < outRect.y2; cy++)
            {
                int by = cy - by0;

                for (int cx = outRect.x1; cx < outRect.x2; cx++)
                {
                    int bx = cx - bx0;

                    // test for height and slope.
                    // xform px and py to heightmap space hx, hy.                        
                    if (brushOp == BrushOps.Paint)
                    {
                        int hx = (int)Math.Round(cx * dx);
                        int hy = (int)Math.Round(cy * dy);
                        float height = hmImg.GetPixelFloat(hx, hy);
                        if (height < minheight || height > maxheight)
                        {
                            continue;
                        }
                        // check slope
                        float slopex = hmImg.GetPixelFloat(hx + 1, hy) - hmImg.GetPixelFloat(hx - 1, hy);
                        float slopeY = hmImg.GetPixelFloat(hx, hy + 1) - hmImg.GetPixelFloat(hx, hy - 1);

                        float slope = Math.Max(Math.Abs(slopex), Math.Abs(slopeY)) / run;
                        float deg = MathHelper.ToDegree((float)Math.Atan(slope));
                        if (deg < minslope || deg > maxslope)
                        {
                            continue;
                        }
                    }

                    float scrPixel = Kernel[size * by + bx];
                    byte* destPixel = mask.GetPixel(cx, cy);
                    *destPixel = ops(*destPixel, (int)Math.Round(scrPixel * 255.0f));
                }
            }

            tmap.ApplyDirtyRegion(outRect);
        }


        private PropertyDescriptor[] m_propertyDescriptor;
        protected override PropertyDescriptor[] PropertyDescriptors
        {
            get
            {
                if (m_propertyDescriptor == null)
                {
                    string category = "General";
                    BoundedFloatEditor editor = new BoundedFloatEditor();                    
                    editor.Min = 0.0f;
                    editor.Max = 1.0f;
                    
                    var propList = new List<PropertyDescriptor>(base.PropertyDescriptors);
                    propList.Add(new UnboundPropertyDescriptor(this.GetType(), "Strength", "Strength", category, "Brush Strength", editor));
                    m_propertyDescriptor = propList.ToArray();

                }
                return m_propertyDescriptor;
            }
        }
    }

    unsafe public class SmoothenBrush : TerrainBrush
    {
        public SmoothenBrush(string name)
            : base(name)
        {
            Falloff = 0.0f;
        }

        public override bool CanApplyTo(ITerrainSurface target)
        {
            return target.Is<TerrainGob>();
                
        }

        private List<float> m_templist = new List<float>();

        public override void Apply(ITerrainSurface target, int x, int y, out TerrainOp op)
        {
            //todo: optimze this function.

            op = null;
            if (!CanApplyTo(target)) return;
            TerrainGob terrain = target.As<TerrainGob>();
            ImageData hmImg = terrain.GetSurface();
            Bound2di outRect;
            ComputeBound(hmImg, x, y, out outRect);
            if (!outRect.isValid || hmImg.Format != ImageDataFORMAT.R32_FLOAT)
                return;
            op = new TerrainOp(target, outRect);
            

            Func<int,int,float> getPixel = (px, py) =>
            {
                px = MathUtil.Clamp(px, 0, hmImg.Width - 1);
                py = MathUtil.Clamp(py, 0, hmImg.Height - 1);
                float pixel = *(float*)hmImg.GetPixel(px, py);
                return pixel;                
            };

            Func<int, int, float> getSmoothPixel = (px, py) =>
            {
                float r1 = getPixel(px - 1, py - 1) + 2.0f * getPixel(px, py - 1) + getPixel(px + 1, py - 1);
                float r2 = 2.0f * getPixel(px - 1, py) + 4.0f * getPixel(px, py) + 2.0f * getPixel(px + 1, py);
                float r3 = getPixel(px - 1, py + 1) + 2.0f * getPixel(px, py + 1) + getPixel(px + 1, py + 1);
                return ((r1 + r2 + r3) / 16.0f);
            };

            m_templist.Clear();

            for (int cy = outRect.y1; cy < outRect.y2; cy++)
            {
                for (int cx = outRect.x1; cx < outRect.x2; cx++)
                {
                    m_templist.Add(getSmoothPixel(cx, cy));                    
                }
            }

            // start point in kernel space.
            int bx0 = x - Radius;
            int by0 = y - Radius;

            int k = 0;
            int size = 2 * Radius + 1;
            for (int cy = outRect.y1; cy < outRect.y2; cy++)
            {
                int by = cy - by0;
                for (int cx = outRect.x1; cx < outRect.x2; cx++)
                {
                    int bx = cx - bx0;

                    float scrPixel = m_templist[k++];
                    float lerp = Kernel[size * by + bx];
                    float* destPixel = (float*)hmImg.GetPixel(cx, cy);
                    *destPixel = Sce.Atf.MathUtil.Interp(lerp, *destPixel, scrPixel);
                }
            }
            terrain.ApplyDirtyRegion(outRect);
        }        
    }

    unsafe public class FlattenBrush : TerrainBrush
    {
        public FlattenBrush(string name)
            : base(name)
        {
            Falloff = 0.2f;
            
        }

        /// <summary>
        /// Gets and sets the height value to flatten the terrain</summary>
        public float Height
        {
            get;
            set;
        }

        public override bool CanApplyTo(ITerrainSurface target)
        {
            return (target is TerrainGob);
        }

        public override void Apply(ITerrainSurface target, int x, int y, out TerrainOp op)
        {
            op = null;
            if (!CanApplyTo(target)) return;
            TerrainGob terrain = target.As<TerrainGob>();
            ImageData hmImg = terrain.GetSurface();
            Bound2di outRect;
            ComputeBound(hmImg, x, y, out outRect);            
            if (!outRect.isValid || hmImg.Format != ImageDataFORMAT.R32_FLOAT)
                return;

            op = new TerrainOp(target, outRect);
            // start point in kernel space.
            int bx0 = x - Radius;
            int by0 = y - Radius;

            int size = 2 * Radius + 1;            
            for (int cy = outRect.y1; cy < outRect.y2; cy++)
            {
                int by = cy - by0;
                for (int cx = outRect.x1; cx < outRect.x2; cx++)
                {
                    int bx = cx - bx0;
                    float scrPixel = Kernel[size * by + bx];
                    float* destPixel = (float*)hmImg.GetPixel(cx, cy);
                    *destPixel = MathUtil.Interp(scrPixel, *destPixel, Height);
                }
            }

            terrain.ApplyDirtyRegion(outRect);
        }
        
    }
       
    unsafe public class RaiseLowerBrush : TerrainBrush
    {
        /// <summary>
        /// create brush with defult size and falloff.</summary>
        public RaiseLowerBrush(string name)
            : base(name)
        {
            m_height = 0.5f;            
        }

        public override bool CanApplyTo(ITerrainSurface target)
        {
            return (target is TerrainGob);
        }

        public override void Apply(ITerrainSurface target, int x, int y, out TerrainOp op)
        {
            op = null;
            if (!CanApplyTo(target)) return;
            Bound2di outRect;
            TerrainGob terrain = target.As<TerrainGob>();
            ImageData hmImg = terrain.GetSurface();
            ComputeBound(hmImg, x, y, out outRect);            
            if (!outRect.isValid || hmImg.Format != ImageDataFORMAT.R32_FLOAT)
                return;

            op = new TerrainOp(target, outRect);
            var brushOp = GetBrushOp();

            Func<float, float, float> ops = null;
            if (brushOp == BrushOps.Add)
                ops = (a, b) => a + b;
            else if (brushOp == BrushOps.Sub)
                ops = (a, b) => a - b;

            if (ops == null)
                throw new ArgumentException("brushOp");


            // start point in kernel space.
            int bx0 = x - Radius;
            int by0 = y - Radius;

            int size = 2 * Radius + 1;
            for (int cy = outRect.y1; cy < outRect.y2; cy++)
            {
                int by = cy - by0;
                for (int cx = outRect.x1; cx < outRect.x2; cx++)
                {
                    int bx = cx - bx0;
                    float scrPixel = Kernel[size * by + bx] * m_height;
                    float* destPixel = (float*)hmImg.GetPixel(cx, cy);
                    *destPixel = ops(*destPixel, scrPixel);
                }
            }            
            terrain.ApplyDirtyRegion(outRect);
        }
        
        /// <summary>
        /// Gets and sets value used to scale the brush.</summary>
        public float Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        public BrushOps GetBrushOp()
        {
            return (Control.ModifierKeys == Keys.Control) ? BrushOps.Sub : BrushOps.Add;            
        }

        private PropertyDescriptor[] m_propertyDescriptor;
        protected override PropertyDescriptor[] PropertyDescriptors
        {
            get
            {
                if (m_propertyDescriptor == null)
                {
                    var propList = new List<PropertyDescriptor>(base.PropertyDescriptors);                    
                    propList.Add(new UnboundPropertyDescriptor(this.GetType(), "Height", "Height", "General", "Brush Height"));
                    m_propertyDescriptor = propList.ToArray();   
                                                  
                }
                return m_propertyDescriptor;
            }
        }
        
        private float m_height;        
        
    }

    unsafe public class NoiseBrush : TerrainBrush
    {
        public NoiseBrush(string name)
            : base(name)
        {
            Falloff = 0.0f;
            Radius = 16;
            m_scale = 1.5f;
            m_noiseGen = new NoiseGenerator(753);
            m_noiseGen.NumFeatures = 4;
            
            GenerateNoise();

            KernelMaskChanged += (sender, e) =>
                {
                    GenerateNoise();
                };
            
        }

        public override bool CanApplyTo(ITerrainSurface target)
        {
            return (target is TerrainGob);
        }

        public override void Apply(ITerrainSurface target, int x, int y, out TerrainOp op)
        {
            op = null;
            if (!CanApplyTo(target)) return;
            Bound2di outRect;
            TerrainGob terrain = target.As<TerrainGob>();
            ImageData hmImg = terrain.GetSurface();
            ComputeBound(hmImg, x, y, out outRect);
            if (!outRect.isValid || hmImg.Format != ImageDataFORMAT.R32_FLOAT)
                return;

            op = new TerrainOp(target, outRect);

            // start point in kernel space.
            int bx0 = x - Radius;
            int by0 = y - Radius;

            int size = 2 * Radius + 1;
            for (int cy = outRect.y1; cy < outRect.y2; cy++)
            {
                int by = cy - by0;

                for (int cx = outRect.x1; cx < outRect.x2; cx++)
                {
                    int bx = cx - bx0;                    
                    float k = Kernel[size * by + bx] * m_noise[bx, by] * m_scale;                    
                    float* destPixel = (float*)hmImg.GetPixel(cx, cy);
                    *destPixel = *destPixel + k;
                }
            }

            terrain.ApplyDirtyRegion(outRect);
        }


        private PropertyDescriptor[] m_propertyDescriptor;
        protected override PropertyDescriptor[] PropertyDescriptors
        {
            get
            {
                if (m_propertyDescriptor == null)
                {
                    string category = "General";                    
                    BoundedIntEditor octaveEditor = new BoundedIntEditor();
                    octaveEditor.Min = 1;
                    octaveEditor.Max = 10;

                    BoundedFloatEditor Persiteditor = new BoundedFloatEditor();
                    Persiteditor.Min = 0.1f;
                    Persiteditor.Max = 1.0f;
                                        
                    var propList = new List<PropertyDescriptor>(base.PropertyDescriptors);
                    propList.Add(new UnboundPropertyDescriptor(this.GetType(), "NumberOfOctaves", "Octaves", category, "NumberOfOctaves", octaveEditor));
                    propList.Add(new UnboundPropertyDescriptor(this.GetType(), "Persistence", "Persistence", category, "Persistence", Persiteditor));
                    propList.Add(new UnboundPropertyDescriptor(this.GetType(), "NumFeatures", "Features", category, "Number of features"));
                    propList.Add(new UnboundPropertyDescriptor(this.GetType(), "FeatureScale", "FeatureScale", category, "Feature Scale"));
                    
                    m_propertyDescriptor = propList.ToArray();

                }
                return m_propertyDescriptor;
            }
        }

        /// <summary>
        /// Gets and sets number of noise functions
        /// that will be applied to generate final ouput.
        /// Each octave have the twice frequency of the previous one</summary>
        public int NumberOfOctaves
        {
            get { return m_noiseGen.NumberOfOctaves; }
            set 
            { 
                m_noiseGen.NumberOfOctaves = value;
                GenerateNoise();
            }
        }

        /// <summary>
        /// Gets and sets base value used for computing
        /// amplitude at each octave.
        /// amplitude = Persistence^i where i [0  NumberOfOctaves-1]
        /// </summary>
        public float Persistence
        {
            get { return m_noiseGen.Persistence; }
            set 
            { 
                m_noiseGen.Persistence = value;
                GenerateNoise();
            }
        }


        /// <summary>
        /// Gets and sets number of features</summary>        
        public int NumFeatures
        {
            get { return (int)m_noiseGen.NumFeatures;}
            set 
            { 
                m_noiseGen.NumFeatures = value;
                GenerateNoise();
            }
        }

        /// <summary>
        /// Gets and sets value to scale feature.</summary>
        public float FeatureScale
        {
            get { return m_scale; }
            set
            {
                m_scale = value;
                if (m_scale < float.Epsilon) m_scale = float.Epsilon;                                
            }
            
        }

        private void GenerateNoise()
        {
            int size = 2 * Radius + 1;
            m_noise = new float[size, size];
            float dx = 1.0f / size;
            float dy = 1.0f / size;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    m_noise[x,y] = m_noiseGen.ComputeNoise(x * dx, y * dy);
                }
            }
        }

        private NoiseGenerator m_noiseGen;
        private float[,] m_noise;
        private float m_scale;
    }

    public enum BrushOps
    {
        None,
        Add,
        Sub,
        Paint,
        Erase,
    }

    /// <summary>
    /// Terrain operation used for undo/redo</summary>
    public class TerrainOp : Sce.Atf.Dom.TransactionContext.Operation
    {        
        /// <summary>
        /// Construct TerrainOp with the give target and bound</summary>        
        public TerrainOp(ITerrainSurface target, Bound2di bound)
        {
            m_target = target;
            m_bound = bound;
            Perform();
        }

        /// <summary>
        /// Free the data used for undo/redo.</summary>
        public void FreeData()
        {
            m_data = null;
            m_dataFreed = true;
        }
        /// <summary>
        /// Does the transaction operation</summary>
        public override void Do()
        {
            Perform();            
        }

        /// <summary>
        /// Rolls back the transaction operation</summary>
        public override void Undo()
        {
            Perform();            
        }

        private void Perform()
        {
            if (m_dataFreed) return;
            ImageData img = m_target.GetSurface();            
            // toggle between data stored in img and m_data.                
            var data = new byte[m_bound.Width * m_bound.Height * img.BytesPerPixel];            
            
            img.CopyRegion(m_bound, data);            
            if (m_data != null)
            {                
                img.ApplyRegion(m_bound, m_data);                             
                m_target.ApplyDirtyRegion(m_bound);                
            }
            m_data = data;            
        }

        /// <summary>
        /// Gets the size of this operation in bytes.
        /// Used to keep track of memory usage.</summary>
        public int SizeInBytes
        {
            get { return m_data == null ? 0 : m_data.Length; }
        }

        private ITerrainSurface m_target;
        private byte[] m_data;
        private Bound2di m_bound; // in map space.
        private bool m_dataFreed;
    }
}
