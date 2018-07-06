using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Map = nv.ArrayGrid<nv.MapElement>;

namespace nv
{
    public class VoxelGridSurface : MonoBehaviour
    {
        public MeshFilter meshFilter;
        public MeshCollider meshCollider;

        [Header("Heights for corner case vertices")]
        public float innerHeightCorner = 0f;
        public float outerHeightCorner = 0f;

        [Header("Heights for edge case vertices")]
        public float innerHeightEdge = 0f;
        public float outerHeightEdge = 0f;

        [Header("Heights for inner quad vertices")]
        public float innerHeight = 0f;

        public bool smoothMesh = true;

        public bool useUVMap = false;

        public VoxelGrid xNeighbor, yNeighbor, xyNeighbor;

        [SerializeField]
        [HideInInspector]
        VoxelGridWall wall;

        public VoxelGridWall Wall
        {
            get { return wall; }
            private set { wall = value; }
        }

        public int WallIndex(int i)
        {
            return i / 2;
        }

        [SerializeField]
        [HideInInspector]
        Mesh mesh;

        [SerializeField]
        [HideInInspector]
        List<Vector3> vertices;

        [SerializeField]
        [HideInInspector]
        Vector3[] normals;

        [SerializeField]
        [HideInInspector]
        List<int> triangles;

        //Note: potential optimization, make caches static if we're not going to use multitrheading
        [SerializeField]
        [HideInInspector]
        int[] rowCacheMax;

        [SerializeField]
        [HideInInspector]
        int[] rowCacheMin;

        [SerializeField]
        [HideInInspector]
        int edgeCacheMin;

        [SerializeField]
        [HideInInspector]
        int edgeCacheMax;

        public int Resolution
        {
            get;
            private set;
        }


        [SerializeField]
        [HideInInspector]
        Vector2[] simple_uvs;

        public void Init(int resolution, VoxelGridWall wall)
        {
            Resolution = resolution;
            Wall = wall;
            gameObject.GetOrAddComponentIfNull(ref meshFilter);
            gameObject.GetOrAddComponentIfNull(ref meshCollider);
            //Dev.GetOrAddComponentIfNull(ref meshFilter, gameObject);
            //Dev.GetOrAddComponentIfNull(ref meshCollider, gameObject);

            meshFilter.mesh = mesh = new Mesh();
            mesh.name = "VoxelGridSurface Mesh";
            vertices = new List<Vector3>();
            triangles = new List<int>();

            rowCacheMax = new int[Resolution * 2 + 1];
            rowCacheMin = new int[Resolution * 2 + 1];
        }

        public void Clear()
        {
            vertices.Clear();
            triangles.Clear();
            mesh.Clear();

            if(Wall != null)
                Wall.Clear();
        }

        public void Apply(bool update_collider = true)
        {
            //only need to calculate nearby vertices if we're going to be using mesh smoothing
            if(smoothMesh)
                CalculateNeighbors();
            CalculateNormals();
            CalculateSimpleUVs();

            mesh.vertices = vertices.ToArray();
            mesh.normals = normals;
            mesh.triangles = triangles.ToArray();

            if(null != simple_uvs && simple_uvs.Length == mesh.vertices.Length)
                mesh.uv = simple_uvs;

            if(Wall != null)
                Wall.Apply(update_collider);

            if(update_collider)
                meshCollider.sharedMesh = meshFilter.sharedMesh;
        }

        [SerializeField]
        [HideInInspector]
        List<int>[] vertex_neighbors;

        [SerializeField]
        [HideInInspector]
        Vector2 maxVertex;

        void AddNeighbor(int vertex, int neighbor_a, int neighbor_b)
        {
            if(vertices[vertex].x > maxVertex.x)
                maxVertex.x = vertices[vertex].x;

            if(vertices[vertex].z > maxVertex.y)
                maxVertex.y = vertices[vertex].z;

            if(vertex_neighbors[vertex] == null)
                vertex_neighbors[vertex] = new List<int>();

            if(!vertex_neighbors[vertex].Contains(neighbor_a))
                vertex_neighbors[vertex].Add(neighbor_a);
            if(!vertex_neighbors[vertex].Contains(neighbor_b))
                vertex_neighbors[vertex].Add(neighbor_b);
        }

