using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GridSystem.Geometry
{
    public struct BoundingBox
    {
        public Vector2 Min;
        public Vector2 Max;

        public static readonly BoundingBox Infinity = new BoundingBox(new Vector2(float.PositiveInfinity), new Vector2(float.NegativeInfinity));

        public BoundingBox(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }


        /// <summary>
        /// Increase a measurement by an additional measurement.
        /// </summary>
        public static BoundingBox operator +(BoundingBox a, BoundingBox b)
        {
            return new BoundingBox
            {
                Min = GeometryUtils.Vector2Min(a.Min, b.Min),
                Max = GeometryUtils.Vector2Max(a.Max, b.Max)
            };
        }


        public static BoundingBox operator +(BoundingBox a, Vector2 b)
        {
            return new BoundingBox
            {
                Min = a.Min + b,
                Max = a.Max + b
            };
        }


        public static BoundingBox operator +(Vector2 b, BoundingBox a)
        {
            return new BoundingBox
            {
                Min = a.Min + b,
                Max = a.Max + b
            };
        }


        public static BoundingBox operator -(BoundingBox a, Vector2 b)
        {
            return new BoundingBox
            {
                Min = a.Min - b,
                Max = a.Max - b
            };
        }

        public static BoundingBox operator -(Vector2 b, BoundingBox a)
        {
            return new BoundingBox
            {
                Min = a.Min - b,
                Max = a.Max - b
            };
        }
        public static bool Intersects(BoundingBox b1, BoundingBox b2)
        {
            var lx = (b1.Max.x + b1.Min.x) / 2;
            var tx = (b2.Max.x + b2.Min.x) / 2;
            var dx = ((b1.Max.x - b1.Min.x) + (b2.Max.x - b2.Min.x)) / 2.0f;

            if (Math.Abs(lx - tx) >= dx) return false;

            var ly = (b1.Max.y + b1.Min.y) / 2;
            var ty = (b2.Max.y + b2.Min.y) / 2;
            var dy = ((b1.Max.y - b1.Min.y) + (b2.Max.y - b2.Min.y)) / 2.0f;

            if (Math.Abs(ly - ty) >= dy) return false;

            return true;
        }

        internal float Width => Max.x - Min.x;
        internal float Height => Max.y - Min.y;
    }
}
