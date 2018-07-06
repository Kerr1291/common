using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using nv;

public class CubeBoid : MonoBehaviour
{
    public BoidRules rules;

    public bool playerControlled = false;
    public bool drawDebugVectors = false;

    public Transform _transform;
    public Rigidbody body;
    public Collider boidCollider;
    public SphereCollider swarmDetectionArea;
    public GameObject target;

    //public bool useRuleUpdater = false;
    //public TimeLock ruleUpdater;

    //private bool exploded = false;

    List<Collider> swarm = new List<Collider>();
    List<CubeBoid> cubeswarm = new List<CubeBoid>();

    bool swarmDirty = false;
    public bool rulesDirty = true;
    public bool rule3Dirty = true;

    public Vector3 movementVector;

    //how many frames to skip between updates
    [Range(0,60)]
    public int ruleUpdateRate = 2;
    int framesUntilUpdate = 0;

    public List<Collider> Swarm
    {
        get
        {
            return swarm;
        }
    }

    public List<CubeBoid> CubeSwarm
    {
        get
        {
            return cubeswarm;
        }
    }

    public void AddBoid( Collider newBoid )
    {
        if( swarm.Contains( newBoid ) )
            return;
        swarm.Add(newBoid);
        swarmDirty = true;
    }

    public void RemoveBoid( Collider oldBoid )
    {
        if( !swarm.Contains( oldBoid ) )
            return;
        swarm.Remove( oldBoid );
        swarmDirty = true;
    }

    public void UpdateSwarm( List<Collider> newSwarm )
    {
        swarmDirty = true;
        swarm = newSwarm;
        CleanSwarmObjects();
    }

    void CleanSwarmObjects()
    {
        if( !swarmDirty )
            return;

        List < Collider > cleanSwarm = new List<Collider>();
        List < CubeBoid > cleanCubeSwarm = new List<CubeBoid>();

        Vector3 pos = _transform.localPosition;
        Dev.VectorXZ( ref pos );

        for(int i = 0; i < swarm.Count; ++i )
        {
            if( swarm[i] == null )
                continue;

            //K2.Tools.LogVar( pos );
            //K2.Tools.LogVar( swarm[ i ].transform.localPosition );
            Vector3 diff = swarm[i].transform.localPosition - pos;
            Dev.VectorXZ( ref diff );
            //K2.Tools.LogVar( diff );
            float dist = diff.magnitude;

            //K2.Tools.LogVar( dist );
            //K2.Tools.LogVar( rules.FlockRadius( this ) );
            if( dist > rules.FlockRadius( this ) )
                continue;

            //K2.Tools.Log( "adding" );
            CubeBoid cube = swarm[i].GetComponentInChildren<CubeBoid>();
            
            if( cube == null )
            {
                cleanSwarm.Add( swarm[ i ] );
            }
            else
            {
                if( cube.gameObject.tag != body.gameObject.tag )
                {
                    cleanSwarm.Add( swarm[ i ] );
                    continue;
                }

                cleanCubeSwarm.Add( cube );
            }
        }

        swarm = cleanSwarm;

        for( int i = 0; i < cubeswarm.Count; ++i )
        {
            if( cubeswarm[ i ] == null )
                continue;

            Vector3 diff = cubeswarm[i]._transform.localPosition - pos;
            Dev.VectorXZ( ref diff );
            float dist = diff.magnitude;
            if( dist > rules.FlockRadius( this ) )
                continue;

            cleanCubeSwarm.Add( cubeswarm[i] );
        }

        cubeswarm = cleanCubeSwarm;

        ////trim all non-null, "nearby" things
        //swarm = swarm.Select( x => x ).Where( x =>
        //{
        //    if( x != null )
        //    {
        //        return true;
        //    }
        //    return false;
        //}
        //).ToList();

        //cubeswarm = swarm.Select( x =>  ).Where( x => x != null ).ToList();
        swarmDirty = false;
    }

    IEnumerator AddToTracking()
    {
        yield return new WaitForSeconds(0.1f);
        GameCamera.GetGameCamera(0).AddObjectToTracking(body.gameObject);
    }

    void OnEnable()
    {
        StartCoroutine(AddToTracking());
    }

    void CheckWithCameraIfThisIsValidAndRemoveFromTracking()
    {
        if( GameCamera.GetGameCamera( 0 ) == null )
            return;
        if( body == null )
            return;
        GameCamera.GetGameCamera( 0 ).RemoveObjectFromTracking( body.gameObject );
    }

    void OnDisable()
    {
        CheckWithCameraIfThisIsValidAndRemoveFromTracking();
    }

    void OnDestroy()
    {
        CheckWithCameraIfThisIsValidAndRemoveFromTracking();
    }

    void Awake()
    {
        fdtCached = Time.fixedDeltaTime;

        if( _transform == null )
            _transform = body.GetComponent<Transform>();
        body.sleepThreshold = 0f;
        //previousPos = transform.position;
        currentBehavior = SwarmBehavior;
        StartCoroutine( GetRules() );
    }

