using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class ConfigureWheel : MonoBehaviour
{
    public WheelCollider wheel;

    public float speedThreshold;
    public int stepsBelowThreshold;
    public int stepsAboveThreshold;

    void Awake()
    {
        wheel.ConfigureVehicleSubsteps( speedThreshold, stepsBelowThreshold, stepsAboveThreshold);
    }
}