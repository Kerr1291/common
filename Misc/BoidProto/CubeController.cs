using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Rewired;



/// Ent is short for Entity and is anything that a player could control in the game.
public class CubeController : Avatar
{
    public TriangleExplosion deathEffect;
    protected PlayerInputInterface _owner;

    public GameObject rootObject;
    public GameObject cameraTrackingPoint;

    public BaseAction buttonA;
    public BaseAction buttonB;
    public BaseAction buttonC;
    public BaseAction buttonD;

    bool keyboadAdapterA = false;

    public TimedSpawnTrigger swarm;

    public GameObject testCubePrefab;
    public GameObject enemyCubePrefab;
    public CubeBoid testCube;

    public List<string> ignoreTags;

    IEnumerator AddToTracking()
    {
        yield return new WaitForSeconds(0.1f);
        GameCamera.GetGameCamera(GetPlayerID()).AddObjectToTracking(cameraTrackingPoint);
    }

    void OnEnable()
    {
        StartCoroutine(AddToTracking());
    }

    void OnDisable()
    {
        if (Application.isEditor == false)
            GameCamera.GetGameCamera(GetPlayerID()).RemoveObjectFromTracking(cameraTrackingPoint);
    }

    public override GameObject GetAvatarObject()
    {
        if (rootObject != null)
            return rootObject;
        return null;
    }

    public int GetPlayerID()
    {
        if (_owner != null)
            return _owner.PlayerID;
        return -1;
    }

    public bool IsPlayerControlled()
    {
        if (_owner == null)
            return false;

        if (_owner.RewiredPlayer.controllers.joystickCount <= 0)
            return false;

        return true;
    }

    Vector2 up { get { return GameCamera.GetGameCamera(_owner.PlayerID).Forward2D; } }
    Vector2 down { get { return -GameCamera.GetGameCamera(_owner.PlayerID).Forward2D; } }
    Vector2 right { get { return GameCamera.GetGameCamera(_owner.PlayerID).Right2D; } }
    Vector2 left { get { return -GameCamera.GetGameCamera(_owner.PlayerID).Right2D; } }

    #region SpecialCallbacks

    ///Called by PlayerInputInterface when SetGameTarget causes a player's GAME input to go to this pix
    public override void NotifyFocus(PlayerInputInterface player)
    {
        _owner = player;
    }


    ///Called by PlayerInputInterface when SetGameTarget causes a player to leave this item's focus
    public override void NotifyFocusLost(PlayerInputInterface player)
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

    public override void HandleInputEvent_StartButton(InputActionEventData data) { }
    public override void HandleInputEvent_SelectButton(InputActionEventData data) { }

    public override void HandleInputEvent_A_BTN(InputActionEventData data)
    {
        Instantiate( enemyCubePrefab );

        if (buttonA != null)
        {
            if (data.actionId == 0)
            {
                keyboadAdapterA = !keyboadAdapterA;
                if (keyboadAdapterA)
                    buttonA.TryStartAction();
                else if (!keyboadAdapterA)
                    buttonA.TryStopAction();
            }
            else
            {
                if (keyboadAdapterA)
                    return;

                if (data.eventType == InputActionEventType.ButtonJustPressed)
                    buttonA.TryStartAction();
                else if (data.eventType == InputActionEventType.ButtonJustReleased)
                    buttonA.TryStopAction();
            }
        }
    }

    public override void HandleInputEvent_B_BTN(InputActionEventData data)
    {
        if (buttonB != null)
        {
            if (data.eventType == InputActionEventType.ButtonJustPressed)
                buttonB.TryStartAction();
            else if (data.eventType == InputActionEventType.ButtonJustReleased)
                buttonB.TryStopAction();
        }
    }

    public override void HandleInputEvent_X_BTN(InputActionEventData data)
    {
        if (buttonC != null)
        {
            if (data.eventType == InputActionEventType.ButtonJustPressed)
                buttonC.TryStartAction();
            else if (data.eventType == InputActionEventType.ButtonJustReleased)
                buttonC.TryStopAction();
        }
    }

    public override void HandleInputEvent_Y_BTN(InputActionEventData data)
    {
        if (buttonD != null)
        {
            if (data.eventType == InputActionEventType.ButtonJustPressed)
                buttonD.TryStartAction();
            else if (data.eventType == InputActionEventType.ButtonJustReleased)
                buttonD.TryStopAction();
        }
    }

    public override void HandleInputEvent_L_BUMPER(InputActionEventData data) { }
    public override void HandleInputEvent_R_BUMPER(InputActionEventData data) { }
    public override void HandleInputEvent_L_TRIGGER(InputActionEventData data)
    {
    }
    public override void HandleInputEvent_R_TRIGGER(InputActionEventData data)
    {
    }
    public override void HandleInputEvent_L_STICK_BTN(InputActionEventData data) { }
    public override void HandleInputEvent_R_STICK_BTN(InputActionEventData data) { }

    #endregion

    void MoveSwarmDirection(Vector2 dir)
    {
        //if( testCube != null )
        //{
        //    testCube.SetMove(dir.normalized);
        //    testCube.DoMove();
        //    return;
        //}
        //else
        //{
        //    testCube = Instantiate(testCubePrefab).GetComponentInChildren<CubeBoid>();
        //    return;
        //}

        //swarm.spawnedObjects = swarm.spawnedObjects.Select(x => {
        //    if (x == null)
        //        return x;
        //    CubeBoid c = x.GetComponentInChildren<CubeBoid>();
        //    if (c == null)
        //        return x;
        //    c.pushDirection = dir; return x; }).ToList();
    }

    #region StickMethods

    public override void HandleInputEvent_LStick_Up(InputActionEventData data)
    {
        //if( data != null )
        //float axis_value = data.GetAxis();
        MoveSwarmDirection(up);
    }
    public override void HandleInputEvent_LStick_Down(InputActionEventData data)
    {
        //float axis_value = data.GetAxis();
        MoveSwarmDirection(down);
    }
    public override void HandleInputEvent_LStick_Left(InputActionEventData data)
    {
        //float axis_value = data.GetAxis();
        MoveSwarmDirection(left);
    }
    public override void HandleInputEvent_LStick_Right(InputActionEventData data)
    {
        //float axis_value = data.GetAxis();
        MoveSwarmDirection(right);
    }

    public override void HandleInputEvent_RStick_Up(InputActionEventData data) { }
    public override void HandleInputEvent_RStick_Down(InputActionEventData data) { }
    public override void HandleInputEvent_RStick_Left(InputActionEventData data) { }
    public override void HandleInputEvent_RStick_Right(InputActionEventData data) { }

    #endregion

    #region DPadMethods

    public override void HandleInputEvent_DPad_Up(InputActionEventData data) { }
    public override void HandleInputEvent_DPad_Down(InputActionEventData data) { }
    public override void HandleInputEvent_DPad_Left(InputActionEventData data) { }
    public override void HandleInputEvent_DPad_Right(InputActionEventData data) { }

    #endregion
}
