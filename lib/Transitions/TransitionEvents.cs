using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace nv
{
    [Serializable]
    public class TransitionEvents
    {
        public TransitionEvent onPlay;
        public TransitionEvent onPause;
        public TransitionEvent onResume;
        public TransitionEvent onStop;
        public TransitionEvent onComplete;
        public TransitionEvent onFinishASAP;
    }

    [Serializable]
    public struct TransitionTime : IComparable<TransitionTime>
    {
        [SerializeField, Range(0f, 1f), Tooltip("The normalized time along the transition in which to trigger this event")]
        float triggerTime;
        public float TriggerTime
        {
            get
            {
                return triggerTime;
            }
            set
            {
                triggerTime = Mathf.Clamp01(value);
            }
        }

        public int CompareTo(TransitionTime other)
        {
            return TriggerTime.CompareTo(other.TriggerTime);
        }

        public static implicit operator float(TransitionTime t)
        {
            return t.TriggerTime;
        }
    }

    [Serializable]
    public abstract class TransitionEventTrigger
    {
        public string eventName = "New Event";

        [Tooltip("The normalized time(s) along the transition in which to trigger the event")]
        public List<TransitionTime> triggerTimes;

        public abstract void Invoke(Transition owner, float triggerTime);

        public TransitionEventTrigger()
        {
            triggerTimes = new List<TransitionTime>();
            triggerTimes.Add(new TransitionTime());
        }
    }

    [Serializable]
    public class TransitionUnityEventTrigger : TransitionEventTrigger
    {
        public override void Invoke(Transition owner, float triggerTime)
        {
            triggerEventWithTransition.Invoke(owner);
            triggerEventWithTime.Invoke(triggerTime);
        }

        [Tooltip("Use this to trigger custom behaviors")]
        public TransitionEvent triggerEventWithTransition;
        [Tooltip("Use this to trigger custom behaviors; Will pass the event time in as a parameter")]
        public TransitionTimeEvent triggerEventWithTime;

        public TransitionUnityEventTrigger()
            : base() { }
    }

    [Serializable] public class TransitionEvent : UnityEvent<Transition> { }
    [Serializable] public class TransitionTimeEvent : UnityEvent<float> { }
}
