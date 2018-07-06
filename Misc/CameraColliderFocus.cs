using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class CameraColliderFocus : MonoBehaviour 
{
    public List<PlayerInputInterface> focusList = new List<PlayerInputInterface>();

    [Header("should this check for inactive players and remove them from tracking?")]
    public bool doCleanFocusList = false;

    public enum FocusType
    {
          Nothing
        , Direction
        , Position
        , Angle
        , ObjectPosition
        , ObjectDirection
        , ResetToDefaults
    }
    public enum UpdateFocusType
    {
          Nothing
        , Direction
        , Position
        , Angle
        , ObjectDirection
    }

    public enum ExitFocusType
    {
          Nothing
        , RestoreDirection
        , RestoreAngle
        , ClearObjectTracking
        , ClearDirectionTracking
        , ResetToDefaults
    }

    [Header("Do this when players enter the collider")]
    public FocusType focusBehavior;

    [Header("Do this when players are inside the collider")]
    public UpdateFocusType updateFocusBehavior;

    [Header("Do this when players exit the collider")]
    public ExitFocusType exitFocusBehavior;

    [Header("Use these with the respective enter or update behaviors")]
    public Vector3 focusPosition;
    public Vector2 focusDirectiton;
    public float focusAngle;
    public GameObject focusObjectPosition;
    public GameObject focusObjectDirection;

    bool loaded = false;

    void OnTriggerEnter( Collider other )
    {
        //an avatar entered!
        Avatar avt = other.GetComponentInChildren<Avatar>();
        if( avt == null )
            return;

        //is it a player?
        PlayerInputInterface pii = GameInput.GetPlayerControllingAvatar( avt );
        if( pii == null )
            return;

        if( focusBehavior != FocusType.Nothing && focusList.Contains( pii ) == false )
            CheckEnterBehaviors( pii );

        if( focusList.Contains( pii ) == false )
            focusList.Add( pii );
    }

    void OnTriggerExit( Collider other )
    {
        //an avatar exited!
        Avatar avt = other.GetComponentInChildren<Avatar>();
        if( avt == null )
            return;

        //is it a player?
        PlayerInputInterface pii = GameInput.GetPlayerControllingAvatar( avt );
        if( pii == null )
            return;

        if( focusList.Contains( pii ) == true )
            focusList.Remove( pii );

        if( focusBehavior != FocusType.Nothing )
            CheckExitBehaviors( pii );
    }

    List<PlayerInputInterface> _remove_list = new List<PlayerInputInterface>();

    void CleanFocusList()
    {
        if( doCleanFocusList == false )
            return;

        //don't track players that aren't active anymore for one reason or another
            foreach( PlayerInputInterface pii in focusList )
        {
            if( pii.GetAvatar() == null )
                _remove_list.Add( pii );

            if( pii.GetAvatar() != null && pii.GetAvatar().gameObject.activeInHierarchy == false )
                _remove_list.Add( pii );
        }

        if( _remove_list.Count > 0 )
        {
            foreach( PlayerInputInterface pii in _remove_list )
            {
                focusList.Remove( pii );
            }
            _remove_list.Clear();
        }
    }

    void Update()
    {
        if( !loaded )
        {
            if( GameInput.GetAllPlayers().Count > 0 )
                loaded = true;

            if( GetComponent<BoxCollider>() )
            {
                foreach( PlayerInputInterface pii in GameInput.GetAllPlayers() )
                {
                    if( pii.GetAvatar() == null )
                        continue;

                    if( GetComponent<BoxCollider>().bounds.Contains( pii.GetAvatar().transform.position ) )
                        if( focusList.Contains( pii ) == false )
                            focusList.Add( pii );
                }
            }
        }

        if( focusList.Count <= 0 )
            return;

        CleanFocusList();

        foreach( PlayerInputInterface pii in focusList )
            CheckUpdateBehaviors( pii );
    }

    //GetGameCamera( owner.PlayerID )

    void CheckEnterBehaviors(PlayerInputInterface pii)
    {
        if( focusBehavior == FocusType.Nothing )
            return;

        GameCamera playerCamera = GameCamera.GetGameCamera(pii.PlayerID);

        if( playerCamera == null )
            return;

        if( focusBehavior == FocusType.Position )
            playerCamera.SetViewCenter( focusPosition );

        if( focusBehavior == FocusType.Direction )
            playerCamera.SetCameraDirection( focusDirectiton );

        if( focusBehavior == FocusType.Angle )
            playerCamera.SetCameraAngle(focusAngle);

        if( focusBehavior == FocusType.ObjectPosition )
            playerCamera.SetOverrideTrackingObject( focusObjectPosition );

        if( focusBehavior == FocusType.ObjectDirection )
            playerCamera.SetOverrideDirectionObject( focusObjectDirection );

        if( focusBehavior == FocusType.ResetToDefaults )
            playerCamera.RestoreCameraToDefaults();
    }

    void CheckUpdateBehaviors( PlayerInputInterface pii )
    {
        if( updateFocusBehavior == UpdateFocusType.Nothing )
            return;

        GameCamera playerCamera = GameCamera.GetGameCamera(pii.PlayerID);

        if( playerCamera == null )
            return;

        if( updateFocusBehavior == UpdateFocusType.Position )
            playerCamera.SetViewCenter( focusPosition );

        if( updateFocusBehavior == UpdateFocusType.Direction )
            playerCamera.SetCameraDirection( focusDirectiton );

        if( updateFocusBehavior == UpdateFocusType.ObjectDirection )
            playerCamera.SetOverrideDirectionObject( focusObjectDirection );

        if( updateFocusBehavior == UpdateFocusType.Angle )
            playerCamera.SetCameraAngle(focusAngle);
    }

    void CheckExitBehaviors( PlayerInputInterface pii )
    {
        if( exitFocusBehavior == ExitFocusType.Nothing )
            return;

        GameCamera playerCamera = GameCamera.GetGameCamera(pii.PlayerID);

        if( playerCamera == null )
            return;

        if( exitFocusBehavior == ExitFocusType.RestoreDirection )
            playerCamera.SetCameraDirection( playerCamera.cameraDefaultDirection );

        if( exitFocusBehavior == ExitFocusType.RestoreAngle )
            playerCamera.SetCameraAngle(playerCamera.defaultViewAngle);

        if( exitFocusBehavior == ExitFocusType.ClearObjectTracking )
            playerCamera.SetOverrideTrackingObject( null );

        if( exitFocusBehavior == ExitFocusType.ClearDirectionTracking )
        {
            playerCamera.SetOverrideDirectionObject( null );
            playerCamera.SetCameraDirectionDefault();
        }

        if( exitFocusBehavior == ExitFocusType.ResetToDefaults )
            playerCamera.RestoreCameraToDefaults();
    }
}
