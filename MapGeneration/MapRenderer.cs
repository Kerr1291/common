using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace nv
{
    public class MapRenderer : MonoBehaviour
    {
        [Header("Call Generate() on mapData on start?")]
        public bool debugGenerate = true;

        [Header("The size (in units) of one unit of map data")]
        public Vector2Int mapScale = Vector2Int.one;

        [Header("Maximum size of a chunk (in units)")]
        public int maxChunkSize = 128;

        [Tooltip("The source map data to render")]
        [EditScriptable]
        public ProcGenMap mapData;

        [Tooltip("Used to generate chunks")]
        [EditScriptable]
        public MapMesh chunkDefinition;

        List<MapMesh> chunks = new List<MapMesh>();
        List<GameObject> chunkViews = new List<GameObject>();

        Vector2Int chunkSize;
        public Vector2Int ChunkSize
        {
            get
            {
                if(mapData == null || mapData.GeneratedMap == null)
                {
                    chunkSize = Vector2Int.zero;
                }
                else if(chunkSize == Vector2Int.zero)
                {
                    Vector2Int s = new Vector2Int(mapData.GeneratedMap.Size.x * mapScale.x, mapData.GeneratedMap.Size.y * mapScale.y);

                    chunkSize.x = GetChunkSize(s.x, maxChunkSize);
                    chunkSize.y = GetChunkSize(s.y, maxChunkSize);
                }
                return chunkSize;
            }
        }
        
        public int Count
        {
            get
            {
                return chunks.Count;
            }
        }

        Vector2Int size;
        public Vector2Int MapSize
        {
            get
            {
                if(mapData == null || mapData.GeneratedMap == null)
                {
                    size = Vector2Int.zero;
                }
                else if(size == Vector2Int.zero)
                {
                    Vector2Int s = new Vector2Int(mapData.GeneratedMap.Size.x * mapScale.x, mapData.GeneratedMap.Size.y * mapScale.y);

                    size.x = GetChunkCount(s.x, maxChunkSize);
                    size.y = GetChunkCount(s.y, maxChunkSize);
                }
                return size;
            }
        }

        int GetChunkSize(int scaledSize, int maxSize)
        {
            int i = 1;
            int s = scaledSize;
            do
            {
                float fs = (float)scaledSize / i++;
                s = Mathf.CeilToInt(fs);
            }
            while(s > maxSize);
            return s;
        }

        int GetChunkCount(int scaledSize, int maxSize)
        {
            int i = 1;
            int s = scaledSize;
            do
            {
                float fs = (float)scaledSize / i++;
                s = Mathf.CeilToInt(fs);
            }
            while(s > maxSize);
            return i - 1;
        }

        //Direct access, no bounds checking
        public MapMesh this[Vector2Int p]
        {
            get
            {
                return this[p.x, p.y];
            }
            set
            {
                this[(p.x), (p.y)] = value;
            }
        }

        //Direct access, no bounds checking
        public MapMesh this[int x, int y]
        {
            get
            {
                return chunks[(y * MapSize.x + x)];
            }
            set
            {
                chunks[(y * MapSize.x + x)] = value;
            }
        }

        IEnumerator Start()
        {
            if(debugGenerate)
            {
                yield return mapData.Generate();
            }

            while(mapData.GeneratedMap == null)
                yield return null;

            yield return CreateChunks();
            yield return RenderChunks();
        }

        public void Clear()
        {
            foreach(var c in chunks)
            {
                Destroy(c);
            }
            foreach(var c in chunkViews)
            {
                Destroy(c);
            }
            chunks.Clear();
            chunkViews.Clear();
        }

        public IEnumerator CreateChunks()
        {
            Vector2Int center = Vector2Int.FloorToInt(new Vector2(MapSize.x, MapSize.y) * .5f);

            chunks = new List<MapMesh>();
            chunkViews = new List<GameObject>();

            var visibleIter = Mathnv.GetAreaEnumerator(MapSize);
            while(visibleIter.MoveNext())
            {
                Vector2Int current = visibleIter.Current;
                CreateChunk(current);
            }

            yield break;
        }

        public IEnumerator RenderChunks()
        {
            var visibleIter = Mathnv.GetAreaEnumerator(MapSize);
            while(visibleIter.MoveNext())
            {
                Vector2Int current = visibleIter.Current;                
                this[current].GenerateMesh(true);
            }
            yield break;
        }

        void CreateChunk(Vector2Int chunkIndex)
        {
            MapMesh chunk = Instantiate(chunkDefinition) as MapMesh;
            chunk.name = "Chunk " + chunks.Count + " " + chunkIndex;
            GameObject chunkRoot = new GameObject(chunk.name + " root");
            chunkRoot.transform.SetParent(transform);
            chunk.Init(CreateChunkMap(chunkIndex), chunkRoot, ChunkSize, chunkIndex);
            chunks.Add(chunk);
            chunkViews.Add(chunkRoot);


            if(chunkIndex.x > 0)
            {
                this[chunkIndex.x - 1, chunkIndex.y].xNeighbor = chunk;
            }
            if(chunkIndex.y > 0)
            {
                this[chunkIndex.x, chunkIndex.y - 1].yNeighbor = chunk;
                if(chunkIndex.x > 0)
                {
                    this[chunkIndex.x - 1, chunkIndex.y - 1].xyNeighbor = chunk;
                }
            }
        }

        ArrayGrid<MapElement> CreateChunkMap(Vector2Int chunkIndex)
        {
            ArrayGrid<MapElement> chunkMap;
            Vector2Int sourceMapSize = new Vector2Int(ChunkSize.x / mapScale.x, ChunkSize.y / mapScale.y);
            Vector2Int sourceAreaPos = new Vector2Int(chunkIndex.x * sourceMapSize.x, chunkIndex.y * sourceMapSize.y);
            Vector2Int chunkMapSize = new Vector2Int(ChunkSize.x, ChunkSize.y);

            chunkMap = mapData.GeneratedMap.MapToSubGrid(sourceAreaPos, sourceMapSize, chunkMapSize);
            return chunkMap;
        }
    }
}