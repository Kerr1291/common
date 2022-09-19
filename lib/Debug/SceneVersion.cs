using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace nv
{
    [ExecuteInEditMode]
    public class SceneVersion : MonoBehaviour
    {
        public bool showProjectVersion;
        public bool showSceneVersion;

        public bool showProjectVersionDate;
        public bool showSceneVersionDate;

        public Text projectVersionLabel;
        public Text sceneVersionLabel;
        public Text projectVersion;
        public Text sceneVersion;        
        public Text projectDate;
        public Text sceneDate;

        [SerializeField,HideInInspector]
        List<Text> versionElements;

        //since this game object might end up being moved to an invalid scene at runtime
        //save the name of the scene it's in to use as a reference during gameplay
        [SerializeField, HideInInspector]
        protected string originalSceneForVersion;

        void Reset()
        {
            gameObject.GetOrAddComponent<Canvas>();
            var grid = gameObject.GetOrAddComponent<GridLayoutGroup>();
            grid.startAxis = GridLayoutGroup.Axis.Vertical;
            grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            grid.constraintCount = 2;
            grid.cellSize = new Vector2( 115f, 20f );
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2( 400, 40 );

            versionElements = gameObject.GetComponentsInChildren<Text>().ToList();
            while( versionElements.Count < 6 )
            {
                string name = "";
                if( versionElements.Count == 0 )
                {
                    name = "projectVersionLabel";
                }
                else if( versionElements.Count == 1 )
                {
                    name = "sceneVersionLabel";
                }
                else if( versionElements.Count == 2 )
                {
                    name = "projectVersion";
                }
                else if( versionElements.Count == 3 )
                {
                    name = "sceneVersion";
                }
                else if( versionElements.Count == 4 )
                {
                    name = "projectDate";
                }
                else if( versionElements.Count == 5 )
                {
                    name = "sceneDate";
                }
                GameObject versionElement = new GameObject(name);
                versionElement.transform.SetParent( transform );
                versionElements.Add(versionElement.AddComponent<Text>());
            }

            projectVersionLabel = versionElements[ 0 ];
            sceneVersionLabel = versionElements[ 1 ];
            projectVersion = versionElements[ 2 ];
            sceneVersion = versionElements[ 3 ];
            projectDate = versionElements[ 4 ];
            sceneDate = versionElements[ 5 ];

            projectVersionLabel.text = "Project Version:";
            sceneVersionLabel.text = "Scene Version:";

            projectDate.horizontalOverflow = HorizontalWrapMode.Overflow;
            sceneDate.horizontalOverflow = HorizontalWrapMode.Overflow;

            UpdateText();
        }

        void Awake()
        {
            gameObject.SetActive( false );

            versionElements[ 0 ] = projectVersionLabel;
            versionElements[ 1 ] = sceneVersionLabel;
            versionElements[ 2 ] = projectVersion;
            versionElements[ 3 ] = sceneVersion;
            versionElements[ 4 ] = projectDate;
            versionElements[ 5 ] = sceneDate;

            UpdateText();
        }

        void Update()
        {
            gameObject.SetActive( false );

            //TODO: get if this is a release build or not
#if !DEBUG
            if ( Application.isPlaying )
            {
                versionElements.ForEach( x => x.gameObject.SetActive( false ) );
            }
            else
#endif
            {
                if (VersionManager.Instance.SceneVersion(gameObject) != "Unversioned")
                    originalSceneForVersion = VersionManager.Instance.SceneVersion(gameObject);
                versionElements.ForEach( x => x.gameObject.SetActive( true ) );
                UpdateText();
            }
        }

        void UpdateText()
        {
#if !DEBUG
            return;
#endif
            if( showProjectVersion )
                projectVersion.text = VersionManager.Instance.ProjectVersion;
            else
                projectVersion.gameObject.SetActive( false );

            if(showSceneVersion)
            {
                if(Application.isPlaying)
                    sceneVersion.text = VersionManager.Instance.SceneVersion(originalSceneForVersion);
                else
                    sceneVersion.text = VersionManager.Instance.SceneVersion(gameObject);
            }
            else
            {
                sceneVersion.gameObject.SetActive(false);
            }

            if( showProjectVersionDate )
                projectDate.text = VersionManager.Instance.ProjectVersionDate;
            else
                projectDate.gameObject.SetActive( false );

            if( showSceneVersionDate )
                sceneDate.text = VersionManager.Instance.SceneVersionDate( gameObject );
            else
                sceneDate.gameObject.SetActive( false );

            if( !showProjectVersion && !showProjectVersionDate )
                projectVersionLabel.gameObject.SetActive( false );

            if( !showSceneVersion && !showSceneVersionDate )
                sceneVersionLabel.gameObject.SetActive( false );
        }

        private void OnEnable()
        {
            if(Application.isPlaying && Application.isEditor)
            {
#if DEBUG
                string versionString = "Version: " + VersionManager.Instance.ProjectVersion;
                string versionDate = "Created: " + VersionManager.Instance.ProjectVersionDate;
                Dev.Log("Starting: " + Application.productName + " " + versionString + " " + versionDate);
#endif
            }
        }
    }
}