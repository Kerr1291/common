using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace nv
{
    public static class RectExtensions
    {
        public static Rect SetMinMax(this Rect input, Vector2 min, Vector2 max)
        {
            Vector2 rMin = Mathnv.Min(min, max);
            Vector2 rMax = Mathnv.Max(min, max);

            Rect r = new Rect((rMax - rMin) * .5f + rMin, rMax - rMin);
            return r;
        }

        public static Vector2 TopLeft(this Rect input, bool flipYAxis = true)
        {
            return new Vector2(input.xMin, flipYAxis ? input.yMin : input.yMax);
        }

        public static Vector2 TopRight(this Rect input, bool flipYAxis = true)
        {
            return new Vector2(input.xMax, flipYAxis ? input.yMin : input.yMax);
        }

        public static Vector2 BottomRight(this Rect input, bool flipYAxis = true)
        {
            return new Vector2(input.xMax, flipYAxis ? input.yMax : input.yMin);
        }

        public static Vector2 BottomLeft(this Rect input, bool flipYAxis = true)
        {
            return new Vector2(input.xMin, flipYAxis ? input.yMax : input.yMin);
        }

        public static void Clamp(this Rect area, Vector2 pos, Vector2 size)
        {
            Mathnv.Clamp(ref area, pos, size);
        }

        public static void Clamp(this Rect area, Rect min_max)
        {
            Mathnv.Clamp(ref area, min_max);
        }

        public static Range GetXRange(this Rect r)
        {
            return new Range(r.xMin, r.xMax);
        }

        public static Range GetYRange(this Rect r)
        {
            return new Range(r.yMin, r.yMax);
        }

        public static Vector2 GetRandomValue(this Rect r)
        {
            return new Vector2(r.GetXRange().RandomValuef(), r.GetYRange().RandomValuef());
        }

        public static Vector2Int GetRandomValueInt(this Rect r)
        {
            return new Vector2Int(r.GetXRange().RandomValuei(), r.GetYRange().RandomValuei());
        }

        public static Vector2 GetRandomValue(this Rect r, RNG rng)
        {
            return new Vector2(r.GetXRange().RandomValuef(rng), r.GetYRange().RandomValuef(rng));
        }

        public static Vector2Int GetRandomValueInt(this Rect r, RNG rng)
        {
            return new Vector2Int(r.GetXRange().RandomValuei(rng), r.GetYRange().RandomValuei(rng));
        }
    }
}
