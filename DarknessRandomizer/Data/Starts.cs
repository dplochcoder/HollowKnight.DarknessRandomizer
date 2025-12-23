using PurenailCore.SystemUtil;
using System.Collections.Generic;

namespace DarknessRandomizer.Data;

public static class Starts
{
    // FIXME: Add clusters
    // Some of these are transition-rando only, and so won't benefit from clusters outside the area
    private static readonly Dictionary<string, HashSet<ClusterName>> ProtectedStartClusters = new()
    {
        {
            "Ancestral Mound",
            new()
            {
                ClusterName.CrossroadsFalseKnight,
                ClusterName.CrossroadsWest,
                ClusterName.CrossroadsEntrance,
                ClusterName.CrossroadsLowerPass,
                ClusterName.CrossroadsStagHub
            }
        },
        {
            "City Storerooms",
            new()
            {
                ClusterName.CityStorerooms,
                ClusterName.CityEastEntrance,
                ClusterName.CityAboveLemm
            }
        },
        {
            "Crystallized Mound",
            new()
            {
                ClusterName.CrossroadsOutsideMound,
                ClusterName.GroundsMain
            }
        },
        {
            "East Blue Lake",
            new()
            {
                ClusterName.GroundsMain,
                ClusterName.CityKingsStation,
                ClusterName.CityRightHub
            }
        },
        {
            "East Crossroads",
            new()
            {
                ClusterName.CrossroadsCentralPass,
                ClusterName.CrossroadsPeaksBridge,
                ClusterName.CrossroadsWest,
                ClusterName.CrossroadsEntrance,
                ClusterName.CrossroadsStagHub,
                ClusterName.CrossroadsLowerPass
            }
        },
        {
            "East Fog Canyon",
            new()
            {
                ClusterName.FogCanyonEast,
                ClusterName.FungalQueensStation,
                ClusterName.FogCanyonEast,
                ClusterName.GreenpathLowerHub,
                ClusterName.GreenpathLowerPass
            }
        },
        {
            "Fungal Wastes",
            new()
            {
                ClusterName.FungalEntrance,
                ClusterName.FungalOgres,
                ClusterName.FungalUpper,
                ClusterName.FungalQueensStation,
                ClusterName.FogCanyonWest
            }
        },
        {
            "Greenpath",
            new()
            {
                ClusterName.GreenpathUpperEast,
                ClusterName.GreenpathUpperWest,
                ClusterName.GreenpathWest,
                ClusterName.GreenpathLowerPass
            }
        },
        {
            "Hallownest's Crown",
            new()
            {
                ClusterName.CrystalPeaksCrown,
                ClusterName.CrystalPeaksRoot,
                ClusterName.CrystalPeaksUpper,
                ClusterName.CrystalPeaksWest,
                ClusterName.CrystalPeaksMiddleBridge
            }
        },
        {
            "King's Pass",
            new()
            {
                ClusterName.KingsPass,
                ClusterName.CrossroadsEntrance,
                ClusterName.CrossroadsWest,
                ClusterName.CrossroadsPeaksBridge,
                ClusterName.CrossroadsLowerPass,
                ClusterName.CrossroadsStagHub
            }
        },
        {
            "King's Station",
            new()
            {
                ClusterName.CityKingsStation,
                ClusterName.CityEastEntrance,
                ClusterName.CityBridgeToBasin,
                ClusterName.BasinFountain
            }
        },
        {
            "Lower Greenpath",
            new()
            {
                ClusterName.GreenpathMMC,
                ClusterName.GreenpathLowerHub,
                ClusterName.GreenpathLowerPass,
                ClusterName.FogCanyonWest,
                ClusterName.FungalQueensStation
            }
        },
        {
            "Mantis Village",
            new()
            {
                ClusterName.FungalMantisVillage,
                ClusterName.FungalPilgrimsWay,
                ClusterName.FungalElderHuWing,
                ClusterName.FungalUpper,
                ClusterName.FungalEntrance,
                ClusterName.FungalQueensStation
            }
        },
        {
            "Outside Colosseum",
            new()
            {
                ClusterName.EdgeColoApproach,
                ClusterName.CityKingsStation,
                ClusterName.CityRightHub,
                ClusterName.EdgeTallRooms,
                ClusterName.EdgeEastUpper
            }
        },
        {
            "Queen's Station",
            new()
            {
                ClusterName.FungalQueensStation,
                ClusterName.FogCanyonWest,
                ClusterName.GreenpathLowerHub,
                ClusterName.GreenpathLowerPass
            }
        },
        {
            "Stag Nest",
            new()
            {
                ClusterName.CliffsMain,
                ClusterName.KingsPass,
                ClusterName.CliffsBaldurShell,
                ClusterName.GreenpathUpperWest,
                ClusterName.GreenpathWest
            }
        },
        {
            "West Blue Lake",
            new()
            {
                ClusterName.CrossroadsShops,
                ClusterName.CrossroadsStagHub,
                ClusterName.BlueLake,
                ClusterName.CrossroadsLowerPass,
                ClusterName.CrossroadsWest
            }
        },
        {
            "West Crossroads",
            new()
            {
                ClusterName.CrossroadsMawlek,
                ClusterName.CrossroadsMawlekApproach,
                ClusterName.CrossroadsWest,
                ClusterName.CrossroadsLowerPass,
                ClusterName.CrossroadsStagHub,
                ClusterName.CrossroadsEntrance
            }
        },
        {
            "West Fog Canyon",
            new()
            {
                ClusterName.FogCanyonWest,
                ClusterName.FogCanyonWestWing,
                ClusterName.FungalQueensStation,
                ClusterName.GreenpathLowerHub,
                ClusterName.GreenpathLowerPass
            }
        }
    };

    public static IReadOnlyCollection<ClusterName> GetStartClusters(string start) => ProtectedStartClusters.GetOrDefault(start, () => []);
}
