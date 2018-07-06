using UnityEngine;
using System.Collections;

public class ConstantForwardVelocity : MonoBehaviour {

    public float velocity;

    public Rigidbody body;

    void Update()
    {
        if( body != null )
            body.velocity = transform.forward * velocity;
    }
}
