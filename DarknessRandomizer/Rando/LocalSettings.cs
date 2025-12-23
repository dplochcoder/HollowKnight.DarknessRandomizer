using DarknessRandomizer.Lib;
using RandomizerMod.RandomizerData;

// Node-based logic for determining Hallownest darkness during randomization.
// This does not deal with logic-overrides of any kind.
namespace DarknessRandomizer.Rando;

public class LocalSettings
{
    public RandomizationSettings Settings;
    public SceneDarknessDict DarknessOverrides = new();
    public AlgorithmStats Stats = new();

    public LocalSettings(RandomizerMod.Settings.GenerationSettings GS, StartDef startDef)
    {
        Settings = DarknessRandomizer.GS.RandomizationSettings.Clone();

        if (Settings.RandomizeDarkness)
        {
            var darknessAlgorithm = DarknessAlgorithm.Select(GS, startDef, Settings);
            darknessAlgorithm.SpreadDarkness(out DarknessOverrides, out Stats);
        }
    }
}