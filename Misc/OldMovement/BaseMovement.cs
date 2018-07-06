using UnityEngine;
using System.Collections;

public class BaseMovement : MonoBehaviour {

    //orientation of the object
    public GameObject model;

    //physical body of the object
    //public Rigidbody body;

    //container of stat objects
    public GameObject stats;
    
    //action to take, given this movement request
    public virtual void Move( Vector2 dir ) { }
}
