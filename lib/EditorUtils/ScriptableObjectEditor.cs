#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System;
using System.Reflection;
using System.Linq;

namespace nv.editor
{
    [CustomEditor(typeof(MonoScript))]
    public class ScriptableObjectEditor : UnityEditor.AssetImporters.AssetImporterEditor
    {
        const int numberOfZeroesToPad = 3;
        const int numberOfLinesInPreview = 2000;

        public override void OnInspectorGUI()
        {
            Type t = null;

            try
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    t = asm.GetTypes().Where(x => x.Name == target.name).Where(x => typeof(ScriptableObject).IsAssignableFrom(x)).FirstOrDefault();
                    if (t != null)
                    {
                        break;
                    }
                }

                EditorGUILayout.LabelField("Script: " + target.name);
                if (t != null && typeof(ScriptableObject).IsAssignableFrom(t) && !t.IsAbstract && !t.IsGenericType)
                {
                    if (GUILayout.Button("Create Scriptable Object", GUILayout.MaxWidth(320)))
                    {
                        string startingAssetLocation = Application.dataPath;
                        string[] foundAssets = AssetDatabase.FindAssets(target.name);
                        if (foundAssets.Length > 0)
                        {
                            startingAssetLocation = System.IO.Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(foundAssets[0]));
                        }

                        CreateAssetWithSavePrompt(t, startingAssetLocation);
                    }
                }
                else
                {
                    //if(t == null)
                    //    EditorGUILayout.LabelField(target.name + " is not a type.");
                    //else
                    //    EditorGUILayout.LabelField(target.name + " is not a ScriptableObject.");
                }


                base.OnInspectorGUI();

                EditorGUILayout.TextArea(((MonoScript)target).text.Substring(0, Mathf.Min(((MonoScript)target).text.Length, numberOfLinesInPreview)) + "\n <Preview truncated>");

            }
            catch (Exception)
            {
                //....
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
        }

        // Creates a new ScriptableObject via the default Save File panel
        private ScriptableObject CreateAssetWithSavePrompt(Type type, string path)
        {
            string[] foundAssets = AssetDatabase.FindAssets(type.Name);
            string count_as_int = foundAssets.Length.ToString().PadLeft(numberOfZeroesToPad, '0');

            path = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", "" + type.Name + "_" + count_as_int + ".asset", "asset", "Enter a name for the "+ type.Name + ".", path);
            if(path == "") return null;
            ScriptableObject asset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            EditorGUIUtility.PingObject(asset);
            return asset;
        }

        /// <summary>
        /// Create a scriptable object. If the application is not playing, this will create a permenant asset. If the application is playing and allowCreateAssetInPlayMode == false, this will create a temporary asset.
        /// </summary>
        public static ScriptableObject CreateScriptableObject(Type type, string path = null, bool useFileBrowswer = true, bool allowCreateAssetInPlayMode = true)
        {
            ScriptableObject asset = null;
#if UNITY_EDITOR
            bool createEditorAsset = (!Application.isPlaying || (Application.isPlaying && allowCreateAssetInPlayMode));

            if (createEditorAsset)
            {
                string[] foundAssets = AssetDatabase.FindAssets(type.Name);
                string count_as_int = foundAssets.Length.ToString().PadLeft(numberOfZeroesToPad, '0');

                if (useFileBrowswer)
                    path = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", "" + type.Name + "_" + count_as_int + ".asset", "asset", "Enter a name for the " + type.Name + ".", path);
                if (path == "")
                    return null;
            }

            asset = ScriptableObject.CreateInstance(type);

            if (createEditorAsset)
            {
                if (!useFileBrowswer)
                {

                    string dir = new System.IO.FileInfo(path).Directory.FullName;
                    if (!System.IO.Directory.Exists(dir))
                        System.IO.Directory.CreateDirectory(dir);
                }
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                EditorGUIUtility.PingObject(asset);
            }
#endif
            return asset;
        }

        public static T CreateScriptableObject<T>(string path, bool useFileBrowswer = true, bool allowCreateAssetInPlayMode = true)
            where T : ScriptableObject
        {
            Type type = typeof(T);
            return (T)CreateScriptableObject(type, path, useFileBrowswer, allowCreateAssetInPlayMode);
        }
    }
}
#endif