        void AddNeighbors(int a, int b, int c)
        {
            AddNeighbor(a, b, c);
            AddNeighbor(b, a, c);
            AddNeighbor(c, b, a);
        }

        void CalculateNeighbors()
        {
            vertex_neighbors = new List<int>[vertices.Count];

            maxVertex = Vector2.zero;

            int t = 0;
            do
            {
                //calculate vertex indices for this triangle
                int a = t;
                int b = (t + 1) % triangles.Count;
                int c = (t + 2) % triangles.Count;

                a = triangles[a];
                b = triangles[b];
                c = triangles[c];

                //add these vertices as neighbors of eachother
                AddNeighbors(a, b, c);

                //increment the active triangle
                t = (t + 3) % triangles.Count;
            } while(t != 0);
        }

        void SmoothVertex(int vertex, float skip_height = 0f, bool skip_seams = true)
        {
            Vector3 vertexToSmooth = vertices[vertex];

            //don't smooth vertices near this value, useful for retaining 'nice' seams
            if(Mathnv.FastApproximately(vertexToSmooth.y, skip_height, .01f))
                return;

            bool xSeam = false;
            bool zSeam = false;

            if(vertexToSmooth.x > maxVertex.x - 1f)
                xSeam = true;

            if(vertexToSmooth.z > maxVertex.y - 1f)
                zSeam = true;

            if(vertexToSmooth.x < 1f)
                xSeam = true;

            if(vertexToSmooth.z < 1f)
                zSeam = true;

            if(skip_seams && (zSeam || xSeam))
                return;

            List<int> neighbors = vertex_neighbors[vertex];

            Vector3 summation = Vector3.zero;
            for(int i = 0; i < neighbors.Count; ++i)
            {
                if(xSeam)
                {
                    summation.z += vertices[neighbors[i]].z;
                }
                if(zSeam)
                {
                    summation.x += vertices[neighbors[i]].x;
                }
                if(!xSeam && !zSeam)
                {
                    summation += vertices[neighbors[i]];
                }
            }

            //summation += vertices[ vertex ];

            summation = summation * (1f / (neighbors.Count));
            if(xSeam || zSeam)
            {
                Vector3 temp = vertexToSmooth;
                if(xSeam)
                    temp.z = summation.z;
                if(zSeam)
                    temp.x = summation.x;
                vertices[vertex] = temp;
            }
            else
            {
                vertices[vertex] = summation;
            }
        }

        void CalculateSimpleUVs()
        {
            if(useUVMap)
                return;

            simple_uvs = new Vector2[vertices.Count];
            for(int i = 0; i < vertices.Count; ++i)
            {
                simple_uvs[i] = new Vector2(vertices[i].x / Resolution, vertices[i].z / Resolution);
            }
        }

        void CalculateNormals()
        {
            normals = new Vector3[vertices.Count];

            int t = 0;
            do
            {
                //calculate vertex indices for this triangle
                int a = t;
                int b = (t + 1) % triangles.Count;
                int c = (t + 2) % triangles.Count;

                a = triangles[a];
                b = triangles[b];
                c = triangles[c];

                //apply smoothing to the vertices
                if(smoothMesh)
                {
                    SmoothVertex(a);
                    SmoothVertex(b);
                    SmoothVertex(c);
                }

                //calculate the triangle's normal
                Vector3 tnormal = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);

                //update the vertex normals
                normals[a] = (normals[a] + tnormal).normalized;
                normals[b] = (normals[b] + tnormal).normalized;
                normals[c] = (normals[c] + tnormal).normalized;

                //increment the active triangle
                t = (t + 3) % triangles.Count;
            } while(t != 0);
        }

        public void FillFirstRowCache(Map mesh_input)
        {
            CacheFirstCorner(mesh_input[0, 0]);
            int i = 0;
            for(; i < Resolution - 1; i++)
            {
                CacheNextEdgeAndCorner(i * 2, mesh_input[i], mesh_input[i + 1]);
            }
            if(xNeighbor != null)
            {
                Vector3 offsetx = new Vector3(Resolution, 0, 0);
                CacheNextEdgeAndCornerWithOffset(i * 2, mesh_input[i], xNeighbor.BoundryLayer[0], offsetx);
            }
        }

