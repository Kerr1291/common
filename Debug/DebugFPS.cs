using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace nv
{
    public class DebugFPS : MonoBehaviour
    {
        public Text fpsDisplay;
        public Text toggleDisplay;

        public bool logLowFPS = true;

        public float lowFPSThreshold = 20;
        public float greatFPSThreshold = 40;

        [Header( "Don't log low fps again until at least this much time has passed" )]
        public float relogDelay = 1.0f;

        float delayTime = 0.0f;

        float deltaTime = 0.0f;

        public Color lowFPSColor = Color.red;
        public Color okFPSColor = Color.white;
        public Color greatFPSColor = Color.green;

        void Update()
        {
#if !DEBUG
            foreach( var v in GetComponentsInParent<Transform>() )
                v.gameObject.SetActive( false );
#endif

            if( fpsDisplay == null )
                return;

            deltaTime += ( Time.deltaTime - deltaTime ) * 0.1f;
        }

        void OnGUI()
        {
#if !DEBUG
            return;
#endif
            if( fpsDisplay == null )
                return;

            if( Mathf.Approximately( Time.deltaTime, Mathf.Epsilon ) )
            {
                fpsDisplay.text = "Timescale Zero";
                return;
            }

            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = string.Format( "{0:0.0} ms ({1:0.} fps)", msec, fps );

            if( fps < lowFPSThreshold )
            {
                fpsDisplay.color = lowFPSColor;
            }
            else if( fps > greatFPSThreshold )
            {
                fpsDisplay.color = greatFPSColor;
            }
            else
            {
                fpsDisplay.color = okFPSColor;
            }

            if( logLowFPS && fps < lowFPSThreshold && delayTime <= 0.0f )
            {
                Dev.LogVar( "Low FPS detected! (FPS < " + lowFPSThreshold.ToString() + ") Details:", text );
                delayTime = relogDelay;
            }
            else
            {
                if( logLowFPS && delayTime > 0.0f )
                {
                    delayTime -= Time.deltaTime;
                }
            }

            fpsDisplay.text = text;
        }

        public void ToggleLogging()
        {
            logLowFPS = !logLowFPS;
            if( toggleDisplay != null && logLowFPS )
                toggleDisplay.text = "Logging enabled";
            if( toggleDisplay != null && logLowFPS == false )
                toggleDisplay.text = "Logging disabled";
        }
    }

}