using DarknessRandomizer.Data;
using DarknessRandomizer.Lib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using Newtonsoft.Json;
using PurenailCore.ICUtil;
using PurenailCore.SystemUtil;
using SFCore.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DarknessRandomizer.IC;

public class DarknessRandomizerModule : ItemChanger.Modules.Module
{
    public SceneDarknessDict DarknessOverrides = new();
    public int NumLanternShardsCollected = 0;

    [JsonIgnore]
    private readonly List<Action> UnloadHooks = [];
    [JsonIgnore]
    private readonly List<EmbeddedSprite> ShatteredLanternSprites =
    [
        new("ShatteredLantern_1"),
        new("ShatteredLantern_2"),
        new("ShatteredLantern_3"),
    ];

    public override void Initialize()
    {
        InstallHook(new LambdaHook(
            () => Modding.ModHooks.GetPlayerBoolHook += OverrideGetBool,
            () => Modding.ModHooks.GetPlayerBoolHook -= OverrideGetBool));
        InstallHook(new LambdaHook(
            () => Modding.ModHooks.SetPlayerBoolHook += OverrideSetBool,
            () => Modding.ModHooks.SetPlayerBoolHook -= OverrideSetBool));
        InstallHook(new LambdaHook(
            () => Modding.ModHooks.GetPlayerIntHook += OverrideGetInt,
            () => Modding.ModHooks.GetPlayerIntHook -= OverrideGetInt));
        InstallHook(new LambdaHook(
            () => Modding.ModHooks.SetPlayerIntHook += OverrideSetInt,
            () => Modding.ModHooks.SetPlayerIntHook -= OverrideSetInt));

        InstallHook(new FsmEditHook(new("Equipment", "Build Equipment List"), ModifyInventory));
        InstallHook(new LanguageEditHook("UI", "INV_NAME_SHATTERED_LANTERN", "Shattered Lumafly Lantern"));
        InstallHook(new LanguageEditHook("UI", "INV_DESC_SHATTERED_LANTERN_1", "A single shard of the old light. There is more to gather."));
        InstallHook(new LanguageEditHook("UI", "INV_DESC_SHATTERED_LANTERN_2", "A half reconstructed lantern, the cracks partially sealed."));
        InstallHook(new LanguageEditHook("UI", "INV_DESC_SHATTERED_LANTERN_3", "A near complete orb of glass, only the cap remains."));

        InstallHook(new LambdaHook(
            () => PriorityEvents.BeforeSceneManagerStart.Subscribe(100f, BeforeSceneManagerStart),
            () => PriorityEvents.BeforeSceneManagerStart.Unsubscribe(100f, BeforeSceneManagerStart)));
        InstallHook(new LambdaHook(
            () => PriorityEvents.AfterSceneManagerStart.Subscribe(100f, AfterSceneManagerStart),
            () => PriorityEvents.AfterSceneManagerStart.Unsubscribe(100f, AfterSceneManagerStart)));
        InstallHook(new FsmEditHook(new("Darkness Region"), ModifyDarknessRegions));

        // Allow dark objects to be used if the room is bright.
        InstallMaybeDisableLanternCheck(SceneName.CrossroadsPeakDarkToll, new("Toll Gate Machine", "Disable if No Lantern"));
        InstallMaybeDisableLanternCheck(SceneName.CrossroadsPeakDarkToll, new("Toll Gate Machine (1)", "Disable if No Lantern"));
        InstallMaybeDisableLanternCheck(SceneName.GreenpathStoneSanctuary, new("Ghost Warrior NPC", "FSM"));

        // Delete ghost warriors in dark rooms.
        InstallDeleteGhostWarriorIfDark(SceneName.CliffsGorb);
        InstallDeleteGhostWarriorIfDark(SceneName.DeepnestGalienArena);
        InstallDeleteGhostWarriorIfDark(SceneName.EdgeMarkothArena);
        InstallDeleteGhostWarriorIfDark(SceneName.FungalElderHu);
        InstallDeleteGhostWarriorIfDark(SceneName.GardensGardensStag);
        InstallDeleteGhostWarriorIfDark(SceneName.GreenpathStoneSanctuary);
        InstallDeleteGhostWarriorIfDark(SceneName.GroundsXero);

        // Make tollgates unusable in dark rooms.
        InstallDarkTollgateCheck(SceneName.BasinCorridortoBrokenVessel, new("Toll Machine Bench", "Toll Machine Bench"));
        InstallDarkTollgateCheck(SceneName.CityTollBench, new("Toll Machine Bench", "Toll Machine Bench"));
        InstallDarkTollgateCheck(SceneName.GreenpathToll, new("Toll Gate Machine", "Toll Machine"));
        InstallDarkTollgateCheck(SceneName.GreenpathToll, new("Toll Gate Machine (1)", "Toll Machine"));

        // The Shade Soul door is inoperable in the dark.
        InstallElegantKeyDarkCheck();

        // Preserve hazard respawns in combat arenas.
        PreservedHazardRespawns.GetOrAddNew(SceneName.CrossroadsGlowingWombArena).Add("Hazard Respawn Trigger v2 (3)");
        PreservedHazardRespawns.GetOrAddNew(SceneName.FogOvergrownMound).Add("Hazard Respawn Trigger v2");
        PreservedHazardRespawns.GetOrAddNew(SceneName.FogUumuuArena).Add("Hazard Respawn Trigger v2 (6)");
        PreservedHazardRespawns.GetOrAddNew(SceneName.FungalMantisLords).Add("Hazard Respawn Trigger (5)");
        PreservedHazardRespawns.GetOrAddNew(SceneName.CrystalMound).Add("Hazard Respawn Trigger v2 (3)");

        // Install escape hatch for dark Dreamnail cutscene
        InstallDreamnailEscape();
    }