        private void CacheFirstCorner(MapElement a)
        {
            //if(a != null && !a.Empty)
            //{
            //    rowCacheMax[0] = vertices.Count;
            //    vertices.Add(a.wposition);
            //    if(Wall != null)
            //        Wall.CacheXEdge(WallIndex(0), a.wposition);
            //}
        }

        private void CacheFirstCornerWithOffset(MapElement a, Vector3 offset)
        {
            //if(a != null && !a.Empty)
            //{
            //    rowCacheMax[0] = vertices.Count;
            //    vertices.Add(a.wposition + offset);
            //    if(Wall != null)
            //        Wall.CacheXEdge(WallIndex(0), a.wposition + offset);
            //}
        }

        private void CacheNextEdgeAndCorner(int i, MapElement xMin, MapElement xMax)
        {
            //if(!MapElement.CompareID(xMin, xMax))
            //{
            //    rowCacheMax[i + 1] = vertices.Count;
            //    vertices.Add(xMin.wxEdgePosition);
            //    if(Wall != null)
            //        Wall.CacheXEdge(WallIndex(i), xMin.wxEdgePosition);
            //}
            //if(xMax != null && !xMax.Empty)
            //{
            //    rowCacheMax[i + 2] = vertices.Count;
            //    vertices.Add(xMax.wposition);
            //}
        }

        private void CacheNextEdgeAndCornerWithOffset(int i, MapElement xMin, MapElement xMax, Vector3 offset)
        {
            //if(!MapElement.CompareID(xMin, xMax))
            //{
            //    rowCacheMax[i + 1] = vertices.Count;
            //    vertices.Add(xMin.wxEdgePosition);
            //    if(Wall != null)
            //        Wall.CacheXEdge(WallIndex(i), xMin.wxEdgePosition);
            //}
            //if(xMax != null && !xMax.Empty)
            //{
            //    rowCacheMax[i + 2] = vertices.Count;
            //    vertices.Add(xMax.wposition + offset);
            //}
        }

        private void CacheNextEdgeAndCornerWithOffset(int i, MapElement xMin, MapElement xMax, Vector3 minOffset, Vector3 maxOffset)
        {
            //if(!MapElement.CompareID(xMin, xMax))
            //{
            //    rowCacheMax[i + 1] = vertices.Count;
            //    vertices.Add(xMin.wxEdgePosition + minOffset);
            //    if(Wall != null)
            //        Wall.CacheXEdge(WallIndex(i), xMin.wxEdgePosition + minOffset);
            //}
            //if(xMax != null && !xMax.Empty)
            //{
            //    rowCacheMax[i + 2] = vertices.Count;
            //    vertices.Add(xMax.wposition + maxOffset);
            //}
        }

        private void CacheNextMiddleEdge(MapElement yMin, MapElement yMax)
        {
            //edgeCacheMin = edgeCacheMax;
            //if(Wall != null)
            //    Wall.PrepareCacheForNextCell();
            //if(!MapElement.CompareID(yMin, yMax))
            //{
            //    edgeCacheMax = vertices.Count;
            //    vertices.Add(yMin.wyEdgePosition);
            //    if(Wall != null)
            //        Wall.CacheYEdge(yMin.wyEdgePosition);
            //}
        }

        private void CacheNextMiddleEdgeWithOffset(MapElement yMin, MapElement yMax, Vector3 offset)
        {
            //edgeCacheMin = edgeCacheMax;
            //if(Wall != null)
            //    Wall.PrepareCacheForNextCell();
            //if(!MapElement.CompareID(yMin, yMax))
            //{
            //    edgeCacheMax = vertices.Count;
            //    vertices.Add(yMin.wyEdgePosition + offset);
            //    if(Wall != null)
            //        Wall.CacheYEdge(yMin.wyEdgePosition + offset);
            //}
        }

