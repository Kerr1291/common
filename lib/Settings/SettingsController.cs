using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace nv
{
    public class SettingsController : GameSingleton<SettingsController>
    {
        [EditScriptable]
        public ApplicationControls applicationControls;

        [EditScriptable]
        public GraphicsSettings graphicsSettings;

        public void Awake()
        {
            graphicsSettings.Init();
        }
    }
}