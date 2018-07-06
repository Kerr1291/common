using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ComponentBuff;

namespace GameEvents
{
    public enum RemoveType { TRAP, TRANSFER, DEFAULT };

    public class UpdateBuffViewEvent
    {
        // Add buff to avatar
        public PlatformerAvatar Avatar;
        public List<Buff> buffStack;
        public RemoveType removeType;
    }
}