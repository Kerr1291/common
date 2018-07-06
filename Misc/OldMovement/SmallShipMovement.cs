using UnityEngine;
using System.Collections;

public class SmallShipMovement : BaseMovement
{
    public Rigidbody body;

    public float stoppingVelocity = 0.1f;

    EngineStat _engine;

    bool CheckEngine()
    {
        if( body == null )
            return false;

        if( _engine == null )
        {
            _engine = stats.GetComponent<EngineStat>();

            if( _engine == null )
                return false;
        }
        else
        {
            if( _engine.Active == false )
                return false;
        }

        return true;
    }

    public override void Move( Vector2 dir )
    {
        if( !CheckEngine() )
            return;

        float movePower = _engine.ThrustPower;

        Vector3 moveVector = Vector3.zero;
        moveVector.x = dir.x;
        moveVector.z = dir.y;
        moveVector *= movePower * Time.fixedDeltaTime;

        body.AddForce( moveVector, ForceMode.VelocityChange );
    }

    void Update()
    {
        if( body.velocity.magnitude > stoppingVelocity )
        {
            Vector3 look = body.velocity.normalized;
            look.y = 0.0f;

            Quaternion q = Quaternion.LookRotation( look, Vector3.up );
            model.transform.rotation = q;
        }
    }
}
