using DarknessRandomizer.IC;
using ItemChanger;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;

namespace DarknessRandomizer.Rando;

internal static class RequestModifier
{
    public static void Setup()
    {
        RequestBuilder.OnUpdate.Subscribe(-500f, SetupRefs);

        // These should come after RandoPlus, which uses 50f.
        RequestBuilder.OnUpdate.Subscribe(51f, ModifyItems);
        RequestBuilder.OnUpdate.Subscribe(52f, RandomizeDarkness);
    }

    private static void SetupRefs(RequestBuilder rb)
    {
        if (!RandoInterop.ShatteredLantern) return;

        var itemName = RandoInterop.LanternShardItemName;
        rb.EditItemRequest(itemName, info =>
        {
            info.getItemDef = () => new()
            {
                Name = itemName,
                Pool = PoolNames.Key,
                MajorItem = false,
                PriceCap = 750
            };
        });
    }

    private static void RandomizeDarkness(RequestBuilder rb)
    {
        if (!RandoInterop.IsEnabled) return;

        RandoInterop.LS = new(rb.gs, rb.ctx.StartDef);
    }

    private static string Placeholder(string item) => $"{PlaceholderItem.Prefix}{item}";

    private static void ModifyItems(RequestBuilder rb)
    {
        var lanternItemName = RandoInterop.LanternItemName;
        if (!RandoInterop.ShatteredLantern || rb.StartItems.GetCount(lanternItemName) > 0)
        {
            // Nothing further to do.
            return;
        }

        rb.RemoveItemByName(lanternItemName);
        rb.RemoveItemByName(Placeholder(lanternItemName));

        string lanternShardItemName = RandoInterop.LanternShardItemName;
        if (rb.gs.PoolSettings.Keys)
        {
            int numShards = LanternShards.TotalNumShards;
            numShards += DarknessRandomizer.GS.RandomizationSettings.TwoDupeShards ? 2 : 0;
            numShards *= rb.gs.DuplicateItemSettings.DuplicateUniqueKeys ? 2 : 1;
            rb.AddItemByName(lanternShardItemName, LanternShards.TotalNumShards);
            rb.AddItemByName(Placeholder(lanternShardItemName), numShards - LanternShards.TotalNumShards);
        }
        else
        {
            rb.RemoveFromVanilla(LocationNames.Sly, lanternItemName);
            rb.RemoveFromVanilla(LocationNames.Sly, Placeholder(lanternItemName));
            
            for (int i = 0; i < LanternShards.TotalNumShards; ++i)
            {
                VanillaDef def = new(lanternShardItemName, LocationNames.Sly, [new("GEO", 300 + i * 100)]);
                rb.AddToVanilla(def);
            }
        }
    }
}
