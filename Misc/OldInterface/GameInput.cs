using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

using nv;
using Rewired;

public class GameInput : MonoBehaviour
{
    static public bool IsControllerNew( ControllerStatusChangedEventArgs args )
    {
        foreach( PlayerInputInterface pii in _loaded_players )
        {
            if( true == pii.RewiredPlayer.controllers.ContainsController( ReInput.controllers.GetJoystick( args.controllerId ) ) )
                return false;
        }

        return true;
    }

    public static GameInput Instance { get; private set; }    
    
    static List<PlayerInputInterface> _loaded_players = new List<PlayerInputInterface>();

    public static PlayerInputInterface GetPlayerControllingAvatar( Avatar avt )
    {
        foreach( PlayerInputInterface p_interface in GameInput._loaded_players )
        {
            if( p_interface.GetAvatar() == avt )
                return p_interface;
        }
        return null;
    }

    public static List<PlayerInputInterface> GetAllPlayers()
    {
        return GameInput._loaded_players;
    }

    public static PlayerGameInterface GetPlayer(int id)
    {
        for(int i = 0; i < _loaded_players.Count; ++i )
        {
            if( _loaded_players[ i ].PlayerID == id )
                return _loaded_players[ i ].GameInterface;
        }
        return null;
    }

    static public void InitPlayerInput(PlayerInputInterface player)
    {
        if( GameInput._loaded_players.Contains( player ) )
            return;

        int next_size = GameInput._loaded_players.Count + 1;

        if( next_size > ReInput.players.Players.Count )
        {
            Dev.LogWarning( "ReWired is currently setup to support " + ReInput.players.Players.Count + " players. You'll need to add more in the rewired editor if you want more. Cannot add new controller/player: " + player.name );
            return;
        }
        
        int player_index = GameInput._loaded_players.Count;

        //player._player = ReInput.players.Players[ player_index ];
        player.INTERNAL_SetRewiredPlayer( ReInput.players.Players[player_index] );
        player.INTERNAL_SetRewiredPlayerIndex( GameInput._loaded_players.Count );
        //player._rewired_player_index = GameInput._loaded_players.Count;

        GameInput._loaded_players.Add( player );
    }

    //static public PlayerInputInterface GetInterfaceFromPlayer( Player player )
    //{
    //    foreach( PlayerInputInterface p_interface in GameInput._loaded_players )
    //    {
    //        if( p_interface._player == player )
    //            return p_interface;
    //    }
    //    return null;
    //}
    

    void Reset()
    {
        Init( this );
    }
    
    void Awake()
    {
        Init( this );        
    }

    //TODO: can't do this on awake, need to do it a short time after
    //float hacktime = 0.0f;
    //void Update()
    //{
    //    if( hacktime < 1.0f )
    //        hacktime += Time.deltaTime;
    //    if( hacktime > 1.0f && hacktime < 1.2f )
    //    {
    //        foreach( PlayerInputInterface p in GetAllPlayers() )
    //        {
    //            p.SetMenuTarget( gameStartupTarget );
    //        }
    //    }
    //}
    
    public void Init()
    {
        if (GameInput.Instance != null)
            return;

        GameInput.Init( this );
    }

    static void Init(GameInput ginput)
    {
        if( GameInput.Instance != null )
            return;

        GameInput.Instance = ginput;
    }
}
