using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace nv.EditorOnly
{
    public class SearchSceneForReferences
    {
        [MenuItem("CONTEXT/Component/Search open scenes for references to this component")]
        private static void FindReferences(MenuCommand data)
        {
            UnityEngine.Object context = data.context;
            if(context)
            {
                var comp = context as Component;
                if(comp)
                    FindReferencesTo(comp);
            }
        }

        private static void FindReferencesTo(UnityEngine.Object to)
        {
            var referencedBy = new List<UnityEngine.Object>();

            for(int s = 0; s < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; ++s)
            {
                UnityEngine.SceneManagement.Scene scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(s);

                if(!scene.IsValid())
                    continue;

                if(!scene.isLoaded)
                    continue;

                foreach(GameObject go in scene.GetRootGameObjects())
                {
                    if(go == null)
                        continue;

                    foreach(Transform t in go.GetComponentsInChildren<Transform>(true))
                    {
                        if(t == null)
                            continue;

                        var current = t.gameObject;

                        if(PrefabUtility.GetPrefabType(current) == PrefabType.PrefabInstance)
                        {
                            if(PrefabUtility.GetPrefabParent(current) == to)
                            {
                                Debug.Log(string.Format("referenced by {0}, {1}", current.name, current.GetType()), current);
                                referencedBy.Add(current);
                            }
                        }

                        var components = current.GetComponents<Component>();
                        for(int i = 0; i < components.Length; i++)
                        {
                            var c = components[i];
                            if(!c) continue;

                            var so = new SerializedObject(c);
                            var sp = so.GetIterator();

                            while(sp.NextVisible(true))
                                if(sp.propertyType == SerializedPropertyType.ObjectReference)
                                {
                                    if(sp.objectReferenceValue == to)
                                    {
                                        Debug.Log(string.Format("referenced by {0}, {1}", c.name, c.GetType()), c);
                                        referencedBy.Add(c.gameObject);
                                    }
                                }
                        }
                    }
                }
            }

            if(referencedBy.Count > 0)
                Selection.objects = referencedBy.ToArray();

            else Debug.Log("no references in scene");
        }
    }
}