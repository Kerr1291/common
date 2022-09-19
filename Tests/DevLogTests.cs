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
    //public class DevLogTestHost : MonoBehaviour, IMonoBehaviourTest
    //{
    //    //how long until this test should fail
    //    float timeout = 2f;

    //    void Awake()
    //    {
    //    }

    //    //required by the unity "MonoBehaviourTest" framework 
    //    public bool IsTestFinished
    //    {
    //        get { return timeout <= 0f }
    //    }

    //    //(optional) for the unity "MonoBehaviourTest" framework 
    //    void Update()
    //    {
    //        timeout -= Time.deltaTime;
    //    }
    //}

    //public class DevLogTests
    //{
    //    [UnityTest]
    //    public IEnumerator CreateDevLogAndPostSomeData()
    //    {
    //        if(GameObject.FindObjectOfType<Camera>() == null)
    //        {
    //            GameObject temp = new GameObject();
    //            temp.AddComponent<Camera>();
    //        }
    //        bool test = false;
    //        for(int i = 0; i < 11; ++i)
    //            nv.Log("Test: "+i);
    //        nv.Dev.LogVar(test);
    //        //new MonoBehaviourTest<TestGameTransition>();
    //        var behaviorTest = Object.FindObjectOfType<DevLog.DevLogObject>();
    //        yield return null;
    //        Assert.That(behaviorTest != null, Is.True, "DevLog was created.");
    //        yield break;
    //    }
    //}
#endif
}