    IEnumerator GetRules()
    {
        while( BoidManager.Instance == null )
        {
            yield return new WaitForEndOfFrame();
        }

        rules = BoidManager.Instance.GetBoidRules(gameObject.tag);
    }
    

    [Header("New properties that control how the cuboids move in swarms")]
    public float boidMovePower = 1.0f;
    public float boidRollPower = 1.0f;
    public float boidMaxSpeed = 100.0f;
    public UnityEngine.ForceMode boidMovementType;

    Vector3 rollVectorCached;
    float fdtCached = 0f;
    public void AddBoidVelocity( Vector3 bVel )
    {
        Dev.VectorXZ(ref bVel);

        rollVectorCached.x = body.velocity.z;
        rollVectorCached.z = -body.velocity.x;
        
        Vector3 rollDirection = rollVectorCached.normalized;
        Vector3 velToAdd = boidMovePower * bVel * fdtCached;
        Vector3 rollToAdd = boidRollPower * rollDirection * fdtCached;

        if( velToAdd.magnitude >= 0.01f )
            body.AddForce( velToAdd, boidMovementType );
        if( rollToAdd.magnitude >= 0.01f )
            body.AddTorque( rollToAdd, boidMovementType );
    }

    Vector3 debugRule1Vector;
    Vector3 debugRule2Vector;
    Vector3 debugRule3Vector;

    void Update()
    {
        if( rules == null )
            return;

        if ( playerControlled )
            return;

        CleanSwarmObjects();

        framesUntilUpdate--;

        if( framesUntilUpdate <= 0 )
        {
            rulesDirty = true;
            rule3Dirty = true;
            swarmDirty = true;

            debugRule1Vector = rules.Rule1Vector( this );
            debugRule2Vector = rules.Rule2Vector( this );
            debugRule3Vector = rules.Rule3Vector( this );

            movementVector = rules.GetMovementVector(this);
            framesUntilUpdate = ruleUpdateRate;
        }

        if( drawDebugVectors )
        {
            Debug.DrawLine( _transform.localPosition, _transform.localPosition + debugRule1Vector, Color.red );
            Debug.DrawLine( _transform.localPosition, _transform.localPosition + debugRule2Vector, Color.green );
            Debug.DrawLine( _transform.localPosition, _transform.localPosition + debugRule3Vector, Color.blue );
        }

    }
    
    void FixedUpdate()
    {
        if( rules == null )
            return;

        if( playerControlled )
            return;

        if( swarm.Count > 0 || cubeswarm.Count > 0 )
            currentBehavior = SwarmBehavior;

        if (currentBehavior != null)
        {
            currentBehavior();
        }
    }


    //////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////
    ///Behaviors
    public System.Action currentBehavior;
    

    //UnityEngine.Coroutine waitBehavior;
    
    public int state = 0;

    IEnumerator WaitBehavior()
    {
        float time = GameRNG.Rand( 0.1f, 2.0f );
        while( currentBehavior == WaitAndDoNothing && time > 0.0f )
        {
            if( swarm.Count > 0 || cubeswarm.Count > 0 )
                break;

                time -= Time.deltaTime;
            body.velocity = Vector3.zero;
            yield return new WaitForEndOfFrame();
        }
        
        currentBehavior = SwarmBehavior;

        //waitBehavior = null;
    }

    void WaitAndDoNothing()
    {
        state = 0;

        if( swarm.Count > 0 || cubeswarm.Count > 0 )
        {
            currentBehavior = SwarmBehavior;
        }
        //if( waitBehavior == null )
        //    waitBehavior = StartCoroutine( WaitBehavior() );
    }

    //Vector3 previousPos;

    Vector3 _xzvel;
    Vector3 _yvel;

    void SwarmBehavior()
    {
        state = 1;
        if( rules == null )
            return;

        if( swarm.Count <= 0 && cubeswarm.Count <= 0 )
        {
            currentBehavior = WaitAndDoNothing;
            return;
        }

        //float delta = Vector3.Distance(_transform.localPosition,previousPos);

        //if( delta <= 0.01f || cubeswarm.Count <= 0 )
        //{
        //    if( swarmDetectionArea.radius < 200.0f )
        //        swarmDetectionArea.radius += 1f;
        //}
        //else
        //{
        //    if( swarmDetectionArea.radius > 30.0f )
        //        swarmDetectionArea.radius -= 1f;
        //}

        Vector3 nextMovementVector = movementVector;
        AddBoidVelocity( nextMovementVector );

        _xzvel.x = body.velocity.x;
        _xzvel.z = body.velocity.z;
        _yvel.y = body.velocity.y + -4.0f;

        //cap max speed
        float speed = _xzvel.magnitude;

        if( speed > boidMaxSpeed )
            _xzvel = _xzvel.normalized * boidMaxSpeed;

        body.velocity = _xzvel + _yvel;
    }
}