    public override void Unload() => UnloadHooks.ForEach(a => a.Invoke());

    private void InstallHook(IHook h)
    {
        h.Load();
        UnloadHooks.Add(() => h.Unload());
    }

    private bool PlayerHasLantern() => PlayerData.instance.GetBool(nameof(PlayerData.hasLantern));

    private static readonly Dictionary<SceneName, HashSet<string>> PreservedHazardRespawns = [];

    private static void DeactiveGameObject(GameObject obj)
    {
        if (obj == null) return;

        obj.GetOrAddComponent<DeactivateInDarknessWithoutLantern>().enabled = true;
        obj.SetActive(false);
    }

    private void DisableDarkRoomObjects(DarknessData sceneData)
    {
        foreach (var obj in UnityEngine.Object.FindObjectsOfType<HazardRespawnTrigger>())
        {
            if (!PreservedHazardRespawns.TryGetValue(sceneData.CurrentScene, out HashSet<string> names) || !names.Contains(obj.name))
            {
                DeactiveGameObject(obj.gameObject);
            }
        }

        if (sceneData.CurrentScene == SceneName.DreamNail && IsDark(SceneName.DreamNail))
        {
            var door = GameObject.Find("door_dreamReturn");
            var hrm = door.transform.Find("Hazard Respawn Marker");
            var hsl = hrm.GetComponent<HazardRespawnMarker>().GetAttr<HazardRespawnMarker, Vector2>("heroSpawnLocation");
            for (int i = 2; i <= 4; i++)
            {
                var door2 = GameObject.Find($"door_dreamReturn{i}");
                door2.transform.position = door.transform.position;
                var hrm2 = door2.transform.Find("Hazard Respawn Marker");
                hrm2.transform.position = hrm.transform.position;
                hrm2.GetComponent<HazardRespawnMarker>().SetAttr("heroSpawnLocation", hsl);
            }
        }
    }

    private void EnableDisabledLanternObjects()
    {
        foreach (var obj in UnityEngine.Object.FindObjectsOfType<DeactivateInDarknessWithoutLantern>(true))
        {
            obj.enabled = false;
            obj.gameObject.SetActive(true);
        }
    }

    private record DarknessData
    {
        public SceneName CurrentScene;
        public Darkness PrevDarkness;
        public Darkness NewDarkness;

        public Darkness DisplayDarkness
        {
            get
            {
                var ddo = Data.SceneData.Get(CurrentScene).DisplayDarknessOverrides;
                return ddo?.Applies(NewDarkness) ?? false ? ddo.SceneDarkness : NewDarkness;
            }
        }

        public bool Brighter => PrevDarkness >= Darkness.SemiDark && PrevDarkness > NewDarkness;

        public bool Darker => NewDarkness >= Darkness.SemiDark && NewDarkness > PrevDarkness;

        public bool Unchanged => PrevDarkness == NewDarkness;
    }
    private string sceneDataCacheName;
    private DarknessData? sceneDataCache;

    private DarknessData? GetSceneData(string sceneName)
    {
        if (sceneDataCacheName == sceneName) return sceneDataCache;

        sceneDataCache = ComputeSceneData(sceneName);
        sceneDataCacheName = sceneName;
        return sceneDataCache;
    }

    private DarknessData? ComputeSceneData(string sceneName)
    {
        if (SceneName.TryGetValue(sceneName, out SceneName currentScene)
            && DarknessOverrides.TryGetValue(currentScene, out Darkness newDarkness))
        {
            return new()
            {
                CurrentScene = currentScene,
                PrevDarkness = SceneMetadata.Get(currentScene).OrigDarkness,
                NewDarkness = newDarkness
            };
        }

        return null;
    }