        public void TriangulateRows(Map mesh_input)
        {
            for(int j = 0; j < mesh_input.ValidArea.size.y; ++j)
            {
                SwapRowCaches();
                CacheFirstCorner(mesh_input[0, j + 1]);
                CacheNextMiddleEdge(mesh_input[0, j], mesh_input[0, j + 1]);

                for(int i = 0; i < mesh_input.ValidArea.size.x; ++i)
                {
                    int cacheIndex = i * 2;

                    CacheNextEdgeAndCorner(cacheIndex, mesh_input[i, j + 1], mesh_input[i + 1, j + 1]);

                    CacheNextMiddleEdge(mesh_input[i + 1, j], mesh_input[i + 1, j + 1]);

                    TriangulateCell(cacheIndex
                                   , mesh_input[i, j]
                                   , mesh_input[i + 1, j]
                                   , mesh_input[i, j + 1]
                                   , mesh_input[i + 1, j + 1]);
                }
                if(xNeighbor != null)
                {
                    TriangulateGapCell(mesh_input, j);
                }
            }
            if(yNeighbor != null)
            {
                TriangulateGapRow(mesh_input);
            }
        }

        private void SwapRowCaches()
        {
            int[] rowSwap = rowCacheMin;
            rowCacheMin = rowCacheMax;
            rowCacheMax = rowSwap;
            if(Wall != null)
                Wall.PrepareCacheForNextRow();
        }

        private void TriangulateGapRow(Map mesh_input)
        {
            Vector3 offset = new Vector3(0, 0, Resolution);

            SwapRowCaches();
            MapElement ta = mesh_input[0, (int)mesh_input.ValidArea.size.y];
            MapElement tb = mesh_input[1, (int)mesh_input.ValidArea.size.y];
            MapElement tc = yNeighbor.BoundryLayer[0, 0];
            MapElement td = yNeighbor.BoundryLayer[1, 0];
            //CacheFirstCornerWithOffset( tc, offset );
            //CacheNextMiddleEdgeWithOffset( mesh_input[ (int)mesh_input.ValidArea.size.y * Resolution ], tc, offset );
            CacheFirstCornerWithOffset(tc, offset);
            CacheNextMiddleEdge(mesh_input[(int)mesh_input.ValidArea.size.y * Resolution], tc);

            //CacheFirstCornerWithOffset( yNeighbor.map.First[ 0, 0 ], offset );
            //CacheNextMiddleEdgeWithOffset( yNeighbor.map.First[ 1, 0 ], yNeighbor.map.First[ 0, 0 ], offset );

            for(int i = 0; i < (int)mesh_input.ValidArea.size.y; ++i)
            {
                MapElement a = mesh_input[i, (int)mesh_input.ValidArea.size.y];
                MapElement b = mesh_input[i + 1, (int)mesh_input.ValidArea.size.y];
                MapElement c = yNeighbor.BoundryLayer[i, 0];
                MapElement d = yNeighbor.BoundryLayer[i + 1, 0];

                int cacheIndex = i * 2;

                CacheNextEdgeAndCornerWithOffset(cacheIndex, c, d, offset, offset);
                CacheNextMiddleEdge(b, d);

                TriangulateCell(cacheIndex, a, b, c, d);
                //TriangulateCellWithOffset( a, b, c, d, Vector3.zero, Vector3.zero, offset, offset);
            }

            if(xyNeighbor != null)
            {
                MapElement ax = mesh_input[(int)mesh_input.ValidArea.size.y, (int)mesh_input.ValidArea.size.y];
                MapElement bx = xNeighbor.BoundryLayer[0, (int)mesh_input.ValidArea.size.y];
                MapElement cx = yNeighbor.BoundryLayer[(int)mesh_input.ValidArea.size.y, 0];
                MapElement dx = xyNeighbor.BoundryLayer[0, 0];

                Vector3 offsetx = new Vector3(Resolution, 0, 0);
                Vector3 offsetz = new Vector3(0, 0, Resolution);

                int cacheIndex = ((int)mesh_input.ValidArea.size.y) * 2;

                CacheNextEdgeAndCornerWithOffset(cacheIndex, cx, dx, offsetz, offsetx + offsetz);
                CacheNextMiddleEdgeWithOffset(bx, dx, offsetx);

                //TriangulateCellWithOffset( ax, bx, cx, dx, Vector3.zero, offsetx, offsetz, offsetx + offsetz );
                TriangulateCell(cacheIndex, ax, bx, cx, dx);
            }
        }

