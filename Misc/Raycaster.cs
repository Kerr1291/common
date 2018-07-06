using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class Raycaster : MonoBehaviour {

    [Header("If enabled will update every frame (may cause performance issues)")]
    [SerializeField]
    private bool debugCheckAlways = false;
    [Header("If enabled show the current state of the caster)")]
    [SerializeField]
    private bool debug = false;
    Color debugColor = Color.white;

    [SerializeField]
    Color debugHitColor = Color.green;
    [SerializeField]
    Color debugNoHitColor = Color.red;

    public float distance = 1.0f;

    public bool updateOnAwake = true;

    [Header("Use y as forward?")]
    public bool is2DRaycast = false;

    public bool use2DTriggersOnly = false;
    List<Transform> _2dTriggers = new List<Transform>();

    [Header("What to check against when casting")]
    public UnityEngine.LayerMask layerMask = Physics.DefaultRaycastLayers;

    [Header("(For 3D) Hit triggers?")]
    public QueryTriggerInteraction triggerInteraction;

    bool _frame_cached = false;
    
    List<Transform> _hits = new List<Transform>();
    
    [Header("Optimize by using a box collider?")]
    public bool useBoxColliderCache = false;
    bool _box_cached = false;

    public List<Collider> ignoreList;
    public List<Collider2D> ignoreList2D;

    //TODO: fix this, would be a good optimization
    //[Header("(For 3D) Only get the first hit?")]
    bool useOneHit3DOptimization = false;
    
    BoxCollider2D _box2d;
    BoxCollider   _box;

    public float boxColliderExtraFactor = 0.01f;

    void Reset()
    {
        //TODO: static layer ref object

        //default this to trigger layer
        gameObject.layer = K2DefaultLayers.GetLayerIndex("Trigger");
    }

    //Creates a temporary raycaster that will destroy itself after a short while
    public static Raycaster CreateTempRaycaster()
    {
        GameObject r_obj = new GameObject("Temp Raycaster");
        Raycaster ray = r_obj.AddComponent<Raycaster>();
        TimedLife time = r_obj.AddComponent<TimedLife>();

        time.Init( 0.5f );

        return ray; 
    }

    public bool RaycastHasHit()
    {
        DoRaycast();

        return _hits.Count > 0;
    }

    public List<Transform> Raycast()
    {
        DoRaycast();
        return _hits;
    }

    void DoRaycast()
    {
        if( debug )
            Debug.Log( "_frame_cached: " + _frame_cached );
        if (_frame_cached == true)
            return;

        if( debug )
            Debug.Log( "_box_cached: " + _box_cached );
        if (useBoxColliderCache && _box_cached)
            return;

        _hits.Clear();

        if( is2DRaycast )
        {
            RaycastHit2D[] hits;
            hits = Physics2D.RaycastAll(CastRay2DOrigin, CastRay2DDirection, distance, layerMask.value);

            for(int i = 0; i < hits.Length; ++i)
            {
                if( hits[ i ].transform.GetComponent<Raycaster>() == null )
                {
                    if( debug )
                        Debug.Log( "Raycaster " + gameObject.name + " hit " + hits[ i ].transform.gameObject.name );
                    _hits.Add( hits[ i ].transform );
                }
            }

            if( use2DTriggersOnly )
            {
                foreach(Transform t in _2dTriggers)
                {
                    if( _hits.Contains( t ) == false )
                        _hits.Add( t );
                }
            }
        }
        else
        {
            RaycastHit[] new_hits = new RaycastHit[1];

            if(useOneHit3DOptimization && _box != null)
            {
                RaycastHit hit = new RaycastHit();
                if( _box.Raycast(CastRay, out hit, distance) )
                    new_hits[0] = hit;
                else
                    new_hits = new RaycastHit[0];
            }
            else
            {
                new_hits = Physics.RaycastAll(CastRay.origin, CastRay.direction, distance, layerMask, triggerInteraction);
            }

            for(int i = 0; i < new_hits.Length; ++i)
            {
                if( new_hits[i].collider.GetComponent<Raycaster>() == null)
                {
                    if( ignoreList.Contains( new_hits[ i ].collider ) )
                        continue;
                    
                    _hits.Add( new_hits[i].transform );
                }
            }
        }

        if(debug)
        {
            if(_hits.Count > 0)
                debugColor = debugHitColor;
            else
                debugColor = debugNoHitColor;
        }

        _frame_cached = true;

        if (useBoxColliderCache && _box_cached == false)
            _box_cached = true;
    }

    void CreateBoxColliderForOptimization()
    {
        if (useBoxColliderCache == false)
            return;

        if(is2DRaycast)
        {
            _box2d = gameObject.AddComponent<BoxCollider2D>();

            //Vector2 min = Vector2.Min(CastRay2DOrigin, CastRay2DOrigin + CastRay2DDirection * (distance + boxColliderExtraFactor));
            //Vector2 max = Vector2.Max(CastRay2DOrigin, CastRay2DOrigin + CastRay2DDirection * (distance + boxColliderExtraFactor));

            _box2d.size = new Vector2(0.05f, (distance + boxColliderExtraFactor));
            _box2d.offset = new Vector2(0.0f, (distance + boxColliderExtraFactor) * 0.5f);
            _box2d.isTrigger = true;
        }
        else
        {
            _box = gameObject.AddComponent<BoxCollider>();

            //Vector3 min = Vector3.Min(CastRay.origin, CastRay.origin + CastRay.direction * (distance + boxColliderExtraFactor));
            //Vector3 max = Vector3.Max(CastRay.origin, CastRay.origin + CastRay.direction * (distance + boxColliderExtraFactor));
            //Vector3 size = max - min;
            _box.size = new Vector3(0.05f, 0.05f, (distance + boxColliderExtraFactor));
            _box.center = new Vector3(0.0f, 0.0f, (distance + boxColliderExtraFactor) * 0.5f);
            _box.isTrigger = true;
        }
    }

    void Awake()
    {
        CreateBoxColliderForOptimization();

        if (updateOnAwake)
            DoRaycast();
    }

    void Update()
    {
        _frame_cached = false;

        if( debug)
            Debug.DrawLine(DebugStartPoint, DebugEndPoint, debugColor);
    }

    Ray _casting_ray = new Ray();
    Ray CastRay
    {
        get
        {
            _casting_ray.origin = transform.position;
            _casting_ray.direction = Forward;
            return _casting_ray;
        }
    }

    Vector2 CastRay2DOrigin
    {
        get
        {
            return new Vector2(CastRay.origin.x, CastRay.origin.y);
        }
    }

    Vector2 CastRay2DDirection
    {
        get
        {
            return new Vector2(CastRay.direction.x, CastRay.direction.y);
        }
    }

    Vector3 Forward
    {
        get
        {
            if (is2DRaycast)
                return transform.up;
            else
                return transform.forward;
        }
    }

    Vector3 DebugStartPoint
    {
        get
        {
            if (is2DRaycast)
                return CastRay2DOrigin;

            return CastRay.origin;
        }
    }

    Vector3 DebugEndPoint
    {
        get
        {
            if( is2DRaycast )
                return CastRay2DOrigin + CastRay2DDirection * distance;

            return CastRay.origin + CastRay.direction * distance;
        }
    }

    void OnCollisionEnter(Collision other)
    {
        UpdateRaycaster( other.collider );
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        UpdateRaycaster( other.collider );
        //Debug.Log( other.gameObject.name + " c entered" );
    }

    void OnTriggerEnter(Collider other)
    {
        UpdateRaycaster( other );
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if( !ignoreList2D.Contains( other ) )
        {
            if( use2DTriggersOnly )
                _2dTriggers.Add( other.transform );
        }
        if( debug )
            Debug.Log( gameObject.name +" hit "+ other.gameObject.name+" entered" );
        UpdateRaycaster( other );
    }

    void OnTriggerExit( Collider other )
    {
        UpdateRaycaster( other );
    }

    void OnTriggerExit2D( Collider2D other )
    {
        if( !ignoreList2D.Contains( other ) )
        {
            if( use2DTriggersOnly )
                _2dTriggers.Remove( other.transform );
        }
        if( debug )
            Debug.Log( gameObject.name + " hit " + other.gameObject.name + " exited" );
        UpdateRaycaster(other);
    }

    void UpdateRaycaster( Collider other )
    {
        if( ignoreList.Contains( other ) )
            return;
        
        if( is2DRaycast == false && useBoxColliderCache )
            _box_cached = false;

        if( debugCheckAlways )
            StartCoroutine( TriggerCheckRaycast() );
    }

    void UpdateRaycaster( Collider2D other )
    {
        if( ignoreList2D.Contains( other ) )
            return;

        if( is2DRaycast && useBoxColliderCache )
            _box_cached = false;

        if( debugCheckAlways )
            StartCoroutine( TriggerCheckRaycast() );
    }

    IEnumerator TriggerCheckRaycast()
    {
        yield return new WaitForSeconds(0.1f);
        if( debugCheckAlways )
        {
            DoRaycast();
        }
    }
}
