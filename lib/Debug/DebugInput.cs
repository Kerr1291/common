using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace nv
{
    public class DebugInput : GameSingleton<DebugInput>
    {
        public bool runDebugInputOnStart = true;

        public KeyCode suspendKey = KeyCode.Q;
        public KeyCode advanceKey = KeyCode.W;
        public KeyCode fastAdvanceKey = KeyCode.E;
        public KeyCode resumeKey = KeyCode.R;

        public IEnumerator Start()
        {
            if(runDebugInputOnStart)
                yield return RunDebugInput();//will never return...
            yield break;
        }

        int framesAdvanced = 0;
        bool suspended = false;
        bool advance = false;
        bool fastadvance = false;

        public void Suspend()
        {
            framesAdvanced = 0;
            Time.timeScale = 0f;
            suspended = true;
        }

        public void Resume()
        {
            if(!suspended)
                return;
            Time.timeScale = 1f;
            suspended = false;
            advance = false;
            fastadvance = false;
        }

        public void Advance()
        {
            if(!suspended)
                return;
            advance = true;
        }

        public void StartFastAdvance()
        {
            if(!suspended)
                return;
            fastadvance = true;
        }

        public void StopFastAdvance()
        {
            if(!suspended)
                return;
            fastadvance = false;
        }

        //use this static bool to keep the input only running on one component 
        IEnumerator RunDebugInput()
        {
            Debug.Log("Running DebugInput: Controls are.. Q-Suspend W-Advance R-FastAdvance E-Resume");
            for(; ; )
            {
                if(suspended)
                    Time.timeScale = 0f;

                //suspend
                if(UnityEngine.Input.GetKeyDown(suspendKey))
                {
                    Suspend();
                    Debug.Log("Suspending.");
                }

                //advance by about one frame
                if(UnityEngine.Input.GetKeyDown(advanceKey) || advance)
                {
                    Time.timeScale = 1f;
                    yield return new WaitForEndOfFrame();
                    yield return new WaitForEndOfFrame();
                    Time.timeScale = 0f;
                    framesAdvanced++;
                    advance = false;
                }

                //advance by many frames (hold R)
                if( UnityEngine.Input.GetKey(fastAdvanceKey) || fastadvance)
                {
                    Time.timeScale = 1f;
                    yield return new WaitForEndOfFrame();
                    yield return new WaitForEndOfFrame();
                    Time.timeScale = 0f;
                    framesAdvanced++;
                }

                //resume from suspend
                if(suspended && UnityEngine.Input.GetKeyDown(resumeKey))
                {
                    Resume();
                    Debug.Log("Resuming from suspend. Frames advanced: " + framesAdvanced);
                }
                yield return new WaitForEndOfFrame();
            }
            yield break;
        }
    }
}
