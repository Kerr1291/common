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
    public class TimedRoutineTests
    {
        [UnityTest]
        public IEnumerator CreateAndStart()
        {
            var testRoutine = new TimedRoutine();
            testRoutine.Start(1f);

            Assert.That(testRoutine.IsRunning, Is.True, "TimedRoutine did not start.");
            Assert.That(SmartRoutineHelper.Instance.Routines.Count, Is.EqualTo(1), "TimedRoutine did not start.");
            yield break;
        }
    }
#endif
}