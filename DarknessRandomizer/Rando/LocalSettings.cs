﻿using DarknessRandomizer.Data;
using DarknessRandomizer.Lib;
using RandomizerMod.RandomizerData;
using System.Collections.Generic;

// Node-based logic for determining Hallownest darkness during randomization.
// This does not deal with logic-overrides of any kind.
namespace DarknessRandomizer.Rando
{
    public class LocalSettings
    {
        public DarknessRandomizationSettings Settings;
        public SceneDarknessDict DarknessOverrides = new();
        public AlgorithmStats Stats = new();

        public LocalSettings(RandomizerMod.Settings.GenerationSettings GS, StartDef startDef)
        {
            Settings = DarknessRandomizer.GS.DarknessRandomizationSettings.Clone();

            if (Settings.RandomizeDarkness)
            {
                Algorithm algorithm = new(GS, startDef, Settings);
                algorithm.SelectDarknessLevels(out DarknessOverrides, out Stats);
            }
        }
    }
}