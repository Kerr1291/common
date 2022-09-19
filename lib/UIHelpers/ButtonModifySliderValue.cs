using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace nv
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class ButtonModifySliderValue : MonoBehaviour
    {
        [Header("Slider to effect with button presses")]
        public Slider slider;

        [Header("How much to change the slider on click")]
        public float delta = 1.0f;

        Button source;

        public void Reset()
        {
            source = GetComponent<Button>();
            source.onClick.RemoveListener(OnClick);
            source.onClick.AddListener(OnClick);
        }

        public void OnDestroy()
        {
            if(source != null)
                source.onClick.RemoveListener(OnClick);
        }

        public void OnClick()
        {
            if(slider == null)
            {
                Debug.LogError("No slider assigned! Button click does nothing!");
                return;
            }

            slider.value += delta;
        }
    }
}