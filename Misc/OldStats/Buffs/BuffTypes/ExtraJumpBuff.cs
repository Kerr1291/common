using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ComponentBuff
{

    [CreateAssetMenu( menuName = "Buffs/ExtraJumpBuff" )]
    public class ExtraJumpBuff : Buff
    {
        public int jumpsToAdd = 1;

        public override void ApplyBuff( PlatformerAvatar avatar )
        {
            avatar.platformerController.numOfAirJumps += jumpsToAdd;
        }

        public override void RemoveBuff( PlatformerAvatar avatar )
        {
            avatar.platformerController.numOfAirJumps -= jumpsToAdd;
        }
    }

}