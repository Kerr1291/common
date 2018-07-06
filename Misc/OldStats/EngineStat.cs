using UnityEngine;
using System.Collections;

public class EngineStat : MonoBehaviour
{
    [SerializeField]
    private float thrustPower = 100.0f;

    public virtual float ThrustPower
    {
        get
        {
            return thrustPower;
        }
    }

    public virtual bool Active
    {
        get; set;
    }

    void Awake()
    {
        Active = true;
    }
}
