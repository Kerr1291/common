using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ComponentBuff
{

    [CreateAssetMenu( menuName = "Buffs/PlayerSizeBuff" )]
    public class PlayerSizeBuff : Buff
    {
        public float sizeToAdd = 1f;

        public override void ApplyBuff( PlatformerAvatar avatar )
        {
            avatar.platformerController.transform.localScale += Vector3.one * sizeToAdd;
        }

        public override void RemoveBuff( PlatformerAvatar avatar )
        {
            avatar.platformerController.transform.localScale -= Vector3.one * sizeToAdd;
        }
    }

}
