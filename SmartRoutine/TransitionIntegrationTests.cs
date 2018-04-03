using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System;
using Object = UnityEngine.Object;
using System.Reflection;

namespace Components.Common.Tests
{
#if UNITY_EDITOR
    public class TestGameTransition : MonoBehaviour, IMonoBehaviourTest
    {
        public static Func<MonoBehaviour, IEnumerator> UpdateFunctionForTest;
        public static Action<MonoBehaviour> OnStartFunctionForTest;
        public static Action<MonoBehaviour> OnCompleteFunctionForTest;

        public Transition TransitionTest { get; private set; }

        //how long until this test should fail
        float timeout = 2f;

        void Awake()
        {
            TransitionTest = new Transition();
            StartSimpleTest();
        }

        public void StartSimpleTest()
        {
            TransitionTest.Start(this, UpdateFunctionForTest, OnStartFunctionForTest, OnCompleteFunctionForTest);
        }

        //required by the unity "MonoBehaviourTest" framework 
        public bool IsTestFinished
        {
            get { return timeout <= 0f || !TransitionTest.Running; }
        }

        //(optional) for the unity "MonoBehaviourTest" framework 
        void Update()
        {
            timeout -= Time.deltaTime;
        }
    }

    public class TransitionIntegrationTests
    {
        public virtual IEnumerator WaitForOneSecond(MonoBehaviour owner)
        {
            yield return new WaitForSeconds(1f);
        }

        public virtual IEnumerator WaitForOneUpdate(MonoBehaviour owner)
        {
            yield return null;
        }

        void ClearPreviousTestCallbacks()
        {
            TestGameTransition.UpdateFunctionForTest = null;
            TestGameTransition.OnStartFunctionForTest = null;
            TestGameTransition.OnCompleteFunctionForTest = null;
        }

        [UnityTest]
        public IEnumerator StartGameTransition()
        {
            ClearPreviousTestCallbacks();
            TestGameTransition.UpdateFunctionForTest = WaitForOneSecond;
            new MonoBehaviourTest<TestGameTransition>();
            TestGameTransition behaviorTest = Object.FindObjectOfType<TestGameTransition>();
            Assert.That(behaviorTest.TransitionTest.Running, Is.True, "Transition did not start.");
            yield break;
        }
        
        [UnityTest]
        public IEnumerator StartGameTransitionAndRunToCompletion()
        {
            ClearPreviousTestCallbacks();
            TestGameTransition.UpdateFunctionForTest = WaitForOneSecond;
            yield return new MonoBehaviourTest<TestGameTransition>();
            TestGameTransition behaviorTest = Object.FindObjectOfType<TestGameTransition>();
            Assert.That(behaviorTest.TransitionTest.Running, Is.False, "Transition did not complete.");
        }

        [UnityTest]
        public IEnumerator StartGameTransitionAndRunToCompletionThenStartAgain()
        {
            ClearPreviousTestCallbacks();
            TestGameTransition.UpdateFunctionForTest = WaitForOneSecond;
            yield return new MonoBehaviourTest<TestGameTransition>();
            TestGameTransition behaviorTest = Object.FindObjectOfType<TestGameTransition>();
            behaviorTest.StartSimpleTest();
            Assert.That(behaviorTest.TransitionTest.Running, Is.True, "Transition did not start.");
        }

        [UnityTest]
        public IEnumerator StartGameTransitionAndStopAfterOneFrame()
        {
            ClearPreviousTestCallbacks();
            TestGameTransition.UpdateFunctionForTest = WaitForOneSecond;
            new MonoBehaviourTest<TestGameTransition>();
            TestGameTransition behaviorTest = Object.FindObjectOfType<TestGameTransition>();
            yield return new WaitForEndOfFrame();
            behaviorTest.TransitionTest.Stop();
            Assert.That(behaviorTest.TransitionTest.Running, Is.False, "Transition did not get stopped.");
        }

        [UnityTest]
        public IEnumerator StartGameTransitionAndStopAfterOneFrameThenCallStopAgain()
        {
            ClearPreviousTestCallbacks();
            TestGameTransition.UpdateFunctionForTest = WaitForOneSecond;
            new MonoBehaviourTest<TestGameTransition>();
            TestGameTransition behaviorTest = Object.FindObjectOfType<TestGameTransition>();
            yield return new WaitForEndOfFrame();
            behaviorTest.TransitionTest.Stop();
            behaviorTest.TransitionTest.Stop();
            Assert.That(behaviorTest.TransitionTest.Running, Is.False, "Transition did not get stopped.");
        }

        [UnityTest]
        public IEnumerator StartGameTransitionAndRunToCompletionAndSetABoolOnComplete()
        {
            ClearPreviousTestCallbacks();
            bool test = false;
            TestGameTransition.UpdateFunctionForTest = WaitForOneSecond;
            TestGameTransition.OnCompleteFunctionForTest = (MonoBehaviour m) => { test = true; };
            yield return new MonoBehaviourTest<TestGameTransition>();
            TestGameTransition behaviorTest = Object.FindObjectOfType<TestGameTransition>();
            Assert.That(test, Is.True, "Callback did not invoke on completion.");
        }

        [UnityTest]
        public IEnumerator StartGameTransitionAndRunToCompletionAndSetABoolOnStart()
        {
            ClearPreviousTestCallbacks();
            bool test = false;
            TestGameTransition.UpdateFunctionForTest = WaitForOneSecond;
            TestGameTransition.OnStartFunctionForTest = (MonoBehaviour m) => { test = true; };
            yield return new MonoBehaviourTest<TestGameTransition>();
            TestGameTransition behaviorTest = Object.FindObjectOfType<TestGameTransition>();
            Assert.That(test, Is.True, "Callback did not invoke on completion.");
        }

        [UnityTest]
        public IEnumerator StartGameTransitionAndRunToCompletionAndSubscribeToOnCompleteInOnStart()
        {
            ClearPreviousTestCallbacks();
            bool test = false;
            TestGameTransition.UpdateFunctionForTest = WaitForOneSecond;

            //on start, subscribe to to oncomplete with a function that sets test to true
            TestGameTransition.OnStartFunctionForTest = (MonoBehaviour m) => {
                m.GetComponent<TestGameTransition>().TransitionTest.OnComplete =
                 (MonoBehaviour n) => { test = true; }; 
                };

            yield return new MonoBehaviourTest<TestGameTransition>();
            TestGameTransition behaviorTest = Object.FindObjectOfType<TestGameTransition>();
            Assert.That(test, Is.True, "Callback did not invoke on completion.");
        }


        //[UnityTest]
        //public IEnumerator StartGameTransitionAndAfterOneFrameCallStartAgain()
        //{
        //    ClearPreviousTestCallbacks();
        //    TestGameTransition.UpdateFunctionForTest = WaitForOneSecond;
        //    new MonoBehaviourTest<TestGameTransition>();
        //    TestGameTransition behaviorTest = Object.FindObjectOfType<TestGameTransition>();
        //    yield return new WaitForEndOfFrame();
        //    TestGameTransition.UpdateFunctionForTest = WaitForOneUpdate;
        //    behaviorTest.StartSimpleTest();

        //    var updateMethod = behaviorTest.GetType().GetProperty("UpdateTransition", BindingFlags.NonPublic | BindingFlags.Instance);

        //    Assert.That(behaviorTest.TransitionTest.Running && (updateMethod.Name == WaitForOneSecond), Is.True, "Transition is running.");
        //}
    }
#endif
}