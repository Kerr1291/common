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
    public class GraphicsSettings : ScriptableSingleton<GraphicsSettings>
    {
        public static void Create()
        {
            CreateEditorInstance();
        }

        public bool VSync
        {
            get
            {
                return QualitySettings.vSyncCount > 0;
            }
            set
            {
                QualitySettings.vSyncCount = value ? 1 : 0;
            }
        }

        public string[] Resolutions
        {
            get
            {
                return Screen.resolutions.Select(x => x.ToString()).ToArray();
            }
        }

        public int CurrentResolution
        {
            get
            {
                var resolutions = Screen.resolutions.Select(x => x.ToString());
                int selection = resolutions.ToList().IndexOf(Screen.currentResolution.ToString());
                return selection;
            }
            set
            {
                var resolutions = Screen.resolutions;
                var selection = resolutions[value];
                Screen.SetResolution(selection.width, selection.height, Screen.fullScreen);
            }
        }

        public bool FullScreen
        {
            get
            {
                return Screen.fullScreen;
            }
            set
            {
                Screen.fullScreen = value;
            }
        }
    }
}