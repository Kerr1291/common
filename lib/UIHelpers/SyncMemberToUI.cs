using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using System.Reflection;
using System;

namespace nv
{
    /// <summary>
    /// This class will attempt to reflect the value in the member you bind to it into the into the bound uiElement.
    /// This class is setup to be easily extendable to enable support for new UI types.
    /// </summary>
    [ExecuteInEditMode]
    public class SyncMemberToUI : MonoBehaviour
    {
        /// <summary>
        /// The owner of the member reference.
        /// </summary>
        [SerializeField]
        protected UnityEngine.Object target;

        /// <summary>
        /// The owner of the target (The game object in the case where the target is a component). May be the same reference as target.
        /// </summary>
        [SerializeField]
        protected UnityEngine.Object rootTarget;

        /// <summary>
        /// The member that will be reflected into the UI
        /// </summary>
        [SerializeField]
        protected SerializableMemberInfo targetRef;

        /// <summary>
        /// The member that will be updated if the target is changed or interacted with. Used for things like input fields, toggles, etc.
        /// May be the same as targetRef.
        /// </summary>
        [SerializeField]
        protected SerializableMemberInfo targetSetRef;

        /// <summary>
        /// Used by the Dropdown list UI to populate the choices from a member.
        /// </summary>
        [SerializeField]
        protected bool useListSourceRef;

        /// <summary>
        /// Used by the Dropdown list UI to populate the choices from a count.
        /// </summary>
        [SerializeField]
        protected bool useListSourceCount;

        /// <summary>
        /// Used by the Dropdown list UI to populate the choices from this member reference. Requires useListSourceRef = true
        /// </summary>
        [SerializeField]
        protected SerializableMemberInfo listSourceRef;

        /// <summary>
        /// The UI element that will display the value of targetRef and provide interaction for targetSetRef.
        /// </summary>
        [SerializeField]
        protected UnityEngine.Object uiElement;

        protected virtual void Reset()
        {
            Setup();
            RegisterUICallbacks();
        }

        protected virtual void Awake()
        {
            Setup();
            RegisterUICallbacks();
        }

        protected virtual void Setup()
        {
            uiElement = GetComponents<UIBehaviour>().Where(x =>
            (x as Text != null) ||
            (x as InputField != null) ||
            (x as Slider != null) ||
            (x as Toggle != null) ||
            (x as Scrollbar != null) ||
            (x as Dropdown != null)).FirstOrDefault();
        }

        protected virtual void SetValueFromInputField(string value)
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

        protected virtual void SetValueFromSliderOrScrollbar(float value)
        {
            if(targetRef.GetValue<float>(target) == value)
                return;

            targetSetRef.SetValue(target, value);
        }

        protected virtual void SetValueFromToggle(bool value)
        {
            if(targetRef.GetValue<bool>(target) == value)
                return;

            targetSetRef.SetValue(target, value);
        }

        protected virtual void SetValueFromDropdown(int value)
        {
            if(targetRef.GetValue<int>(target) == value)
                return;

            targetSetRef.SetValue(target, value);
        }

        protected virtual void RegisterUICallbacks()
        {
            RegisterInputFieldCallbacks();
            RegisterToggleCallbacks();
            RegisterSliderCallbacks();
            RegisterScrollbarCallbacks();
            RegisterDropdownCallbacks();
        }

        protected virtual void RegisterDropdownCallbacks()
        {
            var element = uiElement as Dropdown;
            if(element != null)
            {
                element.onValueChanged.RemoveListener(SetValueFromDropdown);
                element.onValueChanged.AddListener(SetValueFromDropdown);
            }
        }

        protected virtual void RegisterScrollbarCallbacks()
        {
            var element = uiElement as Scrollbar;
            if(element != null)
            {
                element.onValueChanged.RemoveListener(SetValueFromSliderOrScrollbar);
                element.onValueChanged.AddListener(SetValueFromSliderOrScrollbar);
            }
        }

        protected virtual void RegisterSliderCallbacks()
        {
            var element = uiElement as Slider;
            if(element != null)
            {
                element.onValueChanged.RemoveListener(SetValueFromSliderOrScrollbar);
                element.onValueChanged.AddListener(SetValueFromSliderOrScrollbar);
            }
        }

        protected virtual void RegisterToggleCallbacks()
        {
            var element = uiElement as Toggle;
            if(element != null)
            {
                element.onValueChanged.RemoveListener(SetValueFromToggle);
                element.onValueChanged.AddListener(SetValueFromToggle);
            }
        }

