using UnityEngine;
using UnityEditor;
using System.Collections;

public partial class GeneratorWindow
{
    void AddEmptyLayer()
    {
        MapLayer new_layer = MapLayer.EmptyLayer;
        new_layer.ResizeLayer( currentLayer.w, currentLayer.h );
        currentMap.Add( new_layer );
        GenerateCurrentLayerTexture();
    }

    void RemoveCurrentLayer()
    { 
        if( currentMap == null || currentMap.Count <= 1 )
            return;

        currentMap.Remove( currentLayer );
        currentLayer = currentMap.First;
    }

    void Button_AddLayer()
    {
        if( GUILayout.Button( "Add Layer" ) )
        {
            AddEmptyLayer();
        }
    }

    void Button_RemoveLayer()
    {
        if( GUILayout.Button( "Remove Layer" ) )
        {
            RemoveCurrentLayer();
        }
    }

    void Button_PrevLayer()
    {
        if( GUILayout.Button( "Prev" ) )
        {
            currentLayer = currentMap.GetPrevLayer( currentLayer );
            GenerateCurrentLayerTexture();
        }
    }

    void Button_NextLayer()
    {
        if( GUILayout.Button( "Next" ) )
        {
            currentLayer = currentMap.GetNextLayer( currentLayer );
            GenerateCurrentLayerTexture();
        }
    }

    void Field_MapName()
    {
        currentMap.name = EditorGUILayout.TextField( "Loaded Level: ", currentMap.name );
    }

    void Field_LayerName()
    {
        currentLayer.name = EditorGUILayout.TextField( "Current Layer: ", currentLayer.name );
        EditorGUILayout.LabelField( "Layer Index = " + ( 1 + currentMap.GetLayerIndex( currentLayer ) ) + "/" + currentMap.Count );
    }

    void DrawMapWindow()
    {
        Field_MapName();

        GUILayout.Space( 5 );

        EditorGUILayout.BeginHorizontal();

        Field_LayerName();

        Button_PrevLayer();
        Button_NextLayer();
        Button_AddLayer();
        Button_RemoveLayer();

        EditorGUILayout.EndHorizontal();

        GUILayout.Space( 5 );
    }
}
