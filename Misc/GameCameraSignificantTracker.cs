using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using nv;

public class GameCameraSignificantTracker : MonoBehaviour {

    public LayerMask trackingMask;

    void OnTriggerEnter2D( Collider2D other )
    {
        if( Dev.IsLayerInMask( trackingMask, other.gameObject) && GameCamera.GetGameCamera(0) != null )
        {
            GameCamera.GetGameCamera( 0 ).AddObjectToTracking( other.gameObject );
        }
    }

    void OnTriggerExit2D( Collider2D other )
    {
        if( Dev.IsLayerInMask( trackingMask, other.gameObject ) && GameCamera.GetGameCamera( 0 ) != null )
        {
            GameCamera.GetGameCamera( 0 ).RemoveObjectFromTracking( other.gameObject );
        }
    }
}
