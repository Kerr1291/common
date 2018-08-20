using UnityEngine;
using UnityEngine.UI;

namespace nv
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Text))]
    public class ShowMemberInUIText : MonoBehaviour
    {
        [SerializeField]
        Object target;

        [SerializeField]
        Object rootTarget;

        [SerializeField]
        SerializableMemberInfo targetRef;

        [SerializeField]
        Text uiText;

        void Reset()
        {
            uiText = GetComponent<Text>();
        }

        void Update()
        {
            if(target != null && rootTarget != null)
            {
                //TODO: find out why this is returning null
                object targetValue = targetRef.GetValue(target);

                if(targetValue != null)
                    uiText.text = targetValue.ToString();
                else
                    uiText.text = "null";

                return;
            }

            uiText.text = "";
        }
    }
}