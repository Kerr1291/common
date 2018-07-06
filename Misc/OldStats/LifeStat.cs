using UnityEngine;
using System.Collections;

public class LifeStat : MonoBehaviour {

    public GameObject destroyOnLifeZero;

    [SerializeField]
    private float life = 100.0f;

    public virtual float Life
    {
        get
        {
            return life;
        }
        set
        {
            life = value;

            if( life <= 0.0f )
            {
                if( destroyOnLifeZero != null && destroyOnLifeZero.activeInHierarchy )
                    Destroy( destroyOnLifeZero );
            }
        }
    }
}
