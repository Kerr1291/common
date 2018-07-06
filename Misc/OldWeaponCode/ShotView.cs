using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Components;
using nv;

//very basic symbol view
public class ShotView : ListDataView<ShotData>
{
    [Header("Max lifetime of shot")]
    public GameTimerDelayAction lifetime;

    [Header("What kinds of things does this shot hit?")]
    public LayerMask collisionType;

    [Header("Describes how the shot moves, dies, and other things")]
    public BaseShotBehavior shotBehavior;

    //Called each time an object comes into view
    public override void BindDataToView( ShotData data )
    {
        if( this.data != null )
        {
            //Debug.Log( "OOPS this data is still alive " + this.data.shotData );
            this.data.IsAlive = false;
            lifetime.Reset();
            NotifyLifetimeEnd();
        }

        //before this call this object (the view) does not have an instance assigned
        //so we bind our data to this view and then do any additional setup required
        //example: we could set the text used by a ui text object by reading some value from the data
        base.BindDataToView( data );

        //Debug.Log( "Activating shot "+data.shotData );

        lifetime.Lock( NotifyLifetimeEnd );

        shotBehavior.Setup( this );
    }

    void OnTriggerEnter(Collider other)
    {
        //do we collide with this?
        //if( collisionType == ( collisionType | (1 << other.gameObject.layer) ) )
        if( Dev.IsLayerInMask( collisionType, other.gameObject ) )
        {
            NotifyCollision(other.gameObject);
        }
    }

    void OnTriggerEnter2D( Collider2D other )
    {
        //do we collide with this?
        //if( collisionType == ( collisionType | (1 << other.gameObject.layer) ) )
        if( Dev.IsLayerInMask(collisionType,other.gameObject) )
        {
            NotifyCollision( other.gameObject );
        }
    }

    public void NotifyCollision(GameObject hitObject)
    {
        //Debug.Log( "shot hit something " + data.shotData );
        data.IsAlive = shotBehavior.ProcessHit(hitObject);
    }

    public void NotifyLifetimeEnd()
    {
        if( data == null )
            return;

        //Debug.Log( "shot ran out of life " + data.shotData );
        data.IsAlive = false;
        //shotBehavior.DeactivateShot();
    }

    void FixedUpdate()
    {
        if( !lifetime.Locked && gameObject.activeInHierarchy )
            Destroy( gameObject );

        shotBehavior.DoUpdate( this );
    }
}
