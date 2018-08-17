#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using System.IO;

namespace nv.editor
{
    [ScriptedImporter(1, "tmpl")]
    public class ScriptTemplater : ScriptedImporter
    {
        public string defaultNamespaceName = "nv";
        public string defaultClassName = "NewClass";

        [HideInInspector]
        public string text = "Asset not imported.";

        public override void OnImportAsset(AssetImportContext ctx)
        {
            text = File.ReadAllText(ctx.assetPath);

            if(text.Contains(" : Editor"))
            {
                defaultNamespaceName = "nv.editor";
            }
        }
    }

    [CustomEditor(typeof(ScriptTemplater))]
    public class ScriptTemplaterEditor : ScriptedImporterEditor
    {
        const string TEMPLATE_NAMESPACE_NAME = "CUSTOMNAMESPACE";
        const string TEMPLATE_CLASS_NAME = "CUSTOMTYPE";
        const int numberOfLinesInPreview = 2000;
        const int maxWidthOfCreateScriptButton = 320;

        public string namespaceName = "nv";
        public string className = "NewClass";

        ScriptTemplater Target
        {
            get
            {
                return ((ScriptTemplater)target);
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();

            namespaceName = Target.defaultNamespaceName;
            className = Target.defaultClassName;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.LabelField("Script Template: " + target.name);

            namespaceName = EditorGUILayout.TextField("Namespace:", namespaceName);
            className = EditorGUILayout.TextField("Class Name:", className);

            if(GUILayout.Button("Create Script", GUILayout.MaxWidth(maxWidthOfCreateScriptButton)))
            {
                string pwd = EditorData.Instance.GetData<string>(this.GetType().Name, target.name);
                if(string.IsNullOrEmpty(pwd))
                    pwd = Application.dataPath;

                string data = ProcessTemplate(Target.text);

                CreateSourceFileWithSavePrompt(data, className, ref pwd);

                EditorData.Instance.SetData<string>(pwd, this.GetType().Name, target.name);
            }

            DrawPreviewGUI();
        }

        void DrawPreviewGUI()
        {
            string truncatedNotice = "";
            if(Target.text.Length >= numberOfLinesInPreview)
                truncatedNotice = "\n<Preview truncated>";

            EditorGUILayout.TextArea(Target.text.Substring(0, Mathf.Min(Target.text.Length, numberOfLinesInPreview)) + truncatedNotice);
        }

        string ProcessTemplate(string data)
        {
            string result = data;

            result = ReplaceNamespace(result);
            result = ReplaceClassName(result);

            return result;
        }

        string ReplaceNamespace(string data)
        {
            string result = data.Replace(TEMPLATE_NAMESPACE_NAME, namespaceName);
            return result;
        }

        string ReplaceClassName(string data)
        {
            string result = data.Replace(TEMPLATE_CLASS_NAME, className);
            return result;
        }

        void CreateSourceFileWithSavePrompt(string data, string defaultName, ref string path)
        {
            path = EditorUtility.SaveFilePanelInProject("Create script from template ", "" + defaultName + ".cs", "cs", "Enter a file name for the script.", path);

            if(path == "")
                return;

            File.WriteAllText(path, data);
        }
    }
}
#endif
