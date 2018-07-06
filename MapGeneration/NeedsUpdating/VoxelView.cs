using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace nv
{
    public class VoxelView : MonoBehaviour
    {
        public bool centerCollisionsOnly = false;

        public void OnValidate()
        {
            chunkSize = Mathf.ClosestPowerOfTwo(chunkSize);
            if(visibleAreaSize.x <= 0)
                visibleAreaSize = new Vector2(1, visibleAreaSize.y);
            if(visibleAreaSize.y <= 0)
                visibleAreaSize = new Vector2(visibleAreaSize.x, 1);
        }

        [Header("Gameobject to parent chunks to")]
        public GameObject chunkRoot;

        [Header("Chunk template")]
        public VoxelGrid chunkPrefab;

        [Header("Size of a chunk")]
        public int chunkSize = 64;

        [Header("Number of chunks to render")]
        public Vector2 visibleAreaSize;

        [Header("Seed to use before generation of chunks begins")]
        public bool useCustomSeed;
        public int customSeed;

        public bool debugInvertSurface = false;

        //TODO: allow multiple RNGs and store one per view 
        public void InitRNG()
        {
            if(useCustomSeed)
                GameRNG.Seed = customSeed;
            else
                GameRNG.Generator.Reset();
        }

        public float ChunkSize
        {
            get
            {
                return Mathf.ClosestPowerOfTwo(chunkSize);
            }
        }

        [SerializeField]
        [HideInInspector]
        VoxelGrid[] chunks;

        //Direct access, no bounds checking
        public VoxelGrid this[Vector2 p]
        {
            get
            {
                return this[(int)p.x, (int)p.y];
            }
            set
            {
                this[(int)(p.x), (int)(p.y)] = value;
            }
        }

        //Direct access, no bounds checking
        public VoxelGrid this[int x, int y]
        {
            get
            {
                return chunks[(y * (int)visibleAreaSize.x + x)];
            }
            set
            {
                chunks[(y * (int)visibleAreaSize.x + x)] = value;
            }
        }

        public void Awake()
        {
            CreateChunks();
        }

        public void CreateChunks()
        {
            int centerX = (int)(visibleAreaSize.x / 2);
            int centerY = (int)(visibleAreaSize.y / 2);

            //first create chunks and link up neighbors
            chunks = new VoxelGrid[(int)visibleAreaSize.x * (int)visibleAreaSize.y];
            for(int i = 0, y = 0; y < (int)visibleAreaSize.y; y++)
            {
                for(int x = 0; x < (int)visibleAreaSize.x; x++, i++)
                {
                    CreateChunk(i, x, y);
                }
            }

            //init RNG
            InitRNG();

            //generate grid data
            for(int y = 0; y < (int)visibleAreaSize.y; y++)
            {
                for(int x = 0; x < (int)visibleAreaSize.x; x++)
                {
                    if(centerCollisionsOnly)
                        this[x, y].GenerateGridData(debugInvertSurface, (y == centerY && x == centerX));
                    else
                        this[x, y].GenerateGridData(debugInvertSurface);
                }
            }

            //then create the meshes from data
            for(int y = (int)visibleAreaSize.y - 1; y >= 0; y--)
            {
                for(int x = (int)visibleAreaSize.x - 1; x >= 0; x--)
                {
                    this[x, y].GenerateGridMesh();
                }
            }
        }

        private void CreateChunk(int i, int x, int y)
        {
            Vector2 chunkPos = new Vector2(x, y) * ChunkSize;
            Vector3 worldChunkPos = chunkPos.SetY(0f);

            VoxelGrid chunk = Instantiate(chunkPrefab, transform) as VoxelGrid;
            chunk.transform.localPosition = worldChunkPos;
            chunk.Init((int)ChunkSize);
            chunks[i] = chunk;
            chunk.gameObject.name = "Chunk " + i;

            if(x > 0)
            {
                this[x - 1, y].xNeighbor = chunk;
            }
            if(y > 0)
            {
                this[x, y - 1].yNeighbor = chunk;
                if(x > 0)
                {
                    this[x - 1, y - 1].xyNeighbor = chunk;
                }
            }

        }
    }
}