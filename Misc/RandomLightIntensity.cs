using UnityEngine;
using System.Collections;

[RequireComponent(typeof( Light ))]
public class RandomLightIntensity : MonoBehaviour {

    [SerializeField]
    Light lightToRandomize;

    TimeLock updateInterval;

    public float randomBaseTime = 1.0f;
    public float randomUpdateOffset = 1.0f;

    public float minLightIntensity = 1.0f;
    public float maxLightIntensity = 1.0f;

    [Header("Wobble in time with the intensity changes")]
    public bool lightWobble = true;
    public Vector3 wobbleAxis = Vector3.up;
    Vector3 _light_start_position;
    public float lightWobbleMax = 0.1f;

    void Reset()
    {
        lightToRandomize = GetComponent<Light>();
    }

    void Awake()
    {
        if( lightToRandomize == null )
            return;

        updateInterval = new TimeLock();

        lightToRandomize = GetComponent<Light>();

        current_light = lightToRandomize.intensity;

        _light_start_position = transform.localPosition;
    }

    float start_light = 0.0f;
    float current_light = 0.0f;
    float t_time = 0.0f;
    float time = 0.0f;

    // Update is called once per frame
    void Update ()
    {
        if( lightToRandomize == null )
            return;

        if( updateInterval.Locked )
        {
            time += Time.deltaTime;

            lightToRandomize.intensity = Mathf.Lerp( start_light, current_light, time / t_time );

            if( lightWobble )
            {
                float normalize_start = start_light / (maxLightIntensity - minLightIntensity);
                float normalize_current = current_light / (maxLightIntensity - minLightIntensity);

                transform.localPosition = _light_start_position + wobbleAxis * Mathf.Lerp( normalize_start, normalize_current, time / t_time ) * lightWobbleMax;
            }

            return;
        }

        start_light = lightToRandomize.intensity;
        current_light = Random.Range( minLightIntensity, maxLightIntensity );

        updateInterval.lockTime = Mathf.Max(0.01f, randomBaseTime + Random.Range( -randomUpdateOffset, randomUpdateOffset ));

        t_time = updateInterval.lockTime;
        time = 0.0f;

        updateInterval.Lock();	
	}
}
