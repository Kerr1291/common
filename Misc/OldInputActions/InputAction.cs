using UnityEngine;
using System.Collections;

using Rewired;

/// <summary>
/// Base class for all input actions
/// </summary>
public class InputAction : MonoBehaviour 
{
    public virtual string GetActionTypeName() { return "None"; }

    public virtual void DoAction( InputActionEventData player ) { Debug.LogError( "Base class has no action!" ); }

}
