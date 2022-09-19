//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using UnityEngine;
//using UnityEngine.Events;
//using Components.editor;
//using System.Collections.ObjectModel;
//using Game;

//#if UNITY_EDITOR
//using System.IO;
//using System.Text;
//using System.Xml;
//using System.Xml.Serialization;
//using UnityEditorInternal;
//using UnityEditor;
//using UnityEditor.Callbacks;

//namespace nv.editor
//{
//    [CustomPropertyDrawer(typeof(MathXMLElement), useForChildren: true)]
//    public class MathXMLElementDrawer : PropertyDrawer
//    {
//        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//        {
//            label = EditorGUI.BeginProperty(position, label, property);
//            EditorGUILayout.PrefixLabel(label);

//            EditorGUI.indentLevel += 1;

//            var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
//            MathXMLElement mathElement = obj as MathXMLElement;

//            var mathXMLs = GameMath.GetGameMath();

//            var keys = mathXMLs.Keys.ToList();
//            string mathFileID = mathElement.GetFieldValue<MathXMLElement, string>("mathFileID");
//            mathFileID = EditorGUILayoutPopup<string>("MathFile", mathFileID, keys);
//            mathElement.SetFieldValue<MathXMLElement>("mathFileID", mathFileID);

//            GameMath math = null;
//            if(!string.IsNullOrEmpty(mathFileID))
//            {
//                math = GameMath.GetGameMath()[mathFileID];
//            }

//            if(math != null)
//            {
//                string selected = ProcessXMLPath(mathElement, math);
//                mathElement.value = selected;
//            }

//            EditorGUI.indentLevel -= 1;
//            EditorGUI.EndProperty();
//        }

//        public static T EditorGUILayoutPopup<T>(string label, T current, List<T> data)
//        {
//            int currentIndex = 0;

//            currentIndex = data.IndexOf(current);

//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField(label, GUILayout.MaxWidth(120));

//            int c = 0;
//            var dataAsStrings = data.Select(x => c++ + ": " + x.ToString()).ToArray();
//            int maxString = 0;
//            for(int i = 0; i < dataAsStrings.Length; ++i)
//            {
//                maxString = Mathf.Max(dataAsStrings[i].Length, maxString);
//            }

//            int prevIndex = currentIndex;
//            currentIndex = EditorGUILayout.Popup(currentIndex, dataAsStrings, GUILayout.Width(Mathf.Max(60, 10 * maxString)));

//            if(prevIndex != currentIndex)
//                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

//            EditorGUILayout.EndHorizontal();

//            if(data.Count >= 0 && currentIndex >= 0)
//            {
//                T selected = data[currentIndex];
//                return selected;
//            }

//            return default(T);
//        }

//        public static T EditorGUILayoutPopup<T>(string label, T current, List<T> data, out int currentIndex)
//        {
//            currentIndex = data.IndexOf(current);

//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField(label, GUILayout.MaxWidth(120));

//            int c = 0;
//            var dataAsStrings = data.Select(x => c++ + ": " + x.ToString()).ToArray();
//            int maxString = 0;
//            for(int i = 0; i < dataAsStrings.Length; ++i)
//            {
//                maxString = Mathf.Max(dataAsStrings[i].Length, maxString);
//            }

//            int prevIndex = currentIndex;
//            currentIndex = EditorGUILayout.Popup(currentIndex, dataAsStrings, GUILayout.Width(Mathf.Max(60, 10 * maxString)));

//            if(prevIndex != currentIndex)
//                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

//            EditorGUILayout.EndHorizontal();

//            if(data.Count >= 0 && currentIndex >= 0)
//            {
//                T selected = data[currentIndex];
//                return selected;
//            }

//            return default(T);
//        }

//        public static T EditorGUILayoutPopup<T>(string label, T current, List<T> data, List<string> choices)
//        {
//            int currentIndex = 0;

//            currentIndex = data.IndexOf(current);

//            EditorGUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField(label, GUILayout.MaxWidth(120));
            
