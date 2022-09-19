#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;
using UnityEngine.UI;

namespace nv.editor
{
    [CustomEditor(typeof(SyncMemberToUI))]
    public class SyncMemberToUI_Editor : Editor
    {
        protected static BindingFlags bFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

        SyncMemberToUI Target
        {
            get
            {
                return ((SyncMemberToUI)target);
            }
        }

        protected virtual Object GetGetRootTarget()
        {
            return (Object)Target.GetType().GetField("rootGetTarget", bFlags).GetValue(Target);
        }

        protected virtual Object GetGetCurrentTarget()
        {
            return (Object)Target.GetType().GetField("targetGet", bFlags).GetValue(Target);
        }

        protected virtual Object GetSetRootTarget()
        {
            return (Object)Target.GetType().GetField("rootSetTarget", bFlags).GetValue(Target);
        }

        protected virtual Object GetSetCurrentTarget()
        {
            return (Object)Target.GetType().GetField("targetSet", bFlags).GetValue(Target);
        }

        protected virtual Object GetTargetUIElementReference()
        {
            return (Object)Target.GetType().GetField("uiElement", bFlags).GetValue(Target);
        }

        protected virtual List<MemberInfo> GetBindableMembers(Object currentTarget)
        {
            var allMembers = currentTarget.GetType().GetAllMembers();
            return allMembers.ToList();
        }

        public override void OnInspectorGUI()
        {
            //in play mode don't change any bindings, just display the raw data
            if(Application.isPlaying)
            {
                base.OnInspectorGUI();
                return;
            }

            //TODO: see why this doesn't change in the inspector???
            bool changed = EditorGUILayout.PropertyField(serializedObject.FindProperty("updateMode"));
            if(changed)
                serializedObject.ApplyModifiedProperties();

            DrawRootTargetUI(GetGetRootTarget(), "rootGetTarget", "Get");
            DrawCurrentTargetUI(GetGetCurrentTarget(), GetGetRootTarget(), "targetGet", "Get");

            Object currentTarget = GetGetCurrentTarget();
            if(currentTarget == null)
                return;

            List<MemberInfo> allMembers = GetBindableMembers(currentTarget);
            Object uiElement = GetTargetUIElementReference();

            //draw the "getter" portion of the sync
            DrawGetterMembersUI(currentTarget, allMembers, uiElement);

            if(!IsTextUI(uiElement))
            {
                DrawRootTargetUI(GetSetRootTarget(), "rootSetTarget", "Set");
                DrawCurrentTargetUI(GetSetCurrentTarget(), GetSetRootTarget(), "targetSet", "Set");

                currentTarget = GetSetCurrentTarget();
                if(currentTarget == null)
                    return;

                allMembers = GetBindableMembers(currentTarget);

                //draw the "setter" portion of the sync
                DrawSetterMembersUI(currentTarget, allMembers, uiElement);
            }
        }

        protected virtual void DrawRootTargetUI(Object rootTarget, string rootTargetFieldName, string labelPrefix = "Get")
        {
            rootTarget = EditorGUILayout.ObjectField(labelPrefix + " Target Object", rootTarget, typeof(object), true);
            Target.GetType().GetField(rootTargetFieldName, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Target, rootTarget);
        }

        protected virtual void DrawCurrentTargetUI(Object currentTarget, Object rootTarget, string currentTargetFieldName, string labelPrefix = "Get")
        {
            if(rootTarget is GameObject)
            {
                //if our root target is a game object, populate the dropdown with the components and place
                //the game object reference at the top of the list (users will expect this convention)
                List<Object> objects = new List<Object>();
                if(rootTarget is GameObject)
                    objects = (rootTarget as GameObject).GetComponents<Component>().Cast<Object>().ToList();
                objects.Insert(0, rootTarget as GameObject);

                //try to initially set the selected index by searching the list and matching the types
                int tarIndex = 0;
                var objectNames = objects.Select(x => x != null ? x.GetType().Name : "null");
                if(currentTarget != null)
                    tarIndex = objectNames.ToList().IndexOf(currentTarget.GetType().Name);

                if(tarIndex < 0)
                    tarIndex = 0;

                tarIndex = EditorGUILayout.Popup(labelPrefix + " Target Reference", tarIndex, objectNames.ToArray());

                currentTarget = objects[tarIndex];
            }
            else
            {
                currentTarget = rootTarget;
            }
            Target.GetType().GetField(currentTargetFieldName, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Target, currentTarget);
        }

