using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameEvents;
using Rewired;

using nv;

namespace ComponentBuff
{
    public class BuffController : MonoBehaviour
    {
        public CommunicationNode node = new CommunicationNode();

        [SerializeField]
        public List<Buff> buffStack = new List<Buff>();

        [SerializeField]
        public PlatformerAvatar Avatar;

        [NVCallback]
        void ClearBuffHandler(ClearBuffsEvent buffEvent)
        {
            Dev.Where();
            // Clear buff list
            if (buffEvent.Avatar.GetPlayerID() == Avatar.GetPlayerID())
            {
                buffStack.Clear();
                UpdateBuffStackDisplay();
            }
        }
        
        [NVCallback]
        void AddBuffHandler(AddBuffEvent buffEvent)
        {
            Dev.Where();
            // Add to beginning of list
            if (buffEvent.Avatar.GetPlayerID() == Avatar.GetPlayerID())
            {
                if (buffEvent.buff != null)
                {
                    buffStack.Insert(0, buffEvent.buff);

                    buffStack[0].ApplyBuff(Avatar);

                    UpdateBuffStackDisplay();
                }
            }
        }

        [NVCallback]
        void RemoveBuffHandler(RemoveBuffEvent buffEvent)
        {
            Dev.Where();            
            // Remove first buff in the list
            if (buffEvent.Avatar.GetPlayerID() == Avatar.GetPlayerID())
            {
                int buffsRemoved = 0;
                if (buffStack != null && buffStack.Count>0)
                {
                    
                    for (int index = 0; index < buffEvent.amountToRemove && buffStack.Count > 0; index++)
                    {
                        buffStack[0].RemoveBuff(Avatar);
                        buffStack.RemoveAt(0);
                        buffsRemoved++;
                    }

                    UpdateBuffStackDisplay(buffEvent.removeType);
                }
                BuffsRemovedEvent msg = new BuffsRemovedEvent();
                msg.amountRemoved = buffsRemoved;
                msg.Avatar = buffEvent.Avatar;
                msg.removeType = buffEvent.removeType;
                msg.buffStack = buffStack;
                node.Invoke(msg);
            }
        }

        [NVCallback]
        void TransferBuffHandler(TransferBuffEvent buffEvent)
        {
            Dev.Where();
            // Send buff to the other avater
            if ( buffEvent.fromAvatar.GetPlayerID() == Avatar.GetPlayerID())
            {
                if (buffStack.Count > 0)
                {
                    Buff buff = buffStack[0];
                    buffStack.RemoveAt(0);

                    // Transfer buff
                    AddBuffEvent sendDisplayMsg = new AddBuffEvent
                    {
                        Avatar = buffEvent.toAvatar,
                        buff = buff
                    };
                    node.Invoke(sendDisplayMsg);

                    UpdateBuffStackDisplay();
                }
            }
        }

        void UpdateBuffStackDisplay(RemoveType type = RemoveType.DEFAULT)
        {
            // Update buff display
            UpdateBuffViewEvent sendMsg = new UpdateBuffViewEvent
            {
                Avatar = Avatar,
                buffStack = buffStack,
                removeType = type
            };
            node.Invoke(sendMsg);
        }

        public void OnEnable()
        {
            node.Register(this);
        }

        public void OnDisable()
        {
            node.UnRegister();
        }
    }
}
