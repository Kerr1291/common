using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv
{
    public abstract class MapElementEvaluator : ScriptableObject
    {
        public Tags evalTags;

        public virtual Func<MapElement, bool> IsMeshElement
        {
            get
            {
                return DefaultEvalMeshElement;
            }
        }

        bool DefaultEvalMeshElement(MapElement mapElement)
        {
            return evalTags.Matches(mapElement.tags);
        }
    }
}
