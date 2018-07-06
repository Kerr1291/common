using UnityEngine;
using System;

public class TimedLife : MonoBehaviour 
{
    public bool activateOnAwake = false;

    [SerializeField]
    private float lifetime;

    public float Lifetime
    {
        get
        {
            return lifetime;
        }
        set
        {
            lifetime = value;
        }
    }

    Action _callback = null;

    bool _loaded = false;

    public void Init( float lifetime, Action destroy_callback = null )
    {
        Lifetime = lifetime;

        _callback = destroy_callback;

        _loaded = true;
    }

    void Awake()
    {
        if( activateOnAwake )
            Init( Lifetime );
    }
        	
	void Update() 
	{
        if( !_loaded )
            return;
        
        Lifetime -= Time.deltaTime;

        if( Lifetime <= 0.0f )
        {
            if( _callback != null )
                _callback();

            Destroy( gameObject );
        }
    }
}
