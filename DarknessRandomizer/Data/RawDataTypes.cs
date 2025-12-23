using System.Collections.Generic;

using JsonUtil = PurenailCore.SystemUtil.JsonUtil<DarknessRandomizer.DarknessRandomizer>;

namespace DarknessRandomizer.Data;

public class RawSceneMetadata : BaseSceneMetadata<string>
{
    public static SortedDictionary<string, RawSceneMetadata> LoadFromPath(string path) =>
        JsonUtil.DeserializeFromPath<SortedDictionary<string, RawSceneMetadata>>(path);
}

public class RawSceneData : BaseSceneData<string>
{
    public static SortedDictionary<string, RawSceneData> LoadFromPath(string path) =>
        JsonUtil.DeserializeFromPath<SortedDictionary<string, RawSceneData>>(path);
}

public class RawClusterData : BaseClusterData<string, string>
{
    public static SortedDictionary<string, RawClusterData> LoadFromPath(string path) =>
        JsonUtil.DeserializeFromPath<SortedDictionary<string, RawClusterData>>(path);

    public SortedDictionary<string, string> SceneNames = [];

    public SortedDictionary<string, RelativeDarkness> AdjacentClusters = [];

    public override int SceneCount => SceneNames.Count;

    protected override IEnumerable<string> EnumerateSceneNames() => SceneNames.Keys;

    protected override IEnumerable<KeyValuePair<string, RelativeDarkness>> EnumerateRelativeDarkness() => AdjacentClusters;
}
