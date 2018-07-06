using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nv
{
    public class DemoMapGenerator : MonoBehaviour
    {
        public UnityEngine.SceneManagement.Scene test;

        [EditScriptable]
        public ProcGenMap mapToGenerate;
        
        void Start()
        {
            StartCoroutine(mapToGenerate.Generate());
        }
    }
}