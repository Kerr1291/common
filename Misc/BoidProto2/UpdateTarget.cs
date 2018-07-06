using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UpdateTarget : MonoBehaviour {

    [Header("The owner of the swarm")]
    public CubeBoid controller;

    [Tooltip("This list is checked, if found it is added to the swarm")]
    [Header("This list is checked, if found it is added to the swarm")]
    public List<string> swarmTags;

    //public bool useTimeLock = false;

    [Header("How often the nearby area trigger is updated")]
    //how many frames to skip between updates
    [Range(0,60)]
    public int triggerUpdateRate = 2;
    int framesUntilUpdate = 0;

    [Header("This trigger is kept disabled most of the time")]
    public Collider trigger;

    //how often the trigger is enabled for in fixed-update-frames
    int resetFrames = 2;

    void Update()
    {
        framesUntilUpdate--;

        if( framesUntilUpdate > 0 )
            return;

        trigger.enabled = true;
        resetFrames = 2;

        framesUntilUpdate = triggerUpdateRate;
    }

    void FixedUpdate()
    {
        if( resetFrames <= 0 )
            trigger.enabled = false;
        else
            resetFrames--;
    }

    //void OnCollisionEnter( Collision collision )
    //{
    //    if( controller.playerControlled )
    //        return;

    //    Vector2 min = new Vector2(-1f,-1f);
    //    Vector2 max = new Vector2(1f,1f);

    //    if( collision.collider.gameObject.tag == "Wall" )
    //        controller.SetMove( RNG.Rand( min, max ) );
    //}

    void OnTriggerEnter( Collider other )
    {
        if( controller.playerControlled )
            return;


        if( swarmTags.Contains( other.tag ) )
        {
            controller.AddBoid( other );
        }
        //else
        //{
        //    if( ignoreTags.Contains( other.tag ) )
        //        return;
        //    controller.target = other.gameObject;
        //}
    }

    void OnTriggerExit(Collider other)
    {
        if( controller.playerControlled )
            return;

        if( swarmTags.Contains( other.tag ) )
        {
            controller.RemoveBoid( other );
        }
        //else
        //{
        //    if( ignoreTags.Contains( other.tag ) )
        //        return;
        //    if( controller.target == other.gameObject )
        //        controller.target = null;
        //}
    }
}
