using UnityEngine;
using System.Collections;

public class InputActionContainer : MonoBehaviour
{
    [HideInInspector]
    public InputAction[] inputActions;

    public void UpdateAttachedActions()
    {
        inputActions = GetComponents<InputAction>();
    }

    void Reset()
    {
        inputActions = GetComponents<InputAction>();
    }

    void Awake()
    {
        inputActions = GetComponents<InputAction>();
    }
}
