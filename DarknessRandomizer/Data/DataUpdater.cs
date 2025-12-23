using PurenailCore.SystemUtil;
using RandomizerCore.Logic;
using RandomizerMod.RC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using JsonUtil = PurenailCore.SystemUtil.JsonUtil<DarknessRandomizer.DarknessRandomizer>;

namespace DarknessRandomizer.Data;

public static class DataUpdater
{
    private class Stats
    {
        public int NumClusters;
        public int NumSemiDarkOnlyClusters;
        public int NumCursedOnlyClusters;
        public int NumClusterAdjacencies;
        public int NumScenes;
        public int NumSemiDarkOnlyScenes;
        public int NumCursedOnlyScenes;
        public int NumSceneAdjacencies;
        public int NumDarknessSources;
        public int TotalSourceBudget;
        public int TotalCursedSourceBudget;
        public int TotalDarknessBudget;
        public int TotalCursedDarknessBudget;
    }

    private static void SyncDicts<K,V1,V2>(IDictionary<K, V1> src, IDictionary<K, V2> dst, Func<K, V2> creator)
    {
        // Add missing values.
        foreach (var key in src.Keys)
        {
            dst.AddIfEmpty(key, () => creator.Invoke(key));
        }

        // Remove any broken values.
        List<K> toRemove = [];
        foreach (var key in dst.Keys)
        {
            if (!src.ContainsKey(key))
            {
                toRemove.Add(key);
            }
        }
        toRemove.ForEach(k => dst.Remove(k));
    }

