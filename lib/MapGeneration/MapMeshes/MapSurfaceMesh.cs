using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace nv
{
    public class MapSurfaceMesh : ScriptableObject
    {
        public Material renderMat;

        [HideInInspector]
        public MeshRenderer meshRenderer;
        [HideInInspector]
        public MeshFilter meshFilter;
        [HideInInspector]
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
        public float smoothingSkipHeight = 0f;
        public bool smoothingSkipSeams = true;

        public bool useUVMap = false;

        [HideInInspector]
        public MapMesh Owner { get; private set; }
        
        public MapMesh xNeighbor
        {
            get
            {
                return Owner.xNeighbor;
            }
            set
            {
                Owner.xNeighbor = value;
            }
        }

        public MapMesh yNeighbor
        {
            get
            {
                return Owner.yNeighbor;
            }
            set
            {
                Owner.yNeighbor = value;
            }
        }

        public MapMesh xyNeighbor
        {
            get
            {
                return Owner.xyNeighbor;
            }
            set
            {
                Owner.xyNeighbor = value;
            }
        }

        [Tooltip("Defines properties this surface will have when generated")]
        [EditScriptable]
        public MapWallMesh wallDefinition;

        [SerializeField]
        [HideInInspector]
        MapWallMesh wall;

        public MapWallMesh Wall
        {
            get; private set;
        }

        public int WallIndex(int i)
        {
            return i / 2;
        }

        [SerializeField]
        [HideInInspector]
        Mesh mesh;

        Dictionary<int,int> vertHasWall;

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

        public Vector2Int ChunkSize
        {
            get;
            private set;
        }

        [HideInInspector]
        public Bounds mapBounds;

        [SerializeField]
        [HideInInspector]
        Vector2[] simple_uvs;

        [EditScriptable]
        public MapElementEvaluator elementEvaluator;

        public bool IsEqual(MapElement a, MapElement b)
        {
            return elementEvaluator.IsMeshElement(a) && elementEvaluator.IsMeshElement(b);
        }

        Vector3 ToEdgePosX(Vector3 pos)
        {
            return pos + new Vector3(.5f, 0f, 0f);
        }

        Vector3 ToEdgePosY(Vector3 pos)
        {
            return pos + new Vector3(0f, 0f, .5f);
        }

        public void Init(MapMesh owner, GameObject root, Vector2Int chunkSize)
        {
            Owner = owner;
            ChunkSize = chunkSize;

            if(wallDefinition != null)
            {
                Wall = Instantiate(wallDefinition) as MapWallMesh;
                Wall.Init(root, ChunkSize);
            }

            GameObject surfaceRoot = new GameObject(name + " - Surface Mesh");
            surfaceRoot.transform.SetParent(root.transform);

            meshRenderer = surfaceRoot.GetOrAddComponent<MeshRenderer>();
            meshFilter = surfaceRoot.GetOrAddComponent<MeshFilter>();
            meshCollider = surfaceRoot.GetOrAddComponent<MeshCollider>();

            meshFilter.mesh = mesh = new Mesh();
            mesh.name = surfaceRoot.name;
            vertices = new List<Vector3>();
            triangles = new List<int>();
            normals = new Vector3[0];
            vertHasWall = new Dictionary<int, int>();

            rowCacheMax = new int[ChunkSize.x * 2 + 1];
            rowCacheMin = new int[ChunkSize.x * 2 + 1];

            mapBounds = new Bounds();
            mapBounds.SetMinMax(Owner.worldPos, new Vector3(ChunkSize.x, innerHeight + 100f, ChunkSize.y));
        }

        public void Clear()
        {
            if(objectRenderers.Count > 0)
            {
                foreach(var objRenderer in objectRenderers)
                {
                    objRenderer.FreeMemory();
                }
                objectRenderers.Clear();
            }

            vertHasWall.Clear();
            vertices.Clear();
            triangles.Clear();

            if(mesh != null)
                mesh.Clear();

            if(Wall != null)
                Wall.Clear();
        }

        List<MapObjectRenderer> objectRenderers = new List<MapObjectRenderer>();

        public void RenderObjects()
        {
            foreach(var objRenderer in objectRenderers)
            {
                objRenderer.RenderObjects();
            }
        }

        public void Apply(bool update_collider = true)
        {
            //only need to calculate nearby vertices if we're going to be using mesh smoothing
            if(smoothMesh)
                CalculateNeighbors();
            
            CreateObjectRenderers();
                        
            CalculateNormals();
            CalculateSimpleUVs();

            if(vertices.Count != normals.Length)
            {
                Dev.LogError(string.Format("{0} vertices and {1} normals! These values need to match. We have an error somewhere in mesh generation: {2}", vertices.Count, normals.Length, name + "/" + Owner.name));
            }

            if(vertices.Count > 0)
                mesh.vertices = vertices.ToArray();
            else
                mesh.vertices = null;            

            if(normals.Length > 0)
                mesh.normals = normals;
            else
                mesh.normals = null;

            if(triangles.Count > 0)
                mesh.SetTriangles(triangles.ToArray(),0);
            else
                mesh.SetTriangles(new int[0],0);

            if(null != simple_uvs && simple_uvs.Length == mesh.vertices.Length)
                mesh.uv = simple_uvs;

            if(Wall != null)
                Wall.Apply(update_collider);

            if(update_collider)
                meshCollider.sharedMesh = meshFilter.sharedMesh;
            
            meshRenderer.sharedMaterial = renderMat;
        }

        void CreateObjectRenderers()
        {
            if(objectRenderers.Count > 0)
            {
                foreach(var objRenderer in objectRenderers)
                {
                    objRenderer.FreeMemory();
                }
                objectRenderers.Clear();
            }

            //calculate the objects to render
            Dictionary<string, MapElement> mapObjects = new Dictionary<string, MapElement>();
            for(int i = 0; i < Owner.MapData.Count; ++i)
            {
                if(elementEvaluator.IsObjectMeshElement(Owner.MapData[i]) && !mapObjects.ContainsKey(Owner.MapData[i].tags.ToString()))
                {
                    mapObjects.Add(Owner.MapData[i].tags.ToString(), Owner.MapData[i]);
                }
            }

            foreach(var mapObj in mapObjects)
            {
                var positions = Owner.MapData.GetPositionsOfType(mapObj.Value);
                Mesh objMesh = elementEvaluator.ObjectMeshElement(mapObj.Value);
                Material objMat = elementEvaluator.ObjectMaterialElement(mapObj.Value);

                MapObjectRenderer mpr = new MapObjectRenderer(objMesh, objMat, 0);
                mpr.boundingVolume = mapBounds;


                Vector3 meshPivotOffset = mapObj.Value.objectMeshOffset;

                List<Vector3> worldPositions = positions.Select(x => (new Vector3(x.x, 0f, x.y)) + Owner.worldPos + meshPivotOffset).ToList();

                Quaternion meshRot = Quaternion.Euler(mapObj.Value.meshRotation);

                List<Vector4> rotations = positions.Select(x => new Vector4(meshRot.x, meshRot.y, meshRot.z, meshRot.w)).ToList();

                List<float> scales = positions.Select(x => mapObj.Value.meshScale).ToList();

                mpr.SetRenderData(worldPositions, scales, rotations);
                objectRenderers.Add(mpr);
            }
        }

        private void OnDestroy()
        {
            Clear();
        }

        [SerializeField]
        [HideInInspector]
        List<int>[] vertex_neighbors;

        [SerializeField]
        [HideInInspector]
        Vector2 maxVertex;
        [SerializeField]
        [HideInInspector]
        Vector2 minVertex;

        void AddNeighbor(int vertex, int neighbor_a, int neighbor_b)
        {
            if(vertices[vertex].x > maxVertex.x)
                maxVertex.x = vertices[vertex].x;

            if(vertices[vertex].z > maxVertex.y)
                maxVertex.y = vertices[vertex].z;

            if(vertices[vertex].x < minVertex.x)
                minVertex.x = vertices[vertex].x;

            if(vertices[vertex].z < minVertex.y)
                minVertex.y = vertices[vertex].z;

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
            minVertex = new Vector2(float.MaxValue,float.MaxValue);

            if(triangles.Count <= 0)
                return;

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
            //TODO: need to smooth the vertices (x and z positions) that match wall vertices

            Vector3 vertexToSmooth = vertices[vertex];

            //don't smooth vertices near this value, useful for retaining 'nice' seams
            if(Mathnv.FastApproximately(vertexToSmooth.y, skip_height, .01f))
                return;

            bool xSeam = false;
            bool zSeam = false;

            if(vertexToSmooth.x > (maxVertex.x - 1f ))
                xSeam = true;

            if(vertexToSmooth.z > (maxVertex.y - 1f ))
                zSeam = true;

            if(vertexToSmooth.x < (minVertex.x + 1f ))
                xSeam = true;

            if(vertexToSmooth.z < (minVertex.y + 1f ))
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

            if(Wall != null && vertHasWall.ContainsKey(vertex))
            {
                int wallVertex = vertHasWall[vertex];// WallIndex(vertex);
                Wall.SetWallVertex(wallVertex, new Vector2(vertices[vertex].x, vertices[vertex].z));
            }
        }

        void CalculateSimpleUVs()
        {
            if(useUVMap)
                return;

            if(vertices.Count <= 0)
                return;

            simple_uvs = new Vector2[vertices.Count];
            for(int i = 0; i < vertices.Count; ++i)
            {
                simple_uvs[i] = new Vector2(vertices[i].x / ChunkSize.x, vertices[i].z / ChunkSize.y);
            }
        }

        void CalculateNormals()
        {
            normals = new Vector3[vertices.Count];

            if(vertices.Count <= 0)
                return;

            if(triangles.Count <= 0)
                return;

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
                    SmoothVertex(a, smoothingSkipHeight, smoothingSkipSeams);
                    SmoothVertex(b, smoothingSkipHeight, smoothingSkipSeams);
                    SmoothVertex(c, smoothingSkipHeight, smoothingSkipSeams);
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

        public void FillFirstRowCache(ArrayGrid<MapElement> mesh_input)
        {
            CacheFirstCorner(mesh_input[0, 0], Vector2Int.zero);
            int i = 0;
            for(; i < ChunkSize.x - 1; i++)
            {
                CacheNextEdgeAndCorner(i * 2, mesh_input[i], mesh_input[i + 1], mesh_input.GetPositionFromIndex(i), mesh_input.GetPositionFromIndex(i + 1));
            }
            if(xNeighbor != null)
            {
                Vector3 offsetx = new Vector3(ChunkSize.x, 0, 0);
                CacheNextEdgeAndCornerWithOffset(i * 2, mesh_input[i], xNeighbor.MapData[0], offsetx, mesh_input.GetPositionFromIndex(i), xNeighbor.MapData.GetPositionFromIndex(0));
            }
        }

        private void CacheFirstCorner(MapElement a, Vector2Int pos)
        {
            if(a != null && elementEvaluator.IsMeshElement(a))
            {
                if(Wall != null)
                    vertHasWall.Add(vertices.Count, Wall.VertexCount);
                rowCacheMax[0] = vertices.Count;
                vertices.Add(new Vector3(pos.x,0f,pos.y));
                if(Wall != null)
                    Wall.CacheXEdge(WallIndex(0), new Vector3(pos.x, 0f, pos.y));

            }
        }

        private void CacheFirstCornerWithOffset(MapElement a, Vector3 offset, Vector2Int pos)
        {
            if(a != null && elementEvaluator.IsMeshElement(a))
            {
                if(Wall != null)
                    vertHasWall.Add(vertices.Count, Wall.VertexCount);
                Vector3 wpos = new Vector3(pos.x, 0f, pos.y);
                rowCacheMax[0] = vertices.Count;
                vertices.Add(wpos + offset);
                if(Wall != null)
                    Wall.CacheXEdge(WallIndex(0), wpos + offset);
            }
        }

        private void CacheNextEdgeAndCorner(int i, MapElement xMin, MapElement xMax, Vector2Int minPos, Vector2Int maxPos)
        {
            bool hasMeshElement = (elementEvaluator.IsMeshElement(xMin) || elementEvaluator.IsMeshElement(xMax));
            Vector3 wposMin = new Vector3(minPos.x, 0f, minPos.y);
            Vector3 wposMax = new Vector3(maxPos.x, 0f, maxPos.y);
            if(hasMeshElement && !IsEqual(xMin, xMax))
            {
                if(Wall != null)
                    vertHasWall.Add(vertices.Count, Wall.VertexCount);
                rowCacheMax[i + 1] = vertices.Count;
                vertices.Add(ToEdgePosX(wposMin));
                if(Wall != null)
                    Wall.CacheXEdge(WallIndex(i), ToEdgePosX(wposMin));
            }
            if(xMax != null && elementEvaluator.IsMeshElement(xMax))
            {
                rowCacheMax[i + 2] = vertices.Count;
                vertices.Add(wposMax);
            }
        }

        private void CacheNextEdgeAndCornerWithOffset(int i, MapElement xMin, MapElement xMax, Vector3 offset, Vector2Int minPos, Vector2Int maxPos)
        {
            bool hasMeshElement = (elementEvaluator.IsMeshElement(xMin) || elementEvaluator.IsMeshElement(xMax));
            Vector3 wposMin = new Vector3(minPos.x, 0f, minPos.y);
            Vector3 wposMax = new Vector3(maxPos.x, 0f, maxPos.y);
            if(hasMeshElement && !IsEqual(xMin, xMax))
            {
                if(Wall != null)
                    vertHasWall.Add(vertices.Count, Wall.VertexCount);
                rowCacheMax[i + 1] = vertices.Count;
                vertices.Add(ToEdgePosX(wposMin));
                if(Wall != null)
                    Wall.CacheXEdge(WallIndex(i), ToEdgePosX(wposMin));
            }
            if(xMax != null && elementEvaluator.IsMeshElement(xMax))
            {
                rowCacheMax[i + 2] = vertices.Count;
                vertices.Add(wposMax + offset);
            }
        }

        private void CacheNextEdgeAndCornerWithOffset(int i, MapElement xMin, MapElement xMax, Vector3 minOffset, Vector3 maxOffset, Vector2Int minPos, Vector2Int maxPos)
        {
            bool hasMeshElement = (elementEvaluator.IsMeshElement(xMin) || elementEvaluator.IsMeshElement(xMax));
            Vector3 wposMin = new Vector3(minPos.x, 0f, minPos.y);
            Vector3 wposMax = new Vector3(maxPos.x, 0f, maxPos.y);
            if(hasMeshElement && !IsEqual(xMin, xMax))
            {
                if(Wall != null)
                    vertHasWall.Add(vertices.Count, Wall.VertexCount);
                rowCacheMax[i + 1] = vertices.Count;
                vertices.Add(ToEdgePosX(wposMin) + minOffset);
                if(Wall != null)
                    Wall.CacheXEdge(WallIndex(i), ToEdgePosX(wposMin) + minOffset);
            }
            if(xMax != null && elementEvaluator.IsMeshElement(xMax))
            {
                rowCacheMax[i + 2] = vertices.Count;
                vertices.Add(wposMax + maxOffset);
            }
        }

        private void CacheNextMiddleEdge(MapElement yMin, MapElement yMax, Vector2Int minPos, Vector2Int maxPos)
        {
            bool hasMeshElement = (elementEvaluator.IsMeshElement(yMin) || elementEvaluator.IsMeshElement(yMax));
            Vector3 wposMin = new Vector3(minPos.x, 0f, minPos.y);
            edgeCacheMin = edgeCacheMax;
            if(hasMeshElement && Wall != null)
                Wall.PrepareCacheForNextCell();
            if(hasMeshElement && !IsEqual(yMin, yMax))
            {
                if(Wall != null)
                    vertHasWall.Add(vertices.Count, Wall.VertexCount);
                edgeCacheMax = vertices.Count;
                vertices.Add(ToEdgePosY(wposMin));
                if(Wall != null)
                    Wall.CacheYEdge(ToEdgePosY(wposMin));
            }
        }

        private void CacheNextMiddleEdgeWithOffset(MapElement yMin, MapElement yMax, Vector3 offset, Vector2Int minPos, Vector2Int maxPos)
        {
            bool hasMeshElement = (elementEvaluator.IsMeshElement(yMin) || elementEvaluator.IsMeshElement(yMax));
            Vector3 wposMin = new Vector3(minPos.x, 0f, minPos.y);
            edgeCacheMin = edgeCacheMax;
            if(hasMeshElement && Wall != null)
                Wall.PrepareCacheForNextCell();
            if(hasMeshElement && !IsEqual(yMin, yMax))
            {
                if(Wall != null)
                    vertHasWall.Add(vertices.Count, Wall.VertexCount);
                edgeCacheMax = vertices.Count;
                vertices.Add(ToEdgePosY(wposMin) + offset);
                if(Wall != null)
                    Wall.CacheYEdge(ToEdgePosY(wposMin) + offset);
            }
        }

        public void TriangulateRows(ArrayGrid<MapElement> mesh_input)
        {
            for(int j = 0; j < mesh_input.ValidArea.size.y; ++j)
            {
                SwapRowCaches();
                CacheFirstCorner(mesh_input[0, j + 1], new Vector2Int(0, j + 1));
                CacheNextMiddleEdge(mesh_input[0, j], mesh_input[0, j + 1], new Vector2Int(0,j), new Vector2Int(0,j+1));

                for(int i = 0; i < mesh_input.ValidArea.size.x; ++i)
                {
                    int cacheIndex = i * 2;

                    CacheNextEdgeAndCorner(cacheIndex, mesh_input[i, j + 1], mesh_input[i + 1, j + 1], new Vector2Int(i,j+1), new Vector2Int(i + 1, j + 1));

                    CacheNextMiddleEdge(mesh_input[i + 1, j], mesh_input[i + 1, j + 1], new Vector2Int(i + 1,j), new Vector2Int(i+1,j+1));

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

        private void TriangulateGapRow(ArrayGrid<MapElement> mesh_input)
        {
            Vector3 offset = new Vector3(0, 0, ChunkSize.y);

            SwapRowCaches();
            MapElement ta = mesh_input[0, (int)mesh_input.ValidArea.size.y];
            MapElement tb = mesh_input[1, (int)mesh_input.ValidArea.size.y];
            MapElement tc = yNeighbor.MapData[0, 0];
            MapElement td = yNeighbor.MapData[1, 0];
            //-----CacheFirstCornerWithOffset( tc, offset );
            //-----CacheNextMiddleEdgeWithOffset( mesh_input[ (int)mesh_input.ValidArea.size.y * Resolution ], tc, offset );
            CacheFirstCornerWithOffset(tc, offset, new Vector2Int(0,0));
            CacheNextMiddleEdge(mesh_input[(int)mesh_input.ValidArea.size.y * ChunkSize.y], tc, mesh_input.GetPositionFromIndex((int)mesh_input.ValidArea.size.y * ChunkSize.y), new Vector2Int(0,0));

            //-----CacheFirstCornerWithOffset( yNeighbor.map.First[ 0, 0 ], offset );
            //-----CacheNextMiddleEdgeWithOffset( yNeighbor.map.First[ 1, 0 ], yNeighbor.map.First[ 0, 0 ], offset );

            for(int i = 0; i < (int)mesh_input.ValidArea.size.y; ++i)
            {
                MapElement a = mesh_input[i, (int)mesh_input.ValidArea.size.y];
                MapElement b = mesh_input[i + 1, (int)mesh_input.ValidArea.size.y];
                MapElement c = yNeighbor.MapData[i, 0];
                MapElement d = yNeighbor.MapData[i + 1, 0];

                int cacheIndex = i * 2;

                CacheNextEdgeAndCornerWithOffset(cacheIndex, c, d, offset, offset, new Vector2Int(i,0), new Vector2Int(i+1,0));
                CacheNextMiddleEdge(b, d, new Vector2Int(i+1, (int)mesh_input.ValidArea.size.y), new Vector2Int(i+1,0));

                TriangulateCell(cacheIndex, a, b, c, d);
                //-----TriangulateCellWithOffset( a, b, c, d, Vector3.zero, Vector3.zero, offset, offset);
            }

            if(xyNeighbor != null)
            {
                MapElement ax = mesh_input[(int)mesh_input.ValidArea.size.y, (int)mesh_input.ValidArea.size.y];
                MapElement bx = xNeighbor.MapData[0, (int)mesh_input.ValidArea.size.y];
                MapElement cx = yNeighbor.MapData[(int)mesh_input.ValidArea.size.y, 0];
                MapElement dx = xyNeighbor.MapData[0, 0];

                Vector3 offsetx = new Vector3(ChunkSize.x, 0, 0);
                Vector3 offsetz = new Vector3(0, 0, ChunkSize.y);

                int cacheIndex = ((int)mesh_input.ValidArea.size.y) * 2;

                CacheNextEdgeAndCornerWithOffset(cacheIndex, cx, dx, offsetz, offsetx + offsetz, new Vector2Int((int)mesh_input.ValidArea.size.y,0), new Vector2Int(0,0));
                CacheNextMiddleEdgeWithOffset(bx, dx, offsetx,new Vector2Int(0, (int)mesh_input.ValidArea.size.y), new Vector2Int(0,0));

                //-----TriangulateCellWithOffset( ax, bx, cx, dx, Vector3.zero, offsetx, offsetz, offsetx + offsetz );
                TriangulateCell(cacheIndex, ax, bx, cx, dx);
            }
        }

        private void TriangulateGapCell(ArrayGrid<MapElement> mesh_input, int row)
        {
            MapElement a = mesh_input[(int)mesh_input.ValidArea.size.x, row];
            MapElement b = xNeighbor.MapData[0, row];
            MapElement c = mesh_input[(int)mesh_input.ValidArea.size.x, row + 1];
            MapElement d = xNeighbor.MapData[0, row + 1];

            Vector3 offset = new Vector3(ChunkSize.x, 0, 0);

            int cacheIndex = ((int)mesh_input.ValidArea.size.y) * 2;

            CacheNextEdgeAndCornerWithOffset(cacheIndex, c, d, offset, new Vector2Int((int)mesh_input.ValidArea.size.x, row + 1), new Vector2Int(0,row+1));
            CacheNextMiddleEdgeWithOffset(b, d, offset, new Vector2Int(0,row), new Vector2Int(0,row+1));

            TriangulateCell(cacheIndex, a, b, c, d);
            //-----TriangulateCellWithOffset( a, b, c, d, Vector3.zero, offset, Vector3.zero, offset );
        }

        int GetCellType(MapElement a, MapElement b, MapElement c, MapElement d)
        {
            int cellType = 0;
            if(a != null && elementEvaluator.IsMeshElement(a))
            {
                cellType |= 1;
            }
            if(b != null && elementEvaluator.IsMeshElement(b))
            {
                cellType |= 2;
            }
            if(c != null && elementEvaluator.IsMeshElement(c))
            {
                cellType |= 4;
            }
            if(d != null && elementEvaluator.IsMeshElement(d))
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
            try
            {
                //Vector3 v = vertices[vertex];
                //v.y = height;
                vertices[vertex] = vertices[vertex].SetY(height);
            }
            catch(Exception e)
            {
                Dev.LogError("Exception: " + e.Message + string.Format(" :: Params height {0} ; vertx {1} ; vertices.Count {2} ", height, vertex, vertices.Count));
            }
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

                    if(Wall != null) Wall.AddABBD(WallIndex(i));
                    if(Wall != null) Wall.AddCDAC(WallIndex(i));
                    if(Wall != null) Wall.AddABAC(WallIndex(i));
                    if(Wall != null) Wall.AddCDBD(WallIndex(i));
                    break;
                case 9:
                    AddTriangle((rowCacheMin[i]), (edgeCacheMin), (rowCacheMin[i + 1]));
                    AddTriangle((rowCacheMax[i + 2]), (edgeCacheMax), (rowCacheMax[i + 1]));

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