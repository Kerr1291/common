using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class AvatarAnimationTrigger : MonoBehaviour
{
    public List<Avatar> focusList = new List<Avatar>();

    int _players_inside = 0;
    int _non_players_inside = 0;

    [Header("Prefab to spawn on trigger condition")]
    public GameObject triggerPrefab;

    [Header("Use these to choose where to spawn your prefabs")]
    [Tooltip("Use these to choose where to spawn your prefabs")]
    public GameObject spawnPoint = null;
    public Vector3 spawnPosition;
    public bool spawnPositionIsLocal = true;

    [Header("How to parent the spawned objects")]
    [Tooltip("How to parent the spawned objects")]
    public GameObject parentToThis = null;
    public bool parentToTriggeringAvatar = false;

    [Header("Restrict activation in some way?")]
    public bool playersOnlyTrigger = false;
    public bool nonPlayersOnlyTrigger = false;

    [Header("When should this happen?")]
    public bool spawnOnEnter = true;
    public bool spawnOnUpdate = false;
    public bool spawnOnExit = false;

    [Header("Destroy spawned objects after a certain amount of time?")]
    [Tooltip("Destroy spawned objects after a certain amount of time?")]
    public bool useObjectLifetime = false;

    [Header("Time until spawned object should be destroyed")]
    [Tooltip("Time until spawned object should be destroyed")]
    public float objectLifetime = 0.0f;

    [Header("Delay before this may be triggered again")]
    public float retriggerDelay = 0.0f;
    float _delay_time = 0.0f;

    [Header("Max objects this can create. -1 for no limit")]
    [Tooltip("Max objects this can create. -1 for no limit")]
    public int maxObjects = -1;

    ////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////

    public class TriggeredObject
    {
        public float lifetime = 0.0f;
        public GameObject instance = null;

        public TriggeredObject( GameObject obj, GameObject parent )
        {
            instance = obj;

            if( parent != null )
                instance.transform.SetParent( parent.transform );
        }
    }
    
    public List<TriggeredObject> spawnedObjects = new List<TriggeredObject>();

    Avatar _triggering_object = null;

    void OnTriggerEnter( Collider other )
    {
        if( triggerPrefab == null )
            return;

        //an avatar entered!
        Avatar avt = other.GetComponentInChildren<Avatar>();
        if( avt == null )
            return;

        _triggering_object = avt;

        PlayerInputInterface pii = GameInput.GetPlayerControllingAvatar( avt );

        if( playersOnlyTrigger )
        {
            //is it a player?
            if( pii == null )
                return;
        }
        else if( nonPlayersOnlyTrigger )
        {
            if( pii != null )
                return;
        }

        if( pii != null )
            _players_inside += 1;
        else
            _non_players_inside += 1;

        if( focusList.Contains( avt ) == false )
            focusList.Add( avt );

        if( spawnOnEnter == false )
            return;

        TryDoTrigger();
    }

    void OnTriggerExit( Collider other )
    {
        if( triggerPrefab == null )
            return;

        //an avatar exited!
        Avatar avt = other.GetComponentInChildren<Avatar>();
        if( avt == null )
            return;

        _triggering_object = avt;

        if( focusList.Contains( avt ) == true )
            focusList.Remove( avt );

        PlayerInputInterface pii = GameInput.GetPlayerControllingAvatar( avt );

        if( playersOnlyTrigger )
        {
            //is it a player?
            if( pii == null )
                return;
        }
        else if( nonPlayersOnlyTrigger )
        {
            if( pii != null )
                return;
        }

        if( pii != null )
            _players_inside -= 1;
        else
            _non_players_inside -= 1;

        if( spawnOnExit == false )
            return;

        TryDoTrigger();
    }

    void DoTriggerUpdate()
    {
        if( spawnOnUpdate == false )
            return;

        if( focusList.Count <= 0 )
            return;

        if( playersOnlyTrigger && _players_inside <= 0 )
            return;

        if( nonPlayersOnlyTrigger && _non_players_inside <= 0 )
            return;

        TryDoTrigger();
    }

    List<Avatar> _remove_list = new List<Avatar>();

    void CleanFocusList()
    {
        //don't track players that aren't active anymore for one reason or another
        foreach( Avatar avt in focusList )
        {
            if( avt.gameObject.activeInHierarchy == false )
                _remove_list.Add( avt );
        }

        if( _remove_list.Count > 0 )
        {
            foreach( Avatar avt in _remove_list )
            {
                focusList.Remove( avt );
            }
            _remove_list.Clear();
        }
    }

    List<TriggeredObject> _remove_tobj_list = new List<TriggeredObject>();

    void CleanOldObjects()
    {
        if( useObjectLifetime == false )
            return;

        //don't track players that aren't active anymore for one reason or another
        foreach( TriggeredObject tobj in spawnedObjects )
        {
            tobj.lifetime -= Time.deltaTime;
            if( tobj.lifetime <= 0.0f )
                _remove_tobj_list.Add( tobj );
        }

        if( _remove_tobj_list.Count > 0 )
        {
            foreach( TriggeredObject tobj in _remove_tobj_list )
            {
                Destroy( tobj.instance );
                spawnedObjects.Remove( tobj );
            }
            _remove_tobj_list.Clear();
        }
    }

    void Awake()
    {
        _delay_time = retriggerDelay;
    }

    void Update() 
	{
        CleanFocusList();
        CleanOldObjects();
        UpdateDelay();
        DoTriggerUpdate();
    }

    void UpdateDelay()
    {
        if( _delay_time < retriggerDelay )
            _delay_time += retriggerDelay;
    }

    void TryDoTrigger()
    {
        if( _delay_time < retriggerDelay )
            return;

        DoTrigger();
    }

    void DoTrigger()
    {
        if( maxObjects >= 0 && spawnedObjects.Count >= maxObjects )
            return;

        _delay_time = 0.0f;

        GameObject parent_object = parentToThis;
        if( parentToTriggeringAvatar )
            parentToThis = _triggering_object.gameObject;

        TriggeredObject tobj = new TriggeredObject( GameObject.Instantiate( triggerPrefab ), parent_object );
        tobj.lifetime = objectLifetime;

        if( spawnPoint != null )
            tobj.instance.transform.position = spawnPoint.transform.position;
        else
        {
            if( spawnPositionIsLocal == false )
                tobj.instance.transform.localPosition = spawnPosition;
            else
            {
                tobj.instance.transform.position = gameObject.transform.position;
                tobj.instance.transform.Translate( spawnPosition );
            }
        }

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
        _delay_time = retriggerDelay;
    }

    public void ForceDoTrigger()
    {
        DoTrigger();
    }

    public void RemoveAllSpawnedObjects()
    {
        //don't track players that aren't active anymore for one reason or another
        foreach( TriggeredObject tobj in spawnedObjects )
        {
            Destroy( tobj.instance );
        }
        spawnedObjects.Clear();
    }
}
