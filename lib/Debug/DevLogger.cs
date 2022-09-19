using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nv
{
    public class DevLogger : ScriptableSingleton<DevLogger>, Logger.IDevLogger
    {
#if UNITY_EDITOR
        [MenuItem(nv.editor.Consts.Menu.ROOT + nv.editor.Consts.Menu.ASSETS + "/Create (or Enable) Debug Logger")]
        static void EnableDebugLogging()
        {
            Dev.Logger = Instance;
            Instance.LoggingEnabled = true;
            Dev.Log("Logging enabled");
        }
#endif


        //[SerializeField] protected bool breakOnError = true;
        [SerializeField] protected bool captureDefaultDebugLog = true;
        [SerializeField] protected bool showFileAndLineNumber = true;
        [SerializeField] protected bool showMethodParameters = false;
        [SerializeField] protected bool showClassName = true;
        [SerializeField] protected bool loggingEnabled = true;
        [SerializeField] protected bool guiLoggingEnabled = false;
        [SerializeField] protected bool colorizeText = true;
        [SerializeField] protected Color logColor = Color.white;
        [SerializeField] protected Color logWarningColor = Color.yellow;
        [SerializeField] protected Color logErrorColor = Color.red;
        [SerializeField] protected Color methodColor = Color.cyan;
        [SerializeField] protected Color paramColor = Color.green;
        public bool showTimeStampOnGUILogging = true;

        //public bool BreakOnError { get { return breakOnError; } set { breakOnError = value; } }
        public bool CaptureDefaultDebugLog { get { return captureDefaultDebugLog; } set { captureDefaultDebugLog = value; } }
        public bool ShowFileAndLineNumber { get { return showFileAndLineNumber; } set { showFileAndLineNumber = value; } }
        public bool ShowMethodParameters  { get { return showMethodParameters; } set { showMethodParameters = value; } }
        public bool ShowClassName         { get { return showClassName; } set { showClassName = value; } }
        public bool LoggingEnabled        { get { return loggingEnabled; } set { loggingEnabled = value; } }
        public bool GuiLoggingEnabled     { get { return guiLoggingEnabled; } set { guiLoggingEnabled = value; } }
        public bool ColorizeText          { get { return colorizeText; } set { colorizeText = value; } }
        public Color LogColor             { get { return logColor; } set { logColor = value; } }
        public Color LogWarningColor      { get { return logWarningColor; } set { logWarningColor = value; } }
        public Color LogErrorColor        { get { return logErrorColor; } set { logErrorColor = value; } }
        public Color MethodColor          { get { return methodColor; } set { methodColor = value; } }
        public Color ParamColor           { get { return paramColor; } set { paramColor = value; } }

        [SerializeField, Tooltip("Ignore any log messages that contain these strings")]
        protected List<string> ignoreFilters;

        public List<string> IgnoreFilters
        {
            get
            {
                return ignoreFilters;
            }
        }

        [HideInInspector, SerializeField, Header("GUI Logging Settings")]
        GameObject logRoot = null;

        [HideInInspector, SerializeField]
        GameObject logWindow = null;

        [HideInInspector, SerializeField]
        Slider historySlider = null;

        [SerializeField]
        MonoBehaviourFactory debugLogTextPool;

        [SerializeField]
        protected int maxGUILines = 100;
        [SerializeField]
        protected int historySize = 1000;

        [SerializeField]
        protected float guiRenderAreaSize = 1000f;

        [SerializeField]
        protected Color bgColor = new Color(.1f, .1f, .1f, .4f);

        [SerializeField]
        protected Vector2 referenceResolution = new Vector2(1080f, 1920f);

        [SerializeField]
        protected Vector2 logSize = new Vector2(.5f, 20f);

        [SerializeField]
        protected int guiHistoryPosition = 0;
        protected int GUIHistoryPosition
        {
            get
            {
                return guiHistoryPosition;
            }
            set
            {
                guiHistoryPosition = Mathf.Clamp(value, 0, historySize);
                UpdateContent();
            }
        }

        CircularBuffer<string> history;
        CircularBuffer<string> History
        {
            get
            {
                if(history == null || history.Count <= 0)
                    history = new CircularBuffer<string>(historySize, historySize);
                return history;
            }
        }

        Queue<PoolableLogString> content;
        Queue<PoolableLogString> Content
        {
            get
            {
                if(content == null)
                    content = new Queue<PoolableLogString>();
                return content;
            }
        }

        static bool historySliderShowing = false;

        static public int MaxLines
        {
            get
            {
                return Instance.maxGUILines;
            }
        }

        public GameObject LogRoot
        {
            get
            {
                if(logRoot == null)
                {
                    logRoot = new GameObject("Debug Log Root");
                    Canvas canvas = logRoot.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 100;
                    canvas.gameObject.GetOrAddComponent<RectTransform>().sizeDelta = new Vector2(referenceResolution.x * logSize.x, referenceResolution.y * logSize.y);
                    CanvasScaler canvasScaler = LogRoot.AddComponent<CanvasScaler>();
                    canvasScaler.referenceResolution = referenceResolution;

                    if(IsPlaying)
                        GameObject.DontDestroyOnLoad(logRoot);
                    else
                        canvasScaler.StartCoroutine(SetDontDestroyASAP(logRoot));

                    GraphicRaycaster raycaster = LogRoot.AddComponent<GraphicRaycaster>();
                }

                return logRoot;
            }
            set
            {
                logRoot = value;
            }
        }

        IEnumerator SetDontDestroyASAP(GameObject target)
        {
            while(!IsPlaying)
            {
                if(target == null)
                    yield break;
                yield return null;
            }

            GameObject.DontDestroyOnLoad(target);

            if(target == HistorySlider.gameObject)
            {
                HideSlider();
            }
        }

        public GameObject LogWindow
        {
            get
            {
                if(logWindow == null)
                {
                    logWindow = new GameObject("DebugLogWindow");
                    logWindow.transform.SetParent(LogRoot.transform);
                    CanvasRenderer canvas = logWindow.AddComponent<CanvasRenderer>();

                    canvas.gameObject.GetOrAddComponent<RectTransform>().anchorMax = Vector2.one;

                    //create a window that fills its parent
                    canvas.gameObject.GetComponent<RectTransform>().anchorMin = new Vector2(0f,.8f);
                    canvas.gameObject.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
                    canvas.gameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    //add background image
                    Image bg = logWindow.AddComponent<Image>();

                    //mostly black/dark grey transparent background
                    bg.color = bgColor;
                    if(IsPlaying)
                        GameObject.DontDestroyOnLoad(logWindow);
                    else
                        bg.StartCoroutine(SetDontDestroyASAP(logWindow));
                }
                return logWindow;
            }
            set
            {
                logWindow = value;
            }
        }

        public Slider HistorySlider
        {
            get
            {
                if(historySlider == null)
                {
                    var slider = new GameObject("DebugHistorySlider");
                    slider.transform.SetParent(LogWindow.transform);
                    historySlider = slider.AddComponent<Slider>();
                    historySlider.direction = Slider.Direction.BottomToTop;
                    historySlider.maxValue = historySize;
                    historySlider.minValue = 0;
                    historySlider.wholeNumbers = true;

                    Color slideColor = Color.red;
                    var colors = historySlider.colors;
                    colors.normalColor = slideColor;
                    historySlider.colors = colors;

                    {
                        var rt = slider.GetOrAddComponent<RectTransform>();
                        rt.anchorMax = Vector2.one;
                        rt.anchorMin = new Vector2(.9f, 0f);
                        rt.pivot = Vector2.one;
                        rt.sizeDelta = Vector2.zero;
                        rt.anchoredPosition = Vector2.zero;
                    }

                    //add background image
                    {
                        var bgGO = new GameObject("Background");
                        bgGO.transform.SetParent(slider.transform);
                        Image bg = bgGO.AddComponent<Image>();
                        bg.color = bgColor;

                        {
                            var rt = bgGO.GetOrAddComponent<RectTransform>();
                            rt.anchorMax = new Vector2(.75f,1f);
                            rt.anchorMin = new Vector2(.25f,0f);
                            rt.pivot = Vector2.one * .5f;
                            rt.sizeDelta = Vector2.zero;
                            rt.anchoredPosition = Vector2.zero;

                        }
                    }

                    //add fill area
                    {
                        var fillArea = new GameObject("FillArea");
                        fillArea.transform.SetParent(slider.transform);
                        {
                            var rt = fillArea.GetOrAddComponent<RectTransform>();
                            rt.anchorMax = new Vector2(.75f, 1f);
                            rt.anchorMin = new Vector2(.25f, 0f);
                            rt.pivot = Vector2.one * .5f;
                            rt.sizeDelta = Vector2.zero;
                            rt.anchoredPosition = Vector2.zero;
                        }

                        //create internal fill rect
                        Color fillColor = Color.white;
                        fillColor.a = .25f;
                        {
                            var fill = new GameObject("Fill");
                            fill.transform.SetParent(fillArea.transform);
                            Image bg = fill.AddComponent<Image>();
                            bg.color = fillColor;
                            {
                                var rt = fill.GetOrAddComponent<RectTransform>();
                                rt.anchorMax = new Vector2(1f, 0f);
                                rt.anchorMin = new Vector2(0f, 0f);
                                rt.pivot = Vector2.one * .5f;
                                rt.sizeDelta = Vector2.zero;
                                rt.anchoredPosition = Vector2.zero;

                                historySlider.fillRect = rt;
                            }
                        }
                    }


                    //add slide area
                    {
                        var slideArea = new GameObject("SlideArea");
                        slideArea.transform.SetParent(slider.transform);
                        {
                            var rt = slideArea.GetOrAddComponent<RectTransform>();
                            rt.anchorMax = Vector2.one;
                            rt.anchorMin = Vector2.zero;
                            rt.pivot = Vector2.one * .5f;
                            rt.sizeDelta = Vector2.zero;
                            rt.anchoredPosition = Vector2.zero;
                        }

                        //create slide handle
                        float handleSize = 100f;
                        {
                            var handle = new GameObject("Handle");
                            handle.transform.SetParent(slideArea.transform);
                            Image bg = handle.AddComponent<Image>();
                            bg.color = slideColor;
                            {
                                var rt = handle.GetOrAddComponent<RectTransform>();
                                rt.anchorMax = new Vector2(1f, 0f);
                                rt.anchorMin = new Vector2(0f, 0f);
                                rt.pivot = Vector2.one * .5f;
                                rt.sizeDelta = new Vector2(handleSize * .25f, handleSize);
                                rt.anchoredPosition = Vector2.zero;

                                historySlider.handleRect = rt;
                            }
                            historySlider.targetGraphic = bg;
                        }
                    }

                    historySlider.onValueChanged.RemoveListener(UpdateSlider);
                    historySlider.onValueChanged.AddListener(UpdateSlider);

                    if(IsPlaying)
                    {
                        GameObject.DontDestroyOnLoad(slider);
                        HideSlider();
                    }
                    else
                        historySlider.StartCoroutine(SetDontDestroyASAP(slider));
                }
                return historySlider;
            }
            set
            {
                historySlider = value;
            }
        }

        public void Hide()
        {
            LogRoot.SetActive(false);
        }

        public void Show(bool show = true)
        {
            LogRoot.SetActive(show);
        }

        protected void OnEnable()
        {
            SetLogger();
        }

        protected void Setup()
        {
            isPlaying = Application.isPlaying;
            SetLogger();
        }

        public void MoveSliderDown()
        {
            HistorySlider.transform.localPosition = HistorySlider.transform.localPosition - new Vector3(0f, 1000f, 0f);
        }

        public void MoveSliderUp()
        {
            HistorySlider.transform.localPosition = HistorySlider.transform.localPosition + new Vector3(0f, 1000f, 0f);
        }

        public void ShowSlider()
        {
            HistorySlider.gameObject.SetActive(true);
            HistorySlider.maxValue = Mathf.Min(History.Count, historySize);
            historySliderShowing = true;
        }

        public void HideSlider()
        {
            HistorySlider.gameObject.SetActive(false);
            historySliderShowing = false;
        }

        protected bool isPlaying;
        protected bool IsPlaying
        {
            get
            {
                return isPlaying && Dev.Logger != null && Dev.Logger == this;
            }
        }
        
        protected bool canRenderGUILog;
        protected bool CanRenderGUILog
        {
            get
            {
                if(!canRenderGUILog)
                {
                    canRenderGUILog = true;
                    //if(Wms.Game.GameMonoBehavior.Instance != null)
                    //{
                    //    if(Wms.Game.GameMonoBehavior.Instance.gameApp != null)
                    //        canRenderGUILog = true;
                    //}
                }
                return canRenderGUILog;
            }
        }

        void SetLogger()
        {
            Dev.Logger = this;

            Dev.LogCallback -= UnityEngine.Debug.Log;
            //Dev.LogCallback -= LogTrace;
            Dev.LogCallback -= LogGUI;

            Dev.LogCallback += UnityEngine.Debug.Log;
            //Dev.LogCallback += LogTrace;
            Dev.LogCallback += LogGUI;

            if(CaptureDefaultDebugLog)
            {
                Application.logMessageReceived -= HandleApplicationLog;
                Application.logMessageReceived += HandleApplicationLog;
            }

#if CUDLR
            //route cudlr through this logger as well to enable the usage of filters
            if(CUDLR.Console.Instance != null)
            {
                Application.RegisterLogCallback(null);
                Dev.UnformattedLogCallback -= CUDLR.Console.Log;
                Dev.UnformattedLogCallback += CUDLR.Console.Log;                
            }
#endif
        }

        void UpdateSlider(float value)
        {
            GUIHistoryPosition = (int)value;
        }

        void HandleApplicationLog(string logString, string stackTrace, LogType type)
        {
            try
            {
                if(!IsPlaying)
                    return;

                if(IgnoreFilters != null && IgnoreFilters.Count > 0)
                {
                    if(IgnoreFilters.Any(x => logString.Contains(x)))
                        return;
                }

                Application.RegisterLogCallback(null);
                string[] lines = stackTrace.Split(
                               new[] { "\r\n", "\r", "\n" },
                               System.StringSplitOptions.None);
                string header = lines[1];

                if(type != LogType.Warning && !stackTrace.Contains("InvokeLog(String)"))
                {
                    //LogTrace(header + " " + logString);
                    LogGUI(Dev.ToLogString(header, logString));
#if CUDLR
                    CUDLR.Console.Log(logString);
#endif
                }

//                if(BreakOnError && (type == LogType.Exception || type == LogType.Assert || type == LogType.Error))
//                {
//                    Debug.Log("Unfiltered error recieved. Logger set to pause on error.");
//#if UNITY_EDITOR
//                    UnityEditor.EditorApplication.isPaused = true;
//#else
//                    Time.timeScale = 0f;
//#endif
//                }
            }
            catch(System.Exception)
            { }
        }

//        void LogTrace(string s)
//        {
//            if(LoggingEnabled)
//            {
//#if TRACE
//                if(IgnoreFilters != null && IgnoreFilters.Count > 0)
//                {
//                    if(IgnoreFilters.Any(x => s.Contains(x)))
//                        return;
//                }

//                Wms.Framework.Trace.LogDebug(s);
//#endif
//            }
//        }

        PoolableLogString GetGUIString(string logText)
        {
            var pstr = debugLogTextPool.Get<PoolableLogString>(PoolableLogString.ObjectPoolKey, logText, LogWindow.transform);
            var str = pstr.Content;
            str.onSizeChanged -= UpdateLog;
            str.onSizeChanged += UpdateLog;
            pstr.gameObject.SetActive(true);
            str.transform.localScale = Vector3.one;
            str.Content = logText;
            return pstr;
        }

        void FlushLinesOverCount()
        {
            if(Content == null)
                return;

            while(Content.Count > MaxLines)
            {
                PoolableLogString lString = Content.Dequeue();
                debugLogTextPool.EnPool(PoolableLogString.ObjectPoolKey, lString);
            }
        }

        void FlushLinesOverSize()
        {
            if(Content == null)
                return;

            float total_size = Content.Sum(y => y.Size);

            if(total_size <= 0)
                return;

            float max_size = guiRenderAreaSize;
            while(total_size > max_size)
            {
                PoolableLogString lString = Content.Dequeue();
                float objectSize = lString.Size;
                debugLogTextPool.EnPool(PoolableLogString.ObjectPoolKey, lString);
                total_size -= objectSize;
            }
        }

        void UpdatePositions()
        {
            if(Content == null)
                return;

            float sum = 0f;
            foreach(PoolableLogString lstring in Content)
            {
                lstring.Transform.anchoredPosition = new Vector2(0f, -sum);
                sum += lstring.Size;
            }
        }

        void LogGUI(string s)
        {
            if(!IsPlaying && CanRenderGUILog)
                Setup();

            if(!IsPlaying || !CanRenderGUILog)
                return;

            if(IgnoreFilters != null && IgnoreFilters.Count > 0)
            {
                if(IgnoreFilters.Any(x => s.Contains(x)))
                    return;
            }


            if(IsPlaying && GuiLoggingEnabled && LoggingEnabled)
            {
                if(showTimeStampOnGUILogging)
                    s = System.DateTime.Now.ToShortTimeString() + ":" + System.DateTime.Now.Second + ":" + System.DateTime.Now.Millisecond + @" @ " + s;

                History.Enqueue(s);
                if(historySliderShowing)
                    HistorySlider.maxValue = Mathf.Min(History.Count, historySize);
                UpdateContent();
            }
        }

        void UpdateContent()
        {
            int offset = Mathf.Min(MaxLines, Content.Count);
            int i = 0;
            foreach(PoolableLogString lstring in Content)
            {
                int historyIndex = History.Count - 1 - offset - GUIHistoryPosition + i;
                //if(historyIndex < 0)
                    //return;

                string current = History[historyIndex];
                if(string.IsNullOrEmpty(current))
                    current = string.Empty;
                lstring.Content.Content = current;
                lstring.name = current;
                ++i;
            }

            for(; i < MaxLines; ++i)
            {
                int historyIndex = History.Count - 1 - offset - GUIHistoryPosition + i;
                //if(historyIndex < 0)
                    //return;

                string current = History[historyIndex];
                if(!string.IsNullOrEmpty(current))
                    Content.Enqueue(GetGUIString(current));
            }

            UpdateLog();
        }

        void UpdateLog()
        {
            FlushLinesOverCount();
            FlushLinesOverSize();
            UpdatePositions();
        }
    }
}




//Vector2 logWindowSize
//{
//    get
//    {
//        CanvasRenderer canvas = LogWindow.AddComponent<CanvasRenderer>();

//        if(canvas.gameObject.GetComponent<RectTransform>() != null)
//            return canvas.gameObject.GetComponent<RectTransform>().rect.size;

//        return canvas.gameObject.AddComponent<RectTransform>().rect.size;
//    }
//    set
//    {
//        CanvasRenderer canvas = LogWindow.AddComponent<CanvasRenderer>();
//        Rect currentRect;
//        if(canvas.gameObject.GetComponent<RectTransform>() != null)
//            currentRect = canvas.gameObject.GetComponent<RectTransform>().rect;
//        else
//            currentRect = canvas.gameObject.AddComponent<RectTransform>().rect;

//        if(canvas.gameObject.GetComponent<RectTransform>() != null)
//            canvas.gameObject.GetComponent<RectTransform>().sizeDelta = value;
//        else
//            canvas.gameObject.AddComponent<RectTransform>().sizeDelta = value;
//    }
//};