        private void TriangulateGapCell(Map mesh_input, int row)
        {
            MapElement a = mesh_input[(int)mesh_input.ValidArea.size.x, row];
            MapElement b = xNeighbor.BoundryLayer[0, row];
            MapElement c = mesh_input[(int)mesh_input.ValidArea.size.x, row + 1];
            MapElement d = xNeighbor.BoundryLayer[0, row + 1];

            Vector3 offset = new Vector3(Resolution, 0, 0);

            int cacheIndex = ((int)mesh_input.ValidArea.size.y) * 2;

            CacheNextEdgeAndCornerWithOffset(cacheIndex, c, d, offset);
            CacheNextMiddleEdgeWithOffset(b, d, offset);

            TriangulateCell(cacheIndex, a, b, c, d);
            //TriangulateCellWithOffset( a, b, c, d, Vector3.zero, offset, Vector3.zero, offset );
        }

        int GetCellType(MapElement a, MapElement b, MapElement c, MapElement d)
        {
            int cellType = 0;
            if(a != null && !a.Empty)
            {
                cellType |= 1;
            }
            if(b != null && !b.Empty)
            {
                cellType |= 2;
            }
            if(c != null && !c.Empty)
            {
                cellType |= 4;
            }
            if(d != null && !d.Empty)
            {
                cellType |= 8;
            }
            return cellType;
        }

