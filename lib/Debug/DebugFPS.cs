using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace nv
{
    public class DebugFPS : MonoBehaviour
    {
        public Text fpsDisplayLabel;
        public Text toggleDisplay;
        public Text fpsDisplay;

        [SerializeField, HideInInspector]
        List<Text> textElements;

        public float lowFPSThreshold = 20;
        public float greatFPSThreshold = 40;

        float deltaTime = 0.0f;

        public Color lowFPSColor = Color.red;
        public Color okFPSColor = Color.white;
        public Color greatFPSColor = Color.green;

        void Reset()
        {
            gameObject.GetOrAddComponent<Canvas>();
            var grid = gameObject.GetOrAddComponent<GridLayoutGroup>();
            grid.startAxis = GridLayoutGroup.Axis.Vertical;
            grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            grid.constraintCount = 2;
            grid.cellSize = new Vector2( 115f, 20f );
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2( 400, 40 );

            textElements = gameObject.GetComponentsInChildren<Text>().ToList();
            while( textElements.Count < 3 )
            {
                string name = "";
                if( textElements.Count == 0 )
                {
                    name = "fpsDisplayLabel";
                }
                else if( textElements.Count == 1 )
                {
                    name = "toggleDisplay";
                }
                else if( textElements.Count == 2 )
                {
                    name = "fpsDisplay";
                }
                GameObject textElement = new GameObject( name );
                textElement.transform.SetParent( transform );
                var text = textElement.AddComponent<Text>();
                if( textElements.Count == 1 )
                {
                    var toggleButton = textElement.GetOrAddComponent<Button>();
                    toggleButton.onClick.RemovePersistentListener( ToggleLogging );
                    toggleButton.onClick.AddPersistentListener( ToggleLogging );                    
                    toggleButton.targetGraphic = text;
                }

                textElements.Add( text );
            }

            fpsDisplayLabel = textElements[ 0 ];
            toggleDisplay = textElements[ 1 ];
            fpsDisplay = textElements[ 2 ];

            fpsDisplayLabel.text = "FPS: ";
            toggleDisplay.text = "[Hide FPS]";
        }

        void Update()
        {
#if !DEBUG
            gameObject.SetActive( false );
#endif

            if( fpsDisplay == null )
                return;

            if( !fpsDisplay.isActiveAndEnabled )
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

            if( !fpsDisplay.isActiveAndEnabled )
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
            
            fpsDisplay.text = text;
        }

        public void ToggleLogging()
        {
            if( fpsDisplay != null && fpsDisplay.isActiveAndEnabled )
            {
                fpsDisplay.gameObject.SetActive( false );
                toggleDisplay.text = "[Show FPS]";
            }
            else if( fpsDisplay != null && !fpsDisplay.isActiveAndEnabled )
            {
                fpsDisplay.gameObject.SetActive( true );
                toggleDisplay.text = "[Hide FPS]";
            }
        }
    }

}