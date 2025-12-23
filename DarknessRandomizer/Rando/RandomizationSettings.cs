using Newtonsoft.Json;
using System;

namespace DarknessRandomizer.Rando;

public enum DarknessLevel
{
    Dim,
    Dark,
    Darker,
    Cursed
}

public class RandomizationSettings
{
    public bool RandomizeDarkness = false;
    public DarknessLevel DarknessLevel = DarknessLevel.Dark;
    public bool Chaos = false;
    public bool ShatteredLantern = false;
    public bool TwoDupeShards = false;

    [JsonIgnore]
    public bool IsEnabled => RandomizeDarkness || ShatteredLantern;

    public RandomizationSettings Clone() => (RandomizationSettings)MemberwiseClone();

    public int GetDarknessBudget(Random r)
    {
        int min, max;
        switch (DarknessLevel)
        {
            case DarknessLevel.Dim:
                min = 1000;
                max = 1500;
                break;
            case DarknessLevel.Dark:
                min = 3500;
                max = 4500;
                break;
            case DarknessLevel.Darker:
                min = 6000;
                max = 7000;
                break;
            case DarknessLevel.Cursed:
                min = 13000;
                max = 15000;
                break;
            default:
                throw new ArgumentException($"Unknown DarknessLevel: {DarknessLevel}");
        }

        int half = (max - min) / 2;
        return min + r.Next(0, half) + r.Next(0, half);
    }
}
