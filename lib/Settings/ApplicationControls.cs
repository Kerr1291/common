using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nv
{
    public class ApplicationControls : ScriptableSingleton<ApplicationControls>
    {        
        public static void Create()
        {
            CreateEditorInstance();
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