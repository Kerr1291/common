using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace mods.Internal.MediaServiceAPI
{
    [RequireComponent(typeof(Text))]
    public class ColorLabel : MonoBehaviour
    {
        [HideInInspector]
        public ColorPicker picker;

        [HideInInspector]
        public ColorValues type;

        [HideInInspector]
        public string prefix = "R: ";
        public float minValue = 0;
        public float maxValue = 255;

        public int precision = 0;

        private Text label;

        private void Awake()
        {
            label = GetComponent<Text>();

        }

        private void OnEnable()
        {
            if(Application.isPlaying && picker != null)
            {
                picker.onValueChanged.AddListener(ColorChanged);
                picker.onHSVChanged.AddListener(HSVChanged);
            }
        }

        private void OnDestroy()
        {
            if(picker != null)
            {
                picker.onValueChanged.RemoveListener(ColorChanged);
                picker.onHSVChanged.RemoveListener(HSVChanged);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(picker == null)
                picker = GetComponentInParent<ColorPicker>();

            GameObject sliderParent = transform.parent.gameObject;
            char typeName = sliderParent.name[0];
            try
            {
                var types = Enum.GetNames(typeof(ColorValues));
                var selected = types.FirstOrDefault(x => x[0] == typeName);

                type = (ColorValues)System.Enum.Parse(typeof(ColorValues), selected);
                prefix = typeName + ": ";
            }
            catch(Exception e)
            {
                Debug.LogError("Failed to parse color slider type from game object name. Please make sure the parent of the color slider referenced on this object has a name that starts with R/G/B or H/S/V");
            }

            label = GetComponent<Text>();
            UpdateValue();
        }
#endif

        private void ColorChanged(Color color)
        {
            UpdateValue();
        }

        private void HSVChanged(float hue, float sateration, float value)
        {
            UpdateValue();
        }

        private void UpdateValue()
        {
            if(picker == null)
            {
                label.text = prefix + "-";
            }
            else
            {
                float value = minValue + (picker.GetValue(type) * (maxValue - minValue));

                label.text = prefix + ConvertToDisplayString(value);
            }
        }

        private string ConvertToDisplayString(float value)
        {
            if(precision > 0)
                return value.ToString("f " + precision);
            else
                return Mathf.FloorToInt(value).ToString();
        }
    }
}