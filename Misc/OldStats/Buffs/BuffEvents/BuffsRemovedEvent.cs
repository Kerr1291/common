using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ComponentBuff;

namespace GameEvents
{
    public class BuffsRemovedEvent
    {
        // Remove buff from avatar
        public PlatformerAvatar Avatar;
        public int amountRemoved;
        public RemoveType removeType;
        public List<Buff> buffStack;
    }
}