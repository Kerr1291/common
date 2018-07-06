using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using nv;
using Rewired;
using GameEvents;

namespace ComponentBuff
{
    public class BuffStackInitializer : MonoBehaviour
    {
        public CommunicationNode node = new CommunicationNode();

        public int startingBuffCount = 10;

        public List<Buff> buffTemplates;
        
        void GenerateRandomStacksForAllAvatars()
        {
            Dev.Where();
            // Create 10 buffs
            List<Buff> buffs = new List<Buff>();

            for(int i = 0; i < startingBuffCount; i++)
            {
                Buff cloneType = Dev.GetRandomElementFromList(buffTemplates);
                Buff newBuff = Object.Instantiate(cloneType) as Buff;
                buffs.Add(newBuff);
            }
    
            List<PlayerInputInterface> players = GameInput.GetAllPlayers();
            foreach (PlayerInputInterface player in players)
            {
                // generate stack for player
                foreach(Buff playerBuff in buffs)
                {
                    AddBuffEvent sendMsg = new AddBuffEvent
                    {
                        Avatar = player.GetAvatar() as PlatformerAvatar,
                        buff = playerBuff
                    };
                    node.Invoke(sendMsg);
                }

                // Update buff display
                UpdateBuffViewEvent sendDisplayMsg = new UpdateBuffViewEvent
                {
                    Avatar = player.GetAvatar() as PlatformerAvatar,
                    buffStack = buffs
                };
                node.Invoke(sendDisplayMsg);
            }
        }

        [NVCallback]
        void GameStartedHandler(GameStarted buffEvent)
        {
            Dev.Where();
            GenerateRandomStacksForAllAvatars();
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
