﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nv
{
    public static class GameObjectExtensions
    {
        public static bool ContainsType<T>(this UnityEngine.Object obj)
        {
            if(obj is GameObject || obj is Component)
            {
                if(obj is GameObject)
                {
                    return (obj as GameObject).GetComponents<Component>().Any(x => typeof(T).IsAssignableFrom(x.GetType()));
                }
                else// if(obj is Component)
                {
                    return (obj as Component).GetComponents<Component>().Any(x => typeof(T).IsAssignableFrom(x.GetType()));
                }
            }
            return false;
        }
		
        public static bool FindAndDestroyGameObjectInChildren( this GameObject gameObject, string name )
        {
            bool found = false;
            GameObject toDestroy = gameObject.FindGameObjectInChildrenWithName(name);
            if( toDestroy != null )
            {
                GameObject.Destroy( toDestroy );
                found = true;
            }
            return found;
        }

        public static List<GameObject> GetDirectChildren(this GameObject gameObject)
        {
            List<GameObject> children = new List<GameObject>();
            if(gameObject == null)
                return children;

            for(int k = 0; k < gameObject.transform.childCount; ++k)
            {
                Transform child = gameObject.transform.GetChild(k);
                children.Add(child.gameObject);
            }
            return children;
        }

        public static GameObject FindGameObjectInDirectChildren(this GameObject gameObject, string name)
        {
            if(gameObject == null)
                return null;

            for(int k = 0; k < gameObject.transform.childCount; ++k)
            {
                Transform child = gameObject.transform.GetChild(k);
                if(child.name == name)
                    return child.gameObject;
            }
            return null;
        }

        public static GameObject FindGameObjectInChildrenWithName( this GameObject gameObject, string name )
        {
            if( gameObject == null )
                return null;

            foreach( var t in gameObject.GetComponentsInChildren<Transform>( true ) )
            {
                if( t.name == name )
                    return t.gameObject;
            }
            return null;
        }

        public static GameObject FindGameObjectNameContainsInChildren( this GameObject gameObject, string name )
        {
            if( gameObject == null )
                return null;

            foreach( var t in gameObject.GetComponentsInChildren<Transform>( true ) )
            {
                if( t.name.Contains( name ) )
                    return t.gameObject;
            }
            return null;
        }

        public static TComponent FindObjectOfType<TComponent>(bool includeInactive = true)
            where TComponent : Component
        {
            return FindObjectsOfType<TComponent>(includeInactive).FirstOrDefault();
        }

        public static List<TComponent> FindObjectsOfType<TComponent>(bool includeInactive = true, bool searchLoadingScenes = true)
            where TComponent : Component
        {
            List<TComponent> components = new List<TComponent>();
#if UNITY_EDITOR
            if(Application.isPlaying)
            {
                for(int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
                {
                    Scene s = (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));
                    if(!s.IsValid())
                        continue;
                    if(!searchLoadingScenes && !s.isLoaded)
                        continue;
                    var rootObjects = s.GetRootGameObjects();
                    foreach(var rootObject in rootObjects)
                    {
                        var objectsOfType = rootObject.GetComponentsInChildren<TComponent>(includeInactive);
                        if(objectsOfType.Length > 0)
                            components.AddRange(objectsOfType);
                    }
                }
            }
            else
            {
                for(int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; ++i)
                {
                    Scene s = (UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i));
                    if(!s.IsValid())
                        continue;
                    if(!searchLoadingScenes && !s.isLoaded)
                        continue;
                    var rootObjects = s.GetRootGameObjects();
                    foreach(var rootObject in rootObjects)
                    {
                        var objectsOfType = rootObject.GetComponentsInChildren<TComponent>(includeInactive);
                        if(objectsOfType.Length > 0)
                            components.AddRange(objectsOfType);
                    }
                }
            }
#else
            for(int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
            {
                Scene s = (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));
                if(!s.IsValid())
                    continue;
                if(!searchLoadingScenes && !s.isLoaded)
                    continue;
                var rootObjects = s.GetRootGameObjects();
                foreach(var rootObject in rootObjects)
                {
                    var objectsOfType = rootObject.GetComponentsInChildren<TComponent>(includeInactive);
                    if(objectsOfType.Length > 0)
                        components.AddRange(objectsOfType);
                }
            }
#endif
            return components;
        }

        public static IEnumerable<GameObject> EnumerateRootObjects(bool includeInactive = true)
        {
#if UNITY_EDITOR
            if(Application.isPlaying)
            {
                for(int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
                {
                    Scene s = (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));
                    if(!s.IsValid())
                        continue;
                    if(!s.isLoaded)
                        continue;
                    var rootObjects = s.GetRootGameObjects();
                    foreach(var rootObject in rootObjects)
                    {
                        yield return rootObject;
                    }
                }
            }
            else
            {
                for(int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; ++i)
                {
                    Scene s = (UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i));
                    if(!s.IsValid())
                        continue;
                    if(!s.isLoaded)
                        continue;
                    var rootObjects = s.GetRootGameObjects();
                    foreach(var rootObject in rootObjects)
                    {
                        yield return rootObject;
                    }
                }
            }
#else
            for(int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
            {
                Scene s = (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));
                if(!s.IsValid())
                    continue;
                if(!s.isLoaded)
                    continue;
                var rootObjects = s.GetRootGameObjects();
                foreach(var rootObject in rootObjects)
                {
                    yield return rootObject;
                }
            }
#endif
            yield break;
        }

        public static IEnumerable<TComponent> EnumerateRootObjects<TComponent>(bool includeInactive = true)
            where TComponent : Component
        {
#if UNITY_EDITOR
            if(Application.isPlaying)
            {
                for(int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
                {
                    Scene s = (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));
                    if(!s.IsValid())
                        continue;
                    if(!s.isLoaded)
                        continue;
                    var rootObjects = s.GetRootGameObjects();
                    foreach(var rootObject in rootObjects)
                    {
                        yield return rootObject.GetComponent<TComponent>();
                    }
                }
            }
            else
            {
                for(int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; ++i)
                {
                    Scene s = (UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i));
                    if(!s.IsValid())
                        continue;
                    if(!s.isLoaded)
                        continue;
                    var rootObjects = s.GetRootGameObjects();
                    foreach(var rootObject in rootObjects)
                    {
                        yield return rootObject.GetComponent<TComponent>();
                    }
                }
            }
