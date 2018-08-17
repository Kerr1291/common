using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
namespace nv.editor
{
    [UnityEditor.InitializeOnLoad]
    public static class Consts
    {
        public const string MENU_ROOT = "NV";
        public const string MENU_DEBUG_FOLDER = "Debug";
        public const string EDITOR_RESOURCES_PATH = "Assets/Resources/EditorCommon/";
        
        /// <summary>
        /// Create any required directories or perform first time setup stuff
        /// </summary>
        static Consts()
        {
            if(!System.IO.Directory.Exists(editor.Consts.EDITOR_RESOURCES_PATH))
            {
                System.IO.Directory.CreateDirectory(editor.Consts.EDITOR_RESOURCES_PATH);
            }
        }
    }
}
#endif