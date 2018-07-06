using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ComponentBuff
{
    public abstract class Buff : ScriptableObject
    {
        public virtual string GetBuffTypeName()
        {
            return "None";
        }

        public abstract void ApplyBuff( PlatformerAvatar avatar );

        public abstract void RemoveBuff( PlatformerAvatar avatar );
    }
}
