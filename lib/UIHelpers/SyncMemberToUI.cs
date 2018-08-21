using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

namespace nv
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(UIBehaviour))]
    public class SyncMemberToUI : MonoBehaviour
    {
        [SerializeField]
        Object target;

        [SerializeField]
        Object rootTarget;

        [SerializeField]
        SerializableMemberInfo targetRef;

        [SerializeField]
        SerializableMemberInfo targetSetRef;

        [SerializeField]
        UIBehaviour uiElement;

        void Reset()
        {
            GetUnityUIComponent();
            RegisterUICallbacks();
        }

        void Awake()
        {
            GetUnityUIComponent();
            RegisterUICallbacks();
        }

        void GetUnityUIComponent()
        {
            uiElement = GetComponents<UIBehaviour>().Where(x => 
            (x as Text != null) || 
            (x as InputField != null) || 
            (x as Slider != null) || 
            (x as Toggle != null) || 
            (x as Scrollbar != null) || 
            (x as Dropdown != null)).FirstOrDefault();
        }

        void LateUpdate()
        {
            if(target != null && rootTarget != null)
                UpdateUIValue();
        }

        void SetValueFromInputText(string value)
        {
            object currentValue = targetRef.GetValue(target);
            if(currentValue.ToString() == value)
                return;

            var element = uiElement as InputField;

            int? ivalue = null;
            float? fvalue = null;

            if(element.contentType == InputField.ContentType.DecimalNumber)
                fvalue = System.Convert.ToSingle(value);
            else if(element.contentType == InputField.ContentType.IntegerNumber)
                ivalue = System.Convert.ToInt32(value);
            else if(element.contentType == InputField.ContentType.Pin)
                ivalue = System.Convert.ToInt32(value);
            else
            {
                try
                {
                    ivalue = System.Convert.ToInt32(value);
                }
                catch(System.Exception)
                { }

                try
                {
                    fvalue = System.Convert.ToSingle(value);
                }
                catch(System.Exception)
                { }
            }

            if(ivalue != null && ivalue.HasValue)
                targetSetRef.SetValue(target, ivalue.Value);
            else if(fvalue != null && fvalue.HasValue)
                targetSetRef.SetValue(target, fvalue.Value);
            else
                targetSetRef.SetValue(target, value);
        }

        void SetValueFromToggle(bool value)
        {
            Dev.LogVar(value);
            if(targetRef.GetValue<bool>(target) == value)
                return;

            targetSetRef.SetValue(target, value);
        }

        void SetValueFromSliderOrScrollbar(float value)
        {
            Dev.LogVar(value);
            if(targetRef.GetValue<float>(target) == value)
                return;

            targetSetRef.SetValue(target, value);
        }

        void SetValueFromDropdown(int value)
        {
            Dev.LogVar(value);
            if(targetRef.GetValue<int>(target) == value)
                return;

            targetSetRef.SetValue(target, value);
        }

        void RegisterUICallbacks()
        {
            Dev.LogVar(uiElement.GetType().Name);
            {//Input field
                var element = uiElement as InputField;
                if(element != null)
                {
                    element.onEndEdit.RemoveListener(SetValueFromInputText);
                    element.onEndEdit.AddListener(SetValueFromInputText);
                }
            }
            {//Toggle
                var element = uiElement as Toggle;
                if(element != null)
                {
                    element.onValueChanged.RemoveListener(SetValueFromToggle);
                    element.onValueChanged.AddListener(SetValueFromToggle);
                }
            }
            {//Slider
                var element = uiElement as Slider;
                if(element != null)
                {
                    element.onValueChanged.RemoveListener(SetValueFromSliderOrScrollbar);
                    element.onValueChanged.AddListener(SetValueFromSliderOrScrollbar);
                }
            }
            {//Scrollbar
                var element = uiElement as Scrollbar;
                if(element != null)
                {
                    element.onValueChanged.RemoveListener(SetValueFromSliderOrScrollbar);
                    element.onValueChanged.AddListener(SetValueFromSliderOrScrollbar);
                }
            }
            {//Dropdown
                var element = uiElement as Dropdown;
                if(element != null)
                {
                    element.onValueChanged.RemoveListener(SetValueFromDropdown);
                    element.onValueChanged.AddListener(SetValueFromDropdown);
                }
            }
        }

        void UpdateUIValue()
        {
            object targetValue = targetRef.GetValue(target);
            if(targetValue != null)
            {
                if(uiElement as Text || uiElement as InputField)
                {
                    var stringValue = targetValue.ToString();
                    {//Text
                        var element = uiElement as Text;
                        if(element != null && stringValue != element.text)
                            element.text = stringValue;
                    }
                    {//Input field
                        var element = uiElement as InputField;
                        if(element != null && !element.isFocused && stringValue != element.text)
                        {
                            Dev.LogVar(stringValue);
                            element.text = stringValue;
                        }
                    }
                }
                {//Toggle
                    var element = uiElement as Toggle;
                    if(element != null)
                    {
                        bool success = false;
                        //try casting it to each primitive type that could go in a slider
                        if(!success)
                        {
                            try
                            {
                                element.isOn = (bool)targetValue;
                                success = true;
                            }
                            catch(System.InvalidCastException)
                            {
                                Debug.Log("Toggle needs a bool type");
                                element.isOn = false;
                            }
                        }
                    }
                }
                {//Slider
                    var element = uiElement as Slider;
                    if(element != null)
                    {
                        bool success = false;
                        //try casting it to each primitive type that could go in a slider
                        if(!success)
                        {
                            try
                            {
                                element.value = (float)targetValue;
                                success = true;
                            }
                            catch(System.InvalidCastException)
                            {
                                element.value = element.minValue;
                            }
                        }
                        else if(!success)
                        {
                            try
                            {
                                element.value = (int)targetValue;
                                success = true;
                            }
                            catch(System.InvalidCastException)
                            {
                                Debug.Log("Slider needs a float or an int type");
                                element.value = element.minValue;
                            }
                        }
                    }
                }
                {//Scrollbar
                    var element = uiElement as Scrollbar;
                    if(element != null)
                    {
                        try
                        {
                            element.value = (float)targetValue;
                        }
                        catch(System.InvalidCastException)
                        {
                            Debug.Log("Scrollbar needs a float type");
                            element.value = 0f;
                        }
                    }
                }
                {//Dropdown
                    var element = uiElement as Dropdown;
                    if(element != null)
                    {
                        try
                        {
                            element.value = (int)targetValue;
                        }
                        catch(System.InvalidCastException)
                        {
                            Debug.Log("Dropdown needs an int type to select the dropdown index");
                            element.value = 0;
                        }
                    }
                }
            }
        }
    }
}