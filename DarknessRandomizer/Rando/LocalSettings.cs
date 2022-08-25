﻿using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

// Node-based logic for determining Hallownest darkness during randomization.
// This does not deal with logic-overrides of any kind.
namespace DarknessRandomizer.Rando
{
    public class LocalSettings
    {
        public DarknessRandomizationSettings Settings;
        public Dictionary<String, int> DarknessOverrides;

        public LocalSettings(int seed)
        {
            Settings = DarknessRandomizer.GS.DarknessRandomizationSettings.Clone();

            // TODO: Algorithm
        }
    }
}