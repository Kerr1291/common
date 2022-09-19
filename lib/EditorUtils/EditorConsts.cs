using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
namespace nv.editor
{
    [UnityEditor.InitializeOnLoad]
    public static class Consts
    {
        public static class Menu
        {
            public const string ROOT = "NV";
            public const string ASSETS = "/Assets";
            public const string WINDOW = "/Window";
        }

        public static class Paths
        {
            public const string EDITOR_RESOURCES = "Assets/Resources/EditorCommon/";
        }

        public static class AssetDatabase
        {
            public const string REQUIRED_ROOT_PATH = "Assets/";
        }


        /// <summary>
        /// Create any required directories or perform first time setup stuff
        /// </summary>
        static Consts()
        {
            if (!System.IO.Directory.Exists(editor.Consts.Paths.EDITOR_RESOURCES))
            {
                System.IO.Directory.CreateDirectory(editor.Consts.Paths.EDITOR_RESOURCES);
            }
        }
    }
}
#endif