    private void BeforeSceneManagerStart(SceneManager sm)
    {
        var data = GetSceneData(sm.gameObject.scene.name);
        if (data?.Unchanged ?? true) return;

        sm.darknessLevel = (int)data.DisplayDarkness;
    }

    private void AfterSceneManagerStart(SceneManager sm)
    {
        var data = GetSceneData(sm.gameObject.scene.name);
        if (data?.Unchanged ?? true) return;

        if (!PlayerHasLantern())
        {
            if (data.NewDarkness == Darkness.Dark)
            {
                DisableDarkRoomObjects(data);
            }
            else
            {
                EnableDisabledLanternObjects();
            }
        }

        // Deploy additional darkness regions.
        if (data.NewDarkness == Darkness.Dark)
        {
            var ddo = Data.SceneData.Get(data.CurrentScene).DisplayDarknessOverrides;
            ddo?.DarknessRegions.ForEach(dr => dr.Deploy());
        }
    }

    private void ModifyDarknessRegions(PlayMakerFSM fsm)
    {
        if (fsm.gameObject.GetComponent<CustomDarknessRegion>() != null) return;

        var data = GetSceneData(fsm.gameObject.scene.name);
        if (data == null) return;

        Darkness? d = fsm.FsmVariables.FindFsmInt("Darkness").Value.ToDarkness();
        if (d == null) return;

        // Disable this darkness region only if our change obsoletes it.
        if (data.Brighter && d > data.NewDarkness || data.Darker && d < data.NewDarkness)
        {
            GetState(fsm, "Init").ClearTransitions();
        }
    }

    private const string TrueBool = "DarknessRandomizerTrue";
    private const string FalseBool = "DarknessRandomizerFalse";

    private bool OverrideGetBool(string name, bool orig) => name switch
    {
        TrueBool => true,
        FalseBool => false,
        _ => orig
    };

    private bool OverrideSetBool(string name, bool orig)
    {
        if (orig && name == nameof(PlayerData.instance.hasLantern))
        {
            NumLanternShardsCollected = LanternShards.TotalNumShards;
        }

        return orig;
    }

    private int OverrideGetInt(string name, int orig) => name == LanternShards.PDName ? NumLanternShardsCollected : orig;

    private int OverrideSetInt(string name, int orig)
    {
        if (name == LanternShards.PDName)
        {
            NumLanternShardsCollected = Math.Min(LanternShards.TotalNumShards, orig);
            return NumLanternShardsCollected;
        }

        return orig;
    }

    private void ModifyInventory(PlayMakerFSM fsm)
    {
        var lantern = fsm.gameObject.FindChild("Lantern");
        var spriteRenderer = lantern.GetComponent<SpriteRenderer>();
        var origSprite = spriteRenderer.sprite;

        var state = fsm.GetFsmState("Lantern");
        var sets = state.GetActionsOfType<SetFsmString>();
        var setName = sets[0];
        var setDesc = sets[1];

        state.RemoveFirstActionOfType<PlayerDataBoolTest>();
        state.AddFirstAction(new Lambda(() =>
        {
            if (PlayerHasLantern())
            {
                setName.setValue.Value = "INV_NAME_LANTERN";
                setDesc.setValue.Value = "INV_DESC_LANTERN";
                spriteRenderer.sprite = origSprite;
                return;
            }
            if (NumLanternShardsCollected == 0)
            {
                fsm.SendEvent("FINISHED");
                return;
            }

            setName.setValue.Value = $"INV_NAME_SHATTERED_LANTERN";
            setDesc.setValue.Value = $"INV_DESC_SHATTERED_LANTERN_{NumLanternShardsCollected}";
            spriteRenderer.sprite = ShatteredLanternSprites[NumLanternShardsCollected - 1].Value;
        }));
    }

    private bool IsDark(SceneName sceneName)
    {
        if (PlayerHasLantern()) return false;

        if (DarknessOverrides.TryGetValue(sceneName, out Darkness d))
        {
            return d == Darkness.Dark;
        }
        return false;
    }

    // Disambig with SFCore
    private static FsmState GetState(PlayMakerFSM fsm, string name) => ItemChanger.Extensions.PlayMakerExtensions.GetState(fsm, name);

    private void InstallMaybeDisableLanternCheck(SceneName sceneName, FsmID id) => InstallHook(new FsmEditHook(sceneName, id, fsm =>
                                                                                            {
                                                                                                if (!IsDark(sceneName))
                                                                                                {
                                                                                                    GetState(fsm, "Check").GetFirstActionOfType<PlayerDataBoolTest>().boolName = TrueBool;
                                                                                                }
                                                                                            }));

    private static readonly Color darkTollColor = new(0.2647f, 0.2647f, 0.2647f);

