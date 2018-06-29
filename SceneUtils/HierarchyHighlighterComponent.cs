//HierarchyHighlighterComponent.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HierarchyHighlighterComponent : MonoBehaviour
{    
    [Header("Attach this to the gameobject you want to highlight in the hierarchy")]
    public bool highlight = true;
    public Color color = Color.black;

    [TextArea( 4, 20 )]
    [Tooltip( "Space for comments" )]
    public string comment;
}