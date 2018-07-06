using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

namespace nv
{
    [System.Serializable]
    public class ShotData : ListData
    {
        public string shotData;

        public Avatar owner;
        public Vector3 spawnPoint;
        public Vector3 shotDirection;

        public List<GameObject> ignoreList;

        //if this bullet is alive or not
        public bool IsAlive
        {
            get; set;
        }

        protected virtual bool Equals(ShotData other)
        {
            return string.Equals(shotData, other.shotData);
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
                return false;
            return Equals((ShotData)obj);
        }

        public override int GetHashCode()
        {
            return shotData.GetHashCode();
        }
    }
}