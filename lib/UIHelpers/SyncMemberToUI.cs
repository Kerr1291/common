﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;
using System.Reflection;
using System;

namespace nv
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(UIBehaviour))]
    public class SyncMemberToUI : MonoBehaviour
    {
        [SerializeField]
        UnityEngine.Object target;

        [SerializeField]
        UnityEngine.Object rootTarget;

        [SerializeField]
        SerializableMemberInfo targetRef;

        [SerializeField]
        SerializableMemberInfo targetSetRef;

        [SerializeField]
        bool useListSourceRef;

        [SerializeField]
        bool useListSourceCount;

        [SerializeField]
        SerializableMemberInfo listSourceRef;

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

        public Type GetTypeFromMember(MemberInfo m)
        {
            var mi = m as MethodInfo;
            if(mi != null)
                return mi.ReturnType;
            var pi = m as PropertyInfo;
            if(pi != null)
                return pi.PropertyType;
            var fi = m as FieldInfo;
            if(fi != null)
                return fi.FieldType;
            return null;
        }

        public List<MemberInfo> FilterMembersByType<T>(List<MemberInfo> allMembers)
        {
            var fields = allMembers.OfType<FieldInfo>().Cast<FieldInfo>().Where(x => x.FieldType == typeof(T)).Cast<MemberInfo>();
            //Dev.LogVarArray("fields", fields.ToList());
            var props = allMembers.OfType<PropertyInfo>().Cast<PropertyInfo>().Where(x => x.PropertyType == typeof(T)).Cast<MemberInfo>();
            //Dev.LogVarArray("props", fields.ToList());
            var methods = allMembers.OfType<MethodInfo>().Cast<MethodInfo>().Where(x => (x.ReturnType == typeof(T)) || (x.GetParameters().Length > 0 && x.GetParameters()[0].ParameterType == typeof(T))).Cast<MemberInfo>();
            //Dev.LogVarArray("methods", fields.ToList());
            return fields.Concat(props).Concat(methods).ToList();
        }

        public List<MemberInfo> FilterAllowedMembersByUIType(List<MemberInfo> allMembers)
        {
            if(uiElement as Text != null)
                return allMembers;
            if(uiElement as InputField != null)
                return allMembers;
            if(uiElement as Slider != null)
                return FilterMembersByType<float>(allMembers).Concat(FilterMembersByType<int>(allMembers)).ToList();
            if(uiElement as Toggle != null)
                return FilterMembersByType<bool>(allMembers).ToList();
            if(uiElement as Scrollbar != null)
                return FilterMembersByType<float>(allMembers).ToList();
            if(uiElement as Dropdown != null)
                return FilterMembersByType<int>(allMembers).ToList();
            return allMembers;
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
            //Dev.LogVar(value);
            if(targetRef.GetValue<bool>(target) == value)
                return;

            targetSetRef.SetValue(target, value);
        }

        void SetValueFromSliderOrScrollbar(float value)
        {
            //Dev.LogVar(value);
            if(targetRef.GetValue<float>(target) == value)
                return;

            targetSetRef.SetValue(target, value);
        }

        void SetValueFromDropdown(int value)
        {
            //Dev.LogVar(value);
            if(targetRef.GetValue<int>(target) == value)
                return;

            targetSetRef.SetValue(target, value);
        }

        void RegisterUICallbacks()
        {
            //Dev.LogVar(uiElement.GetType().Name);
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
            if(target == null || targetRef.Info == null)
                return;

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
                            element.text = stringValue;
                        }
                    }
                }
                {//Toggle
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
                                if(GetTypeFromMember(listSourceRef.Info).IsArray)
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
                                else if(GetTypeFromMember(listSourceRef.Info).GetInterfaces().Contains(typeof(IList)))
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
    }
}