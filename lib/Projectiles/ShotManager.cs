using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace nv
{
    public class ShotManager : GameSingleton<ShotManager>
    {
        //list of active shots
        public ShotList shotList;

        public IEnumerator updateFunction;

        public void Awake()
        {
            shotList.Setup();

            updateFunction = UpdateFunction();
            StartCoroutine(updateFunction);
        }

        IEnumerator UpdateFunction()
        {
            for(; ; )
            {
                shotList.UpdateView();
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
