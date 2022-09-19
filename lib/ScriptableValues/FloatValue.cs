using UnityEngine;
using System;
using UnityEngine.Events;
using nv;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nv
{
    [CreateAssetMenu(menuName = "ScriptableValue")]
    public class FloatValue : ScriptableValue<float> { }
}
//[Serializable]
//public class FloatReference
//{
//    public bool useConstant = true;
//    public float Constant;
//    public FloatValue Data;

//    object _lock = new object();

//    public float Value
//    {
//        get
//        {
//            lock (_lock)
//            {
//                if(useConstant || Data == null)
//                    return Constant;

//                return Data.Value;
//            }
//        }
//        set
//        {
//            lock (_lock)
//            {
//                if(useConstant)
//                    Constant = value;
//                else
//                    Data.Value = value;
//            }
//        }
//    }
//}




//#if UNITY_EDITOR



//[CustomPropertyDrawer(typeof(FloatReference), true)]
//public class FloatReferenceDrawer : PropertyDrawer
//{
//    // Draw the property inside the given rect
//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        // Using BeginProperty / EndProperty on the parent property means that
//        // prefab override logic works on the entire property.
//        EditorGUI.BeginProperty(position, label, property);

//        label.tooltip = "Special type that may be either a constant or a scriptable object";

//        // Draw label
//        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

//        // Don't make child fields be indented
//        var indent = EditorGUI.indentLevel;
//        EditorGUI.indentLevel = EditorGUI.indentLevel + 1;

//        // Calculate rects
//        var labelRect = new Rect(position.x, position.y, 100, position.height);
//        var unitRect = new Rect(position.x + 100, position.y, position.width - 100, position.height);

//        // Draw fields - passs GUIContent.none to each so they are drawn without labels
//        EditorGUI.LabelField(labelRect, "Use Constant:");
//        EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("useConstant"), GUIContent.none);

//        bool useConstant = property.FindPropertyRelative("useConstant").boolValue;

//        float propertyHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("useConstant"));
//        labelRect.y += 16f;
//        labelRect.width = position.width;
//        labelRect.height = propertyHeight;

//        if(useConstant)
//        {
//            EditorGUILayout.PropertyField(property.FindPropertyRelative("Constant"));
//        }
//        else
//        {
//            EditorGUILayout.PropertyField(property.FindPropertyRelative("Data"));

//            if(property.FindPropertyRelative("Data").objectReferenceValue != null)
//            {
//                EditorGUILayout.LabelField("Value: " + (property.FindPropertyRelative("Data").objectReferenceValue as FloatValue).Value);
//            }
//        }

//        // Set indent back to what it was
//        EditorGUI.indentLevel = indent;

//        EditorGUI.EndProperty();
//    }
//}


//#endif