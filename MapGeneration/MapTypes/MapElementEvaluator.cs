using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv
{
    public abstract class MapElementEvaluator : ScriptableObject
    {
        public abstract Func<MapElement, bool> IsMeshElement { get; }
    }
}
