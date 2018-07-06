using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

public partial class GeneratorWindow
{ 
    public static string GetLevelAssetFile( string level_name )
    {
        return K2FilePath.GetGameFilePath( level_name + "_level.asset" );
    }

    void CreateEmptyMap()
    {
        UnloadCurrentMap(); 
        currentMap = Map.EmptyMap;
        currentLayer = currentMap.First;
    }

    void UnloadCurrentMap()
    {
        if( mapGenerator != null )
        {
            if( mapGenerator.map == currentMap )
                mapGenerator.map = null;
        }

        currentLayer = null;

        if( currentMap != null )
        {
            currentMap.Clear();

            if( Application.isEditor == true )
                DestroyImmediate( currentMap );
            else
                Destroy( currentMap );
        }

        currentMap = null;
        System.GC.Collect(2);
    }

    void GenerateMap()
    {
        UnloadCurrentMap();
        RNG.Instance = rngSource;
        mapGenerator.GenerateMap();
        currentMap = mapGenerator.map;
        currentLayer = currentMap.First;
        System.GC.Collect(2);
    }

    void Button_CreateEmptyMap()
    {
        if( GUILayout.Button( "Create Empty Map" ) )
        {
            if( currentMap != null )
            {
                if( EditorUtility.DisplayDialog( "Confirm new map", "Are you sure? This will unload the current map", "Ok", "Cancel" ) )
                {
                    CreateEmptyMap();
                }
            }
            else
            {
                CreateEmptyMap();
            }
        }
    }

    void Button_UnloadCurrentMap()
    {
        if( currentMap != null )
        {
            if( GUILayout.Button( "Unload Current Map" ) )
            {
                if( EditorUtility.DisplayDialog( "Confirm unload map", "Are you sure? This will unload the current map", "Ok", "Cancel" ) )
                {
                    UnloadCurrentMap();
                }
            }
        }
    }

    void Button_GenerateNewMap()
    {
        if( mapGenerator != null && rngSource != null )
        {
            if( GUILayout.Button( "Generate Map" ) )
            {
                GenerateMap();
            }
        }
    }

    [SerializeField]
    Generator mapGenerator;

    [SerializeField]
    RNG rngSource;

    void Field_MapGenerator()
    {
        EditorGUILayout.BeginHorizontal();
        mapGenerator = (Generator)EditorGUILayout.ObjectField("Generator:",mapGenerator, typeof(Generator),true);

        if( genDropdown.Length > 0 )
        {
            int prev = genDrowndownIndex;
            genDrowndownIndex = EditorGUILayout.Popup( genDrowndownIndex, genDropdown.Select( x => x.name ).ToArray() );
            if( prev != genDrowndownIndex )
            {
                mapGenerator = genDropdown[ genDrowndownIndex ];
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    void Field_RNGSource()
    {
        rngSource = (RNG)EditorGUILayout.ObjectField( "RNG:", rngSource, typeof( RNG ), true );

        if( genDropdown.Length > 0 )
        {
            int prev = rngDrowndownIndex;
            rngDrowndownIndex = EditorGUILayout.Popup( rngDrowndownIndex, rngDropdown.Select( x => x.name ).ToArray() );
            if( prev != rngDrowndownIndex )
            {
                rngSource = rngDropdown[ rngDrowndownIndex ];
            }
        }
    }

    void DrawMapSaveLoadingWindow()
    {
        EditorGUILayout.BeginHorizontal();
        Field_MapGenerator();
        Field_RNGSource();
        EditorGUILayout.EndHorizontal();

        Button_GenerateNewMap();
        //Button_CreateEmptyMap();
        Button_UnloadCurrentMap();
        EditorGUILayout.Separator();

        //loadLevelString = EditorGUILayout.TextField( "Level to load:", loadLevelString );

        //if( GUILayout.Button( "Load Level" ) )
        //{
        //    if( loadedData != null )
        //    {
        //        if( EditorUtility.DisplayDialog( "Confirm load level", "Are you sure? This will unload the current level", "Ok", "Cancel" ) )
        //        {
        //            LoadLevel( GetLevelAssetFile( loadLevelString ) );
        //        }
        //    }
        //    else
        //    {
        //        LoadLevel( GetLevelAssetFile( loadLevelString ) );
        //    }
        //}
    }


    //bool LoadLevel(string file_and_path)
    //{
    //    if( File.Exists( file_and_path ) )
    //    {
    //        BinaryFormatter bf = new BinaryFormatter();
    //        FileStream file = File.Open( file_and_path, FileMode.Open );
    //        GeneratorData load_data = (GeneratorData)bf.Deserialize( file );
    //        file.Close();

    //        loadedData = load_data;
    //        if( loadedData.layers.Count > 0 )
    //            currentLayer = loadedData.layers[0];
    //        if( currentLayer != null )
    //            UpdateLayerTexture();
    //    }
    //    else
    //        return false;

    //    return true;
    //} 

    //void SaveLevel( string file_and_path )
    //{
    //    if( loadedData == null )
    //        return;

    //    BinaryFormatter bf = new BinaryFormatter();
    //    FileStream file = File.Create( file_and_path );
    //    GeneratorData value = loadedData;
    //    bf.Serialize( file, value );
    //    file.Close();
    //}
}
