using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class SyncTransform : MonoBehaviour
{
    public Transform syncTarget;

    void LateUpdate()
    {
        syncTarget.position = transform.position;
        transform.localPosition = Vector3.zero;
    }
}