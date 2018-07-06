using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor( typeof( LeakTester ) )]
public class LeakTesterEditor : Editor
{
    LeakTester _target;
    public bool showDefaultInspector = false;

    int numToAdd = 1;
    int numToRemove = 1;
    
    Vector2 size;

    public override void OnInspectorGUI()
    {
        _target = (LeakTester)target;

        if( GUILayout.Button( "GC Collect" ) )
        {
            System.GC.Collect(2);
        }

        size = EditorGUILayout.Vector2Field( "Resize things", size );
        if( GUILayout.Button( "Reize things" ) )
        {
            for( int i = 0; i < numToAdd; ++i )
                _target.things[i].First.ResizeLayer( size );
        }

        if( GUILayout.Button( "Add thing" ) )
        {
            _target.things.Add( Map.EmptyMap );
        }

        numToAdd = EditorGUILayout.IntField( "Multiadd count", numToAdd );

        if( GUILayout.Button( "Add "+ numToAdd + " things" ) )
        {
            for(int i = 0; i < numToAdd; ++i )
                _target.things.Add( Map.EmptyMap );
        }

        numToRemove = EditorGUILayout.IntField( "Multiremove count", numToRemove );

        if( GUILayout.Button( "Destroy and Remove thing" ) )
        {
            if( _target.things.Count > 0 )
            {
                DestroyImmediate( _target.things[0] );
                _target.things.RemoveAt( 0 );
            }
        }

        if( GUILayout.Button( "Remove and destroy " + numToRemove + " things" ) )
        {
            for( int i = 0; i < numToAdd; ++i )
            {
                if( _target.things.Count > 0 )
                {
                    DestroyImmediate( _target.things[ 0 ] );
                    _target.things.RemoveAt( 0 );
                }
                else
                    break;
            }
        }

        EditorGUILayout.IntField( "Number of things", _target.things.Count );

        showDefaultInspector = EditorGUILayout.Foldout( showDefaultInspector, "Default Inspector" );

        if( showDefaultInspector )
        {
            base.OnInspectorGUI();
        }
    }
}
#endif

public class LeakTester : MonoBehaviour
{
    public List<Map> things;

    public void OnApplicationQuit()
    {
        for( int i = 0; i < things.Count; ++i )
        {
            DestroyImmediate( things[ i ] );
        }
        things.Clear();
    }
}