using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using nv;
using Rewired;
using GameEvents;

using UnityEngine.UI;

public class BuffStackView : MonoBehaviour
{
    public CommunicationNode node = new CommunicationNode();

    // list of UI text
    [SerializeField]
    public Text[] buffText;

    // Use this for initialization
    void Start ()
    {
        Dev.Where();
        buffText = GetComponentsInChildren<Text>();
        foreach( Text text in buffText)
        {
            text.text = "";
        }
	}
	
    [NVCallback]
    void BuffStackViewHandler(UpdateBuffViewEvent buffEvent)
    {
        Dev.Where();
        // Update buff display
        StartCoroutine( UpdatePlayerIDWhenPossible( buffEvent ) );
    }

    //it's possible for the avatar to not be "ready" yet
    IEnumerator UpdatePlayerIDWhenPossible( UpdateBuffViewEvent buffEvent )
    {
        int playerID = -1;
        float timeout = 10f;
        while(timeout > 0f)
        {
            yield return new WaitForEndOfFrame();
            timeout -= Time.deltaTime;

            playerID = buffEvent.Avatar.GetPlayerID();
            if( playerID < 0 )
                continue;

            buffText[ playerID ].text = "Player " + ( playerID + 1 ) + ": ";
            buffText[ playerID ].text += buffEvent.buffStack.Count.ToString();
            yield break;
        }

        /*playerID = buffEvent.Avatar.GetPlayerID();
        if( playerID < 0 )
        {
            Dev.LogError( "Error: " + buffEvent.Avatar.gameObject.name + " has no assigned player ID" );
        }*/
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
