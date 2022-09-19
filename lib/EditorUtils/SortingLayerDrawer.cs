using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace nv
{
    /// Used to mark an 'int' field as a sorting layer so it will use the SortingLayerDrawer to display in the Inspector window.
    public class SortingLayerAttribute : PropertyAttribute
    {
    }
}

#if UNITY_EDITOR
namespace nv.editor
{
    //taken from: https://github.com/nickgravelyn/UnityToolbag/blob/master/SortingLayer/SortingLayerAttribute.cs
    [CustomPropertyDrawer(typeof(SortingLayerAttribute))]
    public class SortingLayerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sortingLayerNames = SortingLayer.layers.Select(l => l.name).ToArray();
            if(property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.HelpBox(position, string.Format("{0} is not an integer but has [SortingLayer].", property.name), MessageType.Error);
            }
            else if(sortingLayerNames != null)
            {
                EditorGUI.BeginProperty(position, label, property);

                // Look up the layer name using the current layer ID
                string oldName = SortingLayer.IDToName(property.intValue);

                // Use the name to look up our array index into the names list
                int oldLayerIndex = Array.IndexOf(sortingLayerNames, oldName);

                // Show the popup for the names
                int newLayerIndex = EditorGUI.Popup(position, label.text, oldLayerIndex, sortingLayerNames);

                // If the index changes, look up the ID for the new index to store as the new ID
                if(newLayerIndex != oldLayerIndex)
                {
                    property.intValue = SortingLayer.NameToID(sortingLayerNames[newLayerIndex]);
                }

                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.BeginProperty(position, label, property);
                int newValue = EditorGUI.IntField(position, label.text, property.intValue);
                if(newValue != property.intValue)
                {
                    property.intValue = newValue;
                }
                EditorGUI.EndProperty();
            }
        }
    }
}
#endif