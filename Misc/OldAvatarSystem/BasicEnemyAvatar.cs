using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Rewired;

/// Ent is short for Entity and is anything that a player could control in the game.
public class BasicEnemyAvatar : Avatar
{
    public GameObject cameraTrackingPoint;

    public PlatformerMotor2D platformerController;

    public PlatformerMovement movementController;

    public List<BaseAction> actions;

    protected PlayerInputInterface _owner;

    void OnEnable()
    {
    }

    void OnDisable()
    {
        if( GameCamera.GetGameCamera( 0 ) != null )
            GameCamera.GetGameCamera( 0 ).RemoveObjectFromTracking( cameraTrackingPoint );
    }

    void OnDestroy()
    {
        if( GameCamera.GetGameCamera( 0 ) != null )
            GameCamera.GetGameCamera( 0 ).RemoveObjectFromTracking( cameraTrackingPoint );
    }

    public override GameObject GetAvatarObject()
    {
        if( platformerController != null )
            return platformerController.gameObject;
        return null;
    }

    Vector2 up { get { return Vector2.up; } }
    Vector2 down { get { return Vector2.down; } }
    Vector2 right { get { return Vector2.right; } }
    Vector2 left { get { return Vector2.left; } }

    #region SpecialCallbacks

    ///Called by PlayerInputInterface when SetGameTarget causes a player's GAME input to go to this pix
    public override void NotifyFocus( PlayerInputInterface player )
    {
        _owner = player;
    }


    ///Called by PlayerInputInterface when SetGameTarget causes a player to leave this item's focus
    public override void NotifyFocusLost( PlayerInputInterface player )
    {
        _owner = null;
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

    public override void HandleInputEvent_StartButton( InputActionEventData data ) { }
    public override void HandleInputEvent_SelectButton( InputActionEventData data ) { }

    public override void HandleInputEvent_A_BTN( InputActionEventData data )
    {
        if( actions.Count > 0 && actions[ 0] != null )
        {
            if( data.eventType == InputActionEventType.ButtonJustPressed )
                actions[ 0 ].TryStartAction();
            else if( data.eventType == InputActionEventType.ButtonJustReleased )
                actions[ 0 ].TryStopAction();
        }
    }

    public override void HandleInputEvent_B_BTN( InputActionEventData data )
    {
        if( actions.Count > 1 && actions[ 1 ] != null )
        {
            if( data.eventType == InputActionEventType.ButtonJustPressed )
                actions[ 1 ].TryStartAction();
            else if( data.eventType == InputActionEventType.ButtonJustReleased )
                actions[ 1 ].TryStopAction();
        }
    }

    public override void HandleInputEvent_X_BTN( InputActionEventData data )
    {
        if( actions.Count > 2 && actions[ 2 ] != null )
        {
            if( data.eventType == InputActionEventType.ButtonJustPressed )
                actions[ 2 ].TryStartAction();
            else if( data.eventType == InputActionEventType.ButtonJustReleased )
                actions[ 2 ].TryStopAction();
        }
    }

    public override void HandleInputEvent_Y_BTN( InputActionEventData data )
    {
        if( actions.Count > 3 && actions[ 3 ] != null )
        {
            if( data.eventType == InputActionEventType.ButtonJustPressed )
                actions[ 3 ].TryStartAction();
            else if( data.eventType == InputActionEventType.ButtonJustReleased )
                actions[ 3 ].TryStopAction();
        }
    }

    public override void HandleInputEvent_L_BUMPER( InputActionEventData data ) { }
    public override void HandleInputEvent_R_BUMPER( InputActionEventData data ) { }
    public override void HandleInputEvent_L_TRIGGER( InputActionEventData data )
    {
    }
    public override void HandleInputEvent_R_TRIGGER( InputActionEventData data )
    {
    }
    public override void HandleInputEvent_L_STICK_BTN( InputActionEventData data ) { }
    public override void HandleInputEvent_R_STICK_BTN( InputActionEventData data ) { }

    #endregion


    #region StickMethods

    public override void HandleInputEvent_LStick_Up( InputActionEventData data )
    {
        //if( data != null )
        //float axis_value = data.GetAxis();
        movementController.Move( up );
    }
    public override void HandleInputEvent_LStick_Down( InputActionEventData data )
    {
        //float axis_value = data.GetAxis();
        movementController.Move( down );
    }
    public override void HandleInputEvent_LStick_Left( InputActionEventData data )
    {
        //float axis_value = data.GetAxis();
        movementController.Move( left );
    }
    public override void HandleInputEvent_LStick_Right( InputActionEventData data )
    {
        //float axis_value = data.GetAxis();
        movementController.Move( right );
    }

    public override void HandleInputEvent_RStick_Up( InputActionEventData data ) { }
    public override void HandleInputEvent_RStick_Down( InputActionEventData data ) { }
    public override void HandleInputEvent_RStick_Left( InputActionEventData data ) { }
    public override void HandleInputEvent_RStick_Right( InputActionEventData data ) { }

    #endregion

    #region DPadMethods

    public override void HandleInputEvent_DPad_Up( InputActionEventData data ) { }
    public override void HandleInputEvent_DPad_Down( InputActionEventData data ) { }
    public override void HandleInputEvent_DPad_Left( InputActionEventData data ) { }
    public override void HandleInputEvent_DPad_Right( InputActionEventData data ) { }

    #endregion
}
