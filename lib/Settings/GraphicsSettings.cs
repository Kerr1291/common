using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nv
{
    public class GraphicsSettings : ScriptableObject
    {
#if UNITY_EDITOR
        [MenuItem(nv.editor.Consts.MENU_ROOT + "/Assets/Create Graphics Settings")]
        public static void CreateApplicationControls()
        {
            ApplicationControls asset = ScriptableObject.CreateInstance<ApplicationControls>();

            AssetDatabase.CreateAsset(asset, "Assets/Resources/GraphicsSettings.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
#endif
        [SerializeField]
        bool vsyncEnabled;

        public bool VSync
        {
            get
            {
                vsyncEnabled = QualitySettings.vSyncCount > 0;
                return vsyncEnabled;
            }
            set
            {
                vsyncEnabled = value;
                if(vsyncEnabled)
                    QualitySettings.vSyncCount = 1;
                else
                    QualitySettings.vSyncCount = 0;
            }
        }

        //setup the property values to match the values that were set in the inspector
        public void Init()
        {
            VSync = vsyncEnabled;
        }
    }
}