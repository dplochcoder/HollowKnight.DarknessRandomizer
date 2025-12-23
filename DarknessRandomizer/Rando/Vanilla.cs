using DarknessRandomizer.IC;
using ItemChanger;
using ItemChanger.Tags;
using RandomizerMod.RC;
using System.Collections.Generic;

namespace DarknessRandomizer.Rando;

internal class SlyLanternShardTag : Tag, IShopRequirementTag
{
    public int RequiredShards;

    public bool MeetsRequirement => ItemChangerMod.Modules.Get<DarknessRandomizerModule>().NumLanternShardsCollected >= RequiredShards;
}

public static class Vanilla
{
    public static void Setup()
    {
        RequestBuilder.OnUpdate.Subscribe(-99f, RemoveLantern);
        On.UIManager.StartNewGame += PlaceVanillaItems;
    }

    private static bool VanillaShatteredLantern(RequestBuilder? rb = null) => RandoInterop.ShatteredLantern && !((rb?.gs ?? RandomizerMod.RandomizerMod.RS?.GenerationSettings)?.PoolSettings.Keys ?? true);
    private static void RemoveLantern(RequestBuilder rb)
    {
        if (!VanillaShatteredLantern(rb)) return;

        rb.EditLocationRequest(LocationNames.Sly, info =>
        {
            var origFetch = info.customPlacementFetch;
            info.customPlacementFetch = (factory, placement) =>
            {
                var origPlacement = origFetch(factory, placement);
                if (origPlacement is ItemChanger.Placements.ShopPlacement sp)
                {
                    sp.defaultShopItems &= ~DefaultShopItems.SlyLantern;
                    return sp;
                }
                return origPlacement;
            };
        });
    }

    private static void PlaceVanillaItems(On.UIManager.orig_StartNewGame orig, UIManager self, bool permaDeath, bool bossRush)
    {
        if (!VanillaShatteredLantern())
        {
            orig(self, permaDeath, bossRush);
            return;
        }

        var placement = Finder.GetLocation(LocationNames.Sly).Wrap();
        for (int i = 0; i < 4; i++)
        {
            var item = Finder.GetItem(RandoInterop.LanternShardItemName);
            item.AddTag<CostTag>().Cost = Cost.NewGeoCost(300 + i * 100);
            item.AddTag<SlyLanternShardTag>().RequiredShards = i;
            placement.Add(item);
        }
        List<AbstractPlacement> placements = [placement];
        ItemChangerMod.AddPlacements(placements, PlacementConflictResolution.MergeKeepingOld);

        orig(self, permaDeath, bossRush);
    }
}
