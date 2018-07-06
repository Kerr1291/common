using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
namespace Components.EditorOnly
{
    [CustomEditor(typeof(ToggleGroup))]
    public class ToggleGroup_Editor : Editor
    {
        ToggleGroup _target;

        public override void OnInspectorGUI()
        {
            _target = (ToggleGroup)target;

            if(GUILayout.Button("Activate Group"))
            {
                _target.ActivateGroup();
            }
            if(GUILayout.Button("Deactivate Group"))
            {
                _target.DeactivateGroup();
            }
            if(GUILayout.Button("Toggle Group"))
            {
                _target.Toggle();
            }
            base.OnInspectorGUI();
        }
    }
}
#endif

namespace Components
{
    public class ToggleGroup : MonoBehaviour
    {
        [Header("Control the active state of a group of game objects")]
        public GameObject[] objectGroup;

        public void SetToggleGroupActive(bool state)
        {
            foreach(GameObject obj in objectGroup)
            {
                obj.SetActive(state);
            }
        }        

        [ContextMenu("ActivateGroup")]
        public void ActivateGroup()
        {
            SetToggleGroupActive(true);
        }

        [ContextMenu("DeactivateGroup")]
        public void DeactivateGroup()
        {
            SetToggleGroupActive(false);
        }

        [ContextMenu("ToggleGroup")]
        public void Toggle()
        {
            foreach(GameObject obj in objectGroup)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }
    }

}