//            int c = 0;
//            var dataAsStrings = choices.Select(x => c++ + ": " + x).ToArray();
//            int maxString = 0;
//            for(int i = 0; i < dataAsStrings.Length; ++i)
//            {
//                maxString = Mathf.Max(dataAsStrings[i].Length, maxString);
//            }

//            int prevIndex = currentIndex;
//            currentIndex = EditorGUILayout.Popup(currentIndex, dataAsStrings, GUILayout.Width(Mathf.Max(60, 10 * maxString)));

//            if(prevIndex != currentIndex)
//                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

//            EditorGUILayout.EndHorizontal();

//            if(data.Count >= 0 && currentIndex >= 0)
//            {
//                T selected = data[currentIndex];
//                return selected;
//            }

//            return default(T);
//        }

//        public string ProcessXMLPath(MathXMLElement mathElement, GameMath math)
//        {
//            List<string> xmlPath = mathElement.GetFieldValue<MathXMLElement, List<string>>("xmlPath");

//            List<string> verifiedXMLPath = new List<string>();
//            object current = math;
//            foreach(string key in xmlPath)
//            {
//                List<string> currentKeys = new List<string>();
//                List<object> currentElements = new List<object>();
//                //Dictionary<string, object> xmlElements = new Dictionary<string, object>();
//                if(current.GetType().Name == "List`1")
//                {
//                    var list = (current as IEnumerable);
//                    foreach(object item in list)
//                    {
//                        currentKeys.Add(item.ToString());
//                        currentElements.Add(item);
//                        //xmlElements[item.ToString()] = item;
//                    }
//                }
//                else
//                {
//                    currentKeys = current.GetType().GetFields().Select(x => x.Name).ToList();
//                }

//                int selection;
//                string newKey = EditorGUILayoutPopup<string>(current.GetType().Name, key, currentKeys, out selection);

//                if(string.Compare(newKey, key) == 0)
//                {
//                    verifiedXMLPath.Add(key);
//                }
//                else
//                {
//                    break;
//                }

//                if(currentElements.Count > 0)
//                    current = currentElements[selection];
//                //current = xmlElements[key];
//                else
//                    current = current.GetType().GetField(key).GetValue(current);
//            }

//            xmlPath = verifiedXMLPath;

//            Type currentType = current.GetType();
//            if(currentType.IsValueType || currentType == typeof(string))
//            {
//                EditorGUILayout.LabelField("Selected Value: " + current.ToString());
//                mathElement.SetFieldValue<MathXMLElement>("xmlPath", xmlPath);
//                return current.ToString();
//            }
//            else
//            {
//                List<string> xmlKeys = new List<string>();
//                if(current.GetType().Name == "List`1")
//                {
//                    var list = (current as IEnumerable);
//                    foreach(object item in list)
//                    {
//                        xmlKeys.Add(item.ToString());
//                    }
//                }
//                else
//                {
//                    xmlKeys = current.GetType().GetFields().Select(x => x.Name).ToList();
//                }

//                string newKey = EditorGUILayoutPopup<string>(current.GetType().Name, string.Empty, xmlKeys);
//                if(!string.IsNullOrEmpty(newKey))
//                {
//                    xmlPath.Add(newKey);
//                }
//            }

//            mathElement.SetFieldValue<MathXMLElement>("xmlPath", xmlPath);
//            return string.Empty;
//        }
//    }
//}
//#endif

//namespace nv
//{
//    [Serializable]
//    public class MathXMLElement
//    {
//        [SerializeField]
//        protected string mathFileID;
//        public string MathFileID
//        {
//            get
//            {
//                return mathFileID;
//            }
//        }

//        [SerializeField]
//        protected List<string> xmlPath = new List<string>();

//        public GameMath MathXML
//        {
//            get
//            {
//                if(MathFileID == string.Empty)
//                    return null;

//                return GameMath.GetGameMath()[MathFileID];
//            }
//        }

//        public string value;

//        public static implicit operator string(MathXMLElement element)
//        {
//            return element.value;
//        }
//    }
//}