        protected virtual void DrawGetterMembersUI(Object currentTarget, List<MemberInfo> allMembers, Object uiElement)
        {
            if(IsDropdownUI(uiElement))
            {
                DrawDropdownUI(currentTarget, allMembers, uiElement, "targetGetRef");
            }
            else
            {
                //get all members and methods that qualify as "getters" (methods that return something)
                var getterMembers = allMembers.Where(x => ((x as MethodInfo) == null) || (((x as MethodInfo) != null) && ((x as MethodInfo).GetParameters().Length == 0) && ((x as MethodInfo).ReturnType != typeof(void)))).ToList();
                getterMembers = FilterAllowedMembersByUIType(uiElement, getterMembers);

                if(getterMembers.Count <= 0)
                    EditorGUI.HelpBox(EditorGUILayout.GetControlRect(), "No valid Types or Get-Methods on Target Reference", MessageType.Error);
                else
                    DrawSerializableMemberInfoUI(currentTarget, getterMembers, "Get-Member", "targetGetRef");
            }
        }

        protected virtual Type GetTypeInsideEnumerable(object enumerableType)
        {
            if(enumerableType != null && enumerableType.GetType().IsArray)
            {
                return enumerableType.GetType().GetElementType();
            }
            else if(enumerableType != null && typeof(IList).IsAssignableFrom(enumerableType.GetType()))
            {
                IList list = enumerableType as IList;
                return list.GetEnumerator().GetType().GetProperty("Current").PropertyType;
            }
            else if(enumerableType != null && typeof(IEnumerable).IsAssignableFrom(enumerableType.GetType()))
            {
                var iterable = (enumerableType as IEnumerable);
                return iterable.GetEnumerator().GetType().GetProperty("Current").PropertyType;
            }
            return null;
        }

        protected virtual void DrawSetterMembersUI(Object currentTarget, List<MemberInfo> allMembers, Object uiElement)
        {
            if(!IsTextUI(uiElement))
            {
                //get all members and methods that qualify as "setters" (methods that take one parameter)
                var setterMembers = allMembers.Where(x => ((x as MethodInfo) == null) || (((x as MethodInfo) != null) && ((x as MethodInfo).GetParameters().Length == 1))).ToList();
                
                if(IsDropdownUI(uiElement))
                {
                    SerializableMemberInfo targetRef = (SerializableMemberInfo)Target.GetType().GetField("targetGetRef", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Target);                    
                    setterMembers = FilterAllowedDropdownMembers(uiElement, setterMembers, GetTypeInsideEnumerable(targetRef.GetValue(GetGetCurrentTarget())));                    
                }
                else
                {
                    setterMembers = FilterAllowedMembersByUIType(uiElement, setterMembers);
                }

                if(setterMembers.Count <= 0)
                    EditorGUI.HelpBox(EditorGUILayout.GetControlRect(), "No valid Types or Set-Methods on Target Reference", MessageType.Error);
                else
                    DrawSerializableMemberInfoUI(currentTarget, setterMembers, "Set-Member", "targetSetRef");
            }
        }

        protected virtual void DrawDropdownUI(Object currentTarget, List<MemberInfo> allMembers, Object uiElement, string fieldName)
        {
            if(!IsDropdownUI(uiElement))
                return;

            //filter methods that return a list/arry type
            var listMethodMembers = allMembers.Where(x =>
            {
                var mi = x as MethodInfo;
                if(mi == null)
                    return false;

                //filter out some basic methods we don't care about
                if(x.Name == "MemberwiseClone")
                    return false;

                if(mi.GetParameters().Length > 0)
                    return false;

                if(mi.ReturnType == typeof(void))
                    return false;

                if(mi.ReturnType.IsArray)
                    return true;

                if(typeof(IList).IsAssignableFrom(mi.ReturnType))
                    return true;

                if(typeof(IEnumerable).IsAssignableFrom(mi.ReturnType))
                    return true;

                return false;
            });

            var listNonMethodMembers = allMembers.Where(x =>
            {
                var mi = x as MethodInfo;
                if(mi != null)
                    return false;

                if(x.Name == "MemberwiseClone")
                    return false;

                var pi = x as PropertyInfo;
                var fi = x as FieldInfo;

                if(pi != null && pi.PropertyType.IsArray)
                    return true;

                if(fi != null && fi.FieldType.IsArray)
                    return true;

                if(pi != null && typeof(IList).IsAssignableFrom(pi.PropertyType))
                    return true;

                if(pi != null && typeof(IEnumerable).IsAssignableFrom(pi.PropertyType))
                    return true;

                if(fi != null && typeof(IList).IsAssignableFrom(fi.FieldType))
                    return true;

                if(fi != null && typeof(IEnumerable).IsAssignableFrom(fi.FieldType))
                    return true;

                return false;
            });

            var listMembers = listNonMethodMembers.Concat(listMethodMembers).ToList();

            if(listMembers.Count <= 0)
                EditorGUI.HelpBox(EditorGUILayout.GetControlRect(), "No Arrays or Lists on Target Reference", MessageType.Error);
            else
                DrawSerializableMemberInfoUI(currentTarget, listMembers, "List/Array Member", fieldName);
            
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawSerializableMemberInfoUI(Object currentTarget, List<MemberInfo> allMembers, string label, string fieldName)
        {
            int memIndex = 0;
            SerializableMemberInfo targetRef = (SerializableMemberInfo)Target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Target);

            memIndex = allMembers.IndexOf(targetRef.Info);

            if(memIndex < 0)
                memIndex = 0;

            var memArrayNames = allMembers.Select(x => x.Name).ToArray();
            var memArray = allMembers.ToArray();

            int prev = memIndex;
            memIndex = EditorGUILayout.Popup(label, memIndex, memArrayNames);

            if(memIndex < memArray.Length)
                targetRef.Info = memArray[memIndex];

            if(prev != memIndex)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }

