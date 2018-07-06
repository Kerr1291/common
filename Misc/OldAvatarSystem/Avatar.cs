using UnityEngine;
using System.Collections;

using Rewired;

/// Avatar is anything that a player could control in the game.
public class Avatar : MonoBehaviour
{
    public virtual GameObject GetAvatarObject() { return null;  }     

 #region SpecialCallbacks

    ///Called by PlayerInputInterface when SetGameTarget causes a player's GAME input to go to this pix
    public virtual void NotifyFocus( PlayerInputInterface player ) { }


    ///Called by PlayerInputInterface when SetGameTarget causes a player to leave this item's focus
    public virtual void NotifyFocusLost( PlayerInputInterface player ) { }

 #endregion


 #region Misc

    //Generic is something that i just included in case you want to make any input do this or whatever
    public virtual void HandleInputEvent_Generic( InputActionEventData player ) { }
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

    public virtual void HandleInputEvent_StartButton( InputActionEventData player ) { }
    public virtual void HandleInputEvent_SelectButton( InputActionEventData player ) { }
    public virtual void HandleInputEvent_A_BTN( InputActionEventData player ) { }
    public virtual void HandleInputEvent_B_BTN( InputActionEventData player ) { }
    public virtual void HandleInputEvent_X_BTN( InputActionEventData player ) { }
    public virtual void HandleInputEvent_Y_BTN( InputActionEventData player ) { }
    public virtual void HandleInputEvent_L_BUMPER( InputActionEventData player ) { }
    public virtual void HandleInputEvent_R_BUMPER( InputActionEventData player ) { }
    public virtual void HandleInputEvent_L_TRIGGER( InputActionEventData player ) { }
    public virtual void HandleInputEvent_R_TRIGGER( InputActionEventData player ) { }
    public virtual void HandleInputEvent_L_STICK_BTN( InputActionEventData player ) { }
    public virtual void HandleInputEvent_R_STICK_BTN( InputActionEventData player ) { }

 #endregion


 #region StickMethods

    public virtual void HandleInputEvent_LStick_Up( InputActionEventData player ) { }
    public virtual void HandleInputEvent_LStick_Down( InputActionEventData player ) { }
    public virtual void HandleInputEvent_LStick_Left( InputActionEventData player ) { }
    public virtual void HandleInputEvent_LStick_Right( InputActionEventData player ) { }

    public virtual void HandleInputEvent_RStick_Up( InputActionEventData player ) { }
    public virtual void HandleInputEvent_RStick_Down( InputActionEventData player ) { }
    public virtual void HandleInputEvent_RStick_Left( InputActionEventData player ) { }
    public virtual void HandleInputEvent_RStick_Right( InputActionEventData player ) { }

 #endregion

 #region DPadMethods

    public virtual void HandleInputEvent_DPad_Up( InputActionEventData player ) { }
    public virtual void HandleInputEvent_DPad_Down( InputActionEventData player ) { }
    public virtual void HandleInputEvent_DPad_Left( InputActionEventData player ) { }
    public virtual void HandleInputEvent_DPad_Right( InputActionEventData player ) { }

 #endregion
}
