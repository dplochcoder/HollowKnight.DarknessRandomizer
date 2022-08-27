﻿using ItemChanger;
using RandomizerMod.RC;

namespace DarknessRandomizer.Rando
{
    public static class RandoInterop
    {
        public static LocalSettings LS { get; set; }

        public static void Setup()
        {
            ConnectionMenu.Setup();
            LogicPatcher.Setup();
            RequestModifier.Setup();

            RandoController.OnExportCompleted += Finish;
            RandomizerMod.Logging.LogManager.AddLogger(new DarknessLogger());
            // FIXME: Condensed spoiler
        }

        public static bool IsEnabled() => DarknessRandomizer.GS.DarknessRandomizationSettings.RandomizeDarkness;

        public static void Finish(RandoController rc)
        {
            if (!IsEnabled()) return;

            // FIXME: Define a module for FSM edits.

            var dlem = ItemChangerMod.Modules.GetOrAdd<ItemChanger.Modules.DarknessLevelEditModule>();
            foreach (var entry in LS.DarknessOverrides) {
                dlem.darknessLevelsByScene[entry.Key] = (int)entry.Value;
            }
        }
    }
}