        protected virtual List<MemberInfo> FilterAllowedMembersByUIType(UnityEngine.Object uiElement, List<MemberInfo> allMembers)
        {
            if(IsTextUI(uiElement))
                return FilterAllowedTextMembers(uiElement, allMembers);
            if(IsInputFieldUI(uiElement))
                return FilterAllowedInputFieldMembers(uiElement, allMembers);
            if(IsSliderUI(uiElement))
                return FilterAllowedSliderMembers(uiElement, allMembers);
            if(IsToggleUI(uiElement))
                return FilterAllowedToggleMembers(uiElement, allMembers);
            if(IsScrollbarUI(uiElement))
                return FilterAllowedScrollbarMembers(uiElement, allMembers);
            return allMembers;
        }

        protected virtual List<MemberInfo> FilterAllowedTextMembers(UnityEngine.Object uiElement, List<MemberInfo> allMembers)
        {
            if(IsTextUI(uiElement))
                return allMembers;
            return allMembers;
        }

        protected virtual List<MemberInfo> FilterAllowedInputFieldMembers(UnityEngine.Object uiElement, List<MemberInfo> allMembers)
        {
            if(IsInputFieldUI(uiElement))
                return allMembers;
            return allMembers;
        }

        protected virtual List<MemberInfo> FilterAllowedSliderMembers(UnityEngine.Object uiElement, List<MemberInfo> allMembers)
        {
            if(IsSliderUI(uiElement))
                return allMembers.FilterMembersByType<float>().Concat(allMembers.FilterMembersByType<int>()).ToList();
            return allMembers;
        }

        protected virtual List<MemberInfo> FilterAllowedToggleMembers(UnityEngine.Object uiElement, List<MemberInfo> allMembers)
        {
            if(IsToggleUI(uiElement))
                return allMembers.FilterMembersByType<bool>().ToList();
            return allMembers;
        }

        protected virtual List<MemberInfo> FilterAllowedScrollbarMembers(UnityEngine.Object uiElement, List<MemberInfo> allMembers)
        {
            if(IsScrollbarUI(uiElement))
                return allMembers.FilterMembersByType<float>().ToList();
            return allMembers;
        }

        protected virtual List<MemberInfo> FilterAllowedDropdownMembers(UnityEngine.Object uiElement, List<MemberInfo> allMembers, Type filterType)
        {
            if(IsDropdownUI(uiElement))
                return allMembers.FilterMembersByType(filterType).ToList();
            return allMembers;
        }

        protected virtual bool IsDropdownUI(Object uiElement)
        {
            return uiElement as Dropdown != null;
        }

        protected virtual bool IsScrollbarUI(Object uiElement)
        {
            return uiElement as Scrollbar != null;
        }

        protected virtual bool IsToggleUI(Object uiElement)
        {
            return uiElement as Toggle != null;
        }

        protected virtual bool IsSliderUI(Object uiElement)
        {
            return uiElement as Slider != null;
        }

        protected virtual bool IsInputFieldUI(Object uiElement)
        {
            return uiElement as InputField != null;
        }

        protected virtual bool IsTextUI(Object uiElement)
        {
            return uiElement as Text != null
            || uiElement as TextMesh != null
            || uiElement as TMPro.TextMeshPro != null
            ;
        }
    }
}
#endif