using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

//change to anything with an Instance.Log function
using DevLoggingOutput = nv.DevLog;

//disable the unreachable code detected warning for this file
#pragma warning disable 0162
    
namespace nv
{
    public class DevLog
    {
        static public DevLog Logger
        {
            get
            {
                DevLogObject d = DevLogObject.Instance;
                return DevLogObject.Logger;
            }
        }


        public class DevLogObject : GameSingleton<DevLogObject>
        {
            static public DevLog Logger;
            public static new DevLogObject Instance
            {
                get
                {
                    DevLogObject logObject = GameSingleton<DevLogObject>.Instance;
                    if(Logger == null)
                    {
                        Logger = new DevLog();
                        Logger.Setup();
                    }
                    return logObject;
                }
            }
        }

        struct LogString
        {
            public string text;
            public GameObject obj;
        }

        Queue<LogString> content = new Queue<LogString>();

        [SerializeField]
        GameObject logRoot = null;

        [SerializeField]
        GameObject logWindow = null;

        [SerializeField]
        GameObject logTextPrefab = null;

        static DevLogSettings settings;

        public static DevLogSettings Settings
        {
            get
            {
#if UNITY_EDITOR
                if(!System.IO.Directory.Exists("Assets/Resources"))
                {
                    System.IO.Directory.CreateDirectory("Assets/Resources");
                }
                settings = (DevLogSettings)AssetDatabase.LoadAssetAtPath("Assets/Resources/DevLogSettings.asset", typeof(DevLogSettings));
                //asset doesn't exist, create it
                if(settings == null)
                {
                    DevLogSettings newSettings = ScriptableObject.CreateInstance<DevLogSettings>();
                    AssetDatabase.CreateAsset(newSettings, "Assets/Resources/DevLogSettings.asset");
                    AssetDatabase.SaveAssets();
                    settings = newSettings;
                }
#else
                if(settings == null)
                    settings = (DevLogSettings)Resources.Load("DevLogSettings", typeof(DevLogSettings));            
#endif
                return settings;
            }
        }

        Vector2 logWindowSize
        {
            get
            {
                CanvasRenderer canvas = logWindow.AddComponent<CanvasRenderer>();
                return canvas.gameObject.GetOrAddComponent<RectTransform>().rect.size;
            }
            set
            {
                CanvasRenderer canvas = logWindow.AddComponent<CanvasRenderer>();
                Rect currentRect = canvas.gameObject.GetOrAddComponent<RectTransform>().rect;
                canvas.gameObject.GetOrAddComponent<RectTransform>().sizeDelta = value;
            }
        }

        public int maxLines = 10;

        public void SetupPrefabs()
        {
#if UNITY_EDITOR
            settings = (DevLogSettings)AssetDatabase.LoadAssetAtPath("devLogSettings", typeof(DevLogSettings));
            //asset doesn't exist, create it
            if(settings == null)
            {

            }
#else
            settings = (DevLogSettings)Resources.Load("devLogSettings", typeof(DevLogSettings));            
#endif


            if(logRoot == null)
            {
                logRoot = DevLogObject.Instance.gameObject;
                //logRoot = new GameObject("DebugLogRoot");
                Canvas canvas = logRoot.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.gameObject.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2( 1920f * .5f, 1080f * 20f );
                CanvasScaler canvasScaler = logRoot.AddComponent<CanvasScaler>();
                canvasScaler.referenceResolution = new Vector2( 1920f, 1080f );
            }
            if(logTextPrefab == null)
            {
                logTextPrefab = new GameObject("DebugLogTextElement");
                logTextPrefab.transform.SetParent(logRoot.transform);
                Text text = logTextPrefab.AddComponent<Text>();
                text.color = Color.red;
                text.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
                text.fontSize = 12;
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.verticalOverflow = VerticalWrapMode.Overflow;
                text.alignment = TextAnchor.MiddleLeft;
                ContentSizeFitter csf = logTextPrefab.AddComponent<ContentSizeFitter>();
                csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                logTextPrefab.gameObject.GetOrAddComponent<RectTransform>().anchorMax = new Vector2(0f, 1f);
                logTextPrefab.gameObject.GetOrAddComponent<RectTransform>().anchorMin = new Vector2(0f, 1f);
                //logTextPrefab.gameObject.GetOrAddComponent<RectTransform>().sizeDelta = Vector2.zero;
                logTextPrefab.gameObject.GetOrAddComponent<RectTransform>().anchoredPosition = new Vector2(0f,0f);
                logTextPrefab.gameObject.GetOrAddComponent<RectTransform>().pivot = new Vector2(0f, 1f);

                logTextPrefab.SetActive(false);
            }
            if( logWindow == null )
            {
                logWindow = new GameObject( "DebugLogWindow" );
                logWindow.transform.SetParent( logRoot.transform );
                CanvasRenderer canvas = logWindow.AddComponent<CanvasRenderer>();

                //create a window that fills its parent
                canvas.gameObject.GetOrAddComponent<RectTransform>().anchorMax = Vector2.one;
                canvas.gameObject.GetOrAddComponent<RectTransform>().anchorMin = Vector2.zero;
                canvas.gameObject.GetOrAddComponent<RectTransform>().sizeDelta = Vector2.zero;
                canvas.gameObject.GetOrAddComponent<RectTransform>().anchoredPosition = Vector2.zero;

                //add background image
                Image bg = logWindow.AddComponent<Image>();

                //mostly black/dark grey transparent background
                bg.color = new Color( .1f, .1f, .1f, .4f );
            }
            GameObject.DontDestroyOnLoad( logTextPrefab );
            GameObject.DontDestroyOnLoad( logRoot );
            GameObject.DontDestroyOnLoad( logWindow );
        }

