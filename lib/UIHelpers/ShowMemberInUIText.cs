using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

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
        if(target != null)
        {
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

[System.Serializable]
public class SerializableMemberInfo
{
    public MemberInfo Info
    {
        get 
        {
            //if the data is lost, rebuild it
            if(info == null)
            {
                if(string.IsNullOrEmpty(memberName))
                    return null;

                if(System.Type.GetType(typeName) == null)
                    return null;

                if(isProperty)
                {
                    info = System.Type.GetType(typeName).GetProperty(memberName, bFlags);
                }
                else
                {
                    info = System.Type.GetType(typeName).GetField(memberName, bFlags);
                }
            }

            return info;
        }
        set
        {
            info = value;
            if(info != null)
            {
                isProperty = (info as PropertyInfo != null);
                typeName = info.DeclaringType.FullName;
                memberName = info.Name;
                bFlags = info.DeclaringType.GetType().IsNotPublic ? BindingFlags.NonPublic : BindingFlags.Public;
            }
        }
    }

    public object GetValue(object instance)
    {
        var fi = Info as FieldInfo; 
        if(fi != null)
        {
            object targetValue = fi.GetValue(instance);
            return targetValue;
        }
        var pi = Info as PropertyInfo;
        if(pi != null)
        {
            object targetValue = pi.GetValue(instance);
            return targetValue;
        }
        return null;
    }

    public void SetValue(object instance, object value)
    {
        var fi = Info as FieldInfo;
        if(fi != null)
        {
            fi.SetValue(instance, value);
        }
        var pi = Info as PropertyInfo;
        if(pi != null)
        {
            pi.SetValue(instance, value);
        }
    }

    MemberInfo info;

    [SerializeField]
    bool isProperty;

    [SerializeField]
    string typeName;

    [SerializeField]
    string memberName;

    [SerializeField]
    BindingFlags bFlags;
}