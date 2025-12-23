using DarknessRandomizer.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DarknessRandomizer.Lib;

public interface ITypedIdFactory<T> where T : ITypedId
{
    int Count();

    T FromId(int id);

    T FromName(string name);
}

public interface ITypedId
{
    int Id();

    string Name();
}

// A custom wrapper around `Dictionary<Id, Value>` which doesn't serialize correctly when Id is a special type.
public class TypedIdDictionary<K, V> where K : ITypedId
{
    private readonly ITypedIdFactory<K> factory;
    private readonly Dictionary<K, V> dict;

    [JsonIgnore]
    public SortedDictionary<string, V> AsSortedDict
    {
        get
        {
            SortedDictionary<string, V> ret = [];
            foreach (var e in dict)
            {
                ret[e.Key.Name()] = e.Value;
            }
            return ret;
        }
        set
        {
            Clear();
            foreach (var e in value)
            {
                dict[factory.FromName(e.Key)] = e.Value;
            }
        }
    }

    public TypedIdDictionary(ITypedIdFactory<K> factory) {
        this.factory = factory;
        dict = [];
    }

    public TypedIdDictionary(TypedIdDictionary<K, V> other)
    {
        factory = other.factory;
        dict = new(other.dict);
    }

    public bool TryGetValue(K id, out V value) => dict.TryGetValue(id, out value);

    public void Clear() => dict.Clear();

    public int Count => dict.Count;

    public IEnumerable<K> Keys => Enumerate().Select(e => e.Key);

    public IEnumerable<KeyValuePair<K, V>> Enumerate() => dict;

    public V this[K k]
    {
        get { return dict[k]; }
        set { dict[k] = value; }
    }
}

public class TypedIdDictionaryConverter<K, V, T> : JsonConverter<T> where K : ITypedId where T : TypedIdDictionary<K, V>, new()
{
    public override T ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        T ret = new()
        {
            AsSortedDict = serializer.Deserialize<SortedDictionary<string, V>>(reader) ?? []
        };
        return ret;
    }

    public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer) => serializer.Serialize(writer, value?.AsSortedDict);
}

[JsonConverter(typeof(TypedIdDictionaryConverter<SceneName, Darkness, SceneDarknessDict>))]
public class SceneDarknessDict : SceneDictionary<Darkness>
{
    public SceneDarknessDict() { }
    public SceneDarknessDict(SceneDarknessDict other) : base(other) { }
}

[JsonConverter(typeof(TypedIdDictionaryConverter<ClusterName, Darkness, ClusterDarknessDict>))]
public class ClusterDarknessDict : ClusterDictionary<Darkness> {
    public ClusterDarknessDict() { }
    public ClusterDarknessDict(ClusterDarknessDict other) : base(other) { }
}
