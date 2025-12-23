using ItemChanger;

namespace DarknessRandomizer.IC;

public class FinalNoLanternShardItem : AbstractFinalLanternShardItem
{
    public const string ItemName = "Final_No_Lantern_Shard";

    public FinalNoLanternShardItem() : base(ItemName, RandoPlus.Consts.NoLantern) { }

    public override AbstractItem Clone() => new FinalNoLanternShardItem();
}

public class NoLanternShardItem : AbstractBaseLanternShardItem
{
    public const string ItemName = "No_Lantern_Shard";

    public NoLanternShardItem() : base(ItemName, FinalNoLanternShardItem.ItemName) { }

    public override AbstractItem Clone() => new NoLanternShardItem();

    public static void DefineICRefs()
    {
        Finder.DefineCustomItem(new FinalNoLanternShardItem());
        Finder.DefineCustomItem(new NoLanternShardItem());
    }
}
