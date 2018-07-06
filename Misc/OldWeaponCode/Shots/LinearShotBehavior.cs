using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Components;
using nv;

public class LinearShotBehavior : BaseShotBehavior
{
    public float shotDamage = 1f;

    public float shotForce;

    public ForceMode2D forceMode;

    public float shotVelocity;

    public List<K2AudioSource> shotSounds;

    ShotView _shotData;

    Rigidbody2D _rbody;

    Transform _transform;

    Vector2 _velocity;

    void PlayRandomShotSound()
    {
        if( shotSounds.Count <= 0 )
            return;

        K2AudioSource shotSound = Dev.GetRandomElementFromList(shotSounds);
        if( shotSound == null )
            return;

        shotSound.UnitySource.Play();
    }

    public override bool ProcessHit( GameObject hitObject )
    {
        if( _shotData.data.ignoreList.Contains( hitObject ) )
            return true;

        Rigidbody2D otherRBody = hitObject.GetComponent<Rigidbody2D>();
        if( otherRBody != null )
            otherRBody.AddForceAtPosition( _velocity.normalized * shotForce, _rbody.position, forceMode );

        StatLink stats = hitObject.GetComponent<StatLink>();
        if( stats != null )
        {
            stats.life.Life -= shotDamage;
        }

        return false;
    }

    public override void DeactivateShot()
    {
        gameObject.SetActive( false );
        _shotData = null;
    }

    void OnEnable()
    {
        PlayRandomShotSound();
    }

    public override void Setup( ShotView shot )
    {
        _shotData = shot;
        _transform = transform;
        _rbody = GetComponent<Rigidbody2D>();
        _velocity = shotVelocity * _shotData.data.shotDirection * Time.fixedDeltaTime;

        _transform.position = shot.data.spawnPoint;
    }

    public override void DoUpdate( ShotView shot )
    {
        if( _shotData == null )
            return;

        _rbody.MovePosition( _rbody.position + _velocity );
    }
}
