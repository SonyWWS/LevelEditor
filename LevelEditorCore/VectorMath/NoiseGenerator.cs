//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace LevelEditorCore.VectorMath
{
    public class NoiseGenerator
    {
        public NoiseGenerator(int seed)
        {
            Persistence = 0.5f;
            NumberOfOctaves = 5;
            NumFeatures = 10;
            Random rand = new Random(seed);
            for (int i = 0; i < TableSize; i++)
            {
                float r = (float)rand.NextDouble() * 2.0f - 1.0f;
                m_randTable[i] = r;
                m_perm[i] = i;
            }

            for (int i = 0; i < TableSize; i++)
            {
                int swapIndex = rand.Next(TableSize);
                int temp = m_perm[swapIndex];
                m_perm[swapIndex] = m_perm[i];
                m_perm[i] = temp;
                m_perm[i + TableSize] = m_perm[i];
            }
        }

        private float m_numFeatures = 4;

        /// <summary>
        /// Gets and sets number of features</summary>        
        public float NumFeatures
        {
            get { return m_numFeatures; }
            set
            {
                m_numFeatures = value;
                if (m_numFeatures < 1.0f) m_numFeatures = 1.0f;
            }
        }
        private float m_persistence = 0.5f;

        /// <summary>
        /// Gets and sets base value used for computing
        /// amplitude at each octave.
        /// amplitude = Persistence^i where i [0  NumberOfOctaves-1]
        /// </summary>
        public float Persistence
        {
            get { return m_persistence; }
            set
            {
                m_persistence = value;
                if (m_persistence < 0.1f) m_persistence = 0.1f;
            }
        }

        private int m_numberOfOctaves = 4;

        /// <summary>
        /// Gets and sets number of noise functions
        /// that will be applied to generate final ouput.
        /// Each octave have the twice frequency of the previous one</summary>
        public int NumberOfOctaves
        {
            get { return m_numberOfOctaves; }
            set
            {
                m_numberOfOctaves = value;
                if (m_numberOfOctaves < 1)
                    m_numberOfOctaves = 1;
                else if (m_numberOfOctaves > 16)
                    m_numberOfOctaves = 16;
            }
        }

        #region Noise functions
        public float Noise(int x)
        {
            x = (x << 13) ^ x;
            return (1.0f - ((x * (x * x * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f);
        }

        public float Noise(int x, int y)
        {
            x = x % TableSize;
            y = y % TableSize;
            int index = m_perm[m_perm[x] + y];
            return m_randTable[index];
            // int n = x + y * 57;
            // n = (n<<13) ^ n;
            // return ( 1.0f - ( (n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f); 
        }
        public float SmoothNoise(int x)
        {
            return Noise(x - 1) / 4.0f + Noise(x) / 2.0f + Noise(x + 1) / 4.0f;
        }

        public float SmoothNoise(int x, int y)
        {
            float r1 = (Noise(x - 1, y - 1) + Noise(x + 1, y - 1) + Noise(x - 1, y + 1) + Noise(x + 1, y + 1)) / 16.0f;
            float r2 = (Noise(x - 1, y) + Noise(x, y - 1) + Noise(x + 1, y) + Noise(x, y + 1)) / 8.0f;
            return r1 + r2 + Noise(x, y) / 4.0f;
        }

        public float InterpolatedNoise(float x)
        {
            int intX = (int)x;
            float fracX = x - intX;
            float v1 = SmoothNoise(intX);
            float v2 = SmoothNoise(intX + 1);
            return CosineInterpolate(v1, v2, fracX);
        }



        public float InterpolatedNoise(float x, float y)
        {
            int ix = (int)x;
            float fx = x - ix;
            int iy = (int)y;
            float fy = y - iy;

            int rx0 = ix;
            int rx1 = (rx0 + 1);

            int ry0 = iy;
            int ry1 = (ry0 + 1);

            float v1 = Noise(rx0, ry0);
            float v2 = Noise(rx1, ry0);
            float v3 = Noise(rx0, ry1);
            float v4 = Noise(rx1, ry1);

            float i1 = SmoothStep(v1, v2, fx);
            float i2 = SmoothStep(v3, v4, fx);
            return SmoothStep(i1, i2, fy);
        }

        #endregion

        #region interpolations

        public float SmoothCurve(float x)
        {
            return x * x * (3 - 2 * x);
        }

        public float SmoothStep(float a, float b, float t)
        {
            float ft = t * t * (3 - 2 * t);
            return a + ft * (b - a);
        }
        float LinearInterpolate(float a, float b, float t)
        {
            return a + t * (b - a);
        }

        public float CosineInterpolate(float a, float b, float t)
        {
            float ft = t * 3.1415927f;
            float f = (1.0f - (float)Math.Cos(ft)) * 0.5f;
            return a + f * (b - a);
        }

        public float CubicInterpolate(float v0, float v1, float v2, float v3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            float P = (v3 - v2) - (v0 - v1);
            float Q = (v0 - v1) - P;
            float R = v2 - v0;
            float S = v1;
            return P * t3 + Q * t2 + R * t + S;
        }


        #endregion

        public float GetMaxAmplitude()
        {
            float maxamp = 0;
            for (int i = 0; i < NumberOfOctaves; i++)
            {
                maxamp = maxamp + (float)Math.Pow(Persistence, i);
            }
            return maxamp;
        }
        public float ComputeNoise(float x, float y)
        {
            float total = 0;
            float p = Persistence;
            int n = NumberOfOctaves;
            for (int i = 0; i < n; i++)
            {
                int frequency = (int)Math.Pow(2, i);
                float amplitude = (float)Math.Pow(p, i);

                float nz = InterpolatedNoise(x * frequency * NumFeatures, y * frequency * NumFeatures);
                total = total + nz * amplitude;
            }
            return total;
        }

        private const int TableSize = 256;
        private float[] m_randTable = new float[TableSize];
        private int[] m_perm = new int[2 * TableSize];
    }
}