    private void InstallDarkTollgateCheck(SceneName sceneName, FsmID id)
    {
        InstallHook(new FsmEditHook(sceneName, id, fsm =>
        {
            if (IsDark(sceneName))
            {
                GetState(fsm, "Can inspet?").GetFirstActionOfType<BoolTest>().boolVariable = new FsmBool() { Value = false };
                fsm.gameObject.GetComponent<tk2dSprite>().color = darkTollColor;
            }
        }));
        InstallHook(new FsmEditHook(sceneName, new("Arrow Prompt(Clone)", "Prompt Control"), fsm =>
        {
            if (IsDark(sceneName))
            {
                DeactiveGameObject(fsm.gameObject);
            }
        }));
    }

    private void InstallDeleteGhostWarriorIfDark(SceneName sceneName) => InstallHook(new FsmEditHook(sceneName, new("Ghost Warrior NPC", "Conversation Control"), fsm =>
                                                                                  {
                                                                                      if (IsDark(sceneName))
                                                                                      {
                                                                                          DeactiveGameObject(fsm.gameObject);
                                                                                      }
                                                                                  }));

    private void InstallElegantKeyDarkCheck() => InstallHook(new FsmEditHook(SceneName.CityTollBench, new("Mage Door", "npc_control"), fsm =>
                                                          {
                                                              if (IsDark(SceneName.CityTollBench))
                                                              {
                                                                  GetState(fsm, "Can Talk?").GetFirstActionOfType<BoolTest>().boolVariable = new FsmBool() { Value = false };
                                                                  fsm.gameObject.GetComponent<tk2dSprite>().color = darkTollColor;
                                                                  DeactiveGameObject(GameObject.Find("/Mage Door/Prompt Marker"));
                                                              }
                                                          }));
    private void InstallDreamnailEscape()
    {
        DeployerHook.Test dreamNailDark = () => IsDark(SceneName.DreamNail);
        DeployerHook.Test dreamNailDarkNoLantern = () => dreamNailDark() && !PlayerData.instance.GetBool(nameof(PlayerData.hasLantern));

        InstallHook(new DeployerHook(new DreamnailWarp(), dreamNailDarkNoLantern));
        InstallHook(new DeployerHook(new DreamnailWarpGlow(), dreamNailDarkNoLantern));
        InstallHook(new DeployerHook(new DreamnailWarpTarget(), dreamNailDark));

        InstallHook(new LambdaHook(
            () => On.GameManager.EnterHero += BlockAdditiveGateSearch,
            () => On.GameManager.EnterHero -= BlockAdditiveGateSearch));
    }

    private void BlockAdditiveGateSearch(On.GameManager.orig_EnterHero orig, GameManager gm, bool additiveGateSearch)
    {
        if (!additiveGateSearch || gm.GetAttr<GameManager, string>("entryGateName") == DreamnailWarpTarget.GATE_NAME) orig(gm, false);
        else orig(gm, additiveGateSearch);
    }
}

interface IHook
{
    public void Load();
    public void Unload();
}

class LambdaHook(Action load, Action unload) : IHook
{
    private readonly Action load = load;
    private readonly Action unload = unload;

    public void Load() => load.Invoke();

    public void Unload() => unload.Invoke();
}

class FsmEditHook : LambdaHook
{
    public FsmEditHook(SceneName scene, FsmID id, Action<PlayMakerFSM> action) : base(
        () => Events.AddFsmEdit(scene.Name(), id, action),
        () => Events.RemoveFsmEdit(scene.Name(), id, action))
    { }

    public FsmEditHook(FsmID id, Action<PlayMakerFSM> action) : base(
        () => Events.AddFsmEdit(id, action),
        () => Events.RemoveFsmEdit(id, action))
    { }
}

class LanguageEditHook(string sheet, string name, string text) : IHook
{
    private readonly string sheet = sheet;
    private readonly string name = name;
    private readonly string text = text;

    public void Load() => Events.AddLanguageEdit(new(sheet, name), EditText);
    public void Unload() => Events.RemoveLanguageEdit(new(sheet, name), EditText);

    private void EditText(ref string value) => value = text;
}

class DeployerHook : LambdaHook
{
    public delegate bool Test();

    public DeployerHook(Deployer deployer, Test test) : this(deployer.SceneName, scene => OnSceneLoad(deployer, test, scene)) { }

    private DeployerHook(string sceneName, Action<Scene> action) : base(
        () => Events.AddSceneChangeEdit(sceneName, action),
        () => Events.RemoveSceneChangeEdit(sceneName, action))
    { }

    private static void OnSceneLoad(Deployer deployer, Test test, Scene scene)
    {
        if (deployer.SceneName == scene.name && test()) deployer.OnSceneChange(scene);
    }
}
