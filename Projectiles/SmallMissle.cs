using UnityEngine;
using System.Collections;

namespace nv
{
    public class SmallMissle : BaseWeapon
    {
        public GameObject projectilePrefab;

        public float shotVelocity = 100.0f;

        public Transform emitPoint;

        [SerializeField]
        float shootCooldownTime;

        [SerializeField]
        TimedRoutine shootCooldown;

        public override void TryActivate()
        {
            base.TryActivate();

            if(shootCooldown.IsRunning)
                return;

            GameObject missle = (GameObject)Instantiate(projectilePrefab, emitPoint.position, Quaternion.identity);

            Rigidbody body = missle.GetComponent<Rigidbody>();

            body.velocity = emitPoint.forward * (owner.velocity.magnitude + shotVelocity);

            shootCooldown = new TimedRoutine(shootCooldownTime);
            shootCooldown.Start();
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled;
            }

            set
            {
                base.Enabled = value;
            }
        }

        void Awake()
        {
            Enabled = true;
        }
    }
}