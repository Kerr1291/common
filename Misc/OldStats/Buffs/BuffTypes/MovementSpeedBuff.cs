using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ComponentBuff
{

    [CreateAssetMenu( menuName = "Buffs/MovementSpeedBuff" )]
    public class MovementSpeedBuff : Buff
    {
        public float speedToAdd = -0.2f;

        public override void ApplyBuff( PlatformerAvatar avatar )
        {
            avatar.platformerController.groundSpeed += speedToAdd;
        }

        public override void RemoveBuff( PlatformerAvatar avatar )
        {
            avatar.platformerController.groundSpeed -= speedToAdd;
        }
    }

}
