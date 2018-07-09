using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

//using Map = nv.ArrayGrid<nv.MapElement>;

namespace nv
{
    public class MapMesh : ScriptableObject
    {
        public GameObject RenderData
        {
            get; private set;
        }

        public Transform WorldLocation
        {
            get; private set;
        }

        [SerializeField]
        [HideInInspector]
        MapSurfaceMesh surface;

        [SerializeField]
        [HideInInspector]
        MapWallMesh wall;
        
        public int ChunkSize
        {
            get; private set;
        }

        public MapMesh xNeighbor
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

        public MapMesh yNeighbor
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

        public MapMesh xyNeighbor
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

        public Vector2Int ChunkIndex
        {
            get; private set;
        }

        public void Init(GameObject root, int chunkSize, Vector2Int chunkIndex)
        {
            //and finish this
            ChunkSize = chunkSize;
            ChunkIndex = chunkIndex;
            WorldLocation = root.transform;

            Vector2Int chunkPos = chunkIndex * ChunkSize;
            Vector3 worldChunkPos = new Vector3(chunkPos.x, 0f, chunkPos.y);
            WorldLocation.localPosition = worldChunkPos;
            
            GameObject surfaceRoot = new GameObject("Surface Mesh");
            GameObject wallRoot = new GameObject("Wall Mesh");

            //create the mesh components
            wall.Init(wallRoot, ChunkSize);            
            surface.Init(surfaceRoot, ChunkSize, wall);
        }

        public void GenerateMesh(ProcGenMap sourceData, bool generateCollisionMesh = true)
        {
            surface.Clear();

            surface.FillFirstRowCache(sourceData);

            surface.TriangulateRows(sourceData);

            surface.Apply(generateCollisionMesh);
        }
    }
}