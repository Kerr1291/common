using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace nv
{
    public class ShotList : MonoBehaviourFactory
    {
        public override TPoolableMonoBehaviour Get<TPoolableMonoBehaviour>(string key, object setupData, params object[] initParams)
        {
            var shot = base.Get<TPoolableMonoBehaviour>(key, setupData, initParams);
            var shotType = shot as ShotView;
            if(shotType != null)
            {
                shotType.objectPool = this;
            }
            return shot;
        }

        public override PoolableMonoBehaviour Get(string key, object setupData, params object[] initParams)
        {
            var shot = base.Get(key, setupData, initParams);
            var shotType = shot as ShotView;
            if(shotType != null)
            {
                shotType.objectPool = this;
            }
            return shot;
        }

        public override void EnPool(string key, PoolableMonoBehaviour objectToEnPool)
        {
            var shotType = objectToEnPool as ShotView;
            if(shotType != null)
            {
                shotType.objectPool = this;
            }
            base.EnPool(key, objectToEnPool);
        }
    }
}

