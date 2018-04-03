using UnityEngine;
using System;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScriptableValue<T> : ScriptableObject
{
    public T Value;
}


//[CreateAssetMenu]
//public class BoolValue : ScriptableValue<bool> { }

//[Serializable]
//public class BoolReference
//{
//    public bool useConstant = true;
//    public bool Constant;
//    public BoolValue Data;

//    object _lock = new object();

//    public bool Value
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



//[CreateAssetMenu]
//public class StringValue : ScriptableValue<string> { }

//[Serializable]
//public class StringReference
//{
//    public bool useConstant = true;
//    public string Constant;
//    public StringValue Data;

//    object _lock = new object();

//    public string Value
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




//[CreateAssetMenu]
//public class IntValue : ScriptableValue<int> { }

//[Serializable]
//public class IntReference
//{
//    public bool useConstant = true;
//    public int Constant;
//    public IntValue Data;

//    object _lock = new object();

//    public int Value
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



//[CreateAssetMenu]
//public class RectValue : ScriptableValue<Rect> { }

//[Serializable]
//public class RectReference
//{
//    public bool useConstant = true;
//    public Rect Constant;
//    public RectValue Data;

//    object _lock = new object();

//    public Rect Value
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



//[CreateAssetMenu]
//public class Vector2Value : ScriptableValue<Vector2> { }

//[Serializable]
//public class Vector2Reference
//{
//    public bool useConstant = true;
//    public Vector2 Constant;
//    public Vector2Value Data;

//    object _lock = new object();

//    public Vector2 Value
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




//[CreateAssetMenu]
//public class Vector3Value : ScriptableValue<Vector3> { }

//[Serializable]
//public class Vector3Reference
//{
//    public bool useConstant = true;
//    public Vector3 Constant;
//    public Vector3Value Data;

//    object _lock = new object();

//    public Vector3 Value
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
//[CustomPropertyDrawer(typeof(BoolReference), true)]
//public class BoolReferenceDrawer : PropertyDrawer
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
//                EditorGUILayout.LabelField("Value: " + (property.FindPropertyRelative("Data").objectReferenceValue as BoolValue).Value);
//            }
//        }
        
//        // Set indent back to what it was
//        EditorGUI.indentLevel = indent;

//        EditorGUI.EndProperty();
//    }
//}


////[CustomPropertyDrawer(typeof(IntReference), true)]
////public class IntReferenceDrawer : PropertyDrawer
////{
////    // Draw the property inside the given rect
////    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
////    {
////        // Using BeginProperty / EndProperty on the parent property means that
////        // prefab override logic works on the entire property.
////        EditorGUI.BeginProperty(position, label, property);

////        label.tooltip = "Special type that may be either a constant or a scriptable object";

////        // Draw label
////        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

////        // Don't make child fields be indented
////        var indent = EditorGUI.indentLevel;
////        EditorGUI.indentLevel = EditorGUI.indentLevel + 1;

////        // Calculate rects
////        var labelRect = new Rect(position.x, position.y, 100, position.height);
////        var unitRect = new Rect(position.x + 100, position.y, position.width - 100, position.height);

////        // Draw fields - passs GUIContent.none to each so they are drawn without labels
////        EditorGUI.LabelField(labelRect, "Use Constant:");
////        EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("useConstant"), GUIContent.none);

////        bool useConstant = property.FindPropertyRelative("useConstant").boolValue;

////        float propertyHeight = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("useConstant"));
////        labelRect.y += 16f;
////        labelRect.width = position.width;
////        labelRect.height = propertyHeight;

////        if(useConstant)
////        {
////            EditorGUILayout.PropertyField(property.FindPropertyRelative("Constant"));
////        }
////        else
////        {
////            EditorGUILayout.PropertyField(property.FindPropertyRelative("Data"));

////            if(property.FindPropertyRelative("Data").objectReferenceValue != null)
////            {
////                EditorGUILayout.LabelField("Value: " + (property.FindPropertyRelative("Data").objectReferenceValue as IntValue).Value);
////            }
////        }

////        // Set indent back to what it was
////        EditorGUI.indentLevel = indent;

////        EditorGUI.EndProperty();
////    }
////}


//[CustomPropertyDrawer(typeof(StringReference), true)]
//public class StringReferenceDrawer : PropertyDrawer
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
//                EditorGUILayout.LabelField("Value: " + (property.FindPropertyRelative("Data").objectReferenceValue as StringValue).Value);
//            }
//        }

//        // Set indent back to what it was
//        EditorGUI.indentLevel = indent;

//        EditorGUI.EndProperty();
//    }
//}







//[CustomPropertyDrawer(typeof(RectReference), true)]
//public class RectDrawer : PropertyDrawer
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
//                EditorGUILayout.LabelField("Value: " + (property.FindPropertyRelative("Data").objectReferenceValue as RectValue).Value);
//            }
//        }

//        // Set indent back to what it was
//        EditorGUI.indentLevel = indent;

//        EditorGUI.EndProperty();
//    }
//}





//[CustomPropertyDrawer(typeof(Vector2Reference), true)]
//public class Vector2ReferenceDrawer : PropertyDrawer
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
//                EditorGUILayout.LabelField("Value: " + (property.FindPropertyRelative("Data").objectReferenceValue as Vector2Value).Value);
//            }
//        }

//        // Set indent back to what it was
//        EditorGUI.indentLevel = indent;

//        EditorGUI.EndProperty();
//    }
//}



//[CustomPropertyDrawer(typeof(Vector3Reference), true)]
//public class Vector3ReferenceDrawer : PropertyDrawer
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
//                EditorGUILayout.LabelField("Value: " + (property.FindPropertyRelative("Data").objectReferenceValue as Vector3Value).Value);
//            }
//        }

//        // Set indent back to what it was
//        EditorGUI.indentLevel = indent;

//        EditorGUI.EndProperty();
//    }
//}
//#endif