#else
            for(int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
            {
                Scene s = (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));
                if(!s.IsValid())
                    continue;
                if(!s.isLoaded)
                    continue;
                var rootObjects = s.GetRootGameObjects();
                foreach(var rootObject in rootObjects)
                {
                    yield return rootObject.GetComponent<TComponent>();
                }
            }
#endif
            yield break;
        }

        public static IEnumerable<TComponent> EnumerateComponentsInChildren<TComponent>(this GameObject go, bool includeInactive = true)
            where TComponent : Component
        {
            yield return go.GetComponent<TComponent>();
            for(int i = 0; i < go.transform.childCount; ++i)
            {
                var child = go.transform.GetChild(i);
                if(!child.gameObject.activeInHierarchy && !includeInactive)
                    continue;
                foreach(var c in EnumerateComponentsInChildren<TComponent>(child.gameObject, includeInactive))
                {
                    yield return c;
                }
            }
        }

        public static IEnumerable<TComponent> EnumerateObjectsOfType<TComponent>(bool includeInactive = true)
            where TComponent : Component
        {
#if UNITY_EDITOR
            if(Application.isPlaying)
            {
                for(int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
                {
                    Scene s = (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));
                    if(!s.IsValid())
                        continue;
                    if(!s.isLoaded)
                        continue;
                    var rootObjects = s.GetRootGameObjects();
                    foreach(var rootObject in rootObjects)
                    {
                        foreach(var c in rootObject.EnumerateComponentsInChildren<TComponent>(includeInactive))
                        {
                            yield return c;
                        }
                    }
                }
            }
            else
            {
                for(int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; ++i)
                {
                    Scene s = (UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i));
                    if(!s.IsValid())
                        continue;
                    if(!s.isLoaded)
                        continue;
                    var rootObjects = s.GetRootGameObjects();
                    foreach(var rootObject in rootObjects)
                    {
                        foreach(var c in rootObject.EnumerateComponentsInChildren<TComponent>(includeInactive))
                        {
                            yield return c;
                        }
                    }
                }
            }
#else
            for(int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
            {
                Scene s = (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));
                if(!s.IsValid())
                    continue;
                if(!s.isLoaded)
                    continue;
                var rootObjects = s.GetRootGameObjects();
                foreach(var rootObject in rootObjects)
                {
                    foreach(var c in rootObject.EnumerateComponentsInChildren<TComponent>(includeInactive))
                    {
                        yield return c;
                    }
                }
            }
