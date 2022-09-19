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

        public virtual Func<MapElement, bool> IsObjectMeshElement
        {
            get
            {
                return DefaultEvalObjectMeshElement;
            }
        }

        bool DefaultEvalObjectMeshElement(MapElement mapElement)
        {
            return evalTags.Matches(mapElement.tags) && mapElement.objectMesh != null;
        }

        public virtual Func<MapElement, Mesh> ObjectMeshElement
        {
            get
            {
                return DefaultObjectMeshElement;
            }
        }

        Mesh DefaultObjectMeshElement(MapElement mapElement)
        {
            return mapElement.objectMesh;
        }

        public virtual Func<MapElement, Material> ObjectMaterialElement
        {
            get
            {
                return DefaultObjectMaterialElement;
            }
        }

        Material DefaultObjectMaterialElement(MapElement mapElement)
        {
            return mapElement.elementMat;
        }
    }
}
