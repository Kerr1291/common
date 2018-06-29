using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System;
using Object = UnityEngine.Object;
using System.Reflection;

namespace nv.Tests
{
#if UNITY_EDITOR
    public class SmartRoutineTests
    {
        public virtual IEnumerator Test()
        {
            float time = .4f;
            yield return new WaitForSeconds(time);
        }

        public virtual IEnumerator WaitFor(params object[] args)
        {
            float time = .4f;
            if(args.Length > 0)
                time = (float)args[0];

            yield return new WaitForSeconds(time);
        }

        public virtual IEnumerator WaitForOneUpdate(params object[] args)
        {
            yield return null;
        }

        int countPos = 0;
        IEnumerator CountToX(params object[] args)
        {
            int x = (int)args[0];
            for(int i = 0; i <= x; ++i)
            {
                countPos = i;
                //Debug.Log("i = " + i);
                yield return new WaitForEndOfFrame();
            }
        }
        
        [UnityTest]
        public IEnumerator CreateAndStart()
        {
            SmartRoutine testObject = new SmartRoutine(WaitFor());
            
            Assert.That(testObject.IsRunning, Is.True, "SmartRoutine did not start.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.EqualTo(1), "SmartRoutine did not start.");
            yield break;
        }

        [UnityTest]
        public IEnumerator CreateAndStartInline()
        {
            SmartRoutine testObject = new SmartRoutine(WaitFor());

            Assert.That(testObject.IsRunning, Is.True, "SmartRoutine did not start.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.EqualTo(1), "SmartRoutine did not start.");
            testObject.Stop();
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not stop.");
            yield break;
        }

        [UnityTest]
        public IEnumerator CreateAndStartAndYieldUntilComplete()
        {
            SmartRoutine testObject = new SmartRoutine(WaitFor());

            yield return testObject;

            Assert.That(testObject.IsRunning, Is.False, "SmartRoutine did not complete.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not complete.");
            yield break;
        }

        [UnityTest]
        public IEnumerator CreateAndStartAndYieldUntilCompleteInline()
        {
            yield return new SmartRoutine(WaitFor());

            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not complete.");
        }

        [UnityTest]
        public IEnumerator CreateAndStartAndYieldUntilCompleteThenStartAgain()
        {
            SmartRoutine testObject = new SmartRoutine(WaitFor);

            yield return testObject;

            testObject.Start();

            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.EqualTo(1), "SmartRoutine did not start the 2nd time.");
            testObject.Stop();
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not stop.");
            yield break;
        }

        [UnityTest]
        public IEnumerator StartAndStopAfterOneFrame()
        {
            SmartRoutine testObject = new SmartRoutine(WaitFor());
            
            yield return new WaitForEndOfFrame();
            testObject.Stop();

            Assert.That(testObject.IsRunning, Is.False, "SmartRoutine did not get stopped.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not complete.");
            yield break;
        }

        [UnityTest]
        public IEnumerator StartAndStopAfterOneFrameThenCallStopAgain()
        {
            SmartRoutine testObject = new SmartRoutine(WaitFor());

            yield return new WaitForEndOfFrame();

            testObject.Stop();
            Assert.That(testObject.IsRunning, Is.False, "SmartRoutine did not get stopped.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not complete.");

            testObject.Stop();

            Assert.That(testObject.IsRunning, Is.False, "SmartRoutine did not get stopped.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not complete.");
            yield break;
        }

        [UnityTest]
        public IEnumerator StartAndRunToCompletionAndSetABoolOnComplete()
        {
            bool test = false;
            SmartRoutine testObject = new SmartRoutine(WaitFor(), () => { test = true; });

            yield return testObject;

            Assert.That(test, Is.True, "Callback did not invoke on completion.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not complete.");
        }

        [UnityTest]
        public IEnumerator StartAndRunToCompletionAndSetABoolOnStopForCompletion()
        {
            bool? test = null;
            SmartRoutine testObject = new SmartRoutine(WaitFor(.5f), (bool x) => { test = x; });

            yield return testObject;

            Assert.That(test, Is.True, "OnStop did not return true for completion.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not complete.");
        }

        [UnityTest]
        public IEnumerator StartAndSetABoolOnStopForIncomplete()
        {
            bool? test = null;
            SmartRoutine testObject = new SmartRoutine(WaitFor(.5f), (bool x) => { test = x; });

            yield return new WaitForEndOfFrame();

            testObject.Stop(false);

            Assert.That(test, Is.False, "OnStop did not return false for incomplete.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine is still running.");
        }

        [UnityTest]
        public IEnumerator StartAndRunToCompletionAndSetABoolOnStart()
        {
            bool test = false;
            SmartRoutine testObject = new SmartRoutine(WaitFor(), () => { }, () => { test = true; });

            yield return testObject;
            
            Assert.That(test, Is.True, "Callback did not invoke on start.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not complete.");
            yield break;
        }

        [UnityTest]
        public IEnumerator StartAndRunToCompletionAndSubscribeToOnCompleteInOnStart()
        {
            bool test = false;
            SmartRoutine testObject = new SmartRoutine(WaitFor);
            testObject.OnStart = () => { testObject.OnComplete = () => { test = true; }; };


            yield return testObject;
            
            Assert.That(test, Is.True, "Callback did not invoke on completion.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not complete.");
            yield break;
        }

        [UnityTest]
        public IEnumerator StartAndRunToCompletionAndSubscribeToOnStartAndOnComplete()
        {
            bool startTest = false;
            bool completeTest = false;

            SmartRoutine testObject = new SmartRoutine(WaitFor);
            testObject.OnStart = () => { startTest = true; };
            testObject.OnComplete = () => { completeTest = true; };

            yield return testObject;

            Assert.That(startTest, Is.True, "Callback did not invoke on OnStart.");
            Assert.That(completeTest, Is.True, "Callback did not invoke on OnComplete.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not complete.");
            yield break;
        }

        [UnityTest]
        public IEnumerator StartAndRunToCompletionAndSubscribeToOnStartAndOnStop()
        {
            bool stopTest = false;
            bool startTest = false;

            SmartRoutine testObject = new SmartRoutine(WaitFor);
            testObject.OnStart = () => { startTest = true; };
            testObject.OnStop = (bool x) => { stopTest = x; };

            yield return testObject;

            Assert.That(startTest, Is.True, "Callback did not invoke on OnStart.");
            Assert.That(stopTest, Is.True, "Callback did not invoke on completion through OnStop.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not complete.");
            yield break;
        }

        [UnityTest]
        public IEnumerator StartAndRunToCompletionAndSubscribeToOnStartAndOnCompleteAndOnStop()
        {
            bool stopTest = false;
            bool startTest = false;
            bool completeTest = false;

            SmartRoutine testObject = new SmartRoutine(WaitFor);
            testObject.OnStart = () => { startTest = true; };
            testObject.OnComplete = () => { completeTest = true; };
            testObject.OnStop = (bool x) => { stopTest = x; };

            yield return testObject;

            Assert.That(startTest, Is.True, "Callback did not invoke on OnStart.");
            Assert.That(stopTest, Is.True, "Callback did not invoke on completion through OnStop.");
            Assert.That(completeTest, Is.True, "Callback did not invoke on OnComplete.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not complete.");
            yield break;
        }

        [UnityTest]
        public IEnumerator StartAndCountTo100()
        {
            countPos = 0;
            SmartRoutine testObject = new SmartRoutine(CountToX(100));

            yield return testObject;

            Assert.That(countPos, Is.EqualTo(100), "Failed to count to 100.");
            Assert.That(testObject.IsRunning, Is.False, "SmartRoutine failed to stop.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not complete.");
            yield break;
        }

        [UnityTest]
        public IEnumerator StartAndCountTo100AndPauseAndResumeAt50()
        {
            countPos = 0;
            SmartRoutine testObject = new SmartRoutine(CountToX(100));
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.EqualTo(1), "1 SmartRoutine should be active.");

            while(countPos != 50)
            {
                Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.EqualTo(1), "1 SmartRoutine should be active.");
                yield return new WaitForEndOfFrame();
            }

            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.EqualTo(1), "1 SmartRoutine should be active.");
            testObject.Stop(false);

            Assert.That(testObject.IsPaused, Is.True, "SmartRoutine failed to pause.");
            Assert.That(testObject.IsRunning, Is.True, "SmartRoutine stopped when it should be paused.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.EqualTo(0), "No SmartRoutine should be active while paused.");

            yield return testObject;

            Assert.That(countPos, Is.EqualTo(100), "Failed to count to 100.");
            Assert.That(testObject.IsRunning, Is.False, "SmartRoutine failed to stop.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not complete.");
            yield break;
        }

        [UnityTest]
        public IEnumerator StartAndRunToCompletionAndWaitFor1SecondWithArgs()
        {
            SmartRoutine testObject = new SmartRoutine(WaitFor);
                       
            yield return testObject.Start(1f);
            
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.AtMost(0), "SmartRoutine did not complete.");
            yield break;
        }
    }
#endif
}