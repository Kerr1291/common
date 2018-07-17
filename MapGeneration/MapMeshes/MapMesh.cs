using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace nv
{
    //enables a hacky workaround to create a list of nicely editable scriptable objects
    [System.Serializable]
    public class MapMeshDefinition
    {
        [EditScriptable]
        public MapSurfaceMesh surfaceDefinition;
    }

    public class MapMesh : ScriptableObject
    {
        public ArrayGrid<MapElement> MapData { get; private set; }

        [EditScriptableList]
        public List<MapMeshDefinition> surfaceDefinitions;
        public List<MapSurfaceMesh> Surfaces { get; private set; }

        [HideInInspector]
        public MapMesh xNeighbor, yNeighbor, xyNeighbor;

        [HideInInspector]
        public GameObject mapMeshRoot;

        [HideInInspector]
        public Vector3 worldPos;

        [HideInInspector]
        public Vector2Int mapScale;

        public void Init(ArrayGrid<MapElement> map, GameObject root, Vector2Int chunkSize, Vector2Int chunkIndex, Vector2Int mapScale)
        {
            MapData = map;
            this.mapScale = mapScale;

            mapMeshRoot = new GameObject("MapMesh " + chunkIndex);
            mapMeshRoot.transform.SetParent(root.transform);
            
            Vector2Int chunkPos = chunkIndex * chunkSize;
            worldPos = new Vector3(chunkPos.x, 0f, chunkPos.y);

            //create all the surfaces
            Surfaces = new List<MapSurfaceMesh>();
            foreach(var def in surfaceDefinitions)
            {
                MapSurfaceMesh mapMesh = Instantiate(def.surfaceDefinition) as MapSurfaceMesh;

                ////create the mesh components
                mapMesh.Init(this, mapMeshRoot, chunkSize);

                Surfaces.Add(mapMesh);
            }
        }

        public void GenerateMesh(bool generateCollisionMesh = true)
        {
            //generate the meshes
            foreach(var surface in Surfaces)
            {
                surface.Clear();
                surface.FillFirstRowCache(MapData);
                surface.TriangulateRows(MapData);
                surface.Apply(generateCollisionMesh);
            }

            mapMeshRoot.transform.localPosition = worldPos;
        }
    }
}