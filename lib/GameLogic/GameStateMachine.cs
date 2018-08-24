using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;

namespace nv
{
    public class GameStateMachine : MonoBehaviour
    {
        public SmartRoutine CurrentState { get; protected set; }
        public SmartRoutine NextState { get; protected set; }

        protected object[] nextStateArgs;

        protected virtual void OnEnable()
        {
            SetNextState(Init, nextStateArgs);
            AdvanceToNextState();
        }

        protected virtual void SetNextState(SmartRoutine.UpdateBehaviorFunc nextStateFunc, params object[] args)
        {
            NextState = new SmartRoutine(nextStateFunc);
            nextStateArgs = args;
        }
        
        protected virtual void AdvanceToNextState()
        {
            CurrentState.OnComplete -= AdvanceToNextState;
            CurrentState = NextState;

            if(CurrentState != null)
            {
                CurrentState.OnComplete += AdvanceToNextState;
                CurrentState.Start(nextStateArgs);
            }

            NextState = null;
            nextStateArgs = null;
        }

        protected virtual IEnumerator Init(params object[] args)
        {
            yield break;
        }
    }

    //Example?
    //public class Transition : GameStateMachine
    //{
    //    public float transitionTime = 2f;

    //    protected override IEnumerator Init(params object[] args)
    //    {
    //        yield return base.Init(args);
    //        SetNextState(Run);
    //    }

    //    protected virtual IEnumerator Run(params object[] args)
    //    {
    //        yield return new WaitForSeconds(transitionTime);
    //        SetNextState(Complete);
    //    }

    //    protected virtual IEnumerator Complete(params object[] args)
    //    {
    //        yield break;
    //    }
    //}
}