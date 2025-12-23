using DarknessRandomizer.Data;
using DarknessRandomizer.Rando;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;
using System;
using System.Collections.Generic;

namespace DarknessRandomizer.Lib;

public class AlgorithmStats
{
    public int DarknessSpent;
    public int DarknessRemaining;
    public ICustomDarknessAlgorithmStats? CustomStats = null;
}

public interface ICustomDarknessAlgorithmStats { }

public abstract class DarknessAlgorithm
{
    public static DarknessAlgorithm Select(GenerationSettings GS, StartDef start, RandomizationSettings DRS) => DRS.Chaos ? new ChaosDarknessAlgorithm(GS, start, DRS) : new DefaultDarknessAlgorithm(GS, start, DRS);

    protected readonly GenerationSettings GS;
    protected readonly StartDef start;
    protected readonly RandomizationSettings DRS;

    protected readonly Random r;
    protected int darknessAvailable;
    protected readonly HashSet<ClusterName> forcedBrightClusters;

    protected DarknessAlgorithm(GenerationSettings GS, StartDef start, RandomizationSettings DRS)
    {
        this.GS = GS;
        this.start = start;
        this.DRS = DRS;
        r = new(GS.Seed + 7);
        darknessAvailable = DRS.GetDarknessBudget(r);

        forcedBrightClusters = [.. Starts.GetStartClusters(start.Name)];

        // Always include the local cluster, even in TRANDO.
        if (SceneName.TryGetValue(start.SceneName, out SceneName sceneName))
        {
            forcedBrightClusters.Add(Data.SceneData.Get(sceneName).Cluster);
        }

        if (!GS.PoolSettings.Keys)
        {
            // If lantern isn't randomized, we need to ensure the vanilla lantern location (Sly) is accessible.
            // This won't work if they also didn't randomize stags but I'm not trying to fix that.
            forcedBrightClusters.Add(ClusterName.CrossroadsStag);
            forcedBrightClusters.Add(ClusterName.CrossroadsStagHub);
            forcedBrightClusters.Add(ClusterName.CrossroadsShops);
        }

        if (GS.CursedSettings.Deranged)
        {
            // Disallow vanilla dark rooms.
            forcedBrightClusters.Add(ClusterName.CliffsJonis);
            forcedBrightClusters.Add(ClusterName.CrystalPeaksDarkRoom);
            forcedBrightClusters.Add(ClusterName.CrystalPeaksToll);
            forcedBrightClusters.Add(ClusterName.DeepnestDark);
            forcedBrightClusters.Add(ClusterName.GreenpathStoneSanctuary);
        }

        // If white palace rando is disabled, add all white palace rooms to the forbidden set.
        if (GS.LongLocationSettings.WhitePalaceRando != LongLocationSettings.WPSetting.Allowed)
        {
            foreach (var cluster in ClusterName.All())
            {
                var cData = ClusterData.Get(cluster);
                if (cData.IsInPathOfPain || (GS.LongLocationSettings.WhitePalaceRando == LongLocationSettings.WPSetting.ExcludeWhitePalace && cData.IsInWhitePalace))
                {
                    forcedBrightClusters.Add(cluster);
                }
            }
        }
    }

    public abstract void SpreadDarkness(out SceneDarknessDict sceneDarkness, out AlgorithmStats stats);
}
