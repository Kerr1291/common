using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace nv
{
    public abstract class TimedTransition : Transition
    {       
        public enum YieldInstructionType
        {
            WaitForEndOfFrame,
            WaitForFixedUpdate,
            WaitForSeconds
        };

        [Tooltip("Generic events which you may configure to fire during the transition")]
        public List<TransitionUnityEventTrigger> timedEvents;

        [Tooltip("The normal rate of the transition animation")]
        public float updateRateScale = 1f;

        [Tooltip("The rate to update by when the user wants the transition to end quickly")]
        public float finishASAPUpdateRate = 4f;

        [Tooltip("Determines when the internal coroutine that runs the transition will execute")]
        public YieldInstructionType updateInstructionType = YieldInstructionType.WaitForFixedUpdate;
                
        [Serializable]
        public class OnTansitionUpdateAction : UnityEvent<float> { }

        [Tooltip("Optional action to perform during the transiton. It is passed the time as a normalized 0-1 value")]
        public OnTansitionUpdateAction onTransitionUpdateAction;

        //cache for a minor optimization since accessing UnityEngine.Time is slightly costly
        protected float currentFrameTimeScale = 0f;
        protected YieldInstruction updateInstruction;

        protected virtual YieldInstruction UpdateInstruction
        {
            get
            {
                return updateInstruction ?? CreateUpdateInstruction(out updateInstruction);
            }
        }

        public abstract float TotalTime
        {
            get; set;
        }

        public virtual float CurrentTime
        {
            get
            {
                return InternalCurrentTime;
            }
            set
            {
                float previousTime = InternalCurrentTime;
                InternalCurrentTime = Mathf.Clamp(value, 0f, TotalTime);
                if(previousTime <= InternalCurrentTime)
                    InvokeEvents(previousTime, InternalCurrentTime);
                else
                    InvokeEvents(previousTime, 1f);
            }
        }

        public virtual float StartTime
        {
            get; set;
        }

        public virtual float NormalizedCurrentTime
        {
            get
            {
                return CurrentTime / TotalTime;
            }
        }

        protected virtual bool IsComplete
        {
            get
            {
                return CurrentTime >= TotalTime;
            }
        }

        /// <summary>
        /// Use this to convert a time value into one that's normalized relative to this transition
        /// </summary>
        public virtual float GetTimeNormalized(float timeToNormalize, bool clamp = true)
        {
            return Mathf.Clamp01(timeToNormalize / TotalTime);
        }

        public virtual float UpdateRate
        {
            get
            {
                float scale = IsFinishingASAP ? finishASAPUpdateRate : updateRateScale;
                return Time.deltaTime * currentFrameTimeScale * scale;
            }
        }

        protected virtual float InternalCurrentTime
        {
            get; set;
        }

        protected virtual void OnValidate()
        {
            if(timedEvents != null)
            {
                timedEvents.ForEach(t =>
                {
                    if(t != null && t.triggerTimes != null)
                    {
                        if(t.triggerTimes.Count <= 0)
                            t.triggerTimes.Add(new TransitionTime());
                    }
                });
            }
        }

        protected virtual YieldInstruction CreateUpdateInstruction(out YieldInstruction updateInstruction)
        {
            updateInstruction = null;
            if(updateInstructionType == YieldInstructionType.WaitForEndOfFrame)
            {
                updateInstruction = new WaitForEndOfFrame();
            }
            else if(updateInstructionType == YieldInstructionType.WaitForFixedUpdate)
            {
                updateInstruction = new WaitForFixedUpdate();
            }
            else if(updateInstructionType == YieldInstructionType.WaitForSeconds)
            {
                updateInstruction = new WaitForSeconds(TotalTime);
            }
            return updateInstruction;
        }

        /// <summary>
        /// When given two time values, invoke all events between the two times given
        /// </summary>
        protected virtual void InvokeEvents(float from, float to)
        {
            float normalizedFrom = GetTimeNormalized(from);
            float normalizedTo = GetTimeNormalized(to);

            for(int i = 0; i < timedEvents.Count; ++i)
            {
                for(int j = 0; j < timedEvents[i].triggerTimes.Count; ++j)
                {
                    if(normalizedFrom < timedEvents[i].triggerTimes[j])
                        continue;

                    if(normalizedTo < timedEvents[i].triggerTimes[j])
                        break;

                    timedEvents[i].Invoke(this, timedEvents[i].triggerTimes[j]);
                }
            }
        }

        protected override void StartTransitionRunner(bool tryResume = false)
        {
            timedEvents.ForEach(t => t.triggerTimes.Sort());
            if(TransitionRunner == null || !tryResume)
                CurrentTime = StartTime;
            base.StartTransitionRunner(tryResume);
        }

        protected override void OnComplete()
        {
            base.OnComplete();
            CurrentTime = TotalTime;
            if(onTransitionUpdateAction != null)
                onTransitionUpdateAction.Invoke(NormalizedCurrentTime);
        }

        protected override IEnumerator TransitionBehavior()
        {
            while(!IsComplete)
            {
                currentFrameTimeScale = Time.timeScale;
                if(currentFrameTimeScale > 0f)
                {
                    if(onTransitionUpdateAction != null)
                        onTransitionUpdateAction.Invoke(NormalizedCurrentTime);
                    CurrentTime += UpdateRate;
                }
                yield return updateInstruction;
            }
            yield break;
        }
    }
}