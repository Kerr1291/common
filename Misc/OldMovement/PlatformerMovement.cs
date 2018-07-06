using UnityEngine;
using System.Collections;

public class PlatformerMovement : BaseMovement
{
    public PlatformerMotor2D platformerController;
    public Animator movementAnimator;

    public float jumpRotationSpeed = 500f;
    
    bool _restored = true;
    bool _enableOneWayPlatforms;
    bool _oneWayPlatformsAreWalls;

    bool _currentFacingLeft;
    bool _isJumping;

    // before enter en freedom state for ladders
    void FreedomStateSave( PlatformerMotor2D motor )
    {
        if( !_restored ) // do not enter twice
            return;

        _restored = false;
        _enableOneWayPlatforms = platformerController.enableOneWayPlatforms;
        _oneWayPlatformsAreWalls = platformerController.oneWayPlatformsAreWalls;
    }

    // after leave freedom state for ladders
    void FreedomStateRestore( PlatformerMotor2D motor )
    {
        if( _restored ) // do not enter twice
            return;

        _restored = true;
        platformerController.enableOneWayPlatforms = _enableOneWayPlatforms;
        platformerController.oneWayPlatformsAreWalls = _oneWayPlatformsAreWalls;
    }

    bool CheckCanRun()
    {
        if( platformerController == null )
            return false;

        return true;
    }

    bool HasXMovementThisFrame {
        get; set;
    }

    public override void Move( Vector2 dir )
    {
        if( !CheckCanRun() )
            return;
        
        // XY freedom movement
        if( platformerController.motorState == PlatformerMotor2D.MotorState.FreedomState )
        {
            platformerController.normalizedXMovement = dir.x;
            platformerController.normalizedYMovement = dir.y;

            return; // do nothing more
        }

        // X axis movement
        if( Mathf.Abs( dir.x ) > PC2D.Globals.INPUT_THRESHOLD )
        {
            platformerController.normalizedXMovement = dir.x;
            HasXMovementThisFrame = true;
        }

        //platformerController.normalizedXMovement = dir.x;

        if( Mathf.Abs(dir.y) >= 0.1f )
        {
            bool up_pressed = dir.y > 0;
            if( platformerController.IsOnLadder() )
            {
                if(
                    ( up_pressed && platformerController.ladderZone == PlatformerMotor2D.LadderZone.Top )
                    ||
                    ( !up_pressed && platformerController.ladderZone == PlatformerMotor2D.LadderZone.Bottom )
                 )
                {
                    // do nothing!
                }
                // if player hit up, while on the top do not enter in freeMode or a nasty short jump occurs
                else
                {
                    // example ladder behaviour

                    platformerController.FreedomStateEnter(); // enter freedomState to disable gravity
                    platformerController.EnableRestrictedArea();  // movements is retricted to a specific sprite bounds

                    // now disable OWP completely in a "trasactional way"
                    FreedomStateSave( platformerController );
                    platformerController.enableOneWayPlatforms = false;
                    platformerController.oneWayPlatformsAreWalls = false;

                    // start XY movement
                    platformerController.normalizedXMovement = dir.x;
                    platformerController.normalizedYMovement = dir.y;
                }
            }
        }
        else if( dir.y < -PC2D.Globals.FAST_FALL_THRESHOLD )
        {
            platformerController.fallFast = false;
        }
    }

    void Update()
    {
        // use last state to restore some ladder specific values
        if( platformerController.motorState != PlatformerMotor2D.MotorState.FreedomState )
        {
            // try to restore, sometimes states are a bit messy because change too much in one frame
            FreedomStateRestore( platformerController );
        }

        CalculateMovementAnimation();
    }

    void LateUpdate()
    {
        if(!HasXMovementThisFrame)
        {
            platformerController.normalizedXMovement = 0f;
        }
        HasXMovementThisFrame = false;
    }

