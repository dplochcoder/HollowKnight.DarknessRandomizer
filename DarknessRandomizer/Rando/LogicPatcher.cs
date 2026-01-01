using DarknessRandomizer.Data;
using ItemChanger;
using Modding;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerCore.LogicItems.Templates;
using RandomizerCore.StringLogic;
using RandomizerCore.StringParsing;
using RandomizerMod.RC;
using RandomizerMod.Settings;
using System.Collections.Generic;
using System.Linq;

namespace DarknessRandomizer.Rando;

internal class LogicOverrides
{
    private delegate void LogicOverride(LogicManagerBuilder lmb, string logicName, LogicClause lc);

    private readonly Dictionary<string, LogicOverride> logicOverridesByName;
    private readonly Dictionary<SceneName, LogicOverride> logicOverridesBySceneTransition;
    private readonly Dictionary<SceneName, LogicOverride> logicOverridesByUniqueScene;

    public NameToken LanternToken { get; }

    public LogicOverrides(LogicManagerBuilder lmb)
    {
        LanternToken = new NameToken(RandoInterop.LanternTermName);

        logicOverridesByName = new()
        {
            // Dream warriors do not appear in dark rooms without lantern.
            { WaypointName.DefeatedElderHu, CustomSceneLogicEdit(SceneName.FungalElderHu, "FALSE") },
            { WaypointName.DefeatedGalien, CustomSceneLogicEdit(SceneName.DeepnestGalienArena, "FALSE") },
            { WaypointName.DefeatedGorb, CustomSceneLogicEdit(SceneName.CliffsGorb, "FALSE") },
            { WaypointName.DefeatedMarkoth, CustomSceneLogicEdit(SceneName.EdgeMarkothArena, "FALSE") },
            { WaypointName.DefeatedMarmu, CustomSceneLogicEdit(SceneName.GardensGardensStag, "FALSE") },
            { WaypointName.DefeatedNoEyes, CustomSceneLogicEdit(SceneName.GreenpathStoneSanctuary, "FALSE") },

            // Dream bosses are coded specially because the checks are located where the dream nail is swung,
            // but we care whether or not the actual fight room is dark. So we have to account for both rooms.
            { WaypointName.DefeatedFailedChampion, CustomSceneLogicEdit(SceneName.DreamFailedChampion, "FALSE") },
            { WaypointName.DefeatedGreyPrinceZote, CustomSceneLogicEdit(SceneName.DreamGreyPrinceZote, "FALSE") },
            { WaypointName.DefeatedLostKin, CustomSceneLogicEdit(SceneName.DreamLostKin, "DARKROOMS + DIFFICULTSKIPS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedSoulTyrant, CustomSceneLogicEdit(SceneName.DreamSoulTyrant, "DARKROOMS + DIFFICULTSKIPS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedWhiteDefender, CustomSceneLogicEdit(SceneName.DreamWhiteDefender, "DARKROOMS + DIFFICULTSKIPS + PROFICIENTCOMBAT") },

            // Specific checks with difficult platforming.
            { LocationNames.Void_Heart, CustomSceneLogicEdit(SceneName.DreamAbyss, "DARKROOMS + DIFFICULTSKIPS") },

            // QG stag checks are free except for these two.
            { LocationNames.Soul_Totem_Below_Marmu, CustomDarkLogicEdit("DARKROOMS") },
            { $"{SceneName.GardensGardensStag}[top1]", CustomDarkLogicEdit("DARKROOMS") },

            // Basin toll bench requires lantern.
            { "Bench-Basin_Toll", CustomDarkLogicEdit("Bench-Basin_Toll") },

            // Cornifer bench makes the left transition free, but not vice versa.
            { $"{SceneName.GardensCornifer}[left1]", SkipDarkLogicFor("Bench-Gardens_Cornifer") },
            { LocationNames.Queens_Gardens_Map, SkipDarkLogicFor("Bench-Gardens_Cornifer") },

            // These checks are specifically dark-guarded in darkrooms.
            { LocationNames.Rancid_Egg_Blue_Lake, StandardLogicEdit },
            { LocationNames.Mask_Shard_Queens_Station, StandardLogicEdit },

            // Greenpath toll bench requires lantern.
            { "Bench-Greenpath_Toll", CustomDarkLogicEdit("Bench-Greenpath_Toll") },

            // Peaks toll requires lantern.
            { $"{SceneName.CrossroadsPeakDarkToll}[left1]", CustomDarkLogicEdit("FALSE") },
            { $"{SceneName.CrossroadsPeakDarkToll}[right1]", CustomDarkLogicEdit("FALSE") },
            { LocationNames.Geo_Rock_Crystal_Peak_Entrance, ManualLogicEdit($"{SceneName.CrossroadsPeakDarkToll}[right1] + ($DarknessLevel[{SceneName.CrossroadsPeakDarkToll}]<2 | DARKROOMS)") },

            // Dream nail has a custom scene which may be dark.
            { LocationNames.Dream_Nail, CustomSceneLogicEdit(SceneName.DreamNail, "DARKROOMS") },

            // These checks are free from bench-rando benches.
            { LocationNames.Crystal_Heart, SkipDarkLogicFor("Bench-Mining_Golem") },
            { LocationNames.Ismas_Tear, SkipDarkLogicFor("Bench-Isma's_Grove") },
            { LocationNames.Kings_Idol_Deepnest, SkipDarkLogicFor("Bench-Zote's_Folly") },
            { LocationNames.Tram_Pass, SkipDarkLogicFor("Bench-Destroyed_Tram") },

            // These bosses are deemed difficult in the dark.
            { "Defeated_Any_Hollow_Knight", CustomDarkLogicEdit("DARKROOMS + DIFFICULTSKIPS + PROFICIENTCOMBAT") },
            { "Defeated_Any_Nightmare_King", CustomDarkLogicEdit("FALSE") },
            { "Defeated_Any_Radiance", CustomDarkLogicEdit("FALSE") },
            { WaypointName.DefeatedBrokenVessel, CustomDarkLogicEdit("DARKROOMS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedBroodingMawlek, CustomDarkLogicEdit("DARKROOMS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedCollector, CustomDarkLogicEdit("DARKROOMS + DIFFICULTSKIPS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedColosseum1, CustomSceneLogicEdit(SceneName.EdgeColo1Arena, "DARKROOMS + DIFFICULTSKIPS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedColosseum2, CustomSceneLogicEdit(SceneName.EdgeColo2Arena, "FALSE") },
            { "Defeated_Colosseum_3", CustomSceneLogicEdit(SceneName.EdgeColo3Arena, "FALSE") },
            { WaypointName.DefeatedCrystalGuardian, CustomDarkLogicEdit("DARKROOMS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedEnragedGuardian, CustomDarkLogicEdit("DARKROOMS + DIFFICULTSKIPS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedFlukemarm, CustomDarkLogicEdit("DARKROOMS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedHiveKnight, CustomDarkLogicEdit("DARKROOMS + DIFFICULTSKIPS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedHornet2, CustomDarkLogicEdit("DARKROOMS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedGrimm, CustomDarkLogicEdit("DARKROOMS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedMantisLords, CustomDarkLogicEdit("DARKROOMS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedNosk, CustomDarkLogicEdit("DARKROOMS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedPaleLurker, CustomDarkLogicEdit("DARKROOMS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedSoulMaster, CustomDarkLogicEdit("DARKROOMS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedTraitorLord, CustomDarkLogicEdit("DARKROOMS + DIFFICULTSKIPS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedUumuu, CustomDarkLogicEdit("DARKROOMS + PROFICIENTCOMBAT") },
            { WaypointName.DefeatedWatcherKnights, CustomDarkLogicEdit("DARKROOMS + DIFFICULTSKIPS + PROFICIENTCOMBAT") },

            // These two checks in the city toll room require lantern. All else is free.
            { "Bench-City_Toll", CustomDarkLogicEdit("Bench-City_Toll") },
            { $"{SceneName.CityTollBench.Name()}[left3]", CustomDarkLogicEdit("FALSE") }
        };

        FreeDarkroomsClique($"{SceneName.AbyssLighthouseClimb}[right3]", "Bench-Abyss_Workshop");
        FreeDarkroomsClique($"{SceneName.BasinBrokenVesselGrub}[bot2]", "Bench-Far_Basin");
        FreeDarkroomsClique($"{SceneName.BasinFountain}[top1]", "Bench-Basin_Hub");
        FreeDarkroomsClique($"{SceneName.CityGrubBelowKings}[left1]", "Bench-Flooded_Stag");
        FreeDarkroomsClique($"{SceneName.CityOutsideNailsmith}[door1]", "Bench-Nailsmith");
        FreeDarkroomsClique($"{SceneName.CityQuirrelBench}[bot1]", "Bench-Quirrel");
        FreeDarkroomsClique($"{SceneName.CitySanctumSpellTwister}[right1]", "Bench-Inner_Sanctum");
        FreeDarkroomsClique($"{SceneName.CliffsMain}[right1]", "Bench-Cliffs_Overhang");
        FreeDarkroomsClique($"{SceneName.CliffsMain}[right3]", "Bench-Blasted_Plains");
        FreeDarkroomsClique($"{SceneName.CrossroadsGreenpathEntrance}[right1]", "Bench-Pilgrim's_Start");
        FreeDarkroomsClique($"{SceneName.CrossroadsGruzMother}[door_charmshop]", $"{SceneName.CrossroadsGruzMother}[right1]", "Bench-Salubra");
        FreeDarkroomsClique($"{SceneName.CrystalDarkBench}[left1]", "Bench-Peak_Dark_Room");
        FreeDarkroomsClique($"{SceneName.CrystalGuardianBench}[left1]", $"{SceneName.CrystalGuardianBench}[right1]", "Bench-Crystal_Guardian");
        FreeDarkroomsClique($"{SceneName.DeepnestLowerCornifer}[top2]", $"{SceneName.DeepnestLowerCornifer}[right1]", "Bench-Deepnest_Gate");
        FreeDarkroomsClique($"{SceneName.DeepnestHotSpring}[left1]", $"{SceneName.DeepnestHotSpring}[right1]", "Bench-Deepnest_Hot_Springs");
        FreeDarkroomsClique($"{SceneName.DeepnestWeaversDen}[left1]", "Bench-Bench-Weaver's_Den");
        FreeDarkroomsClique($"{SceneName.DeepnestNoskArena}[left1]", "Bench-Nosk's_Lair");
        FreeDarkroomsClique($"{SceneName.DeepnestZoteArena}[bot1]", "Bench-Zote's_Folly");
        FreeDarkroomsClique($"{SceneName.EdgeHornetSentinelArena}[left1]", "Bench-Hornet's_Outpost");
        FreeDarkroomsClique($"{SceneName.EdgeOutsideOro}[door1]", $"{SceneName.EdgeOutsideOro}[right1]", "Bench-Oro");
        FreeDarkroomsClique($"{SceneName.EdgePaleLurker}[left1]", "Bench-Lurker's_Overlook");
        FreeDarkroomsClique($"{SceneName.EdgeWhisperingRoot}[left1]", "Bench-Edge_Summit");
        FreeDarkroomsClique($"{SceneName.FogOvergrownMound}[left1]", "Bench-Overgrown_Mound");
        FreeDarkroomsClique($"{SceneName.FungalBrettaBench}[left3]", "Bench-Bretta");
        FreeDarkroomsClique($"{SceneName.FungalCoreUpper}[right1]", "Bench-Fungal_Core");
        FreeDarkroomsClique($"{SceneName.GardensBeforePetraArena}[left1]", "Bench-Gardens_Atrium");
        FreeDarkroomsClique($"{SceneName.GreenpathSheo}[door1]", "Bench-Sheo");
        FreeDarkroomsClique($"{SceneName.GreenpathSheoGauntlet}[right1]", "Bench-Duranda's_Trial");
        FreeDarkroomsClique($"{SceneName.GroundsCrypts}[top1]", "Bench-Crypts");
        FreeDarkroomsClique($"{SceneName.GroundsSpiritsGlade}[left1]", "Bench-Spirits'_Glade");
        FreeDarkroomsClique($"{SceneName.HiveOutsideShortcut}[right2]", "Bench-Hive_Hideaway");
        FreeDarkroomsClique($"{SceneName.KingsPass}[top2]", "Bench-King's_Pass");
        FreeDarkroomsClique($"{SceneName.WaterwaysHiddenGrub}[bot1]", $"{SceneName.WaterwaysHiddenGrub}[right1]");

        logicOverridesBySceneTransition = new()
        {
            // This gets overridden for bench rando.
            { SceneName.GreenpathToll, CustomDarkLogicEdit("FALSE") },

            // The following scenes are trivial to navigate while dark, but may contain a check which is
            // uniquely affected by darkness.
            { SceneName.FungalQueensStation, NoLogicEdit }
        };

        logicOverridesByUniqueScene = new()
        {
            // Checks in these scenes are free, even if dark.
            { SceneName.CityTollBench, CustomDarkLogicEdit("ANY") },
            // Gardens checks by the stag are free; marmu, marmu totem, and the upper transition are exceptions.
            { SceneName.GardensGardensStag, CustomDarkLogicEdit("ANY") },
            { SceneName.GroundsBlueLake, CustomDarkLogicEdit("ANY") },

            // These scenes have difficult dark platforming.
            { SceneName.CrystalCrystalHeartGauntlet, CustomDarkLogicEdit("DARKROOMS + DIFFICULTSKIPS") },
            { SceneName.CrystalDeepFocusGauntlet, CustomDarkLogicEdit("DARKROOMS + DIFFICULTSKIPS") },
            { SceneName.EdgeWhisperingRoot, CustomDarkLogicEdit("DARKROOMS + DIFFICULTSKIPS") },
            { SceneName.GreenpathSheoGauntlet, CustomDarkLogicEdit("DARKROOMS + DIFFICULTSKIPS") },
            { SceneName.POPEntrance, CustomDarkLogicEdit("DARKROOMS + DIFFICULTSKIPS") },
            { SceneName.POPFinal, CustomDarkLogicEdit("DARKROOMS + DIFFICULTSKIPS") },
            { SceneName.POPLever, CustomDarkLogicEdit("DARKROOMS + DIFFICULTSKIPS") },
            { SceneName.POPVertical, CustomDarkLogicEdit("DARKROOMS + DIFFICULTSKIPS") },
        };

        if (ModHooks.GetMod("BenchRando") is Mod)
        {
            DoBenchRandoInterop(lmb);
        }
    }

    private readonly Dictionary<string, SceneName> customSceneInferences = new()
    {
        { "Queen's_Gardens_Stag", SceneName.GardensGardensStag }
    };
    private readonly Dictionary<string, SceneNameInferrer> sceneNameInferrerOverrides = [];

    private void DoBenchRandoInterop(LogicManagerBuilder lmb)
    {
        foreach (var e in BenchRando.BRData.BenchLookup)
        {
            var benchName = e.Key;
            var def = e.Value;
            if (!SceneName.TryGetValue(def.SceneName, out SceneName sceneName)) continue;

            // Make sure we apply darkness logic to other checks in the room obtainable from the bench.
            customSceneInferences[benchName] = sceneName;

            // Bench checks are obtainable even in dark rooms, if the player has the benchwarp pickup.
            sceneNameInferrerOverrides[benchName] = (string term, out SceneName sceneName) => InferSceneName(term, out sceneName) && term != benchName;
        }

        // Apply custom overrides only applicable in bench rando.
        if (lmb.LogicLookup.ContainsKey("Bench-Greenpath_Toll"))
        {
            logicOverridesByUniqueScene[SceneName.GreenpathToll] = CustomDarkLogicEdit("DARKROOMS | Bench-Greenpath_Toll");
        }
    }

    public SceneNameInferrer GetSceneNameInferrer(string logicName) => sceneNameInferrerOverrides.TryGetValue(logicName, out SceneNameInferrer sni) ? sni : InferSceneName;

    // Don't infer scene names for rooms which cannot be dark.
    private static bool SceneCanBeDark(SceneName sceneName) => Data.SceneData.Get(sceneName).MaximumDarkness >= Darkness.Dark && ClusterData.Get(sceneName).MaximumDarkness(DarknessRandomizer.GS.RandomizationSettings) >= Darkness.Dark;

    private bool InferSceneName(string term, out SceneName sceneName)
    {
        if (SceneName.IsTransition(term, out sceneName))
        {
            return SceneCanBeDark(sceneName);
        }

        if (customSceneInferences.TryGetValue(term, out sceneName))
        {
            return SceneCanBeDark(sceneName);
        }

        sceneName = SceneName.KingsPass;
        return false;
    }

    private bool InferSingleSceneName(LogicClause lc, out SceneName sceneName)
    {
        HashSet<SceneName> ret = [];
        foreach (var token in lc.Expr.GetTokens())
        {
            if (InferSceneName(token.Print(), out SceneName newName))
                ret.Add(newName);
        }

        if (ret.Count == 1)
        {
            sceneName = ret.Single();
            return true;
        }

        sceneName = SceneName.KingsPass;
        return false;
    }

    private static readonly Expression<LogicExpressionType> DarkroomsExpression = LogicExpressionUtil.Parse("DARKROOMS");

    public void EditLogicClause(LogicManagerBuilder lmb, string logicName, LogicClause lc)
    {
        if (logicOverridesByName.TryGetValue(logicName, out LogicOverride handler))
        {
            handler.Invoke(lmb, logicName, lc);
            return;
        }

        if (InferSceneName(logicName, out SceneName sceneName)
            && logicOverridesBySceneTransition.TryGetValue(sceneName, out handler))
        {
            handler.Invoke(lmb, logicName, lc);
            return;
        }

        // Check for an inferred scene match.
        if (InferSingleSceneName(lc, out SceneName inferred) && logicOverridesByUniqueScene.TryGetValue(inferred, out handler))
        {
            handler.Invoke(lmb, logicName, lc);
            return;
        }

        // Do the standard logic edit.
        StandardLogicEdit(lmb, logicName, lc);
    }

    // Specifies that each logic name given is accessible from the others even in darkrooms.
    private void FreeDarkroomsClique(params string[] logicNames)
    {
        HashSet<string> all = [.. logicNames];
        foreach (var ln in logicNames)
        {
            HashSet<string> others = [.. all];
            others.Remove(ln);
            logicOverridesByName[ln] = SkipDarkLogicFor([.. others]);
        }
    }

    private readonly Dictionary<string, Expression<LogicExpressionType>> logicCache = [];

    private Expression<LogicExpressionType> GetCachedLogic(string logic) => logicCache.TryGetValue(logic, out var expr) ? expr : (logicCache[logic] = LogicExpressionUtil.Parse(logic));

    private void NoLogicEdit(LogicManagerBuilder lmb, string logicName, LogicClause lc) { }

    private void StandardLogicEdit(LogicManagerBuilder lmb, string logicName, LogicClause lc) =>
        LogicClauseEditor.EditDarkness(lmb, logicName, GetSceneNameInferrer(logicName), LanternToken, DarkroomsExpression);

    private LogicOverride ManualLogicEdit(string logic) => (lmb, logicName, lc) => lmb.LogicLookup[logicName] = new(logic);

    private LogicOverride CustomDarkLogicEdit(string darkLogic) => (lmb, logicName, lc) => LogicClauseEditor.EditDarkness(lmb, logicName, GetSceneNameInferrer(logicName), LanternToken, GetCachedLogic(darkLogic));

    private LogicOverride SkipDarkLogicFor(params string[] locTerms)
    {
        HashSet<string> set = [.. locTerms];
        return (lmb, logicName, lc) =>
        {
            var orig = GetSceneNameInferrer(logicName);
            bool inferScene(string term, out SceneName sceneName)
            {
                if (set.Contains(term))
                {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    sceneName = default;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    return false;
                }
                return orig(term, out sceneName);
            }

            LogicClauseEditor.EditDarkness(lmb, logicName, inferScene, LanternToken, DarkroomsExpression);
        };
    }

    private LogicOverride CustomSceneLogicEdit(SceneName sceneName, string darkLogic) => (lmb, logicName, lc) =>
                                                                                                  {
                                                                                                      StandardLogicEdit(lmb, logicName, lc);
                                                                                                      if (SceneCanBeDark(sceneName))
                                                                                                      {
                                                                                                          lmb.DoLogicEdit(new(logicName, $"ORIG + ($DarknessLevel[{sceneName}]<2 | {LanternToken.Content} | {darkLogic})"));
                                                                                                      }
                                                                                                  };
}

internal static class LogicPatcher
{
    public static void Setup() => RCData.RuntimeLogicOverride.Subscribe(60f, ModifyLMB);

    public static void ModifyLMB(GenerationSettings gs, LogicManagerBuilder lmb)
    {
        if (RandoInterop.ShatteredLantern)
        {
            var shardsTerm = lmb.GetOrAddTerm("LANTERNSHARDS");
            var lanternTerm = lmb.GetOrAddTerm(RandoInterop.LanternTermName);
            lmb.AddItem(new BranchedItemTemplate(RandoInterop.LanternShardItemName, $"{shardsTerm.Name}<3",
                new SingleItem("LanternShard-GetShard", new(shardsTerm, 1)),
                new CappedItem("LanternShard-GetLantern", [new(shardsTerm, 1), new(lanternTerm, 1)], new(lanternTerm, 1))));
        }

        if (!RandoInterop.RandomizeDarkness) return;

        lmb.VariableResolver = new DarknessVariableResolver(lmb.VariableResolver);
        LogicOverrides overrides = new(lmb);

        // We want to generically modify logic by location (SceneName), but unfortunately the LogicManager is constructed
        // before any of the location info is provided via the RequestBuilder, so we have to be creative.
        //
        // We'll inspect every logic clause for the set of scene transitions it references. If there is only one scene,
        // we will update the logic accordingly. If there are zero scenes, we ignore it, and if there are two or more, we
        // require custom handling.
        //
        // We defer the edits to avoid messing with dictionary iteration order.
        List<string> names = [.. lmb.LogicLookup.Keys];
        names.ForEach(n => overrides.EditLogicClause(lmb, n, lmb.LogicLookup[n]));

        // Darkness can block routes around infection walls in crossroads.
        lmb.DoLogicEdit(new("Crossroads_10[left1]", "ORIG + (ROOMRANDO | Crossroads_06[left1])"));
        lmb.DoLogicEdit(new("Crossroads_06[right1]", "ORIG + (ROOMRANDO | Defeated_False_Knight)"));
        lmb.DoLogicEdit(new("Crossroads_03[bot1]", "ORIG + (ROOMRANDO | Crossroads_19[left1])"));
        lmb.DoLogicEdit(new("Crossroads_19[top1]", "ORIG + (ROOMRANDO | Crossroads_03[right1])"));

        if (ModHooks.GetMod("MoreDoors") is Mod) MoreDoorsInterop(gs, lmb);
    }

    private static void MoreDoorsInterop(GenerationSettings gs, LogicManagerBuilder lmb)
    {
        if (!MoreDoors.Rando.RandoInterop.IsEnabled) return;

        foreach (var doorName in MoreDoors.Rando.RandoInterop.LS.EnabledDoorNames)
        {
            var data = MoreDoors.Data.DoorData.GetDoor(doorName)!;

            var leftGate = data.Door!.LeftLocation!.TransitionName;
            if (SceneName.IsTransition(leftGate, out var leftScene))
            {
                lmb.DoLogicEdit(new(leftGate, $"ORIG + ({RandoInterop.LanternTermName} | $DarknessLevel[{leftScene}] < 2)"));
            }

            var rightGate = data.Door!.RightLocation!.TransitionName;
            if (SceneName.IsTransition(rightGate, out var rightScene))
            {
                lmb.DoLogicEdit(new(rightGate, $"ORIG + ({RandoInterop.LanternTermName} | $DarknessLevel[{rightScene}] < 2)"));
            }
        }
    }
}
