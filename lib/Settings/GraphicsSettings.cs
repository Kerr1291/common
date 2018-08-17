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
    }
}