using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Xml.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nv.editor
{
    public static class EditorTools
    {
#if UNITY_EDITOR
        /// <summary>
        /// Given a default folder path, depending on the options provided...
        /// See if the path exists, if it doesn't exist, see if we're allowed to create a new one and/or ask the user for a new path
        /// </summary>
        public static string GetFolderPath(string defaultFolderPath, bool createIfNotFound = true, bool showBrowserIfNotFound = true, string browserLabel = "Select folder")
        {
            string outputFolder = defaultFolderPath;
            if(System.IO.Directory.Exists(outputFolder))
                return outputFolder;

            if(createIfNotFound)
            {
                if(showBrowserIfNotFound)
                {
                    outputFolder = UnityEditor.EditorUtility.SaveFolderPanel(browserLabel, Application.dataPath, "");
                }

                try
                {
                    if(!System.IO.Directory.Exists(outputFolder))
                    {
                        System.IO.Directory.CreateDirectory(outputFolder);
                    }
                }
                catch(Exception e)
                {
                    outputFolder = null;
                    throw new Exception(e.Message, e);
                }
            }
            else
            {
                if(showBrowserIfNotFound)
                {
                    outputFolder = UnityEditor.EditorUtility.OpenFolderPanel(browserLabel, Application.dataPath, "");
                }
                else
                {
                    throw new DirectoryNotFoundException("Failed find folder: " + defaultFolderPath);
                }
            }
            return outputFolder;
        }

        public static bool SelectionContainsType<T>()
        {
            return UnityEditor.Selection.objects.Any(x => x.ContainsType<T>() || typeof(T).IsAssignableFrom(x.GetType()));
        }

        public static T GetOrCreateWindow<T>(string title = "")
            where T : EditorWindow
        {
            T currentWindow = UnityEditor.EditorWindow.GetWindow<T>();
            if(currentWindow == null)
            {
                currentWindow = EditorWindow.CreateInstance<T>();
            }

            if(!string.IsNullOrEmpty(title))
            {
                currentWindow.titleContent = new GUIContent(title);
            }

            return currentWindow;
        }

        public static void BeginScrollView(string key, bool alwaysShowHorizontal = false, bool alwaysShowVertical = false, float? minWidth = null, float? minHeight = null, Color? customBGColor = null)
        {
            List<GUILayoutOption> options = new List<GUILayoutOption>();

            if(minWidth != null && minWidth.HasValue)
                options.Add(GUILayout.MinWidth(minWidth.Value));
            if(minHeight != null && minHeight.HasValue)
                options.Add(GUILayout.MinHeight(minHeight.Value));

            GUIStyle hbarStyle = new GUIStyle(GUI.skin.horizontalScrollbar);
            GUIStyle vbarStyle = new GUIStyle(GUI.skin.verticalScrollbar);
            GUIStyle bgStyle = new GUIStyle(GUI.skin.scrollView);

            if(customBGColor != null)
            {
                bgStyle.normal.background = MakeTex(1, 1, customBGColor.Value);
            }

            Vector2 result = GetEditorPref<Vector2>(key);
            result = EditorGUILayout.BeginScrollView(result, alwaysShowHorizontal, alwaysShowVertical, hbarStyle, vbarStyle, bgStyle, options.ToArray());

            SetEditorPref<Vector2>(key, result);
        }

        public static void EndScrollView()
        {
            EditorGUILayout.EndScrollView();
        }

        public static void DrawHorizontalLabels<T>(params T[] args)
        {
            EditorGUILayout.BeginHorizontal();

            foreach(var arg in args)
            {
                EditorGUILayout.LabelField(arg.ToString());
            }

            EditorGUILayout.EndHorizontal();
        }

        public static void DrawHorizontalHeaderLabels<T>(params T[] args)
        {
            EditorGUILayout.BeginHorizontal();

            GUIStyle labelStyle = GUI.skin.label;
            labelStyle.fontStyle = FontStyle.Bold;

            foreach(var arg in args)
            {
                EditorGUILayout.LabelField(arg.ToString(), labelStyle);
            }

            EditorGUILayout.EndHorizontal();
        }

        public static void DrawSeparator(bool showHorizontalRule = false)
        {
            if(showHorizontalRule)
            {
                EditorGUILayout.LabelField(new string('_', (int)EditorGUILayout.GetControlRect().width));
            }
            else
            {
                EditorGUILayout.Separator();
            }
        }

        public static T FindScriptableObject<T>(string name)
            where T : ScriptableObject
        {
            string objectPath = AssetDatabase.FindAssets("t:" + typeof(T)).Select(x => AssetDatabase.GUIDToAssetPath(x)).FirstOrDefault(y => AssetDatabase.LoadAssetAtPath<T>(y).name == name);
            T objectRef = null;
            if(!string.IsNullOrEmpty(objectPath))
            {
                objectRef = AssetDatabase.LoadAssetAtPath<T>(objectPath);
                return objectRef;
            }
            return objectRef;
        }

        public static List<T> FindScriptableObjects<T>(string searchString = null)
            where T : ScriptableObject
        {
            var paths = AssetDatabase.FindAssets("t:" + typeof(T)).Select(x => AssetDatabase.GUIDToAssetPath(x));
            List<string> objectPaths;
            if(!string.IsNullOrEmpty(searchString))
            {
                objectPaths = paths.Where(x => AssetDatabase.LoadAssetAtPath<T>(x).name.Contains(searchString)).ToList();
            }
            else
            {
                objectPaths = paths.ToList();
            }

            List<T> objects = objectPaths.Select(x => AssetDatabase.LoadAssetAtPath<T>(x)).ToList();
            return objects;
        }

        public static bool EditorPrefFoldout(string key, string label = "")
        {
            if(string.IsNullOrEmpty(label))
                label = key + " Foldout:";

            var result = EditorPrefs.GetBool(key);
            result = EditorGUILayout.Foldout(result, label, true);
            EditorPrefs.SetBool(key, result);
            return result;
        }

        /// <summary>
        /// Render an editor field that is backed by an EditorPrefs entry.
        /// </summary>
        public static T EditorPrefField<T>(string key, string label = "")
        {
            if(string.IsNullOrEmpty(label))
                label = typeof(T).Name + " " + key;

            if(typeof(T) == typeof(bool))
            {
                var result = EditorPrefs.GetBool(key);
                result = EditorGUILayout.Toggle(label, result);
                EditorPrefs.SetBool(key, result);
                return (T)(object)result;
            }
            else if(typeof(T) == typeof(int))
            {
                var result = EditorPrefs.GetInt(key);
                result = EditorGUILayout.IntField(label, result);
                EditorPrefs.SetInt(key, result);
                return (T)(object)result;
            }
            else if(typeof(T) == typeof(float))
            {
                var result = EditorPrefs.GetFloat(key);
                result = EditorGUILayout.FloatField(label, result);
                EditorPrefs.SetFloat(key, result);
                return (T)(object)result;
            }
            else if(typeof(T) == typeof(string))
            {
                var result = EditorPrefs.GetString(key);
                result = EditorGUILayout.TextField(label, result);
                EditorPrefs.SetString(key, result);
                return (T)(object)result;
            }
            else if(typeof(T) == typeof(Vector2))
            {
                var resultx = EditorPrefs.GetFloat(key + "_Vector2_x");
                var resulty = EditorPrefs.GetFloat(key + "_Vector2_y");
                var result = new Vector2(resultx, resulty);
                result = EditorGUILayout.Vector2Field(label, result);
                return (T)(object)result;
            }
            else if(typeof(T) == typeof(Vector3))
            {
                var resultx = EditorPrefs.GetFloat(key + "_Vector3_x");
                var resulty = EditorPrefs.GetFloat(key + "_Vector3_y");
                var resultz = EditorPrefs.GetFloat(key + "_Vector3_y");
                var result = new Vector3(resultx, resulty, resultz);
                result = EditorGUILayout.Vector3Field(label, result);
                return (T)(object)result;
            }
            else
            {
                throw new System.NotSupportedException(typeof(T).Name + " is not a supported editor preference type");
            }
        }

        public static T GetEditorPref<T>(string key)
        {
            if(string.IsNullOrEmpty(key))
                throw new KeyNotFoundException("Cannot use a null or empty key");

            if(typeof(T) == typeof(bool))
            {
                var result = EditorPrefs.GetBool(key);
                return (T)(object)result;
            }
            else if(typeof(T) == typeof(int))
            {
                var result = EditorPrefs.GetInt(key);
                return (T)(object)result;
            }
            else if(typeof(T) == typeof(float))
            {
                var result = EditorPrefs.GetFloat(key);
                return (T)(object)result;
            }
            else if(typeof(T) == typeof(string))
            {
                var result = EditorPrefs.GetString(key);
                return (T)(object)result;
            }
            else if(typeof(T) == typeof(Vector2))
            {
                var resultx = EditorPrefs.GetFloat(key + "_Vector2_x");
                var resulty = EditorPrefs.GetFloat(key + "_Vector2_y");
                return (T)(object)(new Vector2(resultx, resulty));
            }
            else if(typeof(T) == typeof(Vector3))
            {
                var resultx = EditorPrefs.GetFloat(key + "_Vector3_x");
                var resulty = EditorPrefs.GetFloat(key + "_Vector3_y");
                var resultz = EditorPrefs.GetFloat(key + "_Vector3_z");
                return (T)(object)(new Vector3(resultx, resulty, resultz));
            }
            else
            {
                throw new System.NotSupportedException(typeof(T).Name + " is not a supported editor preference type");
            }
        }

        public static void SetEditorPref<T>(string key, T value)
        {
            if(string.IsNullOrEmpty(key))
                throw new KeyNotFoundException("Cannot use a null or empty key");

            if(typeof(T) == typeof(bool))
            {
                EditorPrefs.SetBool(key, (bool)(object)value);
            }
            else if(typeof(T) == typeof(int))
            {
                EditorPrefs.SetInt(key, (int)(object)value);
            }
            else if(typeof(T) == typeof(float))
            {
                EditorPrefs.SetFloat(key, (float)(object)value);
            }
            else if(typeof(T) == typeof(string))
            {
                EditorPrefs.SetString(key, (string)(object)value);
            }
            else if(typeof(T) == typeof(Vector2))
            {
                EditorPrefs.SetFloat(key + "_Vector2_x", ((Vector2)(object)value).x);
                EditorPrefs.SetFloat(key + "_Vector2_y", ((Vector2)(object)value).y);
            }
            else if(typeof(T) == typeof(Vector3))
            {
                EditorPrefs.SetFloat(key + "_Vector3_x", ((Vector3)(object)value).x);
                EditorPrefs.SetFloat(key + "_Vector3_y", ((Vector3)(object)value).y);
                EditorPrefs.SetFloat(key + "_Vector3_z", ((Vector3)(object)value).z);
            }
            else
            {
                throw new System.NotSupportedException(typeof(T).Name + " is not a supported editor preference type");
            }
        }

        /// <summary>
        /// Render a set of editor "tabs" and returns the selected tab's index and string name
        /// </summary>
        public static KeyValuePair<int, string> EditorPrefToolbar(string key, params string[] tabs)
        {
            if(tabs.Length <= 0)
                return new KeyValuePair<int, string>(-1, string.Empty);

            var result = EditorPrefs.GetInt(key, 0);
            result = GUILayout.Toolbar(result, tabs);
            EditorPrefs.SetInt(key, result);
            return new KeyValuePair<int, string>(result, tabs[result]);
        }

        public static void SetEditorPrefListContent<T>(string key, List<T> content)
        {
            EditorPrefs.SetString(key + "_listContent", string.Join("^", content.Select(x => x.ToString()).ToArray()));
        }

        public static List<T> GetEditorPrefListContent<T>(string key)
        {
            System.ComponentModel.TypeConverter typeConverter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));

            List<string> listContent = EditorPrefs.GetString(key + "_listContent", string.Empty).Split('^').ToList();
            return listContent.Select(x => (T)(typeConverter.ConvertFromString(x))).ToList();
        }

        public static KeyValuePair<int, T> EditorPrefList<T>(string key, string label = "", bool allowEditListItems = false)
        {
            System.ComponentModel.TypeConverter typeConverter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));

            T selected = default(T);

            int selectedIndex = EditorPrefs.GetInt(key + "_selected", -1);
            List<string> listContent = EditorPrefs.GetString(key + "_listContent", string.Empty).Split('^').ToList();


            EditorGUILayout.LabelField(label);

            EditorGUILayout.BeginHorizontal();

            if(selectedIndex >= 0)
            {
                if(allowEditListItems)
                {
                    listContent[selectedIndex] = EditorGUILayout.DelayedTextField(listContent[selectedIndex]);
                }
            }

            selectedIndex = EditorGUILayout.Popup(selectedIndex, listContent.ToArray());

            if(allowEditListItems)
            {
                if(GUILayout.Button("+"))
                {
                    selectedIndex = listContent.Count;
                    try
                    {
                        listContent.Add(default(T).ToString());
                    }
                    catch(NullReferenceException)
                    {
                        listContent.Add(string.Empty);
                    }
                }
                else if(GUILayout.Button("-"))
                {
                    if(listContent.Count < selectedIndex && selectedIndex >= 0)
                    {
                        listContent.RemoveAt(selectedIndex);
                        selectedIndex = selectedIndex - 1;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();


            EditorPrefs.SetInt(key + "_selected", selectedIndex);
            EditorPrefs.SetString(key + "_listContent", string.Join("^", listContent.ToArray()));

            if(selectedIndex >= 0 && selectedIndex < listContent.Count)
            {
                try
                {
                    selected = (T)(typeConverter.ConvertFromString(listContent[selectedIndex]));
                }
                catch(Exception)
                {
                    EditorGUILayout.HelpBox("Cannot get value from the list since it does not parse to the type " + typeof(T).Name, MessageType.Error);
                }
            }

            return new KeyValuePair<int, T>(selectedIndex, selected);
        }

        public static KeyValuePair<int, string> EditorPrefDropdown(string key, string label, List<string> content, string tooltip = null)
        {
            int selectedIndex = EditorPrefs.GetInt(key + "_selected", -1);

            if(!string.IsNullOrEmpty(tooltip))
            {
                EditorGUILayout.LabelField(new GUIContent(label, tooltip));
            }
            else
            {
                EditorGUILayout.LabelField(new GUIContent(label));
            }

            EditorGUILayout.BeginHorizontal();

            selectedIndex = EditorGUILayout.Popup(selectedIndex, content.Select(x => x.Replace('/', '-')).ToArray());
            
            EditorGUILayout.EndHorizontal();

            EditorPrefs.SetInt(key + "_selected", selectedIndex);

            string selected = string.Empty;

            if(selectedIndex >= 0 && selectedIndex < content.Count)
            {
                try
                {
                    selected = content[selectedIndex];
                }
                catch(Exception)
                {
                    selected = string.Empty;
                    selectedIndex = -1;
                }
            }

            return new KeyValuePair<int, string>(selectedIndex, selected);
        }

        static Texture2D MakeTex(int width, int height, Color textureColor, RectOffset border = null, Color? bordercolor = null)
        {
            int widthInner = width;
            if(border != null && bordercolor != null)
            {
                width += border.left;
                width += border.right;

                Color[] pix = new Color[width * (height + border.top + border.bottom)];



                for(int i = 0; i < pix.Length; i++)
                {
                    if(i < (border.bottom * width))
                        pix[i] = bordercolor.Value;
                    else if(i >= ((border.bottom * width) + (height * width)))  //Border Top
                        pix[i] = bordercolor.Value;
                    else
                    { //Center of Texture

                        if((i % width) < border.left) // Border left
                            pix[i] = bordercolor.Value;
                        else if((i % width) >= (border.left + widthInner)) //Border right
                            pix[i] = bordercolor.Value;
                        else
                            pix[i] = textureColor;    //Color texture
                    }
                }

                Texture2D result = new Texture2D(width, height + border.top + border.bottom);
                result.SetPixels(pix);
                result.Apply();
                return result;
            }
            else
            {
                Color[] pix = new Color[width * height];

                for(int i = 0; i < pix.Length; i++)
                    pix[i] = textureColor;

                Texture2D result = new Texture2D(width, height);
                result.SetPixels(pix);
                result.Apply();

                return result;
            }
        }
#endif
    }
}
