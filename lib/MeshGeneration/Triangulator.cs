// Unity Community
// http://wiki.unity3d.com/index.php?title=Triangulator

using UnityEngine;
using System.Collections.Generic;

namespace nv
{

    public static class Triangulator
    {
        public static int[] Triangulate(List<Vector3> points)
        {
            List<int> indices = new List<int>();

            int n = points.Count;
            if(n < 3)
                return indices.ToArray();

            int[] V = new int[n];
            if(Area(points) > 0)
            {
                for(int v = 0; v < n; v++)
                    V[v] = v;
            }
            else
            {
                for(int v = 0; v < n; v++)
                    V[v] = (n - 1) - v;
            }

            int nv = n;
            int count = 2 * nv;
            for(int m = 0, v = nv - 1; nv > 2;)
            {
                if((count--) <= 0)
                    return indices.ToArray();

                int u = v;
                if(nv <= u)
                    u = 0;
                v = u + 1;
                if(nv <= v)
                    v = 0;
                int w = v + 1;
                if(nv <= w)
                    w = 0;

                if(Snip(points, u, v, w, nv, V))
                {
                    int a, b, c, s, t;
                    a = V[u];
                    b = V[v];
                    c = V[w];
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);
                    m++;
                    for(s = v, t = v + 1; t < nv; s++, t++)
                        V[s] = V[t];
                    nv--;
                    count = 2 * nv;
                }
            }

            indices.Reverse();
            return indices.ToArray();
        }

        private static float Area(List<Vector3> points)
        {
            int n = points.Count;
            float A = 0.0f;
            for(int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector3 pval = points[p];
                Vector3 qval = points[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }
            return (A * 0.5f);
        }

        private static bool Snip(List<Vector3> points, int u, int v, int w, int n, int[] V)
        {
            int p;
            Vector3 A = points[V[u]];
            Vector3 B = points[V[v]];
            Vector3 C = points[V[w]];
            if(Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
                return false;
            for(p = 0; p < n; p++)
            {
                if((p == u) || (p == v) || (p == w))
                    continue;
                Vector3 P = points[V[p]];
                if(InsideTriangle(A, B, C, P))
                    return false;
            }
            return true;
        }

        private static bool InsideTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
        {
            float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
            float cCROSSap, bCROSScp, aCROSSbp;

            ax = C.x - B.x; ay = C.y - B.y;
            bx = A.x - C.x; by = A.y - C.y;
            cx = B.x - A.x; cy = B.y - A.y;
            apx = P.x - A.x; apy = P.y - A.y;
            bpx = P.x - B.x; bpy = P.y - B.y;
            cpx = P.x - C.x; cpy = P.y - C.y;

            aCROSSbp = ax * bpy - ay * bpx;
            cCROSSap = cx * apy - cy * apx;
            bCROSScp = bx * cpy - by * cpx;

            return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
        }
    }
}