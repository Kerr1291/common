using UnityEngine;
using System.Collections;

using Rewired;

/// Null avatars are for misc things that will never be controlled
public class NullAvatar : Avatar
{
    public override GameObject GetAvatarObject()
    {
        return null;
    }    

    public bool IsPlayerControlled()
    {
        return false;
    }

    #region SpecialCallbacks

    ///Called by PlayerInputInterface when SetGameTarget causes a player's GAME input to go to this pix
    public override void NotifyFocus( PlayerInputInterface player )
    {
    }


    ///Called by PlayerInputInterface when SetGameTarget causes a player to leave this item's focus
    public override void NotifyFocusLost( PlayerInputInterface player )
    {
    }

    #endregion



    /// I use the xbox button inputs for ease of reference
    /// In case you forget :) here's the layout
    /// Also note that pressing down on a stick is a button
    /// 
    ///LTRIGGER                                RTRIGGER  
    /// LBUMPER                                RBUMPER
    /// 
    ///  LSTICK                             Y_BTN
    ///             SELECT (X) START    X_BTN   B_BTN
    ///      DPAD                RSTRICK    A_BTN
    /// 
    #region Buttons

    public override void HandleInputEvent_StartButton( InputActionEventData player ) { }
    public override void HandleInputEvent_SelectButton( InputActionEventData player ) { }
    public override void HandleInputEvent_A_BTN( InputActionEventData player )
    {
    }

    public override void HandleInputEvent_B_BTN( InputActionEventData player ) { }
    public override void HandleInputEvent_X_BTN( InputActionEventData player ) { }
    public override void HandleInputEvent_Y_BTN( InputActionEventData player ) { }
    public override void HandleInputEvent_L_BUMPER( InputActionEventData player ) { }
    public override void HandleInputEvent_R_BUMPER( InputActionEventData player ) { }
    public override void HandleInputEvent_L_TRIGGER( InputActionEventData player ) { }
    public override void HandleInputEvent_R_TRIGGER( InputActionEventData player ) { }
    public override void HandleInputEvent_L_STICK_BTN( InputActionEventData player ) { }
    public override void HandleInputEvent_R_STICK_BTN( InputActionEventData player ) { }

    #endregion


    #region StickMethods

    public override void HandleInputEvent_LStick_Up( InputActionEventData player )
    {
    }
    public override void HandleInputEvent_LStick_Down( InputActionEventData player )
    {
    }
    public override void HandleInputEvent_LStick_Left( InputActionEventData player )
    {
    }
    public override void HandleInputEvent_LStick_Right( InputActionEventData player )
    {
    }

    public override void HandleInputEvent_RStick_Up( InputActionEventData player ) { }
    public override void HandleInputEvent_RStick_Down( InputActionEventData player ) { }
    public override void HandleInputEvent_RStick_Left( InputActionEventData player ) { }
    public override void HandleInputEvent_RStick_Right( InputActionEventData player ) { }

    #endregion

    #region DPadMethods

    public override void HandleInputEvent_DPad_Up( InputActionEventData player ) { }
    public override void HandleInputEvent_DPad_Down( InputActionEventData player ) { }
    public override void HandleInputEvent_DPad_Left( InputActionEventData player ) { }
    public override void HandleInputEvent_DPad_Right( InputActionEventData player ) { }

    #endregion
}
