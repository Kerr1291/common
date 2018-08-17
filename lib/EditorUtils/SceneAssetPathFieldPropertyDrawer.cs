using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
namespace nv.editor
{
    [CustomPropertyDrawer(typeof(SceneAssetPathField))]
    public class SceneAssetPathFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var oldPath = AssetDatabase.LoadAssetAtPath<SceneAsset>(property.stringValue);

            position = EditorGUI.PrefixLabel(position, new GUIContent(property.displayName));

            EditorGUI.BeginChangeCheck();
            var newScene = EditorGUI.ObjectField(position, oldPath, typeof(SceneAsset), false) as SceneAsset;

            if(EditorGUI.EndChangeCheck())
            {
                var newPath = AssetDatabase.GetAssetPath(newScene);
                property.stringValue = newPath;
            }
        }
    }

}
#endif