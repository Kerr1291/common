using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
[CustomEditor(typeof( BoidManager))]
public class BoidManagerEditor : Editor
{
    BoidManager _target;

    public override void OnInspectorGUI()
    {
        _target = ( BoidManager )target;

        if( GUILayout.Button("Add Empty Rule") )
        {
            _target.keys.Add("Empty" );
            _target.values.Add( null );
        }

        for(int i = 0; i < _target.keys.Count; ++i )
        {
            string prev = _target.keys[i];

            EditorGUILayout.BeginHorizontal();
            string key = EditorGUILayout.TagField("Tag", prev);
            BoidRules value = (BoidRules)EditorGUILayout.ObjectField("BoidRules", _target.values[i], typeof(BoidRules), true);
            EditorGUILayout.EndHorizontal();

            if( prev != key )
            {
                _target.keys.RemoveAt( i );
                _target.values.RemoveAt( i );
            }

            _target.keys[ i ] = key;
            _target.values[ i ] = value;
        }

        //foreach( var b in _target.boidRules )
        //{
        //    string prev = b.Key;

        //    EditorGUILayout.BeginHorizontal();
        //    string key = EditorGUILayout.TagField("Tag", b.Key);
        //    BoidRules value = (BoidRules)EditorGUILayout.ObjectField("BoidRules", b.Value, typeof(BoidRules), true);
        //    EditorGUILayout.EndHorizontal();

        //    if( prev != key )
        //    {
        //        _target.boidRules.Remove( prev );
        //    }

        //    _target.boidRules[ key ] = value;
        //}

        base.OnInspectorGUI();
    }
}
#endif

public class BoidManager : MonoBehaviour
{
    [HideInInspector]
    public List<string> keys;

    [HideInInspector]
    public List<BoidRules> values;

    public Dictionary<string,BoidRules> boidRules = new Dictionary<string,BoidRules>();
    
    public static BoidManager Instance { get; private set; }

    public BoidRules GetBoidRules( string tag )
    {
        return boidRules[tag];
    }

    void Awake()
    {
        Instance = this;

        for(int i = 0; i < keys.Count; ++i )
        {
            boidRules.Add(keys[i],values[i]);
        }
    }
}