﻿using UnityEngine;
using System.Collections;

using Rewired;

public class IA_Game_R_STICK_H : InputAction
{
    //What the action will act on
    PlayerGameInterface _input_interface;

    void Awake()
    {
        _input_interface = GetComponent<PlayerGameInterface>();
    }

    public override string GetActionTypeName() { return this.GetType().Name; }

    /// <summary>
    /// Implement the action here
    /// </summary>
    /// <param name="player">source of the action</param>
    public override void DoAction( InputActionEventData player )
    {
        if( _input_interface.GetTarget() == null )
            return;

        if( player.GetAxis() < 0 )
        {
            _input_interface.GetTarget().HandleInputEvent_RStick_Left( player );
        }

        if( player.GetAxis() > 0 )
        {
            _input_interface.GetTarget().HandleInputEvent_RStick_Right( player );
        }
    }
}
