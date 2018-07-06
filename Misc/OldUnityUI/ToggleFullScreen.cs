using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToggleFullScreen : MonoBehaviour 
{

    public void SetFullScreen( )
    {
        bool value = GetComponent<Toggle>().isOn;

        if( Application.isEditor )
        {
            Debug.Log( "fullscreen = " + value );
            return;
        }

        Resolution current = Screen.currentResolution;
        Screen.SetResolution( current.width, current.height, value );
        GameCamera.SaveResolutionData();
    }
}