#endif
            yield break;
        }

        public static GameObject FindGameObject(string pathName)
        {
            string[] path = pathName.Trim('/').Split('/');

            //Dev.Log("Searching " + string.Join(", ",path));
            //Dev.LogVarArray("splitpath", path);

            if(path.Length <= 0)
                return null;

            GameObject root = null;

            //search for a game object with a name that matches the first string
            for(int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
            {
                Scene s = (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));
                if(!s.IsValid() || !s.isLoaded)
                    continue;
                //Dev.Log("Searching " + s.name);
                root = s.GetRootGameObjects().Where(x => string.Compare(x.name,path[0]) == 0).FirstOrDefault();
                //Dev.LogVarArray("root scene object", s.GetRootGameObjects().Select(x => x.name).ToArray());

                if(root != null)
                    break;
            }

            //if(root == null)
            //{
            //    Dev.Log("did not find a root object");
            //}
            
            if(root == null)
                return null;

            return root.FindGameObject(pathName);
        }

        public static GameObject FindGameObject(this GameObject gameObject, string pathName)
        {
            string[] path = pathName.Trim('/').Split('/');

            if(gameObject.name != path[0])
                return null;

            List<string> remainingPath = new List<string>(path);
            remainingPath.RemoveAt(0);

            if(remainingPath.Count <= 0)
                return gameObject;

            string subPath = string.Join("/", remainingPath.ToArray());

            var children = gameObject.GetDirectChildren();

            foreach(var child in children)
            {
                GameObject found = child.FindGameObject(subPath);
                if(found != null)
                    return found;
            }

            return null;
        }

        public static string GetSceneHierarchyPath( this GameObject gameObject )
        {
            if( gameObject == null )
                return "null";

            string objStr = gameObject.name;

            if( gameObject.transform.parent != null )
                objStr = gameObject.transform.parent.gameObject.GetSceneHierarchyPath() + "/" + gameObject.name;

            return objStr;
        }

        public static IEnumerable<GameObject> EnumerateChildren(this GameObject gameObject)
        {
            if(gameObject == null)
                yield break;

            //string parentObject = gameObject.GetSceneHierarchyPath();
            //Debug.Log(parentObject);

            for(int k = 0; k < gameObject.transform.childCount; ++k)
            {
                Transform child = gameObject.transform.GetChild(k);
                yield return child.gameObject;
                //string objectNameAndPath = child.gameObject.GetSceneHierarchyPath();

                //string inactiveString = string.Empty;
                //if(child != null && child.gameObject != null && !child.gameObject.activeInHierarchy)
                //    inactiveString = " (inactive)";

                //Debug.Log(objectNameAndPath + inactiveString);


                //if(printComponents)
                //{
                //    string componentHeader = "";
                //    for(int i = 0; i < (objectNameAndPath.Length - child.gameObject.name.Length); ++i)
                //        componentHeader += " ";

                //    foreach(Component c in child.GetComponents<Component>())
                //    {
                //        c.PrintComponentType(componentHeader, file);

                //        if(c is Transform)
                //            c.PrintTransform(componentHeader, file);
                //        else
                //            c.PrintComponentWithReflection(componentHeader, file);
                //    }
                //}
            }
        }

        public static IEnumerable<TComponent> EnumerateChildren<TComponent>(this GameObject gameObject)
             where TComponent : Component
        {
            if(gameObject == null)
                yield break;

            //string parentObject = gameObject.GetSceneHierarchyPath();
            //Debug.Log(parentObject);

            for(int k = 0; k < gameObject.transform.childCount; ++k)
            {
                Transform child = gameObject.transform.GetChild(k);
                TComponent c = child.GetComponent<TComponent>();
                if(c == null)
                    continue;
                yield return c;
                //string objectNameAndPath = child.gameObject.GetSceneHierarchyPath();

                //string inactiveString = string.Empty;
                //if(child != null && child.gameObject != null && !child.gameObject.activeInHierarchy)
                //    inactiveString = " (inactive)";

                //Debug.Log(objectNameAndPath + inactiveString);


                //if(printComponents)
                //{
                //    string componentHeader = "";
                //    for(int i = 0; i < (objectNameAndPath.Length - child.gameObject.name.Length); ++i)
                //        componentHeader += " ";

                //    foreach(Component c in child.GetComponents<Component>())
                //    {
                //        c.PrintComponentType(componentHeader, file);

                //        if(c is Transform)
                //            c.PrintTransform(componentHeader, file);
                //        else
                //            c.PrintComponentWithReflection(componentHeader, file);
                //    }
                //}
            }
        }

        public static void PrintSceneHierarchyChildren(this GameObject gameObject, bool printComponents = false, System.IO.StreamWriter file = null)
        {
            if(gameObject == null)
                return;

            if(file != null)
            {
                file.WriteLine("START =====================================================");
                file.WriteLine("Printing scene hierarchy for game object: " + gameObject.name);
            }
            else
            {
                Debug.Log("START =====================================================");
                Debug.Log("Printing scene hierarchy for game object: " + gameObject.name);
            }

            string parentObject = gameObject.GetSceneHierarchyPath();

            if(file != null)
            {
                file.WriteLine(parentObject);
            }
            else
            {
                Debug.Log(parentObject);
            }

            for(int k = 0; k < gameObject.transform.childCount; ++k)
            {
                Transform child = gameObject.transform.GetChild(k);
                string objectNameAndPath = child.gameObject.GetSceneHierarchyPath();

                string inactiveString = string.Empty;
                if(child != null && child.gameObject != null && !child.gameObject.activeInHierarchy)
                    inactiveString = " (inactive)";

                if(file != null)
                {
                    file.WriteLine(objectNameAndPath + inactiveString);
                }
                else
                {
                    Debug.Log(objectNameAndPath + inactiveString);
                }


                if(printComponents)
                {
                    string componentHeader = "";
                    for(int i = 0; i < (objectNameAndPath.Length - child.gameObject.name.Length); ++i)
                        componentHeader += " ";

                    foreach(Component c in child.GetComponents<Component>())
                    {
                        c.PrintComponentType(componentHeader, file);

                        if(c is Transform)
                            c.PrintTransform(componentHeader, file);
                        else
                            c.PrintComponentWithReflection(componentHeader, file);
                    }
                }
            }

            if(file != null)
            {
                file.WriteLine("END +++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            }
            else
            {
                Debug.Log("END +++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            }
        }

        public static void PrintSceneHierarchyTree( this GameObject gameObject, bool printComponents = false, System.IO.StreamWriter file = null )
        {
            if( gameObject == null )
                return;

            if( file != null )
            {
                file.WriteLine( "START =====================================================" );
                file.WriteLine( "Printing scene hierarchy for game object: " + gameObject.name );
            }
            else
            {
                Debug.Log( "START =====================================================" );
                Debug.Log( "Printing scene hierarchy for game object: " + gameObject.name );
            }

            foreach( Transform t in gameObject.GetComponentsInChildren<Transform>( true ) )
            {
                string objectNameAndPath = t.gameObject.GetSceneHierarchyPath();

                string inactiveString = string.Empty;
                if(t != null && t.gameObject != null && !t.gameObject.activeInHierarchy)
                    inactiveString = " (inactive)";

                if( file != null )
                {
                    file.WriteLine( objectNameAndPath + inactiveString);
                }
                else
                {
                    Debug.Log( objectNameAndPath + inactiveString);
                }


                if( printComponents )
                {
                    string componentHeader = "";
                    for( int i = 0; i < ( objectNameAndPath.Length - t.gameObject.name.Length ); ++i )
                        componentHeader += " ";

                    foreach( Component c in t.GetComponents<Component>() )
                    {
                        c.PrintComponentType( componentHeader, file );

                        if(c is Transform)
                            c.PrintTransform(componentHeader, file);
                        else
                            c.PrintComponentWithReflection(componentHeader, file);
                    }
                }
            }

            if( file != null )
            {
                file.WriteLine( "END +++++++++++++++++++++++++++++++++++++++++++++++++++++++" );
            }
            else
            {
                Debug.Log( "END +++++++++++++++++++++++++++++++++++++++++++++++++++++++" );
            }
        }

        public static T GetOrAddComponent<T>( this GameObject source ) where T : UnityEngine.Component
        {
            T result = source.GetComponent<T>();
            if( result != null )
                return result;
            result = source.AddComponent<T>();
            return result;
        }


        public static void SafeSetActive( this GameObject go, bool state )
        {
            if( go == null )
                return;

            go.SetActive( state );
        }


        public static bool SafeIsActive( this GameObject go )
        {
            if( go == null )
                return false;

            return go.activeInHierarchy;
        }
    }
}
