using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv
{
    public static class BoundsExtensions
    {
        public static Rect GetRect( this Bounds bounds )
        {
            float x = bounds.size.x;
            float y = bounds.size.y;
            return new Rect( 0f, 0f, x, y )
            {
                center = bounds.center
            };
        }
    }
}