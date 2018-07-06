using UnityEngine;
using System.Collections;

public class RunSpeedStat : MonoBehaviour
{
    [SerializeField]
    private float runSpeed = 100.0f;

    public virtual float RunSpeed
    {
        get
        {
            return runSpeed;
        }
    }
}
