using DarknessRandomizer.IC;
using DarknessRandomizer.Lib;
using DarknessRandomizer.Rando;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DarknessRandomizer.Data;

public enum Darkness : int
{
    SuperBright = -1,
    Bright = 0,
    SemiDark = 1,
    Dark = 2
}

// Because we use compiled identifier types, and the code which updates those types also depends on them, we need two classes
// of data types in order to bootstrap. One, DataTypes.cs, uses the strongly-typed identifiers, where as RawDataTypes.cs uses
// only strings. The DataUpdater code uses raw data types, whereas the rest uses DataTypes.cs.
//
// Both sets of classes load from the same json files and therefore must have compatible serialization as well.

public class SceneDictionary<V> : TypedIdDictionary<SceneName, V>
{
    public SceneDictionary() : base(SceneName.Factory) { }
    public SceneDictionary(SceneDictionary<V> other) : base(other) { }
}

public class ClusterDictionary<V> : TypedIdDictionary<ClusterName, V>
{
    public ClusterDictionary() : base(ClusterName.Factory) { }
    public ClusterDictionary(ClusterDictionary<V> other) : base(other) { }
}

public class BaseSceneMetadata<SceneNameT>
{
    public string Alias;
    public string MapArea;
    public Darkness OrigDarkness;
    public SortedSet<SceneNameT> AdjacentScenes;
}

public class DisplayDarknessOverrides
{
    public SortedSet<Darkness> Conditions = [Darkness.Dark];
    public Darkness SceneDarkness = Darkness.SemiDark;
    public List<DarknessRegion> DarknessRegions;

    public bool Applies(Darkness d) => Conditions.Contains(d);
}

public class BaseSceneData<ClusterNameT>
{
    public string Alias;
    public ClusterNameT Cluster;
    public Darkness MinimumDarkness = Darkness.Bright;
    public Darkness MaximumDarkness = Darkness.Dark;

    // The presence of this data means the scene should could as Dark in logic, but display as SemiDark.
    // The DarknessRegions contained within will be deployed to make the relevant checks Dark.
    public DisplayDarknessOverrides? DisplayDarknessOverrides;

    public Darkness ClampDarkness(Darkness d) => d.Clamp(MinimumDarkness, MaximumDarkness);
}

public enum RelativeDarkness
{
    Unspecified,
    Brighter,
    Any,
    Darker,
    Disconnected
}

public class DarkSettings
{
    public int ProbabilityWeight = 100;
    public int CostWeight = 100;
}

public abstract class BaseClusterData<SceneNameT, ClusterNameT>
{
    public bool? OverrideCannotBeDarknessSource = null;
    public bool? CursedOnly = false;
    public DarkSettings? DarkSettings = null;

    public delegate BaseSceneData<ClusterNameT> SceneLookup(SceneNameT scene);

    [JsonIgnore]
    public abstract int SceneCount { get; }

    protected abstract IEnumerable<SceneNameT> EnumerateSceneNames();

    protected abstract IEnumerable<KeyValuePair<ClusterNameT, RelativeDarkness>> EnumerateRelativeDarkness();

    [JsonIgnore]
    public int? ProbabilityWeight => DarkSettings?.ProbabilityWeight;

    [JsonIgnore]
    public int? CostWeight => DarkSettings?.CostWeight;

    public bool CanBeDarknessSource(SceneLookup SL, RandomizationSettings settings = null)
    {
        if (MaximumDarkness(SL, settings) < Darkness.Dark) return false;
        if (OverrideCannotBeDarknessSource ?? false) return false;
        return EnumerateRelativeDarkness().All(e => e.Value != RelativeDarkness.Darker);
    }

    public Darkness MaximumDarkness(SceneLookup SL, RandomizationSettings settings = null)
    {
        var d = Darkness.Bright;
        foreach (var sn in EnumerateSceneNames())
        {
            Darkness d2 = SL.Invoke(sn).MaximumDarkness;
            if (d2 > d) d = d2;
        }
        
        if (d == Darkness.Dark && (CursedOnly ?? false) && (settings?.DarknessLevel ?? DarknessLevel.Cursed) != DarknessLevel.Cursed)
        {
            return Darkness.SemiDark;
        }
        return d;
    }

    public Darkness MinimumDarkness(SceneLookup SL)
    {
        var d = Darkness.Dark;
        foreach (var sn in EnumerateSceneNames())
        {
            Darkness d2 = SL.Invoke(sn).MinimumDarkness;
            if (d2 < d) d = d2;
        }
        return d;
    }
}

public static class DarknessUtil
{
    public static Darkness Min(Darkness a, Darkness b) => a < b ? a : b;

    public static Darkness Max(Darkness a, Darkness b) => a > b ? a : b;

    public static Darkness Clamp(this Darkness self, Darkness min, Darkness max) => Min(Max(self, min), max);
}

public static class DataExtensions
{
    public static Darkness? ToDarkness(this int darkness) => darkness switch
    {
        -1 => (Darkness?)Darkness.SuperBright,
        0 => (Darkness?)Darkness.Bright,
        1 => (Darkness?)Darkness.SemiDark,
        2 => (Darkness?)Darkness.Dark,
        _ => null,
    };

    public static RelativeDarkness Opposite(this RelativeDarkness rd) => rd switch
    {
        RelativeDarkness.Any => RelativeDarkness.Any,
        RelativeDarkness.Brighter => RelativeDarkness.Darker,
        RelativeDarkness.Darker => RelativeDarkness.Brighter,
        RelativeDarkness.Unspecified => RelativeDarkness.Unspecified,
        RelativeDarkness.Disconnected => RelativeDarkness.Disconnected,
        _ => throw new ArgumentException($"Unknown RelativeDarkness {rd}"),
    };
}
