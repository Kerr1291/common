using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class DestroyOnContact : MonoBehaviour
{
    void OnCollisionEnter( Collision c )
    {
        Destroy(c.collider.gameObject);
    }
}