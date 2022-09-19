using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv
{

    public class LinearShotBehavior : BaseShotBehavior
    {
        public float shotDamage = 1f;

        public float shotForce;

        public ForceMode2D forceMode;

        public float shotVelocity;

        public AudioClipSet shotSounds;

        ShotView _shotData;

        Rigidbody2D _rbody;

        Transform _transform;

        Vector2 _velocity;

        void PlayRandomShotSound()
        {
            shotSounds.PlayRandom();
        }

        public override bool ProcessHit(GameObject hitObject)
        {
            if(_shotData.Data.ignoreList.Contains(hitObject))
                return true;

            Rigidbody2D otherRBody = hitObject.GetComponent<Rigidbody2D>();
            if(otherRBody != null)
                otherRBody.AddForceAtPosition(_velocity.normalized * shotForce, _rbody.position, forceMode);

            //TODO:
            //StatLink stats = hitObject.GetComponent<StatLink>();
            //if(stats != null)
            //{
            //    stats.life.Life -= shotDamage;
            //}

            return false;
        }

        public override void DeactivateShot()
        {
            gameObject.SetActive(false);
            _shotData = null;
        }

        void OnEnable()
        {
            PlayRandomShotSound();
        }

        public override void Setup(ShotView shot)
        {
            _shotData = shot;
            _transform = transform;
            _rbody = GetComponent<Rigidbody2D>();
            _velocity = shotVelocity * _shotData.Data.shotDirection * Time.fixedDeltaTime;

            _transform.position = shot.Data.spawnPoint;
        }

        public override void DoUpdate(ShotView shot)
        {
            if(_shotData == null)
                return;

            _rbody.MovePosition(_rbody.position + _velocity);
        }
    }
}