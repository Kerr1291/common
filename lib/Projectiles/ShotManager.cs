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
    }
}
