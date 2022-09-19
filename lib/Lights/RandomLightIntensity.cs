using UnityEngine;
using System.Collections;

namespace nv
{
    [RequireComponent(typeof(Light))]
    public class RandomLightIntensity : MonoBehaviour
    {
        [SerializeField]
        Light lightToRandomize;

        TimedRoutine updateInterval;
        
        public Range updateIntervalRange = new Range(0f,2f);
        public Range lightIntensity = new Range(1f, 1f);

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
            if(lightToRandomize == null)
                return;

            updateInterval = new TimedRoutine();

            lightToRandomize = GetComponent<Light>();

            current_light = lightToRandomize.intensity;

            _light_start_position = transform.localPosition;
        }

        float start_light = 0.0f;
        float current_light = 0.0f;

        // Update is called once per frame
        void Update()
        {
            if(lightToRandomize == null)
                return;

            if(updateInterval.IsRunning)
            {
                lightToRandomize.intensity = Mathf.Lerp(start_light, current_light, updateInterval.TimeNormalized);

                if(lightWobble)
                {
                    float normalize_start = start_light / (lightIntensity.Max - lightIntensity.Min);
                    float normalize_current = current_light / (lightIntensity.Max - lightIntensity.Min);

                    transform.localPosition = _light_start_position + wobbleAxis * Mathf.Lerp(normalize_start, normalize_current, updateInterval.TimeNormalized) * lightWobbleMax;
                }

                return;
            }

            start_light = lightToRandomize.intensity;
            current_light = Random.Range(lightIntensity.Min, lightIntensity.Max);

            float intervalTime = Mathf.Max(0.01f, updateIntervalRange.RandomValuef());
            
            updateInterval.Start(intervalTime);
        }
    }
}