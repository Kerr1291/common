using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace nv
{
    /// <summary>
    /// Use this to pull a value from a unity slider and place it in a unity text object
    /// </summary>
    [ExecuteInEditMode]
    public class SliderToUITextValue : MonoBehaviour
    {
        [SerializeField]
        bool setTextInEditor;

        [SerializeField]
        private Text text;

        [SerializeField]
        private Slider slider;

        public enum DisplayType
        {
            Normal, Normalized, NormalizedPercent, Min, Max
        }

        [SerializeField]
        private DisplayType displayType = DisplayType.Normal;

        [SerializeField]
        [Header("Auto sync every frame? (Not suggested)")]
        private bool syncOnUpdate = false;

        void InternalOnValueChanged(float value)
        {
            UpdateText();
        }

        public void OnValueChanged()
        {
            UpdateText();
        }

        void Update()
        {
            if(Application.isEditor && !Application.isPlaying && !setTextInEditor)
                return;

            if(syncOnUpdate == false)
                return;

            UpdateText();
        }

        void UpdateText()
        {
            if(Application.isEditor && !Application.isPlaying && !setTextInEditor)
                return;

            if(text == null)
                return;

            if(slider == null)
                return;

            if(displayType == DisplayType.Normalized)
            {
                text.text = slider.normalizedValue.ToString();
            }
            else if(displayType == DisplayType.NormalizedPercent)
            {
                float v = slider.normalizedValue * 100.0f;
                int i_v = System.Convert.ToInt32(v);
                text.text = i_v.ToString() + "%";
            }
            else if(displayType == DisplayType.Min)
            {
                text.text = slider.minValue.ToString();
            }
            else if(displayType == DisplayType.Max)
            {
                text.text = slider.maxValue.ToString();
            }
            else
            {
                text.text = slider.value.ToString();
            }
        }

        void Awake()
        {
            slider.onValueChanged.RemoveListener(InternalOnValueChanged);
            slider.onValueChanged.AddListener(InternalOnValueChanged);
            UpdateText();
        }

        void OnValidate()
        {
            if(slider != null && gameObject.activeInHierarchy)
            {
                slider.onValueChanged.RemoveListener(InternalOnValueChanged);
                slider.onValueChanged.AddListener(InternalOnValueChanged);
            }
        }
    }

}