    public static void Run()
    {
        string root = JsonUtil.InferGitRoot();
        Console.WriteLine("Updating data");

        // Load all the data.
        var SM = RawSceneMetadata.LoadFromPath($"{root}/DarknessRandomizer/Resources/Data/scene_metadata.json");
        var SD = RawSceneData.LoadFromPath($"{root}/DarknessRandomizer/Resources/Data/scene_data.json");
        var CD = RawClusterData.LoadFromPath($"{root}/DarknessRandomizer/Resources/Data/cluster_data.json");

        // We delete scenes from metadata to not track them; make sure adjacencies are also updated.
        foreach (var e in SM)
        {
            var scene = e.Key;
            var sData = e.Value;
            sData.AdjacentScenes.RemoveWhere(s => !SM.ContainsKey(s));

            // Ensure adjacencies are reflected.
            foreach (var aScene in sData.AdjacentScenes)
            {
                SM[aScene].AdjacentScenes.Add(scene);
            }
        }

        // Sync SceneData to SceneMetadata.
        SyncDicts(SM, SD, k => new() { Alias = SM[k].Alias });

        // Validate SceneData
        List<string> exceptions = [];
        foreach (var e in SD)
        {
            var scene = e.Key;
            var sData = e.Value;
            if (sData.MaximumDarkness < sData.MinimumDarkness)
            {
                exceptions.Add($"Invalid darkness settings for scene {scene}");
            }
            if (sData.Cluster == null || sData.Alias == null)
            {
                exceptions.Add($"Scene {scene} is missing fields");
            }

            sData.DisplayDarknessOverrides = CleanDDO(sData);
        }
        MaybeThrowException(exceptions);

        // Build up metadata maps.
        Dictionary<string, HashSet<string>> clusterToScenes = [];
        Dictionary<string, string> sceneToCluster = [];
        foreach (var e in SD)
        {
            var scene = e.Key;
            var sData = e.Value;
            if (sData.Cluster == "UNASSIGNED") continue;

            sceneToCluster[scene] = sData.Cluster;
            clusterToScenes.GetOrAddNew(sData.Cluster).Add(scene);
        }

        // Determine cluster adjacency.
        Dictionary<string, HashSet<string>> clusterAdjacency = [];
        foreach (var e in clusterToScenes)
        {
            var cluster = e.Key;
            var scenes = e.Value;

            foreach (var scene in scenes)
            {
                foreach (var aScene in SM[scene].AdjacentScenes)
                {
                    if (sceneToCluster.TryGetValue(aScene, out string aCluster) && aCluster != cluster)
                    { 
                        clusterAdjacency.GetOrAddNew(cluster).Add(aCluster);
                    }
                }
            }
        }

        SyncDicts(clusterToScenes, CD, k => new());

        // Update and validate each cluster.
        foreach (var e in CD)
        {
            var cluster = e.Key;
            var cData = e.Value;

            // Update scene names.
            cData.SceneNames = [];
            foreach (var scene in clusterToScenes[cluster])
            {
                cData.SceneNames[scene] = SM[scene].Alias;
            }
        }

        foreach (var e in CD)
        {
            var cluster = e.Key;
            var cData = e.Value;

            if (!clusterAdjacency.TryGetValue(cluster, out HashSet<string> aClusters))
            {
                aClusters = [];
            }

            // Sync adjacencies.
            foreach (var aCluster in aClusters)
            {
                cData.AdjacentClusters.AddIfEmpty(aCluster, () => RelativeDarkness.Unspecified);
            }

            // Remove obsolete adjacencies.
            List<string> toRemove = [];
            foreach (var aCluster in cData.AdjacentClusters.Keys)
            {
                if (!aClusters.Contains(aCluster))
                {
                    toRemove.Add(aCluster);
                }
            }
            toRemove.ForEach(c => cData.AdjacentClusters.Remove(c));

            // Validate the remaining adjacencies.
            List<Action> deferred = [];
            foreach (var e2 in cData.AdjacentClusters)
            {
                var aCluster = e2.Key;
                var aData = CD[aCluster];
                var rd = e2.Value;
                if (rd == RelativeDarkness.Disconnected
                    || (aData.AdjacentClusters.TryGetValue(cluster, out RelativeDarkness ard) && ard == RelativeDarkness.Disconnected))
                {
                    deferred.Add(() => cData.AdjacentClusters[aCluster] = RelativeDarkness.Disconnected);
                    aData.AdjacentClusters[cluster] = RelativeDarkness.Disconnected;
                    continue;
                }

                // If a certain relationship is forced, apply that.
                bool canBeDark = cData.MaximumDarkness(s => SD[s]) == Darkness.Dark;
                bool aCanBeDark = aData.MaximumDarkness(s => SD[s]) == Darkness.Dark;
                if (!canBeDark && !aCanBeDark)
                {
                    deferred.Add(() => cData.AdjacentClusters[aCluster] = RelativeDarkness.Any);
                    aData.AdjacentClusters[cluster] = RelativeDarkness.Any;
                    continue;
                }
                else if (canBeDark && !aCanBeDark)
                {
                    deferred.Add(() => cData.AdjacentClusters[aCluster] = RelativeDarkness.Brighter);
                    aData.AdjacentClusters[cluster] = RelativeDarkness.Darker;
                    continue;
                }
                else if (!canBeDark && aCanBeDark)
                {
                    deferred.Add(() => cData.AdjacentClusters[aCluster] = RelativeDarkness.Darker);
                    aData.AdjacentClusters[cluster] = RelativeDarkness.Brighter;
                    continue;
                }

                // If we're unspecified, let the other side force our hand.
                if (rd == RelativeDarkness.Unspecified) continue;

                // Inspect and validate te opposing cluster.
                if (!aData.AdjacentClusters.TryGetValue(cluster, out ard) || ard == RelativeDarkness.Unspecified)
                {
                    aData.AdjacentClusters[cluster] = rd.Opposite();
                }
                else if (ard != rd.Opposite())
                {
                    exceptions.Add($"RelativeDarkness mismatch between {cluster} and {aCluster}");
                }
            }
            deferred.ForEach(a => a.Invoke());

            // Don't set optional bools if they have no effect.
            bool? origOverride = cData.OverrideCannotBeDarknessSource;
            bool? origCursed = cData.CursedOnly;
            cData.OverrideCannotBeDarknessSource = null;
            cData.CursedOnly = null;
            if (!cData.CanBeDarknessSource(s => SD[s]))
            {
                origOverride = null;
            }
            else
            {
                origOverride ??= false;
            }
            if (cData.MaximumDarkness(s => SD[s]) < Darkness.Dark)
            {
                origCursed = null;
            }
            else
            {
                origCursed ??= false;
            }
            cData.OverrideCannotBeDarknessSource = origOverride;
            cData.CursedOnly = origCursed;

            // Handle darkness settings.
            if (cData.MaximumDarkness(s => SD[s]) < Darkness.Dark)
            {
                cData.DarkSettings = null;
            }
            else
            {
                cData.DarkSettings ??= new();
            }
        }
        MaybeThrowException(exceptions);

        var DS = ComputeDataStats(SM, SD, CD);
        RewriteJsonFile(DS, $"{root}/DarknessRandomizer/Resources/Data/data_stats.json");

        // Only update data at the end, if we have no exceptions.
        RewriteJsonFile(SM, $"{root}/DarknessRandomizer/Resources/Data/scene_metadata.json");
        RewriteJsonFile(SD, $"{root}/DarknessRandomizer/Resources/Data/scene_data.json");
        RewriteJsonFile(CD, $"{root}/DarknessRandomizer/Resources/Data/cluster_data.json");

        UpdateCSFile($"{root}/DarknessRandomizer/Data/SceneName.cs", "INSERT_SCENE_NAMES", SM,
            (n, sm) => $"public static readonly SceneName {CSharpClean(sm.Alias)} = new(\"{n}\")");
        UpdateCSFile($"{root}/DarknessRandomizer/Data/ClusterName.cs", "INSERT_CLUSTER_NAMES", CD,
            (n, cd) => $"public static readonly ClusterName {CSharpClean(n)} = new(\"{n}\")");
        UpdateCSFile($"{root}/DarknessRandomizer/Data/WaypointName.cs", "INSERT_WAYPOINTS", GetWaypointsDict(),
            (k, v) => $"public const string {k} = \"{v}\"");
        Console.WriteLine("Updated data!");
    }

