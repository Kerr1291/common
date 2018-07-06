using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor( typeof( CubeTeam ) )]
public class CubeTeamEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CubeTeam _target = (CubeTeam)target;

        string previous = _target.gameObject.tag;
        _target.gameObject.tag = EditorGUILayout.TagField("Cube Team:", _target.gameObject.tag);
        if( previous != _target.gameObject.tag && _target.explodeLogic != null )
        {
            _target.ChangeTeam( previous );
        }

        base.OnInspectorGUI();
    }
}
#endif


public class CubeTeam : MonoBehaviour
{
    public CubeBoid cube;
    public ExplodeOnContact explodeLogic;

    public void ChangeTeam(string previous = "")
    {
        if( cube != null )
        {
            cube.body.gameObject.tag = gameObject.tag;
        }

        if( explodeLogic != null )
        {
            //remove previous
            if( previous != string.Empty )
                explodeLogic.ignoreTags.Remove( previous );

            //add new team tag
            if( explodeLogic.ignoreTags.Contains( gameObject.tag ) == false )
                explodeLogic.ignoreTags.Add(gameObject.tag);
        }
    }
}
