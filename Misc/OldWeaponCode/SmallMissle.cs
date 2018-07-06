using UnityEngine;
using System.Collections;

using Components;

public class SmallMissle : BaseWeapon
{
    public GameObject projectilePrefab;

    public float shotVelocity = 100.0f;

    public Transform emitPoint;

    [SerializeField]
    GameTimer shootDelay;

    public override void TryActivate()
    {
        base.TryActivate();

        if ( shootDelay.Locked )
            return;

        GameObject missle = (GameObject)Instantiate(projectilePrefab, emitPoint.position, Quaternion.identity);

        Rigidbody body = missle.GetComponent<Rigidbody>();

        body.velocity = emitPoint.forward * ( owner.velocity.magnitude + shotVelocity );

        shootDelay.Lock();
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
