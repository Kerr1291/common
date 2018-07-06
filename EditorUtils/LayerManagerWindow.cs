using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nv.editor
{
    public class GameLayers : ScriptableObject
    {
#if UNITY_EDITOR
        [MenuItem(nv.editor.Consts.MENU_ROOT + "/Assets/Create GameLayers")]
        public static GameLayers CreateLayerContainer()
        {
            GameLayers asset = ScriptableObject.CreateInstance<GameLayers>();

            AssetDatabase.CreateAsset(asset, "Assets/Resources/GameLayers.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;

            return asset;
        }
#endif

        static string[] defaultLayers =
        {
         "UNITY_RESERVED" //0
        ,"UNITY_RESERVED"
        ,"UNITY_RESERVED"
        ,"UNITY_RESERVED"
        ,"UNITY_RESERVED" //4 (5th item)
         
        ,"UNITY_RESERVED"
        ,"UNITY_RESERVED"
        ,"UNITY_RESERVED" //7 (8th item)
         
         //start defaults
        ,""
        ,"" //layer 9 - 10th

        ,"World" //layer 11 - world
        ,""
        ,""
        ,""
        ,"" //layer 14 - 15th

        ,"Trigger" //layer 15 - triggers
        ,""
        ,""
        ,""
        ,"" //layer 19 - 20th
         
        ,""
        ,""
        ,""
        ,"Avatar" //layer 23 - avatar special
        ,"" //layer 24 - 25th
         
        ,""
        ,""
        ,"Camera" //layer 27 - special camera layer
        ,"Debug" //layer 28
        ,"" //layer 29 - 30th
         
        ,""
        ,"" //layer 31 - 32nd
    };


        public string[] layers;

        public string[] Layers
        {
            get
            {
                if(layers == null)
                    layers = new List<string>(DefaultLayers).ToArray();
                return layers;
            }
        }

        static public List<string> DefaultLayers
        {
            get
            {
                return defaultLayers.ToList();
            }
        }

        static public int GetLayerIndex(string layer)
        {
            for(int i = 0; i < defaultLayers.Length; ++i)
            {
                if(layer == defaultLayers[i])
                    return i;
            }
            return 0; //Default layer
        }
    }

#if UNITY_EDITOR
    public class LayerManagerWindow : EditorWindow
    {
        const float _base_w = 480.0f;
        const float _base_h = 640.0f;

        const float _window_pos_x = 200.0f;
        const float _window_pos_y = 200.0f;

        //place the window under mouse when created?
        static bool repositionEnabled = false;
        static bool repositionWindowOnce = false;

        GameLayers layersObject;

        static float WindowHeight
        {
            get
            {
                return _base_h;
            }
        }

        static float WindowWidth
        {
            get
            {
                return _base_w;
            }
        }

        static Vector2 DefaultWindowPos
        {
            get
            {
                return new Vector2(_window_pos_x, _window_pos_y);
            }
        }

        public static string LayerAssetPath
        {
            get
            {
                return "ProjectSettings/TagManager.asset";
            }
        }

        static Rect CalculateWindowDimensions()
        {
            Rect windowDimensions = new Rect(DefaultWindowPos.x, DefaultWindowPos.x //x, y
                                                        ,
                                                        WindowWidth, WindowHeight //w x h
                                                        );

            return windowDimensions;
        }

        //Hotkey: Ctrl + shift + alt + L
        [MenuItem(nv.editor.Consts.MENU_ROOT + "/Window/Game Layer Manager %#&l")]
        static void OpenLayerWindow()
        {
            repositionWindowOnce = repositionEnabled;

            Rect windowDimensions = CalculateWindowDimensions();

            CreateWindow(windowDimensions);
        }

        static void CreateWindow(Rect dimensions)
        {
            LayerManagerWindow window = ScriptableObject.CreateInstance<LayerManagerWindow>();
            window.position = dimensions;
            window.ShowUtility();
        }

        Vector2 scrollPos;

        void OnGUI()
        {
            //TODO: Create an object field and have the user set the game layers object through that instead of it auto-creating below

            if(layersObject == null)
            {
                layersObject = GameLayers.CreateLayerContainer();
            }

            Rect newRect = CalculateWindowDimensions();
            if(LayerManagerWindow.repositionWindowOnce)
                this.position = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), new Vector2(newRect.width, newRect.height));

            LayerManagerWindow.repositionWindowOnce = false;

            EditorGUILayout.LabelField("Game Layers:", EditorStyles.wordWrappedLabel);
            EditorGUILayout.LabelField("", EditorStyles.wordWrappedLabel);
            GUILayout.Space(5);

            SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(LayerAssetPath)[0]);
            SerializedProperty layersProp = manager.FindProperty("layers");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Layer", GUILayout.MaxWidth(100));
            EditorGUILayout.LabelField("Current", EditorStyles.label, GUILayout.MaxWidth(200));
            EditorGUILayout.LabelField("Default", EditorStyles.label, GUILayout.MaxWidth(200));
            EditorGUILayout.EndHorizontal();

            //allow editing of the project's layers
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            for(int i = 8; i <= 31; i++)
            {
                SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);


                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Layer " + i, GUILayout.MaxWidth(100));
                EditorGUILayout.PropertyField(sp, GUIContent.none, GUILayout.Width(180));
                if(layersObject.Layers[i].Length > 0)
                    EditorGUILayout.LabelField("" + layersObject.Layers[i], EditorStyles.label, GUILayout.MaxWidth(200));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            manager.ApplyModifiedProperties();

            GUILayout.Space(5);

            //Set any default layers to the Game Default layers
            if(GUILayout.Button("Add Game defaults (will not replace)"))
            {
                for(int i = 8; i <= 31; i++)
                {
                    SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);

                    if(sp.stringValue.Length > 0)
                        continue;

                    if(layersObject.Layers[i].Length > 0)
                        sp.stringValue = layersObject.Layers[i];
                }
                manager.ApplyModifiedProperties();
            }

            //Set any default layers to the Game Default layers
            if(GUILayout.Button("Set to Game defaults"))
            {
                for(int i = 8; i <= 31; i++)
                {
                    SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);

                    if(layersObject.Layers[i].Length > 0)
                        sp.stringValue = layersObject.Layers[i];
                }
                manager.ApplyModifiedProperties();
            }

            if(GUILayout.Button("Close"))
            {
                this.Close();
            }

            Event e = Event.current;
            switch(e.type)
            {
                case EventType.KeyDown:
                    {
                        if(Event.current.keyCode == (KeyCode.Escape))
                        {
                            this.Close();
                        }
                        break;
                    }
            }
        }
    }
}

#endif