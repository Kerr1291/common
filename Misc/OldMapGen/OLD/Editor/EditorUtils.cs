using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EditorUtils
{
    public static void DelayedVector2Field( string label, ref Vector2 field_data )
    { 
        EditorGUILayout.BeginHorizontal();
        field_data.x = EditorGUILayout.DelayedFloatField( label + " X: ", field_data.x );
        field_data.y = EditorGUILayout.DelayedFloatField( "Y: ", field_data.y );
        EditorGUILayout.EndHorizontal();
    }
}
