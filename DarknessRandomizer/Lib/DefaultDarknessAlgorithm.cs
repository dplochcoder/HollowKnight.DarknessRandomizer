using DarknessRandomizer.Data;
using DarknessRandomizer.Rando;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;
using System.Linq;

namespace DarknessRandomizer.Lib;

public record DefaultDarknessAlgorithmStats : ICustomDarknessAlgorithmStats
{
    public ClusterDarknessDict ClusterDarkness = new();
}

public class DefaultDarknessAlgorithm(GenerationSettings GS, StartDef start, RandomizationSettings DRS) : DarknessAlgorithm(GS, start, DRS)
{
    private readonly ClusterDarknessDict clusterDarkness = new();
    private readonly WeightedHeap<ClusterName> darkCandidates = new();

    public override void SpreadDarkness(out SceneDarknessDict darknessOverrides, out AlgorithmStats stats)
    {
        // Phase 0: Everything starts as bright.
        foreach (var c in ClusterName.All())
        {
            clusterDarkness[c] = Darkness.Bright;
        }

        // Phase 1: Select source nodes until we run out of darkness.
        foreach (var c in ClusterName.All())
        {
            var cData = ClusterData.Get(c);
            if (cData.CanBeDarknessSource(DRS) && !forcedBrightClusters.Contains(c))
            {
                darkCandidates.Add(c, cData.ProbabilityWeight.Value);
            }
        }
        while (!darkCandidates.IsEmpty() && darknessAvailable > 0)
        {
            SelectNewDarknessNode();
        }

        // Inject custom darkness here.
        // The code below does a full inversion - all traditionally lit rooms are dark, and vice versa.
        // Mainly for debugging/testing purposes. You'll need to turn on DARKROOMS for this to get past Logic.
        //
        // foreach (var c in ClusterName.All())
        // {
        //     var cData = ClusterData.Get(c);
        //     clusterDarkness[c] = cData.MaximumDarkness(settings);
        // }
        // clusterDarkness[ClusterName.GreenpathStoneSanctuary] = Darkness.Bright;
        // clusterDarkness[ClusterName.DeepnestDark] = Darkness.Bright;
        // clusterDarkness[ClusterName.CrystalPeaksToll] = Darkness.Bright;
        // clusterDarkness[ClusterName.CrystalPeaksDarkRoom] = Darkness.Bright;
        // clusterDarkness[ClusterName.CliffsJonis] = Darkness.Bright;

        // Phase 2: Calculate semi-dark clusters.
        foreach (var name in ClusterName.All())
        {
            var darkness = clusterDarkness[name];
            
            if (darkness == Darkness.Dark)
            {
                var cData = ClusterData.Get(name);
                foreach (var e in cData.AdjacentClusters.Enumerate())
                {
                    var aName = e.Key;
                    var rd = e.Value;
                    if (clusterDarkness[aName] == Darkness.Bright && rd != RelativeDarkness.Disconnected)
                    {
                        clusterDarkness[aName] = Darkness.SemiDark;
                    }
                }
            }
        }

        // Phase 3: Output the per-scene darkness levels.
        GetPerSceneDarknessLevels(out darknessOverrides, out stats);
    }

    private void SelectNewDarknessNode()
    {
        var name = darkCandidates.Remove(r);
        clusterDarkness[name] = Darkness.Dark;

        // This can go negative; fixing that would require pruning the heap of high cost clusters.
        var cData = ClusterData.Get(name);
        darknessAvailable -= cData.CostWeight.Value;

        // Add adjacent clusters if constraints are satisfied.
        foreach (var e in cData.AdjacentClusters.Enumerate())
        {
            var aName = e.Key;
            var rd = e.Value;

            if (clusterDarkness[aName] == Darkness.Dark || rd == RelativeDarkness.Disconnected || forcedBrightClusters.Contains(aName))
            {
                continue;
            }

            var aData = ClusterData.Get(aName);
            if (!darkCandidates.Contains(aName) && aData.MaximumDarkness(DRS) == Darkness.Dark
                && aData.AdjacentClusters.Enumerate().All(
                    e => e.Value != RelativeDarkness.Darker || clusterDarkness[e.Key] == Darkness.Dark))
            {
                darkCandidates.Add(aName, aData.ProbabilityWeight.Value);
            }
        }
    }

    private void GetPerSceneDarknessLevels(out SceneDarknessDict darknessOverrides, out AlgorithmStats stats)
    {
        darknessOverrides = new();
        foreach (var e in clusterDarkness.Enumerate())
        {
            var cluster = e.Key;
            var cData = ClusterData.Get(cluster);
            var darkness = e.Value;
            foreach (var scene in ClusterData.Get(cluster).SceneNames.Keys)
            {
                if (darkness == Darkness.SemiDark)
                {
                    // Only make a scene semi-dark if it has a dark neighbor.
                    var anyDarkNeighbor = SceneMetadata.Get(scene).AdjacentScenes.Any(aScene =>
                    {
                        var aCluster = Data.SceneData.Get(aScene).Cluster;
                        return aCluster != cluster && clusterDarkness[aCluster] == Darkness.Dark
                            && cData.AdjacentClusters.TryGetValue(aCluster, out RelativeDarkness rd) && rd != RelativeDarkness.Disconnected;
                    });

                    darknessOverrides[scene] = Data.SceneData.Get(scene).ClampDarkness(anyDarkNeighbor ? Darkness.SemiDark : Darkness.Bright);
                }
                else
                {
                    darknessOverrides[scene] = Data.SceneData.Get(scene).ClampDarkness(darkness);
                }
            }
        }

        stats = new()
        {
            DarknessSpent = 0,
            DarknessRemaining = 0,
            CustomStats = new DefaultDarknessAlgorithmStats()
            {
                ClusterDarkness = new(clusterDarkness)
            }
        };
        foreach (var e in clusterDarkness.Enumerate())
        {
            var cData = ClusterData.Get(e.Key);
            if (e.Value == Darkness.Dark)
            {
                stats.DarknessSpent += cData.CostWeight.Value;
            }
            else if (cData.MaximumDarkness(DRS) == Darkness.Dark && !forcedBrightClusters.Contains(e.Key))
            {
                stats.DarknessRemaining += cData.CostWeight.Value;
            }
        }
    }
}
