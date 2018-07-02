using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
namespace nv.editor
{
    [CustomPropertyDrawer(typeof(Range))]
    public class RangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var obj = fieldInfo.GetValue(property.serializedObject.targetObject);
            Range r = obj as Range;
            if(obj.GetType().IsArray)
            {
                var index = Convert.ToInt32(new string(property.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
                r = ((Range[])obj)[index];
            }

            float valueA = (float)r.GetType().GetField("valueA", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(r);
            float valueB = (float)r.GetType().GetField("valueB", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(r);

            float min = (float)r.GetType().GetProperty("Min", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(r, null);
            float max = (float)r.GetType().GetProperty("Max", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(r, null);

            string minMaxLabelString = string.Format("[{0},{1}]", min, max);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label.text, GUILayout.MaxWidth(label.text.Length * 8));
            EditorGUILayout.LabelField(minMaxLabelString, GUILayout.MaxWidth(20 + minMaxLabelString.Length * 7));
            EditorGUILayout.LabelField("ValueA", GUILayout.MaxWidth(58));
            r.ValueA = EditorGUILayout.DelayedFloatField(valueA, GUILayout.MaxWidth(58));
            EditorGUILayout.LabelField("ValueB", GUILayout.MaxWidth(58));
            r.ValueB = EditorGUILayout.DelayedFloatField(valueB, GUILayout.MaxWidth(58));
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif

namespace nv
{
    [System.Serializable]
    public class Range : IComparable<Range>
    {
        [SerializeField]
        float valueA;

        [SerializeField]
        float valueB;
        
        public float ValueA
        {
            get
            {
                return valueA;
            }
            set
            {
                valueA = value;
            }
        }

        public float ValueB
        {
            get
            {
                return valueB;
            }
            set
            {
                valueB = value;
            }
        }

        /// <summary>
        /// Will always get/set the smaller of the two values.
        /// </summary>
        public float Min
        {
            get
            {
                return Mathf.Min(ValueA, ValueB);
            }
        }

        /// <summary>
        /// Will always get/set the larger of the two values.
        /// </summary>
        public float Max
        {
            get
            {
                return Mathf.Max(ValueA, ValueB);
            }
        }

        public float Size
        {
            get
            {
                return (Max - Min);
            }
        }

        public Range()
        {
            ValueA = 0f;
            ValueB = 1f;
        }

        public Range(float min, float max)
        {
            ValueA = min;
            ValueB = max;
        }


        public Range(float size)
        {
            ValueA = 0f;
            ValueB = size;
        }

        public Range(AnimationCurve data, bool getDataFromXAxis = false)
        {
            ValueA = 0f;
            ValueB = 1f;

            if(data.length < 1)
                return;

            if(getDataFromXAxis)
            {
                ValueA = data.keys.Min(x => x.time);
                ValueB = data.keys.Max(x => x.time);
            }
            else
            {
                ValueA = data.keys.Min(x => x.value);
                ValueB = data.keys.Max(x => x.value);
            }
        }

        public float this[int i]
        {
            get
            {
                i = Mathf.Clamp(i, 0, 1);
                if(i == 0)
                    return ValueA;
                return ValueB;
            }
            set
            {
                i = Mathf.Clamp(i, 0, 1);
                if(i == 0)
                    ValueA = value;
                ValueB = value;
            }
        }

        public static implicit operator List<float>(Range r)
        {
            return r.ToList();
        }

        public static implicit operator float[](Range r)
        {
            return r.ToArray();
        }

        public static implicit operator KeyValuePair<float, float>(Range r)
        {
            return new KeyValuePair<float, float>(r.Min, r.Max);
        }

        public static implicit operator Vector2(Range r)
        {
            return new Vector2(r.Min, r.Max);
        }

        public static implicit operator Range(List<float> collection)
        {
            return new Range(collection.Min(), collection.Max());
        }

        public static implicit operator Range(KeyValuePair<float, float> pair)
        {
            return new Range(Mathf.Min(pair.Key, pair.Value), Mathf.Max(pair.Key, pair.Value));
        }

        public static implicit operator Range(Vector2 v)
        {
            return new Range(v.x, v.y);
        }

        public static Range operator *(Range r, float s)
        {
            return new Range(r.Min * s, r.Max * s);
        }

        public static Range operator +(Range r, float s)
        {
            return new Range(r.Min + s, r.Max + s);
        }

        public static Range operator -(Range r, float s)
        {
            return new Range(r.Min - s, r.Max - s);
        }

        public static Range operator /(Range r, float s)
        {
            return new Range(r.Min / s, r.Max / s);
        }

        /// <summary>
        /// Returns the value at normalizedTime where normalizedTime is a [0,1] float. Input outside this range will be clamped.
        /// </summary>
        /// <param name="normalizedTime">A [0,1] float. Input outside this range will be clamped.</param>
        /// <returns>The corrosponding value</returns>
        public float Evaluate(float normalizedTime)
        {
            normalizedTime = Mathf.Clamp01(normalizedTime);
            return Min + Size * normalizedTime;
        }

        /// <summary>
        /// Determins if the value is inside this range.
        /// </summary>
        public bool Contains(float x)
        {
            if(x < Min)
                return false;
            if(x > Max)
                return false;
            return true;
        }

        /// <summary>
        /// Determins if the given range is inside this range.
        /// </summary>
        public bool Contains(Range r)
        {
            if(!Contains(r.Min))
                return false;
            if(!Contains(r.Max))
                return false;
            return true;
        }

        /// <summary>
        /// Returns the value normalized to the range. Input outside this range will be clamped.
        /// </summary>
        /// <param name="x">A float in the range. Input outside this range will be clamped.</param>
        /// <returns>The corrosponding value</returns>
        public float NormalizedValue(float x)
        {
            x = Mathf.Clamp(x, Min, Max);
            return (x - Min) / Size;
        }

        public float RandomValue()
        {
            return GameRNG.Rand(Min, Max);
        }

        public float RandomValue(RNG rng)
        {
            return rng.Rand(Min, Max);
        }

        public float RandomNormalizedValue()
        {
            return NormalizedValue(GameRNG.Rand(Min, Max));
        }

        public float RandomNormalizedValue(RNG rng)
        {
            return NormalizedValue(rng.Rand(Min, Max));
        }

#if UNITY_2017_1_OR_NEWER && NET46
        public override string ToString()
        {
            return $"[+{Min.ToString()}+,+{Max.ToString()}+]";
        }
#else
        public override string ToString()
        {
            return "["+Min.ToString()+","+Max.ToString()+"]";
        }

        /// <summary>
        /// Convert the range into a set of discrete steps. Example, [0,1] with 10 steps will give a list of [.1,.2,.3, .., 1]
        /// </summary>
        /// <param name="steps"></param>
        /// <returns></returns>
        public List<float> ToSteps(int steps)
        {
            float stepSize = Size / steps;
            List<float> sets = new List<float>();
            for(int i = 1; i <= steps; ++i)
            {
                sets.Add(Min + stepSize * i);
            }
            return sets;
        }

        public List<float> ToList()
        {
            return new List<float>() { Min, Max };
        }

        public float[] ToArray()
        {
            return new float[] { Min, Max };
        }

        /// <summary>
        /// Ranges are sorted by min value, then max value
        /// </summary>
        public int CompareTo(Range other)
        {
            if(Min < other.Min)
                return -1;
            else if(Min > other.Min)
                return 1;
            else
            {
                if(Max < other.Max)
                    return -1;
                else if(Max > other.Max)
                    return 1;
            }
            return 0;
        }
#endif
    }
}