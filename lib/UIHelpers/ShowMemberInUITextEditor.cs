#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace nv.editor
{
    [CustomEditor(typeof(ShowMemberInUIText))]
    public class ShowMemberInUIText_Editor : Editor
    {
        ShowMemberInUIText Target
        {
            get
            {
                return ((ShowMemberInUIText)target);
            }
        }

        public override void OnInspectorGUI()
        {
            Object currentTarget = (Object)Target.GetType().GetField("target", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Target);
            Object rootTarget = (Object)Target.GetType().GetField("rootTarget", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Target);

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
                var publicMembers = currentTarget.GetType().GetMembers().Select(x => x);
                var nonpublicMembers = currentTarget.GetType().GetMembers(BindingFlags.NonPublic).Select(x => x);

                var allMembers = publicMembers.Concat(nonpublicMembers).ToList();
                var allNonMethods = allMembers.Where(x => (x as MethodInfo) == null).ToList();

                int memIndex = 0;
                if(currentTarget != null)
                {
                    SerializableMemberInfo targetRef = (SerializableMemberInfo)Target.GetType().GetField("targetRef", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Target);

                    memIndex = allNonMethods.IndexOf(targetRef.Info);

                    if(memIndex < 0)
                        memIndex = 0;

                    var memArrayNames = allNonMethods.Select(x => x.Name).ToArray();
                    var memArray = allNonMethods.ToArray();

                    memIndex = EditorGUILayout.Popup("Member", memIndex, memArrayNames);

                    if(memIndex < memArray.Length)
                        targetRef.Info = memArray[memIndex];
                }
            }
        }
    }
}
#endif