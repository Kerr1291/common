using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace nv
{
    public class GameActionManager : GameSingleton<GameActionManager>
    {
        Dictionary<GameAction,IEnumerator> actions = new Dictionary<GameAction,IEnumerator>();

        public List<GameAction> RunningActions {
            get {
                return new List<GameAction>( actions.Keys );
            }
        }

        public void StartAction( GameAction action, IEnumerator actionMain )
        {
            if( actions.ContainsKey( action ) )
                return;
            actions.Add( action, actionMain );
            StartCoroutine( actionMain );
        }

        public void StopAction( GameAction action )
        {
            if( !actions.ContainsKey( action ) )
                return;
            StopCoroutine( actions[ action ] );
            actions.Remove( action );
        }

        public bool IsRunning( GameAction action )
        {
            return ( actions.ContainsKey( action ) );
        }
    }

    public class GameAction
    {
        public enum UpdateRateType
        {
            Fixed
            , Frame
            , Custom
        }

        UpdateRateType updateRate;
        YieldInstruction yieldUpdate;
        float lerpRate;

        public virtual System.Action OnStart { get; set; }
        public virtual System.Action OnUpdate { get; set; }
        public virtual System.Action<float> OnLerp { get; set; }
        public virtual System.Action OnStop { get; set; }
        public virtual System.Action OnLoop { get; set; }
        public virtual bool Looping { get; set; }

        public virtual float Period { get; set; }

        public virtual float Time { get; private set; }

        public virtual float NormalizedTime {
            get {
                return Time / Period;
            }
        }

        public virtual float TimeRemaining {
            get {
                return Period - Time;
            }
        }

        public virtual float NormalizedTimeRemaining {
            get {
                return TimeRemaining / Period;
            }
        }

        public virtual bool IsRunning {
            get {
                return GameActionManager.Instance.IsRunning( this );
            }
        }

        public void SetUpdateRate( UpdateRateType type )
        {
            updateRate = type;
            if( updateRate == UpdateRateType.Fixed )
            {
                yieldUpdate = new WaitForFixedUpdate();
                lerpRate = UnityEngine.Time.fixedDeltaTime;
            }
            else if( updateRate == UpdateRateType.Frame )
            {
                yieldUpdate = new WaitForEndOfFrame();
                lerpRate = UnityEngine.Time.deltaTime;
            }
            else
            {
                SetUpdateRate( .1f );
            }
        }

        public void SetUpdateRate( float customRate )
        {
            lerpRate = customRate;
            updateRate = UpdateRateType.Custom;
            yieldUpdate = new WaitForSeconds( customRate );
        }

        public void Start()
        {
            if( Time > 0f )
                return;
            if( IsRunning )
                return;
            InvokeAction( OnStart );
            GameActionManager.Instance.StartAction( this, Main() );
        }

        public void Stop()
        {
            if( IsRunning )
                InvokeAction( OnStop );
            Reset();
        }

        public void Reset()
        {
            GameActionManager.Instance.StopAction( this );
            Time = 0f;
        }

        IEnumerator Main()
        {
            do
            {
                Time = 0f;

                while( Time < Period )
                {
                    InvokeAction( OnUpdate );
                    InvokeAction( OnLerp, NormalizedTime );
                    Time += lerpRate;
                    yield return yieldUpdate;
                }

                Time = Period;
                InvokeAction( OnUpdate );
                InvokeAction( OnLerp, NormalizedTime );

                if( Looping )
                    InvokeAction( OnLoop );
            }
            while( Looping );
            Stop();
        }

        public GameAction()
        {
            updateRate = UpdateRateType.Fixed;
            yieldUpdate = new WaitForFixedUpdate();
            lerpRate = .02f;
            Period = 1f;
        }

        public GameAction( System.Action onStop, float period = 1f )
        {
            updateRate = UpdateRateType.Fixed;
            yieldUpdate = new WaitForFixedUpdate();
            lerpRate = .02f;

            OnStop = onStop;
            Period = period;
        }

        static void InvokeAction( System.Action action )
        {
            if( action == null )
                return;
            action.Invoke();
        }

        static void InvokeAction( System.Action<float> action, float t )
        {
            if( action == null )
                return;
            action.Invoke( t );
        }
    }
}