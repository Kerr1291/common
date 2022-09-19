using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace mods.Internal.MediaServiceAPI
{
    public class ColorPresets : MonoBehaviour
    {
        [HideInInspector]
        public ColorPicker picker;

        [HideInInspector]
        public GameObject[] presets;

        [HideInInspector]
        public UnityEngine.UI.Image createPresetImage;

        private ColorPresetList _colors;

        public void ClearNullEvents(UnityEventBase e)
        {
#if UNITY_EDITOR
            for(int i = 0; i < e.GetPersistentEventCount(); ++i)
            {
                if(e.GetPersistentTarget(i) == null)
                {
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(e, i);
                }
            }
#endif
        }

        private void OnValidate()
        {
            if(picker == null)
                picker = GetComponentInParent<ColorPicker>();

            if(presets == null || presets.Length <= 0)
                presets = GetComponentsInChildren<Transform>(true).Where(t => t.name.Contains("Preset (")).Select(x => x.gameObject).ToArray();

            presets.Select(x => x.GetComponent<UnityEngine.UI.Button>()).ToList().ForEach(p =>
            {
#if UNITY_EDITOR
                ClearNullEvents(p.onClick);
                UnityEditor.Events.UnityEventTools.RemovePersistentListener<UnityEngine.UI.Image>(p.onClick, PresetSelect);
                UnityEditor.Events.UnityEventTools.AddObjectPersistentListener<UnityEngine.UI.Image>(p.onClick, PresetSelect, p.GetComponent<UnityEngine.UI.Image>());
#endif
            });

            if(createPresetImage == null)
                createPresetImage = GetComponentsInChildren<Transform>(true).Where(t => t.name.Contains("Create Button")).FirstOrDefault().GetComponent<UnityEngine.UI.Image>();

#if UNITY_EDITOR

            var createButton = createPresetImage.GetComponent<UnityEngine.UI.Button>();
            ClearNullEvents(createButton.onClick);

            UnityEditor.Events.UnityEventTools.RemovePersistentListener(createButton.onClick, CreatePresetButton);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(createButton.onClick, CreatePresetButton);
#endif
        }

        void Awake()
        {
            //		picker.onHSVChanged.AddListener(HSVChanged);
            picker.onValueChanged.AddListener(ColorChanged);
        }

        void Start()
        {
            _colors = ColorPresetManager.Get(picker.Setup.PresetColorsId);

            if(_colors.Colors.Count < picker.Setup.DefaultPresetColors.Length)
            {
                _colors.UpdateList(picker.Setup.DefaultPresetColors);
            }

            _colors.OnColorsUpdated += OnColorsUpdate;
            OnColorsUpdate(_colors.Colors);
        }

        private void OnColorsUpdate(List<Color> colors)
        {
            for(int cnt = 0; cnt < presets.Length; cnt++)
            {
                if(colors.Count <= cnt)
                {
                    presets[cnt].SetActive(false);
                    continue;
                }


                presets[cnt].SetActive(true);
                presets[cnt].GetComponent<UnityEngine.UI.Image>().color = colors[cnt];

            }

            createPresetImage.gameObject.SetActive(colors.Count < presets.Length);

        }

        public void CreatePresetButton()
        {
            _colors.AddColor(picker.CurrentColor);

            //      for (var i = 0; i < presets.Length; i++)
            //{
            //	if (!presets[i].activeSelf)
            //	{
            //		presets[i].SetActive(true);
            //		presets[i].GetComponent<Image>().color = picker.CurrentColor;
            //		break;
            //	}
            //}
        }

        public void PresetSelect(UnityEngine.UI.Image sender)
        {
            picker.CurrentColor = sender.color;
        }

        // Not working, it seems ConvertHsvToRgb() is broken. It doesn't work when fed
        // input h, s, v as shown below.
        //	private void HSVChanged(float h, float s, float v)
        //	{
        //		createPresetImage.color = HSVUtil.ConvertHsvToRgb(h, s, v, 1);
        //	}
        private void ColorChanged(Color color)
        {
            createPresetImage.color = color;
        }
    }
}