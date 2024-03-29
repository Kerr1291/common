#if UNITY_EDITOR
using System;
using System.Reflection;

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Linq;

namespace nv.editor
{
    public class BacktraceReference : EditorWindow
    {
        static void FindReferences(UnityEngine.Object to)
        {
            var objectsThatReferenceComponent = new List<UnityEngine.Object>();

            for(int s = 0; s < EditorSceneManager.sceneCount; ++s)
            {
                var rootObjects = EditorSceneManager.GetSceneAt(s).GetRootGameObjects();
                foreach(var rootGo in rootObjects)
                {
                    var objectsToSearch = rootGo.GetComponentsInChildren<Transform>(true).Select(x => x.gameObject).ToList();
                    SearchObjects(to, objectsThatReferenceComponent, objectsToSearch);
                }

                if(objectsThatReferenceComponent.Count > 0)
                    Selection.objects = objectsThatReferenceComponent.ToArray();
                else
                    Debug.Log("no references in scene");
            }
        }

        private static void SearchObjects(UnityEngine.Object to, List<UnityEngine.Object> objectsThatReferenceComponent, List<GameObject> objectsToSearch)
        {
            Component toComponent = to as Component;
            GameObject toGameObject = toComponent != null ? toComponent.gameObject : null;

            for(int j = 0; j < objectsToSearch.Count; j++)
            {
                var searchThisGameObject = objectsToSearch[j];

                if(PrefabUtility.GetPrefabType(searchThisGameObject) == PrefabType.PrefabInstance)
                {
                    if(PrefabUtility.GetPrefabObject(searchThisGameObject) == to)
                    {
                        Debug.Log(string.Format("referenced by {0}, {1}", searchThisGameObject.name, searchThisGameObject.GetType()), searchThisGameObject);
                        objectsThatReferenceComponent.Add(searchThisGameObject);
                    }
                }

                var components = searchThisGameObject.GetComponents<Component>();
                for(int i = 0; i < components.Length; i++)
                {
                    var c = components[i];
                    if(c == null)
                        continue;

                    var serializedObjectDummy = new SerializedObject(c);
                    var serializedPropertyIterator = serializedObjectDummy.GetIterator();

                    while(serializedPropertyIterator.NextVisible(true))
                    {
                        if(serializedPropertyIterator.propertyType == SerializedPropertyType.ObjectReference)
                        {
                            if(serializedPropertyIterator.objectReferenceValue == to || serializedPropertyIterator.objectReferenceValue == toGameObject)
                            {
                                Debug.Log(string.Format("referenced by {0}, {1}", c.name, c.GetType()), c);
                                objectsThatReferenceComponent.Add(c.gameObject);
                            }
                        }
                    }
                }
            }
        }

        [MenuItem("CONTEXT/Component/Find references to component")]
        static void FindReferences(MenuCommand data)
        {
            UnityEngine.Object objectFromContext = data.context;
            if(objectFromContext)
            {
                var componentToSearchFor = objectFromContext as Component;
                if(componentToSearchFor)
                    FindReferences(componentToSearchFor);
            }
        }
    }
}
#endif