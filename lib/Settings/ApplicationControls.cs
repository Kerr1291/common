using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nv
{
    public class ApplicationControls : ScriptableObject
    {
        public static ApplicationControls instance;
        public static ApplicationControls Instance
        {
            get
            {
                if(instance == null)
                {
#if UNITY_EDITOR
                    instance = AssetDatabase.LoadAssetAtPath<ApplicationControls>("Assets/Resources/ApplicationControls.asset");
                    if(instance == null)
                        CreateApplicationControls();
#else
                    instance = Resources.FindObjectsOfTypeAll<ApplicationControls>()[0];
#endif
                }
                return instance;
            }
        }

        //TODO: fix this hack so this isn't inside the application controls..... (see typeSelection in ReorderableArrayInspector.cs for details)
        public Dictionary<string, int> typeSelection = new Dictionary<string, int>();

#if UNITY_EDITOR
        [MenuItem(nv.editor.Consts.MENU_ROOT + "/Assets/Create Application Controls")]
        public static void CreateApplicationControls()
        {
            ApplicationControls asset = ScriptableObject.CreateInstance<ApplicationControls>();

            AssetDatabase.CreateAsset(asset, "Assets/Resources/ApplicationControls.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
            instance = asset;
        }
#endif

        public void OnEnable()
        {
            instance = this;
        }

        public void Awake()
        {
            instance = this;
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void SetFullScreen(bool enable)
        {
            bool value = enable;

            if(Application.isEditor)
            {
                Debug.Log("fullscreen = " + value);
                return;
            }

            Resolution current = Screen.currentResolution;
            Screen.SetResolution(current.width, current.height, value);
            //GameCamera.SaveResolutionData();
        }
    }
}