        protected virtual void RegisterInputFieldCallbacks()
        {
            var element = uiElement as InputField;
            if(element != null)
            {
                element.onEndEdit.RemoveListener(SetValueFromInputField);
                element.onEndEdit.AddListener(SetValueFromInputField);
            }
        }

        protected virtual void LateUpdate()
        {
            UpdateUIValue();
        }

        protected virtual void UpdateUIValue()
        {
            if(target == null || rootTarget == null || targetRef.Info == null)
                return;

            object targetValue = targetRef.GetValue(target);

            TryUpdateTextUI(targetValue);
            TryUpdateInputFieldUI(targetValue);
            TryUpdateToggleUI(targetValue);
            TryUpdateSliderUI(targetValue);
            TryUpdateScrollbarUI(targetValue);
            TryUpdateDropdownUI(targetValue);
        }

        protected virtual void TryUpdateScrollbarUI(object targetValue)
        {
            if(targetValue == null)
                return;

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

        protected virtual void TryUpdateSliderUI(object targetValue)
        {
            if(targetValue == null)
                return;

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

        protected virtual void TryUpdateToggleUI(object targetValue)
        {
            if(targetValue == null)
                return;

            var element = uiElement as Toggle;
            if(element != null)
            {
                bool success = false;
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

        protected virtual void TryUpdateInputFieldUI(object targetValue)
        {
            if(targetValue == null)
                return;

            if(uiElement as InputField != null)
            {
                var stringValue = targetValue.ToString();
                var element = uiElement as InputField;
                if(element != null && !element.isFocused && stringValue != element.text)
                {
                    element.text = stringValue;
                }
            }
        }

        protected virtual void TryUpdateTextUI(object targetValue)
        {
            if(targetValue == null)
                return;

            if(uiElement as Text != null)
            {
                var stringValue = targetValue.ToString();
                var element = uiElement as Text;
                if(element != null && stringValue != element.text)
                    element.text = stringValue;
            }
        }

        protected virtual void TryUpdateDropdownUI(object targetValue)
        {
            if(targetValue == null)
                return;

            var element = uiElement as Dropdown;
            if(element != null)
            {
                if(listSourceRef.Info != null && listSourceRef.Info.ReflectedType != target.GetType())
                    return;

                if(useListSourceRef)
                    useListSourceCount = false;

                try
                {
                    if(useListSourceCount && listSourceRef.Info != null)
                    {
                        int count = (int)listSourceRef.GetValue(target);
                        if(element.options.Count != count)
                        {
                            element.options = new List<Dropdown.OptionData>();

                            for(int i = 0; i < count; ++i)
                            {
                                element.options.Add(new Dropdown.OptionData(i.ToString()));
                            }
                        }
                    }

                    if(useListSourceRef && listSourceRef.Info != null)
                    {
                        if(listSourceRef.Info.GetTypeFromMember().IsArray)
                        {
                            Array data = listSourceRef.GetValue(target) as Array;

                            if(element.options.Count != data.Length)
                            {
                                element.options = new List<Dropdown.OptionData>();

                                foreach(var value in data)
                                {
                                    element.options.Add(new Dropdown.OptionData(value == null ? "null" : value.ToString()));
                                }
                            }
                            else
                            {
                                int i = 0;
                                foreach(var value in data)
                                {
                                    element.options[i].text = (value == null ? "null" : value.ToString());
                                    ++i;
                                }
                            }
                        }
                        else if(listSourceRef.Info.GetTypeFromMember().GetInterfaces().Contains(typeof(IList)))
                        {
                            var listData = listSourceRef.GetValue(target);
                            var items = listData.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x as FieldInfo != null && x.Name.Contains("_items")).FirstOrDefault() as FieldInfo;
                            var arrayData = items.GetValue(listData);

                            //get the list's internal data array
                            Array data = items.GetValue(listData) as Array;

                            if(element.options.Count != data.Length)
                            {
                                element.options = new List<Dropdown.OptionData>();

                                foreach(var value in data)
                                {
                                    element.options.Add(new Dropdown.OptionData(value == null ? "null" : value.ToString()));
                                }
                            }
                            else
                            {
                                int i = 0;
                                foreach(var value in data)
                                {
                                    element.options[i].text = (value == null ? "null" : value.ToString());
                                    ++i;
                                }
                            }
                        }
                    }

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