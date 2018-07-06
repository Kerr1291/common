using UnityEngine;
using UnityEngine.UI;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
namespace nv.editor
{
    [CustomEditor(typeof(SceneVersion))]
    public class SceneVersion_Editor : Editor
    {
        SceneVersion _target;
        bool _showDefault;

        public override void OnInspectorGUI()
        {
            _target = (SceneVersion)target;

            if(GUILayout.Button("Upgrade Release Version"))
            {
                _target.UpdateToNewPrimeVersion();
            }
            if(GUILayout.Button("Upgrade Build Version"))
            {
                _target.UpdateToNewMajorVersion();
            }
            _target.updateMinorOnSave = EditorGUILayout.Toggle("Update on Save", _target.updateMinorOnSave);

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scenesInVersion"),true);
            serializedObject.ApplyModifiedProperties();

            _showDefault = EditorGUILayout.Toggle("Show Default Inspector", _showDefault);
            if(_showDefault)
                base.OnInspectorGUI();
        }
    }
}
#endif

namespace nv
{
    [ExecuteInEditMode]
    public class SceneVersion : MonoBehaviour
    {
        [Header("Use the context menu options to update version")]
        public Text primeVersion;
        public Text majorVersion;

        public int majorVersionDigits = 2;

        public Text minorVersion;

        public int minorVersionDigits = 4;

        public GameObject textRoot;

        public bool updateMinorOnSave = true;

        [Header("If not null, will display the date")]
        public Text dateText;

        [SceneAssetPathField]
        public string[] scenesInVersion;

        [SerializeField]
        [HideInInspector]
        bool[] _dirtyScenes;

        static public void SetVisible()
        {
            //GameObject.FindObjectOfType<SceneVersion>()
        }

        //use this for releasing to QA or anywhere outside the local dev group
        [ContextMenu("UpdateToNewPrimeVersion -- Release Update")]
        public void UpdateToNewPrimeVersion()
        {
            IncPrimeVersion();
            majorVersion.text = "1";
            minorVersion.text = "0";
            majorVersion.text = majorVersion.text.PadLeft(majorVersionDigits, '0');
            minorVersion.text = minorVersion.text.PadLeft(minorVersionDigits, '0');
        }

        //use this each time a build or other large checkpoint is reached -- often used on perforce checkins
        [ContextMenu("UpdateToNewMajorVersion -- Build Update")]
        public void UpdateToNewMajorVersion()
        {
            IncMajorVersion();
            minorVersion.text = "0";
            minorVersion.text = minorVersion.text.PadLeft(minorVersionDigits, '0');
        }

        public void IncPrimeVersion()
        {
            string primeVersionStr = primeVersion.text;

            try
            {
                int version_num = System.Convert.ToInt32(primeVersionStr);

                version_num++;

                primeVersionStr = version_num.ToString();

                primeVersion.text = primeVersionStr;
            }
            catch
            {
                //do nothing
                Debug.Log("invalid prime version number format");
            }
        }

        public void IncMajorVersion()
        {
            string majorVersionStr = majorVersion.text;

            try
            {
                int version_num = System.Convert.ToInt32(majorVersionStr);

                version_num++;

                majorVersionStr = version_num.ToString();

                majorVersionStr = majorVersionStr.PadLeft(majorVersionDigits, '0');

                majorVersion.text = majorVersionStr;
            }
            catch
            {
                //do nothing
                Debug.Log("invalid major version number format");
            }
        }

        public void IncMinorVersion()
        {
            string minorVersionStr = minorVersion.text;

            try
            {
                int version_num = System.Convert.ToInt32(minorVersionStr);

                version_num++;

                minorVersionStr = version_num.ToString();

                minorVersionStr = minorVersionStr.PadLeft(minorVersionDigits, '0');

                minorVersion.text = minorVersionStr;
            }
            catch
            {
                //do nothing
                Debug.Log("invalid minor version number format");
            }
        }

        bool IsSetup()
        {
            return primeVersion != null && majorVersion != null && minorVersion != null && textRoot != null;
        }

        void Update()
        {
            if(updateMinorOnSave == false)
                return;

            if(IsSetup() != true)
                return;

#if UNITY_EDITOR

            if(_dirtyScenes == null || _dirtyScenes.Length != scenesInVersion.Length)
                _dirtyScenes = new bool[scenesInVersion.Length];

            bool need_save = false;

            for(int i = 0; i < _dirtyScenes.Length; ++i)
            {
                if(UnityEditor.SceneManagement.EditorSceneManager.GetSceneByName(scenesInVersion[i]).isDirty == false
                    && _dirtyScenes[i] == true)
                    need_save = true;
            }

            //did they just save it?
            if(need_save)
            {
                //update the minor version
                IncMinorVersion();

                //update the date
                if(dateText != null)
                    dateText.text = "Created: " + System.DateTime.Now.ToLongDateString();
            }
            for(int i = 0; i < scenesInVersion.Length; ++i)
            {
                _dirtyScenes[i] = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByName(scenesInVersion[i]).isDirty;
            }

#endif

        }
    }
}