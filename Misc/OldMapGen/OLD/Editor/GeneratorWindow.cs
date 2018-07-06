using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public partial class GeneratorWindow : EditorWindow
{
    //Hotkey: Ctrl + shift + alt + W
    [MenuItem( "K2/Level Generator %#&w" )]
    static void OpenGeneratorWindow()
    {
        CreateWindow();
    }

    static void CreateWindow()
    { 
        GeneratorWindow window = ScriptableObject.CreateInstance<GeneratorWindow>();
        window.titleContent = new GUIContent( "Level Generator" );
        window.Show();
    }

    Generator[] genDropdown;
    int genDrowndownIndex = 0;

    RNG[] rngDropdown;
    int rngDrowndownIndex = 0;

    public void OnEnable()
    {
        //pre-load generators and rng sources in the scene in case the user wants to use them
        genDropdown = GameObject.FindObjectsOfType<Generator>();
        rngDropdown = GameObject.FindObjectsOfType<RNG>();
            
        if( genDropdown.Length > 0 )
            mapGenerator = genDropdown[0];
        if( rngDropdown.Length > 0 )
            rngSource = rngDropdown[ 0 ];
    }

    [SerializeField]
    Map currentMap;

    void OnGUI()
    {
        Event e = Event.current;
        switch( e.type )
        {
            case EventType.keyDown:
            {
                if( Event.current.keyCode == ( KeyCode.Escape ) )
                {
                    Close();
                }
                break;
            }
        }

        DrawWindow();
    }


    void DrawWindow()
    {
        DrawMapSaveLoadingWindow();

        if( currentMap != null && currentMap.Count <= 0 )
            UnloadCurrentMap();

        if( currentMap == null )
            return;

        if( currentLayer == null )
            currentLayer = currentMap.First;

        DrawMapWindow();
        DrawLayerWindow();

        DrawFooter();
    }

    void DrawFooter()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.BeginVertical();

        //EditorGUILayout.LabelField( "Layer Data" );
        //{
        //    EditorGUILayout.IntField( "Empty Elements", cachedEmptyCount );
        //    EditorGUILayout.IntField( "Filled Elements", cachedFilledCount );
        //}

        EditorGUILayout.EndVertical();
    }

    void OnDestroy()
    {
        UnloadCurrentMap();
        System.GC.Collect(2);
    }
}
