using DarknessRandomizer.Data;
using DarknessRandomizer.Rando;
using PurenailCore.SystemUtil;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;
using System.Collections.Generic;

namespace DarknessRandomizer.Lib;

public class ChaosDarknessAlgorithm(GenerationSettings GS, StartDef start, RandomizationSettings DRS) : DarknessAlgorithm(GS, start, DRS)
{
    private void GetPerSceneStats(SceneName s, out Darkness maxDarkness, out int costWeight)
    {
        var sData = Data.SceneData.Get(s);
        if (s.Name() == start.SceneName)
        {
            maxDarkness = Darkness.Bright;
            costWeight = 0;
            return;
        }

        var cData = ClusterData.Get(s);
        maxDarkness = DarknessUtil.Min(sData.MaximumDarkness, cData.MaximumDarkness(DRS));
        costWeight = cData.CostWeight ?? (50 * cData.SceneCount) / cData.SceneCount;
    }

    public override void SpreadDarkness(out SceneDarknessDict darknessOverrides, out AlgorithmStats stats)
    {
        // Phase 0: Everything starts as bright.
        darknessOverrides = new();
        foreach (var s in SceneName.All())
        {
            darknessOverrides[s] = Darkness.Bright;
        }

        // Randomly select individual scenes to be dark or semi-dark, ignoring all usual constraints.
        List<SceneName> scenes = [.. SceneName.All()];
        scenes.Shuffle(r);

        // Phase 1: assign darkness and semi-darkness randomly.
        int semiDarknessAvailable = darknessAvailable;
        for (int i = 0; i < scenes.Count && (darknessAvailable > 0 || semiDarknessAvailable > 0); ++i)
        {
            var s = scenes[i];
            GetPerSceneStats(s, out Darkness maxDarkness, out int costWeight);

            if (darknessAvailable > 0 && maxDarkness >= Darkness.Dark)
            {
                darknessOverrides[s] = Darkness.Dark;
                darknessAvailable -= costWeight;
            }
            else if (semiDarknessAvailable > 0 && maxDarkness >= Darkness.SemiDark)
            {
                darknessOverrides[s] = Darkness.SemiDark;
                semiDarknessAvailable -= costWeight;
            }
        }

        stats = new();
        foreach (var s in SceneName.All())
        {
            GetPerSceneStats(s, out Darkness maxDarkness, out int costWeight);
            if (darknessOverrides[s] >= Darkness.Dark)
            {
                stats.DarknessSpent += costWeight;
            }
            else if (maxDarkness >= Darkness.Dark)
            {
                stats.DarknessRemaining += costWeight;
            }
        }
    }
}
