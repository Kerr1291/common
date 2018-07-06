using UnityEngine;
using System.Collections;

public class SimpleMovement : MonoBehaviour {

    public GameObject model;

    public Rigidbody body;

    public float movePower = 100.0f;

    public void Move( Vector2 dir )
    {
        if( body == null )
            return;

        Vector3 moveVector = Vector3.zero;
        moveVector.x = dir.x;
        moveVector.z = dir.y;
        moveVector *= movePower * Time.fixedDeltaTime;

        body.AddForce( moveVector, ForceMode.VelocityChange );
    }
    void Update()
    {
        if( body.velocity.magnitude > 0.1f )
        {
            Vector3 look = body.velocity.normalized;
            look.y = 0.0f; 

            Quaternion q = Quaternion.LookRotation( look, Vector3.up );
            model.transform.rotation = q;
        }
    }
}
