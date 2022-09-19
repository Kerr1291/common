using UnityEngine;
using UnityEngine.UI;

namespace mods.Internal.MediaServiceAPI
{
    [RequireComponent(typeof(Image))]
    public class ColorImage : MonoBehaviour
    {
        [HideInInspector]
        public ColorPicker picker;

        private Image image;

        private void OnValidate()
        {
            if(picker == null)
                picker = GetComponentInParent<ColorPicker>();
        }

        private void Awake()
        {
            image = GetComponent<Image>();
            picker.onValueChanged.AddListener(ColorChanged);
        }

        private void OnDestroy()
        {
            picker.onValueChanged.RemoveListener(ColorChanged);
        }

        private void ColorChanged(Color newColor)
        {
            image.color = newColor;
        }
    }
}