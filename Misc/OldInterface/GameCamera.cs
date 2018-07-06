using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class GameCamera : MonoBehaviour {

    static List<GameCamera> _instances = new List<GameCamera>();
    static List<Camera> _game_cams = new List<Camera>();

    private int _camera_player_id = -1;
    //The player this camera is assigned to
    public int CameraPlayerID
    {
        get
        {
            return _camera_player_id;
        }
        private set
        {
            _camera_player_id = value;
        }
    }

    //static Camera _game_cam = null;

    public const int defaultResolutionW = 1920;
    public const int defaultResolutionH = 1080;
    public const bool defaultGameIsFullScreen = true;

    //public static GameCamera Instance
    //{
    //    get; private set;
    //}

    //public static Camera GameCam
    //{
    //    get
    //    {
    //        return _game_cam;
    //    }
    //}

    public static GameCamera GetGameCamera( int player )
    {
        if( player < _instances.Count )
            return _instances[ player ];

        if( _instances.Count > 0 )
            return _instances[ 0 ];

        return null;
    }

    public static Camera GetCamera(int player)
    {
        if( player < _game_cams.Count )
            return _game_cams[ player ];

        if( _instances.Count > 0 )
            return _game_cams[ 0 ];

        return null;
    }

    static public void CreateDefaultResolutionData()
    {
        //if( Application.isEditor )
        //    return;

        //int current_res_w = defaultResolutionW;
        //int current_res_h = defaultResolutionH;
        //bool is_fullscreen = defaultGameIsFullScreen;

        //BinaryFormatter bf = new BinaryFormatter();
        //FileStream file = File.Create( GameFunctions.GetResolutionDataPath() );

        //bf.Serialize( file, current_res_w );
        //bf.Serialize( file, current_res_h );
        //bf.Serialize( file, is_fullscreen );
        //file.Close();
    }

    static public void SaveResolutionData()
    {
        //if( Application.isEditor )
        //    return;

        //Dev.Log( "Saving resolution data from: " + GameFunctions.GetResolutionDataPath() );

        //int current_res_w = Screen.currentResolution.width;
        //int current_res_h = Screen.currentResolution.height;
        //bool is_fullscreen = Screen.fullScreen;

        //BinaryFormatter bf = new BinaryFormatter();
        //FileStream file = File.Create( GameFunctions.GetResolutionDataPath() );

        //bf.Serialize( file, current_res_w );
        //bf.Serialize( file, current_res_h );
        //bf.Serialize( file, is_fullscreen );
        //file.Close();
    }

    static public void LoadResolutionData()
    {
        //if( Application.isEditor )
        //    return;

        //Dev.Log( "Loading resolution data from: " + GameFunctions.GetResolutionDataPath() );

        //if( File.Exists( GameFunctions.GetResolutionDataPath() ) )
        //{
        //    BinaryFormatter bf = new BinaryFormatter();
        //    FileStream file = File.Open( GameFunctions.GetResolutionDataPath(), FileMode.Open );
        //    int current_res_w = (int)bf.Deserialize( file );
        //    int current_res_h = (int)bf.Deserialize( file );
        //    bool is_fullscreen = (bool)bf.Deserialize( file );
        //    file.Close();

        //    Dev.Log( "Loading screen resolution " + current_res_w + " x " + current_res_h );

        //    Screen.SetResolution( current_res_w, current_res_h, is_fullscreen );
        //}
    }

    void Awake()
    {
        _localCamera = GetComponent<Camera>();

        _instances.Add( this );

        if( GetComponent<Camera>() )
        {
            if( forceCameraPlayerID )
            {
                CameraPlayerID = forcedPlayerID;
            }
            else
            {
                CameraPlayerID = _game_cams.Count;
            }

            _game_cams.Add( GetComponent<Camera>() );
        }

        if( Application.isPlaying )
        {
            if( directionTranslator2D == null )
            {
                GameObject directionTranslatorFor2D = new GameObject("2DDirectionTranslator");
                directionTranslatorFor2D.transform.SetParent(transform);
                directionTranslator2D = directionTranslatorFor2D.transform;
                directionTranslator2D.localPosition = Vector3.zero;
                directionTranslator2D.localRotation = Quaternion.identity;
            }
        }

        //generate default settings file
        //if( false == File.Exists( GameFunctions.GetResolutionDataPath() ) )
        //    CreateDefaultResolutionData();

            //GameCamera.LoadResolutionData();

        cameraDesiredDirection = cameraCurrentDirection;
        desiredAngle = viewAngle;

        UpdateResolutions();
    }

    void OnDestroy()
    {
        _instances.Remove( this );

        if( GetComponent<Camera>() )
            _game_cams.Remove( GetComponent<Camera>() );
    }

    [Header("Used to place the camera manually for debugging")]
    [SerializeField]
    private bool debugStopUpdates = false;
    [SerializeField]
    private bool debugStopPan = false;
    [SerializeField]
    private bool debugStopZoom = false;
    [SerializeField]
    private bool debugStopRotate = false;

    [Header("Force this camera to be assigned to this player?")]
    [SerializeField]
    private bool forceCameraPlayerID = false;
    [SerializeField]
    private int forcedPlayerID = 0;

    [SerializeField]
    [Header("Change this to change where the camera will start in the scene")]
    public Vector3 cameraDefaultPosition = Vector3.zero;

    [Header("If disabled camera will not track anything")]
    public bool cameraTrackingEnabled = true;
    
    [Header("Current center of the camera's focus")]
    public Vector3 currentViewCenter = Vector3.zero;
    Vector3 calculatedViewCenter = Vector3.zero;

    [Header("How fast the camera moves to the focus point")]
    public float cameraPanningRate = 1.0f;
    public float cameraMinPanningRate = 0.01f;
    [Header("If we get closer than this, stop panning")]
    public Vector3 cameraMinPanningDistance = new Vector3(0.01f,0.01f,0.01f);
    
    [Header("Objects that the camera is trying to focus on")]
    public List<GameObject> significantObjects;

    [Header("Override object that may be used for camera positional focusing")]
    public GameObject centerObject = null;

    [Header("Override object that may be used for camera directional focusing")]
    public GameObject directionObject = null;

    [Header("Used to give objects relative 2D plane directions for this camera regardless of angle")]
    [Tooltip("Used to give objects relative 2D plane directions for this camera regardless of angle")]
    public Transform directionTranslator2D = null;

    [Header("The angle the camera sits at")]
    public float viewAngle = 90.0f;
    public float desiredAngle = 90.0f;
    public float desiredAngleAdjustRate = 0.1f;
    public float desiredAngleMinAdjustRate = 0.01f;
    public float defaultViewAngle = 54.0f;
    
    [Header("The direction the camera is facing")]
    public Vector2 cameraCurrentDirection = new Vector2(0.0f,1.0f);
    public Vector2 cameraDesiredDirection = new Vector2(0.0f,1.0f);
    public Vector2 cameraDefaultDirection = new Vector2(0.0f,1.0f);

    [Header("How fast the camera rotates to the focus direction")]
    public float cameraDirectionRate = 0.1f;
    public float cameraMinDirectionRate = 0.01f;

    [Header("Camera view distances")]
    public float minCameraDistance = 0.0f;
    public float maxCameraDistance = 100.0f;
    public float currentViewDistance = 10.0f;
    public float defaultViewDistance = 34.0f;

    [Header("Used to determine game view area")]
    public SphereCollider visibleCollider;
    public SphereCollider visibleOuterCollider;

    public float visibleScale = 0.8f;
    public float visibleOuterScale = 0.9f;

    [Header("How fast the camera adjusts to get keep objects on screen")]
    public float cameraZoomRate = 1.0f;
    public float cameraMinZoomRate = 0.01f;
    [Header("Distances under this value will not trigger zoom-ins")]
    public float cameraMinZoomDistance = 1.0f;

    [Header("Time (in seconds) until the camera will zoom in")]
    public float cameraZoomInDelay = 2.0f;
    float _zoom_in_delay_time = 0.0f;

    Camera _localCamera;

    public enum CameraZoomState
    {
          Stable
        , Out
        , In
    }
    
    ///You can check this to see if the camera is currently "zooming" in some way
    public CameraZoomState ZoomState { get; private set; }

    //void OnDisable()
    //{
    //    AudioListener al = GetComponent<AudioListener>();

    //    if( al == null )
    //        return;
            
    //    if( Application.isPlaying )
    //        Destroy( al );

    //    foreach( GameCamera c in _instances )
    //    {
    //        if( c == this )
    //            continue;

    //        AudioListener al2 = c.GetComponent<AudioListener>();
    //        if( al == null &&  == true )
    //            al = c.gameObject.AddComponent<AudioListener>();

    //        if( need_listener == true && c.gameObject.activeInHierarchy == true && al.enabled == false )
    //        {
    //            al.enabled = true;
    //            need_listener = false;
    //        }
    //        else
    //        {
    //            if( Application.isPlaying )
    //                Destroy( al );
    //        }
    //    }
    //}
    
    void LateUpdate()
    {
        if( _localCamera == null )
            _localCamera = GetComponent<Camera>();

        //bool need_listener = true;
        //foreach( GameCamera c in _instances )
        //{
        //    AudioListener al = c.GetComponent<AudioListener>();

            //    if( al == null )
            //        continue;

            //    if( need_listener == true && c.gameObject.activeInHierarchy == true && al.enabled == false )
            //    {
            //        al.enabled = true;
            //        need_listener = false;
            //    }
            //    else
            //    {
            //        if( Application.isPlaying )
            //            Destroy( al );
            //    }
            //}
            //if( need_listener )
            //{
            //    foreach( GameCamera c in _instances )
            //    {
            //        AudioListener al = c.GetComponent<AudioListener>();
            //        if( al == null && need_listener == true )
            //            al = c.gameObject.AddComponent<AudioListener>();

            //        if( need_listener == true && c.gameObject.activeInHierarchy == true && al.enabled == false )
            //        {
            //            al.enabled = true;
            //            need_listener = false;
            //        }
            //        else
            //        {
            //            if( Application.isPlaying )
            //                Destroy(al);
            //        }
            //    }
            //}

        if( debugStopUpdates )
            return;

        if( Application.isEditor && Application.isPlaying == false )
        {
            SnapViewCenter( cameraDefaultPosition );
            UpdateCameraOrientation();
            return;
        }

        if( cameraTrackingEnabled )
            UpdateCameraTracking();
    }
    
    PlayerGameInterface forcedPlayer = null;

    void UpdateCameraTracking()
    {
        if( forceCameraPlayerID )
        {
            if( forcedPlayer == null )
                forcedPlayer = GameInput.GetPlayer( forcedPlayerID );

            if( forcedPlayer != null 
                && significantObjects.Count <= 0 
                && forcedPlayer.GetTarget() != null
                && forcedPlayer.GetTarget().GetAvatarObject() != null )
                AddObjectToTracking( forcedPlayer.GetTarget().GetAvatarObject() );
        }

        if( debugStopPan == false )
        {
            UpdateViewCenter();
        }

        UpdateCameraOrientation();
    }

    void UpdateCameraOrientation()
    {
        if( directionObject != null )
            SetCameraDirectionFromObject( directionObject );

        UpdateCameraDirectionAngleUsingRate();

        UpdateCameraPosition();

        UpdateDistance();
        
        if( directionTranslator2D != null )
        {
            float x_rotation = -transform.localRotation.eulerAngles.x;
            directionTranslator2D.localRotation = Quaternion.Euler(x_rotation,0.0f,0.0f);
        }
    }


    void PanCamera( Vector3 adjust_distance )
    {
        Vector3 adjust_direction = adjust_distance.normalized;
        float clamped_adjust = Mathf.Max( adjust_distance.magnitude * cameraPanningRate, cameraMinPanningRate );

        Vector3 adjust_amount = adjust_direction * clamped_adjust;

        //stop panning if we're close to the target
        if( Mathf.Abs( adjust_amount.x ) <= cameraMinPanningDistance.x )
            adjust_amount.x = 0.0f;
        if( Mathf.Abs( adjust_amount.y ) <= cameraMinPanningDistance.y )
            adjust_amount.y = 0.0f;
        if( Mathf.Abs( adjust_amount.z ) <= cameraMinPanningDistance.z )
            adjust_amount.z = 0.0f;

        currentViewCenter = currentViewCenter + adjust_amount;
    }

    void UpdateCameraDirectionAngleUsingRate()
    {
        Vector2 direction_delta = cameraDesiredDirection - cameraCurrentDirection;

        float clamped_adjust = Mathf.Max( direction_delta.magnitude * cameraDirectionRate, cameraMinDirectionRate );
        
        if( debugStopRotate == true || Application.isPlaying == false )
        {
            clamped_adjust = 0;
        }

        Vector2 adjust_amount = direction_delta.normalized * clamped_adjust;

        cameraCurrentDirection = cameraCurrentDirection + adjust_amount;

        cameraCurrentDirection = cameraCurrentDirection.normalized;

        direction_delta = cameraDesiredDirection - cameraCurrentDirection;
        if( Mathf.Abs( direction_delta.x ) <= 0.01f )
            cameraCurrentDirection.x = cameraDesiredDirection.x;
        if( Mathf.Abs( direction_delta.y ) <= 0.01f )
            cameraCurrentDirection.y = cameraDesiredDirection.y;

        if( debugStopRotate == true || Application.isPlaying == false )
        {
            cameraCurrentDirection = cameraDefaultDirection;
        }

        Quaternion viewRot = Quaternion.LookRotation(new Vector3(cameraCurrentDirection.x, 0.0f, cameraCurrentDirection.y), new Vector3(0.0f,1.0f,0.0f));

        transform.localRotation = viewRot;
    }

    void UpdateCameraAngle()
    {
        float angle_delta = desiredAngle - viewAngle;

        if( angle_delta < Mathf.Abs(0.1f) )
        {
            viewAngle = desiredAngle;
            return;
        }

        float clamped_adjust = Mathf.Max( angle_delta * desiredAngleAdjustRate, desiredAngleMinAdjustRate );
        viewAngle = viewAngle + clamped_adjust;
    }

    void UpdateCameraPosition()
    {
        UpdateCameraAngle();

        Quaternion viewRot = Quaternion.AngleAxis( viewAngle, transform.right );
        transform.localRotation = viewRot * transform.localRotation;
        transform.position = currentViewCenter;
        transform.localPosition -= transform.forward * currentViewDistance;

        if(_localCamera.orthographic)
        {
            _localCamera.orthographicSize = currentViewDistance;
        }
    }


    void UpdateViewCenter()
    {
        if( centerObject != null )
        {
            calculatedViewCenter = centerObject.transform.position;
            SetViewCenter( calculatedViewCenter );
            return;
        }

        if( significantObjects.Count <= 0 )
            return;

        float count = 0.0f;

        calculatedViewCenter = Vector3.zero;

        for( int i = 0; i < significantObjects.Count; ++i )
        {
            if( significantObjects[ i ] == null )
                continue;

            calculatedViewCenter += significantObjects[ i ].transform.position;
            count += 1.0f;
        }
        
        calculatedViewCenter = calculatedViewCenter / count;

        SetViewCenter( calculatedViewCenter );
    }

    void UpdateDistance()
    {
        if( visibleCollider == null || visibleOuterCollider == null )
            return;

        CalculateCameraViewColliders();

        if( debugStopZoom )
            return;

        float farthest_from_center = CalculateFarthestSignificanObjectFromCenter();

        float zoom_out_radius = visibleOuterCollider.radius;
        float zoom_in_radius = visibleCollider.radius;

        //zoom out -- not allowed with only one object
        if( significantObjects.Count > 1 
            && farthest_from_center > zoom_out_radius )
        {
            _zoom_in_delay_time = 0.0f;
            ZoomState = CameraZoomState.Out;
            float adjust_distance = farthest_from_center - zoom_out_radius;
            ZoomCamera( adjust_distance );
        }
        //or zoom in -- not allowed with only one object
        //tighten up the view focus
        else if( significantObjects.Count > 1 
            && cameraMinZoomDistance < farthest_from_center 
            && farthest_from_center < zoom_in_radius 
            && _zoom_in_delay_time >= cameraZoomInDelay )
        {
            ZoomState = CameraZoomState.In;
            float adjust_distance = farthest_from_center - zoom_in_radius;
            ZoomCamera( adjust_distance );
        }
        //or zoom to default
        //tighten up the view focus
        else if( significantObjects.Count == 1
            && Mathf.Abs(defaultViewDistance - currentViewDistance) > .001f
            && _zoom_in_delay_time >= cameraZoomInDelay )
        {
            ZoomState = CameraZoomState.In;
            float adjust_distance = defaultViewDistance - currentViewDistance;
            ZoomCamera( adjust_distance );
        }
        //no change
        else
        {
            if( _zoom_in_delay_time < cameraZoomInDelay )
                _zoom_in_delay_time += Time.deltaTime;
            ZoomState = CameraZoomState.Stable;
        }
    }

    void CalculateCameraViewColliders()
    {
        visibleCollider.radius = 0.5f * currentViewDistance * visibleScale;
        visibleCollider.transform.position = currentViewCenter;

        visibleOuterCollider.radius = 0.5f * currentViewDistance * visibleOuterScale;
        visibleOuterCollider.transform.position = currentViewCenter;
    }

    float CalculateFarthestSignificanObjectFromCenter()
    {
        if( centerObject != null )
        {
            return ( centerObject.transform.position - currentViewCenter ).magnitude;
        }

        float max_dist = 0.0f;

        //TODO: clean this up and have it remove nulls
        foreach( GameObject g in significantObjects )
        {
            if( g == null )
                continue;

            float dist = ( g.transform.position - currentViewCenter ).magnitude;
            if( dist > max_dist )
                max_dist = dist;
        }

        return max_dist;
    }

    void ZoomCamera(float adjust_distance)
    {
        float sgn = Mathf.Sign(adjust_distance);
        float clamped_adjust = sgn * Mathf.Max( Mathf.Abs(adjust_distance) * cameraZoomRate, cameraMinZoomRate );
        currentViewDistance = Mathf.Clamp( currentViewDistance + clamped_adjust, minCameraDistance, maxCameraDistance );
    }

    void UpdateResolutions()
    {
        int num_active = 0;
        for( int i = 0; i < _instances.Count; ++i )
        {
            if( _instances[i].gameObject.activeInHierarchy == true )
                num_active++;
        }

        Rect camera_rect = new Rect();

        //one camera resolution setup
        if( num_active > 0 && num_active <= 1 )
        {
            camera_rect.x = 0.0f;
            camera_rect.y = 0.0f;
            camera_rect.width = 1.0f;
            camera_rect.height = 1.0f;

            for( int i = 0; i < _instances.Count; ++i )
            {
                if( _instances[ i ].gameObject.activeInHierarchy == true )
                    _game_cams[ i ].rect = camera_rect;
            }
        }

        //2 camera resolution setup
        if( num_active > 1 && num_active <= 2 )
        {
            camera_rect.x = 0.0f;
            camera_rect.y = 0.0f;
            camera_rect.width = 1.0f;
            camera_rect.height = 0.5f;

            for( int i = 0; i < _instances.Count; ++i )
            {
                if( _instances[ i ].gameObject.activeInHierarchy == true )
                {
                    _game_cams[ i ].rect = camera_rect;
                    camera_rect.y += 0.5f;
                }
            }
        }

        //3 camera resolution setup
        if( num_active > 2 && num_active <= 3 )
        {
            camera_rect.x = 0.0f;
            camera_rect.y = 0.0f;
            camera_rect.width = 0.5f;
            camera_rect.height = 0.5f;

            for( int i = 0; i < _instances.Count; ++i )
            {
                if( _instances[ i ].gameObject.activeInHierarchy == true )
                {
                    _game_cams[ i ].rect = camera_rect;
                    camera_rect.x += 0.5f;

                    if( camera_rect.x >= 1.0f )
                    {
                        camera_rect.width = 1.0f;
                        camera_rect.x = 0.0f;
                        camera_rect.y = 0.5f;
                    }
                }
            }
        }

        //4 camera resolution setup
        if( num_active > 3 && num_active <= 4 )
        {
            camera_rect.x = 0.0f;
            camera_rect.y = 0.0f;
            camera_rect.width = 0.5f;
            camera_rect.height = 0.5f;

            for( int i = 0; i < _instances.Count; ++i )
            {
                if( _instances[ i ].gameObject.activeInHierarchy == true )
                {
                    _game_cams[ i ].rect = camera_rect;
                    camera_rect.x += 0.5f;

                    if( camera_rect.x >= 1.0f )
                    {
                        camera_rect.x = 0.0f;
                        camera_rect.y = 0.5f;
                    }
                }
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////

    public void SetCameraActive(bool value)
    {
        gameObject.SetActive( value );
        UpdateResolutions();
    }

    [ContextMenu("Restore Camera To Defaults")]
    public void RestoreCameraToDefaults()
    {
        viewAngle = defaultViewAngle;
        SnapViewCenter( cameraDefaultPosition );
        SnapCameraDirection( cameraDefaultDirection );
        currentViewDistance = defaultViewDistance;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////

    public void AddObjectToTracking( GameObject obj )
    {
        if( significantObjects.Contains( obj ) )
            return;

        if( obj == null )
            return;

        //Dev.Log( "adding " + obj.name + " to camera tracking." );

        significantObjects.Add( obj );
    }

    public void RemoveObjectFromTracking( GameObject obj )
    {
        if( significantObjects.Contains( obj ) == false )
            return;

        if( obj == null )
            return;

        //Dev.Log( "removing " + obj.name + " to camera tracking." );

        significantObjects.Remove( obj );
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////

    public void SetCameraAngle( float angle )
    {
        desiredAngle = angle;
    }

    public void SnapCameraAngle( float angle )
    {
        desiredAngle = angle;
        viewAngle = angle;
    }

    public void SetOverrideTrackingObject( GameObject target )
    {
        centerObject = target;
    }

    public void SetOverrideDirectionObject( GameObject target )
    {
        directionObject = target;
    }

    public void SetCameraDirectionDefault()
    {
        SetCameraDirection( cameraDefaultDirection );
    }
    
    public void SetCameraDirection( Vector2 direction )
    {
        cameraDesiredDirection = direction;
        cameraDesiredDirection = cameraDesiredDirection.normalized;
    }

    public void SnapCameraDirection( Vector2 direction )
    {
        cameraDesiredDirection = direction;
        cameraDesiredDirection = cameraDesiredDirection.normalized;
        cameraCurrentDirection = cameraDesiredDirection;
    }

    public void SetCameraDirectionFromObject( GameObject direction_object )
    {
        Vector3 direction_to_object = direction_object.transform.position - transform.position;
        direction_to_object.y = 0.0f;
        direction_to_object = direction_to_object.normalized;
        Vector2 direction = new Vector2( direction_to_object.x, direction_to_object.z );

        cameraDesiredDirection = direction;
        cameraDesiredDirection = cameraDesiredDirection.normalized;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////

    public void SetViewCenter( Vector3 pos )
    {
        Vector3 adjustDirection = pos - currentViewCenter;
        PanCamera( adjustDirection );
    }

    public void SnapViewCenter( Vector3 pos )
    {
        currentViewCenter = pos;
    }

    public Vector3 GetViewCenter()
    {
        if( significantObjects.Count <= 0 )
            return currentViewCenter;

        return calculatedViewCenter;
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////

    public Vector3 Forward
    {
        get
        {
            return new Vector3( directionTranslator2D.forward.x, 0.0f, directionTranslator2D.forward.z );
        }
    }

    public Vector3 Right
    {
        get
        {
            return new Vector3( directionTranslator2D.right.x, 0.0f, directionTranslator2D.right.z );
        }
    }

    public Vector3 Forward2D
    {
        get
        {
            return new Vector2( directionTranslator2D.forward.x, directionTranslator2D.forward.z );
        }
    }

    public Vector3 Right2D
    {
        get
        {
            return new Vector2( directionTranslator2D.right.x, directionTranslator2D.right.z );
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////
}
