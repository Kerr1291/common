using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimedSpawnTrigger : MonoBehaviour
{
    [Header("Prefabs to spawn (One chosen randomly)")]
    public GameObject[] triggerPrefabs;

    [Header("Is the spawner enabled?")]
    public bool active = true;

    [Header("Destroy spawned objects after a certain amount of time?")]
    [Tooltip("Destroy spawned objects after a certain amount of time?")]
    public bool useObjectLifetime = false;

    [Header("Time until spawned object should be destroyed")]
    [Tooltip("Time until spawned object should be destroyed")]
    public float objectLifetime = 1.0f;

    [Header("Delay before this may be triggered again")]
    public float retriggerDelay = 1.0f;

    [Header("Max objects this can create. -1 for no limit")]
    [Tooltip("Max objects this can create. -1 for no limit")]
    public int maxObjects = -1;
    
    public Transform spawnLocation;
        
    private float retriggerTimer = 0.0f;

    ////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////


    [Header("For Debugging: List of spawned things")]
    public List<TriggerSpawnedObject> spawnedObjects = new List<TriggerSpawnedObject>();

    public void RemoveObject( TriggerSpawnedObject obj )
    {
        spawnedObjects.Remove( obj );
    }  

    void DoTriggerUpdate()
    {
        if( active == false )
            return;

        TryDoTrigger();
    }

    List<TriggerSpawnedObject> _remove_tobj_list = new List<TriggerSpawnedObject>();

    void CleanOldObjects()
    {
        if( useObjectLifetime == false )
            return;

        //don't track players that aren't active anymore for one reason or another
        foreach( TriggerSpawnedObject tobj in spawnedObjects )
        {
            tobj.lifetime -= Time.deltaTime;
            if( tobj.lifetime <= 0.0f )
                _remove_tobj_list.Add( tobj );
        }

        if( _remove_tobj_list.Count > 0 )
        {
            foreach( TriggerSpawnedObject tobj in _remove_tobj_list )
            {
                Destroy( tobj.gameObject );
                spawnedObjects.Remove( tobj );
            }
            _remove_tobj_list.Clear();
        }
    }

    void Awake()
    {
        retriggerTimer = retriggerDelay;
    }

    void Update()
    {
        CleanOldObjects();
        UpdateDelay();
        DoTriggerUpdate();
    }

    void UpdateDelay()
    {
        if( retriggerTimer < retriggerDelay )
            retriggerTimer += Time.deltaTime;
    }

    void TryDoTrigger()
    {
        if( retriggerTimer < retriggerDelay )
            return;

        DoTrigger();
    }

    void DoTrigger()
    {
        if( maxObjects >= 0 && spawnedObjects.Count >= maxObjects )
            return;

        retriggerTimer = 0.0f;

        GameObject prefab = triggerPrefabs[ UnityEngine.Random.Range(0, triggerPrefabs.Length) ];

        GameObject obj = GameObject.Instantiate( prefab );

        TriggerSpawnedObject tobj = obj.GetComponent<TriggerSpawnedObject>();
        if( tobj == null )
            tobj = obj.AddComponent<TriggerSpawnedObject>();

        tobj.Init( this, gameObject );
        tobj.lifetime = objectLifetime;

        tobj.gameObject.transform.position = spawnLocation.transform.position;

        spawnedObjects.Add( tobj );
    }

    ////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////

    public void SetRetriggerDelay( float new_delay )
    {
        retriggerDelay = new_delay;
    }

    public void ResetTriggerDelay()
    {
        retriggerTimer = retriggerDelay;
    }

    public void ForceDoTrigger()
    {
        DoTrigger();
    }

    public void RemoveAllSpawnedObjects()
    {
        //don't track players that aren't active anymore for one reason or another
        foreach( TriggerSpawnedObject tobj in spawnedObjects )
        {
            Destroy( tobj.gameObject );
        }
        spawnedObjects.Clear();
    }
}
