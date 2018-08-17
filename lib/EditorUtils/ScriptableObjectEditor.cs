#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System;
using System.Reflection;
using System.Linq;

namespace nv.editor
{
    [CustomEditor(typeof(MonoScript))]
    public class ScriptableObjectEditor : AssetImporterEditor
    {
        const int numberOfZeroesToPad = 3;
        const int numberOfLinesInPreview = 2000;

        public override void OnInspectorGUI()
        {
            Type t = null;

            foreach(Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetTypes().Where(x => x.Name.Contains(target.name)).Where(x=> x.IsSubclassOf(typeof(ScriptableObject))).FirstOrDefault();
                if(t != null)
                    break;             
            }

            EditorGUILayout.LabelField("Script: "+target.name);
            if(t != null && t.IsSubclassOf(typeof(ScriptableObject)) && !t.IsAbstract && !t.IsGenericType)
            {
                if(GUILayout.Button("Create Scriptable Object", GUILayout.MaxWidth(320)))
                {
                    string startingAssetLocation = Consts.EDITOR_RESOURCES_PATH;
                    string[] foundAssets = AssetDatabase.FindAssets(target.name);
                    if(foundAssets.Length > 0)
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
    }
}
#endif