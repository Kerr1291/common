using UnityEngine;
using System.Collections;

public class TriggerSpawnedObject : MonoBehaviour 
{
    public TimedSpawnTrigger owner;

    public float lifetime = 0.0f;

    public void Init( TimedSpawnTrigger spawner, GameObject parent )
    {
        owner = spawner;

        if( parent != null )
            gameObject.transform.SetParent( parent.transform );
    }

    void OnDestroy()
    {
        owner.RemoveObject( this );
    }
}
