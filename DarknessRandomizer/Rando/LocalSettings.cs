﻿using DarknessRandomizer.Lib;
using Modding;
using RandomizerMod.RandomizerData;
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
        public Dictionary<string, Darkness> DarknessOverrides;

        public LocalSettings(int seed, StartDef startDef)
        {
            Settings = DarknessRandomizer.GS.DarknessRandomizationSettings.Clone();

            Algorithm algorithm = new(seed, startDef, Settings, Graph.Instance);
            DarknessOverrides = algorithm.SelectDarknessLevels();
        }
    }
}