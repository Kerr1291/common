using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace Components.Common.Tests
{

    public class TransitionTests
    {
        [Test]
        public void NewGameTransition()
        {
            var gameTransition = new Transition();

            Assert.IsNotNull(gameTransition);
        }
    }


    //public class TransitionIntegrationTests
    //{

    //    [UnityTest]
    //    public IEnumerator StartGameTransitionAndRunToCompletion()
    //    {
    //        yield return new MonoBehaviourTest<MyMonoBehaviourTest>();
    //        MyMonoBehaviourTest behaviorTest = Object.FindObjectOfType<MyMonoBehaviourTest>();
    //        Assert.That(behaviorTest.TransitionTest.Running, Is.False);
    //    }
    //}
 }