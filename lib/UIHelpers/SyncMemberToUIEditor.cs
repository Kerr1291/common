#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

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

        public override void OnInspectorGUI()
        {
            if(Application.isPlaying)
            {
                base.OnInspectorGUI();
                return; 
            }

            Object rootTarget = (Object)Target.GetType().GetField("rootTarget", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Target);
            Object currentTarget = (Object)Target.GetType().GetField("target", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Target);

            rootTarget = EditorGUILayout.ObjectField("Target Object", rootTarget, typeof(object), true);

            if(rootTarget as GameObject != null)
            {
                List<Object> objects = new List<Object>();
                objects = (rootTarget as GameObject).GetComponents<Component>().Cast<Object>().ToList();
                objects.Insert(0, rootTarget as GameObject);

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
            Target.GetType().GetField("rootTarget", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Target, rootTarget);

            if(currentTarget != null)
            {
                var publicMembers = currentTarget.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance).Select(x => x);
                var nonpublicMembers = currentTarget.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Instance).Select(x => x);

                var allMembers = publicMembers.Concat(nonpublicMembers).ToList();
                allMembers = allMembers.Where(x => x.GetCustomAttribute(typeof(ObsoleteAttribute)) == null).ToList();
                allMembers = allMembers.Where(x => ((x as MethodInfo) == null) || (((x as MethodInfo) != null) && !((x as MethodInfo).IsSpecialName) && !((x as MethodInfo).ContainsGenericParameters))).ToList();

                var getterMembers = allMembers.Where(x => ((x as MethodInfo) == null) || (((x as MethodInfo) != null) && ((x as MethodInfo).GetParameters().Length == 0) && ((x as MethodInfo).ReturnType != typeof(void)))).ToList();

                getterMembers = Target.FilterAllowedMembersByUIType(getterMembers);

                if(getterMembers.Count <= 0)
                    EditorGUILayout.LabelField("No valid Types or Get-Methods on Target Reference");
                else
                    DrawTargetRefUI(currentTarget, getterMembers, "Get-Member", "targetRef", true);

                var uiElement = (Object)Target.GetType().GetField("uiElement", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Target);

                if(uiElement as UnityEngine.UI.Text == null)
                {
                    var setterMembers = allMembers.Where(x => ((x as MethodInfo) == null) || (((x as MethodInfo) != null) && ((x as MethodInfo).GetParameters().Length == 1))).ToList();

                    setterMembers = Target.FilterAllowedMembersByUIType(setterMembers);

                    if(setterMembers.Count <= 0)
                        EditorGUILayout.LabelField("No valid Types or Set-Methods on Target Reference");
                    else
                        DrawTargetRefUI(currentTarget, setterMembers, "Set-Member", "targetSetRef", false);
                }
            } 
        }

        void DrawTargetRefUI(Object currentTarget, List<MemberInfo> allMembers, string label, string fieldName, bool isGetter)
        {           
            int memIndex = 0; 
            if(currentTarget != null)
            {
                SerializableMemberInfo targetRef = (SerializableMemberInfo)Target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Target);

                memIndex = allMembers.IndexOf(targetRef.Info);

                if(memIndex < 0)
                {
                    memIndex = 0;
                }

                var memArrayNames = allMembers.Select(x => x.Name).ToArray();
                var memArray = allMembers.ToArray();

                memIndex = EditorGUILayout.Popup(label, memIndex, memArrayNames);

                if(memIndex < memArray.Length)
                {
                    targetRef.Info = memArray[memIndex];
                }
            }
        }
    }
}
#endif