    public void CalculateMovementAnimation()
    {
        if( movementAnimator == null )
            return;

        if( platformerController.motorState == PlatformerMotor2D.MotorState.Jumping ||
                _isJumping &&
                    ( platformerController.motorState == PlatformerMotor2D.MotorState.Falling ||
                                 platformerController.motorState == PlatformerMotor2D.MotorState.FallingFast ) )
        {
            _isJumping = true;
            movementAnimator.Play( "Jump" );

            Vector3 rotateDir = _currentFacingLeft ? Vector3.forward : Vector3.back;
            model.transform.Rotate( rotateDir, jumpRotationSpeed * Time.deltaTime );
            SetFacingWithVelocity();
        }
        else
        {
            _isJumping = false;
            model.transform.rotation = Quaternion.identity;

            if( platformerController.motorState == PlatformerMotor2D.MotorState.Falling ||
                             platformerController.motorState == PlatformerMotor2D.MotorState.FallingFast )
            {
                movementAnimator.Play( "Fall" );
                SetFacingWithVelocity();
            }
            else if( platformerController.motorState == PlatformerMotor2D.MotorState.WallSliding ||
                     platformerController.motorState == PlatformerMotor2D.MotorState.WallSticking )
            {
                movementAnimator.Play( "Cling" );
            }
            else if( platformerController.motorState == PlatformerMotor2D.MotorState.OnCorner )
            {
                movementAnimator.Play( "On Corner" );
            }
            else if( platformerController.motorState == PlatformerMotor2D.MotorState.Slipping )
            {
                movementAnimator.Play( "Slip" );
            }
            else if( platformerController.motorState == PlatformerMotor2D.MotorState.Dashing )
            {
                movementAnimator.Play( "Dash" );
                SetFacingWithVelocity();
            }
            else
            {
                if( platformerController.velocity.sqrMagnitude >= 0.1f * 0.1f )
                {
                    movementAnimator.Play( "Walk" );
                    SetFacingWithVelocity();
                }
                else
                {
                    movementAnimator.Play( "Idle" );
                }
            }

            //if( platformerController.velocity.x <= -0.1f )
            //{
            //    _currentFacingLeft = true;
            //}
            //else if( platformerController.velocity.x >= 0.1f )
            //{
            //    _currentFacingLeft = false;
            //}

            //// Facing
            //float valueCheck = platformerController.normalizedXMovement;

            //if( platformerController.motorState == PlatformerMotor2D.MotorState.Slipping ||
            //    platformerController.motorState == PlatformerMotor2D.MotorState.Dashing ||
            //    platformerController.motorState == PlatformerMotor2D.MotorState.Jumping )
            //{
            //    valueCheck = platformerController.velocity.x;
            //}

            //if( Mathf.Abs( valueCheck ) >= 0.1f )
            //{
            //    Vector3 newScale = model.transform.localScale;
            //    newScale.x = Mathf.Abs( newScale.x ) * ( ( valueCheck > 0 ) ? 1.0f : -1.0f );
            //    model.transform.localScale = newScale;
            //}
        }
    }

    void SetFacingWithVelocity()
    {
        if( platformerController.velocity.x <= -0.1f )
        {
            SetFacingLeft( true );
        }
        else if( platformerController.velocity.x >= 0.1f )
        {
            SetFacingLeft( false );
        }
    }

    void SetFacingLeft(bool left)
    {
        Vector3 newScale = model.transform.localScale;
        newScale.x = Mathf.Abs( newScale.x ) * ( ( left ) ? -1.0f : 1.0f );
        model.transform.localScale = newScale;
    }

    void Awake()
    {
        platformerController.onJump -= SetCurrentFacingLeft;
        platformerController.onJump += SetCurrentFacingLeft;
    }

    void OnEnable()
    {
        if( movementAnimator == null )
            return;
        movementAnimator.Play( "Idle" );
    }

    void SetCurrentFacingLeft()
    {
        _currentFacingLeft = platformerController.facingLeft;
    }
}
