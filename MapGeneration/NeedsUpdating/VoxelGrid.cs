using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

using Map = nv.ArrayGrid<nv.MapElement>;

namespace nv
{
    public class VoxelGrid : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        Map map;

        public Map GridData
        {
            get
            {
                return map;
            }
            set
            {
                map = value;
            }
        }

        public Map BoundryLayer
        {
            get
            {
                return map;
            }
        }

        public VoxelGridSurface surfacePrefab;

        [SerializeField]
        [HideInInspector]
        VoxelGridSurface surface;

        public VoxelGridWall wallPrefab;

        [SerializeField]
        [HideInInspector]
        VoxelGridWall wall;

        [SerializeField]
        [HideInInspector]
        int minChunkSize = 8;

        public int Resolution
        {
            get;
            private set;
        }

        public VoxelGrid xNeighbor
        {
            get
            {
                return surface.xNeighbor;
            }
            set
            {
                surface.xNeighbor = value;
            }
        }

        public VoxelGrid yNeighbor
        {
            get
            {
                return surface.yNeighbor;
            }
            set
            {
                surface.yNeighbor = value;
            }
        }

        public VoxelGrid xyNeighbor
        {
            get
            {
                return surface.xyNeighbor;
            }
            set
            {
                surface.xyNeighbor = value;
            }
        }

        public void Init(int resolution)
        {
            Resolution = resolution;

            //create empty map
            //Map.UnloadMap(ref map);

            map.Clear();


            //map = Map.EmptyMap;

            //of the chunk size
            Resolution = Mathf.Max(minChunkSize, Resolution);

            GridData.Size = (new Vector2Int(Resolution, Resolution));

            //create the surface components
            if(wallPrefab != null)
            {
                wall = Instantiate(wallPrefab, transform, false) as VoxelGridWall;
                wall.Init(Resolution);
            }

            surface = Instantiate(surfacePrefab, transform, false) as VoxelGridSurface;
            surface.Init(Resolution, wall);
        }

        bool debugInvert = false;
        bool debugHasCollisionMesh = false;

        public virtual void GenerateGridData(bool invert = false, bool generate_collision_mesh = true)
        {
            debugHasCollisionMesh = generate_collision_mesh;
            debugInvert = invert;
            float perlinRes = 4.0f;

            Map boundry_layer = BoundryLayer;

            float res = perlinRes;
            for(int i = 0; i < boundry_layer.Count; ++i)
            {
                MapElement m = boundry_layer[i];
                Vector2 pos = boundry_layer.GetPositionFromIndex(i);
                float p = Mathf.PerlinNoise(transform.localPosition.x / boundry_layer.Size.x * res + pos.x / boundry_layer.Size.x * res, transform.localPosition.z / boundry_layer.Size.y * res + pos.y / boundry_layer.Size.y * res);
                int v = (int)Mathf.Round(p);
                v = (int)Mathf.Clamp01(v);

                if(invert)
                {
                    if(v == 0)
                        m.id = Color.white;
                    else
                        m.id = Color.clear;
                }
                else
                {
                    if(v == 1)
                        m.id = Color.white;
                    else
                        m.id = Color.clear;
                }
            }
        }

        public void GenerateGridMesh()
        {
            Refresh(BoundryLayer, debugHasCollisionMesh);
        }

        public virtual void Refresh(Map mesh_input, bool generate_collision_mesh = true)
        {
            Triangulate(mesh_input, generate_collision_mesh);
        }

        void Triangulate(Map mesh_input, bool generate_collision_mesh = true)
        {
            surface.Clear();

            surface.FillFirstRowCache(mesh_input);

            surface.TriangulateRows(mesh_input);

            surface.Apply(generate_collision_mesh);
        }

        void OnApplicationQuit()
        {
            //Map.UnloadMap(ref map);
            map.Clear();
        }

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            Refresh(BoundryLayer, debugHasCollisionMesh);
        }

        [ContextMenu("StartTest")]
        public void StartTest()
        {
            StartCoroutine(TestUpdateArea(BoundryLayer));
        }

        [ContextMenu("StopTest")]
        public void StopTest()
        {
            StopAllCoroutines();
        }

        IEnumerator TestUpdateArea(Map boundry_layer)
        {
            float perlinRes = 4.0f;
            float res = perlinRes;
            float x = 0;
            while(true)
            {
                x += Time.deltaTime;
                if(x > (boundry_layer.Size.x * res))
                    x -= boundry_layer.Size.x * res;

                for(int i = 0; i < boundry_layer.Count; ++i)
                {
                    MapElement m = boundry_layer[i];
                    //Vector2 pos = m.pos;
                    Vector2 pos = boundry_layer.GetPositionFromIndex(i);
                    float p = Mathf.PerlinNoise(transform.localPosition.x / boundry_layer.Size.x * res + x + pos.x / boundry_layer.Size.x * res, transform.localPosition.z / boundry_layer.Size.y * res + pos.y / boundry_layer.Size.y * res);
                    int v = (int)Mathf.Round(p);
                    v = (int)Mathf.Clamp01(v);
                    if(debugInvert)
                    {
                        if(v == 0)
                            m.id = Color.white;
                        else
                            m.id = Color.clear;
                    }
                    else
                    {
                        if(v == 1)
                            m.id = Color.white;
                        else
                            m.id = Color.clear;
                    }
                }
                Refresh(boundry_layer, debugHasCollisionMesh);
                //yield return null;
                yield return new WaitForEndOfFrame();
            }
        }
    }
}