        public void Hide()
        {
            logRoot.SetActive( false );
        }

        public void Show( bool show = true )
        {
            logRoot.SetActive( show );
        }

        float LineSize()
        {
            return (float)logTextPrefab.GetComponent<Text>().fontSize + logTextPrefab.GetComponent<Text>().lineSpacing;
        }

        void UpdateLog()
        {
            float line_size = LineSize();
            float total_size = content.Count * line_size;
            float max_size = logWindow.GetComponent<RectTransform>().rect.height;
            while( total_size > max_size )
            {
                LogString lString = content.Dequeue();
                GameObject.Destroy(lString.obj.gameObject);
                total_size -= line_size;
            }
            while(content.Count > maxLines)
            {
                LogString lString = content.Dequeue();
                GameObject.Destroy(lString.obj.gameObject);
            }

            UpdatePositions();
        }

        void UpdatePositions()
        {
            float size = LineSize();
            int index = 0;
            foreach(LogString lstring in content)
            {
                lstring.obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -size * index);
                ++index;
            }
        }

        public static void Log(string s)
        {
            if(Application.isPlaying && Settings.guiLoggingEnabled)
            {
                if(DevLog.Logger.logTextPrefab == null)
                    return;
                if(DevLog.Logger.logWindow == null)
                    return;
                if(DevLog.Logger.logRoot == null)
                    return;

                LogString str = new LogString() { text = s, obj = GameObject.Instantiate(DevLog.Logger.logTextPrefab) as GameObject };
                str.obj.transform.parent = DevLog.Logger.logWindow.transform;
                str.obj.SetActive(true);
                str.obj.transform.localScale = Vector3.one;
                str.obj.GetComponent<Text>().text = s;
                DevLog.Logger.content.Enqueue(str);
                DevLog.Logger.UpdateLog();
            }

            if(Settings.loggingEnabled)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.Log(s);
#else
            Wms.Framework.Trace.LogDebug(s);
#endif
            }
        }

        void Setup()
        {
            SetupPrefabs();
        }
    }

    /// <summary>
    /// Collection of tools, debug or otherwise, to improve the quality of life
    /// </summary>
    public class Dev
    {
#if UNITY_EDITOR
        public const string MENU_ROOT = "NV";
        public const string MENU_DEBUG_FOLDER = "Debug";
        [MenuItem(MENU_ROOT +"/"+ MENU_DEBUG_FOLDER + "/Print Hideflags In Selected (And Children)")]
        static void Menu_PrintHideFlags()
        {
            UnityEngine.Object[] selectedAssets = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
            foreach(var obj in selectedAssets)
            {
                GameObject go = obj as GameObject;
                if(go != null)
                {
                    PrintHideFlagsInChildren(go);
                }
            }
        }

        [MenuItem(MENU_ROOT + "/" + MENU_DEBUG_FOLDER + "/Clear Hideflags In Selected (And Children)")]
        static void Menu_ClearHideFlags()
        {
            UnityEngine.Object[] selectedAssets = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
            foreach(var obj in selectedAssets)
            {
                GameObject go = obj as GameObject;
                if(go != null)
                {
                    ClearHideFlagsInChildren(go);
                }
            }
        }

        [MenuItem(MENU_ROOT + "/" + MENU_DEBUG_FOLDER + "/Enable Runtime GUI Debug Logging")]
        static void EnableGUIDebugLogging()
        {
            DevLog.Settings.guiLoggingEnabled = true;
            Dev.Log("Runtime GUI Logging enabled");
        }

        [MenuItem(MENU_ROOT + "/" + MENU_DEBUG_FOLDER + "/Disable Runtime GUI Debug Logging")]
        static void DisableGUIDebugLogging()
        {
            Dev.Log("Runtime GUI Logging disabled");
            DevLog.Settings.guiLoggingEnabled = false;
        }

        [MenuItem(MENU_ROOT + "/" + MENU_DEBUG_FOLDER + "/Enable Debug Logging")]
        static void EnableDebugLogging()
        {
            DevLog.Settings.loggingEnabled = true;
            Dev.Log("Logging enabled");
        }

        [MenuItem(MENU_ROOT + "/" + MENU_DEBUG_FOLDER + "/Disable Debug Logging")]
        static void DisableDebugLogging()
        {
            Dev.Log("Logging disabled");
            DevLog.Settings.loggingEnabled = false;
        }
#endif
        #region Internal
        
        public static string ColorStr(int r, int g, int b)
        {
            return r.ToHexString() + g.ToHexString() + b.ToHexString();
        }

        public static string ColorStr(float r, float g, float b)
        {
            return ((int)(255.0f * Mathf.Clamp01(r))).ToHexString() + ((int)(255.0f * Mathf.Clamp01(g))).ToHexString() + ((int)(255.0f * Mathf.Clamp01(b))).ToHexString();
        }

        static string GetFunctionHeader(int frameOffset = 0, bool fileInfo = false)
        {
            if(frameOffset <= -BaseFunctionHeader)
                frameOffset = -BaseFunctionHeader;

            //get stacktrace info
            StackTrace stackTrace = new StackTrace(true);
            StackFrame stackFrame = stackTrace.GetFrame(BaseFunctionHeader + frameOffset);
            System.Reflection.MethodBase method = stackFrame.GetMethod();
            Type classType = method.ReflectedType;
            string class_name = classType.Name;

            bool isIEnumerator = false;
            
            //we're in a coroutine, get a better set of info
            if(class_name.Contains(">c__Iterator"))
            {
                isIEnumerator = true;
                classType = method.ReflectedType.DeclaringType;
                class_name = classType.Name;
            }

            //build parameters string
            System.Reflection.ParameterInfo[] parameters = method.GetParameters();
            string parameters_name = "";
            bool add_comma = false;
            foreach( System.Reflection.ParameterInfo parameter in parameters )
            {
                if( add_comma )
                {
                    parameters_name += ", ";
                }

                parameters_name += Dev.Colorize( parameter.ParameterType.Name, _param_color );
                parameters_name += " ";
                parameters_name += Dev.Colorize( parameter.Name, _log_color );

                add_comma = true;
            }

            //build function header
            string function_name = "";

            if(isIEnumerator)
            {
                string realMethodName = method.ReflectedType.Name.Substring(method.ReflectedType.Name.IndexOf('<') + 1, method.ReflectedType.Name.IndexOf('>') - 1);

                function_name = "IEnumerator:" + realMethodName;
            }
            else
            {
                function_name = method.Name + "(" + parameters_name + ")";
            }

            //string file = stackFrame.GetFileName().Remove(0, Application.dataPath.Length);
            string file = System.IO.Path.GetFileName(stackFrame.GetFileName());
            int line = stackFrame.GetFileLineNumber();

            string fileLineHeader = "";
            if(fileInfo)
                fileLineHeader = file + "(" + line + "):";

            return fileLineHeader + class_name + "." + function_name;
        }

        static string Colorize( string text, string colorhex )
        {
#if ENABLE_COLOR
            string str = "<color=#" + colorhex + ">" + "<b>" + text + "</b>" + "</color>";
#else
            string str = text;
#endif
            return str;
        }

        static string FunctionHeader( int frameOffset = 0, bool fileInfo = true )
        {
            return Dev.Colorize( Dev.GetFunctionHeader( frameOffset, fileInfo), Dev._method_color );
        }

#endregion

#region Settings
        
        public static int BaseFunctionHeader = 3;

        static string _method_color
        {
            get
            {
                return DevLog.Settings.methodColor.ColorToHex();
            }
            set
            {
                ColorUtility.TryParseHtmlString(value, out DevLog.Settings.methodColor);
            }
        }

        static string _log_color
        {
            get
            {
                return DevLog.Settings.logColor.ColorToHex();
            }
            set
            {
                ColorUtility.TryParseHtmlString(value, out DevLog.Settings.logColor);
            }
        }

        static string _log_warning_color
        {
            get
            {
                return DevLog.Settings.logWarningColor.ColorToHex();
            }
            set
            {
                ColorUtility.TryParseHtmlString(value, out DevLog.Settings.logWarningColor);
            }
        }

        static string _log_error_color
        {
            get
            {
                return DevLog.Settings.logErrorColor.ColorToHex();
            }
            set
            {
                ColorUtility.TryParseHtmlString(value, out DevLog.Settings.logErrorColor);
            }
        }

        static string _param_color
        {
            get
            {
                return DevLog.Settings.paramColor.ColorToHex();
            }
            set
            {
                ColorUtility.TryParseHtmlString(value, out DevLog.Settings.paramColor);
            }
        }

        public class Settings
        {

            public static void SetMethodColor( int r, int g, int b ) { Dev._method_color = ColorStr( r, g, b ); }
            public static void SetMethodColor( float r, float g, float b ) { Dev._method_color = ColorStr( r, g, b ); }
            public static void SetMethodColor( Color c ) { Dev._method_color = c.ColorToHex(); }

            public static void SetLogColor( int r, int g, int b ) { Dev._log_color = ColorStr( r, g, b ); }
            public static void SetLogColor( float r, float g, float b ) { Dev._log_color = ColorStr( r, g, b ); }
            public static void SetLogColor( Color c ) { Dev._log_color = c.ColorToHex(); }

            public static void SetLogWarningColor(int r, int g, int b) { Dev._log_warning_color = ColorStr(r, g, b); }
            public static void SetLogWarningColor(float r, float g, float b) { Dev._log_warning_color = ColorStr(r, g, b); }
            public static void SetLogWarningColor(Color c) { Dev._log_warning_color = c.ColorToHex(); }

            public static void SetLogErrorColor(int r, int g, int b) { Dev._log_error_color = ColorStr(r, g, b); }
            public static void SetLogErrorColor(float r, float g, float b) { Dev._log_error_color = ColorStr(r, g, b); }
            public static void SetLogErrorColor(Color c) { Dev._log_error_color = c.ColorToHex(); }

            public static void SetParamColor( int r, int g, int b ) { Dev._param_color = ColorStr( r, g, b ); }
            public static void SetParamColor( float r, float g, float b ) { Dev._param_color = ColorStr( r, g, b ); }
            public static void SetParamColor( Color c ) { Dev._param_color = c.ColorToHex(); }

        }
#endregion

#region Logging


        public static void Where(int stackFrameOffset = 0)
        {
            DevLoggingOutput.Log(" :::: " + Dev.FunctionHeader(stackFrameOffset));
        }

        public static void LogError( string text )
        {
            DevLoggingOutput.Log(Dev.FunctionHeader() + Dev.Colorize(text, _log_error_color));
        }

        public static void LogWarning( string text )
        {
            DevLoggingOutput.Log(Dev.FunctionHeader() + Dev.Colorize(text, _log_warning_color));
        }

        public static void Log( string text )
        {
            DevLoggingOutput.Log(Dev.FunctionHeader() + Dev.Colorize(text, _log_color));
        }

        public static void Log( string text, int r, int g, int b )
        {
            DevLoggingOutput.Log((Dev.FunctionHeader() + Dev.Colorize(text, Dev.ColorStr(r, g, b))));
        }
        public static void Log( string text, float r, float g, float b )
        {
            DevLoggingOutput.Log(Dev.FunctionHeader() + Dev.Colorize(text, Dev.ColorStr(r, g, b)));
        }

        public static void Log( string text, Color color )
        {
            DevLoggingOutput.Log(Dev.FunctionHeader() + Dev.Colorize(text, color.ColorToHex()));
        }

        /// <summary>
        /// Print the value of the variable in a simple and clean way... 
        /// ONLY USE FOR QUICK AND TEMPORARY DEBUGGING (will not work as expected outside the editor)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="var"></param>
        public static void LogVar<T>( T var )
        {
#if UNITY_EDITOR 
            string var_name = GetVarName(var);// var.GetType().
#else
            string var_name = var == null ? "Null" : var.GetType().Name;
#endif
            string var_value = Convert.ToString( var );
            DevLoggingOutput.Log(Dev.FunctionHeader() + Dev.Colorize(var_name, _param_color) + " = " + Dev.Colorize(var_value, _log_color));
        }

        /// <summary>
        /// Print the value of the variable in a simple and clean way
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="var"></param>
        public static void LogVar<T>(string label, T var)
        {
            string var_name = label;
            string var_value = Convert.ToString(var);
            DevLoggingOutput.Log(Dev.FunctionHeader() + Dev.Colorize(var_name, _param_color) + " = " + Dev.Colorize(var_value, _log_color));
        }

        /// <summary>
        /// Print the content of the array passed in
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        public static void LogVarArray<T>( string label, IList<T> array )
        {
            int size = array.Count;
            for( int i = 0; i < size; ++i )
            {
                string vname = label + "[" + Dev.Colorize(Convert.ToString(i), _log_color) + "]";
                DevLoggingOutput.Log(Dev.FunctionHeader() + Dev.Colorize(vname, _param_color) + " = " + Dev.Colorize(Convert.ToString(array[i]), _log_color));
            }
        }

        public static void LogVarOnlyThis<T>( string label, T var, string input_name, string this_name )
        {
            if(this_name != input_name)
                return;

            string var_name = label;
            string var_value = Convert.ToString(var);
            DevLoggingOutput.Log(Dev.FunctionHeader() + Dev.Colorize(var_name, _param_color) + " = " + Dev.Colorize(var_value, _log_color));
        }
#endregion

        #region Helpers

        public static string ColorString( string input, Color color )
        {
            return Dev.Colorize( input, color.ColorToHex());
        }

        public static void PrintHideFlagsInChildren( GameObject parent, bool print_nones = false )
        {
            bool showed_where = false;

            if( print_nones )
            {
                Dev.Where();
                showed_where = true;
            }

            foreach( Transform child in parent.GetComponentsInChildren<Transform>() )
            {
                if(print_nones && child.gameObject.hideFlags == HideFlags.None)
                    DevLoggingOutput.Log(Dev.Colorize(child.gameObject.name, Color.white.ColorToHex()) + ".hideflags = " + Dev.Colorize(Convert.ToString(child.gameObject.hideFlags), _param_color));
                else if(child.gameObject.hideFlags != HideFlags.None)
                {
                    if( !showed_where )
                    {
                        Dev.Where();
                        showed_where = true;
                    }
                    DevLoggingOutput.Log(Dev.Colorize(child.gameObject.name, Color.white.ColorToHex()) + ".hideflags = " + Dev.Colorize(Convert.ToString(child.gameObject.hideFlags), _param_color));
                }
            }
        }

        public static void ClearHideFlagsInChildren( GameObject parent )
        {
            foreach( Transform child in parent.GetComponentsInChildren<Transform>() )
            {
                child.gameObject.hideFlags = HideFlags.None;
            }
        }

#if UNITY_EDITOR
        class GetVarNameHelper
        {
            public static Dictionary<string, string> _cached_name = new Dictionary<string, string>();
        }

        static string GetVarName( object obj )
        {
            StackFrame stackFrame = new StackTrace(true).GetFrame(2);
            string fileName = stackFrame.GetFileName();
            int lineNumber = stackFrame.GetFileLineNumber();
            string uniqueId = fileName + lineNumber;
            if( GetVarNameHelper._cached_name.ContainsKey( uniqueId ) )
                return GetVarNameHelper._cached_name[ uniqueId ];
            else
            {
                System.IO.StreamReader file = new System.IO.StreamReader(fileName);
                for( int i = 0; i < lineNumber - 1; i++ )
                    file.ReadLine();
                string varName = file.ReadLine().Split(new char[] { '(', ')' })[1];
                GetVarNameHelper._cached_name.Add( uniqueId, varName );
                return varName;
            }
        }
#else
        class GetVarNameHelper
        {
            public static Dictionary<string, string> _cached_name = new Dictionary<string, string>();
        }

        static string GetVarName( object obj )
        {
            return obj == null ? "Null" : obj.GetType().Name;
        }
#endif
        #endregion
    }

}

#pragma warning restore 0162
