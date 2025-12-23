using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using Modding;
using RandomizerMod.Menu;
using RandoSettingsManager;
using System;
using System.Collections.Generic;
using static RandomizerMod.Localization;

namespace DarknessRandomizer.Rando;

internal class ConnectionMenu
{
    public static ConnectionMenu Instance { get; private set; }

    public static void Setup()
    {
        RandomizerMenuAPI.AddMenuPage(OnRandomizerMenuConstruction, TryGetMenuButton);
        MenuChangerMod.OnExitMainMenu += () => Instance = null;

        if (ModHooks.GetMod("RandoSettingsManager") is Mod)
        {
            HookRandoSettingsManager();
        }
    }

    private static void HookRandoSettingsManager() => RandoSettingsManagerMod.Instance.RegisterConnection(new SettingsProxy());

    public static void OnRandomizerMenuConstruction(MenuPage page) => Instance = new(page);

    public static bool TryGetMenuButton(MenuPage page, out SmallButton button)
    {
        button = Instance.entryButton;
        return true;
    }

    private SmallButton entryButton;
    private MenuItem<bool> randomizeDarkness;
    private MenuItem<DarknessLevel> darknessLevel;
    private MenuItem<bool> chaos;
    private MenuItem<bool> shatteredLantern;
    private MenuItem<bool> twoDupeShards;

    private static T Lookup<T>(MenuElementFactory<RandomizationSettings> factory, string name) where T : MenuItem => factory.ElementLookup[name] as T ?? throw new ArgumentException("Menu error");

    private void LockIfFalse(MenuItem<bool> src, List<ILockable> dest)
    {
        void onChange(bool value)
        {
            foreach (var lockable in dest)
            {
                if (value) lockable.Unlock();
                else lockable.Lock();
            }
            SetEnabledColor();
        }

        src.ValueChanged += onChange;
        onChange(src.Value);
    }

    private void SetEnabledColor() => entryButton.Text.color = DarknessRandomizer.GS.RandomizationSettings.IsEnabled ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR; 

    private ConnectionMenu(MenuPage landingPage)
    {
        MenuPage mainPage = new("DarknessRando Main Page", landingPage);
        entryButton = new(landingPage, Localize("Darkness Rando"));
        entryButton.AddHideAndShowEvent(mainPage);

        var settings = DarknessRandomizer.GS.RandomizationSettings;
        MenuElementFactory<RandomizationSettings> factory = new(mainPage, settings);
        randomizeDarkness = Lookup<MenuItem<bool>>(factory, nameof(settings.RandomizeDarkness));
        darknessLevel = Lookup<MenuItem<DarknessLevel>>(factory, nameof(settings.DarknessLevel));
        chaos = Lookup<MenuItem<bool>>(factory, nameof(settings.Chaos));
        shatteredLantern = Lookup<MenuItem<bool>>(factory, nameof(settings.ShatteredLantern));
        twoDupeShards = Lookup<MenuItem<bool>>(factory, nameof(settings.TwoDupeShards));

        LockIfFalse(randomizeDarkness, [darknessLevel, chaos]);
        LockIfFalse(shatteredLantern, [twoDupeShards]);
        SetEnabledColor();

        GridItemPanel gridItemPanel = new(mainPage, SpaceParameters.TOP_CENTER_UNDER_TITLE, 2, SpaceParameters.VSPACE_MEDIUM, SpaceParameters.HSPACE_LARGE, true);
        gridItemPanel.Insert(0, 0, randomizeDarkness);
        gridItemPanel.Insert(0, 1, shatteredLantern);
        gridItemPanel.Insert(1, 0, darknessLevel);
        gridItemPanel.Insert(1, 1, twoDupeShards);
        gridItemPanel.Insert(2, 0, chaos);
        gridItemPanel.ResetNavigation();
    }

    public void ApplySettings(RandomizationSettings settings)
    {
        darknessLevel.Unlock();
        darknessLevel.SetValue(settings.DarknessLevel);
        chaos.Unlock();
        chaos.SetValue(settings.Chaos);
        twoDupeShards.Unlock();
        twoDupeShards.SetValue(settings.TwoDupeShards);

        randomizeDarkness.SetValue(settings.RandomizeDarkness);
        shatteredLantern.SetValue(settings.ShatteredLantern);
    }
}
