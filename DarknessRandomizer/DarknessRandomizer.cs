using DarknessRandomizer.Data;
using DarknessRandomizer.IC;
using DarknessRandomizer.Rando;
using ItemChanger.Internal.Menu;
using Modding;
using PurenailCore.ModUtil;
using RandomizerMod;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DarknessRandomizer;

public class DarknessRandomizer : Mod, IGlobalSettings<GlobalSettings>, ICustomMenuMod
{
    public static DarknessRandomizer Instance { get; private set; }
    public static GlobalSettings GS { get; private set; } = new();

    public bool ToggleButtonInsideMenu => false;

    public static new void Log(string msg) => ((Loggable)Instance).Log(msg);

    public DarknessRandomizer() : base("DarknessRandomizer")
    {
        Instance = this;
    }

    public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
    {
        Preloader.Instance.Initialize(preloadedObjects);

        if (ModHooks.GetMod("Randomizer 4") is Mod)
        {
            Vanilla.Setup();
            RandoInterop.Setup();
        }

        SceneMetadata.Load();
        Data.SceneData.Load();
        ClusterData.Load();

        LanternShardItem.DefineICRefs();
        RandoPlusInterop.DefineICRefs();
    }

    public override List<(string, string)> GetPreloadNames() => [.. Preloader.Instance.GetPreloadNames()];

    public void OnLoadGlobal(GlobalSettings s) => GS = s ?? new();

    public GlobalSettings OnSaveGlobal() => GS ?? new();

    private static readonly string Version = VersionUtil.ComputeVersion<DarknessRandomizer>();

    public override string GetVersion() => Version;

    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
    {
        ModMenuScreenBuilder builder = new(Localization.Localize("Darkness Randomizer Viewer"), modListMenu);
        builder.AddButton(Localization.Localize("Open DarknessSpoiler.json"), null, OpenDarknessSpoiler);
        return builder.CreateMenuScreen();
    }

    private void OpenDarknessSpoiler()
    {
        string fname = Path.Combine(RandomizerMod.Logging.LogManager.RecentDirectory, "DarknessSpoiler.json");
        try
        {
            System.Diagnostics.Process.Start(fname);
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }
}