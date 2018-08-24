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
        SyncMemberToUI Target
        {
            get
            {
                return ((SyncMemberToUI)target);
            }
        }

        protected virtual Object GetRootTarget()
        {
            return (Object)Target.GetType().GetField("rootTarget", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Target);
        }

        protected virtual Object GetCurrentTarget()
        {
            return (Object)Target.GetType().GetField("target", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Target);
        }

        protected virtual Object GetTargetUIElementReference()
        {
            return (Object)Target.GetType().GetField("uiElement", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Target);
        }

        protected virtual List<MemberInfo> GetBindableMembers(Object currentTarget)
        {
            var allMembers = currentTarget.GetType().GetInstanceMembers();
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

            DrawRootTargetUI();
            DrawCurrentTargetUI();

            Object currentTarget = GetCurrentTarget();
            if(currentTarget == null)
                return;

            List<MemberInfo> allMembers = GetBindableMembers(currentTarget);
            Object uiElement = GetTargetUIElementReference();

            //draw the "getter" portion of the UI
            DrawGetterMembersUI(currentTarget, allMembers, uiElement);

            //draw the "setter" portion of the sync
            DrawSetterMembersUI(currentTarget, allMembers, uiElement);

            //dropdown UI should me moved into a new derived type, but for now it lives here and requires additional data
            DrawDropdownUI(currentTarget, allMembers, uiElement);
        }

        protected virtual void DrawRootTargetUI()
        {
            Object rootTarget = GetRootTarget();
            rootTarget = EditorGUILayout.ObjectField("Target Object", rootTarget, typeof(object), true);
            Target.GetType().GetField("rootTarget", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Target, rootTarget);
        }

        protected virtual void DrawCurrentTargetUI()
        {
            Object rootTarget = GetRootTarget();
            Object currentTarget = GetCurrentTarget();
            if(rootTarget as GameObject != null)
            {
                //if our root target is a game object, populate the dropdown with the components and place
                //the game object reference at the top of the list (users will expect this convention)
                List<Object> objects = new List<Object>();
                objects = (rootTarget as GameObject).GetComponents<Component>().Cast<Object>().ToList();
                objects.Insert(0, rootTarget as GameObject);

                //try to initially set the selected index by searching the list and matching the types
                int tarIndex = 0;
                if(currentTarget != null)
                    tarIndex = objects.Select(x => x != null ? x.GetType().Name : "null").ToList().IndexOf(currentTarget.GetType().Name);

                if(tarIndex < 0)
                    tarIndex = 0;

                tarIndex = EditorGUILayout.Popup("Target Reference", tarIndex, objects.Select(x => x != null ? x.GetType().Name : "null").ToArray());

                currentTarget = objects[tarIndex];
            }
            else
            {
                currentTarget = rootTarget;
            }
            Target.GetType().GetField("target", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Target, currentTarget);
        }

        protected virtual void DrawGetterMembersUI(Object currentTarget, List<MemberInfo> allMembers, Object uiElement)
        {
            //get all members and methods that qualify as "getters" (methods that return something)
            var getterMembers = allMembers.Where(x => ((x as MethodInfo) == null) || (((x as MethodInfo) != null) && ((x as MethodInfo).GetParameters().Length == 0) && ((x as MethodInfo).ReturnType != typeof(void)))).ToList();
            getterMembers = FilterAllowedMembersByUIType(uiElement, getterMembers);

            if(getterMembers.Count <= 0)
                EditorGUILayout.LabelField("No valid Types or Get-Methods on Target Reference");
            else
                DrawSerializableMemberInfoUI(currentTarget, getterMembers, "Get-Member", "targetRef");
        }

        protected virtual void DrawSetterMembersUI(Object currentTarget, List<MemberInfo> allMembers, Object uiElement)
        {
            if(IsTextUI(uiElement))
                return;

            //get all members and methods that qualify as "setters" (methods that take one parameter)
            var setterMembers = allMembers.Where(x => ((x as MethodInfo) == null) || (((x as MethodInfo) != null) && ((x as MethodInfo).GetParameters().Length == 1))).ToList();
            setterMembers = FilterAllowedMembersByUIType(uiElement, setterMembers);

            if(setterMembers.Count <= 0)
                EditorGUILayout.LabelField("No valid Types or Set-Methods on Target Reference");
            else
                DrawSerializableMemberInfoUI(currentTarget, setterMembers, "Set-Member", "targetSetRef");
        }

        protected virtual void DrawDropdownUI(Object currentTarget, List<MemberInfo> allMembers, Object uiElement)
        {
            if(!IsDropdownUI(uiElement))
                return;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("useListSourceRef"));

            bool useListRef = (bool)Target.GetType().GetField("useListSourceRef", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Target);

            if(!useListRef)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("useListSourceCount"));

            bool useListCount = (bool)Target.GetType().GetField("useListSourceCount", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Target);

            if(useListRef)
                useListCount = false;

            if(useListRef)
            {
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
                    if(mi.ReturnType.GetInterfaces().Contains(typeof(IList)))
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

                    if(pi != null && pi.PropertyType.GetInterfaces().Contains(typeof(IList)))
                        return true;

                    if(fi != null && fi.FieldType.GetInterfaces().Contains(typeof(IList)))
                        return true;

                    return false;
                });

                var listMembers = listNonMethodMembers.Concat(listMethodMembers).ToList();

                if(listMembers.Count <= 0)
                    EditorGUILayout.LabelField("No Arrays or Lists on Target Reference");
                else
                    DrawSerializableMemberInfoUI(currentTarget, listMembers, "List/Array Member", "listSourceRef");
            }

            if(useListCount)
            {
                var countMembers = allMembers.Where(x => ((x as MethodInfo) == null) || (((x as MethodInfo) != null) && ((x as MethodInfo).GetParameters().Length == 0) && ((x as MethodInfo).ReturnType != typeof(void)))).ToList();
                countMembers = countMembers.FilterMembersByType<int>();

                if(countMembers.Count <= 0)
                    EditorGUILayout.LabelField("No valid Types or Get-Methods on Target Reference");
                else
                    DrawSerializableMemberInfoUI(currentTarget, countMembers, "Count Member", "listSourceRef");
            }

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawSerializableMemberInfoUI(Object currentTarget, List<MemberInfo> allMembers, string label, string fieldName)
        {
            if(currentTarget == null)
                return;

            int memIndex = 0;
            SerializableMemberInfo targetRef = (SerializableMemberInfo)Target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Target);

            memIndex = allMembers.IndexOf(targetRef.Info);

            if(memIndex < 0)
                memIndex = 0;

            var memArrayNames = allMembers.Select(x => x.Name).ToArray();
            var memArray = allMembers.ToArray();

            memIndex = EditorGUILayout.Popup(label, memIndex, memArrayNames);

            if(memIndex < memArray.Length)
                targetRef.Info = memArray[memIndex];
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
            if(IsDropdownUI(uiElement))
                return FilterAllowedDropdownMembers(uiElement, allMembers);
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

        protected virtual List<MemberInfo> FilterAllowedDropdownMembers(UnityEngine.Object uiElement, List<MemberInfo> allMembers)
        {
            if(IsDropdownUI(uiElement))
                return allMembers.FilterMembersByType<int>().ToList();
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
            return uiElement as Text != null;
        }
    }
}
#endif