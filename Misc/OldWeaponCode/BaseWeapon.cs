using UnityEngine;
using System.Collections;

public class BaseWeapon : MonoBehaviour {

    public Rigidbody owner;

    //Call when button is pressed
    public virtual void TryActivate() { }

    //Call when button is released
    public virtual void TryDeactivate() { }

    public virtual bool Enabled
    {
        get; set;
    }
}
