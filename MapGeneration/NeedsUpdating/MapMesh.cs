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

        [EditScriptable]
        public MapSurfaceMesh surfaceDefinition;

        [EditScriptable]
        public MapWallMesh wallDefinition;

        ProcGenMap mapData;

        public int ChunkSize
        {
            get; private set;
        }

        public MapMesh xNeighbor
        {
            get
            {
                return surfaceDefinition.xNeighbor;
            }
            set
            {
                surfaceDefinition.xNeighbor = value;
            }
        }

        public MapMesh yNeighbor
        {
            get
            {
                return surfaceDefinition.yNeighbor;
            }
            set
            {
                surfaceDefinition.yNeighbor = value;
            }
        }

        public MapMesh xyNeighbor
        {
            get
            {
                return surfaceDefinition.xyNeighbor;
            }
            set
            {
                surfaceDefinition.xyNeighbor = value;
            }
        }

        public Vector2Int ChunkIndex
        {
            get; private set;
        }

        public float ChunkScale
        {
            get; private set;
        }

        ArrayGrid<MapElement> subMap;
        public ArrayGrid<MapElement> SubMap
        {
            get
            {
                if(subMap == null)
                {
                    Vector2Int sourceAreaPos = ChunkIndex * ChunkSize; //TODO: add an offset for the "current position" on the map to render from
                    Vector2 sourceAreaSize = new Vector2(ChunkSize, ChunkSize) * ChunkScale;
                    Vector2Int chunkMapSize = new Vector2Int(ChunkSize, ChunkSize);

                    Debug.Log("CI " + ChunkIndex);
                    subMap = mapData.GeneratedMap.MapToSubGrid(sourceAreaPos, Vector2Int.FloorToInt(sourceAreaSize), chunkMapSize);
                }
                return subMap;
            }
        }

        public void Init(ProcGenMap map, GameObject root, int chunkSize, Vector2Int chunkIndex, float chunkScale)
        {
            mapData = map;
            ChunkScale = chunkScale;

            //and finish this
            ChunkSize = chunkSize;
            ChunkIndex = chunkIndex;
            WorldLocation = root.transform;

            Vector2Int chunkPos = chunkIndex * ChunkSize;
            Vector3 worldChunkPos = new Vector3(chunkPos.x, 0f, chunkPos.y);
            WorldLocation.localPosition = worldChunkPos;

            GameObject surfaceRoot = new GameObject("Surface Mesh " + ChunkIndex);
            GameObject wallRoot = new GameObject("Wall Mesh " + ChunkIndex);

            surfaceRoot.transform.SetParent(root.transform);
            surfaceRoot.transform.SetParent(root.transform);

            //create the mesh components
            wallDefinition.Init(wallRoot, ChunkSize);
            surfaceDefinition.Init(surfaceRoot, ChunkSize, wallDefinition);
        }

        public void GenerateMesh(bool generateCollisionMesh = true)
        {
            surfaceDefinition.Clear();

            surfaceDefinition.FillFirstRowCache(SubMap);

            surfaceDefinition.TriangulateRows(SubMap);

            surfaceDefinition.Apply(generateCollisionMesh);
        }
    }
}