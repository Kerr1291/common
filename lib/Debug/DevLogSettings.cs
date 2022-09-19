#if LOGLIB
using UnityEngine;
using System.Collections;
namespace nv
{
    public class DevLogSettings : ScriptableObject
    {
        public bool showFileAndLineNumber = true;
        public bool showMethodParameters = false;
        public bool showClassName = true;
        public bool loggingEnabled = true;
        public bool guiLoggingEnabled = false;
        public bool colorizeText = true;
        public Color logColor = Color.white;
        public Color logWarningColor = Color.yellow;
        public Color logErrorColor = Color.red;
        public Color methodColor = Color.cyan;
        public Color paramColor = Color.green;
    }
}
#endif