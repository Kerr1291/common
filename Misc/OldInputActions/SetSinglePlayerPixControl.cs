using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Rewired;

public class SetSinglePlayerPixControl : MonoBehaviour 
{
    //[Tooltip("Assign player menu control to this")]
    //public Pix pixToControl;

    [HideInInspector]
    public bool assigned { private set; get; }

    public void SetPlayerToControlMenu( PlayerInputInterface _player )
    {
        if( _player == null )
            return;
        
        if( assigned == true )
        {
            Debug.LogWarning( "This pix " + name + " is already owned. Cannot assign new owner: "+ _player.RewiredPlayer.descriptiveName );
            return;
        };

        assigned = true;

        //_player.SetMenuTarget( pixToControl );
        
        //if( pixToControl.GetComponent<PixPlayerMenu>() )
        //{
        //    pixToControl.GetComponent<PixPlayerMenu>()._owner = _player;
        //}

        //pixToControl.gameObject.SetActive( true );
    }

    void Awake()
    {
        assigned = false;
    }
}
