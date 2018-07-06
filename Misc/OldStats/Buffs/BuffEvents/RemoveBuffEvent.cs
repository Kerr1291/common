using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ComponentBuff;

namespace GameEvents
{
    public class RemoveBuffEvent
    {
        // Remove buff from avatar
        public PlatformerAvatar Avatar;
        public int amountToRemove;
        public RemoveType removeType;
    }
}
