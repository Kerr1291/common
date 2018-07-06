using UnityEngine;
using System.Collections;

public class HumanoidMovement : BaseMovement
{
    public Rigidbody body;

    public float stoppingVelocity = 0.1f;

    public Animator runAnimatior;

    public string runAnimatorProperty;

    RunSpeedStat _run_speed;
    
    bool CheckCanRun()
    {
        if( body == null )
            return false;

        if( _run_speed == null )
        {
            _run_speed = stats.GetComponent<RunSpeedStat>();

            if( _run_speed == null )
                return false;
        }

        return true;
    }

    public override void Move( Vector2 dir )
    {
        if( !CheckCanRun() )
            return;

        float movePower = _run_speed.RunSpeed;

        Vector3 moveVector = Vector3.zero;
        moveVector.x = dir.x;
        moveVector.z = dir.y;
        moveVector *= movePower * Time.fixedDeltaTime;

        body.AddForce( moveVector, ForceMode.VelocityChange );
    }

    void Update()
    {
        Vector3 xzvelocity = body.velocity;
        xzvelocity.y = 0.0f;
        if( xzvelocity.magnitude > stoppingVelocity )
        {
            Vector3 look = body.velocity.normalized;
            look.y = 0.0f;

            if( look != Vector3.zero && look.magnitude > Mathf.Epsilon )
            {
                Quaternion q = Quaternion.LookRotation( look, Vector3.up );
                model.transform.rotation = q;
            }
        }

        CalculateRunAnimation();
    }

    public void CalculateRunAnimation()
    {
        if( runAnimatior == null )
            return;

        runAnimatior.SetFloat( runAnimatorProperty, body.velocity.magnitude );
    }
}
