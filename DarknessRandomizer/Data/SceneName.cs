using DarknessRandomizer.Lib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DarknessRandomizer.Data;

[JsonConverter(typeof(SceneNameConverter))]
public class SceneName : ITypedId, IComparable<SceneName>
{
    private class FactoryImpl : ITypedIdFactory<SceneName>
    {
        public int Count() => scenesById.Count;
        public SceneName FromId(int id) => SceneName.FromId(id);
        public SceneName FromName(string name) => SceneName.FromName(name);
    }

    public static readonly ITypedIdFactory<SceneName> Factory = new FactoryImpl();

    private static readonly Dictionary<string, SceneName> scenesByName = [];
    private static readonly List<SceneName> scenesById = [];
    private static int NextId = 0;

    private readonly string name;
    private readonly int id;

    private SceneName(string name)
    {
        if (scenesByName.ContainsKey(name))
        {
            throw new ArgumentException($"Duplicate scene name: {name}");
        }

        this.name = name;
        id = NextId++;
        scenesByName.Add(name, this);
        scenesById.Add(this);
    }

    public int Id() => id;

    public string Name() => name;

    public static bool TryGetValue(string s, out SceneName sceneName) => scenesByName.TryGetValue(s, out sceneName);

    public static SceneName FromName(string name)
    {
        if (!TryGetValue(name, out SceneName scene))
        {
            throw new ArgumentException($"Not a SceneName: {name}");
        }
        return scene;
    }

    public static SceneName FromId(int id) => scenesById[id];

    public static bool IsScene(string s) => scenesByName.ContainsKey(s);

    public override string ToString() => name;

    public static IReadOnlyList<SceneName> All() => scenesById;

    public int CompareTo(SceneName other) => Math.Sign(id - other.id);

    public override bool Equals(object obj) => obj is SceneName name &&
               id == name.id;

    public override int GetHashCode() => id;

    public static bool IsTransition(string token, out SceneName sceneName)
    {
        int i = token.IndexOf('[');
        if (i != -1)
        {
            string sn = token.Substring(0, i);
            if (sn.EndsWith("_Proxy"))
            {
                return TryGetValue(sn.Substring(0, i - 6), out sceneName);
            }
            else
            {
                return TryGetValue(token.Substring(0, i), out sceneName);
            }
        }

        sceneName = default;
        return false;
    }

