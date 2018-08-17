using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nv
{
    public abstract class ScriptableSingleton : ScriptableObject
    {
    }

    ///Implementation taken based on: http://wiki.unity3d.com/index.php?title=Singleton
    /// <summary>
    /// Be aware this will not prevent a non singleton constructor
    ///   such as `T myT = new T();`
    /// To prevent that, add `protected T () {}` to your singleton class.
    /// 
    /// As a note, this is made as .
    /// </summary>
    public class ScriptableSingleton<T> : ScriptableSingleton where T : ScriptableObject
    {
        private static T _instance;

        private static object _lock = new object();

        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if(_instance == null)
                    {
#if UNITY_EDITOR
                        string[] foundAssets = AssetDatabase.FindAssets("t:"+typeof(T).ToString());
                        if(foundAssets.Length > 0)
                        {
                            _instance = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(foundAssets[0]));
                        }

                        if(foundAssets.Length > 1)
#else
                        _instance = (T)FindObjectOfType(typeof(T));

                        if(FindObjectsOfType(typeof(T)).Length > 1)
#endif
                        {
                            Debug.LogError("[Singleton] More than one scriptable singleton found. " +
                                " - there should never be more than 1 singleton!" +
                                " This singleton Instance will return the first one found...");
                            return _instance;
                        }

                        if(_instance == null)
                        {
#if UNITY_EDITOR
                            CreateEditorInstance();
#else
                            CreateRuntimeInstance();
#endif
                            _instance.name = "[Singleton] " + typeof(T).ToString();

                            Debug.Log("[Singleton] An instance of " + typeof(T) +
                                " is needed, so '" + _instance +
                                "' was created.");
                        }
                        else {
                            //Debug.Log("[Singleton] Using instance already created: " +
                            //    _instance.name);
                        }
                    }

                    return _instance;
                }
            }
        }

        //TODO
        //Better idea for the scriptable object generation (since menu item doesn't work):
        //(because of editor constraints)
        //Create a custom editor window
        //In the window, show all types that are scriptable objects in the project and allow the user to select/create them
        //--
        //other idea, add a custom inspector for scriptable object scripts to allow the generation of one of them.
        
        static protected T CreateEditorInstance()
        {
#if UNITY_EDITOR
            if(_instance != null)
                return _instance;

            _instance = ScriptableObject.CreateInstance<T>();

            AssetDatabase.CreateAsset(_instance, string.Format("{0}{1}.asset", editor.Consts.EDITOR_RESOURCES_PATH, typeof(T).ToString()));
            AssetDatabase.SaveAssets();

            //EditorUtility.FocusProjectWindow();

            //Selection.activeObject = _instance;
#endif
            return _instance;
        }

        static protected T CreateRuntimeInstance()
        {
            if(_instance != null)
                return _instance;

            _instance = ScriptableObject.CreateInstance<T>();
            return _instance;
        }
    }

}