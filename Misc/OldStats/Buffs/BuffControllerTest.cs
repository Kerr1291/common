using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using nv;

using ComponentBuff;
using GameEvents;

public class BuffControllerTest : MonoBehaviour
{
    public PlatformerAvatar fromAvatar;
    public PlatformerAvatar toAvatar;

    public CommunicationNode node = new CommunicationNode();

    public void Start()
    {
        node.Invoke(new BuffInitializerEvent());
    }
}
