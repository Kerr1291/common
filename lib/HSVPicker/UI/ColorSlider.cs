using System;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace mods.Internal.MediaServiceAPI
{
    /// <summary>
    /// Displays one of the color values of aColorPicker
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class ColorSlider : MonoBehaviour
    {
        [HideInInspector]
        public ColorPicker hsvpicker;

        /// <summary>
        /// Which value this slider can edit.
        /// </summary>
        [HideInInspector]
        public ColorValues type;

        private Slider slider;

        private bool listen = true;

        private void OnValidate()
        {
            if(hsvpicker == null)
                hsvpicker = GetComponentInParent<ColorPicker>();

            if(name == "Slider")
            {
                GameObject sliderParent = transform.parent.gameObject;
                char typeName = sliderParent.name[0];
                try
                {
                    var types = Enum.GetNames(typeof(ColorValues));
                    var selected = types.FirstOrDefault(x => x[0] == typeName);

                    type = (ColorValues)System.Enum.Parse(typeof(ColorValues), selected);
                }
                catch(Exception e)
                {
                    Debug.LogError("Failed to parse color slider type from game object name. Please make sure the parent of the color slider referenced on this object has a name that starts with R/G/B or H/S/V");
                }
            }
        }

        private void Awake()
        {
            slider = GetComponent<Slider>();

            hsvpicker.onValueChanged.AddListener(ColorChanged);
            hsvpicker.onHSVChanged.AddListener(HSVChanged);
            slider.onValueChanged.AddListener(SliderChanged);
        }

        private void OnDestroy()
        {
            hsvpicker.onValueChanged.RemoveListener(ColorChanged);
            hsvpicker.onHSVChanged.RemoveListener(HSVChanged);
            slider.onValueChanged.RemoveListener(SliderChanged);
        }

        private void ColorChanged(Color newColor)
        {
            listen = false;
            switch(type)
            {
                case ColorValues.R:
                    slider.normalizedValue = newColor.r;
                    break;
                case ColorValues.G:
                    slider.normalizedValue = newColor.g;
                    break;
                case ColorValues.B:
                    slider.normalizedValue = newColor.b;
                    break;
                case ColorValues.A:
                    slider.normalizedValue = newColor.a;
                    break;
                default:
                    break;
            }
        }

        private void HSVChanged(float hue, float saturation, float value)
        {
            listen = false;
            switch(type)
            {
                case ColorValues.Hue:
                    slider.normalizedValue = hue; //1 - hue;
                    break;
                case ColorValues.Saturation:
                    slider.normalizedValue = saturation;
                    break;
                case ColorValues.Value:
                    slider.normalizedValue = value;
                    break;
                default:
                    break;
            }
        }

        private void SliderChanged(float newValue)
        {
            if(listen)
            {
                newValue = slider.normalizedValue;
                //if (type == ColorValues.Hue)
                //    newValue = 1 - newValue;

                hsvpicker.AssignColor(type, newValue);
            }
            listen = true;
        }
    }
}