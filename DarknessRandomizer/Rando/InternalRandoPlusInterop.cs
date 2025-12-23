using DarknessRandomizer.IC;
using ItemChanger;

namespace DarknessRandomizer.Rando;

public static class InternalRandoPlusInterop
{
    public static bool NoLantern => RandoPlus.RandoPlus.GS.NoLantern;

    public static string LanternTermName => NoLantern ? "NOLANTERN" : "LANTERN";

    public static string LanternItemName => NoLantern ? RandoPlus.Consts.NoLantern : ItemNames.Lumafly_Lantern;

    public static string LanternShardItemName => NoLantern ? NoLanternShardItem.ItemName : LanternShardItem.ItemName;

    public static void DefineICRefs() => NoLanternShardItem.DefineICRefs();
}
