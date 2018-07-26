using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace nv
{

    public class MapWallMesh : ScriptableObject
    {
        public Material renderMat;

        [HideInInspector]
        public MeshRenderer meshRenderer;
        [HideInInspector]
        public MeshFilter meshFilter;
        [HideInInspector]
        public MeshCollider meshCollider;

        public float bottom, top;

        //needed for proper UV mapping
        //public bool duplicateVertices = true;

        public bool useUVMap = false;

        [SerializeField]
        [HideInInspector]
        Mesh mesh;

        [SerializeField]
        [HideInInspector]
        List<Vector3> vertices;

        public int VertexCount
        {
            get
            {
                return vertices.Count;
            }
        }

        [SerializeField]
        [HideInInspector]
        Vector3[] normals;

        [SerializeField]
        [HideInInspector]
        List<int> triangles;

        [SerializeField]
        [HideInInspector]
        int[] xEdgesMin;

        [SerializeField]
        [HideInInspector]
        int[] xEdgesMax;

        [SerializeField]
        [HideInInspector]
        int yEdgeMin;

        [SerializeField]
        [HideInInspector]
        int yEdgeMax;

        public Vector2Int ChunkSize
        {
            get;
            private set;
        }

        [SerializeField]
        [HideInInspector]
        List<Vector2> simple_uvs;

        public void Init(GameObject root, Vector2Int chunkSize)
        {
            ChunkSize = chunkSize;

            GameObject wallRoot = new GameObject(name + " - Wall Mesh");
            wallRoot.transform.SetParent(root.transform);

            meshRenderer = wallRoot.GetOrAddComponent<MeshRenderer>();
            meshFilter = wallRoot.GetOrAddComponent<MeshFilter>();
            meshCollider = wallRoot.GetOrAddComponent<MeshCollider>();

            meshFilter.mesh = mesh = new Mesh();

            mesh.name = wallRoot.name;
            vertices = new List<Vector3>();
            triangles = new List<int>();

            xEdgesMin = new int[ChunkSize.x];
            xEdgesMax = new int[ChunkSize.x];
        }

        public void Clear()
        {
            vertices.Clear();
            triangles.Clear();
            if(mesh != null)
                mesh.Clear();
            simple_uvs = new List<Vector2>();
        }

        public void Apply(bool update_collider = true)
        {
            if(triangles.Count <= 0)
                return;

            mesh.vertices = vertices.ToArray();

            CalculateNormals();

            if(!useUVMap)
            {
                //mesh.uv = simple_uvs.ToArray();
            }

            mesh.SetTriangles(triangles, 0);

            if(update_collider)
                meshCollider.sharedMesh = meshFilter.sharedMesh;

            meshRenderer.sharedMaterial = renderMat;
        }

        public void CalculateNormals()
        {
            if(triangles.Count <= 0)
                return;

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

                //calculate the trigangle's normal
                Vector3 tnormal = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);

                //update the vertex normals
                normals[a] = (normals[a] + tnormal).normalized;
                normals[b] = (normals[b] + tnormal).normalized;
                normals[c] = (normals[c] + tnormal).normalized;

                //increment the active triangle
                t = (t + 3) % triangles.Count;
            } while(t != 0);
            
            mesh.normals = normals;
        }

        public void MoveWallVertex(int i, Vector2 delta)
        {
            vertices[i] = new Vector3(vertices[i].x + delta.x, vertices[i].y, vertices[i].z + delta.y);
            vertices[i + 1] = new Vector3(vertices[i + 1].x + delta.x, vertices[i + 1].y, vertices[i + 1].z + delta.y);
        }

        public void SetWallVertex(int i, Vector2 update)
        {
            vertices[i] = new Vector3(update.x, vertices[i].y, update.y);
            vertices[i + 1] = new Vector3(update.x, vertices[i + 1].y, update.y);
        }

        public void CacheXEdge(int i, Vector3 e)
        {
            xEdgesMax[i] = vertices.Count;
            Vector3 v = e;
            v.y = bottom;
            vertices.Add(v);
            v.y = top;
            vertices.Add(v);
        }

        public void CacheYEdge(Vector3 e)
        {
            yEdgeMax = vertices.Count;
            Vector3 v = e;
            v.y = bottom;
            vertices.Add(v);
            v.y = top;
            vertices.Add(v);
        }

        public void PrepareCacheForNextCell()
        {
            yEdgeMin = yEdgeMax;
        }

        public void PrepareCacheForNextRow()
        {
            int[] swap = xEdgesMin;
            xEdgesMin = xEdgesMax;
            xEdgesMax = swap;
        }

        public void CheckExpandSimpleUVs(int index)
        {
            while(simple_uvs.Count < index + 1)
                simple_uvs.Add(Vector2.zero);
        }

        public void CalculateUV(int a, int b, int c, int d)
        {
            CheckExpandSimpleUVs(a);
            CheckExpandSimpleUVs(b);
            CheckExpandSimpleUVs(c);
            CheckExpandSimpleUVs(d);

            Vector2 uvA = new Vector2(0f, 1f);
            Vector2 uvB = Vector2.zero;
            Vector2 uvC = new Vector2(1f, 0f);
            Vector2 uvD = Vector2.one;

            simple_uvs[a] = (uvA);
            simple_uvs[b] = (uvB);
            simple_uvs[c] = (uvC);
            simple_uvs[d] = (uvD);
        }

        private void AddSection(int a, int b)
        {
            triangles.Add(a);
            triangles.Add(b);
            triangles.Add(b + 1);
            triangles.Add(a);
            triangles.Add(b + 1);
            triangles.Add(a + 1);

            if(!useUVMap)
            {
                //CalculateUV( b + 1, b, a, a + 1 );
            }
        }

        public void AddACAB(int i)
        {
            AddSection(yEdgeMin, xEdgesMin[i]);
        }

        public void AddACBD(int i)
        {
            AddSection(yEdgeMin, yEdgeMax);
        }

        public void AddACCD(int i)
        {
            AddSection(yEdgeMin, xEdgesMax[i]);
        }

        public void AddABAC(int i)
        {
            AddSection(xEdgesMin[i], yEdgeMin);
        }

        public void AddABBD(int i)
        {
            AddSection(xEdgesMin[i], yEdgeMax);
        }

        public void AddABCD(int i)
        {
            AddSection(xEdgesMin[i], xEdgesMax[i]);
        }

        public void AddBDAB(int i)
        {
            AddSection(yEdgeMax, xEdgesMin[i]);
        }

        public void AddBDAC(int i)
        {
            AddSection(yEdgeMax, yEdgeMin);
        }

        public void AddCDAB(int i)
        {
            AddSection(xEdgesMax[i], xEdgesMin[i]);
        }

        public void AddCDAC(int i)
        {
            AddSection(xEdgesMax[i], yEdgeMin);
        }

        public void AddCDBD(int i)
        {
            AddSection(xEdgesMax[i], yEdgeMax);
        }

        public void AddBDCD(int i)
        {
            AddSection(yEdgeMax, xEdgesMax[i]);
        }
    }
}