    private static DisplayDarknessOverrides CleanDDO(RawSceneData sceneData)
    {
        var ddo = sceneData.DisplayDarknessOverrides;
        if (ddo == null) return null;

        ddo.DarknessRegions.RemoveAll(dr => dr.Darkness == ddo.SceneDarkness);
        ddo.DarknessRegions.Sort((a, b) => Math.Sign((a.X != b.X) ? a.X - b.X : a.Y - b.Y));

        ddo.Conditions.RemoveWhere(d => d < sceneData.MinimumDarkness || d > sceneData.MaximumDarkness);
        ddo.Conditions.RemoveWhere(d => ddo.SceneDarkness == d && ddo.DarknessRegions.All(dr => dr.Darkness == d));
        if (ddo.Conditions.Count == 0) return null;

        return ddo;
    }

    private static void MaybeThrowException(List<string> exceptions)
    {
        exceptions.ForEach(Console.WriteLine);
        if (exceptions.Count > 0)
        {
            throw new ArgumentException($"{exceptions.Count} data errors encountered");
        }
    }

    private static Stats ComputeDataStats(SortedDictionary<string, RawSceneMetadata> SM,
        SortedDictionary<string, RawSceneData> SD, SortedDictionary<string, RawClusterData> CD)
    {
        Stats stats = new();
        foreach (var e in CD)
        {
            var cluster = e.Key;
            var cData = e.Value;

            bool darkSource = cData.CanBeDarknessSource(s => SD[s]);
            bool semiDark = cData.MaximumDarkness(s => SD[s]) == Darkness.SemiDark;
            bool cursed = cData.CursedOnly ?? false;
            int darkCost = cData.CostWeight ?? 0;

            ++stats.NumClusters;
            stats.NumDarknessSources += darkSource ? 1 : 0;
            stats.NumClusterAdjacencies += cData.AdjacentClusters.Count;
            stats.NumCursedOnlyClusters += cursed ? 1 : 0;
            stats.NumSemiDarkOnlyClusters += semiDark ? 1 : 0;
            stats.TotalDarknessBudget += cursed ? 0 : darkCost;
            stats.TotalCursedDarknessBudget += darkCost;
            stats.TotalSourceBudget += darkSource ? (cursed ? 0 : darkCost) : 0;
            stats.TotalCursedSourceBudget += darkSource ? darkCost : 0;
        }
        foreach (var e in SD)
        {
            var scene = e.Key;
            var sData = e.Value;
            var cData = CD[sData.Cluster];

            bool semiDark = sData.MaximumDarkness == Darkness.SemiDark;

            ++stats.NumScenes;
            stats.NumSceneAdjacencies += SM[scene].AdjacentScenes.Count;
            stats.NumSemiDarkOnlyScenes += semiDark ? 1 : 0;
            stats.NumCursedOnlyScenes += cData.CursedOnly ?? false ? 1 : 0;
        }
        stats.NumSceneAdjacencies /= 2;
        stats.NumClusterAdjacencies /= 2;
        return stats;
    }

    private static SortedDictionary<string, string> GetWaypointsDict()
    {
        SortedDictionary<string, string> dict = [];
        LogicManager lm = RCData.GetNewLogicManager(new());
        foreach (var lw in lm.Waypoints)
        {
            dict[CSharpClean(lw.term.Name)] = lw.term.Name;
        }
        return dict;
    }

    private static void RewriteJsonFile<T>(T data, string path)
    {
        File.Delete(path);
        JsonUtil.Serialize(data, path);
    }

    private static string CSharpClean(string name) => name.Replace("_", "").Replace("'", "").Replace("-", "");

    private delegate string CSAssignment<K, V>(K key, V value);

    private static void UpdateCSFile<K, V>(string path, string marker, IDictionary<K, V> dict, CSAssignment<K, V> assigner)
    {
        using StreamReader sr = new(path);
        List<string> outLines = [];
        string line, indent;
        int state = 0;
        while (true)
        {
            line = sr.ReadLine();
            if (line == null) break;

            if (state == 0)
            {
                outLines.Add(line);
                Match match = Regex.Match(line, $"^(.+)// @@@ {Regex.Escape(marker)} START @@@$");
                if (match.Success)
                {
                    state = 1;
                    indent = match.Groups[1].Value;
                    foreach (var e in dict)
                    {
                        outLines.Add($"{indent}{assigner.Invoke(e.Key, e.Value)};");
                    }
                }
            }
            else if (state == 1)
            {
                Match match = Regex.Match(line, $"^.+// @@@ {Regex.Escape(marker)} END @@@$");
                if (match.Success)
                {
                    state = 2;
                    outLines.Add(line);
                }
            }
            else
            {
                outLines.Add(line);
            }
        }

        sr.Close();
        File.Delete(path);

        using StreamWriter sw = new(path);
        outLines.ForEach(l => sw.WriteLine(l));
    }
}
