using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatLink : MonoBehaviour
{
    public GameObject stats;

    public LifeStat life;

    public T GetStat<T>() where T : MonoBehaviour
    {
        return stats.GetComponent<T>();
    }

    void Awake()
    {
        life = GetStat<LifeStat>();
    }

    void Reset()
    {
        life = GetComponentInChildren<LifeStat>();
        if( life != null )
            stats = life.gameObject;
    }
}
