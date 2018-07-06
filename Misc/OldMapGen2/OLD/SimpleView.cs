using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using nv;


#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(SimpleView))]
public class SimpleViewEditor : Editor
{
    SimpleView _target;
    public bool showDefaultInspector = true;

    public override void OnInspectorGUI()
    {
        _target = (SimpleView)target;

        if( GUILayout.Button( "Generate Level" ) )
        {
            _target.TestGenerate();
        }

        showDefaultInspector = EditorGUILayout.Foldout( showDefaultInspector, "Default Inspector");

        if( showDefaultInspector )
        {
            base.OnInspectorGUI();
        }
    }
}
#endif


public class SimpleView : MonoBehaviour
{
    public Generator data;



    public Transform root;
    public Color32 start;

    public GameObject player;

    [System.Serializable]
    public class GameElement
    {
        public Color32 type;
        public GameObject prefab;
        [HideInInspector]
        public GameObject root;
        public bool combineMeshes;
    }

    public List<GameElement> mappings = new List<GameElement>();

    void Awake()
    { 
        for( int i = 0; i < mappings.Count; ++i )
        {
            if( mappings[i].root == null )
            {
                mappings[ i ].root = new GameObject( "[Root] "+ mappings[ i ].prefab.name);
                mappings[ i ].root.transform.SetParent( root );
            }
        }
    }

    [ContextMenu("GenerateView")]
    public void TestGenerate() 
    {
        if( data == null )
            return;

        data.GenerateMap();

        for(int i = 0; i < data.map.Count; ++i )
        {
            GenerateLayer( data.map[i] );
        }

        for( int i = 0; i < mappings.Count; ++i )
        {
            if( mappings[ i ].combineMeshes )
            {
                MeshFilter[] meshFilters = mappings[ i ].root.GetComponentsInChildren<MeshFilter>();
                if( meshFilters.Length <= 0 )
                    continue;

                Material mat = mappings[ i ].root.GetComponentInChildren<MeshRenderer>().sharedMaterial;
                //MeshFilter mesh = 
                    mappings[ i ].root.AddComponent<MeshFilter>();
                MeshRenderer mr = mappings[ i ].root.AddComponent<MeshRenderer>();

                CombineInstance[] combine = new CombineInstance[meshFilters.Length];
                int j = 0;
                mr.sharedMaterial = mat;
                while( j < meshFilters.Length )
                {
                    combine[ j ].mesh = meshFilters[ j ].sharedMesh;
                    combine[ j ].transform = meshFilters[ j ].transform.localToWorldMatrix;
                    j++;
                }
                mappings[ i ].root.transform.GetComponent<MeshFilter>().mesh = new Mesh();
                mappings[ i ].root.transform.GetComponent<MeshFilter>().mesh.CombineMeshes( combine );
                //MeshCollider mc = 
                    mappings[ i ].root.AddComponent<MeshCollider>();

                j = 0;
                while( j < meshFilters.Length )
                {
                    Destroy( meshFilters[ j ].gameObject );
                    j++;
                }
            }
        }

        if( player != null )
        {
            List<MapElement> start_spots = data.map[1].GetElementsOfType(start);
            if( start_spots.Count > 0 )
            {
                player.transform.position = Dev.VectorXZ(start_spots[0].position, 2.0f);
            }            
        }
    }

    GameElement GetGameElement( Color type )
    {
        for(int i = 0; i < mappings.Count; ++i )
        {
            if( mappings[i].type == type )
                return mappings[i];
        }
        return null;
    }

    void GenerateLayer(MapLayer layer)
    {
        for(int i = 0; i < layer.Count; ++i )
        { 
            MapElement m = layer[i];
            if(m.id == Color.clear)
                continue;

            GameElement type = GetGameElement(m.id);
            if( type == null )
                continue;

            Vector3 p = Dev.VectorXZ(m.pos, type.prefab.transform.localPosition.y);

            GameObject.Instantiate(type.prefab, p, type.prefab.transform.localRotation, type.root.transform );
        }
    }
}
