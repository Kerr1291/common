using UnityEngine;
using System;
using UnityEngine.Events;
using nv;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nv
{
    [CreateAssetMenu(menuName = "ScriptableValue")]
    public class IntValue : ScriptableValue<int> { }
}