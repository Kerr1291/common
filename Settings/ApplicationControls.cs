using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nv
{
    public class ApplicationControls : ScriptableObject
    {
#if UNITY_EDITOR
        [MenuItem(nv.editor.Consts.MENU_ROOT + "/Assets/Create Application Controls")]
        public static void CreateApplicationControls()
        {
            ApplicationControls asset = ScriptableObject.CreateInstance<ApplicationControls>();

            AssetDatabase.CreateAsset(asset, "Assets/Resources/ApplicationControls.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
#endif

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