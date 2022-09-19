//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Playables;
//using UnityEngine.Timeline;
//using System.Linq;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//namespace nv
//{
//    [RequireComponent(typeof(Animator))]
//    [RequireComponent(typeof(PlayableDirector))]
//    public class TimelineTransition : TimedTransition
//    {
//        [SerializeField, HideInInspector]
//        protected PlayableDirector director;
//        [SerializeField, HideInInspector]
//        protected Animator animator;
//        [SerializeField, HideInInspector]
//        protected TimelineAsset timelineAsset;

//        public PlayableDirector Director
//        {
//            get
//            {
//                if(director == null)
//                {
//                    director = GetComponent<PlayableDirector>();
//                    director.playOnAwake = false;
//                    director.timeUpdateMode = DirectorUpdateMode.Manual;
//                    director.extrapolationMode = DirectorWrapMode.None;
//                }
//                else
//                {
//                    if(director.timeUpdateMode != DirectorUpdateMode.Manual)
//                        Debug.Log("TimelineTransition requires the update mode to be manual! If a different update rate is required, change it through the TimedTransition script!");

//                    director.timeUpdateMode = DirectorUpdateMode.Manual;
//                }

//                return director;
//            }
//        }

//        public Animator Animator
//        {
//            get
//            {
//                return animator ?? (animator = GetComponent<Animator>());
//            }
//        }

//        public override float TotalTime
//        {
//            get
//            {
//                return IsFixedTimeMode ? (float)TLAsset.fixedDuration : (float)Director.duration;                
//            }
//            set
//            {
//                IsFixedTimeMode = true;
//                TLAsset.fixedDuration = value;
//            }
//        }

//        public override float StartTime
//        {
//            get
//            {
//                return (float)Director.initialTime;
//            }

//            set
//            {
//                Director.initialTime = value;
//            }
//        }

//        public virtual string TimelineName
//        {
//            get
//            {
//                return TLAsset == null ? string.Empty : TLAsset.name;
//            }
//        }

//        protected override float InternalCurrentTime
//        {
//            get
//            {
//                return (float)director.time;
//            }
//            set
//            {
//                Director.time = value;
//                Director.Evaluate();
//            }
//        }

//        protected virtual TimelineAsset TLAsset
//        {
//            get
//            {
//                if(timelineAsset == null)
//                {
//                    timelineAsset = Director.playableAsset as TimelineAsset;
//                    if(timelineAsset != null)
//                        timelineAsset.durationMode = TimelineAsset.DurationMode.FixedLength;
//                }
//                return timelineAsset;
//            }
//        }

//        public virtual bool IsFixedTimeMode
//        {
//            get
//            {
//                return (TLAsset != null && TLAsset.durationMode == TimelineAsset.DurationMode.FixedLength);
//            }
//            set
//            {
//                if(TLAsset != null)
//                {
//                    TLAsset.durationMode = (value ? TimelineAsset.DurationMode.FixedLength : TimelineAsset.DurationMode.BasedOnClips);
//                }
//            }
//        }

//        public override bool IsRunning
//        {
//            get
//            {
//                return base.IsRunning || Director.state == PlayState.Playing;
//            }
//        }

//        public override bool IsPaused
//        {
//            get
//            {
//                return base.IsPaused || Director.state == PlayState.Paused;
//            }
//        }

//        public override void Play(bool skip = false, bool resume = false)
//        {
//            base.Play(skip, resume);
//            Director.Play();
//        }

//        public override void Pause()
//        {
//            base.Pause();
//            Director.Pause();
//        }

//        public override void Resume()
//        {
//            base.Resume();
//            Director.Resume();
//        }

//        protected override void OnComplete()
//        {
//            base.OnComplete();
//            Director.Evaluate();
//        }

//        public override void Stop(bool transitionWasCanceled)
//        {
//            base.Stop(transitionWasCanceled);
//            Director.Stop();
//        }

//        [ContextMenu("Create Timeline Asset")]
//        public void CreateTimelineAsset()
//        {
//#if UNITY_EDITOR
//            Director.playableAsset = new TimelineAsset();
//            AssetDatabase.CreateAsset(Director.playableAsset, "Assets/Resources/" + name + ".playable");
//            AssetDatabase.SaveAssets();
//            AssetDatabase.Refresh();
//            AssetDatabase.ImportAsset("Assets/Resources/" + name + ".playable", ImportAssetOptions.ForceUpdate);
//            EditorGUIUtility.PingObject(Director.playableAsset);
//#endif
//        }

//        public void CreateTimelineAsset(string assetName)
//        {
//#if UNITY_EDITOR
//            Director.playableAsset = new TimelineAsset();
//            AssetDatabase.CreateAsset(Director.playableAsset, "Assets/Resources/"+ assetName + ".playable");
//            AssetDatabase.SaveAssets();
//            AssetDatabase.Refresh();
//            AssetDatabase.ImportAsset("Assets/Resources/" + assetName + ".playable", ImportAssetOptions.ForceUpdate);
//            EditorGUIUtility.PingObject(Director.playableAsset);
//#endif
//        }
//    }
//}