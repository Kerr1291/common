using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageOnContact : MonoBehaviour {
    
    public bool hitShields = true;

    public List<GameObject> ignoreOnContact;

    public List<string> ignoreTags;

    public float damage = 10.0f;

    void OnTriggerEnter( Collider other )
    {
        if( hitShields && other.GetComponent<BaseShield>() )
        {
            if( ignoreOnContact.Contains( other.gameObject ) )
                return;

            if( ignoreTags.Contains( other.tag ) )
                return;

            other.GetComponent<BaseShield>().Power -= damage;
        }
        
        //LifeStat life = other.GetComponent<LifeStat>();
        //if( life == null )
        //    return;

        //life.Life -= damage;
    }

    void OnCollisionEnter( Collision collision )
    {
        if( ignoreOnContact.Contains( collision.transform.gameObject ) )
            return;

        if( ignoreTags.Contains( collision.transform.tag ) )
            return;

        LifeStat life = collision.transform.GetComponentInChildren<LifeStat>();
        if( life == null )
            return;

        life.Life -= damage;
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
