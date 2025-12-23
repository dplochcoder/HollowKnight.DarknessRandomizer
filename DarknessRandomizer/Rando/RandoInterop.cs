using DarknessRandomizer.IC;
using DarknessRandomizer.Imports;
using ItemChanger;
using RandomizerMod.RC;
using System.IO;

using JsonUtil = PurenailCore.SystemUtil.JsonUtil<DarknessRandomizer.DarknessRandomizer>;

namespace DarknessRandomizer.Rando;

public static class RandoInterop
{
    public static LocalSettings LS { get; set; }

    public static void Setup()
    {
        ConnectionMenu.Setup();
        LogicPatcher.Setup();
        RequestModifier.Setup();
        CondensedSpoilerLogger.AddCategory("Lantern Shards", _ => ShatteredLantern, [LanternShardItemName]);

        RandoController.OnExportCompleted += Finish;
        RandomizerMod.Logging.SettingsLog.AfterLogSettings += LogSettings;
        RandomizerMod.Logging.LogManager.AddLogger(new DarknessLogger());
    }

    public static bool IsEnabled => DarknessRandomizer.GS.RandomizationSettings.IsEnabled;

    public static bool RandomizeDarkness => DarknessRandomizer.GS.RandomizationSettings.RandomizeDarkness;

    public static bool ShatteredLantern => DarknessRandomizer.GS.RandomizationSettings.ShatteredLantern;

    public static string LanternTermName => RandoPlusInterop.LanternTermName;

    public static string LanternItemName => RandoPlusInterop.LanternItemName;

    public static string LanternShardItemName => RandoPlusInterop.LanternShardItemName;

    private static void Finish(RandoController rc)
    {
        if (!IsEnabled) return;

        ItemChangerMod.Modules.GetOrAdd<DarknessRandomizerModule>().DarknessOverrides = new(LS.DarknessOverrides);
    }

    private static void LogSettings(RandomizerMod.Logging.LogArguments args, TextWriter tw)
    {
        if (!IsEnabled) return;
        tw.WriteLine("Logging DarknessRando DarknessRandomizationSettings:");
        JsonUtil.Serialize(LS.Settings, tw);
        tw.WriteLine();
    }
}
