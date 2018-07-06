using UnityEngine;
using UnityEditor;
using System.Collections;

public partial class GeneratorWindow
{
    Vector2 renderLayerSettingsScrollPos;

    void DrawSettingsWindow()
    { 
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField( "Settings" );
        EditorGUILayout.Separator();
        renderLayerSettingsScrollPos = EditorGUILayout.BeginScrollView( renderLayerSettingsScrollPos, false, true );
        {

            int junk = 0;
            EditorGUILayout.IntField( "Placeholder", junk );
            EditorGUILayout.IntField( "Placeholder", junk );
            EditorGUILayout.IntField( "Placeholder", junk );
            EditorGUILayout.IntField( "Placeholder", junk );
            EditorGUILayout.IntField( "Placeholder", junk );
            EditorGUILayout.IntField( "Placeholder", junk );
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }
}
