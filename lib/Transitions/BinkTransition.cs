//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Playables;
//using UnityEngine.Timeline;
//using System.Linq;
//using UnityEngine.Events;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//namespace nv
//{
//    public class BinkTransition : Transition
//    {
//        [HideInInspector]
//        protected Bink binkController;

//        [Tooltip("Generic events which you may configure to fire during the transition")]
//        public List<TransitionUnityEventTrigger> timedEvents;

//        [SerializeField]
//        protected int frameRate = 30;

//        [SerializeField]
//        protected bool stopOnComplete = true;

//        [System.Serializable]
//        public class OnTansitionUpdateAction : UnityEvent<float> { }

//        [Tooltip("Optional action to perform during the transiton. It is passed the time as a normalized 0-1 value")]
//        public OnTansitionUpdateAction onTransitionUpdateAction;

//        protected float currentFrameTimeScale = 0f;

//        protected bool isComplete;

//        public Bink BinkController
//        {
//            get
//            {
//                if(binkController == null)
//                {
//                    binkController = GetComponent<Bink>();
//                }

//                return binkController;
//            }
//        }     

//        public override bool IsRunning
//        {
//            get
//            {
//                return base.IsRunning && isActiveAndEnabled && BinkController.IsPlaying;
//            }
//        }

//        public override bool IsPaused
//        {
//            get
//            {
//                return base.IsPaused && BinkController.IsPaused;
//            }
//        }

//        public virtual bool IsComplete
//        {
//            get
//            {
//                return isComplete;
//            }
//        }

//        protected float time = 0f;

//        public virtual float TotalTime
//        {
//            get
//            {
//                return BinkController.GetTotalTime(frameRate);
//            }
//        }

//        /// <summary>
//        /// Use this to convert a time value into one that's normalized relative to this transition
//        /// </summary>
//        public virtual float GetTimeNormalized(float timeToNormalize, bool clamp = true)
//        {
//            return Mathf.Clamp01(timeToNormalize / TotalTime);
//        }

//        protected virtual void OnValidate()
//        {
//            if(timedEvents != null)
//            {
//                timedEvents.ForEach(t =>
//                {
//                    if(t != null && t.triggerTimes != null)
//                    {
//                        if(t.triggerTimes.Count <= 0)
//                            t.triggerTimes.Add(new TransitionTime());
//                    }
//                });
//            }
//        }

//        protected virtual void Awake()
//        {
//            if(BinkController == null)
//                throw new System.NullReferenceException("GDK Bink monobehaviour must be attached to this component!");

//            if(BinkController.LoopMovie)
//            {
//                BinkController.AddCallbackOnComplete(() => { time = 0f; });
//            }
//            else
//            {
//                BinkController.AddCallbackOnComplete(() => { isComplete = true; });
//            }
//        }

//        protected override void StartTransitionRunner(bool tryResume = false)
//        {
//            timedEvents.ForEach(t => t.triggerTimes.Sort());
//            isComplete = false;
//            if(TransitionRunner == null || !tryResume)
//                time = 0f;

//            base.StartTransitionRunner(tryResume);

//            if(tryResume && BinkController.IsPaused)
//            {
//                BinkController.Pause(false);
//            }
//            else
//            {
//                BinkController.Play(false);
//                //BinkController.PlayFromDisable();
//            }
//        }

//        protected override void StopTransitionRunner(bool clearCoroutine = false)
//        {
//            base.StopTransitionRunner(clearCoroutine);
//            if(clearCoroutine)
//            {
//                if(BinkController.IsPlaying && BinkController.LoopMovie)
//                {
//                    BinkController.Stop();
//                    isComplete = true;
//                    time = TotalTime;
//                    if(demoSettings.IsDemoPlaceholderEnabled)
//                        demoSettings.HideDemoPlaceholder();
//                    OnComplete();
//                }
//                else
//                {
//                    if(stopOnComplete)
//                        BinkController.Stop();
//                }
//            }
//            else
//            {
//                BinkController.Pause(true);
//            }
//        }

//        protected override void OnComplete()
//        {
//            base.OnComplete();
//            time = TotalTime;
//            if(onTransitionUpdateAction != null)
//                onTransitionUpdateAction.Invoke(1f);
//            if(stopOnComplete)
//                BinkController.Stop();
//        }

//        protected override IEnumerator TransitionBehavior()
//        {
//            while(!IsComplete)
//            {
//                currentFrameTimeScale = Time.timeScale;
//                if(currentFrameTimeScale > 0f)
//                {
//                    if(onTransitionUpdateAction != null)
//                        onTransitionUpdateAction.Invoke(time / BinkController.GetTotalTime());

//                    float previousTime = time;
//                    time += 1f / frameRate;
//                    time = Mathf.Clamp(time, 0f, TotalTime);
//                    if(previousTime <= time)
//                        InvokeEvents(previousTime, time);
//                    else
//                        InvokeEvents(previousTime, 1f);
//                }
//                yield return new WaitForSeconds(1f / frameRate);
//            }
//            yield break;
//        }

//        public virtual void SetAlpha(float alpha)
//        {
//            BinkController.SetAlpha(alpha);
//        }

//        public virtual void SetAlphaInverse(float alpha)
//        {
//            BinkController.SetAlpha(1f - alpha);
//        }

//        /// <summary>
//        /// When given two time values, invoke all events between the two times given
//        /// </summary>
//        protected virtual void InvokeEvents(float from, float to)
//        {
//            float normalizedFrom = GetTimeNormalized(from);
//            float normalizedTo = GetTimeNormalized(to);

//            for(int i = 0; i < timedEvents.Count; ++i)
//            {
//                for(int j = 0; j < timedEvents[i].triggerTimes.Count; ++j)
//                {
//                    if(normalizedFrom < timedEvents[i].triggerTimes[j])
//                        continue;

//                    if(normalizedTo < timedEvents[i].triggerTimes[j])
//                        break;

//                    timedEvents[i].Invoke(this, timedEvents[i].triggerTimes[j]);
//                }
//            }
//        }
//    }
//}