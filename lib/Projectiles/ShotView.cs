using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv
{
    public class ShotView : PoolableMonoBehaviour<ShotData>
    {
        public ShotList objectPool;

        [Header("Max lifetime of shot")]
        public float maxLifetime;
        public TimedRoutine lifetime;

        [Header("What kinds of things does this shot hit?")]
        public LayerMask collisionType;

        [Header("Describes how the shot moves, dies, and other things")]
        public BaseShotBehavior shotBehavior;

        protected override void Setup(ShotData data)
        {
            if(this.Data != null)
            {
                //Debug.Log( "OOPS this data is still alive " + this.data.shotData );
                this.Data.IsAlive = false;
                lifetime.Reset();
                NotifyLifetimeEnd();
            }

            //before this call this object (the view) does not have an instance assigned
            //so we bind our data to this view and then do any additional setup required
            //example: we could set the text used by a ui text object by reading some value from the data
            base.Setup(data);

            //Debug.Log( "Activating shot "+data.shotData );

            lifetime = new TimedRoutine(maxLifetime, NotifyLifetimeEnd);
            lifetime.Start();

            shotBehavior.Setup(this);
        }

        void OnTriggerEnter(Collider other)
        {
            //do we collide with this?
            //if( collisionType == ( collisionType | (1 << other.gameObject.layer) ) )
            if(collisionType.Any(other.gameObject))
            {
                NotifyCollision(other.gameObject);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            //do we collide with this?
            //if( collisionType == ( collisionType | (1 << other.gameObject.layer) ) )
            if(collisionType.Any(other.gameObject))
            {
                NotifyCollision(other.gameObject);
            }
        }

        public void NotifyCollision(GameObject hitObject)
        {
            //Debug.Log( "shot hit something " + data.shotData );
            Data.IsAlive = shotBehavior.ProcessHit(hitObject);
        }

        public void NotifyLifetimeEnd()
        {
            if(Data == null)
                return;

            //Debug.Log( "shot ran out of life " + data.shotData );
            Data.IsAlive = false;
            //shotBehavior.DeactivateShot();
        }

        void FixedUpdate()
        {
            if(!lifetime.IsRunning && gameObject.activeInHierarchy)
                Destroy(gameObject);

            shotBehavior.DoUpdate(this);
        }
    }
}