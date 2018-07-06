using UnityEngine;
using System.Collections;

public class ForcePusher : MonoBehaviour
{
    [Header("How hard to push?")]
    public float forcePower = 1.0f;

    [Header("What direction to push?")]
    public Vector3 forceDirection = Vector3.up;
    [Header("Override direction with object's direction?")]
    public bool useForward = false;
    public bool useRight = false;
    public bool useUp = false;

    [Header("Push with the forward direction on this transform?")]
    public Transform pushTransform;

    [Header("What direction to push?")]
    public UnityEngine.ForceMode forceType;

    public AudioSource triggerSound;

    [Header("Keep pushing things every frame? (Expensive!)")]
    public bool onStay;
    
    public void TryPush( GameObject other )
    {
        Rigidbody body = other.GetComponentInChildren<Rigidbody>();
        Rigidbody[] bodys = other.GetComponentsInChildren<Rigidbody>();
        if( body != null )
        {
            //BasicRockMovement avt =  other.GetComponentInChildren<BasicRockMovement>();
            //if( avt != null )
            //{
            //    if( avt.player != null && avt.player.IsPlayerControlled() )
            //    {
            //        if( triggerSound != null )
            //        {
            //            triggerSound.Play();
            //        }
            //    }
            //}
        
            if( pushTransform != null )
            {
                foreach( Rigidbody b in bodys )
                {
                    if( b.isKinematic )
                        continue;
                    b.AddForce( pushTransform.forward * forcePower, forceType );
                }

                if(body != null )
                    body.AddForce( pushTransform.forward * forcePower, forceType );
            }
            else
            {
                if( useForward )
                    body.AddForce( transform.forward * forcePower, forceType );
                else if( useRight )
                    body.AddForce( transform.right * forcePower, forceType );
                else if( useUp )
                    body.AddForce( transform.up * forcePower, forceType );
                else
                    body.AddForce( forceDirection * forcePower, forceType );
            }
        }
        else
        {
            //DLog.Log( "want to push object "+gameObject.name+" but it has no body!" );
        }
    }

    void OnCollisionEnter( Collision other )
    {
        TryPush( other.gameObject );
    }

    void OnTriggerEnter( Collider other )
    {
        TryPush( other.gameObject );
    }

    void OnCollisionStay( Collision other )
    {
        if( !onStay )
            return;

        TryPush( other.gameObject );
    }
}
