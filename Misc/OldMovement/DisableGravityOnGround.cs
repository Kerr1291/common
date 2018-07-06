using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class DisableGravityOnGround : MonoBehaviour
{
    public Rigidbody bodyToControl;

    public Raycaster groundDetection;

    void Update()
    {
        if( groundDetection.RaycastHasHit() )
            bodyToControl.useGravity = false;
        else
            bodyToControl.useGravity = true;
    }
}