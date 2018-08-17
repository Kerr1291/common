using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace nv
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioClipSet : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private float volume = 1.0f;

        public virtual float Volume
        {
            get
            {
                return volume;
            }
            set
            {
                volume = Mathf.Max(value, 0.0f);
            }
        }

        [SerializeField, HideInInspector]
        AudioSource source;

        /// <summary>
        /// Get the attached audiosource component.
        /// </summary>
        public AudioSource Source
        {
            get
            {
                if(source == null)
                    source = GetComponent<AudioSource>();

                return source;
            }
        }
        
        [Reorderable]
        public List<AudioClip> clips;

        [SerializeField, HideInInspector]
        protected int currentIndex = 0;

        protected int previousIndex = -1;

        public float NormalizedTime
        {
            get
            {
                return Source.time / CurrentClip.length;
            }
            set
            {
                Source.time = value * CurrentClip.length;
            }
        }

        public virtual AudioClip CurrentClip
        {
            get
            {
                if(clips == null)
                    return null;

                if(clips.Count <= 0)
                    return null;

                currentIndex = currentIndex % clips.Count;
                if(currentIndex >= clips.Count)
                    return null;

                return clips[currentIndex];
            }
        }

        public virtual int CurrentIndex
        {
            get
            {
                return currentIndex;
            }
            set
            {
                if(currentIndex != value && currentIndex >= 0)
                    Source.Stop();
                previousIndex = currentIndex;
                if(clips == null || clips.Count <= 0)
                {
                    currentIndex = 0;
                    return;
                }
                currentIndex = value % clips.Count;
                Source.clip = clips[currentIndex];
            }
        }

        public virtual int PreviousIndex
        {
            get
            {
                return previousIndex;
            }
        }

        public virtual AudioClip PreviousClip
        {
            get
            {
                if(clips.Count <= 0)
                    return null;

                previousIndex = previousIndex % clips.Count;
                if(previousIndex >= clips.Count)
                    return null;

                return clips[previousIndex];
            }
        }

        [SerializeField, HideInInspector]
        protected List<float> savedClipTimes;

        public virtual float SavedClipTime
        {
            get
            {
                return savedClipTimes[currentIndex];
            }
            protected set
            {
                savedClipTimes[currentIndex] = value;
            }
        }

        /// <summary>
        /// By calling this Play() method the user may resume from the saved time
        /// </summary>
        public virtual void Play(bool resume = false)
        {
            Stop();
            if(resume == true)
            {
                SavedClipTime = SavedClipTime >= Source.clip.length ? 0f : SavedClipTime;
                Source.time = SavedClipTime;
            }
            else
            {
                SavedClipTime = 0.0f;
                Source.time = 0f;
            }

            Source.Play();
        }

        /// <summary>
        /// By calling this Play() method the user may start the clip at a different time
        /// </summary>
        public virtual void Play(float startTime)
        {
            Stop();
            SavedClipTime = startTime;
            Play(true);
        }

        public virtual void Play(bool resume, int index)
        {
            Stop();

            CurrentIndex = index;
            
            Play(resume);
        }

        public virtual void PlayRandom(bool resume = false, bool canPlayPreviousClip = true)
        {
            int index = UnityEngine.Random.Range(0, clips.Count);

            if(!canPlayPreviousClip)
            {
                //get an index we haven't used
                while(clips.Count > 1 && clips[index] == PreviousClip)
                    index = UnityEngine.Random.Range(0, clips.Count);
            }

            Play(resume, index);
        }

        protected virtual void OnDisable()
        {
            Stop();
        }

        /// <summary>
        /// Stop the sound right now.
        /// If the audio source is playing, the time will be saved off and may be used on next play.
        /// </summary>
        public virtual void Stop()
        {
            SavedClipTime = Source.time;
            Source.Stop();
        }

        protected virtual void Reset()
        {
            source = GetComponent<AudioSource>();
#if UNITY_EDITOR
            UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
#endif
            Source.volume = 0.0f;
        }

        protected virtual void OnValidate()
        {
            if(source == null)
                source = GetComponent<AudioSource>();

            //keep the current clip and the unity source clips in sync
            if(CurrentClip != Source.clip)
            {
                if(CurrentClip == null)
                {
                    if(clips.Count <= 0)
                        clips.Add(Source.clip);
                    else
                        clips[currentIndex] = Source.clip;
                }
                else
                {
                    Source.clip = CurrentClip;
                }
            }

            if(savedClipTimes == null || savedClipTimes.Count != clips.Count)
            {
                if(clips == null)
                    clips = new List<AudioClip>();
                savedClipTimes = new List<float>();
                for(int i = 0; i < clips.Count; ++i)
                {
                    savedClipTimes.Add(0f);
                }
            }
        }        
    }
}





//#if UNITY_EDITOR
//using UnityEditor;
//using System.Reflection;
//namespace nv.editor
//{
//    [CanEditMultipleObjects]
//    [CustomEditor(typeof(AudioClipSet))]
//    public class AudioClipSet_Editor : Editor
//    {
//        public override void OnInspectorGUI()
//        {
//            AudioClipSet localTarget = target as AudioClipSet;

//            localTarget = (AudioClipSet)target;
//            EditorGUILayout.BeginHorizontal();
//            if(GUILayout.Button("Play"))
//            {
//                if(Application.isPlaying)
//                    localTarget.Play();
//                else
//                {
//                    StopAllClips();
//                    PlayClip(localTarget.Source.clip);
//                }
//            }

//            string stopButtonText = "";
//            if(Application.isPlaying)
//                stopButtonText = "Stop";
//            else
//                stopButtonText = "Stop All Clips";

//            if(GUILayout.Button(stopButtonText))
//            {
//                if(Application.isPlaying)
//                    localTarget.Stop();
//                else
//                    StopAllClips();
//            }
//            EditorGUILayout.EndHorizontal();

//            if(Application.isPlaying)
//            {
//                EditorGUI.BeginDisabledGroup(true);
//                EditorGUILayout.Toggle("IsPlaying?", localTarget.Source.isPlaying);
//                EditorGUI.EndDisabledGroup();
//            }

//            if(localTarget != null)
//            {
//                localTarget.CurrentIndex = EditorGUILayout.DelayedIntField("Current Index",localTarget.CurrentIndex);
//            }
//            base.OnInspectorGUI();
//        }
//        ////////
//        ///The code snippets below allow for sounds to be played in editor mode.
//        ///Code from: https://forum.unity3d.com/threads/way-to-play-audio-in-editor-using-an-editor-script.132042/
//        ///

//        public static void PlayClip(AudioClip clip)
//        {
//            if(clip == null)
//                return;

//            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
//            System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
//            MethodInfo method = audioUtilClass.GetMethod(
//                "PlayClip",
//                BindingFlags.Static | BindingFlags.Public,
//                null,
//                new System.Type[] {
//                typeof(AudioClip)
//            },
//                null
//            );

//            method.Invoke(null, new object[] { clip });
//        }

//        public static void StopAllClips()
//        {
//            Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
//            System.Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
//            MethodInfo method = audioUtilClass.GetMethod(
//                "StopAllClips",
//                BindingFlags.Static | BindingFlags.Public,
//                null,
//                new System.Type[] { },
//                null
//            );

//            method.Invoke(null, new object[] { });
//        }
//        ///
//        ///End imported code.
//        ////////
//    }

//}
//#endif