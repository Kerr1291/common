using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(SystemSettings))]
public class SystemSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SystemSettings _target = (SystemSettings)target;

        if( GUILayout.Button("Reload settings") )
        {
            _target.Init();
        }

        base.OnInspectorGUI();
    }
}
#endif

public class SystemSettings : MonoBehaviour
{
    static public SystemSettings Instance
    {
        get; private set;
    }

    public void Awake()
    {
        Init();
    }

    [Serializable]
    public class Graphics
    {
        [SerializeField]
        bool vsyncEnabled;

        public bool VSync
        {
            get
            {
                vsyncEnabled = QualitySettings.vSyncCount > 0;
                return vsyncEnabled;
            }
            set
            {
                vsyncEnabled = value;
                if( vsyncEnabled )
                    QualitySettings.vSyncCount = 1;
                else
                    QualitySettings.vSyncCount = 0;
            }
        }

        //setup the property values to match the values that were set in the inspector
        public void Init()
        {
            VSync = vsyncEnabled;
        }
    }

    public Graphics graphicsSettings;

    public void Init()
    {
        if( Instance == null )
            Instance = this;
        else
        {
            if( Instance != this )
                Debug.LogError( "Warning: An instance of SystemSettings is already defined in game object " + Instance.name + " cannot create a new instance in gameo object " + name );
        }

        graphicsSettings.Init();
    }

}