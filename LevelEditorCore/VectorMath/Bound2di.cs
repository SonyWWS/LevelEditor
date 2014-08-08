//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Runtime.InteropServices;

namespace LevelEditorCore.VectorMath
{
    /// <summary>
    /// Represents a 2d region or bound with integer coordinate.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Bound2di
    {
        public int x1;
        public int y1;
        public int x2;
        public int y2;

        /// <summary>
        /// Returns true if this a valid bound.</summary>
        public bool isValid
        {
            get { return ((x2 - x1) > 0 && (y2 - y1) > 0); }
        }

        /// <summary>
        /// Gets width</summary>
        public int Width
        {
            get { return x2 - x1; }
        }

        /// <summary>
        /// Gets Height</summary>
        public int Height
        {
            get { return y2 - y1; }
        }
        public static bool Intersect(Bound2di r1, Bound2di r2, out Bound2di rout)
        {
            // early reject.
            if (r1.x2 <= r2.x1
                || r1.x1 >= r2.x2
                || r1.y1 >= r2.y2
                || r1.y2 <= r2.y1)
            {
                rout.x1 = 0;
                rout.x2 = 0;
                rout.y1 = 0;
                rout.y2 = 0;
                return false;
            }

            // find intersection rect.
            rout.x1 = Math.Max(r1.x1, r2.x1);
            rout.x2 = Math.Min(r1.x2, r2.x2);
            rout.y1 = Math.Max(r1.y1, r2.y1);
            rout.y2 = Math.Min(r1.y2, r2.y2);
            return true;
        }
    }
}
