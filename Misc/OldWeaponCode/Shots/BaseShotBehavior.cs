using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseShotBehavior : MonoBehaviour
{
    //time between shots
    public float cooldown = .1f;

    public abstract bool ProcessHit( GameObject hitObject );

    public abstract void DeactivateShot();

    public abstract void Setup( ShotView shot );

    public abstract void DoUpdate( ShotView shot );
}