    // @@@ INSERT_SCENE_NAMES START @@@
    public static readonly SceneName CityBrokenElevator = new("Abyss_01");
    public static readonly SceneName BasinBrokenBridge = new("Abyss_02");
    public static readonly SceneName BasinTram = new("Abyss_03");
    public static readonly SceneName DeepnestTram = new("Abyss_03_b");
    public static readonly SceneName EdgeTram = new("Abyss_03_c");
    public static readonly SceneName BasinFountain = new("Abyss_04");
    public static readonly SceneName BasinPalaceGrounds = new("Abyss_05");
    public static readonly SceneName AbyssCore = new("Abyss_06_Core");
    public static readonly SceneName AbyssLifebloodCore = new("Abyss_08");
    public static readonly SceneName AbyssLighthouseClimb = new("Abyss_09");
    public static readonly SceneName AbyssShadeCloak = new("Abyss_10");
    public static readonly SceneName AbyssShriek = new("Abyss_12");
    public static readonly SceneName AbyssBirthplace = new("Abyss_15");
    public static readonly SceneName AbyssCorridortoLighthouse = new("Abyss_16");
    public static readonly SceneName BasinCloth = new("Abyss_17");
    public static readonly SceneName BasinCorridortoBrokenVessel = new("Abyss_18");
    public static readonly SceneName BasinBrokenVesselGrub = new("Abyss_19");
    public static readonly SceneName BasinSimpleKey = new("Abyss_20");
    public static readonly SceneName BasinMonarchWings = new("Abyss_21");
    public static readonly SceneName BasinHiddenStation = new("Abyss_22");
    public static readonly SceneName AbyssLighthouse = new("Abyss_Lighthouse_room");
    public static readonly SceneName CliffsMain = new("Cliffs_01");
    public static readonly SceneName CliffsGorb = new("Cliffs_02");
    public static readonly SceneName CliffsStagNest = new("Cliffs_03");
    public static readonly SceneName CliffsJonisDark = new("Cliffs_04");
    public static readonly SceneName CliffsJonisPickup = new("Cliffs_05");
    public static readonly SceneName CliffsGrimmLantern = new("Cliffs_06");
    public static readonly SceneName CrossroadsWell = new("Crossroads_01");
    public static readonly SceneName CrossroadsOutsideTemple = new("Crossroads_02");
    public static readonly SceneName CrossroadsOutsideStag = new("Crossroads_03");
    public static readonly SceneName CrossroadsGruzMother = new("Crossroads_04");
    public static readonly SceneName CrossroadsCentralGrub = new("Crossroads_05");
    public static readonly SceneName CrossroadsOutsideMound = new("Crossroads_06");
    public static readonly SceneName CrossroadsGruzzer = new("Crossroads_07");
    public static readonly SceneName CrossroadsAspidArena = new("Crossroads_08");
    public static readonly SceneName CrossroadsMawlekBoss = new("Crossroads_09");
    public static readonly SceneName CrossroadsFalseKnightArena = new("Crossroads_10");
    public static readonly SceneName CrossroadsGreenpathEntrance = new("Crossroads_11_alt");
    public static readonly SceneName CrossroadsCorridortoAcidGrub = new("Crossroads_12");
    public static readonly SceneName CrossroadsGoamMaskShard = new("Crossroads_13");
    public static readonly SceneName CrossroadsOutsideMyla = new("Crossroads_14");
    public static readonly SceneName CrossroadsCorridortoTram = new("Crossroads_15");
    public static readonly SceneName CrossroadsAboveLever = new("Crossroads_16");
    public static readonly SceneName CrossroadsFungalEntrance = new("Crossroads_18");
    public static readonly SceneName CrossroadsBeforeGruzMother = new("Crossroads_19");
    public static readonly SceneName CrossroadsOutsideFalseKnight = new("Crossroads_21");
    public static readonly SceneName CrossroadsGlowingWombArena = new("Crossroads_22");
    public static readonly SceneName CrossroadsMawlekEntrance = new("Crossroads_25");
    public static readonly SceneName CrossroadsOutsideTram = new("Crossroads_27");
    public static readonly SceneName CrossroadsHotSpring = new("Crossroads_30");
    public static readonly SceneName CrossroadsSpikeGrub = new("Crossroads_31");
    public static readonly SceneName CrossroadsCornifer = new("Crossroads_33");
    public static readonly SceneName CrossroadsAcidGrub = new("Crossroads_35");
    public static readonly SceneName CrossroadsMawlekMiddle = new("Crossroads_36");
    public static readonly SceneName CrossroadsVesselFragment = new("Crossroads_37");
    public static readonly SceneName CrossroadsCorridorRightofTemple = new("Crossroads_39");
    public static readonly SceneName CrossroadsCorridorRightofCentralGrub = new("Crossroads_40");
    public static readonly SceneName CrossroadsRightOfMaskShard = new("Crossroads_42");
    public static readonly SceneName CrossroadsCorridortoElevator = new("Crossroads_43");
    public static readonly SceneName CrossroadsMyla = new("Crossroads_45");
    public static readonly SceneName CrossroadsTram = new("Crossroads_46");
    public static readonly SceneName GroundsTram = new("Crossroads_46b");
    public static readonly SceneName CrossroadsStag = new("Crossroads_47");
    public static readonly SceneName CrossroadsGuardedGrub = new("Crossroads_48");
    public static readonly SceneName CrossroadsElevator = new("Crossroads_49");
    public static readonly SceneName CityLeftElevator = new("Crossroads_49b");
    public static readonly SceneName GroundsBlueLake = new("Crossroads_50");
    public static readonly SceneName CrossroadsGoamJournal = new("Crossroads_52");
    public static readonly SceneName CrossroadsAncestralMound = new("Crossroads_ShamanTemple");
    public static readonly SceneName FungalDeepnestFall = new("Deepnest_01");
    public static readonly SceneName DeepnestUpperCornifer = new("Deepnest_01b");
    public static readonly SceneName DeepnestOutsideMimics = new("Deepnest_02");
    public static readonly SceneName DeepnestLeftOfHotSpring = new("Deepnest_03");
    public static readonly SceneName DeepnestDistantVillageStag = new("Deepnest_09");
    public static readonly SceneName DeepnestDistantVillage = new("Deepnest_10");
    public static readonly SceneName DeepnestFailedTramwayBench = new("Deepnest_14");
    public static readonly SceneName DeepnestAfterLowerCornifer = new("Deepnest_16");
    public static readonly SceneName DeepnestGarpedeCorridorBelowCornifer = new("Deepnest_17");
    public static readonly SceneName DeepnestFailedTramwayLifeblood = new("Deepnest_26");
    public static readonly SceneName DeepnestTramPass = new("Deepnest_26b");
    public static readonly SceneName DeepnestHotSpring = new("Deepnest_30");
    public static readonly SceneName DeepnestNoskCorridor = new("Deepnest_31");
    public static readonly SceneName DeepnestNoskArena = new("Deepnest_32");
    public static readonly SceneName DeepnestZoteArena = new("Deepnest_33");
    public static readonly SceneName DeepnestFirstDevout = new("Deepnest_34");
    public static readonly SceneName DeepnestOutsideGalien = new("Deepnest_35");
    public static readonly SceneName DeepnestMimics = new("Deepnest_36");
    public static readonly SceneName DeepnestCorridortoTram = new("Deepnest_37");
    public static readonly SceneName DeepnestVesselFragment = new("Deepnest_38");
    public static readonly SceneName DeepnestWhisperingRoot = new("Deepnest_39");
    public static readonly SceneName DeepnestGalienArena = new("Deepnest_40");
    public static readonly SceneName DeepnestMidwife = new("Deepnest_41");
    public static readonly SceneName DeepnestOutsideMaskMaker = new("Deepnest_42");
    public static readonly SceneName GardensCorridorToDeepnest = new("Deepnest_43");
    public static readonly SceneName DeepnestSharpShadow = new("Deepnest_44");
    public static readonly SceneName DeepnestWeaversDen = new("Deepnest_45_v02");
    public static readonly SceneName EdgeLeftOfHive = new("Deepnest_East_01");
    public static readonly SceneName EdgeAboveHive = new("Deepnest_East_02");
    public static readonly SceneName EdgeEntrance = new("Deepnest_East_03");
    public static readonly SceneName EdgeBardoon = new("Deepnest_East_04");
    public static readonly SceneName EdgeOutsideOro = new("Deepnest_East_06");
    public static readonly SceneName EdgeWhisperingRoot = new("Deepnest_East_07");
    public static readonly SceneName EdgeGreatHopperIdol = new("Deepnest_East_08");
    public static readonly SceneName EdgeCorridorOutsideColosseum = new("Deepnest_East_09");
    public static readonly SceneName EdgeMarkothArena = new("Deepnest_East_10");
    public static readonly SceneName EdgeBelowCampBench = new("Deepnest_East_11");
    public static readonly SceneName EdgeHornetSentinelCorridor = new("Deepnest_East_12");
    public static readonly SceneName EdgeCampBench = new("Deepnest_East_13");
    public static readonly SceneName EdgeBelowOro = new("Deepnest_East_14");
    public static readonly SceneName EdgeQuickSlash = new("Deepnest_East_14b");
    public static readonly SceneName EdgeLifeblood = new("Deepnest_East_15");
    public static readonly SceneName EdgeOroScarecrow = new("Deepnest_East_16");
    public static readonly SceneName Edge420GeoRock = new("Deepnest_East_17");
    public static readonly SceneName EdgeOutsideMarkoth = new("Deepnest_East_18");
    public static readonly SceneName EdgeHornetSentinelArena = new("Deepnest_East_Hornet");
    public static readonly SceneName DeepnestBeastsDen = new("Deepnest_Spider_Town");
    public static readonly SceneName DreamFailedChampion = new("Dream_01_False_Knight");
    public static readonly SceneName DreamSoulTyrant = new("Dream_02_Mage_Lord");
    public static readonly SceneName DreamLostKin = new("Dream_03_Infected_Knight");
    public static readonly SceneName DreamWhiteDefender = new("Dream_04_White_Defender");
    public static readonly SceneName DreamAbyss = new("Dream_Abyss");
    public static readonly SceneName EggRadiance = new("Dream_Final");
    public static readonly SceneName DreamGreyPrinceZote = new("Dream_Mighty_Zote");
    public static readonly SceneName DreamNail = new("Dream_Nailcollection");
    public static readonly SceneName GreenpathEntrance = new("Fungus1_01");
    public static readonly SceneName GreenpathWaterfallBench = new("Fungus1_01b");
    public static readonly SceneName GreenpathFirstHornetSighting = new("Fungus1_02");
    public static readonly SceneName GreenpathStorerooms = new("Fungus1_03");
    public static readonly SceneName GreenpathHornet = new("Fungus1_04");
    public static readonly SceneName GreenpathOutsideThorns = new("Fungus1_05");
    public static readonly SceneName GreenpathCornifer = new("Fungus1_06");
    public static readonly SceneName GreenpathOutsideHunter = new("Fungus1_07");
    public static readonly SceneName GreenpathHunter = new("Fungus1_08");
    public static readonly SceneName GreenpathSheoGauntlet = new("Fungus1_09");
    public static readonly SceneName GreenpathAcidBridge = new("Fungus1_10");
    public static readonly SceneName GreenpathAboveFogCanyon = new("Fungus1_11");
    public static readonly SceneName GreenpathMMCCorridor = new("Fungus1_12");
    public static readonly SceneName GreenpathWhisperingRoot = new("Fungus1_13");
    public static readonly SceneName GreenpathThornsofAgony = new("Fungus1_14");
    public static readonly SceneName GreenpathOutsideSheo = new("Fungus1_15");
    public static readonly SceneName GreenpathStag = new("Fungus1_16_alt");
    public static readonly SceneName GreenpathChargerCorridor = new("Fungus1_17");
    public static readonly SceneName GreenpathAboveSanctuaryBench = new("Fungus1_19");
    public static readonly SceneName GreenpathVengeflyKing = new("Fungus1_20_v02");
    public static readonly SceneName GreenpathOutsideHornet = new("Fungus1_21");
    public static readonly SceneName GreenpathOutsideStag = new("Fungus1_22");
    public static readonly SceneName GardensFirstLoodleCorridor = new("Fungus1_23");
    public static readonly SceneName GardensCornifer = new("Fungus1_24");
    public static readonly SceneName GreenpathCorridortoUnn = new("Fungus1_25");
    public static readonly SceneName GreenpathLakeOfUnn = new("Fungus1_26");
    public static readonly SceneName CliffsBaldursShell = new("Fungus1_28");
    public static readonly SceneName GreenpathMassiveMossCharger = new("Fungus1_29");
    public static readonly SceneName GreenpathBelowTollBench = new("Fungus1_30");
    public static readonly SceneName GreenpathToll = new("Fungus1_31");
    public static readonly SceneName GreenpathMossKnightArena = new("Fungus1_32");
    public static readonly SceneName GreenpathStoneSanctuaryEntrance = new("Fungus1_34");
    public static readonly SceneName GreenpathStoneSanctuary = new("Fungus1_35");
    public static readonly SceneName GreenpathSanctuaryBench = new("Fungus1_37");
    public static readonly SceneName GreenpathUnn = new("Fungus1_Slug");
    public static readonly SceneName FungalQueensStation = new("Fungus2_01");
    public static readonly SceneName FungalQueensStag = new("Fungus2_02");
    public static readonly SceneName FungalOutsideQueens = new("Fungus2_03");
    public static readonly SceneName FungalBelowOgres = new("Fungus2_04");
    public static readonly SceneName FungalShrumalOgres = new("Fungus2_05");
    public static readonly SceneName FungalOutsideLegEater = new("Fungus2_06");
    public static readonly SceneName FungalShrumalWarriorAcidBridge = new("Fungus2_07");
    public static readonly SceneName FungalOutsideElderHu = new("Fungus2_08");
    public static readonly SceneName FungalClothCorridor = new("Fungus2_09");
    public static readonly SceneName FungalLeftOfPilgrimsWay = new("Fungus2_10");
    public static readonly SceneName FungalEpogo = new("Fungus2_11");
    public static readonly SceneName FungalMantisCorridor = new("Fungus2_12");
    public static readonly SceneName FungalBrettaBench = new("Fungus2_13");
    public static readonly SceneName FungalMantisVillage = new("Fungus2_14");
    public static readonly SceneName FungalMantisLords = new("Fungus2_15");
    public static readonly SceneName FungalAboveMantisVillage = new("Fungus2_17");
    public static readonly SceneName FungalCornifer = new("Fungus2_18");
    public static readonly SceneName FungalRightOfSporeShroom = new("Fungus2_19");
    public static readonly SceneName FungalSporeShroom = new("Fungus2_20");
    public static readonly SceneName FungalPilgrimsWay = new("Fungus2_21");
    public static readonly SceneName FungalDashmaster = new("Fungus2_23");
    public static readonly SceneName DeepnestLowerCornifer = new("Fungus2_25");
    public static readonly SceneName FungalLegEater = new("Fungus2_26");
    public static readonly SceneName FungalShrumalWarriorLoop = new("Fungus2_28");
    public static readonly SceneName FungalCoreUpper = new("Fungus2_29");
    public static readonly SceneName FungalCoreLower = new("Fungus2_30");
    public static readonly SceneName FungalMantisRewards = new("Fungus2_31");
    public static readonly SceneName FungalElderHu = new("Fungus2_32");
    public static readonly SceneName FungalLegEaterRoot = new("Fungus2_33");
    public static readonly SceneName FungalWilloh = new("Fungus2_34");
    public static readonly SceneName FogUpperWestTall = new("Fungus3_01");
    public static readonly SceneName FogLowerWestTall = new("Fungus3_02");
    public static readonly SceneName FogQueensGardensAcidEntrance = new("Fungus3_03");
    public static readonly SceneName GardensBeforePetraArena = new("Fungus3_04");
    public static readonly SceneName GardensPetraArena = new("Fungus3_05");
    public static readonly SceneName GardensLowerPetraCorridor = new("Fungus3_08");
    public static readonly SceneName GardensMainArena = new("Fungus3_10");
    public static readonly SceneName GardensWhisperingRoot = new("Fungus3_11");
    public static readonly SceneName GardensOutsideStag = new("Fungus3_13");
    public static readonly SceneName GardensCorridortoTraitorLord = new("Fungus3_21");
    public static readonly SceneName GardensUpperGrub = new("Fungus3_22");
    public static readonly SceneName GardensTraitorLordArena = new("Fungus3_23");
    public static readonly SceneName FogCorridortoOvergrownMound = new("Fungus3_24");
    public static readonly SceneName FogCornifer = new("Fungus3_25");
    public static readonly SceneName FogCorridortoCornifer = new("Fungus3_25b");
    public static readonly SceneName FogEastTall = new("Fungus3_26");
    public static readonly SceneName FogCorridortoArchives = new("Fungus3_27");
    public static readonly SceneName FogCharmNotch = new("Fungus3_28");
    public static readonly SceneName FogLifeblood = new("Fungus3_30");
    public static readonly SceneName GardensEntrance = new("Fungus3_34");
    public static readonly SceneName FogMillibelle = new("Fungus3_35");
    public static readonly SceneName GardensMossProphet = new("Fungus3_39");
    public static readonly SceneName GardensGardensStag = new("Fungus3_40");
    public static readonly SceneName FogOvergrownMoundEntrance = new("Fungus3_44");
    public static readonly SceneName FogArchivesEntrance = new("Fungus3_47");
    public static readonly SceneName GardensOutsideWhiteLady = new("Fungus3_48");
    public static readonly SceneName GardensTraitorsChildsGrave = new("Fungus3_49");
    public static readonly SceneName GardensTollBench = new("Fungus3_50");
    public static readonly SceneName FogArchivesBench = new("Fungus3_archive");
    public static readonly SceneName FogUumuuArena = new("Fungus3_archive_02");
    public static readonly SceneName EdgePaleLurker = new("GG_Lurker");
    public static readonly SceneName WaterwaysFlukemungaCorridor = new("GG_Pipeway");
    public static readonly SceneName WaterwaysJunkPit = new("GG_Waterways");
    public static readonly SceneName GrimmTent = new("Grimm_Main_Tent");
    public static readonly SceneName GrimmNKG = new("Grimm_Nightmare");
    public static readonly SceneName HiveEntranceAndBench = new("Hive_01");
    public static readonly SceneName HiveWhisperingRoot = new("Hive_02");
    public static readonly SceneName HiveOutsideGrub = new("Hive_03");
    public static readonly SceneName HiveOutsideShortcut = new("Hive_03_c");
    public static readonly SceneName HiveMaskShard = new("Hive_04");
    public static readonly SceneName HiveKnightArena = new("Hive_05");
    public static readonly SceneName CrystalDiveEntrance = new("Mines_01");
    public static readonly SceneName CrystalMainEntrance = new("Mines_02");
    public static readonly SceneName CrystalSpikeGrub = new("Mines_03");
    public static readonly SceneName CrystalEntranceConveyors = new("Mines_04");
    public static readonly SceneName CrystalAboveSpikeGrub = new("Mines_05");
    public static readonly SceneName CrystalDeepFocusGauntlet = new("Mines_06");
    public static readonly SceneName CrystalDarkRoom = new("Mines_07");
    public static readonly SceneName CrystalElevatorEntrance = new("Mines_10");
    public static readonly SceneName CrystalLeftOfGuardian = new("Mines_11");
    public static readonly SceneName CrystalTopCorridor = new("Mines_13");
    public static readonly SceneName CrystalMimic = new("Mines_16");
    public static readonly SceneName CrystalCorridortoSpikeGrub = new("Mines_17");
    public static readonly SceneName CrystalGuardianBench = new("Mines_18");
    public static readonly SceneName CrystalGrubCrushers = new("Mines_19");
    public static readonly SceneName CrystalEastTall = new("Mines_20");
    public static readonly SceneName CrystalCrownWhisperingRoot = new("Mines_23");
    public static readonly SceneName CrystalCrownGrub = new("Mines_24");
    public static readonly SceneName CrystalCrownClimb = new("Mines_25");
    public static readonly SceneName CrystalOutsideMound = new("Mines_28");
    public static readonly SceneName CrystalDarkBench = new("Mines_29");
    public static readonly SceneName CrystalCornifer = new("Mines_30");
    public static readonly SceneName CrystalCrystalHeartGauntlet = new("Mines_31");
    public static readonly SceneName CrystalEnragedGuardianArena = new("Mines_32");
    public static readonly SceneName CrossroadsPeakDarkToll = new("Mines_33");
    public static readonly SceneName CrystalCrownPeak = new("Mines_34");
    public static readonly SceneName CrystalMound = new("Mines_35");
    public static readonly SceneName CrystalDeepFocus = new("Mines_36");
    public static readonly SceneName CrystalChestCrushers = new("Mines_37");
    public static readonly SceneName GroundsXero = new("RestingGrounds_02");
    public static readonly SceneName GroundsDreamNailEntrance = new("RestingGrounds_04");
    public static readonly SceneName GroundsWhisperingRoot = new("RestingGrounds_05");
    public static readonly SceneName GroundsCorridorBelowXero = new("RestingGrounds_06");
    public static readonly SceneName GroundsSpiritsGlade = new("RestingGrounds_08");
    public static readonly SceneName GroundsStag = new("RestingGrounds_09");
    public static readonly SceneName GroundsCrypts = new("RestingGrounds_10");
    public static readonly SceneName GroundsDreamshield = new("RestingGrounds_17");
    public static readonly SceneName Bretta = new("Room_Bretta");
    public static readonly SceneName BrettaBasement = new("Room_Bretta_Basement");
    public static readonly SceneName EdgeColoEntrance = new("Room_Colosseum_01");
    public static readonly SceneName EdgeColoBench = new("Room_Colosseum_02");
    public static readonly SceneName EdgeColo1Arena = new("Room_Colosseum_Bronze");
    public static readonly SceneName EdgeColo3Arena = new("Room_Colosseum_Gold");
    public static readonly SceneName EdgeColo2Arena = new("Room_Colosseum_Silver");
    public static readonly SceneName EdgeColoSpectate = new("Room_Colosseum_Spectate");
    public static readonly SceneName EggBench = new("Room_Final_Boss_Atrium");
    public static readonly SceneName EggHollowKnight = new("Room_Final_Boss_Core");
    public static readonly SceneName FogOvergrownMound = new("Room_Fungus_Shaman");
    public static readonly SceneName WaterwaysFlukeHermit = new("Room_GG_Shortcut");
    public static readonly SceneName CliffsMato = new("Room_nailmaster");
    public static readonly SceneName GreenpathSheo = new("Room_nailmaster_02");
    public static readonly SceneName EdgeOro = new("Room_nailmaster_03");
    public static readonly SceneName CrossroadsRescueSly = new("Room_ruinhouse");
    public static readonly SceneName GreenpathUnnBench = new("Room_Slug_Shrine");
    public static readonly SceneName BlackEggTemple = new("Room_temple");
    public static readonly SceneName CityPleasureHouseBench = new("Ruins_Bathhouse");
    public static readonly SceneName CityPleasureHouseElevator = new("Ruins_Elevator");
    public static readonly SceneName CityGuardedGrub = new("Ruins_House_01");
    public static readonly SceneName CityGorgeousHusk = new("Ruins_House_02");
    public static readonly SceneName CityEmilitia = new("Ruins_House_03");
    public static readonly SceneName CityPilgrimsEntrance = new("Ruins1_01");
    public static readonly SceneName CityQuirrelBench = new("Ruins1_02");
    public static readonly SceneName CityRafters = new("Ruins1_03");
    public static readonly SceneName CityOutsideNailsmith = new("Ruins1_04");
    public static readonly SceneName CityGrubAboveLemm = new("Ruins1_05");
    public static readonly SceneName CityLemm = new("Ruins1_05b");
    public static readonly SceneName CityEggAboveLemm = new("Ruins1_05c");
    public static readonly SceneName CityCorridortoStorerooms = new("Ruins1_06");
    public static readonly SceneName CitySoulTwisterArena = new("Ruins1_09");
    public static readonly SceneName CityWhisperingRoot = new("Ruins1_17");
    public static readonly SceneName CityCorridortoSpire = new("Ruins1_18");
    public static readonly SceneName CitySanctumEntrance = new("Ruins1_23");
    public static readonly SceneName CitySoulMasterArena = new("Ruins1_24");
    public static readonly SceneName CitySanctumEastElevators = new("Ruins1_25");
    public static readonly SceneName CityHollowKnightFountain = new("Ruins1_27");
    public static readonly SceneName CityStorerooms = new("Ruins1_28");
    public static readonly SceneName CityStoreroomsStag = new("Ruins1_29");
    public static readonly SceneName CitySanctumSpellTwister = new("Ruins1_30");
    public static readonly SceneName CityTollBench = new("Ruins1_31");
    public static readonly SceneName CityShadeSoulArena = new("Ruins1_31b");
    public static readonly SceneName CitySoulMasterRewards = new("Ruins1_32");
    public static readonly SceneName CitySpireGreatHusk = new("Ruins2_01");
    public static readonly SceneName CitySpireBase = new("Ruins2_01_b");
    public static readonly SceneName CitySpireWatchers = new("Ruins2_03");
    public static readonly SceneName CitySpireBelowWatchers = new("Ruins2_03b");
    public static readonly SceneName CityRightHub = new("Ruins2_04");
    public static readonly SceneName CityAboveKings = new("Ruins2_05");
    public static readonly SceneName CityKingsStation = new("Ruins2_06");
    public static readonly SceneName CityGrubBelowKings = new("Ruins2_07");
    public static readonly SceneName CityKingsStag = new("Ruins2_08");
    public static readonly SceneName CityKingsVesselFragment = new("Ruins2_09");
    public static readonly SceneName GroundsElevator = new("Ruins2_10");
    public static readonly SceneName CityRightElevator = new("Ruins2_10b");
    public static readonly SceneName CityCollectorArena = new("Ruins2_11");
    public static readonly SceneName CityTowerofLove = new("Ruins2_11_b");
    public static readonly SceneName CityLurienElevator = new("Ruins2_Watcher_Room");
    public static readonly SceneName KingsPass = new("Tutorial_01");
    public static readonly SceneName WaterwaysEntrance = new("Waterways_01");
    public static readonly SceneName WaterwaysMainBench = new("Waterways_02");
    public static readonly SceneName WaterwaysTuk = new("Waterways_03");
    public static readonly SceneName WaterwaysHiddenGrub = new("Waterways_04");
    public static readonly SceneName WaterwaysMaskShard = new("Waterways_04b");
    public static readonly SceneName WaterwaysDungDefenderArena = new("Waterways_05");
    public static readonly SceneName WaterwaysCorridortoBrokenElevator = new("Waterways_06");
    public static readonly SceneName WaterwaysLeftOfIsmasGrove = new("Waterways_07");
    public static readonly SceneName WaterwaysOutsideFlukemarm = new("Waterways_08");
    public static readonly SceneName WaterwaysCornifer = new("Waterways_09");
    public static readonly SceneName WaterwaysFlukemarmArena = new("Waterways_12");
    public static readonly SceneName WaterwaysIsmasGrove = new("Waterways_13");
    public static readonly SceneName WaterwaysEdgeAcidCorridor = new("Waterways_14");
    public static readonly SceneName WaterwaysDungDefendersCave = new("Waterways_15");
    public static readonly SceneName PalaceEntrance = new("White_Palace_01");
    public static readonly SceneName PalaceFirstMold = new("White_Palace_02");
    public static readonly SceneName PalaceHub = new("White_Palace_03_hub");
    public static readonly SceneName PalaceLeftOfHub = new("White_Palace_04");
    public static readonly SceneName PalaceSawRoom = new("White_Palace_05");
    public static readonly SceneName PalaceBalcony = new("White_Palace_06");
    public static readonly SceneName PalaceLampPogo = new("White_Palace_07");
    public static readonly SceneName PalaceWorkshop = new("White_Palace_08");
    public static readonly SceneName PalaceThrone = new("White_Palace_09");
    public static readonly SceneName PalaceOutside = new("White_Palace_11");
    public static readonly SceneName PalaceSpikeDrop = new("White_Palace_12");
    public static readonly SceneName PalaceThornJump = new("White_Palace_13");
    public static readonly SceneName PalaceHellCorridor = new("White_Palace_14");
    public static readonly SceneName PalaceCagedLever = new("White_Palace_15");
    public static readonly SceneName PalaceSawClimb = new("White_Palace_16");
    public static readonly SceneName POPLever = new("White_Palace_17");
    public static readonly SceneName POPEntrance = new("White_Palace_18");
    public static readonly SceneName POPVertical = new("White_Palace_19");
    public static readonly SceneName POPFinal = new("White_Palace_20");
    // @@@ INSERT_SCENE_NAMES END @@@
}

class SceneNameConverter : JsonConverter<SceneName>
{
    public override SceneName ReadJson(JsonReader reader, Type objectType, SceneName? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (serializer.Deserialize(reader, typeof(string)) is string name && SceneName.TryGetValue(name, out SceneName sceneName))
        {
            return sceneName;
        }
        throw new JsonReaderException("Error decoding SceneName");
    }

    public override void WriteJson(JsonWriter writer, SceneName? value, JsonSerializer serializer) => serializer.Serialize(writer, value?.Name());
}
