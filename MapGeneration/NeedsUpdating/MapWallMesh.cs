﻿using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace nv
{

    public class MapWallMesh : ScriptableObject
    {
        public MeshFilter meshFilter;
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

        public int Resolution
        {
            get;
            private set;
        }

        [SerializeField]
        [HideInInspector]
        List<Vector2> simple_uvs;

        public void Init(GameObject root, int chunkSize)
        {
            Resolution = chunkSize;

            root.GetOrAddComponentIfNull(ref meshFilter);
            root.GetOrAddComponentIfNull(ref meshCollider);

            meshFilter.mesh = mesh = new Mesh();

            mesh.name = "Wall Mesh";
            vertices = new List<Vector3>();
            triangles = new List<int>();

            xEdgesMin = new int[Resolution];
            xEdgesMax = new int[Resolution];
        }

        public void Clear()
        {
            vertices.Clear();
            triangles.Clear();
            mesh.Clear();
            simple_uvs = new List<Vector2>();
        }

        public void Apply(bool update_collider = true)
        {
            mesh.vertices = vertices.ToArray();
            CalculateNormals();

            if(!useUVMap)
            {
                //mesh.uv = simple_uvs.ToArray();
            }

            mesh.triangles = triangles.ToArray();

            if(update_collider)
                meshCollider.sharedMesh = meshFilter.sharedMesh;
        }

        public void CalculateNormals()
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