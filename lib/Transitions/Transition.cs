using System.Collections;
using UnityEngine;

namespace nv
{
    public abstract class Transition : MonoBehaviour
    {
        public enum OnEnableBehavior
        {
            Play,
            Resume,
            Reset,
            Pause,
            Nothing,
        };

        public enum OnDisableBehavior
        {
            Reset,
            Pause,
            Nothing,
        };

        [Tooltip("Determines the behavior of the transition when the game object is enabled")]
        public OnEnableBehavior onEnableBehavior = OnEnableBehavior.Play;

        [Tooltip("Determines the behavior of the transition when the game object is disabled")]
        public OnDisableBehavior onDisableBehavior = OnDisableBehavior.Reset;

        [SerializeField]
        protected DemoSettings demoSettings;

        [SerializeField, Tooltip("Events for specific common transition moments")]
        protected TransitionEvents transitionEvents;

        protected bool isRunning;
        protected bool isFinishingASAP;

        public virtual TransitionEvents Events
        {
            get
            {
                if(transitionEvents == null)
                    transitionEvents = new TransitionEvents();
                return transitionEvents;
            }
        }

        public virtual bool IsRunning
        {
            get
            {
                return TransitionRunner != null && isRunning;
            }
        }

        public virtual bool IsPaused
        {
            get
            {
                return TransitionRunner != null && !isRunning;
            }
        }

        public virtual bool IsFinishingASAP
        {
            get
            {
                return TransitionRunner != null && isRunning && isFinishingASAP;
            }
            set
            {
                isFinishingASAP = value;
            }
        }

        protected virtual IEnumerator TransitionRunner { get; set; }

        [ContextMenu("Play")]
        public virtual void Play()
        {
            Play(false, false);
        }

        public virtual void Play(bool skip)
        {
            Play(skip, false);
        }

        public virtual void Play(bool skip, bool resume)
        {
            OnPlay();
            if(skip)
                OnComplete();
            else
                StartTransitionRunner(resume);
        }

        [ContextMenu("Pause")]
        public virtual void Pause()
        {
            StopTransitionRunner(false);
            OnPause();
        }

        [ContextMenu("Resume")]
        public virtual void Resume()
        {
            StartTransitionRunner(true);
            OnResume();
        }

        [ContextMenu("Stop")]
        public virtual void Stop()
        {
            Stop(true);
        }

        public virtual void Stop(bool transitionWasCanceled)
        {
            OnStop(transitionWasCanceled);
            StopTransitionRunner(true);
        }

        [ContextMenu("Finish ASAP")]
        public virtual void FinishASAP()
        {
            IsFinishingASAP = true;
            OnFinishASAP();
        }

        protected virtual void OnEnable()
        {
            ProcessStateChangeBehavior(onEnableBehavior);
        }

        protected virtual void OnDisable()
        {
            ProcessStateChangeBehavior(onDisableBehavior);
        }

        protected virtual void ProcessStateChangeBehavior(OnEnableBehavior behavior)
        {
            if(behavior == OnEnableBehavior.Resume)
            {
                Resume();
            }
            else if(behavior == OnEnableBehavior.Reset)
            {
                StopTransitionRunner(true);
            }
            else if(behavior == OnEnableBehavior.Play)
            {
                Play(false, false);
            }
            else if(behavior == OnEnableBehavior.Pause)
            {
                Pause();
            }
            else if(behavior == OnEnableBehavior.Nothing)
            {
                //...
            }
        }
        
        protected virtual void ProcessStateChangeBehavior(OnDisableBehavior behavior)
        {
            if(behavior == OnDisableBehavior.Reset)
            {
                StopTransitionRunner(true);
            }
            else if(behavior == OnDisableBehavior.Pause)
            {
                Pause();
            }
            else if(behavior == OnDisableBehavior.Nothing)
            {
                //...
            }
        }

        protected virtual void OnPlay()
        {
            Events.onPlay.Invoke(this);
        }

        protected virtual void OnPause()
        {
            Events.onPause.Invoke(this);
        }

        protected virtual void OnResume()
        {
            Events.onResume.Invoke(this);
        }

        protected virtual void OnStop(bool transitionWasCanceled)
        {
            Events.onStop.Invoke(this);
        }

        protected virtual void OnFinishASAP()
        {
            Events.onFinishASAP.Invoke(this);
        }

        protected virtual void OnComplete()
        {
            Stop(false);
            Events.onComplete.Invoke(this);
        }

        protected virtual void StartTransitionRunner(bool tryResume = false)
        {
            if(TransitionRunner != null)
            {
                if(tryResume)
                    StartCoroutine(TransitionRunner);
                else
                    StopCoroutine(TransitionRunner);
            }

            TransitionRunner = Run();
            StartCoroutine(TransitionRunner);
            IsFinishingASAP = false;
            isRunning = true;
        }

        protected virtual void StopTransitionRunner(bool clearCoroutine = false)
        {
            if(TransitionRunner != null)
            {
                StopCoroutine(TransitionRunner);
                if(clearCoroutine)
                    TransitionRunner = null;
            }
            isRunning = false;
            isFinishingASAP = false;
        }

        protected virtual IEnumerator Run()
        {
            if(demoSettings.IsDemoPlaceholderEnabled)
                demoSettings.ShowDemoPlaceholder(transform);
            yield return TransitionBehavior();
            if(demoSettings.IsDemoPlaceholderEnabled)
                demoSettings.HideDemoPlaceholder();
            OnComplete();
        }

        /// <summary>
        /// Defines how the transition is processed
        /// </summary>
        protected abstract IEnumerator TransitionBehavior();



        [System.Serializable]
        protected class DemoSettings
        {
            [SerializeField, Tooltip("If enabled, will show the demoPlaceholder object during transition")]
            protected bool useDemoPlaceholder = false;

            [SerializeField, Tooltip("Specifies if the object is a prefab or a scene object")]
            protected bool demoPlaceholderIsPrefab = false;

            [SerializeField, Tooltip("If it's a game object, it will be enabled during a transition; If this is a prefab, it will create an instance and enable/disable that instance during/after transitions.")]
            protected GameObject demoPlaceholder;
            protected GameObject demoPlaceholderInstance;

            public virtual bool IsDemoPlaceholderEnabled
            {
                get
                {
                    return useDemoPlaceholder;
                }
            }

            public virtual void ShowDemoPlaceholder(Transform parent)
            {
                if(demoPlaceholderInstance == null && demoPlaceholder != null)
                {
                    if(demoPlaceholderIsPrefab)
                        demoPlaceholderInstance = (GameObject)Instantiate(demoPlaceholder, parent);
                    else
                        demoPlaceholderInstance = demoPlaceholder;
                }

                if(demoPlaceholderInstance != null)
                    demoPlaceholderInstance.SetActive(true);
            }

            public virtual void HideDemoPlaceholder()
            {
                if(demoPlaceholderInstance != null)
                    demoPlaceholderInstance.SetActive(false);
            }
        }
    }
}