        private void AddTriangle(int a, int b, int c)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
        }

        private void AddQuad(int a, int b, int c, int d)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(d);
        }

        private void AddPentagon(int a, int b, int c, int d, int e)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(c);
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(d);
            triangles.Add(a);
            triangles.Add(d);
            triangles.Add(e);
        }

        void SetVertexHeight(float height, int vertex)
        {
            Vector3 v = vertices[vertex];
            v.y = height;
            vertices[vertex] = v;
        }

        //taken from 
        //http://catlikecoding.com/unity/tutorials/marching-squares/
        private void TriangulateCell(int i, MapElement a, MapElement b, MapElement c, MapElement d)
        {
            int cellType = GetCellType(a, b, c, d);

            switch(cellType)
            {
                //simple case, no triangle
                case 0:
                    return;

                //corner case
                case 1:
                    AddTriangle((rowCacheMin[i]), (edgeCacheMin), (rowCacheMin[i + 1]));

                    SetVertexHeight(innerHeightCorner, rowCacheMin[i]);
                    SetVertexHeight(outerHeightCorner, edgeCacheMin);
                    SetVertexHeight(outerHeightCorner, rowCacheMin[i + 1]);

                    if(Wall != null) Wall.AddACAB(WallIndex(i));
                    break;
                case 2:
                    AddTriangle((rowCacheMin[i + 2]), (rowCacheMin[i + 1]), (edgeCacheMax));

                    SetVertexHeight(innerHeightCorner, rowCacheMin[i + 2]);
                    SetVertexHeight(outerHeightCorner, rowCacheMin[i + 1]);
                    SetVertexHeight(outerHeightCorner, edgeCacheMax);

                    if(Wall != null) Wall.AddABBD(WallIndex(i));
                    break;
                case 4:
                    AddTriangle((rowCacheMax[i]), (rowCacheMax[i + 1]), (edgeCacheMin));

                    SetVertexHeight(innerHeightCorner, rowCacheMax[i]);
                    SetVertexHeight(outerHeightCorner, rowCacheMax[i + 1]);
                    SetVertexHeight(outerHeightCorner, edgeCacheMin);

                    if(Wall != null) Wall.AddCDAC(WallIndex(i));
                    break;
                case 8:
                    AddTriangle((rowCacheMax[i + 2]), (edgeCacheMax), (rowCacheMax[i + 1]));

                    SetVertexHeight(innerHeightCorner, rowCacheMax[i + 2]);
                    SetVertexHeight(outerHeightCorner, edgeCacheMax);
                    SetVertexHeight(outerHeightCorner, rowCacheMax[i + 1]);

                    if(Wall != null) Wall.AddBDCD(WallIndex(i));
                    break;

                //edge case
                case 3:
                    AddQuad((rowCacheMin[i]), (edgeCacheMin), (edgeCacheMax), (rowCacheMin[i + 2]));

                    //top edge
                    SetVertexHeight(innerHeightEdge, rowCacheMin[i]);
                    SetVertexHeight(outerHeightEdge, edgeCacheMin);
                    SetVertexHeight(outerHeightEdge, edgeCacheMax);
                    SetVertexHeight(innerHeightEdge, rowCacheMin[i + 2]);

                    if(Wall != null) Wall.AddACBD(WallIndex(i));
                    break;
                case 5:
                    AddQuad((rowCacheMin[i]), (rowCacheMax[i]), (rowCacheMax[i + 1]), (rowCacheMin[i + 1]));

                    //right edge
                    SetVertexHeight(innerHeightEdge, rowCacheMin[i]);
                    SetVertexHeight(innerHeightEdge, rowCacheMax[i]);
                    SetVertexHeight(outerHeightEdge, rowCacheMax[i + 1]);
                    SetVertexHeight(outerHeightEdge, rowCacheMin[i + 1]);

                    if(Wall != null) Wall.AddCDAB(WallIndex(i));
                    break;
                case 10:
                    AddQuad((rowCacheMin[i + 1]), (rowCacheMax[i + 1]), (rowCacheMax[i + 2]), (rowCacheMin[i + 2]));

                    //left edge
                    SetVertexHeight(outerHeightEdge, rowCacheMin[i + 1]);
                    SetVertexHeight(outerHeightEdge, rowCacheMax[i + 1]);
                    SetVertexHeight(innerHeightEdge, rowCacheMax[i + 2]);
                    SetVertexHeight(innerHeightEdge, rowCacheMin[i + 2]);

                    if(Wall != null) Wall.AddABCD(WallIndex(i));
                    break;
                case 12:
                    AddQuad((edgeCacheMin), (rowCacheMax[i]), (rowCacheMax[i + 2]), (edgeCacheMax));

                    //bottom edge
                    SetVertexHeight(outerHeightEdge, edgeCacheMin);
                    SetVertexHeight(innerHeightEdge, rowCacheMax[i]);
                    SetVertexHeight(innerHeightEdge, rowCacheMax[i + 2]);
                    SetVertexHeight(outerHeightEdge, edgeCacheMax);

                    if(Wall != null) Wall.AddBDAC(WallIndex(i));
                    break;

                //The cases with three filled and one empty voxel each require a square with a single corner cut away. 
                //This is basically a pentagon that's stretched out of shape. 
                //A pentagon can be created with five vertices and three triangles that form a small fan.
                case 7:
                    AddPentagon((rowCacheMin[i]), (rowCacheMax[i]), (rowCacheMax[i + 1]), (edgeCacheMax), (rowCacheMin[i + 2]));

                    SetVertexHeight(innerHeight, rowCacheMin[i]);
                    SetVertexHeight(innerHeightEdge, rowCacheMax[i]);
                    SetVertexHeight(outerHeightCorner, rowCacheMax[i + 1]);
                    SetVertexHeight(outerHeightCorner, edgeCacheMax);
                    SetVertexHeight(innerHeightEdge, rowCacheMin[i + 2]);

                    if(Wall != null) Wall.AddCDBD(WallIndex(i));
                    break;
                case 11:
                    AddPentagon((rowCacheMin[i + 2]), (rowCacheMin[i]), (edgeCacheMin), (rowCacheMax[i + 1]), (rowCacheMax[i + 2]));

                    SetVertexHeight(innerHeight, rowCacheMin[i + 2]);
                    SetVertexHeight(innerHeightEdge, rowCacheMin[i]);
                    SetVertexHeight(outerHeightCorner, edgeCacheMin);
                    SetVertexHeight(outerHeightCorner, rowCacheMax[i + 1]);
                    SetVertexHeight(innerHeightEdge, rowCacheMax[i + 2]);

                    if(Wall != null) Wall.AddACCD(WallIndex(i));
                    break;
                case 13:
                    AddPentagon((rowCacheMax[i]), (rowCacheMax[i + 2]), (edgeCacheMax), (rowCacheMin[i + 1]), (rowCacheMin[i]));

                    SetVertexHeight(innerHeight, rowCacheMax[i]);
                    SetVertexHeight(innerHeightEdge, rowCacheMax[i + 2]);
                    SetVertexHeight(outerHeightCorner, edgeCacheMax);
                    SetVertexHeight(outerHeightCorner, rowCacheMin[i + 1]);
                    SetVertexHeight(innerHeightEdge, rowCacheMin[i]);

                    if(Wall != null) Wall.AddBDAB(WallIndex(i));
                    break;
                case 14:
                    AddPentagon((rowCacheMax[i + 2]), (rowCacheMin[i + 2]), (rowCacheMin[i + 1]), (edgeCacheMin), (rowCacheMax[i]));

                    SetVertexHeight(innerHeight, rowCacheMax[i + 2]);
                    SetVertexHeight(innerHeightEdge, rowCacheMin[i + 2]);
                    SetVertexHeight(outerHeightCorner, rowCacheMin[i + 1]);
                    SetVertexHeight(outerHeightCorner, edgeCacheMin);
                    SetVertexHeight(innerHeightEdge, rowCacheMax[i]);

                    if(Wall != null) Wall.AddABAC(WallIndex(i));
                    break;

                //Only the two opposite-corner cases are still missing, 6 and 9. 
                //I decided to disconnect them, so they require two triangles each, which you can copy from the single-triangle cases. 
                //Connecting them would've required a hexagon instead.
                case 6:
                    AddTriangle((rowCacheMin[i + 2]), (rowCacheMin[i + 1]), (edgeCacheMax));
                    AddTriangle((rowCacheMax[i]), (rowCacheMax[i + 1]), (edgeCacheMin));

                    //SetVertexHeight( innerHeightCorner, rowCacheMin[ i + 2 ] );
                    //SetVertexHeight( outerHeightCorner, rowCacheMin[ i + 1 ] );
                    //SetVertexHeight( outerHeightCorner, edgeCacheMax );

                    //SetVertexHeight( innerHeightCorner, rowCacheMax[ i ] );
                    //SetVertexHeight( outerHeightCorner, rowCacheMax[ i + 1 ] );
                    //SetVertexHeight( outerHeightCorner, edgeCacheMin );

                    if(Wall != null) Wall.AddABBD(WallIndex(i));
                    if(Wall != null) Wall.AddCDAC(WallIndex(i));
                    if(Wall != null) Wall.AddABAC(WallIndex(i));
                    if(Wall != null) Wall.AddCDBD(WallIndex(i));
                    break;
                case 9:
                    AddTriangle((rowCacheMin[i]), (edgeCacheMin), (rowCacheMin[i + 1]));
                    AddTriangle((rowCacheMax[i + 2]), (edgeCacheMax), (rowCacheMax[i + 1]));

                    //SetVertexHeight( innerHeightCorner, rowCacheMin[ i ] );
                    //SetVertexHeight( outerHeightCorner, edgeCacheMin );
                    //SetVertexHeight( outerHeightCorner, rowCacheMin[ i + 1 ] );

                    //SetVertexHeight( innerHeightCorner, rowCacheMax[ i + 2 ] );
                    //SetVertexHeight( outerHeightCorner, edgeCacheMax );
                    //SetVertexHeight( outerHeightCorner, rowCacheMax[ i + 1 ] );

                    if(Wall != null) Wall.AddACAB(WallIndex(i));
                    if(Wall != null) Wall.AddBDCD(WallIndex(i));
                    if(Wall != null) Wall.AddBDAB(WallIndex(i));
                    if(Wall != null) Wall.AddACCD(WallIndex(i));
                    break;

                //center case
                case 15:
                    AddQuad((rowCacheMin[i]), (rowCacheMax[i]), (rowCacheMax[i + 2]), (rowCacheMin[i + 2]));

                    SetVertexHeight(innerHeight, rowCacheMin[i]);
                    SetVertexHeight(innerHeight, rowCacheMax[i]);
                    SetVertexHeight(innerHeight, rowCacheMax[i + 2]);
                    SetVertexHeight(innerHeight, rowCacheMin[i + 2]);

                    break;
            }
        }
    }
}