using DarknessRandomizer.Data;
using DarknessRandomizer.IC;
using ItemChanger;
using Newtonsoft.Json;
using RandomizerCore.Logic;
using System.Collections.Generic;
using System.Linq;

namespace DarknessRandomizer.Rando;

public class DarknessVariableResolver : VariableResolver
{
    public DarknessVariableResolver(VariableResolver inner) => Inner = inner;

    [JsonConstructor]
    DarknessVariableResolver() { }

    public static bool TryGetDarkness(SceneName sceneName, out Darkness darkness)
    {
        // Both of these paths are needed. The former during randomization, before the module is created; the latter on save
        // load, where LS is no longer populated.
        if (RandoInterop.LS != null)
            return RandoInterop.LS.DarknessOverrides.TryGetValue(sceneName, out darkness);
        else if (ItemChangerMod.Modules.Get<DarknessRandomizerModule>()?.DarknessOverrides.TryGetValue(sceneName, out darkness) ?? false)
            return true;
        else
        {
            darkness = default;
            return false;
        }
    }

    public override bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
    {
        if (TryMatchPrefix(term, "$DarknessLevel", out var parameters) && parameters.Length == 1 &&
            SceneName.TryGetValue(parameters[0], out var sceneName))
        {
            variable = new DarknessLevelInt(sceneName);
            return true;
        }

#pragma warning disable CS8601 // Possible null reference assignment.
        if (Inner?.TryMatch(lm, term, out variable) ?? false) return true;
#pragma warning restore CS8601 // Possible null reference assignment.

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        variable = default;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        return false;
    }
}

internal class DarknessLevelInt(SceneName sceneName) : LogicInt
{
    public SceneName sceneName = sceneName;

    public override string Name { get; } = $"$DarknessLevel[{sceneName}]";

    public override IEnumerable<Term> GetTerms() => Enumerable.Empty<Term>();

    // Darkness levels don't change during randomization, so it's safe to cache this.
    private int? cache;

    public override int GetValue(object? sender, ProgressionManager pm) => cache ?? (cache = GetValueImpl()).Value;

    private int GetValueImpl() => DarknessVariableResolver.TryGetDarkness(sceneName, out Darkness d) ? (int)d : (int)Darkness.Bright;
}
