using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace nv
{
    public class MapRenderer : MonoBehaviour
    {
        public bool debugInvertSurface = false;
        public bool generateCollisionMesh = false;

        [EditScriptable]
        public ProcGenMap mapData;
        
        [EditScriptable]
        public MapMesh chunkDefinition;

        [Header("Number of chunks to render")]
        public Vector2Int visibleArea;

        [Header("Size of a chunk")]
        public int chunkSize = 64;
        public int ChunkSize
        {
            get
            {
                return Mathf.ClosestPowerOfTwo(chunkSize);
            }
        }

        public float chunkScale = 1f;

        [SerializeField, HideInInspector]
        List<MapMesh> chunks;

        List<GameObject> chunkViews;

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
                return chunks[(y * visibleArea.x + x)];
            }
            set
            {
                chunks[(y * visibleArea.x + x)] = value;
            }
        }

        IEnumerator Start()
        {
            yield return mapData.Generate();
            yield return CreateChunks();
            yield return RenderChunks();
        }

        public IEnumerator CreateChunks()
        {
            Vector2Int center = Vector2Int.FloorToInt(new Vector2(visibleArea.x, visibleArea.y) * .5f);

            chunks = new List<MapMesh>();
            chunkViews = new List<GameObject>();

            var visibleIter = Mathnv.GetAreaEnumerator(visibleArea);
            while(visibleIter.MoveNext())
            {
                Vector2Int current = visibleIter.Current;
                CreateChunk(current);
            }

            yield break;
        }

        public IEnumerator RenderChunks()
        {
            var visibleIter = Mathnv.GetAreaEnumerator(visibleArea);
            while(visibleIter.MoveNext())
            {
                Vector2Int current = visibleIter.Current;                
                this[current].GenerateMesh(generateCollisionMesh);
            }
            yield break;
        }

        void CreateChunk(Vector2Int chunkIndex)
        {
            MapMesh chunk = Instantiate(chunkDefinition) as MapMesh;
            chunk.name = "Chunk " + chunks.Count + " " + chunkIndex;
            GameObject chunkRoot = new GameObject(chunk.name + " root");
            chunkRoot.transform.SetParent(transform);
            chunk.Init(mapData, chunkRoot, ChunkSize, chunkIndex, chunkScale);
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

        public void OnValidate()
        {
            chunkSize = Mathf.ClosestPowerOfTwo(chunkSize);
            if(visibleArea.x <= 0)
                visibleArea.x = 1;
            if(visibleArea.y <= 0)
                visibleArea.y = 1;
        }
    }
}