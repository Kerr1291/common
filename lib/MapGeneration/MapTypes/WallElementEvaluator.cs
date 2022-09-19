using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace nv
{
    public class WallElementEvaluator : MapElementEvaluator
    {
        public override Func<MapElement, bool> IsMeshElement
        {
            get
            {
                return EvalMeshElement;
            }
        }

        bool EvalMeshElement(MapElement mapElement)
        {
            return evalTags.Matches(mapElement.tags);
        }
    }
}
