using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv
{
    public class MapElement : ScriptableObject, IEqualityComparer
    {
        public int type;

        public new bool Equals(object x, object y)
        {
            return (x as MapElement).type == (y as MapElement).type;
        }

        public int GetHashCode(object obj)
        {
            return (obj as MapElement).type.GetHashCode();
        }
    }
}
