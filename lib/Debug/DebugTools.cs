using UnityEngine;
using System.Collections.Generic;

namespace nv
{
    /// <summary>
    /// Collection of tools, debug or otherwise, to improve the quality of life
    /// </summary>
    public class DebugTools
    {
        public static List<GameObject> CreateBoxOfLineRenderers( Bounds b, Color c, float z = 0f, float width = .5f )
        {
            Rect boundsRect = new Rect(b.min, b.size);

            GameObject leftSide = CreateLineRenderer(boundsRect.BottomLeft(),boundsRect.TopLeft(),c,z,width);
            GameObject topSide = CreateLineRenderer(boundsRect.TopLeft(),boundsRect.TopRight(),c,z,width);
            GameObject rightSide = CreateLineRenderer(boundsRect.TopRight(),boundsRect.BottomRight(),c,z,width);
            GameObject bottomSide = CreateLineRenderer(boundsRect.BottomRight(),boundsRect.BottomLeft(),c,z,width);

            List<GameObject> boundryLines = new List<GameObject>()
            {
                leftSide,topSide,rightSide,bottomSide
            };

            return boundryLines;
        }

        public static GameObject CreateLineRenderer( Vector2 from, Vector2 to, Color c, float z = 0f, float width = .5f )
        {
            Dev.Log( "Creating line renderer from " + from + " to " + to );
            List<Vector2> points = new List<Vector2>() { from, to };
            return points.CreateLineRenderer( c, z, width );
        }
    }
}

