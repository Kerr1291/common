using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using nv;
using System;
using System.Reflection;

namespace Components.Common
{
    public interface ITransition
    {
        bool Running { get; }
        
        void Start(MonoBehaviour owner, Func<MonoBehaviour,IEnumerator> updateTransition);

        void Stop(bool isComplete);

        void CompleteAsSoonAsPossible();

        Action<MonoBehaviour> OnStart { get; set; }

        Action<MonoBehaviour> OnComplete { get; set; }
    }

    public class Transition : ITransition
    {
        IEnumerator watchUpdateTransition;
        MonoBehaviour transitionOwner;
            
        public virtual bool Running
        {
            get
            {
                return UpdateTransition != null;
            }
        }

        public virtual void Start(MonoBehaviour owner, Func<MonoBehaviour,IEnumerator> updateTransition)
        {
            transitionOwner = owner;
            
            if(OnStart != null)
                OnStart(transitionOwner);

            UpdateTransition = updateTransition(transitionOwner);
            watchUpdateTransition = WatchUpdateTransition();

            transitionOwner.StartCoroutine(UpdateTransition);
            transitionOwner.StartCoroutine(watchUpdateTransition);
        }

        public virtual void Start(MonoBehaviour owner, Func<MonoBehaviour, IEnumerator> updateTransition, Action<MonoBehaviour> onStart = null, Action<MonoBehaviour> onComplete = null)
        {
            OnStart -= onStart;
            OnStart += onStart;

            OnComplete -= onComplete;
            OnComplete += onComplete;

            Start(owner, updateTransition);
        }

        public virtual void Stop(bool isComplete = false)
        {
            transitionOwner.StopCoroutine(UpdateTransition);
            CompleteTransition(isComplete);
        }

        public virtual void CompleteAsSoonAsPossible()
        {
            Stop(true);
        }

        protected virtual void CompleteTransition(bool isComplete)
        {
            UpdateTransition = null;
            if(isComplete && OnComplete != null)
                OnComplete(transitionOwner);
            transitionOwner = null;
        }

        protected virtual IEnumerator UpdateTransition { get; set; }

        public virtual Action<MonoBehaviour> OnStart { get; set; }

        protected Action<MonoBehaviour> onComplete;
        public virtual Action<MonoBehaviour> OnComplete
        {
            get
            {
                return onComplete;
            }
            set
            {
                if(Running)
                {
                    throw new InvalidOperationException("Cannot change the completion callback while the transition is running!");
                }
                else
                {
                    onComplete = value;
                }
            }
        }

        IEnumerator WatchUpdateTransition()
        {
            for(;;)
            {
                yield return null;
                if(IsDone())
                    break;
            }
            CompleteTransition(true);
        }

        bool IsDone()
        {
            bool result = false;
            try
            {
                if(UpdateTransition != null)
                {
                    //use reflection to check if the internal iterator is past the end
                    var isDoneField = UpdateTransition.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)[2];
                    bool isDone = (int)isDoneField.GetValue(UpdateTransition) == -1;

                    if(isDone)
                        result = true;
                }
                else
                {
                    result = true;
                }
            }
            catch(Exception e)
            {
                result = true;
                Debug.LogError(e.Message);
            }
            return result;
        }
    }
}